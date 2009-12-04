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
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;

namespace PaintDotNet
{
    public static class UpdateMonitor
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetExitCodeProcess(IntPtr hProcess, out uint lpExitCode);

        private const uint ERROR_SUCCESS = 0;
        private const uint INFINITE = 0xffffffff;
        private const uint WAIT_ABANDONED = 0x00000080;
        private const uint WAIT_OBJECT_0 = 0;
        private const uint WAIT_TIMEOUT = 0x00000102;
        private const uint WAIT_FAILED = 0xffffffff;
        private const uint STILL_ACTIVE = 259;

        public static int Main(string[] args)
        {
            try
            {
                int returnValue = MainImpl(args);
                return returnValue;
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return 1;
            }
        }

        private static int MainImpl(string[] args)
        {
            Console.WriteLine("Command line is: " + Environment.CommandLine);

            uint dwResult = ERROR_SUCCESS;
            bool bResult = true;

            if (args.Length != 1)
            {
                // Must specify a handle value
                return 1;
            }

            ulong uHandleValue = ulong.Parse(args[0], CultureInfo.InvariantCulture);
            Console.WriteLine("Handle value is: " + uHandleValue);

            long handleValue = unchecked((long)uHandleValue);
            IntPtr hProcess = new IntPtr(handleValue);

            // Wait for the given process to finish
            Console.Write("Waiting for process to exit ... ");
            dwResult = WaitForSingleObject(hProcess, INFINITE);
            Console.WriteLine("done");

            // Get its exit code
            uint dwExitCode = 0;
            Console.Write("Retrieving process exit code ... ");
            bResult = GetExitCodeProcess(hProcess, out dwExitCode);

            if (!bResult)
            {
                int error = Marshal.GetLastWin32Error();
                throw new Win32Exception(error, "GetExitCodeProcess returned false, and error " + error);
            }

            if (dwExitCode == STILL_ACTIVE)
            {
                throw new ApplicationException("process was still active even though WaitForSingleObject() completed");
            }

            if (dwExitCode != 0)
            {
                throw new ApplicationException("process did not complete successfully, its exit code was " + dwExitCode);
            }

            Console.WriteLine("exit code was " + dwExitCode);

            // Retrieve paint.net's location from the registry
            Console.Write(@"Retrieving install directory from registry, HKLM \ Software \ Paint.NET \ TARGETDIR ... ");

            const string subKeyName = @"SOFTWARE\Paint.NET";
            RegistryKey key = Registry.LocalMachine.OpenSubKey(subKeyName, false);

            if (key == null)
            {
                throw new ApplicationException("registry key HKLM\\" + subKeyName + " could not be opened: OpenSubKey returned null");
            }

            const string valueName = "TARGETDIR";
            object targetDirObj = key.GetValue(valueName, null, RegistryValueOptions.DoNotExpandEnvironmentNames);

            if (targetDirObj == null)
            {
                throw new ApplicationException(valueName + " value was retrieved from registry as null");
            }

            string targetDir = targetDirObj as string;

            if (targetDir == null)
            {
                throw new ApplicationException(valueName + " was not a string; its retrieved .NET type was " + targetDirObj.GetType().Name + ", and ToString() value was " + targetDirObj.ToString());
            }

            Console.WriteLine(targetDir);

            // Validate as dir name
            Console.Write("Validating directory name ... ");
            if (!Directory.Exists(targetDir))
            {
                throw new ApplicationException("Directory.Exists(" + targetDir + ") returned false");
            }

            Console.WriteLine("done");

            // Determine the exe that we will be launching
            Console.Write("Building executable path to launch: ");

            const string pdnExe = "PaintDotNet.exe";
            string pdnExePath = Path.Combine(targetDir, pdnExe);

            Console.WriteLine(pdnExePath);

            // Validate as path/exe name
            Console.Write("Validating path name ... ");

            if (!File.Exists(pdnExePath))
            {
                throw new ApplicationException("File.Exists(" + pdnExePath + ") returned false");
            }

            // Launch it
            Console.Write("Executing: ");
            ProcessStartInfo psi = new ProcessStartInfo(pdnExePath);
            psi.UseShellExecute = true;
            psi.WorkingDirectory = targetDir;

            Process process = Process.Start(psi);

            Console.WriteLine("success. PID = " + process.Id);
            process.Dispose();

            return 0;
        }
    }
}
