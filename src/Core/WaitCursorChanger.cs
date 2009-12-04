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
    /// <summary>
    /// Simply sets a control's Cursor to the WaitCursor (hourglass) on creation,
    /// and sets it back to its original setting upon disposal.
    /// </summary>
    public sealed class WaitCursorChanger
        : IDisposable
    {
        private Control control;
        private Cursor oldCursor;
        private static int nextID = 0;
        private int id = System.Threading.Interlocked.Increment(ref nextID);

        public WaitCursorChanger(Control control)
        {
            this.control = control;
            this.oldCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
        }

        ~WaitCursorChanger()
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
                if (this.oldCursor != null)
                {
                    Cursor.Current = this.oldCursor;
                    this.oldCursor = null;
                }
            }
        }
    }
}
