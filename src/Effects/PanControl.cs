/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PaintDotNet.Effects
{
    public class PanControl 
        : UserControl
    {
        private bool mouseDown = false;
        private PointF startPosition = new PointF(0, 0);
        private Point startMouse = new Point(0, 0);
        private Bitmap renderSurface = null; // used for double-buffering
        private Cursor handCursor;
        private Cursor handMouseDownCursor;

        public PanControl()
        {
            if (!this.DesignMode)
            {
                handCursor = new Cursor(PdnResources.GetResourceStream("Cursors.PanToolCursor.cur"));
                handMouseDownCursor = new Cursor(PdnResources.GetResourceStream("Cursors.PanToolCursorMouseDown.cur"));
                this.Cursor = handCursor;
            }

            InitializeComponent();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (handCursor != null)
                {
                    handCursor.Dispose();
                    handCursor = null;
                }

                if (handMouseDownCursor != null)
                {
                    handMouseDownCursor.Dispose();
                    handMouseDownCursor = null;
                }
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            // 
            // PanControl
            // 
            this.Name = "PanControl";
            this.Size = new System.Drawing.Size(184, 168);
        }

        private PointF position = new PointF(0, 0);

        [Browsable(false)]
        public PointF Position
        {
            get 
            {
                return position;
            }

            set
            {
                if (position != value)
                {
                    position = value;

                    this.Invalidate();
                    OnPositionChanged();
                    this.Update();
                }
            }
        }

        public event EventHandler PositionChanged;
        protected void OnPositionChanged()
        {
            if (PositionChanged != null)
            {
                PositionChanged(this, EventArgs.Empty);
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (mouseDown)
            {
                return;
            }

            if (e.Button == MouseButtons.Left)
            {
                mouseDown = true;
                startPosition = position;
                startMouse = new Point(e.X, e.Y);

                Cursor = handMouseDownCursor;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove (e);

            if (mouseDown && e.Button == MouseButtons.Left)
            {
                Position = new PointF(2.0f * e.X / this.Width - 1, 2.0f * e.Y / this.Height - 1);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp (e);

            if (mouseDown)
            {
                Cursor = handCursor;
                mouseDown = false;
            }
        }

        private void CheckRenderSurface()
        {
            if (renderSurface != null && renderSurface.Size != Size)
            {
                renderSurface.Dispose();
                renderSurface = null;
            }

            if (renderSurface == null)
            {
                renderSurface = new Bitmap(Width, Height);

                using (Graphics g = Graphics.FromImage(renderSurface))
                {
                    DrawToGraphics(g);
                }
            }
        }

        private void DoPaint(Graphics g)
        {
            CheckRenderSurface();
            g.DrawImage(renderSurface, ClientRectangle, ClientRectangle, GraphicsUnit.Pixel);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint (e);
            renderSurface = null;
            DoPaint(e.Graphics);
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            DoPaint(pevent.Graphics);
        }

        protected void DrawToGraphics(Graphics g)
        {
            PointF ptCenter = new PointF(Width / 2.0f, Height / 2.0f);
            PointF ptDot = new PointF((1 + position.X) * Width / 2.0f, (1 + position.Y) * Height / 2.0f);
            PointF ptArrow;

            if (-1 <= position.X && position.X <= 1 &&
                -1 <= position.Y && position.Y <= 1)
            {
                ptArrow = new PointF((1 + position.X) * Width / 2, (1 + position.Y) * Height / 2);
            }
            else
            {
                ptArrow = new PointF((1 + position.X) * Width / 2, (1 + position.Y) * Height / 2);

                if (Math.Abs(Position.X) > Math.Abs(Position.Y))
                {
                    if (position.X > 0)
                    {
                        ptArrow.X = this.Width - 1;
                        ptArrow.Y = (1 + position.Y / position.X) * Height / 2;
                    }
                    else
                    {
                        ptArrow.X = 0;
                        ptArrow.Y = (1 - position.Y / position.X) * Height / 2;
                    }
                }
                else
                {
                    if (position.Y > 0)
                    {
                        ptArrow.X = (1 + position.X / position.Y) * Width / 2;
                        ptArrow.Y = this.Height - 1;
                    }
                    else
                    {
                        ptArrow.X = (1 - position.X / position.Y) * Width / 2;
                        ptArrow.Y = 0;
                    }
                }
            }

            g.Clear(this.BackColor);
            SmoothingMode oldSM = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.HighQuality;

            using (Pen pen = (Pen)Pens.Black.Clone())
            {
                pen.SetLineCap(LineCap.Round, LineCap.DiamondAnchor, DashCap.Flat);
                pen.EndCap = LineCap.ArrowAnchor;
                pen.Width = 4.0f;
                pen.Color = SystemColors.ControlDark;

                g.DrawLine(pen, ptCenter, ptArrow);
            }

            using (Pen pen = (Pen)Pens.Black.Clone())
            { 
                pen.SetLineCap(LineCap.DiamondAnchor, LineCap.DiamondAnchor, DashCap.Flat);
                pen.Width = 3.0f;
                pen.Color = SystemColors.ControlText;

                g.DrawLine(pen, ptDot.X - 6, ptDot.Y, ptDot.X + 6, ptDot.Y);
                g.DrawLine(pen, ptDot.X, ptDot.Y - 6, ptDot.X, ptDot.Y + 6);
            }

            g.SmoothingMode = oldSM;
        }
    }
}
