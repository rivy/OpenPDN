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

namespace PaintDotNet
{
    /// <summary>
    /// Renders a preview of the brush
    /// </summary>
    internal class BrushPreviewRenderer
        : SurfaceBoxRenderer
    {
        private float brushSize;
        public float BrushSize
        {
            get
            {
                return this.brushSize;
            }

            set
            {
                RectangleF rect1 = GetInvalidateBrushRect();
                this.brushSize = value;
                RectangleF rect2 = GetInvalidateBrushRect();
                Invalidate(RectangleF.Union(rect1, rect2));
            }
        }

        private PointF brushLocation = new PointF(-500.0f, -500.0f);
        public PointF BrushLocation
        {
            get
            {
                return this.brushLocation;
            }

            set
            {
                RectangleF rect1 = GetInvalidateBrushRect();
                this.brushLocation = value;
                RectangleF rect2 = GetInvalidateBrushRect();
                Invalidate(RectangleF.Union(rect1, rect2));
            }
        }

        private int brushAlpha = 255;
        public int BrushAlpha
        {
            get
            {
                return this.brushAlpha;
            }

            set
            {
                this.brushAlpha = value;
                InvalidateBrushLocation();
            }
        }

        protected override void OnVisibleChanged()
        {
            InvalidateBrushLocation();
        }

        private RectangleF GetInvalidateBrushRect()
        {
            float ratio = (float)this.OwnerList.ScaleFactor.Ratio;
            PointF location = this.BrushLocation;
            RectangleF rectF = Utility.RectangleFromCenter(location, this.brushSize);
            rectF.Inflate(Math.Max(4.0f, 4.0f / ratio), Math.Max(4.0f, 4.0f / ratio));
            return rectF;
        }

        private void InvalidateBrushLocation()
        {
            RectangleF rectF = GetInvalidateBrushRect();
            Invalidate(rectF);
        }

        public override void Render(Surface dst, System.Drawing.Point offset)
        {
            using (RenderArgs ra = new RenderArgs(dst))
            {
                ra.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                PointF ptF = this.BrushLocation;

                if (this.BrushSize == 0.5f)
                {
                    ptF.X += 0.5f;
                    ptF.Y += 0.5f;
                }

                ptF.X *= (float)OwnerList.ScaleFactor.Ratio;
                ptF.Y *= (float)OwnerList.ScaleFactor.Ratio;

                ra.Graphics.TranslateTransform(-offset.X, -offset.Y, MatrixOrder.Append);
                RectangleF brushRect = Utility.RectangleFromCenter(ptF, this.BrushSize * (float)OwnerList.ScaleFactor.Ratio);

                using (Pen white = new Pen(Color.FromArgb(this.brushAlpha, Color.White), -1.0f), 
                           black = new Pen(Color.FromArgb(this.brushAlpha, Color.Black), -1.0f))
                {
                    brushRect.Inflate(-2, -2);
                    ra.Graphics.DrawEllipse(white, brushRect);
                    brushRect.Inflate(1, 1);
                    ra.Graphics.DrawEllipse(black, brushRect);
                    brushRect.Inflate(1, 1);
                    ra.Graphics.DrawEllipse(white, brushRect);
                }
            }
        }

        public BrushPreviewRenderer(SurfaceBoxRendererList ownerList)
            : base(ownerList)
        {
        }
    }
}
