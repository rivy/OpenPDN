/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace PaintDotNet
{
    internal class IconBox : 
        System.Windows.Forms.UserControl
    {
        private Bitmap renderSurface = null;
        private Bitmap icon = null;

        public Bitmap Icon
        {
            get
            {
                return icon;
            }

            set
            {
                if (value == null)
                {
                    value = new Bitmap(1, 1);

                    using (Graphics g = Graphics.FromImage(value))
                    {
                        g.Clear(Color.Transparent);
                    }
                }

                icon = value;

                if (renderSurface != null)
                {
                    renderSurface.Dispose();
                }

                renderSurface = null;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(this.BackColor);
            Rectangle srcBounds = new Rectangle(new Point(0, 0), this.icon.Size);
            Rectangle dstBounds = new Rectangle(new Point(0, 0), this.ClientSize);
            e.Graphics.DrawImage(this.Icon, dstBounds, srcBounds, GraphicsUnit.Pixel);

            base.OnPaint(e);
        }

        public IconBox()
        {
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);

            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            this.ResizeRedraw = true;
        }

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
        }
        #endregion
    }
}
