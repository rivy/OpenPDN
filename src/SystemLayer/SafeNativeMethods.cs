/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security;

namespace PaintDotNet.SystemLayer
{
    [SuppressUnmanagedCodeSecurity]
    internal static class SafeNativeMethods
    {
        [DllImport("kernel32.dll", SetLastError = false)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool IsProcessorFeaturePresent(uint ProcessorFeature);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DrawMenuBar(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = false)]
        internal static extern IntPtr GetSystemMenu(
            IntPtr hWnd,
            [MarshalAs(UnmanagedType.Bool)] bool bRevert);

        [DllImport("user32.dll", SetLastError = false)]
        internal static extern int EnableMenuItem(
            IntPtr hMenu,
            uint uIDEnableItem,
            uint uEnable);

        [DllImport("user32.dll", SetLastError = false)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool FlashWindow(
            IntPtr hWnd, 
            [MarshalAs(UnmanagedType.Bool)] bool bInvert);

        [DllImport("dwmapi.dll")]
        internal unsafe static extern int DwmGetWindowAttribute(
            IntPtr hwnd,
            uint dwAttribute,
            void* pvAttribute,
            uint cbAttribute);

        [DllImport("kernel32.dll", SetLastError = false)]
        internal static extern IntPtr GetCurrentThread();

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetThreadPriority(
            IntPtr hThread,
            int nPriority);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern IntPtr CreateFileMappingW(
            IntPtr hFile,
            IntPtr lpFileMappingAttributes,
            uint flProtect,
            uint dwMaximumSizeHigh,
            uint dwMaximumSizeLow,
            [MarshalAs(UnmanagedType.LPTStr)] string lpName);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern IntPtr MapViewOfFile(
            IntPtr hFileMappingObject,
            uint dwDesiredAccess,
            uint dwFileOffsetHigh,
            uint dwFileOffsetLow,
            UIntPtr dwNumberOfBytesToMap);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool UnmapViewOfFile(IntPtr lpBaseAddress);
       
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ShowScrollBar(
            IntPtr hWnd, 
            int wBar, 
            [MarshalAs(UnmanagedType.Bool)] bool bShow);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetVersionEx(ref NativeStructs.OSVERSIONINFOEX lpVersionInfo);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetLayeredWindowAttributes(
            IntPtr hwnd,
            out uint pcrKey,
            out byte pbAlpha,
            out uint pdwFlags);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", MessageId = "2")]
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetLayeredWindowAttributes(
            IntPtr hwnd,
            uint crKey,
            byte bAlpha,
            uint dwFlags);

        [DllImport("gdi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern IntPtr CreateFontW(
            int nHeight,              
            int nWidth,               
            int nEscapement,          
            int nOrientation,         
            int fnWeight,             
            uint fdwItalic,           
            uint fdwUnderline,        
            uint fdwStrikeOut,        
            uint fdwCharSet,          
            uint fdwOutputPrecision,  
            uint fdwClipPrecision,    
            uint fdwQuality,          
            uint fdwPitchAndFamily,   
            string lpszFace);
        
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern int DrawTextW(
            IntPtr hdc,
            string lpString,
            int nCount,
            ref NativeStructs.RECT lpRect,
            uint uFormat);

        [DllImport("gdi32.dll", SetLastError = true)]
        internal static extern IntPtr CreateDIBSection(
            IntPtr hdc,
            ref NativeStructs.BITMAPINFO pbmi,
            uint iUsage,
            out IntPtr ppvBits,
            IntPtr hSection,
            uint dwOffset);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern IntPtr CreateFileW(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal unsafe static extern bool WriteFile(
            IntPtr hFile,
            void *lpBuffer,
            uint nNumberOfBytesToWrite,
            out uint lpNumberOfBytesWritten,
            IntPtr lpOverlapped);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal unsafe static extern bool ReadFile(
            SafeFileHandle sfhFile,
            void *lpBuffer,
            uint nNumberOfBytesToRead,
            out uint lpNumberOfBytesRead,
            IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetHandleInformation(
            IntPtr hObject,
            uint dwMask,
            uint dwFlags);

        [DllImport("user32.dll", SetLastError = false)]
        internal static extern int GetUpdateRgn(
            IntPtr hWnd, 
            IntPtr hRgn, 
            [MarshalAs(UnmanagedType.Bool)] bool bErase);

        [DllImport("user32.dll", SetLastError = false)]
        internal static extern uint GetWindowThreadProcessId(
            IntPtr hWnd,
            out uint lpdwProcessId);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern IntPtr FindWindowW(
            [MarshalAs(UnmanagedType.LPWStr)] string lpClassName,
            [MarshalAs(UnmanagedType.LPWStr)] string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern IntPtr FindWindowExW(
            IntPtr hwndParent, 
            IntPtr hwndChildAfter, 
            [MarshalAs(UnmanagedType.LPWStr)] string lpszClass, 
            [MarshalAs(UnmanagedType.LPWStr)] string lpszWindow);

        [DllImport("user32.dll", SetLastError = false)]
        internal static extern IntPtr SendMessageW(
            IntPtr hWnd, 
            uint msg, 
            IntPtr wParam, 
            IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal extern static bool PostMessageW(
            IntPtr handle, 
            uint msg, 
            IntPtr wParam, 
            IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern uint GetWindowLongW(
            IntPtr hWnd,
            int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern uint SetWindowLongW(
            IntPtr hWnd,
            int nIndex,
            uint dwNewLong);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool QueryPerformanceCounter(out ulong lpPerformanceCount);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool QueryPerformanceFrequency(out ulong lpFrequency);

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        internal static extern unsafe void memcpy(void* dst, void* src, UIntPtr length);

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        internal static extern unsafe void memset(void* dst, int c, UIntPtr length);

        [DllImport("User32.dll", SetLastError = false)]
        internal static extern int GetSystemMetrics(int nIndex);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern uint WaitForSingleObject(
            IntPtr hHandle,
            uint dwMilliseconds);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern uint WaitForMultipleObjects(
            uint nCount,
            IntPtr[] lpHandles,
            [MarshalAs(UnmanagedType.Bool)] bool bWaitAll,
            uint dwMilliseconds);

        internal static uint WaitForMultipleObjects(IntPtr[] lpHandles, bool bWaitAll, uint dwMilliseconds)
        {
            return WaitForMultipleObjects((uint)lpHandles.Length, lpHandles, bWaitAll, dwMilliseconds);
        }

        [DllImport("wtsapi32.dll", SetLastError = true)]
        internal static extern uint WTSRegisterSessionNotification(IntPtr hWnd, uint dwFlags);

        [DllImport("wtsapi32.dll", SetLastError = true)]
        internal static extern uint WTSUnRegisterSessionNotification(IntPtr hWnd);

        [DllImport("Gdi32.dll", SetLastError = true)]
        internal unsafe static extern uint GetRegionData(
            IntPtr hRgn,
            uint dwCount,
            NativeStructs.RGNDATA *lpRgnData);

        [DllImport("Gdi32.dll", SetLastError = true)]
        internal unsafe static extern IntPtr CreateRectRgn(
            int nLeftRect,
            int nTopRect,
            int nRightRect,
            int nBottomRect);

        [DllImport("Gdi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal extern static bool MoveToEx(
            IntPtr hdc,
            int X,
            int Y,
            out NativeStructs.POINT lpPoint);

        [DllImport("Gdi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal extern static bool LineTo(
            IntPtr hdc,
            int nXEnd,
            int nYEnd);

        [DllImport("User32.dll", SetLastError = true)]
        internal extern static int FillRect(
            IntPtr hDC,
            ref NativeStructs.RECT lprc,
            IntPtr hbr);

        [DllImport("Gdi32.dll", SetLastError = true)]
        internal extern static IntPtr CreatePen(
            int fnPenStyle,
            int nWidth,
            uint crColor);

        [DllImport("Gdi32.dll", SetLastError = true)]
        internal extern static IntPtr CreateSolidBrush(uint crColor);

        [DllImport("Gdi32.dll", SetLastError = false)]
        internal extern static IntPtr SelectObject(
            IntPtr hdc,
            IntPtr hgdiobj);

        [DllImport("Gdi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal extern static bool DeleteObject(IntPtr hObject);

        [DllImport("Gdi32.dll", SetLastError = true)]
        internal extern static uint DeleteDC(IntPtr hdc);

        [DllImport("Gdi32.Dll", SetLastError = true)]
        internal extern static IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("Gdi32.Dll", SetLastError = true)]
        internal extern static uint BitBlt(
            IntPtr hdcDest, 
            int nXDest,     
            int nYDest,     
            int nWidth,     
            int nHeight,    
            IntPtr hdcSrc,  
            int nXSrc,      
            int nYSrc,      
            uint dwRop);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr VirtualAlloc(
            IntPtr lpAddress, 
            UIntPtr dwSize, 
            uint flAllocationType, 
            uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool VirtualFree(
            IntPtr lpAddress, 
            UIntPtr dwSize, 
            uint dwFreeType);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool VirtualProtect(
            IntPtr lpAddress, 
            UIntPtr dwSize, 
            uint flNewProtect, 
            out uint lpflOldProtect);
        
        [DllImport("Kernel32.dll", SetLastError = false)]
        internal static extern IntPtr HeapAlloc(IntPtr hHeap, uint dwFlags, UIntPtr dwBytes);

        [DllImport("Kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool HeapFree(IntPtr hHeap, uint dwFlags, IntPtr lpMem);

        [DllImport("Kernel32.dll", SetLastError = false)]
        internal static extern UIntPtr HeapSize(IntPtr hHeap, uint dwFlags, IntPtr lpMem);

        [DllImport("Kernel32.dll", SetLastError = true)]
        internal static extern IntPtr HeapCreate(
            uint flOptions,
            [MarshalAs(UnmanagedType.SysUInt)] IntPtr dwInitialSize,
            [MarshalAs(UnmanagedType.SysUInt)] IntPtr dwMaximumSize
            );

        [DllImport("Kernel32.dll", SetLastError = true)]
        internal static extern uint HeapDestroy(IntPtr hHeap);

        [DllImport("Kernel32.Dll", SetLastError = true)]
        internal unsafe static extern uint HeapSetInformation(
            IntPtr HeapHandle,
            int HeapInformationClass,
            void *HeapInformation,
            uint HeapInformationLength
            );

        [DllImport("winhttp.dll", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool WinHttpGetIEProxyConfigForCurrentUser(ref NativeStructs.WINHTTP_CURRENT_USER_IE_PROXY_CONFIG pProxyConfig);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr GlobalFree(IntPtr hMem);

        [DllImport("user32.dll", SetLastError = false)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = false)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = false)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    }
}
