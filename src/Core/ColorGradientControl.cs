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
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace PaintDotNet
{
    public sealed class ColorGradientControl 
        : UserControl
    {
        private Point lastTrackingMouseXY = new Point(-1, -1);

        private int tracking = -1;
        private int highlight = -1;

        private const int triangleSize = 7;
        private const int triangleHalfLength = (triangleSize - 1) / 2;

        private Orientation orientation = Orientation.Vertical;

        private Color[] customGradient = null;

        private bool drawNearNub = true;
        public bool DrawNearNub
        {
            get
            {
                return this.drawNearNub;
            }

            set
            {
                this.drawNearNub = value;
                Invalidate();
            }
        }

        private bool drawFarNub = true;
        public bool DrawFarNub
        {
            get
            {
                return this.drawFarNub;
            }

            set
            {
                this.drawFarNub = value;
                Invalidate();
            }
        }

        private int[] vals;

        // value from [0,255] that specifies the hsv "value" component
        // where we should draw little triangles that show the value
        public int Value 
        {
            get 
            {
                return GetValue(0);
            }

            set
            {
                SetValue(0, value);
            }
        }

        public Color[] CustomGradient
        {
            get
            {
                if (this.customGradient == null)
                {
                    return null;
                }
                else
                {
                    return (Color[])this.customGradient.Clone();
                }
            }

            set
            {
                if (value != this.customGradient)
                {
                    if (value == null)
                    {
                        this.customGradient = null;
                    }
                    else
                    {
                        this.customGradient = (Color[])value.Clone();
                    }

                    Invalidate();
                }
            }
        }

        public Orientation Orientation
        {
            get
            {
                return this.orientation;
            }

            set
            {
                if (value != this.orientation)
                {
                    this.orientation = value;
                    Invalidate();
                }
            }
        }

        public int Count
        {
            get 
            {
                return vals.Length;
            }

            set 
            {
                if (value < 0 || value > 16) 
                {
                    throw new ArgumentOutOfRangeException("value", value, "Count must be between 0 and 16");
                }

                vals = new int[value];

                if (value > 1) 
                {
                    for (int i = 0; i < value; i++) 
                    {
                        vals[i] = i * 255 / (value - 1);
                    }
                } 
                else if (value == 1) 
                {
                    vals[0] = 128;
                }

                OnValueChanged(0);
                Invalidate();
            }
        }

        public int GetValue(int index) 
        {
            if (index < 0 || index >= vals.Length) 
            {
                throw new ArgumentOutOfRangeException("index", index, "Index must be within the bounds of the array");
            }

            int val = vals[index];
            return val;
        }

        public void SetValue(int index, int val)
        {
            int min = -1;
            int max = 256;

            if (index < 0 || index >= vals.Length) 
            {
                throw new ArgumentOutOfRangeException("index", index, "Index must be within the bounds of the array");
            }

            if (index - 1 >= 0) 
            {
                min = vals[index - 1];
            }

            if (index + 1 < vals.Length) 
            {
                max = vals[index + 1];
            }

            if (vals[index] != val) 
            {
                int newVal = Utility.Clamp(val, min + 1, max - 1);
                vals[index] = newVal;
                OnValueChanged(index);
                Invalidate();
            }

            Update();
        }

        public event IndexEventHandler ValueChanged;
        private void OnValueChanged(int index)
        {
            if (ValueChanged != null)
            {
                ValueChanged(this, new IndexEventArgs(index));
            }
        }

        [Obsolete("Use MinColor property instead", true)]
        public Color BottomColor
        {
            get
            {
                return MinColor;
            }

            set
            {
                MinColor = value;
            }
        }

        [Obsolete("Use MaxColor property instead", true)]
        public Color TopColor
        {
            get
            {
                return MaxColor;
            }

            set
            {
                MaxColor = value;
            }
        }

        private Color maxColor;
        public Color MaxColor
        {
            get
            {
                return maxColor;
            }

            set
            {
                if (maxColor != value)
                {
                    maxColor = value;
                    Invalidate();
                }
            }
        }


        private Color minColor;
        public Color MinColor
        {
            get
            {
                return minColor;
            }
            
            set
            {
                if (minColor != value)
                {
                    minColor = value;
                    Invalidate();
                }
            }
        }

        public ColorGradientControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            this.DoubleBuffered = true;
            this.ResizeRedraw = true;
            this.Count = 1;
        }

        private void DrawGradient(Graphics g)
        {
            g.PixelOffsetMode = PixelOffsetMode.Half;
            Rectangle gradientRect;

            float gradientAngle;

            switch (this.orientation)
            {
                case Orientation.Horizontal:
                    gradientAngle = 180.0f;
                    break;

                case Orientation.Vertical:
                    gradientAngle = 90.0f;
                    break;

                default:
                    throw new InvalidEnumArgumentException();
            }

            // draw gradient
            gradientRect = ClientRectangle;

            switch (this.orientation)
            {
                case Orientation.Horizontal:
                    gradientRect.Inflate(-triangleHalfLength, -triangleSize + 3);
                    break;

                case Orientation.Vertical:
                    gradientRect.Inflate(-triangleSize + 3, -triangleHalfLength);
                    break;

                default:
                    throw new InvalidEnumArgumentException();
            }

            if (this.customGradient != null && gradientRect.Width > 1 && gradientRect.Height > 1)
            {
                Surface gradientSurface = new Surface(gradientRect.Size);

                using (RenderArgs ra = new RenderArgs(gradientSurface))
                {
                    Utility.DrawColorRectangle(ra.Graphics, ra.Bounds, Color.Transparent, false);

                    if (Orientation == Orientation.Horizontal)
                    {
                        for (int x = 0; x < gradientSurface.Width; ++x)
                        {
                            // TODO: refactor, double buffer, save this computation in a bitmap somewhere
                            double index = (double)(x * (this.customGradient.Length - 1)) / (double)(gradientSurface.Width - 1);
                            int indexL = (int)Math.Floor(index);
                            double t = 1.0 - (index - indexL);
                            int indexR = (int)Math.Min(this.customGradient.Length - 1, Math.Ceiling(index));
                            Color colorL = this.customGradient[indexL];
                            Color colorR = this.customGradient[indexR];

                            double a1 = colorL.A / 255.0;
                            double r1 = colorL.R / 255.0;
                            double g1 = colorL.G / 255.0;
                            double b1 = colorL.B / 255.0;

                            double a2 = colorR.A / 255.0;
                            double r2 = colorR.R / 255.0;
                            double g2 = colorR.G / 255.0;
                            double b2 = colorR.B / 255.0;

                            double at = (t * a1) + ((1.0 - t) * a2);

                            double rt;
                            double gt;
                            double bt;
                            if (at == 0)
                            {
                                rt = 0;
                                gt = 0;
                                bt = 0;
                            }
                            else
                            {
                                rt = ((t * a1 * r1) + ((1.0 - t) * a2 * r2)) / at;
                                gt = ((t * a1 * g1) + ((1.0 - t) * a2 * g2)) / at;
                                bt = ((t * a1 * b1) + ((1.0 - t) * a2 * b2)) / at;
                            }

                            int ap = Utility.Clamp((int)Math.Round(at * 255.0), 0, 255);
                            int rp = Utility.Clamp((int)Math.Round(rt * 255.0), 0, 255);
                            int gp = Utility.Clamp((int)Math.Round(gt * 255.0), 0, 255);
                            int bp = Utility.Clamp((int)Math.Round(bt * 255.0), 0, 255);

                            for (int y = 0; y < gradientSurface.Height; ++y)
                            {
                                ColorBgra src = gradientSurface[x, y];

                                // we are assuming that src.A = 255

                                int rd = ((rp * ap) + (src.R * (255 - ap))) / 255;
                                int gd = ((gp * ap) + (src.G * (255 - ap))) / 255;
                                int bd = ((bp * ap) + (src.B * (255 - ap))) / 255;

                                // TODO: proper alpha blending!
                                gradientSurface[x, y] = ColorBgra.FromBgra((byte)bd, (byte)gd, (byte)rd, 255);
                            }
                        }

                        g.DrawImage(ra.Bitmap, gradientRect, ra.Bounds, GraphicsUnit.Pixel);
                    }
                    else if (Orientation == Orientation.Vertical)
                    {
                        // TODO
                    }
                    else
                    {
                        throw new InvalidEnumArgumentException();
                    }
                }

                gradientSurface.Dispose();
            }
            else
            {
                using (LinearGradientBrush lgb = new LinearGradientBrush(this.ClientRectangle,
                           maxColor, minColor, gradientAngle, false))
                {
                    g.FillRectangle(lgb, gradientRect);
                }
            }

            // fill background
            using (PdnRegion nonGradientRegion = new PdnRegion())
            {
                nonGradientRegion.MakeInfinite();
                nonGradientRegion.Exclude(gradientRect);

                using (SolidBrush sb = new SolidBrush(this.BackColor))
                {
                    g.FillRegion(sb, nonGradientRegion.GetRegionReadOnly());
                }
            }

            // draw value triangles
            for (int i = 0; i < this.vals.Length; i++)
            {
                int pos = ValueToPosition(vals[i]);
                Brush brush;
                Pen pen;

                if (i == highlight) 
                {
                    brush = Brushes.Blue;
                    pen = (Pen)Pens.White.Clone();
                } 
                else 
                {
                    brush = Brushes.Black;
                    pen = (Pen)Pens.Gray.Clone();
                }

                g.SmoothingMode = SmoothingMode.AntiAlias;

                Point a1;
                Point b1;
                Point c1;

                Point a2;
                Point b2;
                Point c2;

                switch (this.orientation)
                {
                    case Orientation.Horizontal:
                        a1 = new Point(pos - triangleHalfLength, 0);
                        b1 = new Point(pos, triangleSize - 1);
                        c1 = new Point(pos + triangleHalfLength, 0);

                        a2 = new Point(a1.X, Height - 1 - a1.Y);
                        b2 = new Point(b1.X, Height - 1 - b1.Y);
                        c2 = new Point(c1.X, Height - 1 - c1.Y);
                        break;

                    case Orientation.Vertical:
                        a1 = new Point(0, pos - triangleHalfLength);
                        b1 = new Point(triangleSize - 1, pos);
                        c1 = new Point(0, pos + triangleHalfLength);

                        a2 = new Point(Width - 1 - a1.X, a1.Y);
                        b2 = new Point(Width - 1 - b1.X, b1.Y);
                        c2 = new Point(Width - 1 - c1.X, c1.Y);
                        break;

                    default:
                        throw new InvalidEnumArgumentException();
                }

                if (this.drawNearNub)
                {
                    g.FillPolygon(brush, new Point[] { a1, b1, c1, a1 });
                }

                if (this.drawFarNub)
                {
                    g.FillPolygon(brush, new Point[] { a2, b2, c2, a2 });
                }

                if (pen != null)
                {
                    if (this.drawNearNub)
                    {
                        g.DrawPolygon(pen, new Point[] { a1, b1, c1, a1 });
                    }

                    if (this.drawFarNub)
                    {
                        g.DrawPolygon(pen, new Point[] { a2, b2, c2, a2 });
                    }

                    pen.Dispose();
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint (e);
            DrawGradient(e.Graphics);
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            DrawGradient(pevent.Graphics);
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

            base.Dispose(disposing);
        }

        private int PositionToValue(int pos)
        {
            int max;

            switch (this.orientation)
            {
                case Orientation.Horizontal:
                    max = Width;
                    break;
                    
                case Orientation.Vertical:
                    max = Height;
                    break;

                default:
                    throw new InvalidEnumArgumentException();
            }

            int val = (((max - triangleSize) - (pos - triangleHalfLength)) * 255) / (max - triangleSize);

            if (this.orientation == Orientation.Horizontal)
            {
                val = 255 - val;
            }

            return val;
        }

        private int ValueToPosition(int val)
        {
            int max;

            if (this.orientation == Orientation.Horizontal)
            {
                val = 255 - val;
            }

            switch (this.orientation)
            {
                case Orientation.Horizontal:
                    max = Width;
                    break;

                case Orientation.Vertical:
                    max = Height;
                    break;

                default:
                    throw new InvalidEnumArgumentException();
            }

            int pos = triangleHalfLength + ((max - triangleSize) - (((val * (max - triangleSize)) / 255)));
            return pos;
        }

        private int WhichTriangle(int val) 
        {
            int bestIndex = -1;
            int bestDistance = int.MaxValue;
            int v = PositionToValue(val);

            for (int i = 0; i < this.vals.Length; i++) 
            {
                int distance = Math.Abs(this.vals[i] - v);

                if (distance < bestDistance) 
                {
                    bestDistance = distance;
                    bestIndex = i;
                }
            }

            return bestIndex;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left)
            {
                int val = GetOrientedValue(e);
                tracking = WhichTriangle(val);
                Invalidate();
                OnMouseMove(e);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.Button == MouseButtons.Left)
            {
                OnMouseMove(e);
                tracking = -1;
                Invalidate();
            }
        }

        private int GetOrientedValue(MouseEventArgs me)
        {
            return GetOrientedValue(new Point(me.X, me.Y));
        }

        private int GetOrientedValue(Point pt)
        {
            int pos;

            switch (this.orientation)
            {
                case Orientation.Horizontal:
                    pos = pt.X;
                    break;

                case Orientation.Vertical:
                    pos = pt.Y;
                    break;

                default:
                    throw new InvalidEnumArgumentException();
            }

            return pos;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            int pos = GetOrientedValue(e);

            Point newMouseXY = new Point(e.X, e.Y);

            if (tracking >= 0 && newMouseXY != this.lastTrackingMouseXY)
            {
                int val = PositionToValue(pos);
                this.SetValue(tracking, val);
                this.lastTrackingMouseXY = newMouseXY;
            }
            else
            {
                int oldHighlight = highlight;
                highlight = WhichTriangle(pos);

                if (highlight != oldHighlight)
                {
                    this.InvalidateTriangle(oldHighlight);
                    this.InvalidateTriangle(highlight);
                }
            }

        }

        protected override void OnMouseLeave(EventArgs e)
        {
            int oldhighlight = highlight;
            highlight = -1;
            this.InvalidateTriangle(oldhighlight);
        }

        private void InvalidateTriangle(int index) 
        {
            if (index < 0 || index >= this.vals.Length) 
            {
                return;
            }

            int value = ValueToPosition(this.vals[index]);
            Rectangle rect;

            switch (this.orientation)
            {
                case Orientation.Horizontal:
                    rect = new Rectangle(value - triangleHalfLength, 0, triangleSize, this.Height);
                    break;

                case Orientation.Vertical:
                    rect = new Rectangle(0, value - triangleHalfLength, this.Width, triangleSize);
                    break;

                default:
                    throw new InvalidEnumArgumentException();
            }

            this.Invalidate(rect, true);
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
