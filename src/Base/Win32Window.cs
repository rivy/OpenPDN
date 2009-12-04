/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Windows.Forms;

namespace PaintDotNet
{
    public sealed class Win32Window
        : IWin32Window
    {
        private IntPtr handle;
        private object container;

        public IntPtr Handle
        {
            get
            {
                return this.handle;
            }
        }

        public Win32Window(IntPtr handle, object container)
        {
            this.handle = handle;
            this.container = container;
        }
    }
}
