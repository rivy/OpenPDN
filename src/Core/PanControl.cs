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
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace PaintDotNet.Core
{
    internal sealed class PanControl
        : UserControl
    {
        private bool mouseDown = false;
        private PointF startPosition = new PointF(0, 0);
        private Point startMouse = new Point(0, 0);
        private Bitmap renderSurface = null; // used for double-buffering
        private Cursor handCursor;
        private Cursor handMouseDownCursor;
        private ImageResource staticImageUnderlay;
        private Bitmap cachedUnderlay;
        private Rectangle dragAreaRect;

        public ImageResource StaticImageUnderlay
        {
            get
            {
                return this.staticImageUnderlay;
            }

            set
            {
                this.staticImageUnderlay = value;

                if (this.cachedUnderlay != null)
                {
                    this.cachedUnderlay.Dispose();
                    this.cachedUnderlay = null;
                }

                RefreshDragAreaRect();
                Invalidate(true);
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            if (this.cachedUnderlay != null)
            {
                this.cachedUnderlay.Dispose();
                this.cachedUnderlay = null;
            } 
            
            RefreshDragAreaRect();

            base.OnSizeChanged(e);
        }

        private void RefreshDragAreaRect()
        {
            if (this.staticImageUnderlay == null)
            {
                this.dragAreaRect = ClientRectangle;
            }
            else
            {
                Image image = this.staticImageUnderlay.Reference;
                Rectangle srcRect = new Rectangle(0, 0, image.Width, image.Height);

                Size maxThumbSize = new Size(
                    Math.Min(ClientSize.Width - 4, srcRect.Width),
                    Math.Min(ClientSize.Height - 4, srcRect.Height));

                Size dstSize = Utility.ComputeThumbnailSize(image.Size, Math.Min(maxThumbSize.Width, maxThumbSize.Height));
                Rectangle dstRect = new Rectangle((ClientSize.Width - dstSize.Width) / 2, (ClientSize.Height - dstSize.Height) / 2, dstSize.Width, dstSize.Height);

                this.dragAreaRect = new Rectangle(ClientRectangle.Left + dstRect.Left, ClientRectangle.Top + dstRect.Top, dstRect.Width, dstRect.Height);
            }
        }

        public PanControl()
        {
            if (!this.DesignMode)
            {
                handCursor = new Cursor(PdnResources.GetResourceStream("Cursors.PanToolCursor.cur"));
                handMouseDownCursor = new Cursor(PdnResources.GetResourceStream("Cursors.PanToolCursorMouseDown.cur"));
                this.Cursor = handCursor;
            }

            InitializeComponent();

            RefreshDragAreaRect();
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
            this.TabStop = false;
        }

        private PointF position = new PointF(0, 0);

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
        private void OnPositionChanged()
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

            if (!Enabled)
            {
                return;
            }

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

        private PointF MousePtToPosition(Point clientMousePt)
        {
            float centerX = ClientRectangle.Left + ((ClientRectangle.Right - ClientRectangle.Left) / 2.0f);
            float centerY = ClientRectangle.Top + ((ClientRectangle.Bottom - ClientRectangle.Top) / 2.0f);

            float deltaX = clientMousePt.X - centerX;
            float deltaY = clientMousePt.Y - centerY;

            float posX = deltaX / (this.dragAreaRect.Width / 2.0f);
            float posY = deltaY / (this.dragAreaRect.Height / 2.0f);

            return new PointF(posX, posY);
        }

        private PointF PositionToClientPt(PointF pos)
        {
            float centerX = ClientRectangle.Left + ((ClientRectangle.Right - ClientRectangle.Left) / 2.0f);
            float centerY = ClientRectangle.Top + ((ClientRectangle.Bottom - ClientRectangle.Top) / 2.0f);

            float halfWidth = this.dragAreaRect.Width / 2.0f;
            float halfHeight = this.dragAreaRect.Height / 2.0f;

            float ptX = centerX + pos.X * halfWidth;
            float ptY = centerY + pos.Y * halfHeight;

            return new PointF(ptX, ptY);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (mouseDown && e.Button == MouseButtons.Left)
            {
                Position = MousePtToPosition(new Point(e.X, e.Y));
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (mouseDown)
            {
                if (e.Button == MouseButtons.Left)
                {
                    Position = MousePtToPosition(new Point(e.X, e.Y));
                }

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
            base.OnPaint(e);
            renderSurface = null;
            DoPaint(e.Graphics);
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            DoPaint(pevent.Graphics);
        }

        private void DrawToGraphics(Graphics g)
        {
            PointF clientPos = new PointF(
                (this.position.X * this.dragAreaRect.Width) / ClientSize.Width,
                (this.position.Y * this.dragAreaRect.Height) / ClientSize.Height);

            PointF ptCenter = new PointF(ClientSize.Width / 2.0f, ClientSize.Height / 2.0f);
            PointF ptDot = new PointF((1 + clientPos.X) * ClientSize.Width / 2.0f, (1 + clientPos.Y) * ClientSize.Height / 2.0f);
            PointF ptArrow;

            if (-1 <= clientPos.X && clientPos.X <= 1 &&
                -1 <= clientPos.Y && clientPos.Y <= 1)
            {
                ptArrow = new PointF((1 + clientPos.X) * ClientSize.Width / 2, (1 + clientPos.Y) * ClientSize.Height / 2);
            }
            else
            {
                ptArrow = new PointF((1 + clientPos.X) * ClientSize.Width / 2, (1 + clientPos.Y) * ClientSize.Height / 2);

                if (Math.Abs(clientPos.X) > Math.Abs(clientPos.Y))
                {
                    if (clientPos.X > 0)
                    {
                        ptArrow.X = ClientSize.Width - 1;
                        ptArrow.Y = (1 + clientPos.Y / clientPos.X) * ClientSize.Height / 2;
                    }
                    else
                    {
                        ptArrow.X = 0;
                        ptArrow.Y = (1 - clientPos.Y / clientPos.X) * ClientSize.Height / 2;
                    }
                }
                else
                {
                    if (clientPos.Y > 0)
                    {
                        ptArrow.X = (1 + clientPos.X / clientPos.Y) * ClientSize.Width / 2;
                        ptArrow.Y = ClientSize.Height - 1;
                    }
                    else
                    {
                        ptArrow.X = (1 - clientPos.X / clientPos.Y) * ClientSize.Width / 2;
                        ptArrow.Y = 0;
                    }
                }
            }

            CompositingMode oldCM = g.CompositingMode;

            g.CompositingMode = CompositingMode.SourceCopy;
            g.Clear(this.BackColor);
            g.CompositingMode = CompositingMode.SourceOver;

            if (this.staticImageUnderlay != null)
            {
                Size dstSize;

                if (this.cachedUnderlay != null)
                {
                    dstSize = new Size(this.cachedUnderlay.Width, this.cachedUnderlay.Height);
                }
                else
                {
                    Image image = this.staticImageUnderlay.Reference;
                    Rectangle srcRect = new Rectangle(0, 0, image.Width, image.Height);

                    Size maxThumbSize = new Size(
                        Math.Max(1, Math.Min(ClientSize.Width - 4, srcRect.Width)),
                        Math.Max(1, Math.Min(ClientSize.Height - 4, srcRect.Height)));

                    dstSize = Utility.ComputeThumbnailSize(image.Size, Math.Min(maxThumbSize.Width, maxThumbSize.Height));

                    this.cachedUnderlay = new Bitmap(dstSize.Width, dstSize.Height, PixelFormat.Format24bppRgb);

                    Surface checkers = new Surface(dstSize);
                    checkers.ClearWithCheckboardPattern();
                    Bitmap checkersBmp = checkers.CreateAliasedBitmap();

                    Rectangle gcuRect = new Rectangle(0, 0, this.cachedUnderlay.Width, this.cachedUnderlay.Height);

                    using (Graphics gcu = Graphics.FromImage(this.cachedUnderlay))
                    {
                        gcu.CompositingMode = CompositingMode.SourceOver;
                        gcu.DrawImage(checkersBmp, gcuRect, new Rectangle(0, 0, checkersBmp.Width, checkersBmp.Height), GraphicsUnit.Pixel);

                        gcu.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        RectangleF gcuRect2 = RectangleF.Inflate(gcuRect, 0.5f, 0.5f);
                        gcu.DrawImage(image, gcuRect2, srcRect, GraphicsUnit.Pixel);
                    }

                    checkersBmp.Dispose();
                    checkersBmp = null;

                    checkers.Dispose();
                    checkers = null;
                }

                Rectangle dstRect = new Rectangle((ClientSize.Width - dstSize.Width) / 2, (ClientSize.Height - dstSize.Height) / 2, dstSize.Width, dstSize.Height);
                g.DrawImage(this.cachedUnderlay, dstRect, new Rectangle(0, 0, this.cachedUnderlay.Width, this.cachedUnderlay.Height), GraphicsUnit.Pixel);
                g.DrawRectangle(Pens.Black, new Rectangle(dstRect.Left - 1, dstRect.Top - 1, dstRect.Width + 1, dstRect.Height + 1));
                Utility.DrawDropShadow1px(g, new Rectangle(dstRect.Left - 2, dstRect.Top - 2, dstRect.Width + 4, dstRect.Height + 4));
            }

            PixelOffsetMode oldPOM = g.PixelOffsetMode;
            g.PixelOffsetMode = PixelOffsetMode.Half;

            SmoothingMode oldSM = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.HighQuality;

            // Draw the center -> end point arrow
            using (Pen pen = (Pen)Pens.Black.Clone())
            {
                pen.SetLineCap(LineCap.Round, LineCap.DiamondAnchor, DashCap.Flat);
                pen.EndCap = LineCap.ArrowAnchor;
                pen.Width = 2.0f;
                pen.Color = SystemColors.ControlDark;

                g.DrawLine(pen, ptCenter, ptArrow);
            }

            // Draw the compass
            using (Pen pen = new Pen(Color.White))
            {
                pen.SetLineCap(LineCap.DiamondAnchor, LineCap.DiamondAnchor, DashCap.Flat);

                // Draw white outline
                pen.Width = 3f;
                pen.Color = Color.White;

                g.DrawLine(pen, ptDot.X - 5.0f, ptDot.Y, ptDot.X + 5.0f, ptDot.Y);
                g.DrawLine(pen, ptDot.X, ptDot.Y - 5.0f, ptDot.X, ptDot.Y + 5.0f);

                // Draw black inset
                pen.Width = 2f;
                pen.Color = Color.Black;

                g.DrawLine(pen, ptDot.X - 5.0f, ptDot.Y, ptDot.X + 5.0f, ptDot.Y);
                g.DrawLine(pen, ptDot.X, ptDot.Y - 5.0f, ptDot.X, ptDot.Y + 5.0f);
            }

            g.SmoothingMode = oldSM;
            g.PixelOffsetMode = oldPOM;
            g.CompositingMode = oldCM;
        }
    }
}