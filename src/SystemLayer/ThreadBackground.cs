/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.Threading;

namespace PaintDotNet.SystemLayer
{
    // Current implementation deficiency: The interface is such that an implied push/pop of
    // flags is presented. However, once you 'push' a background mode, it is not 'popped'
    // until all ThreadBackground objects have been disposed on the current thread.

    public sealed class ThreadBackground
        : IDisposable
    {
        [ThreadStatic]
        private static int count = 0;

        [ThreadStatic]
        private ThreadBackgroundFlags activeFlags = ThreadBackgroundFlags.None;

        private Thread currentThread;
        private ThreadPriority oldThreadPriority;
        private ThreadBackgroundFlags flags;

        public ThreadBackground(ThreadBackgroundFlags flags)
        {
            this.flags = flags;
            this.currentThread = Thread.CurrentThread;
            this.oldThreadPriority = this.currentThread.Priority;

            if ((flags & ThreadBackgroundFlags.Cpu) == ThreadBackgroundFlags.Cpu &&
                (activeFlags & ThreadBackgroundFlags.Cpu) != ThreadBackgroundFlags.Cpu)
            {
                this.currentThread.Priority = ThreadPriority.BelowNormal;
                activeFlags |= ThreadBackgroundFlags.Cpu;
            }

            if (Environment.OSVersion.Version >= OS.WindowsVista &&
                (flags & ThreadBackgroundFlags.IO) == ThreadBackgroundFlags.IO &&
                (activeFlags & ThreadBackgroundFlags.IO) != ThreadBackgroundFlags.IO)
            {
                IntPtr hThread = SafeNativeMethods.GetCurrentThread();
                bool bResult = SafeNativeMethods.SetThreadPriority(hThread, NativeConstants.THREAD_MODE_BACKGROUND_BEGIN);

                if (!bResult)
                {
                    NativeMethods.ThrowOnWin32Error("SetThreadPriority(THREAD_MODE_BACKGROUND_BEGIN) returned FALSE");
                }
            }

            activeFlags |= flags;

            ++count;
        }

        ~ThreadBackground()
        {
            Debug.Assert(false, "ThreadBackgroundMode() object must be manually Disposed()");
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            --count;

            if (Thread.CurrentThread.ManagedThreadId != this.currentThread.ManagedThreadId)
            {
                throw new InvalidOperationException("Dispose() was called on a thread other than the one that this object was created on");
            }

            if (count == 0)
            {
                if ((activeFlags & ThreadBackgroundFlags.Cpu) == ThreadBackgroundFlags.Cpu)
                {
                    this.currentThread.Priority = this.oldThreadPriority;
                    activeFlags &= ~ThreadBackgroundFlags.Cpu;
                }

                if (Environment.OSVersion.Version >= OS.WindowsVista &&
                    (activeFlags & ThreadBackgroundFlags.IO) == ThreadBackgroundFlags.IO)
                {
                    IntPtr hThread = SafeNativeMethods.GetCurrentThread();
                    bool bResult = SafeNativeMethods.SetThreadPriority(hThread, NativeConstants.THREAD_MODE_BACKGROUND_END);

                    if (!bResult)
                    {
                        NativeMethods.ThrowOnWin32Error("SetThreadPriority(THREAD_MODE_BACKGROUND_END) returned FALSE");
                    }

                    activeFlags &= ~ThreadBackgroundFlags.IO;
                }
            }
        }
    }
}
