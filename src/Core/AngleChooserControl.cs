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
    public class AngleChooserControl 
        : UserControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;
        private bool tracking = false;
        private bool hover = false;
        private Point lastMouseXY;

        public event EventHandler ValueChanged;
        protected virtual void OnValueChanged()
        {
            if (ValueChanged != null)
            {
                ValueChanged(this, EventArgs.Empty);
            }
        }

        public double angleValue;
        public int Value
        {
            get
            {
                return (int)angleValue;
            }

            set
            {
                double v = value % 360;
                if (angleValue != v)
                {
                    angleValue = v;
                    OnValueChanged();
                    Invalidate();
                }
            }
        }
        /// <summary>
        /// ValueDouble exposes the double-precision angle
        /// </summary>
        public double ValueDouble
        {
            get
            {
                return angleValue;
            }

            set
            {
                double v = Math.IEEERemainder(value, 360.0);
                if (angleValue != v)
                {
                    angleValue = v;
                    OnValueChanged();
                    Invalidate();
                }
            }
        }

        private void DrawToGraphics(Graphics g)
        {
            g.Clear(this.BackColor);

            SmoothingMode oldSM = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            RectangleF ourRect = RectangleF.Inflate(ClientRectangle, -1, -1);
            double diameter = Math.Min(ourRect.Width, ourRect.Height);

            double radius = (diameter / 2.0);

            PointF center = new PointF(
                (float)(ourRect.X + radius), 
                (float)(ourRect.Y + radius));

            double theta = (this.angleValue * 2.0 * Math.PI) / 360.0;

            RectangleF ellipseRect = new RectangleF(ourRect.Location, new SizeF((float)diameter, (float)diameter));
            g.FillEllipse(SystemBrushes.ControlLightLight, RectangleF.Inflate(ellipseRect, -2, -2));

            RectangleF ellipseOutlineRect = this.hover ? RectangleF.Inflate(ellipseRect, -1.0f, -1.0f) : ellipseRect;

            using (Pen ellipseOutlinePen = new Pen(SystemColors.ControlDark))
            {
                ellipseOutlinePen.Width = this.hover ? 2.0f : 1.0f;
                g.DrawEllipse(ellipseOutlinePen, ellipseOutlineRect);
            }

            double endPointRadius = radius - 2;
            PointF endPoint = new PointF(
                (float)(center.X + (endPointRadius * Math.Cos(theta))),
                (float)(center.Y - (endPointRadius * Math.Sin(theta))));

            float gripSize = 2.5f;
            RectangleF gripEllipseRect = new RectangleF(center.X - gripSize, center.Y - gripSize, gripSize * 2, gripSize * 2);
            g.FillEllipse(SystemBrushes.ControlDark, gripEllipseRect);

            using (Pen anglePen = (Pen)SystemPens.ControlDark.Clone())
            {
                anglePen.Width = 2.0f;
                anglePen.Alignment = PenAlignment.Center;
                g.DrawLine(anglePen, center, endPoint);
            }

            g.SmoothingMode = oldSM;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            DrawToGraphics(e.Graphics);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            this.hover = true;
            Invalidate(true);
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            this.hover = false;
            Invalidate(true);
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            tracking = true;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            tracking = false;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove (e);

            lastMouseXY = new Point(e.X, e.Y);

            if (tracking)
            {
                Rectangle ourRect = Rectangle.Inflate(ClientRectangle, -2, -2);
                int diameter = Math.Min(ourRect.Width, ourRect.Height);
                Point center = new Point(ourRect.X + (diameter / 2), ourRect.Y + (diameter / 2));

                int dx = e.X - center.X;
                int dy = e.Y - center.Y;
                double theta = Math.Atan2(-dy, dx);

                double newAngle = (theta * 360) / (2 * Math.PI);

                if ((ModifierKeys & Keys.Shift) != 0)
                {
                    const double constraintAngle = 15.0;

                    double multiple = newAngle / constraintAngle;
                    double top = Math.Floor(multiple);
                    double topDelta = Math.Abs(top - multiple);
                    double bottom = Math.Ceiling(multiple);
                    double bottomDelta = Math.Abs(bottom - multiple);

                    double bestMultiple;
                    if (bottomDelta < topDelta)
                    {
                        bestMultiple = bottom;
                    }
                    else
                    {
                        bestMultiple = top;
                    }

                    newAngle = bestMultiple * constraintAngle;
                }

                this.ValueDouble = newAngle;

                Update();
            }
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick (e);
            tracking = true;
            OnMouseMove(new MouseEventArgs(MouseButtons.Left, 1, lastMouseXY.X, lastMouseXY.Y, 0));
            tracking = false;
        }

        protected override void OnDoubleClick(EventArgs e)
        {
            base.OnDoubleClick (e);
            tracking = true;
            OnMouseMove(new MouseEventArgs(MouseButtons.Left, 1, lastMouseXY.X, lastMouseXY.Y, 0));
            tracking = false;
        }

        public AngleChooserControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            SetStyle(ControlStyles.Selectable, false);
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw, true);

            DoubleBuffered = true;

            TabStop = false;
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
            // 
            // AngleChooserControl
            // 
            this.Name = "AngleChooserControl";
            this.Size = new System.Drawing.Size(168, 144);

        }
        #endregion
    }
}
