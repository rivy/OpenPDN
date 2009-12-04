/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) Rick Brewster, Tom Jackson, and past contributors.            //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.PropertySystem;
using PaintDotNet.SystemLayer;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace PaintDotNet.IndirectUI
{
    // ColorBgra is essentially a 32-bit integer. This control lets you treat an Int32 as an RGB
    // color -- alpha is not supported in this version for simplicity's sake. Thus, you must
    // use an Int32Property that has a range of [0, 16777215].
    [PropertyControlInfo(typeof(Int32Property), PropertyControlType.ColorWheel)]
    internal sealed class Int32ColorWheelPropertyControl
        : PropertyControl<int, ScalarProperty<int>>
    {
        private const int requiredMin = 0;
        private const int requiredMax = 0xffffff;

        private HeaderLabel header;
        private ColorWheel hsvColorWheel;
        private ColorGradientControl valueSlider;
        private ColorGradientControl saturationSlider;
        private ColorRectangleControl colorRectangle;
        private Label redLabel;
        private PdnNumericUpDown redNud;
        private Label greenLabel;
        private PdnNumericUpDown greenNud;
        private Label blueLabel;
        private PdnNumericUpDown blueNud;
        private Button resetButton;
        private Label description;

        [PropertyControlProperty(DefaultValue = (object)true)]
        public bool ShowResetButton
        {
            get
            {
                return this.resetButton.Visible;
            }

            set
            {
                this.resetButton.Visible = value;
                this.resetButton.AutoSize = value;
                PerformLayout();
            }
        }

        internal sealed class ColorWheel 
            : UserControl
        {
            private Bitmap renderBitmap = null;
            private bool tracking = false;
            private Point lastMouseXY = new Point(-1, -1);

            // this number controls what you might call the tesselation of the color wheel. higher #'s = slower, lower #'s = looks worse
            private const int colorTesselation = 60;

            private PictureBox wheelPictureBox; 

            private HsvColor hsvColor;
            public HsvColor HsvColor
            {
                get
                {
                    return hsvColor;
                }

                set
                {
                    if (hsvColor != value)
                    {
                        HsvColor oldColor = hsvColor;
                        hsvColor = value;
                        this.OnColorChanged();
                        Refresh();
                    }
                }
            }
                    
            public ColorWheel()
            {
                // This call is required by the Windows.Forms Form Designer.
                InitializeComponent();

                //wheelRegion = new PdnRegion();
                hsvColor = new HsvColor(0, 0, 0);
            }

            private static PointF SphericalToCartesian(float r, float theta)
            {
                float x;
                float y;

                x = r * (float)Math.Cos(theta);
                y = r * (float)Math.Sin(theta);

                return new PointF(x,y);
            }

            private static PointF[] GetCirclePoints(float r, PointF center)
            {
                PointF[] points = new PointF[colorTesselation];
                
                for (int i = 0; i < colorTesselation; i++)
                {
                    float theta = ((float)i / (float)colorTesselation) * 2 * (float)Math.PI;
                    points[i] = SphericalToCartesian(r, theta);
                    points[i].X += center.X;
                    points[i].Y += center.Y;
                }
                
                return points;
            }

            private Color[] GetColors()
            {
                Color[] colors = new Color[colorTesselation];

                for (int i = 0; i < colorTesselation; i++)
                {
                    int hue = (i * 360) / colorTesselation;
                    colors[i] = new HsvColor(hue, 100, 100).ToColor();
                }

                return colors;
            }

            protected override void OnLoad(EventArgs e)
            {
                InitRendering();
                base.OnLoad(e);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                InitRendering();
                base.OnPaint(e);
            }

            private void InitRendering()
            {
                if (this.renderBitmap == null)
                {
                    InitRenderSurface();
                    this.wheelPictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
                    int size = (int)Math.Ceiling(ComputeDiameter(this.Size));
                    this.wheelPictureBox.Size = new Size(size, size);
                    this.wheelPictureBox.Image = this.renderBitmap;
                }
            }

            private void WheelPictureBox_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
            {
                float radius = ComputeRadius(Size);
                float theta = ((float)HsvColor.Hue / 360.0f) * 2.0f * (float)Math.PI;
                float alpha = ((float)HsvColor.Saturation / 100.0f);
                float x = (alpha * (radius - 1) * (float)Math.Cos(theta)) + radius;
                float y = (alpha * (radius - 1) * (float)Math.Sin(theta)) + radius;
                int ix = (int)x;
                int iy = (int)y;

                // Draw the 'target rectangle'
                GraphicsContainer container = e.Graphics.BeginContainer();
                e.Graphics.PixelOffsetMode = PixelOffsetMode.None;
                e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
                e.Graphics.DrawRectangle(Pens.Black, ix - 1, iy - 1, 3, 3);
                e.Graphics.DrawRectangle(Pens.White, ix, iy, 1, 1);
                e.Graphics.EndContainer(container);
            }

            private void InitRenderSurface()
            {
                if (renderBitmap != null)
                {
                    renderBitmap.Dispose();
                }

                int wheelDiameter = (int)ComputeDiameter(Size);

                renderBitmap = new Bitmap(Math.Max(1, (wheelDiameter * 4) / 3), 
                                          Math.Max(1, (wheelDiameter * 4) / 3), PixelFormat.Format24bppRgb);

                using (Graphics g1 = Graphics.FromImage(renderBitmap))
                {
                    g1.Clear(this.BackColor);
                    DrawWheel(g1, renderBitmap.Width, renderBitmap.Height);
                }
            }

            private void DrawWheel(Graphics g, int width, int height)
            {
                float radius = ComputeRadius(new Size(width, height));
                PointF[] points = GetCirclePoints(Math.Max(1.0f, (float)radius - 1), new PointF(radius, radius));
                
                using (PathGradientBrush pgb = new PathGradientBrush(points))
                {
                    pgb.CenterColor = new HsvColor(0, 0, 100).ToColor();
                    pgb.CenterPoint = new PointF(radius, radius);
                    pgb.SurroundColors = GetColors();

                    g.FillEllipse(pgb, 0, 0, radius * 2, radius * 2);
                }
            }

            private static float ComputeRadius(Size size)
            {
                return Math.Min((float)size.Width / 2, (float)size.Height / 2);
            }

            private static float ComputeDiameter(Size size)
            {
                return Math.Min((float)size.Width, (float)size.Height);       
            }

            protected override void OnResize(EventArgs e)
            {
                base.OnResize (e);

                if (renderBitmap != null && (ComputeRadius(Size) != ComputeRadius(renderBitmap.Size)))
                {
                    renderBitmap.Dispose();
                    renderBitmap = null;
                }

                Invalidate();
            }

            public event EventHandler ColorChanged;
            private void OnColorChanged()
            {
                if (ColorChanged != null)
                {
                    ColorChanged(this, EventArgs.Empty);
                }
            }

            private void GrabColor(Point mouseXY)
            {
                // center our coordinate system so the middle is (0,0), and positive Y is facing up
                int cx = mouseXY.X - (Width / 2);
                int cy = mouseXY.Y - (Height / 2);

                double theta = Math.Atan2(cy, cx);

                if (theta < 0)
                {
                    theta += 2 * Math.PI;
                }

                double alpha = Math.Sqrt((cx * cx) + (cy * cy));

                int h = (int)((theta / (Math.PI * 2)) * 360.0);
                int s = (int)Math.Min(100.0, (alpha / (double)(Width / 2)) * 100);
                int v = 100;

                hsvColor = new HsvColor(h, s, v);
                OnColorChanged();
                Invalidate(true);
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left)
                {
                    tracking = true;
                }

                base.OnMouseDown(e);
            }

            protected override void OnMouseUp(MouseEventArgs e)
            {
                if (tracking)
                {
                    GrabColor(new Point(e.X, e.Y));
                }

                tracking = false;

                base.OnMouseUp(e);
            }

            protected override void OnMouseMove(MouseEventArgs e)
            {
                Point thisMouseXY = new Point(e.X, e.Y);

                if (tracking && thisMouseXY != this.lastMouseXY)
                {
                    GrabColor(new Point(e.X, e.Y));
                }

                this.lastMouseXY = new Point(e.X, e.Y);

                base.OnMouseMove(e);
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

            #region Component Designer generated code
            /// <summary> 
            /// Required method for Designer support - do not modify 
            /// the contents of this method with the code editor.
            /// </summary>
            private void InitializeComponent()
            {
                this.wheelPictureBox = new System.Windows.Forms.PictureBox();
                this.SuspendLayout();
                // 
                // wheelPictureBox
                // 
                this.wheelPictureBox.Location = new System.Drawing.Point(0, 0);
                this.wheelPictureBox.Name = "wheelPictureBox";
                this.wheelPictureBox.TabIndex = 0;
                this.wheelPictureBox.TabStop = false;
                this.wheelPictureBox.Click += new System.EventHandler(this.wheelPictureBox_Click);
                this.wheelPictureBox.Paint += new System.Windows.Forms.PaintEventHandler(this.WheelPictureBox_Paint);
                this.wheelPictureBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.wheelPictureBox_MouseUp);
                this.wheelPictureBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.wheelPictureBox_MouseMove);
                this.wheelPictureBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.wheelPictureBox_MouseDown);
                // 
                // ColorWheel
                // 
                this.Controls.Add(this.wheelPictureBox);
                this.Name = "ColorWheel";
                this.ResumeLayout(false);

            }
            #endregion

            private void wheelPictureBox_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
            {
                OnMouseMove(e);
            }

            private void wheelPictureBox_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
            {
                OnMouseUp(e);
            }

            private void wheelPictureBox_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
            {
                OnMouseDown(e);
            }

            private void wheelPictureBox_Click(object sender, System.EventArgs e)
            {
                OnClick(e);
            }
        }

       internal class ColorRectangleControl 
            : UserControl
        {
            private Color rectangleColor;
            public Color RectangleColor
            {
                get
                {
                    return rectangleColor;
                }

                set
                {
                    rectangleColor = value;
                    Invalidate(true);
                }
            }

            public ColorRectangleControl()
            {
                this.ResizeRedraw = true;
                this.DoubleBuffered = true;
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                Utility.DrawColorRectangle(e.Graphics, this.ClientRectangle, rectangleColor, true);
                base.OnPaint(e);
            }
        }

        public Int32ColorWheelPropertyControl(PropertyControlInfo propInfo)
            : base(propInfo)
        {
            if (Property.MinValue != requiredMin || Property.MaxValue != requiredMax)
            {
                throw new ArgumentException("The only range allowed for this control is [" + requiredMin + ", " + requiredMax + "]");
            }

            SuspendLayout();

            this.header = new HeaderLabel();
            this.header.Name = "header";
            this.header.RightMargin = 0;
            this.header.Text = this.DisplayName;

            this.colorRectangle = new ColorRectangleControl();
            this.colorRectangle.Name = "colorRectangle";
            this.colorRectangle.TabStop = false;
            this.colorRectangle.TabIndex = 0;

            this.hsvColorWheel = new ColorWheel();
            this.hsvColorWheel.Name = "hsvColorWheel";
            this.hsvColorWheel.ColorChanged += new EventHandler(HsvColorWheel_ColorChanged);
            this.hsvColorWheel.TabStop = false;
            this.hsvColorWheel.TabIndex = 1;

            this.saturationSlider = new ColorGradientControl();
            this.saturationSlider.Name = "saturationSlider";
            this.saturationSlider.Orientation = Orientation.Vertical;
            this.saturationSlider.ValueChanged += new IndexEventHandler(SaturationSlider_ValueChanged);
            this.saturationSlider.TabStop = false;
            this.saturationSlider.TabIndex = 2;

            this.valueSlider = new ColorGradientControl();
            this.valueSlider.Name = "valueSlider";
            this.valueSlider.Orientation = Orientation.Vertical;
            this.valueSlider.ValueChanged += new IndexEventHandler(ValueSlider_ValueChanged);
            this.valueSlider.TabStop = false;
            this.valueSlider.TabIndex = 3;

            this.redLabel = new Label();
            this.redLabel.Name = "redLabel";
            this.redLabel.AutoSize = true;
            this.redLabel.Text = PdnResources.GetString("ColorsForm.RedLabel.Text");

            this.redNud = new PdnNumericUpDown();
            this.redNud.Name = "redNud";
            this.redNud.Minimum = 0;
            this.redNud.Maximum = 255;
            this.redNud.TextAlign = HorizontalAlignment.Right;
            this.redNud.ValueChanged += new EventHandler(RedNud_ValueChanged);
            this.redNud.TabIndex = 4;

            this.greenLabel = new Label();
            this.greenLabel.Name = "greenLabel";
            this.greenLabel.AutoSize = true;
            this.greenLabel.Text = PdnResources.GetString("ColorsForm.GreenLabel.Text");

            this.greenNud = new PdnNumericUpDown();
            this.greenNud.Name = "greenNud";
            this.greenNud.Minimum = 0;
            this.greenNud.Maximum = 255;
            this.greenNud.TextAlign = HorizontalAlignment.Right;
            this.greenNud.ValueChanged += new EventHandler(GreenNud_ValueChanged);
            this.greenNud.TabIndex = 5;

            this.blueLabel = new Label();
            this.blueLabel.Name = "blueLabel";
            this.blueLabel.AutoSize = true;
            this.blueLabel.Text = PdnResources.GetString("ColorsForm.BlueLabel.Text");

            this.blueNud = new PdnNumericUpDown();
            this.blueNud.Name = "blueNud";
            this.blueNud.Minimum = 0;
            this.blueNud.Maximum = 255;
            this.blueNud.TextAlign = HorizontalAlignment.Right;
            this.blueNud.ValueChanged += new EventHandler(BlueNud_ValueChanged);
            this.blueNud.TabIndex = 6;

            this.resetButton = new Button();
            this.resetButton.AutoSize = true;
            this.resetButton.Name = "resetButton";
            this.resetButton.FlatStyle = FlatStyle.Standard;
            this.resetButton.Click += new EventHandler(ResetButton_Click);
            this.resetButton.Image = PdnResources.GetImage("Icons.ResetIcon.png");
            this.resetButton.Width = 1;
            this.resetButton.Visible = (bool)propInfo.ControlProperties[ControlInfoPropertyNames.ShowResetButton].Value;
            this.ToolTip.SetToolTip(this.resetButton, PdnResources.GetString("Form.ResetButton.Text").Replace("&", ""));
            this.resetButton.TabIndex = 7;

            this.description = new Label();
            this.description.Name = "description";
            this.description.Text = this.Description;

            this.Controls.AddRange(
                new Control[]
                {
                    this.header,
                    this.hsvColorWheel,
                    this.saturationSlider,
                    this.valueSlider,
                    this.colorRectangle,
                    this.redLabel,
                    this.redNud,
                    this.greenLabel,
                    this.greenNud,
                    this.blueLabel,
                    this.blueNud,
                    this.resetButton,
                    this.description
                });

            ResumeLayout(false);
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            if (Property.Value != Property.DefaultValue)
            {
                Property.Value = Property.DefaultValue;
            }
        }

        private int changingStack = 0;

        private void HsvColorWheel_ColorChanged(object sender, EventArgs e)
        {
            if (this.changingStack == 0)
            {
                ++this.changingStack;

                HsvColor hsv = this.hsvColorWheel.HsvColor;
                SetPropertyValueFromHsv(hsv);

                --this.changingStack;
            }
        }

        private void ValueSlider_ValueChanged(object sender, IndexEventArgs ce)
        {
            if (this.changingStack == 0)
            {
                ++this.changingStack;

                HsvColor hsv = this.hsvColorWheel.HsvColor;
                hsv.Value = (this.valueSlider.Value * 100) / 255;
                SetPropertyValueFromHsv(hsv);

                --this.changingStack;
            }
        }

        private void SaturationSlider_ValueChanged(object sender, IndexEventArgs ce)
        {
            if (this.changingStack == 0)
            {
                ++this.changingStack;

                HsvColor hsv = this.hsvColorWheel.HsvColor;
                hsv.Saturation = (this.saturationSlider.Value * 100) / 255;
                SetPropertyValueFromHsv(hsv);

                --this.changingStack;
            }
        }

        private void RedNud_ValueChanged(object sender, EventArgs e)
        {
            if (this.changingStack == 0)
            {
                ++this.changingStack;
                SetPropertyValueFromRgb((int)this.redNud.Value, (int)this.greenNud.Value, (int)this.blueNud.Value);
                --this.changingStack;
            }
        }

        private void GreenNud_ValueChanged(object sender, EventArgs e)
        {
            if (this.changingStack == 0)
            {
                ++this.changingStack;
                SetPropertyValueFromRgb((int)this.redNud.Value, (int)this.greenNud.Value, (int)this.blueNud.Value);
                --this.changingStack;
            }
        }

        private void BlueNud_ValueChanged(object sender, EventArgs e)
        {
            if (this.changingStack == 0)
            {
                ++this.changingStack;
                SetPropertyValueFromRgb((int)this.redNud.Value, (int)this.greenNud.Value, (int)this.blueNud.Value);
                --this.changingStack;
            }
        }

        private void SetPropertyValueFromRgb(int red, int green, int blue)
        {
            UI.SuspendControlPainting(this);

            try
            {
                if (this.redNud.Value != red)
                {
                    this.redNud.Value = red;
                }

                if (this.greenNud.Value != green)
                {
                    this.greenNud.Value = green;
                }

                if (this.blueNud.Value != blue)
                {
                    this.blueNud.Value = blue;
                }

                if (this.inOnPropertyValueChanged == 0)
                {
                    int newValue = (red << 16) | (green << 8) | blue;
                    if (Property.Value != newValue)
                    {
                        Property.Value = newValue;
                    }
                }

                RgbColor rgb = new RgbColor(red, green, blue);
                HsvColor hsv = rgb.ToHsv();
                this.hsvColorWheel.HsvColor = hsv;
                this.valueSlider.Value = (hsv.Value * 255) / 100;
                this.saturationSlider.Value = (hsv.Saturation * 255) / 100;

                HsvColor hsvValMin = hsv;
                hsvValMin.Value = 0;
                HsvColor hsvValMax = hsv;
                hsvValMax.Value = 100;
                this.valueSlider.MinColor = hsvValMin.ToColor();
                this.valueSlider.MaxColor = hsvValMax.ToColor();

                HsvColor hsvSatMin = hsv;
                hsvSatMin.Saturation = 0;
                HsvColor hsvSatMax = hsv;
                hsvSatMax.Saturation = 100;
                this.saturationSlider.MinColor = hsvSatMin.ToColor();
                this.saturationSlider.MaxColor = hsvSatMax.ToColor();
            }

            finally
            {
                UI.ResumeControlPainting(this);

                if (UI.IsControlPaintingEnabled(this))
                {
                    Refresh();
                }
            }
        }

        private void SetPropertyValueFromHsv(HsvColor hsv)
        {
            UI.SuspendControlPainting(this);

            try
            {
                RgbColor rgb = hsv.ToRgb();
                SetPropertyValueFromRgb(rgb.Red, rgb.Green, rgb.Blue);

                if (this.hsvColorWheel.HsvColor != hsv)
                {
                    this.hsvColorWheel.HsvColor = hsv;
                }

                if (this.valueSlider.Value != (hsv.Value * 255) / 100)
                {
                    this.valueSlider.Value = (hsv.Value * 255) / 100;
                }

                if (this.saturationSlider.Value != (hsv.Saturation * 255) / 100)
                {
                    this.saturationSlider.Value = (hsv.Saturation * 255) / 100;
                }

                HsvColor hsvValMin = hsv;
                hsvValMin.Value = 0;
                HsvColor hsvValMax = hsv;
                hsvValMax.Value = 100;
                this.valueSlider.MinColor = hsvValMin.ToColor();
                this.valueSlider.MaxColor = hsvValMax.ToColor();

                HsvColor hsvSatMin = hsv;
                hsvSatMin.Saturation = 0;
                HsvColor hsvSatMax = hsv;
                hsvSatMax.Saturation = 100;
                this.saturationSlider.MinColor = hsvSatMin.ToColor();
                this.saturationSlider.MaxColor = hsvSatMax.ToColor();
            }

            finally
            {
                UI.ResumeControlPainting(this);

                if (UI.IsControlPaintingEnabled(this))
                {
                    Refresh();
                }
            }
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            int vMargin = UI.ScaleHeight(4);
            int hMargin = UI.ScaleWidth(4);

            this.header.Location = new Point(0, 0);
            this.header.Size = string.IsNullOrEmpty(DisplayName) ?
                new Size(0, 0) : 
                this.header.GetPreferredSize(new Size(ClientSize.Width, 0));

            if (this.resetButton.Visible)
            {
                this.resetButton.PerformLayout();
            }
            else
            {
                this.resetButton.Size = new Size(0, 0);
            }

            int baseNudWidth = UI.ScaleWidth(50);
            int nudWidth = Math.Max(baseNudWidth, this.resetButton.Width);

            this.redNud.PerformLayout();
            this.redNud.Width = nudWidth;
            this.redNud.Location = new Point(
                ClientSize.Width - this.redNud.Width, 
                this.header.Bottom + vMargin);

            this.redLabel.PerformLayout();
            this.redLabel.Location = new Point(
                this.redNud.Left - this.redLabel.Width - hMargin,
                this.redNud.Top + (this.redNud.Height - this.redLabel.Height) / 2);

            this.greenNud.PerformLayout();
            this.greenNud.Width = nudWidth;
            this.greenNud.Location = new Point(
                ClientSize.Width - this.greenNud.Width,
                Math.Max(this.redNud.Bottom, this.redLabel.Bottom) + vMargin);

            this.greenLabel.PerformLayout();
            this.greenLabel.Location = new Point(
                this.greenNud.Left - this.greenLabel.Width - hMargin,
                this.greenNud.Top + (this.greenNud.Height - this.greenLabel.Height) / 2);

            this.blueNud.PerformLayout();
            this.blueNud.Width = nudWidth;
            this.blueNud.Location = new Point(
                ClientSize.Width - this.blueNud.Width,
                Math.Max(this.greenNud.Bottom, this.greenLabel.Bottom) + vMargin);

            this.blueLabel.PerformLayout();
            this.blueLabel.Location = new Point(
                this.blueNud.Left - this.blueLabel.Width - hMargin,
                this.blueNud.Top + (this.blueNud.Height - this.blueLabel.Height) / 2);

            this.resetButton.Location = new Point(
                ClientSize.Width - this.resetButton.Width,
                Math.Max(this.blueNud.Bottom, this.blueLabel.Bottom) + vMargin);
            this.resetButton.Width = Math.Max(this.resetButton.Width, nudWidth);

            int colorsMaxRight = Math.Min(this.redLabel.Left, Math.Min(this.greenLabel.Left, this.blueLabel.Left));
            int colorsMinLeft = 0;
            int colorsAvailableWidth = colorsMaxRight - colorsMinLeft;

            this.colorRectangle.Top = this.header.Bottom + vMargin;
            this.hsvColorWheel.Top = this.header.Bottom + vMargin;

            int hsvSide = this.resetButton.Bottom - this.hsvColorWheel.Top;

            this.colorRectangle.Size = UI.ScaleSize(new Size(28, 28));

            this.hsvColorWheel.Size = new Size(hsvSide, hsvSide);

            this.saturationSlider.Top = this.header.Bottom + vMargin;
            this.saturationSlider.Size = new Size(UI.ScaleWidth(20), this.hsvColorWheel.Height);

            this.valueSlider.Top = this.header.Bottom + vMargin;
            this.valueSlider.Size = new Size(UI.ScaleWidth(20), this.hsvColorWheel.Height);

            int colorsWidth = 
                this.colorRectangle.Width + hMargin + 
                this.hsvColorWheel.Width + hMargin + 
                this.saturationSlider.Width + hMargin + 
                this.valueSlider.Width;

            int colorsLeft = colorsMinLeft + (colorsAvailableWidth - colorsWidth) / 2;

            this.colorRectangle.Left = colorsLeft;
            this.hsvColorWheel.Left = this.colorRectangle.Right + hMargin;
            this.saturationSlider.Left = this.hsvColorWheel.Right + hMargin;
            this.valueSlider.Left = this.saturationSlider.Right + hMargin;

            this.description.Location = new Point(
                0,
                (string.IsNullOrEmpty(Description) ? 0 : vMargin) + this.hsvColorWheel.Bottom);

            this.description.Width = ClientSize.Width;
            this.description.Height = string.IsNullOrEmpty(Description) ? 0 :
                this.description.GetPreferredSize(new Size(this.description.Width, 1)).Height;

            ClientSize = new Size(ClientSize.Width, this.description.Bottom);

            base.OnLayout(e);
        }

        protected override void OnPropertyReadOnlyChanged()
        {
            this.hsvColorWheel.Enabled = !Property.ReadOnly;
            this.valueSlider.Enabled = !Property.ReadOnly;
            this.redNud.Enabled = !Property.ReadOnly;
            this.redLabel.Enabled = !Property.ReadOnly;
            this.greenNud.Enabled = !Property.ReadOnly;
            this.greenLabel.Enabled = !Property.ReadOnly;
            this.blueNud.Enabled = !Property.ReadOnly;
            this.blueLabel.Enabled = !Property.ReadOnly;
            this.resetButton.Enabled = !Property.ReadOnly;
        }

        private int inOnPropertyValueChanged = 0; // this field is necessary to avoid having Effect`1.OnSetRenderInfo() called 3-4+ times at effect startup
        protected override void OnPropertyValueChanged()
        {
            ++this.inOnPropertyValueChanged;

            try
            {
                int value = Property.Value;
                int red = (value >> 16) & 0xff;
                int green = (value >> 8) & 0xff;
                int blue = value & 0xff;
                SetPropertyValueFromRgb(red, green, blue);

                ColorBgra color = ColorBgra.FromBgr((byte)blue, (byte)green, (byte)red);
                this.colorRectangle.RectangleColor = color.ToColor();
            }

            finally
            {
                --this.inOnPropertyValueChanged;
            }
        }

        protected override bool OnFirstSelect()
        {
            this.redNud.Select();
            return true;
        }
    }
}
