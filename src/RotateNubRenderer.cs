/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.SystemLayer;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace PaintDotNet
{
    internal class RotateNubRenderer
        : SurfaceBoxGraphicsRenderer
    {
        private const int size = 6;
        private PointF location;
        private float angle;

        public PointF Location
        {
            get
            {
                return this.location;
            }

            set
            {
                InvalidateOurself();
                this.location = value;
                InvalidateOurself();
            }
        }

        public float Angle
        {
            get
            {
                return this.angle;
            }

            set
            {
                InvalidateOurself();
                this.angle = value;
                InvalidateOurself();
            }
        }

        private RectangleF GetOurRectangle()
        {
            RectangleF rectF = new RectangleF(this.Location, new SizeF(0, 0));
            float ratio = 1.0f / (float)OwnerList.ScaleFactor.Ratio;
            float ourSize = UI.ScaleWidth(size);
            rectF.Inflate(ratio * ourSize, ratio * ourSize);
            return rectF;
        }

        private void InvalidateOurself()
        {
            RectangleF rectF = GetOurRectangle();
            Rectangle rect = Utility.RoundRectangle(rectF);
            rect.Inflate(2, 2);
            Invalidate(rect);
        }

        public bool IsPointTouching(Point pt)
        {
            RectangleF rectF = GetOurRectangle();
            Rectangle rect = Utility.RoundRectangle(rectF);
            return pt.X >= rect.Left && pt.Y >= rect.Top && pt.X < rect.Right && pt.Y < rect.Bottom;
        }

        protected override void OnVisibleChanged()
        {
            InvalidateOurself();
        }

        public override void RenderToGraphics(Graphics g, Point offset)
        {
            // We round these values to the nearest integer to avoid an interesting rendering
            // anomaly (or bug? what a surprise ... GDI+) where the nub appears to rotate
            // off-center, or the 'screw-line' is off-center
            float centerX = this.Location.X * (float)OwnerList.ScaleFactor.Ratio;
            float centerY = this.Location.Y * (float)OwnerList.ScaleFactor.Ratio;
            Point center = new Point((int)Math.Round(centerX), (int)Math.Round(centerY));

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TranslateTransform(-center.X, -center.Y, MatrixOrder.Append);
            g.RotateTransform(this.angle, MatrixOrder.Append);
            g.TranslateTransform(center.X - offset.X, center.Y - offset.Y, MatrixOrder.Append);

            float ourSize = UI.ScaleWidth(size);

            using (Pen white = new Pen(Color.FromArgb(128, Color.White), -1.0f), 
                       black = new Pen(Color.FromArgb(128, Color.Black), -1.0f))
            {
                RectangleF rectF = new RectangleF(center, new SizeF(0, 0));
                rectF.Inflate(ourSize - 3, ourSize - 3);

                g.DrawEllipse(white, Rectangle.Truncate(rectF));
                rectF.Inflate(1, 1);
                g.DrawEllipse(black, Rectangle.Truncate(rectF));
                rectF.Inflate(1, 1);
                g.DrawEllipse(white, Rectangle.Truncate(rectF));

                rectF.Inflate(-2, -2);
                g.DrawLine(white, rectF.X + rectF.Width / 2.0f - 1.0f, rectF.Top, rectF.X + rectF.Width / 2.0f - 1.0f, rectF.Bottom);
                g.DrawLine(white, rectF.X + rectF.Width / 2.0f + 1.0f, rectF.Top, rectF.X + rectF.Width / 2.0f + 1.0f, rectF.Bottom);
                g.DrawLine(black, rectF.X + rectF.Width / 2.0f, rectF.Top, rectF.X + rectF.Width / 2.0f, rectF.Bottom);
            }
        }
            
        public RotateNubRenderer(SurfaceBoxRendererList ownerList)
            : base(ownerList)
        {
            this.location = new Point(0, 0);
        }
    }
}
