/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.SystemLayer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace PaintDotNet
{
    internal sealed class Startup
    {
        private static Startup instance;
        private static DateTime startupTime;
        private string[] args;
        private MainForm mainForm;

        private Startup(string[] args)
        {
            this.args = args;
        }

        /// <summary>
        /// Starts a new instance of Paint.NET with the give arguments.
        /// </summary>
        /// <param name="fileName">The name of the filename to open, or null to start with a blank canvas.</param>
        public static void StartNewInstance(IWin32Window parent, bool requireAdmin, string[] args)
        {
            StringBuilder allArgsSB = new StringBuilder();

            foreach (string arg in args)
            {
                allArgsSB.Append(' ');

                if (arg.IndexOf(' ') != -1)
                {
                    allArgsSB.Append('"');
                }

                allArgsSB.Append(arg);

                if (arg.IndexOf(' ') != -1)
                {
                    allArgsSB.Append('"');
                }
            }

            string allArgs;

            if (allArgsSB.Length > 0)
            {
                allArgs = allArgsSB.ToString(1, allArgsSB.Length - 1);
            }
            else
            {
                allArgs = null;
            }

            Shell.Execute(
                parent, 
                Application.ExecutablePath, 
                allArgs, 
                requireAdmin ? ExecutePrivilege.RequireAdmin : ExecutePrivilege.AsInvokerOrAsManifest, 
                ExecuteWaitType.ReturnImmediately);
        }

        public static void StartNewInstance(IWin32Window parent, string fileName)
        {
            string arg;

            if (fileName != null && fileName.Length != 0)
            {
                arg = "\"" + fileName + "\"";
            }
            else
            {
                arg = "";
            }

            StartNewInstance(parent, false, new string[1] { arg });
        }

        private static bool CloseForm(Form form)
        {
            ArrayList openForms = new ArrayList(Application.OpenForms);

            if (openForms.IndexOf(form) == -1)
            {
                return false;
            }

            form.Close();

            ArrayList openForms2 = new ArrayList(Application.OpenForms);

            if (openForms2.IndexOf(form) == -1)
            {
                return true;
            }

            return false;
        }

        public static bool CloseApplication()
        {
            bool returnVal = true;

            List<Form> allFormsButMainForm = new List<Form>();

            foreach (Form form in Application.OpenForms)
            {
                if (form.Modal && !object.ReferenceEquals(form, instance.mainForm))
                {
                    allFormsButMainForm.Add(form);
                }
            }

            if (allFormsButMainForm.Count > 0)
            {
                // Cannot close application if there are modal dialogs
                return false;
            }

            returnVal = CloseForm(instance.mainForm);
            return returnVal;
        }
        
        /// <summary>
        /// Checks to make sure certain files are present, and tries to repair the problem.
        /// </summary>
        /// <returns>
        /// true if any repairs had to be made, at which point PDN must be restarted.
        /// false otherwise, if everything's okay.
        /// </returns>
        private bool CheckForImportantFiles()
        {
            string[] requiredFiles =
                new string[]
                {
                    "FileTypes\\DdsFileType.dll",
                    "ICSharpCode.SharpZipLib.dll",
                    "Interop.WIA.dll",
                    "PaintDotNet.Base.dll",
                    "PaintDotNet.Core.dll",
                    "PaintDotNet.Data.dll",
                    "PaintDotNet.Effects.dll",
                    "PaintDotNet.Resources.dll",
                    "PaintDotNet.Strings.3.DE.resources",
                    "PaintDotNet.Strings.3.ES.resources",
                    "PaintDotNet.Strings.3.FR.resources",
                    "PaintDotNet.Strings.3.IT.resources",
                    "PaintDotNet.Strings.3.JA.resources",
                    "PaintDotNet.Strings.3.KO.resources",
                    "PaintDotNet.Strings.3.PT-BR.resources",
                    "PaintDotNet.Strings.3.resources",
                    "PaintDotNet.Strings.3.ZH-CN.resources",
                    "PaintDotNet.StylusReader.dll",
                    "PaintDotNet.SystemLayer.dll",
                    "SetupNgen.exe",
                    "ShellExtension_x64.dll",
                    "ShellExtension_x86.dll",
                    "Squish_x64.dll",
                    "Squish_x86.dll",
                    "Squish_x86_SSE2.dll",
                    "UpdateMonitor.exe",
                    "WiaProxy32.exe"
                };

            string dirName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            List<string> missingFiles = null;

            foreach (string requiredFile in requiredFiles)
            {
                bool missing;

                try
                {
                    string pathName = Path.Combine(dirName, requiredFile);
                    FileInfo fileInfo = new FileInfo(pathName);
                    missing = !fileInfo.Exists;
                }

                catch (Exception)
                {
                    missing = true;
                }

                if (missing)
                {
                    if (missingFiles == null)
                    {
                        missingFiles = new List<string>();
                    }

                    missingFiles.Add(requiredFile);
                }
            }

            if (missingFiles == null)
            {
                return false;
            }
            else
            {
                if (Shell.ReplaceMissingFiles(missingFiles.ToArray()))
                {
                    // Everything is repaired and happy.
                    return true;
                }
                else
                {
                    // Things didn't get fixed. Bail.
                    Process.GetCurrentProcess().Kill();
                    return false;
                }
            }
        }

        public void Start()
        {
            // Set up unhandled exception handlers
#if DEBUG
            // In debug builds we'd prefer to have it dump us into the debugger
#else
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
#endif

            // Initialize some misc. Windows Forms settings
            Application.SetCompatibleTextRenderingDefault(false);
            Application.EnableVisualStyles();

            // If any files are missing, try to repair.
            // However, support /skipRepairAttempt for when developing in the IDE 
            // so that we don't needlessly try to repair in that case.
            if (this.args.Length > 0 && 
                string.Compare(this.args[0], "/skipRepairAttempt", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                // do nothing: we need this so that we can run from IDE/debugger
                // without it trying to repair itself all the time
            }
            else
            {
                if (CheckForImportantFiles())
                {
                    Startup.StartNewInstance(null, false, args);
                    return;
                }
            }

            // The rest of the code is put in a separate method so that certain DLL's
            // won't get delay loaded until after we try to do repairs.
            StartPart2();
        }

        private void StartPart2()
        {
            // Set up locale / resource details
            string locale = Settings.CurrentUser.GetString(SettingNames.LanguageName, null);

            if (locale == null)
            {
                locale = Settings.SystemWide.GetString(SettingNames.LanguageName, null);
            }

            if (locale != null)
            {
                try
                {
                    CultureInfo ci = new CultureInfo(locale, true);
                    Thread.CurrentThread.CurrentUICulture = ci;
                }

                catch (Exception)
                {
                    // Don't want bad culture name to crash us
                }
            }

            // Check system requirements
            if (!OS.CheckOSRequirement())
            {
                string message = PdnResources.GetString("Error.OSRequirement");
                Utility.ErrorBox(null, message);
                return;
            }

            // Parse command-line arguments
            if (this.args.Length == 1 && 
                this.args[0] == Updates.UpdatesOptionsDialog.CommandLineParameter)
            {
                Updates.UpdatesOptionsDialog.ShowUpdateOptionsDialog(null, false);
            }
            else
            {
                SingleInstanceManager singleInstanceManager = new SingleInstanceManager(InvariantStrings.SingleInstanceMonikerName);

                // If this is not the first instance of PDN.exe, then forward the command-line
                // parameters over to the first instance.
                if (!singleInstanceManager.IsFirstInstance)
                {
                    singleInstanceManager.FocusFirstInstance();

                    foreach (string arg in this.args)
                    {
                        singleInstanceManager.SendInstanceMessage(arg, 30);
                    }

                    singleInstanceManager.Dispose();
                    singleInstanceManager = null;

                    return;
                }

                // Create main window
                this.mainForm = new MainForm(this.args);

                this.mainForm.SingleInstanceManager = singleInstanceManager;
                singleInstanceManager = null; // mainForm owns it now

                // 3 2 1 go
                Application.Run(this.mainForm);

                try
                {
                    this.mainForm.Dispose();
                }

                catch (Exception)
                {
                }

                this.mainForm = null;
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static int Main(string[] args) 
        {
            startupTime = DateTime.Now;

#if !DEBUG
            try
            {
#endif
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

                instance = new Startup(args);
                instance.Start();
#if !DEBUG
            }

            catch (Exception ex)
            {
                try
                {
                    UnhandledException(ex);
                    Process.GetCurrentProcess().Kill();
                }

                catch (Exception)
                {
                    MessageBox.Show(ex.ToString());
                    Process.GetCurrentProcess().Kill();
                }
            }
#endif

            return 0;
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            // For v3.05, we renamed PdnLib.dll to PaintDotNet.Core.dll. So we should really make
            // sure we stay compatible with old plugin DLL's.
            const string oldCoreName = "PdnLib";

            int index = args.Name.IndexOf(oldCoreName, StringComparison.InvariantCultureIgnoreCase);
            Assembly newAssembly = null;

            if (index == 0)
            {
                newAssembly = typeof(ColorBgra).Assembly;
            }

            return newAssembly;
        }

        private static void UnhandledException(Exception ex)
        {
            string dir = Shell.GetVirtualPath(VirtualFolderName.UserDesktop, true);
            const string fileName = "pdncrash.log";
            string fullName = Path.Combine(dir, fileName);

            using (StreamWriter stream = new System.IO.StreamWriter(fullName, true))
            {
                stream.AutoFlush = true;
                WriteCrashLog(ex, stream);
            }

            string errorFormat;
            string errorText;

            try
            {
                errorFormat = PdnResources.GetString("Startup.UnhandledError.Format");
            }

            catch (Exception)
            {
                errorFormat = InvariantStrings.StartupUnhandledErrorFormatFallback;
            }

            errorText = string.Format(errorFormat, fileName);
            Utility.ErrorBox(null, errorText);
        }

        public static string GetCrashLogHeader()
        {
            StringBuilder headerSB = new StringBuilder();
            StringWriter headerSW = new StringWriter(headerSB);
            WriteCrashLog(null, headerSW);
            return headerSB.ToString();
        }

        private static void WriteCrashLog(Exception ex, TextWriter stream)
        {
            string headerFormat;

            try
            {
                headerFormat = PdnResources.GetString("CrashLog.HeaderText.Format");
            }

            catch (Exception ex13)
            {
                headerFormat = 
                    InvariantStrings.CrashLogHeaderTextFormatFallback + 
                    ", --- Exception while calling PdnResources.GetString(\"CrashLog.HeaderText.Format\"): " + 
                    ex13.ToString() + 
                    Environment.NewLine;
            }

            string header;

            try
            {
                header = string.Format(headerFormat, InvariantStrings.CrashlogEmail);
            }

            catch
            {
                header = string.Empty;
            }

            stream.WriteLine(header);

            const string noInfoString = "err";

            string fullAppName = noInfoString;
            string timeOfCrash = noInfoString;
            string appUptime = noInfoString;
            string osVersion = noInfoString;
            string osRevision = noInfoString;
            string osType = noInfoString;
            string processorNativeArchitecture = noInfoString;
            string clrVersion = noInfoString;
            string fxInventory = noInfoString;
            string processorArchitecture = noInfoString;
            string cpuName = noInfoString;
            string cpuCount = noInfoString;
            string cpuSpeed = noInfoString;
            string cpuFeatures = noInfoString;
            string totalPhysicalBytes = noInfoString;
            string dpiInfo = noInfoString;
            string localeName = noInfoString;
            string inkInfo = noInfoString;
            string updaterInfo = noInfoString;
            string featuresInfo = noInfoString;
            string assembliesInfo = noInfoString;

            try
            {
                try
                {
                    fullAppName = PdnInfo.GetFullAppName();
                }

                catch (Exception ex1)
                {
                    fullAppName = Application.ProductVersion + ", --- Exception while calling PdnInfo.GetFullAppName(): " + ex1.ToString() + Environment.NewLine;
                }

                try
                {
                    timeOfCrash = DateTime.Now.ToString();
                }

                catch (Exception ex2)
                {
                    timeOfCrash = "--- Exception while populating timeOfCrash: " + ex2.ToString() + Environment.NewLine;
                }

                try
                {
                    appUptime = (DateTime.Now - startupTime).ToString();
                }

                catch (Exception ex13)
                {
                    appUptime = "--- Exception while populating appUptime: " + ex13.ToString() + Environment.NewLine;
                }

                try
                {
                    osVersion = System.Environment.OSVersion.Version.ToString();
                }

                catch (Exception ex3)
                {
                    osVersion = "--- Exception while populating osVersion: " + ex3.ToString() + Environment.NewLine;
                }

                try
                {
                    osRevision = OS.Revision;
                }

                catch (Exception ex4)
                {
                    osRevision = "--- Exception while populating osRevision: " + ex4.ToString() + Environment.NewLine;
                }

                try
                {
                    osType = OS.Type.ToString();
                }

                catch (Exception ex5)
                {
                    osType = "--- Exception while populating osType: " + ex5.ToString() + Environment.NewLine;
                }

                try
                {
                    processorNativeArchitecture = Processor.NativeArchitecture.ToString().ToLower();
                }

                catch (Exception ex6)
                {
                    processorNativeArchitecture = "--- Exception while populating processorNativeArchitecture: " + ex6.ToString() + Environment.NewLine;
                }

                try
                {
                    clrVersion = System.Environment.Version.ToString();
                }

                catch (Exception ex7)
                {
                    clrVersion = "--- Exception while populating clrVersion: " + ex7.ToString() + Environment.NewLine;
                }

                try
                {
                    fxInventory =
                        (SystemLayer.OS.IsDotNetVersionInstalled(2, 0, 0, false) ? "2.0 " : "") +
                        (SystemLayer.OS.IsDotNetVersionInstalled(2, 0, 1, false) ? "2.0SP1 " : "") +
                        (SystemLayer.OS.IsDotNetVersionInstalled(2, 0, 2, false) ? "2.0SP2 " : "") +
                        (SystemLayer.OS.IsDotNetVersionInstalled(3, 0, 0, false) ? "3.0 " : "") +
                        (SystemLayer.OS.IsDotNetVersionInstalled(3, 0, 1, false) ? "3.0SP1 " : "") +
                        (SystemLayer.OS.IsDotNetVersionInstalled(3, 0, 2, false) ? "3.0SP2 " : "") +
                        (SystemLayer.OS.IsDotNetVersionInstalled(3, 5, 0, false) ? "3.5 " : "") +
                        (SystemLayer.OS.IsDotNetVersionInstalled(3, 5, 1, false) ? "3.5SP1 " : "") +
                        (SystemLayer.OS.IsDotNetVersionInstalled(3, 5, 1, true) ? "3.5SP1_Client " : "") +
                        (SystemLayer.OS.IsDotNetVersionInstalled(3, 5, 2, false) ? "3.5SP2 " : "") +
                        (SystemLayer.OS.IsDotNetVersionInstalled(4, 0, 0, false) ? "4.0 " : "") +
                        (SystemLayer.OS.IsDotNetVersionInstalled(4, 0, 1, false) ? "4.0SP1 " : "") +
                        (SystemLayer.OS.IsDotNetVersionInstalled(4, 0, 2, false) ? "4.0SP2 " : "")
                        .Trim();
                }

                catch (Exception ex30)
                {
                    fxInventory = "--- Exception while populating fxInventory: " + ex30.ToString() + Environment.NewLine;
                }

                try
                {
                    processorArchitecture = Processor.Architecture.ToString().ToLower();
                }

                catch (Exception ex8)
                {
                    processorArchitecture = "--- Exception while populating processorArchitecture: " + ex8.ToString() + Environment.NewLine;
                }

                try
                {
                    cpuName = SystemLayer.Processor.CpuName;
                }

                catch (Exception ex9)
                {
                    cpuName = "--- Exception while populating cpuName: " + ex9.ToString() + Environment.NewLine;
                }

                try
                {
                    cpuCount = SystemLayer.Processor.LogicalCpuCount.ToString() + "x";
                }

                catch (Exception ex10)
                {
                    cpuCount = "--- Exception while populating cpuCount: " + ex10.ToString() + Environment.NewLine;
                }

                try
                {
                    cpuSpeed = "@ ~" + SystemLayer.Processor.ApproximateSpeedMhz.ToString() + "MHz";
                }

                catch (Exception ex16)
                {
                    cpuSpeed = "--- Exception while populating cpuSpeed: " + ex16.ToString() + Environment.NewLine;
                }

                try
                {
                    cpuFeatures = string.Empty;
                    string[] featureNames = Enum.GetNames(typeof(ProcessorFeature));
                    bool firstFeature = true;

                    for (int i = 0; i < featureNames.Length; ++i)
                    {
                        string featureName = featureNames[i];
                        ProcessorFeature feature = (ProcessorFeature)Enum.Parse(typeof(ProcessorFeature), featureName);

                        if (Processor.IsFeaturePresent(feature))
                        {
                            if (firstFeature)
                            {
                                cpuFeatures = "(";
                                firstFeature = false;
                            }
                            else
                            {
                                cpuFeatures += ", ";
                            }

                            cpuFeatures += featureName;
                        }
                    }

                    if (cpuFeatures.Length > 0)
                    {
                        cpuFeatures += ")";
                    }
                }

                catch (Exception ex17)
                {
                    cpuFeatures = "--- Exception while populating cpuFeatures: " + ex17.ToString() + Environment.NewLine;
                }

                try
                {
                    totalPhysicalBytes = ((SystemLayer.Memory.TotalPhysicalBytes / 1024) / 1024) + " MB";
                }

                catch (Exception ex11)
                {
                    totalPhysicalBytes = "--- Exception while populating totalPhysicalBytes: " + ex11.ToString() + Environment.NewLine;
                }

                try
                {
                    float xScale;

                    try
                    {
                        xScale = UI.GetXScaleFactor();
                    }

                    catch (Exception)
                    {
                        using (Control c = new Control())
                        {
                            UI.InitScaling(c);
                            xScale = UI.GetXScaleFactor();
                        }
                    }

                    dpiInfo = string.Format("{0} dpi ({1}x scale)", (96.0f * xScale).ToString("F2"), xScale.ToString("F2"));
                }

                catch (Exception ex19)
                {
                    dpiInfo = "--- Exception while populating dpiInfo: " + ex19.ToString() + Environment.NewLine;
                }

                try
                {
                    localeName = 
                        "pdnr.c: " + PdnResources.Culture.Name +
                        ", hklm: " + Settings.SystemWide.GetString(SettingNames.LanguageName, "n/a") +
                        ", hkcu: " + Settings.CurrentUser.GetString(SettingNames.LanguageName, "n/a") + 
                        ", cc: " + CultureInfo.CurrentCulture.Name + 
                        ", cuic: " + CultureInfo.CurrentUICulture.Name;
                }

                catch (Exception ex14)
                {
                    localeName = "--- Exception while populating localeName: " + ex14.ToString() + Environment.NewLine;
                }

                try
                {
                    inkInfo = Ink.IsAvailable() ? "yes" : "no";
                }

                catch (Exception ex15)
                {
                    inkInfo = "--- Exception while populating inkInfo: " + ex15.ToString() + Environment.NewLine;
                }

                try
                {
                    string autoCheckForUpdates = Settings.SystemWide.GetString(SettingNames.AutoCheckForUpdates, noInfoString);

                    string lastUpdateCheckTimeInfo;

                    try
                    {
                        string lastUpdateCheckTimeString = Settings.CurrentUser.Get(SettingNames.LastUpdateCheckTimeTicks);
                        long lastUpdateCheckTimeTicks = long.Parse(lastUpdateCheckTimeString);
                        DateTime lastUpdateCheckTime = new DateTime(lastUpdateCheckTimeTicks);
                        lastUpdateCheckTimeInfo = lastUpdateCheckTime.ToShortDateString();
                    }

                    catch (Exception)
                    {
                        lastUpdateCheckTimeInfo = noInfoString;
                    }

                    updaterInfo = string.Format(
                        "{0}, {1}",
                        (autoCheckForUpdates == "1") ? "true" : (autoCheckForUpdates == "0" ? "false" : (autoCheckForUpdates ?? "null")),
                        lastUpdateCheckTimeInfo);
                }

                catch (Exception ex17)
                {
                    updaterInfo = "--- Exception while populating updaterInfo: " + ex17.ToString() + Environment.NewLine;
                }

                try
                {
                    StringBuilder featureSB = new StringBuilder();

                    IEnumerable<string> featureList = SystemLayer.Tracing.GetLoggedFeatures();

                    bool first = true;
                    foreach (string feature in featureList)
                    {
                        if (!first)
                        {
                            featureSB.Append(", ");
                        }

                        featureSB.Append(feature);

                        first = false;
                    }

                    featuresInfo = featureSB.ToString();
                }

                catch (Exception ex18)
                {
                    featuresInfo = "--- Exception while populating featuresInfo: " + ex18.ToString() + Environment.NewLine;
                }

                try
                {
                    StringBuilder assembliesInfoSB = new StringBuilder();

                    Assembly[] loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

                    foreach (Assembly assembly in loadedAssemblies)
                    {
                        assembliesInfoSB.AppendFormat("{0}    {1} @ {2}", Environment.NewLine, assembly.FullName, assembly.Location);
                    }

                    assembliesInfo = assembliesInfoSB.ToString();
                }

                catch (Exception ex16)
                {
                    assembliesInfo = "--- Exception while populating assembliesInfo: " + ex16.ToString() + Environment.NewLine;
                }
            }

            catch (Exception ex12)
            {
                stream.WriteLine("Exception while gathering app and system info: " + ex12.ToString());
            }

            stream.WriteLine("Application version: " + fullAppName);
            stream.WriteLine("Time of crash: " + timeOfCrash);
            stream.WriteLine("Application uptime: " + appUptime);

            stream.WriteLine("OS Version: " + osVersion + (string.IsNullOrEmpty(osRevision) ? "" : (" " + osRevision)) + " " + osType + " " + processorNativeArchitecture);
            stream.WriteLine(".NET version: CLR " + clrVersion + " " + processorArchitecture + ", FX " + fxInventory);
            stream.WriteLine("Processor: " + cpuCount + " \"" + cpuName + "\" " + cpuSpeed + " " + cpuFeatures);
            stream.WriteLine("Physical memory: " + totalPhysicalBytes);
            stream.WriteLine("UI DPI: " + dpiInfo);
            stream.WriteLine("Tablet PC: " + inkInfo);
            stream.WriteLine("Updates: " + updaterInfo);
            stream.WriteLine("Locale: " + localeName);
            stream.WriteLine("Features log: " + featuresInfo);
            stream.WriteLine("Loaded assemblies: " + assembliesInfo);
            stream.WriteLine();

            stream.WriteLine("Exception details:");

            if (ex == null)
            {
                stream.WriteLine("(null)");
            }
            else
            {
                stream.WriteLine(ex.ToString());

                // Determine if there is any 'secondary' exception to report
                Exception[] otherEx = null;

                if (ex is System.Reflection.ReflectionTypeLoadException)
                {
                    otherEx = ((System.Reflection.ReflectionTypeLoadException)ex).LoaderExceptions;
                }

                if (otherEx != null)
                {
                    for (int i = 0; i < otherEx.Length; ++i)
                    {
                        stream.WriteLine();
                        stream.WriteLine("Secondary exception details:");

                        if (otherEx[i] == null)
                        {
                            stream.WriteLine("(null)");
                        }
                        else
                        {
                            stream.WriteLine(otherEx[i].ToString());
                        }
                    }
                }
            }

            stream.WriteLine("------------------------------------------------------------------------------");
            stream.Flush();
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            UnhandledException((Exception)e.ExceptionObject);
            Process.GetCurrentProcess().Kill();
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            UnhandledException(e.Exception);
            Process.GetCurrentProcess().Kill();
        }
    }
}
