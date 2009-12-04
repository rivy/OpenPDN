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
    /// This class will set the cursor of a control to the requested one,
    /// and then when this class is Disposed it will reset the cursor
    /// to the original cursor.
    /// </summary>
    public sealed class CursorChanger
        : IDisposable
    {
        private Control control;
        private Cursor oldCursor;

        private Control FindTopParent(Control childControl)
        {
            Control parent = childControl.Parent;

            if (parent == null)
            {
                return childControl;
            }
            else
            {
                return FindTopParent(parent);
            }
        }

        public CursorChanger(Control control, Cursor newCursor)
        {
            this.control = control;
            this.oldCursor = this.control.Cursor;
            FindTopParent(control).Cursor = newCursor;
        }

        ~CursorChanger()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool disposed = false;
        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    FindTopParent(control).Cursor = oldCursor;
                }

                disposed = true;
            }
        }
    }
}
