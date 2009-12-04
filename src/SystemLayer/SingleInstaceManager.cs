/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PaintDotNet.SystemLayer
{
    /// <summary>
    /// Provides a way to manage and communicate between instances of an application
    /// in the same user session.
    /// </summary>
    public sealed class SingleInstanceManager
        : IDisposable
    {
        private const int mappingSize = 8; // sizeof(int64)
        private string mappingName;
        private Form window = null;
        private IntPtr hWnd = IntPtr.Zero;
        private IntPtr hFileMapping;
        private List<string> pendingInstanceMessages = new List<string>();
        private bool isFirstInstance;

        public bool IsFirstInstance
        {
            get
            {
                return this.isFirstInstance;
            }
        }

        public bool AreMessagesPending
        {
            get
            {
                lock (this.pendingInstanceMessages)
                {
                    return (this.pendingInstanceMessages.Count > 0);
                }
            }
        }

        public void SetWindow(Form newWindow)
        {
            if (this.window != null)
            {
                UnregisterWindow();
            }

            RegisterWindow(newWindow);
        }

        private void UnregisterWindow()
        {
            if (this.window != null)
            {
                this.window.HandleCreated -= new EventHandler(Window_HandleCreated);
                this.window.HandleDestroyed -= new EventHandler(Window_HandleDestroyed);
                this.window.Disposed -= new EventHandler(Window_Disposed);
                WriteHandleValueToMappedFile(IntPtr.Zero);
                this.hWnd = IntPtr.Zero;
                this.window = null;
            }
        }

        private void RegisterWindow(Form newWindow)
        {
            this.window = newWindow;

            if (this.window != null)
            {
                this.window.HandleCreated += new EventHandler(Window_HandleCreated);
                this.window.HandleDestroyed += new EventHandler(Window_HandleDestroyed);
                this.window.Disposed += new EventHandler(Window_Disposed);

                if (this.window.IsHandleCreated)
                {
                    this.hWnd = this.window.Handle;
                    WriteHandleValueToMappedFile(this.hWnd);
                }
            }

            GC.KeepAlive(newWindow);
        }

        private void Window_Disposed(object sender, EventArgs e)
        {
            UnregisterWindow();
        }

        private void Window_HandleDestroyed(object sender, EventArgs e)
        {
            UnregisterWindow();
        }

        private void Window_HandleCreated(object sender, EventArgs e)
        {
            this.hWnd = this.window.Handle;
            WriteHandleValueToMappedFile(this.hWnd);
            GC.KeepAlive(this.window);
        }

        public string[] GetPendingInstanceMessages()
        {
            string[] messages;

            lock (this.pendingInstanceMessages)
            {
                messages = this.pendingInstanceMessages.ToArray();
                this.pendingInstanceMessages.Clear();
            }

            return messages;
        }

        public event EventHandler InstanceMessageReceived;
        private void OnInstanceMessageReceived()
        {
            if (InstanceMessageReceived != null)
            {
                InstanceMessageReceived(this, EventArgs.Empty);
            }
        }

        public void SendInstanceMessage(string text)
        {
            SendInstanceMessage(text, 1);
        }

        public void SendInstanceMessage(string text, int timeoutSeconds)
        {
            IntPtr ourHwnd = IntPtr.Zero;
            DateTime now = DateTime.Now;
            DateTime timeoutTime = DateTime.Now + new TimeSpan(0, 0, 0, timeoutSeconds);

            while (ourHwnd == IntPtr.Zero && now < timeoutTime)
            {
                ourHwnd = ReadHandleFromFromMappedFile();
                now = DateTime.Now;

                if (ourHwnd == IntPtr.Zero)
                {
                    System.Threading.Thread.Sleep(100);
                }
            }

            if (ourHwnd != IntPtr.Zero)
            {
                NativeStructs.COPYDATASTRUCT copyDataStruct = new NativeStructs.COPYDATASTRUCT();
                IntPtr szText = IntPtr.Zero;

                try
                {
                    unsafe
                    {
                        szText = Marshal.StringToCoTaskMemUni(text);
                        copyDataStruct.dwData = UIntPtr.Zero;
                        copyDataStruct.lpData = szText;
                        copyDataStruct.cbData = (uint)(2 * (1 + text.Length));
                        IntPtr lParam = new IntPtr((void*)&copyDataStruct);

                        SafeNativeMethods.SendMessageW(ourHwnd, NativeConstants.WM_COPYDATA, this.hWnd, lParam);
                    }
                }

                finally
                {
                    if (szText != IntPtr.Zero)
                    {
                        Marshal.FreeCoTaskMem(szText);
                        szText = IntPtr.Zero;
                    }
                }
            }
        }

        public void FocusFirstInstance()
        {
            IntPtr ourHwnd = this.ReadHandleFromFromMappedFile();

            if (ourHwnd != IntPtr.Zero)
            {
                if (SafeNativeMethods.IsIconic(ourHwnd))
                {
                    SafeNativeMethods.ShowWindow(ourHwnd, NativeConstants.SW_RESTORE);
                }

                SafeNativeMethods.SetForegroundWindow(ourHwnd);
            }
        }

        public void FilterMessage(ref Message m)
        {
            if (m.Msg == NativeConstants.WM_COPYDATA)
            {
                unsafe
                {
                    NativeStructs.COPYDATASTRUCT* pCopyDataStruct = (NativeStructs.COPYDATASTRUCT*)m.LParam.ToPointer();
                    string message = Marshal.PtrToStringUni(pCopyDataStruct->lpData);

                    lock (this.pendingInstanceMessages)
                    {
                        this.pendingInstanceMessages.Add(message);
                    }

                    OnInstanceMessageReceived();
                }
            }
        }

        public SingleInstanceManager(string moniker)
        {
            int error = NativeConstants.ERROR_SUCCESS;

            if (moniker.IndexOf('\\') != -1)
            {
                throw new ArgumentException("moniker must not have a backslash character");
            }

            this.mappingName = "Local\\" + moniker;

            this.hFileMapping = SafeNativeMethods.CreateFileMappingW(
                NativeConstants.INVALID_HANDLE_VALUE,
                IntPtr.Zero,
                NativeConstants.PAGE_READWRITE | NativeConstants.SEC_COMMIT,
                0,
                mappingSize,
                mappingName);

            error = Marshal.GetLastWin32Error();

            if (this.hFileMapping == IntPtr.Zero)
            {
                throw new Win32Exception(error, "CreateFileMappingW() returned NULL (" + error.ToString() + ")");
            }

            this.isFirstInstance = (error != NativeConstants.ERROR_ALREADY_EXISTS);
        }

        private void WriteHandleValueToMappedFile(IntPtr hValue)
        {
            int error = NativeConstants.ERROR_SUCCESS;
            bool bResult = true;

            IntPtr lpData = SafeNativeMethods.MapViewOfFile(
                this.hFileMapping,
                NativeConstants.FILE_MAP_WRITE,
                0,
                0,
                new UIntPtr((uint)mappingSize));

            error = Marshal.GetLastWin32Error();

            if (lpData == IntPtr.Zero)
            {
                throw new Win32Exception(error, "MapViewOfFile() returned NULL (" + error + ")");
            }

            long int64 = hValue.ToInt64();
            byte[] int64Bytes = new byte[(int)mappingSize];

            for (int i = 0; i < mappingSize; ++i)
            {
                int64Bytes[i] = (byte)((int64 >> (i * 8)) & 0xff);
            }

            Marshal.Copy(int64Bytes, 0, lpData, mappingSize);

            bResult = SafeNativeMethods.UnmapViewOfFile(lpData);
            error = Marshal.GetLastWin32Error();

            if (!bResult)
            {
                throw new Win32Exception(error, "UnmapViewOfFile() returned FALSE (" + error + ")");
            }
        }

        private IntPtr ReadHandleFromFromMappedFile()
        {
            int error = NativeConstants.ERROR_SUCCESS;

            IntPtr lpData = SafeNativeMethods.MapViewOfFile(
                this.hFileMapping,
                NativeConstants.FILE_MAP_READ,
                0,
                0,
                new UIntPtr((uint)mappingSize));

            error = Marshal.GetLastWin32Error();

            if (lpData == IntPtr.Zero)
            {
                throw new Win32Exception(error, "MapViewOfFile() returned NULL (" + error + ")");
            }

            byte[] int64Bytes = new byte[(int)mappingSize];
            Marshal.Copy(lpData, int64Bytes, 0, mappingSize);

            long int64 = 0;
            for (int i = 0; i < mappingSize; ++i)
            {
                int64 += (long)(int64Bytes[i] << (i * 8));
            }

            bool bResult = SafeNativeMethods.UnmapViewOfFile(lpData);
            error = Marshal.GetLastWin32Error();

            if (!bResult)
            {
                throw new Win32Exception(error, "UnmapViewOfFile() returned FALSE (" + error + ")");
            }

            IntPtr hValue = new IntPtr(int64);
            return hValue;
        }

        ~SingleInstanceManager()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                UnregisterWindow();
            }

            if (this.hFileMapping != IntPtr.Zero)
            {
                SafeNativeMethods.CloseHandle(this.hFileMapping);
                this.hFileMapping = IntPtr.Zero;
            }
        }
    }        
}