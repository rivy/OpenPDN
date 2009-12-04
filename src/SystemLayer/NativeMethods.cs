/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace PaintDotNet.SystemLayer
{
    internal static class NativeMethods
    {
        internal static bool SUCCEEDED(int hr)
        {
            return hr >= 0;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        internal static extern void SHGetFolderPathW(
            IntPtr hwndOwner,
            int nFolder,
            IntPtr hToken,
            uint dwFlags,
            StringBuilder lpszPath);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteFileW(
            [MarshalAs(UnmanagedType.LPWStr)] string lpFileName);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool RemoveDirectoryW(
            [MarshalAs(UnmanagedType.LPWStr)] string lpPathName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint WaitForInputIdle(
            IntPtr hProcess,
            uint dwMilliseconds);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumWindows(
            [MarshalAs(UnmanagedType.FunctionPtr)] NativeDelegates.EnumWindowsProc lpEnumFunc,
            IntPtr lParam);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(
            uint dwDesiredAccess,
            [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle,
            uint dwProcessId);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool OpenProcessToken(
            IntPtr ProcessHandle,
            uint DesiredAccess,
            out IntPtr TokenHandle);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DuplicateTokenEx(
            IntPtr hExistingToken,
            uint dwDesiredAccess,
            IntPtr lpTokenAttributes,
            NativeConstants.SECURITY_IMPERSONATION_LEVEL ImpersonationLevel,
            NativeConstants.TOKEN_TYPE TokenType,
            out IntPtr phNewToken);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CreateProcessWithTokenW(
            IntPtr hToken,
            uint dwLogonFlags,
            IntPtr lpApplicationName,
            IntPtr lpCommandLine,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            IntPtr lpCurrentDirectory,
            IntPtr lpStartupInfo,
            out NativeStructs.PROCESS_INFORMATION lpProcessInfo);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseClipboard();

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetClipboardData(uint format);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsClipboardFormatAvailable(uint format);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        internal static extern void SHCreateItemFromParsingName(
            [MarshalAs(UnmanagedType.LPWStr)] string pszPath,
            IntPtr pbc,
            ref Guid riid,
            out IntPtr ppv);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool VerifyVersionInfo(
            ref NativeStructs.OSVERSIONINFOEX lpVersionInfo,
            uint dwTypeMask,
            ulong dwlConditionMask);

        [DllImport("kernel32.dll")]
        internal static extern ulong VerSetConditionMask(
            ulong dwlConditionMask,
            uint dwTypeBitMask,
            byte dwConditionMask);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeviceIoControl(
            IntPtr hDevice,
            uint dwIoControlCode,
            IntPtr lpInBuffer,
            uint nInBufferSize,
            IntPtr lpOutBuffer,
            uint nOutBufferSize,
            ref uint lpBytesReturned,
            IntPtr lpOverlapped);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ShellExecuteExW(ref NativeStructs.SHELLEXECUTEINFO lpExecInfo);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GlobalMemoryStatusEx(ref NativeStructs.MEMORYSTATUSEX lpBuffer);

        [DllImport("shell32.dll", SetLastError = false)]
        internal static extern void SHAddToRecentDocs(uint uFlags, IntPtr pv);

        [DllImport("kernel32.dll", SetLastError = false)]
        internal static extern void GetSystemInfo(ref NativeStructs.SYSTEM_INFO lpSystemInfo);

        [DllImport("kernel32.dll", SetLastError = false)]
        internal static extern void GetNativeSystemInfo(ref NativeStructs.SYSTEM_INFO lpSystemInfo);

        [DllImport("Wintrust.dll", PreserveSig = true, SetLastError = false)]
        internal extern static unsafe int WinVerifyTrust(
            IntPtr hWnd,
            ref Guid pgActionID,
            ref NativeStructs.WINTRUST_DATA pWinTrustData
            );

        [DllImport("SetupApi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern IntPtr SetupDiGetClassDevsW(
            ref Guid ClassGuid,
            [MarshalAs(UnmanagedType.LPWStr)] string Enumerator,
            IntPtr hwndParent,
            uint Flags);

        [DllImport("SetupApi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

        [DllImport("SetupApi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetupDiEnumDeviceInfo(
            IntPtr DeviceInfoSet,
            uint MemberIndex,
            ref NativeStructs.SP_DEVINFO_DATA DeviceInfoData);

        [DllImport("SetupApi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetupDiGetDeviceInstanceIdW(
            IntPtr DeviceInfoSet,
            ref NativeStructs.SP_DEVINFO_DATA DeviceInfoData,
            IntPtr DeviceInstanceId,
            uint DeviceInstanceIdSize,
            out uint RequiredSize);

        internal static void ThrowOnWin32Error(string message)
        {
            int lastWin32Error = Marshal.GetLastWin32Error();
            ThrowOnWin32Error(message, lastWin32Error);
        }

        internal static void ThrowOnWin32Error(string message, NativeErrors lastWin32Error)
        {
            ThrowOnWin32Error(message, (int)lastWin32Error);
        }

        internal static void ThrowOnWin32Error(string message, int lastWin32Error)
        {
            if (lastWin32Error != NativeConstants.ERROR_SUCCESS)
            {
                string exMessageFormat = "{0} ({1}, {2})";
                string exMessage = string.Format(exMessageFormat, message, lastWin32Error, ((NativeErrors)lastWin32Error).ToString());

                throw new Win32Exception(lastWin32Error, exMessage);
            }
        }
    }
}
