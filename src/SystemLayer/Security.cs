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
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;

namespace PaintDotNet.SystemLayer
{
    /// <summary>
    /// Security related static methods and properties.
    /// </summary>
    public static class Security
    {
        private static bool isAdmin = GetIsAdministrator();

        private static bool GetIsAdministrator()
        {
            AppDomain domain = Thread.GetDomain();
            domain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);
            WindowsPrincipal principal = (WindowsPrincipal)Thread.CurrentPrincipal;
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        /// <summary>
        /// Gets a flag indicating whether the user has administrator-level privileges.
        /// </summary>
        /// <remarks>
        /// This is used to control access to actions that require the user to be an administrator.
        /// An example is checking for and installing updates, actions which are not normally able
        /// to be performed by normal or "limited" users. A user must also be an administrator in
        /// order to write to any Settings.SystemWide entries.
        /// </remarks>
        public static bool IsAdministrator
        {
            get
            {
                return isAdmin;
            }
        }

        /// <summary>
        /// Gets a flag indicating whether the current user is able to elevate to obtain
        /// administrator-level privileges.
        /// </summary>
        /// <remarks>
        /// This flag has no meaning if IsAdministrator returns true.
        /// This flag indicates whether a new process may be spawned which has administrator
        /// privilege. It does not indicate the ability to elevate the current process to
        /// administrator privilege. For Windows this indicates that the user is running
        /// Vista and has UAC enabled. This property should be used instead of checking
        /// the OS version anytime this check must be performed.
        /// Note to implementors: This may be written to simply return false.
        /// </remarks>
        public static bool CanElevateToAdministrator
        {
            get
            {
                if (OS.IsVistaOrLater && !Security.IsAdministrator)
                {
                    return IsUacEnabled;
                }
                else
                {
                    return false;
                }
            }
        }

        private static bool IsUacEnabled
        {
            get
            {
                bool returnVal = false;
                const string keyName = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System";
                const string valueName = "EnableLUA";
                
                try
                {
                    if (Environment.OSVersion.Version >= OS.WindowsVista)
                    {
                        using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyName, false))
                        {
                            if (key != null)
                            {
                                RegistryValueKind valueKind = key.GetValueKind(valueName);

                                if (valueKind == RegistryValueKind.DWord)
                                {
                                    int value = unchecked((int)key.GetValue(valueName));
                                    returnVal = (value == 1);
                                }
                            }
                        }
                    }
                }

                catch (Exception ex)
                {
                    Tracing.Ping(ex.ToString());
                    returnVal = false;
                }

                return returnVal;            }
        }

        /// <summary>
        /// If IsAdministrator is true, this returns true if we can launch a process with limited privilege.
        /// </summary>
        /// <remarks>
        /// Here's the truth table for this:
        /// Windows XP + Admin User -> false
        /// Windows XP + Standard User -> true
        /// Windows Vista + Admin User + UAC Enabled -> true
        /// Windows Vista + Admin User + UAC Disabled -> false
        /// Windows Vista + Standard User -> true
        /// </remarks>
        public static bool CanLaunchNonAdminProcess
        {
            get
            {
                if (!Security.IsAdministrator)
                {
                    return true;
                }
                else if (OS.IsVistaOrLater)
                {
                    return Security.IsUacEnabled;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Verifies that a file has a valid digital signature.
        /// </summary>
        /// <param name="owner">The parent/owner window for any UI that may be shown.</param>
        /// <param name="fileName">The path to the file to be validate.</param>
        /// <param name="showNegativeUI">Whether or not to show a UI in the case that the signature can not be found or validated.</param>
        /// <param name="showPositiveUI">Whether or not to show a UI in the case that the signature is successfully found and validated.</param>
        /// <returns>true if the file has a digital signature that validates up to a trusted root, or false otherwise</returns>
        public static bool VerifySignedFile(IWin32Window owner, string fileName, bool showNegativeUI, bool showPositiveUI)
        {
            unsafe
            {
                fixed (char *szFileName = fileName)
                {
                    Guid pgActionID = NativeConstants.WINTRUST_ACTION_GENERIC_VERIFY_V2;
                
                    NativeStructs.WINTRUST_FILE_INFO fileInfo = new NativeStructs.WINTRUST_FILE_INFO();
                    fileInfo.cbStruct = (uint)sizeof(NativeStructs.WINTRUST_FILE_INFO);
                    fileInfo.pcwszFilePath = szFileName;

                    NativeStructs.WINTRUST_DATA wintrustData = new NativeStructs.WINTRUST_DATA();
                    wintrustData.cbStruct = (uint)sizeof(NativeStructs.WINTRUST_DATA);

                    if (!showNegativeUI && !showPositiveUI)
                    {
                        wintrustData.dwUIChoice = NativeConstants.WTD_UI_NONE;
                    }
                    else if (!showNegativeUI && showPositiveUI)
                    {
                        wintrustData.dwUIChoice = NativeConstants.WTD_UI_NOBAD;
                    }
                    else if (showNegativeUI && !showPositiveUI)
                    {
                        wintrustData.dwUIChoice = NativeConstants.WTD_UI_NOGOOD;
                    }
                    else // if (showNegativeUI && showPositiveUI)
                    {
                        wintrustData.dwUIChoice = NativeConstants.WTD_UI_ALL;
                    }

                    wintrustData.fdwRevocationChecks = NativeConstants.WTD_REVOKE_WHOLECHAIN;
                    wintrustData.dwUnionChoice = NativeConstants.WTD_CHOICE_FILE;
                    wintrustData.pInfo = (void *)&fileInfo;

                    IntPtr handle;

                    if (owner == null)
                    {
                        handle = IntPtr.Zero;
                    }
                    else
                    {
                        handle = owner.Handle;
                    }

                    int result = NativeMethods.WinVerifyTrust(handle, ref pgActionID, ref wintrustData);

                    GC.KeepAlive(owner);
                    return result >= 0;
                }
            }
        }
    }
}
