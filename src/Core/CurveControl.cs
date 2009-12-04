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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// This class is for manipulation of transfer functions.
    /// It is intended for curve adjustment
    /// </summary>
    public abstract class CurveControl 
        : UserControl
    {
        private System.ComponentModel.Container components = null;
        private int[] curvesInvalidRange = new int[] { int.MaxValue, int.MinValue };
        private Point lastMouseXY = new Point(int.MinValue, int.MinValue);
        private int lastKey = -1;
        private int lastValue = -1;
        private bool tracking = false;
        private Point[] ptSave;
        private int[] pointsNearMousePerChannel;
        private bool[] effectChannel;

        public abstract ColorTransferMode ColorTransferMode
        {
            get;
        }


        protected SortedList<int, int>[] controlPoints;
        public SortedList<int, int>[] ControlPoints
        {
            get
            {
                return this.controlPoints;
            }

            set
            {
                if (value.Length != controlPoints.Length)
                {
                    throw new ArgumentException("value must have a matching channel count", "value");
                }

                this.controlPoints = value;
                Invalidate();
            }
        }

        protected int channels;
        public int Channels
        {
            get
            {
                return this.channels;
            }
        }

        protected int entries;
        public int Entries
        {
            get
            {
                return entries;
            }
        }

        protected ColorBgra[] visualColors;
        public ColorBgra GetVisualColor(int channel)
        {
            return visualColors[channel];
        }

        protected string[] channelNames;
        public string GetChannelName(int channel)
        {
            return channelNames[channel];
        }

        protected bool[] mask;
        public void SetSelected(int channel, bool val)
        {
            mask[channel] = val;
            Invalidate();
        }

        public bool GetSelected(int channel)
        {
            return mask[channel];
        }

        protected internal CurveControl(int channels, int entries)
        {
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

            this.channels = channels;
            this.entries = entries;

            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            pointsNearMousePerChannel = new int[channels];
            for (int i = 0; i < channels; ++i)
            {
                pointsNearMousePerChannel[i] = -1;
            }
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
                    components = null;
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
            this.TabStop = false;
        }

        #endregion

        public event EventHandler ValueChanged;
        protected virtual void OnValueChanged()
        {
            if (ValueChanged != null)
            {
                ValueChanged(this, EventArgs.Empty);
            }
        }

        public event EventHandler<EventArgs<Point>> CoordinatesChanged;
        protected virtual void OnCoordinatesChanged()
        {
            if (CoordinatesChanged != null)
            {
                CoordinatesChanged(this, new EventArgs<Point>(new Point(lastKey, lastValue)));
            }
        }

        public void ResetControlPoints()
        {
            controlPoints = new SortedList<int, int>[Channels];

            for (int i = 0; i < Channels; ++i)
            {
                SortedList<int, int> newList = new SortedList<int, int>();

                newList.Add(0, 0);
                newList.Add(Entries - 1, Entries - 1);
                controlPoints[i] = newList;
            }

            Invalidate();
            OnValueChanged();
        }

        private void DrawToGraphics(Graphics g)
        {
            ColorBgra colorSolid = ColorBgra.FromColor(this.ForeColor);
            ColorBgra colorGuide = ColorBgra.FromColor(this.ForeColor);
            ColorBgra colorGrid = ColorBgra.FromColor(this.ForeColor);

            colorGrid.A = 128;
            colorGuide.A = 96;

            Pen penSolid = new Pen(colorSolid.ToColor(), 1);
            Pen penGrid = new Pen(colorGrid.ToColor(), 1);
            Pen penGuide = new Pen(colorGuide.ToColor(), 1);

            penGrid.DashStyle = DashStyle.Dash;

            g.Clear(this.BackColor);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle ourRect = ClientRectangle;

            ourRect.Inflate(-1, -1);

            if (lastMouseXY.Y >= 0)
            {
                g.DrawLine(penGuide, 0, lastMouseXY.Y, Width, lastMouseXY.Y);
            }

            if (lastMouseXY.X >= 0)
            {
                g.DrawLine(penGuide, lastMouseXY.X, 0, lastMouseXY.X, Height);
            }

            for (float f = 0.25f; f <= 0.75f; f += 0.25f)
            {
                float x = Utility.Lerp(ourRect.Left, ourRect.Right, f);
                float y = Utility.Lerp(ourRect.Top, ourRect.Bottom, f);

                g.DrawLine(penGrid,
                    Point.Round(new PointF(x, ourRect.Top)),
                    Point.Round(new PointF(x, ourRect.Bottom)));

                g.DrawLine(penGrid,
                    Point.Round(new PointF(ourRect.Left, y)),
                    Point.Round(new PointF(ourRect.Right, y)));
            }

            g.DrawLine(penGrid, ourRect.Left, ourRect.Bottom, ourRect.Right, ourRect.Top);

            float width = this.ClientRectangle.Width;
            float height = this.ClientRectangle.Height;

            for (int c = 0; c < channels; ++c)
            {
                SortedList<int, int> channelControlPoints = controlPoints[c];
                int points = channelControlPoints.Count;

                ColorBgra color = GetVisualColor(c);
                ColorBgra colorSelected = ColorBgra.Blend(color, ColorBgra.White, 128);

                const float penWidthNonSelected = 1;
                const float penWidthSelected = 2;
                float penWidth = mask[c] ? penWidthSelected : penWidthNonSelected;
                Pen penSelected = new Pen(color.ToColor(), penWidth);

                color.A = 128;

                Pen pen = new Pen(color.ToColor(), penWidth);
                Brush brush = new SolidBrush(color.ToColor());
                SolidBrush brushSelected = new SolidBrush(Color.White);

                SplineInterpolator interpolator = new SplineInterpolator();
                IList<int> xa = channelControlPoints.Keys;
                IList<int> ya = channelControlPoints.Values;
                PointF[] line = new PointF[Entries];

                for (int i = 0; i < points; ++i)
                {
                    interpolator.Add(xa[i], ya[i]);
                }
                
                for (int i = 0; i < line.Length; ++i)
                {
                    line[i].X = (float)i * (width - 1) / (entries - 1);
                    line[i].Y = (float)(Utility.Clamp(entries - 1 - interpolator.Interpolate(i), 0, entries - 1)) * 
                        (height - 1) / (entries - 1);
                }

                pen.LineJoin = LineJoin.Round;
                g.DrawLines(pen, line);

                for (int i = 0; i < points; ++i)
                {
                    int k = channelControlPoints.Keys[i];
                    float x = k * (width - 1) / (entries - 1);
                    float y = (entries - 1 - channelControlPoints.Values[i]) * (height - 1) / (entries - 1);

                    const float radiusSelected = 4;
                    const float radiusNotSelected = 3;
                    const float radiusUnMasked = 2;

                    bool selected = (mask[c] && pointsNearMousePerChannel[c] == i);
                    float size = selected ? radiusSelected : (mask[c] ? radiusNotSelected : radiusUnMasked);
                    RectangleF rect = Utility.RectangleFromCenter(new PointF(x, y), size);

                    g.FillEllipse(selected ? brushSelected : brush, rect.X, rect.Y, rect.Width, rect.Height);
                    g.DrawEllipse(selected ? penSelected : pen, rect.X, rect.Y, rect.Width, rect.Height);
                }

                pen.Dispose();
            }

            penSolid.Dispose();
            penGrid.Dispose();
            penGuide.Dispose();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            DrawToGraphics(e.Graphics);
            base.OnPaint(e);
        }

        /* This is not used now, but may be used later
        /// <summary>
        /// Reduces the number of control points by at least given factor.
        /// </summary>
        /// <param name="factor"></param>
        public void Simplify(float factor)
        {
            for (int c = 0; c < channels; ++c)
            {
                SortedList<int, int> channelControlPoints = controlPoints[c];
                int targetPoints = (int)Math.Ceiling(channelControlPoints.Count / factor);

                float minPointWorth = float.MaxValue;

                //remove points until the target point count is reached, but always remove unnecessary
                while (channelControlPoints.Count > 2)
                {
                    minPointWorth = float.MaxValue;
                    int minPointWorthIndex = -1;

                    for (int i = 1; i < channelControlPoints.Count - 1; ++i)
                    {
                        Point left = new Point(
                            channelControlPoints.Keys[i - 1],
                            channelControlPoints.Values[i - 1]);
                        Point right = new Point(
                            channelControlPoints.Keys[i + 1],
                            channelControlPoints.Values[i + 1]);
                        Point actual = new Point(
                            channelControlPoints.Keys[i],
                            channelControlPoints.Values[i]);

                        float targetY = left.Y + (actual.X - left.X) * (right.Y - left.Y) / (float)(right.X - left.X);
                        float error = targetY - actual.Y;
                        float pointWorth = error * error * (right.X - left.X);

                        if (pointWorth < minPointWorth)
                        {
                            minPointWorth = pointWorth;
                            minPointWorthIndex = i;
                        }
                    }


                    if (channelControlPoints.Count > targetPoints || minPointWorth == 0)
                    {
                        //if we found a point and it's not the first point
                        if (minPointWorthIndex > 0)
                        {
                            channelControlPoints.RemoveAt(minPointWorthIndex);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }

            Invalidate();
            OnValueChanged();
        }
        */

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            float width = this.ClientRectangle.Width;
            float height = this.ClientRectangle.Height;
            int mx = (int)Utility.Clamp(0.5f + e.X * (entries - 1) / (width - 1), 0, Entries - 1);
            int my = (int)Utility.Clamp(0.5f + Entries - 1 - e.Y * (entries - 1) / (height - 1), 0, Entries - 1);

            ptSave = new Point[channels];
            for (int i = 0; i < channels; ++i)
            {
                ptSave[i].X = -1;
            }

            if (0 != e.Button)
            {
                tracking = (e.Button == MouseButtons.Left);
                lastKey = mx;

                bool anyNearMouse = false;

                effectChannel = new bool[channels];
                for (int c = 0; c < channels; ++c)
                {
                    SortedList<int, int> channelControlPoints = controlPoints[c];
                    int index = pointsNearMousePerChannel[c];
                    bool hasPoint = (index >= 0);
                    int key = hasPoint ? channelControlPoints.Keys[index] : index;

                    anyNearMouse = (anyNearMouse || hasPoint);

                    effectChannel[c] = hasPoint;

                    if (mask[c] && hasPoint && 
                        key > 0 && key < entries - 1)
                    {
                        channelControlPoints.RemoveAt(index);
                        OnValueChanged();
                    }
                }

                if (!anyNearMouse)
                {
                    for (int c = 0; c < channels; ++c)
                    {
                        effectChannel[c] = true;
                    }
                }
            }

            OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (0 != (e.Button & MouseButtons.Left) && tracking)
            {
                tracking = false;
                lastKey = -1;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            lastMouseXY = new Point(e.X, e.Y);
            float width = this.ClientRectangle.Width;
            float height = this.ClientRectangle.Height;
            int mx = (int)Utility.Clamp(0.5f + e.X * (entries - 1) / (width - 1), 0, Entries - 1);
            int my = (int)Utility.Clamp(0.5f + Entries - 1 - e.Y * (entries - 1) / (height - 1), 0, Entries - 1);

            Invalidate();

            if (tracking && e.Button == MouseButtons.None)
            {
                tracking = false;
            }

            if (tracking)
            {
                bool changed = false;
                for (int c = 0; c < channels; ++c)
                {
                    SortedList<int, int> channelControlPoints = controlPoints[c];

                    pointsNearMousePerChannel[c] = -1;
                    if (mask[c] && effectChannel[c])
                    {
                        int lastIndex = channelControlPoints.IndexOfKey(lastKey);

                        if (ptSave[c].X >= 0 && ptSave[c].X != mx)
                        {
                            channelControlPoints[ptSave[c].X] = ptSave[c].Y;
                            ptSave[c].X = -1;

                            changed = true;
                        }
                        else if (lastKey > 0 && lastKey < Entries - 1 && lastIndex >= 0 && mx != lastKey)
                        {
                            channelControlPoints.RemoveAt(lastIndex);
                        }

                        if (mx >= 0 && mx < Entries)
                        {
                            int newValue = Utility.Clamp(my, 0, Entries - 1);
                            int oldIndex = channelControlPoints.IndexOfKey(mx);
                            int oldValue = (oldIndex >= 0) ? channelControlPoints.Values[oldIndex] : -1;

                            if (oldIndex >= 0 && mx != lastKey) 
                            {
                                // if we drag onto an existing point, delete it, but save it in case we drag away
                                ptSave[c].X = mx;
                                ptSave[c].Y = channelControlPoints.Values[oldIndex];
                            }

                            if (oldIndex < 0 ||
                                channelControlPoints[mx] != newValue)
                            {
                                channelControlPoints[mx] = newValue;
                                changed = true;
                            }

                            pointsNearMousePerChannel[c] = channelControlPoints.IndexOfKey(mx);
                        }
                    }
                }

                if (changed)
                {
                    Update();
                    OnValueChanged();
                }
            }
            else
            {
                pointsNearMousePerChannel = new int[channels];

                for (int c = 0; c < channels; ++c)
                {
                    SortedList<int, int> channelControlPoints = controlPoints[c];
                    int minRadiusSq = 30;
                    int bestIndex = -1;

                    if (mask[c])
                    {
                        for (int i = 0; i < channelControlPoints.Count; ++i)
                        {
                            int sumsq = 0;
                            int diff = 0;

                            diff = channelControlPoints.Keys[i] - mx;
                            sumsq += diff * diff;

                            diff = channelControlPoints.Values[i] - my;
                            sumsq += diff * diff;

                            if (sumsq < minRadiusSq)
                            {
                                minRadiusSq = sumsq;
                                bestIndex = i;
                            }
                        }
                    }

                    pointsNearMousePerChannel[c] = bestIndex;
                }

                Update();
            }

            lastKey = mx;
            lastValue = my;
            OnCoordinatesChanged();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            lastKey = -1;
            lastValue = -1;
            lastMouseXY = new Point(int.MinValue, int.MinValue);
            Invalidate();
            OnCoordinatesChanged();
            base.OnMouseLeave(e);
        }

        public virtual void InitFromPixelOp(UnaryPixelOp op)
        {
            OnValueChanged();
            Invalidate();
        }
    }
}
