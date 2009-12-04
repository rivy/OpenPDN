/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace PaintDotNet.SystemLayer
{
    public static class Shell
    {
        /// <summary>
        /// Repairs the installation of Paint.NET by replacing any files that have gone missing.
        /// This method should only be called after it has been determined that the files are missing,
        /// and not as a way to determine which files are missing.
        /// This is used, for instance, if the resource files, such as PaintDotNet.Strings.3.resources,
        /// cannot be found. This is actually a top support issue, and by automatically repairing
        /// this problem we save a lot of people a lot of trouble.
        /// </summary>
        /// <param name="missingFiles">
        /// Friendly names for the files that are missing. These will not be used as part of the
        /// repair process but rather as part of any UI presented to the user, or in an exception that 
        /// will be thrown in the case of an error.
        /// </param>
        /// <returns>
        /// true if everything was successful, false if the user cancelled or does not have administrator
        /// privilege (and cannot elevate). An exception is thrown for errors.
        /// </returns>
        /// <remarks>
        /// Note to implementors: This may be implemented as a no-op. Just return true in this case.
        /// </remarks>
        public static bool ReplaceMissingFiles(string[] missingFiles)
        {
            // Generate a friendly, comma separated list of the missing file names
            StringBuilder missingFilesSB = new StringBuilder();

            for (int i = 0; i < missingFiles.Length; ++i)
            {
                missingFilesSB.Append(missingFiles[i]);

                if (i != missingFiles.Length - 1)
                {
                    missingFilesSB.Append(", ");
                }
            }

            try
            {
                // If they are not an admin and have no possibility of elevating, such as for a standard User
                // in XP, then give them an error. Unfortunately we do not know if we can even load text
                // resources at this point, and so must provide an English-only error message.
                if (!Security.IsAdministrator && !Security.CanElevateToAdministrator)
                {
                    MessageBox.Show(
                        null,
                        "Paint.NET has detected that some important installation files are missing. Repairing " +
                        "this requires administrator privilege. Please run the 'PdnRepair.exe' program in the installation " +
                        "directory after logging in with a user that has administrator privilege." + Environment.NewLine + 
                        Environment.NewLine + 
                        "The missing files are: " + missingFilesSB.ToString(),
                        "Paint.NET",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return false;
                }

                const int hMargin = 8;
                const int vMargin = 8;
                Form form = new Form();
                form.Text = "Paint.NET";
                form.ClientSize = new Size(400, 10);
                form.StartPosition = FormStartPosition.CenterScreen;

                Label infoLabel = new Label();
                form.Controls.Add(infoLabel);
                infoLabel.Text = 
                    "Paint.NET has detected that some important installation files are missing. If you click " +
                    "the Repair button it will attempt to repair this and then continue loading." + Environment.NewLine + 
                    Environment.NewLine +
                    "The missing files are: " + missingFilesSB.ToString();

#if DEBUG
                infoLabel.Text += Environment.NewLine + Environment.NewLine + 
                    "*** Since this is a DEBUG build, you should probably add /skipRepairAttempt to the command-line.";
#endif

                infoLabel.Location = new Point(hMargin, vMargin);
                infoLabel.Width = form.ClientSize.Width - hMargin * 2;
                infoLabel.Height = infoLabel.GetPreferredSize(new Size(infoLabel.Width, 1)).Height;

                Button repairButton = new Button();
                form.Controls.Add(repairButton);
                repairButton.Text = "&Repair";

                Exception exception = null;

                repairButton.Click +=
                    delegate(object sender, EventArgs e)
                    {
                        form.DialogResult = DialogResult.Yes;
                        repairButton.Enabled = false;

                        try
                        {
                            Shell.Execute(form, "PdnRepair.exe", "/noPause", ExecutePrivilege.AsInvokerOrAsManifest, ExecuteWaitType.WaitForExit);
                        }

                        catch (Exception ex)
                        {
                            exception = ex;
                        }
                    };

                repairButton.AutoSize = true;
                repairButton.PerformLayout();
                repairButton.Width += 20;
                repairButton.Location = new Point((form.ClientSize.Width - repairButton.Width) / 2, infoLabel.Bottom + vMargin * 2);
                repairButton.FlatStyle = FlatStyle.System;
                UI.EnableShield(repairButton, true);
                
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MinimizeBox = false;
                form.MaximizeBox = false;
                form.ShowInTaskbar = true;
                form.Icon = null;
                form.ClientSize = new Size(form.ClientRectangle.Width, repairButton.Bottom + vMargin);

                DialogResult result = form.ShowDialog(null);
                form.Dispose();
                form = null;

                if (result == DialogResult.Yes)
                {
                    return true;
                }
                else if (exception == null)
                {
                    return false;
                }
                else
                {
                    throw new Exception("Error while attempting to repair", exception);
                }                
            }

            catch (Exception ex)
            {
                throw new Exception("Could not repair installation after it was determined that the following files are missing: " +
                    missingFilesSB.ToString(), ex);
            }
        }

        /// <summary>
        /// Opens the requested directory in the shell's file/folder browser.
        /// </summary>
        /// <param name="parent">The window that is currently in the foreground.</param>
        /// <param name="folderPath">The folder to open.</param>
        /// <remarks>
        /// This UI is presented modelessly, in another process, and in the foreground.
        /// Error handling and messaging (error dialogs) will be handled by the shell,
        /// and these errors will not be communicated to the caller of this method.
        /// </remarks>
        public static void BrowseFolder(IWin32Window parent, string folderPath)
        {
            NativeStructs.SHELLEXECUTEINFO sei = new NativeStructs.SHELLEXECUTEINFO();

            sei.cbSize = (uint)Marshal.SizeOf(typeof(NativeStructs.SHELLEXECUTEINFO));
            sei.fMask = NativeConstants.SEE_MASK_NO_CONSOLE;
            sei.lpVerb = "open";
            sei.lpFile = folderPath;
            sei.nShow = NativeConstants.SW_SHOWNORMAL;
            sei.hwnd = parent.Handle;

            bool bResult = NativeMethods.ShellExecuteExW(ref sei);

            if (bResult)
            {
                if (sei.hProcess != IntPtr.Zero)
                {
                    SafeNativeMethods.CloseHandle(sei.hProcess);
                    sei.hProcess = IntPtr.Zero;
                }
            }
            else
            {
                NativeMethods.ThrowOnWin32Error("ShellExecuteW returned FALSE");
            }

            GC.KeepAlive(parent);
        }

#if false
        [Obsolete("Do not use this method.", true)]
        public static void Execute(
            IWin32Window parent,
            string exePath,
            string args,
            bool requireAdmin)
        {
            Execute(parent, exePath, args, requireAdmin ? ExecutePrivilege.RequireAdmin : ExecutePrivilege.AsInvokerOrAsManifest, ExecuteWaitType.ReturnImmediately);
        }
#endif

        private const string updateExeFileName = "UpdateMonitor.exe";

        private delegate int ExecuteHandOff(IWin32Window parent, string exePath, string args, out IntPtr hProcess);

        /// <summary>
        /// Uses the shell to execute the command. This method must only be used by Paint.NET
        /// and not by plugins.
        /// </summary>
        /// <param name="parent">
        /// The window that is currently in the foreground. This may be null if requireAdmin 
        /// is false and the executable that exePath refers to is not marked (e.g. via a 
        /// manifest) as requiring administrator privilege.
        /// </param>
        /// <param name="exePath">
        /// The path to the executable to launch.
        /// </param>
        /// <param name="args">
        /// The command-line arguments for the executable.
        /// </param>
        /// <param name="execPrivilege">
        /// The privileges to execute the new process with.
        /// If the executable is already marked as requiring administrator privilege
        /// (e.g. via a "requiresAdministrator" UAC manifest), this parameter should be 
        /// set to AsInvokerOrAsManifest.
        /// </param>
        /// <remarks>
        /// If administrator privilege is required, a consent UI may be displayed asking the
        /// user to approve the action. A parent window must be provided in this case so that
        /// the consent UI will know where to position itself. Administrator privilege is
        /// required if execPrivilege is set to RequireAdmin, or if the executable being launched
        /// has a manifest declaring that it requires this privilege and if the operating
        /// system recognizes the manifest.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// execPrivilege was RequireAdmin, but parent was null.
        /// </exception>
        /// <exception cref="SecurityException">
        /// execPrivilege was RequireAdmin, but the user does not have this privilege, nor do they 
        /// have the ability to acquire or elevate to obtain this privilege.
        /// </exception>
        /// <exception cref="Win32Exception">
        /// There was an error launching the program.
        /// </exception>
        public static void Execute(
            IWin32Window parent, 
            string exePath, 
            string args,
            ExecutePrivilege execPrivilege, 
            ExecuteWaitType execWaitType)
        {
            if (exePath == null)
            {
                throw new ArgumentNullException("exePath");
            }

            if (execPrivilege == ExecutePrivilege.RequireAdmin && parent == null)
            {
                throw new ArgumentException("If requireAdmin is true, a parent window must be provided");
            }

            // If this action requires admin privilege, but the user does not have this
            // privilege and is not capable of acquiring this privilege, then we will
            // throw an exception.
            if (execPrivilege == ExecutePrivilege.RequireAdmin && 
                !Security.IsAdministrator && 
                !Security.CanElevateToAdministrator)
            {
                throw new SecurityException("Executable requires administrator privilege, but user is not an administrator and cannot elevate");
            }

            ExecuteHandOff executeHandOff = null;
            switch (execPrivilege)
            {
                case ExecutePrivilege.AsInvokerOrAsManifest:
                    executeHandOff = new ExecuteHandOff(ExecAsInvokerOrAsManifest);
                    break;

                case ExecutePrivilege.RequireAdmin:
                    executeHandOff = new ExecuteHandOff(ExecRequireAdmin);
                    break;

                case ExecutePrivilege.RequireNonAdminIfPossible:
                    if (Security.CanLaunchNonAdminProcess)
                    {
                        executeHandOff = new ExecuteHandOff(ExecRequireNonAdmin);
                    }
                    else
                    {
                        executeHandOff = new ExecuteHandOff(ExecAsInvokerOrAsManifest);
                    }
                    break;

                default:
                    throw new InvalidEnumArgumentException("ExecutePrivilege");
            }

            string updateMonitorExePath = null;
            if (execWaitType == ExecuteWaitType.RelaunchPdnOnExit)
            {
                RelaunchPdnHelperPart1(out updateMonitorExePath);
            }

            IntPtr hProcess = IntPtr.Zero;
            int nResult = executeHandOff(parent, exePath, args, out hProcess);

            if (nResult == NativeConstants.ERROR_SUCCESS)
            {
                if (execWaitType == ExecuteWaitType.WaitForExit)
                {
                    SafeNativeMethods.WaitForSingleObject(hProcess, NativeConstants.INFINITE);
                }
                else if (execWaitType == ExecuteWaitType.RelaunchPdnOnExit)
                {
                    bool bResult2 = SafeNativeMethods.SetHandleInformation(
                        hProcess, 
                        NativeConstants.HANDLE_FLAG_INHERIT, 
                        NativeConstants.HANDLE_FLAG_INHERIT);

                    RelaunchPdnHelperPart2(updateMonitorExePath, hProcess);

                    // Ensure that we don't close the process handle right away in the next few lines of code.
                    // It must be inherited by the child process. Yes, this is technically a leak but we are
                    // planning to terminate in just a moment anyway.
                    hProcess = IntPtr.Zero; 
                }
                else if (execWaitType == ExecuteWaitType.ReturnImmediately)
                {
                }

                if (hProcess != IntPtr.Zero)
                {
                    SafeNativeMethods.CloseHandle(hProcess);
                    hProcess = IntPtr.Zero;
                }
            }
            else
            {
                if (nResult == NativeConstants.ERROR_CANCELLED ||
                    nResult == NativeConstants.ERROR_TIMEOUT)
                {
                    // no problem
                }
                else
                {
                    NativeMethods.ThrowOnWin32Error("ExecuteHandoff failed", nResult);
                }

                if (updateMonitorExePath != null)
                {
                    try
                    {
                        File.Delete(updateMonitorExePath);
                    }

                    catch (Exception)
                    {
                    }

                    updateMonitorExePath = null;
                }
            }

            GC.KeepAlive(parent);
        }

        private static int ExecAsInvokerOrAsManifest(IWin32Window parent, string exePath, string args, out IntPtr hProcess)
        {
            return ExecShellExecuteEx(parent, exePath, args, null, out hProcess);
        }

        private static int ExecRequireAdmin(IWin32Window parent, string exePath, string args, out IntPtr hProcess)
        {
            const string runAs = "runas";
            string verb;

            if (Security.IsAdministrator)
            {
                verb = null;
            }
            else
            {
                verb = runAs;
            }

            return ExecShellExecuteEx(parent, exePath, args, verb, out hProcess);
        }

        private static int ExecRequireNonAdmin(IWin32Window parent, string exePath, string args, out IntPtr hProcess)
        {
            int nError = NativeConstants.ERROR_SUCCESS;
            string commandLine = "\"" + exePath + "\"" + (args == null ? "" : (" " + args));

            string dir;

            try
            {
                dir = Path.GetDirectoryName(exePath);
            }

            catch (Exception)
            {
                dir = null;
            }

            IntPtr hWndShell = IntPtr.Zero;
            IntPtr hShellProcess = IntPtr.Zero;
            IntPtr hShellProcessToken = IntPtr.Zero;
            IntPtr hTokenCopy = IntPtr.Zero;
            IntPtr bstrExePath = IntPtr.Zero;
            IntPtr bstrCommandLine = IntPtr.Zero;
            IntPtr bstrDir = IntPtr.Zero;
            NativeStructs.PROCESS_INFORMATION procInfo = new NativeStructs.PROCESS_INFORMATION();

            try
            {
                hWndShell = SafeNativeMethods.FindWindowW("Progman", null);
                if (hWndShell == IntPtr.Zero)
                {
                    NativeMethods.ThrowOnWin32Error("FindWindowW() returned NULL");
                }

                uint dwPID;
                uint dwThreadId = SafeNativeMethods.GetWindowThreadProcessId(hWndShell, out dwPID);
                if (0 == dwPID)
                {
                    NativeMethods.ThrowOnWin32Error("GetWindowThreadProcessId returned 0", NativeErrors.ERROR_FILE_NOT_FOUND);
                }

                hShellProcess = NativeMethods.OpenProcess(NativeConstants.PROCESS_QUERY_INFORMATION, false, dwPID);
                if (IntPtr.Zero == hShellProcess)
                {
                    NativeMethods.ThrowOnWin32Error("OpenProcess() returned NULL");
                }

                bool optResult = NativeMethods.OpenProcessToken(
                    hShellProcess,
                    NativeConstants.TOKEN_ASSIGN_PRIMARY | NativeConstants.TOKEN_DUPLICATE | NativeConstants.TOKEN_QUERY,
                    out hShellProcessToken);

                if (!optResult)
                {
                    NativeMethods.ThrowOnWin32Error("OpenProcessToken() returned FALSE");
                }

                bool dteResult = NativeMethods.DuplicateTokenEx(
                    hShellProcessToken,
                    NativeConstants.MAXIMUM_ALLOWED,
                    IntPtr.Zero,
                    NativeConstants.SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation,
                    NativeConstants.TOKEN_TYPE.TokenPrimary,
                    out hTokenCopy);

                if (!dteResult)
                {
                    NativeMethods.ThrowOnWin32Error("DuplicateTokenEx() returned FALSE");
                }

                bstrExePath = Marshal.StringToBSTR(exePath);
                bstrCommandLine = Marshal.StringToBSTR(commandLine);
                bstrDir = Marshal.StringToBSTR(dir);

                bool cpwtResult = NativeMethods.CreateProcessWithTokenW(
                    hTokenCopy,
                    0,
                    bstrExePath,
                    bstrCommandLine,
                    0,
                    IntPtr.Zero,
                    bstrDir,
                    IntPtr.Zero,
                    out procInfo);

                if (cpwtResult)
                {
                    hProcess = procInfo.hProcess;
                    procInfo.hProcess = IntPtr.Zero;
                    nError = NativeConstants.ERROR_SUCCESS;
                }
                else
                {
                    hProcess = IntPtr.Zero;
                    nError = Marshal.GetLastWin32Error();
                }
            }

            catch (Win32Exception ex)
            {
                Tracing.Ping(ex.ToString());
                nError = ex.ErrorCode;
                hProcess = IntPtr.Zero;
            }

            finally
            {
                if (bstrExePath != IntPtr.Zero)
                {
                    Marshal.FreeBSTR(bstrExePath);
                    bstrExePath = IntPtr.Zero;
                }

                if (bstrCommandLine != IntPtr.Zero)
                {
                    Marshal.FreeBSTR(bstrCommandLine);
                    bstrCommandLine = IntPtr.Zero;
                }

                if (bstrDir != IntPtr.Zero)
                {
                    Marshal.FreeBSTR(bstrDir);
                    bstrDir = IntPtr.Zero;
                }

                if (hShellProcess != IntPtr.Zero)
                {
                    SafeNativeMethods.CloseHandle(hShellProcess);
                    hShellProcess = IntPtr.Zero;
                }
                
                if (hShellProcessToken != IntPtr.Zero)
                {
                    SafeNativeMethods.CloseHandle(hShellProcessToken);
                    hShellProcessToken = IntPtr.Zero;
                }

                if (hTokenCopy != IntPtr.Zero)
                {
                    SafeNativeMethods.CloseHandle(hTokenCopy);
                    hTokenCopy = IntPtr.Zero;
                }

                if (procInfo.hThread != IntPtr.Zero)
                {
                    SafeNativeMethods.CloseHandle(procInfo.hThread);
                    procInfo.hThread = IntPtr.Zero;
                }

                if (procInfo.hProcess != IntPtr.Zero)
                {
                    SafeNativeMethods.CloseHandle(procInfo.hProcess);
                    procInfo.hProcess = IntPtr.Zero;
                }
            }

            return nError;
        }

        private static int ExecShellExecuteEx(IWin32Window parent, string exePath, string args, string verb, out IntPtr hProcess)
        {
            string dir;

            try
            {
                dir = Path.GetDirectoryName(exePath);
            }

            catch (Exception)
            {
                dir = null;
            }

            NativeStructs.SHELLEXECUTEINFO sei = new NativeStructs.SHELLEXECUTEINFO();
            sei.cbSize = (uint)Marshal.SizeOf(typeof(NativeStructs.SHELLEXECUTEINFO));

            sei.fMask =
                NativeConstants.SEE_MASK_NOCLOSEPROCESS |
                NativeConstants.SEE_MASK_NO_CONSOLE |
                NativeConstants.SEE_MASK_FLAG_DDEWAIT;

            sei.lpVerb = verb;
            sei.lpDirectory = dir;
            sei.lpFile = exePath;
            sei.lpParameters = args;

            sei.nShow = NativeConstants.SW_SHOWNORMAL;

            if (parent != null)
            {
                sei.hwnd = parent.Handle;
            }

            bool bResult = NativeMethods.ShellExecuteExW(ref sei);
            hProcess = sei.hProcess;
            sei.hProcess = IntPtr.Zero;

            int nResult = NativeConstants.ERROR_SUCCESS;

            if (!bResult)
            {
                nResult = Marshal.GetLastWin32Error();    
            }

            return nResult;
        }

        private static void RelaunchPdnHelperPart1(out string updateMonitorExePath)
        {
            string srcDir = Application.StartupPath;
            string srcPath = Path.Combine(srcDir, updateExeFileName);
            string srcPath2 = srcPath + ".config";

            string dstDir = Environment.ExpandEnvironmentVariables(@"%TEMP%\PdnSetup");
            string dstPath = Path.Combine(dstDir, updateExeFileName);
            string dstPath2 = dstPath + ".config";

            if (!Directory.Exists(dstDir))
            {
                Directory.CreateDirectory(dstDir);
            }

            File.Copy(srcPath, dstPath, true);
            File.Copy(srcPath2, dstPath2, true);
            updateMonitorExePath = dstPath;
        }

        private static void RelaunchPdnHelperPart2(string updateMonitorExePath, IntPtr hProcess)
        {
            string args = hProcess.ToInt64().ToString(CultureInfo.InstalledUICulture);
            ProcessStartInfo psi = new ProcessStartInfo(updateMonitorExePath, args);
            psi.UseShellExecute = false;
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            Process process = Process.Start(psi);
            process.Dispose();
        }

        /// <summary>
        /// Asynchronously restarts Paint.NET.
        /// </summary>
        /// <remarks>
        /// This method does not restart Paint.NET immediately. Instead, it waits
        /// for Paint.NET to terminate and then restarts it. It does not perform
        /// the termination or shutdown.
        /// </remarks>
        public static void RestartApplication()
        {
            string srcDir = Application.StartupPath;
            string updateExePath = Path.Combine(srcDir, updateExeFileName);

            Process thisProcess = Process.GetCurrentProcess();
            IntPtr hProcess = thisProcess.Handle;

            bool bResult = SafeNativeMethods.SetHandleInformation(
                hProcess,
                NativeConstants.HANDLE_FLAG_INHERIT,
                NativeConstants.HANDLE_FLAG_INHERIT);

            if (!bResult)
            {
                NativeMethods.ThrowOnWin32Error("SetHandleInformation() returned false");
            }

            RelaunchPdnHelperPart2(updateExePath, hProcess);
        }

        /// <summary>
        /// Launches the default browser and opens the given URL.
        /// </summary>
        /// <param name="url">The URL to show. The maximum length is 512 characters.</param>
        /// <remarks>
        /// This method will not present an error dialog if the URL could not be launched.
        /// Note: This method must only be used by Paint.NET, and not any plugins. It may
        /// change or be removed in future versions.
        /// </remarks>
        public static bool LaunchUrl(IWin32Window owner, string url)
        {
            if (url.Length > 512)
            {
                throw new ArgumentOutOfRangeException("url.Length must be <= 512");
            }

            bool success = false;
            string quotedUrl = "\"" + url + "\"";
            ExecutePrivilege executePrivilege;

            if (!Security.IsAdministrator || (Security.IsAdministrator && !Security.CanLaunchNonAdminProcess))
            {
                executePrivilege = ExecutePrivilege.AsInvokerOrAsManifest;
            }
            else
            {
                executePrivilege = ExecutePrivilege.RequireNonAdminIfPossible;
            }

            // Method 1. Just launch the url, and hope that the shell figures out the association correctly. 
            // This method will not work with ExecutePrivilege.RequireNonAdmin though.
            if (!success && executePrivilege != ExecutePrivilege.RequireNonAdminIfPossible)
            {
                try
                {
                    Execute(owner, quotedUrl, null, executePrivilege, ExecuteWaitType.ReturnImmediately);
                    success = true;
                }

                catch (Exception ex)
                {
                    Tracing.Ping("Exception while using method 1 to launch url, " + quotedUrl + ", :" + ex.ToString());
                    success = false;
                }
            }

            // Method 2. Launch the url through explorer
            if (!success)
            {
                const string shellFileLoc = @"%WINDIR%\explorer.exe";
                string shellExePath = "(n/a)";

                try
                {
                    shellExePath = Environment.ExpandEnvironmentVariables(shellFileLoc);
                    Execute(owner, shellExePath, quotedUrl, executePrivilege, ExecuteWaitType.ReturnImmediately);
                    success = true;
                }

                catch (Exception ex)
                {
                    Tracing.Ping("Exception while using method 2 to launch url through '" + shellExePath + "', " + quotedUrl + ", : " + ex.ToString());
                    success = false;
                }
            }

            return success;
        }

        [Obsolete("Use PdnInfo.OpenUrl() instead. Shell.LaunchUrl() must only be used by Paint.NET code, not by plugins.", true)]
        public static bool OpenUrl(IWin32Window owner, string url)
        {
            return LaunchUrl(owner, url);
        }

        public static void AddToRecentDocumentsList(string fileName)
        {
            // Apparently SHAddToRecentDocs can block for a very long period of time when certain
            // conditions are met: so we just stick it on "the backburner."
            ThreadPool.QueueUserWorkItem(new WaitCallback(AddToRecentDocumentsListImpl), fileName);
        }

        private static void AddToRecentDocumentsListImpl(object fileNameObj)
        {
            string fileName = (string)fileNameObj;
            IntPtr bstrFileName = IntPtr.Zero;

            try
            {
                bstrFileName = Marshal.StringToBSTR(fileName);
                NativeMethods.SHAddToRecentDocs(NativeConstants.SHARD_PATHW, bstrFileName);
            }

            finally
            {
                if (bstrFileName != IntPtr.Zero)
                {
                    Marshal.FreeBSTR(bstrFileName);
                    bstrFileName = IntPtr.Zero;
                }
            }
        }

        // TODO: convert to extension method in the 4.0 codebase, which can use .NET 3.5
        private static T2 Map<T1, T2>(T1 mapFrom, Pair<T1, T2>[] mappings)
        {
            foreach (Pair<T1, T2> mapping in mappings)
            {
                if (mapping.First.Equals(mapFrom))
                {
                    return mapping.Second;
                }
            }

            throw new KeyNotFoundException();
        }

        private static string GetCSIDLPath(int csidl, bool tryCreateIfAbsent)
        {
            // First, try calling SHGetFolderPathW with the "CSIDL_FLAG_CREATE" flag. However, if it 
            // returns an error then ignore it. We've had some crash logs with "access denied" coming
            // from this function.
            int csidlWithFlags = csidl | (tryCreateIfAbsent ? NativeConstants.CSIDL_FLAG_CREATE : 0);
            StringBuilder sbWithFlags = new StringBuilder(NativeConstants.MAX_PATH);
            Do.TryBool(() => NativeMethods.SHGetFolderPathW(IntPtr.Zero, csidlWithFlags, IntPtr.Zero, NativeConstants.SHGFP_TYPE_CURRENT, sbWithFlags));

            StringBuilder sb = new StringBuilder(NativeConstants.MAX_PATH);
            NativeMethods.SHGetFolderPathW(IntPtr.Zero, csidl, IntPtr.Zero, NativeConstants.SHGFP_TYPE_CURRENT, sb);

            // If we get back something like 'Z:' then we need to put a backslash on it.
            // Otherwise other path-related functions will freak out.
            if (sb.Length == 2 && sb[1] == ':')
            {
                sb.Append(Path.DirectorySeparatorChar);
            }

            string path = sb.ToString();

            return path;
        }

        private static readonly Pair<VirtualFolderName, int>[] pathMappings = new Pair<VirtualFolderName, int>[]
            {
                Pair.Create(VirtualFolderName.SystemProgramFiles, NativeConstants.CSIDL_PROGRAM_FILES),
                Pair.Create(VirtualFolderName.UserDesktop, NativeConstants.CSIDL_DESKTOP_DIRECTORY),
                Pair.Create(VirtualFolderName.UserDocuments, NativeConstants.CSIDL_PERSONAL),
                Pair.Create(VirtualFolderName.UserLocalAppData, NativeConstants.CSIDL_LOCAL_APPDATA),
                Pair.Create(VirtualFolderName.UserPictures, NativeConstants.CSIDL_MYPICTURES),
                Pair.Create(VirtualFolderName.UserRoamingAppData, NativeConstants.CSIDL_APPDATA) 
            };

        public static string GetVirtualPath(VirtualFolderName folderName, bool tryCreateIfAbsent)
        {
            try
            {
                int csidl = Map(folderName, pathMappings);
                string path = GetCSIDLPath(csidl, tryCreateIfAbsent);
                return path;
            }

            catch (KeyNotFoundException)
            {
                throw new InvalidEnumArgumentException("folderName", (int)folderName, typeof(VirtualFolderName));
            }
        }
    }
}
