/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

// I made this little utility because I was getting a lot of crash logs that
// said the strings or resources files were missing. Reinstalling almost 
// always fixed the problem. I had this happen on my own system a few times,
// but I could never figure out the cause. So what this utility does is
// run an MSI reinstall operation with a flag telling it to only replace
// missing files. The main PaintDotNet.exe can detect this situation of
// missing files and it then gives the user the ability to run this utility
// by clicking a button.
// This utility must have a UAC manifest for requiring administrator, and it
// should also be signed with Authenticode so that the UAC consent UI in
// Vista does not give horrible warnings.
// -Rick Brewster

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using System.Text;

namespace PdnRepair
{
    public sealed class Program
    {
        [DllImport("msi.dll", CharSet = CharSet.Unicode)]
        internal static extern uint MsiReinstallProductW(
            [MarshalAs(UnmanagedType.LPWStr)] string szProduct,
            uint dwReinstallMode);

        internal const uint REINSTALLMODE_REPAIR = 0x00000001;
        internal const uint REINSTALLMODE_FILEMISSING = 0x00000002;
        internal const uint REINSTALLMODE_FILEOLDERVERSION = 0x00000004;
        internal const uint REINSTALLMODE_FILEEQUALVERSION = 0x00000008;
        internal const uint REINSTALLMODE_FILEEXACT = 0x00000010;
        internal const uint REINSTALLMODE_FILEVERIFY = 0x00000020;
        internal const uint REINSTALLMODE_FILEREPLACE = 0x00000040;
        internal const uint REINSTALLMODE_MACHINEDATA = 0x00000080;
        internal const uint REINSTALLMODE_USERDATA = 0x00000100;
        internal const uint REINSTALLMODE_SHORTCUT = 0x00000200;
        internal const uint REINSTALLMODE_PACKAGE = 0x00000400;

        public static int Main(string[] args)
        {
            int returnVal = 0;

            try
            {
                returnVal = MainImpl(args);
            }

            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("--- Error: ");
                Console.WriteLine(ex.ToString());

                returnVal = -1;
            }

            if (args.Length == 0)
            {
                Console.WriteLine();
                Console.Write("Press Enter to exit...");
                Console.ReadLine();
            }

            return returnVal;
        }

        private static int MainImpl(string[] args)
        {
            bool success = true;
            int returnVal = 0;

            Console.WriteLine("Paint.NET Repair Tool");
            Console.WriteLine();

            if (success)
            {
                AppDomain domain = Thread.GetDomain();
                domain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);
                WindowsPrincipal principal = (WindowsPrincipal)Thread.CurrentPrincipal;
                bool isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);

                if (!isAdmin)
                {
                    Console.WriteLine("This utility must be run with administrator privilege.");
                    success = false;
                    returnVal = 740; // ERROR_ELEVATION_REQUIRED
                }
            }

            RegistryKey key = null;
            if (success)
            {
                Console.Write(@"* Opening registry key, HKEY_LOCAL_MACHINE\SOFTWARE\Paint.NET: ");
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Paint.NET", false);

                if (key != null)
                {
                    Console.WriteLine("ok");
                }
                else
                {
                    Console.WriteLine("null");
                    success = false;
                    returnVal = -1;
                }
            }

            string productCode = null;
            if (success)
            {
                Console.Write("* Retrieving MSI product code GUID: ");
                string productCodeString = (string)key.GetValue("ProductCode", null, RegistryValueOptions.DoNotExpandEnvironmentNames);
                Guid productCodeGuid = new Guid(productCodeString);
                productCode = productCodeGuid.ToString("B", CultureInfo.InvariantCulture).ToUpper(CultureInfo.InvariantCulture);
                Console.WriteLine(productCode);
            }

            if (success)
            {
                Console.Write("* Attempting to repair: ");
                uint dwResult = MsiReinstallProductW(productCode, REINSTALLMODE_FILEMISSING);

                if (dwResult == 0)
                {
                    Console.WriteLine("success");
                }
                else
                {
                    Console.WriteLine("failed, dwResult=" + dwResult.ToString());
                    returnVal = unchecked((int)dwResult);
                }
            }

            return returnVal;
        }
    }
}
