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

namespace PaintDotNet
{
    /// <summary>
    /// Gets around a limitation in System.Windows.Forms.PaintEventArgs in that it disposes
    /// the Graphics instance that is associated with it when it is disposed.
    /// </summary>
    public sealed class PaintEventArgs2
        : EventArgs
    {
        private Graphics graphics;
        public Graphics Graphics
        {
            get
            {
                return graphics;
            }
        }

        private Rectangle clipRectangle;
        public Rectangle ClipRectangle
        {
            get
            {
                return clipRectangle;
            }
        }

        public PaintEventArgs2(Graphics graphics, Rectangle clipRectangle)
        {
            this.graphics = graphics;
            this.clipRectangle = clipRectangle;
        }
    }
}
