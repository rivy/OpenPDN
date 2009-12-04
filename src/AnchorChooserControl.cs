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
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace PaintDotNet
{
    internal class AnchorChooserControl 
        : System.Windows.Forms.UserControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        private Image centerImage = null;
        private AnchorEdge[][] xyToAnchorEdge;
        private Hashtable anchorEdgeToXy; // maps AnchorEdge -> Point
        private AnchorEdge anchorEdge = AnchorEdge.TopLeft;

        public event EventHandler AnchorEdgeChanged;
        protected virtual void OnAnchorEdgeChanged()
        {
            if (AnchorEdgeChanged != null)
            {
                AnchorEdgeChanged(this, EventArgs.Empty);
            }
        }

        [DefaultValue(AnchorEdge.TopLeft)]
        public AnchorEdge AnchorEdge
        {
            get
            {
                return anchorEdge;
            }

            set
            {
                if (anchorEdge != value)
                {
                    anchorEdge = value;
                    OnAnchorEdgeChanged();
                    Invalidate();
                    Update();
                }
            }
        }
        
        public AnchorChooserControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            this.ResizeRedraw = true;

            this.centerImage = PdnResources.GetImageResource("Images.AnchorChooserControl.AnchorImage.png").Reference;
            this.xyToAnchorEdge = new AnchorEdge[][] {
                                                         new AnchorEdge[] { AnchorEdge.TopLeft, AnchorEdge.Top, AnchorEdge.TopRight },
                                                         new AnchorEdge[] { AnchorEdge.Left, AnchorEdge.Middle, AnchorEdge.Right },
                                                         new AnchorEdge[] { AnchorEdge.BottomLeft, AnchorEdge.Bottom, AnchorEdge.BottomRight }
                                                     };

            this.anchorEdgeToXy = new Hashtable();
            this.anchorEdgeToXy.Add(AnchorEdge.TopLeft, new Point(0, 0));
            this.anchorEdgeToXy.Add(AnchorEdge.Top, new Point(1, 0));
            this.anchorEdgeToXy.Add(AnchorEdge.TopRight, new Point(2, 0));
            this.anchorEdgeToXy.Add(AnchorEdge.Left, new Point(0, 1));
            this.anchorEdgeToXy.Add(AnchorEdge.Middle, new Point(1, 1));
            this.anchorEdgeToXy.Add(AnchorEdge.Right, new Point(2, 1));
            this.anchorEdgeToXy.Add(AnchorEdge.BottomLeft, new Point(0, 2));
            this.anchorEdgeToXy.Add(AnchorEdge.Bottom, new Point(1, 2));
            this.anchorEdgeToXy.Add(AnchorEdge.BottomRight, new Point(2, 2));

            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | 
                ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
        }
        #endregion

        private MouseButtons mouseButtonDown;
        private bool mouseDown = false;
        private Point mouseDownPoint;
        private bool drawHotPush = false;
        private Point hotAnchorButton = new Point(-1, -1);

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (!this.mouseDown)
            {
                this.mouseDown = true;
                this.mouseButtonDown = e.Button;
                this.mouseDownPoint = new Point(e.X, e.Y);

                int anchorX = (e.X * 3) / this.Width;
                int anchorY = (e.Y * 3) / this.Height;

                this.hotAnchorButton = new Point(anchorX, anchorY);
                this.drawHotPush = true;
                Invalidate();
            }

            base.OnMouseDown (e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (this.mouseDown && e.Button == this.mouseButtonDown)
            {
                int anchorX = (e.X * 3) / this.Width;
                int anchorY = (e.Y * 3) / this.Height;

                this.drawHotPush = (anchorX == this.hotAnchorButton.X && anchorY == this.hotAnchorButton.Y);
            }

            Invalidate();
            base.OnMouseMove (e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (this.mouseDown && e.Button == this.mouseButtonDown)
            {
                int anchorX = (e.X * 3) / this.Width;
                int anchorY = (e.Y * 3) / this.Height;

                if (anchorX == this.hotAnchorButton.X && anchorY == this.hotAnchorButton.Y &&
                    anchorX >= 0 && anchorX <= 2 &&
                    anchorY >= 0 && anchorY <= 2)
                {
                    AnchorEdge newEdge = (AnchorEdge)this.xyToAnchorEdge[anchorY][anchorX];
                    this.AnchorEdge = newEdge;
                    Invalidate();
                }
            }

            this.drawHotPush = false;
            this.mouseDown = false;
            base.OnMouseUp (e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Clear background
            e.Graphics.Clear(SystemColors.Control);

            // Draw each part
            Point selection = (Point)this.anchorEdgeToXy[this.anchorEdge];

            double controlCenterX = (double)this.Width / 2.0;
            double controlCenterY = (double)this.Height / 2.0;

            Pen linePen = new Pen(SystemColors.WindowText, (((float)Width + (float)Height) / 2.0f) / 64.0f);
            AdjustableArrowCap cap = new AdjustableArrowCap((float)Width / 32.0f, (float)Height / 32.0f, true);
            linePen.CustomEndCap = cap;

            Point mousePoint = PointToClient(Control.MousePosition);
            int mouseAnchorX = (int)Math.Floor(((float)mousePoint.X * 3.0f) / (float)this.Width);
            int mouseAnchorY = (int)Math.Floor(((float)mousePoint.Y * 3.0f) / (float)this.Height);

            for (int y = 0; y < 3; ++y)
            {
                for (int x = 0; x < 3; ++x)
                {
                    AnchorEdge edge = this.xyToAnchorEdge[y][x];
                    Point offset = (Point)this.anchorEdgeToXy[edge];
                    Point vector = new Point(offset.X - selection.X, offset.Y - selection.Y);

                    int left = (this.Width * x) / 3;
                    int top = (this.Height * y) / 3;
                    int right = Math.Min(this.Width - 1, (this.Width * (x + 1)) / 3);
                    int bottom = Math.Min(this.Height - 1, (this.Height * (y + 1)) / 3);
                    int width = right - left;
                    int height = bottom - top;

                    if (vector.X == 0 && vector.Y == 0)
                    {
                        ButtonRenderer.DrawButton(e.Graphics, new Rectangle(left, top, width, height), PushButtonState.Pressed);
                        e.Graphics.DrawImage(this.centerImage, left + 3, top + 3, width - 6, height - 6);
                    }
                    else 
                    {
                        PushButtonState state;

                        if (drawHotPush && x == this.hotAnchorButton.X && y == this.hotAnchorButton.Y)
                        {
                            state = PushButtonState.Pressed;
                        }
                        else
                        {
                            state = PushButtonState.Normal;

                            if (!mouseDown && mouseAnchorX == x && mouseAnchorY == y)
                            {
                                state = PushButtonState.Hot;
                            }
                        }

                        ButtonRenderer.DrawButton(e.Graphics, new Rectangle(left, top, width, height), state);

                        if (vector.X <= 1 && vector.X >= -1 && vector.Y <= 1 && vector.Y >= -1)
                        {
                            double vectorMag = Math.Sqrt((double)((vector.X * vector.X) + (vector.Y * vector.Y)));
                            double normalX = (double)vector.X / vectorMag;
                            double normalY = (double)vector.Y / vectorMag;

                            Point center = new Point((left + right) / 2, (top + bottom) / 2);

                            Point start = new Point(center.X - (width / 4) * vector.X, center.Y - (height / 4) * vector.Y);
                            Point end = new Point(
                                start.X + (int)(((double)width / 2.0) * normalX),
                                start.Y + (int)(((double)height / 2.0) * normalY));

                            PixelOffsetMode oldPOM = e.Graphics.PixelOffsetMode;
                            e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
                            e.Graphics.DrawLine(linePen, start, end);
                            e.Graphics.PixelOffsetMode = oldPOM;
                        }
                    }
                }
            }

            linePen.Dispose();
            base.OnPaint(e);
        }
    }
}
