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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace PaintDotNet
{
    internal class MoveNubRenderer
        : CanvasControl
    {
        private Matrix transform;
        private float transformAngle;
        private int alpha;
        private MoveNubShape shape;

        public MoveNubShape Shape
        {
            get
            {
                return this.shape;
            }

            set
            {
                InvalidateOurself();
                this.shape = value;
                InvalidateOurself();
            }
        }

        protected override void OnLocationChanging()
        {
            InvalidateOurself();
            base.OnLocationChanging();
        }

        protected override void OnLocationChanged()
        {
            InvalidateOurself();
            base.OnLocationChanged();
        }

        protected override void OnSizeChanging()
        {
            InvalidateOurself();
            base.OnSizeChanging();
        }

        protected override void OnSizeChanged()
        {
            InvalidateOurself();
            base.OnSizeChanged();
        }

        public Matrix Transform
        {
            get
            {
                return this.transform.Clone();
            }

            set
            {
                InvalidateOurself();

                if (value == null)
                {
                    throw new ArgumentNullException();
                }

                if (this.transform != null)
                {
                    this.transform.Dispose();
                    this.transform = null;
                }

                this.transform = value.Clone();
                this.transformAngle = Utility.GetAngleOfTransform(this.transform);
                InvalidateOurself();
            }
        }

        public int Alpha
        {
            get
            {
                return this.alpha;
            }

            set
            {
                if (value < 0 || value > 255)
                {
                    throw new ArgumentOutOfRangeException("value", value, "value must be [0, 255]");
                }

                if (this.alpha != value)
                {
                    this.alpha = value;
                    InvalidateOurself();
                }
            }
        }

        private RectangleF GetOurRectangle()
        {
            PointF[] ptFs = new PointF[1] { this.Location };
            this.transform.TransformPoints(ptFs);
            float ratio = (float)Math.Ceiling(1.0 / OwnerList.ScaleFactor.Ratio);

            float ourWidth = UI.ScaleWidth(this.Size.Width);
            float ourHeight = UI.ScaleHeight(this.Size.Height);

            if (!Single.IsNaN(ratio))
            {
                RectangleF rectF = new RectangleF(ptFs[0], new SizeF(0, 0));
                rectF.Inflate(ratio * ourWidth, ratio * ourHeight);
                return rectF;
            }
            else
            {
                return RectangleF.Empty;
            }
        }

        private void InvalidateOurself()
        {
            InvalidateOurself(false);
        }

        private void InvalidateOurself(bool force)
        {
            if (this.Visible || force)
            {
                RectangleF rectF = GetOurRectangle();
                Rectangle rect = Utility.RoundRectangle(rectF);
                rect.Inflate(1, 1);
                Invalidate(rect);
            }
        }

        public bool IsPointTouching(PointF ptF, bool pad)
        {
            RectangleF rectF = GetOurRectangle();

            if (pad)
            {
                float padding = 2.0f * 1.0f / (float)this.OwnerList.ScaleFactor.Ratio;
                rectF.Inflate(padding + 1.0f, padding + 1.0f);
            }

            return rectF.Contains(ptF);
        }

        public bool IsPointTouching(Point pt, bool pad)
        {
            RectangleF rectF = GetOurRectangle();

            if (pad)
            {
                float padding = 2.0f * 1.0f / (float)this.OwnerList.ScaleFactor.Ratio;
                rectF.Inflate(padding + 1.0f, padding + 1.0f);
            }

            return pt.X >= rectF.Left && pt.Y >= rectF.Top && pt.X < rectF.Right && pt.Y < rectF.Bottom;
        }

        protected override void OnVisibleChanged()
        {
            InvalidateOurself(true);
        }

        protected override void OnRender(Graphics g, Point offset)
        {
            lock (this)
            {
                float ourSize = UI.ScaleWidth(Math.Min(Width, Height));
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TranslateTransform(-offset.X, -offset.Y, MatrixOrder.Append);

                PointF ptF = (PointF)this.Location;

                ptF = Utility.TransformOnePoint(this.transform, ptF);

                ptF.X *= (float)OwnerList.ScaleFactor.Ratio;
                ptF.Y *= (float)OwnerList.ScaleFactor.Ratio;

                PointF[] pts = new PointF[8] 
                                         { 
                                             new PointF(-1, -1), // up+left
                                             new PointF(+1, -1), // up+right
                                             new PointF(+1, +1), // down+right
                                             new PointF(-1, +1), // down+left

                                             new PointF(-1, 0),  // left
                                             new PointF(+1, 0),  // right
                                             new PointF(0, -1),  // up
                                             new PointF(0, +1)   // down
                                         };

                Utility.RotateVectors(pts, this.transformAngle);
                Utility.NormalizeVectors(pts);

                using (Pen white = new Pen(Color.FromArgb(this.alpha, Color.White), -1.0f),
                           black = new Pen(Color.FromArgb(this.alpha, Color.Black), -1.0f))
                {
                    PixelOffsetMode oldPOM = g.PixelOffsetMode;
                    g.PixelOffsetMode = PixelOffsetMode.None;

                    if (this.shape != MoveNubShape.Circle)
                    {
                        PointF[] outer = new PointF[4]
                        {
                            Utility.AddVectors(ptF, Utility.MultiplyVector(pts[0], ourSize)),
                            Utility.AddVectors(ptF, Utility.MultiplyVector(pts[1], ourSize)),
                            Utility.AddVectors(ptF, Utility.MultiplyVector(pts[2], ourSize)),
                            Utility.AddVectors(ptF, Utility.MultiplyVector(pts[3], ourSize))
                        };

                        PointF[] middle = new PointF[4]
                        {
                            Utility.AddVectors(ptF, Utility.MultiplyVector(pts[0], ourSize - 1)),
                            Utility.AddVectors(ptF, Utility.MultiplyVector(pts[1], ourSize - 1)),
                            Utility.AddVectors(ptF, Utility.MultiplyVector(pts[2], ourSize - 1)),
                            Utility.AddVectors(ptF, Utility.MultiplyVector(pts[3], ourSize - 1))
                        };

                        PointF[] inner = new PointF[4] 
                        {
                            Utility.AddVectors(ptF, Utility.MultiplyVector(pts[0], ourSize - 2)),
                            Utility.AddVectors(ptF, Utility.MultiplyVector(pts[1], ourSize - 2)),
                            Utility.AddVectors(ptF, Utility.MultiplyVector(pts[2], ourSize - 2)),
                            Utility.AddVectors(ptF, Utility.MultiplyVector(pts[3], ourSize - 2))
                        };

                        g.DrawPolygon(white, outer);
                        g.DrawPolygon(black, middle);
                        g.DrawPolygon(white, inner);
                    }
                    else if (this.shape == MoveNubShape.Circle)
                    {
                        RectangleF rect = new RectangleF(ptF, new SizeF(0, 0));
                        rect.Inflate(ourSize - 1, ourSize - 1);
                        g.DrawEllipse(white, rect);
                        rect.Inflate(-1.0f, -1.0f);
                        g.DrawEllipse(black, rect);
                        rect.Inflate(-1.0f, -1.0f);
                        g.DrawEllipse(white, rect);
                    }

                    if (this.shape == MoveNubShape.Compass)
                    {
                        black.SetLineCap(LineCap.Round, LineCap.DiamondAnchor, DashCap.Flat);
                        black.EndCap = LineCap.ArrowAnchor;
                        black.StartCap = LineCap.ArrowAnchor;
                        white.SetLineCap(LineCap.Round, LineCap.DiamondAnchor, DashCap.Flat);
                        white.EndCap = LineCap.ArrowAnchor;
                        white.StartCap = LineCap.ArrowAnchor;

                        PointF ul = Utility.AddVectors(ptF, Utility.MultiplyVector(pts[0], ourSize - 1));
                        PointF ur = Utility.AddVectors(ptF, Utility.MultiplyVector(pts[1], ourSize - 1));
                        PointF lr = Utility.AddVectors(ptF, Utility.MultiplyVector(pts[2], ourSize - 1));
                        PointF ll = Utility.AddVectors(ptF, Utility.MultiplyVector(pts[3], ourSize - 1));

                        PointF top = Utility.MultiplyVector(Utility.AddVectors(ul, ur), 0.5f);
                        PointF left = Utility.MultiplyVector(Utility.AddVectors(ul, ll), 0.5f);
                        PointF right = Utility.MultiplyVector(Utility.AddVectors(ur, lr), 0.5f);
                        PointF bottom = Utility.MultiplyVector(Utility.AddVectors(ll, lr), 0.5f);

                        using (SolidBrush whiteBrush = new SolidBrush(white.Color))
                        {
                            PointF[] poly = new PointF[] { ul, ur, lr, ll };
                            g.FillPolygon(whiteBrush, poly, FillMode.Winding);
                        }

                        g.DrawLine(black, top, bottom);
                        g.DrawLine(black, left, right);
                    }

                    g.PixelOffsetMode = oldPOM;
                }
            }
        }
            
        public MoveNubRenderer(SurfaceBoxRendererList ownerList)
            : base(ownerList)
        {
            this.shape = MoveNubShape.Square;
            this.transform = new Matrix();
            this.transform.Reset();
            this.alpha = 255;
            Size = new SizeF(5, 5);
        }
    }
}
