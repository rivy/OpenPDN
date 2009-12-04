/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;

namespace PaintDotNet.SystemLayer
{
    /// <summary>
    /// Sometimes you need a Graphics instance when you don't really have access to one.
    /// Example situations include retrieving the bounds or scanlines of a Region.
    /// So use this to create a 'null' Graphics instance that effectively eats all
    /// rendering calls.
    /// </summary>
    public sealed class NullGraphics
        : IDisposable
    {
        private IntPtr hdc = IntPtr.Zero;
        private Graphics graphics = null;
        private bool disposed = false;

        public Graphics Graphics
        {
            get
            {
                return graphics;
            }
        }

        public NullGraphics()
        {
            this.hdc = SafeNativeMethods.CreateCompatibleDC(IntPtr.Zero);

            if (this.hdc == IntPtr.Zero)
            {
                NativeMethods.ThrowOnWin32Error("CreateCompatibleDC returned NULL");
            }

            this.graphics = Graphics.FromHdc(this.hdc);
        }

        ~NullGraphics()
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
            if (!disposed)
            {
                if (disposing)
                {
                    this.graphics.Dispose();
                    this.graphics = null;
                }

                SafeNativeMethods.DeleteDC(this.hdc);
                disposed = true;
            }
        }
    }
}
