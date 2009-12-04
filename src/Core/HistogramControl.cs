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

namespace PaintDotNet
{
    public sealed class HistogramControl 
        : UserControl
    {
        private Histogram histogram;
        public Histogram Histogram
        {
            get
            {
                return histogram;
            }
            
            set
            {
                if (histogram != value)
                {
                    if (histogram != null)
                    {
                        histogram.HistogramChanged -= histogramChangedDelegate;
                    }
                    histogram = value;
                    histogram.HistogramChanged += histogramChangedDelegate;

                    int channels = histogram.Channels;

                    if (selected == null || channels != selected.GetLength(0))
                    {
                        selected = new bool[channels];
                    }

                    Invalidate();
                }
            }
        }

        public int Channels
        {
            get
            {
                return histogram.Channels;
            }
        }

        public int Entries
        {
            get
            {
                return histogram.Entries;
            }
        }

        private bool[] selected;

        public void SetSelected(int channel, bool val)
        {
            selected[channel] = val;
            Invalidate();
        }

        public bool GetSelected(int channel)
        {
            return selected[channel];
        }

        private bool flipHorizontal;
        public bool FlipHorizontal
        {
            get 
            {
                return flipHorizontal;
            }
            set 
            {
                flipHorizontal = value;
            }
        }

        private bool flipVertical;
        public bool FlipVertical
        {
            get 
            {
                return flipVertical;
            }
            set 
            {
                flipVertical = value;
            }
        }

        public EventHandler histogramChangedDelegate;
        public HistogramControl()
        {
            histogramChangedDelegate = new EventHandler(Histogram_HistogramChanged);

            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose (disposing);
        }

        private const int tickSize = 4;

        private void RenderChannel(Graphics g, ColorBgra color, int channel, long max, float mean)
        {
            Rectangle innerRect = ClientRectangle;

            int l = innerRect.Left;
            int t = innerRect.Top;
            int b = innerRect.Bottom;
            int r = innerRect.Right;
            int channels = histogram.Channels;
            int entries = histogram.Entries;
            long[] hist = Histogram.HistogramValues[channel];

            ++max;

            if (flipHorizontal)
            {
                Utility.Swap(ref l, ref r);
            }

            if (!flipVertical)
            {
                Utility.Swap(ref t, ref b);
            }

            PointF[] points = new PointF[entries + 2];

            points[entries] = new PointF(Utility.Lerp(l, r, -1), Utility.Lerp(t, b, 20));
            points[entries + 1] = new PointF(Utility.Lerp(l, r, -1), Utility.Lerp(b, t, 20));

            for (int i = 0; i < entries; i += entries - 1)
            {
                points[i] = new PointF(
                    Utility.Lerp(l, r, (float)hist[i] / (float)max),
                    Utility.Lerp(t, b, (float)i / (float)entries));
            }

            long sum3 = hist[0] + hist[1];
            
            for (int i = 1; i < entries - 1; ++i)
            {
                sum3 += hist[i + 1];

                points[i] = new PointF(
                    Utility.Lerp(l, r, (float)(sum3) / (float)(max * 3.1f)),
                    Utility.Lerp(t, b, (float)i / (float)entries));

                sum3 -= hist[i - 1];
            }

            byte intensity = selected[channel] ? (byte)96 : (byte)32;
            ColorBgra colorPen = ColorBgra.Blend(ColorBgra.Black, color, intensity);
            ColorBgra colorBrush = color;

            colorBrush.A = intensity;

            Pen pen = new Pen(colorPen.ToColor(), 1.3f);
            SolidBrush brush = new SolidBrush(colorBrush.ToColor());
            
            g.FillPolygon(brush, points, FillMode.Alternate);
            g.DrawPolygon(pen, points);
        }

        private void RenderHistogram(Graphics g)
        {
            long max = histogram.GetMax();
            float[] mean = histogram.GetMean();

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(BackColor);
            int channels = histogram.Channels;

            for (int i = 0; i < channels; ++i)
            {
                RenderChannel(g, histogram.GetVisualColor(i), i, max, mean[i]);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint (e);

            RenderHistogram(e.Graphics);
        }

        private void Histogram_HistogramChanged(object sender, EventArgs e)
        {
            Invalidate();
        }
    }
}
