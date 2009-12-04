/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) Rick Brewster, Tom Jackson, and past contributors.            //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet;
using PaintDotNet.PropertySystem;
using PaintDotNet.SystemLayer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PaintDotNet.IndirectUI
{
    [PropertyControlInfo(typeof(DoubleVectorProperty), PropertyControlType.PanAndSlider, IsDefault = true)]
    internal sealed class DoubleVectorPanAndSliderPropertyControl
         : PropertyControl<Pair<double, double>, VectorProperty<double>>
    {
        private HeaderLabel header;
        private PaintDotNet.Core.PanControl panControl;
        private DoubleVectorSliderPropertyControl sliders;
        private Label textDescription;

        [PropertyControlProperty(DefaultValue = (object)true)]
        public bool ShowResetButton
        {
            get
            {
                return this.sliders.ShowResetButton;
            }

            set
            {
                this.sliders.ShowResetButton = value;
            }
        }

        [PropertyControlProperty(DefaultValue = (object)2)]
        public int DecimalPlaces
        {
            get
            {
                return this.sliders.DecimalPlaces;
            }

            set
            {
                this.sliders.DecimalPlaces = value;
            }
        }


        [PropertyControlProperty(DefaultValue = (object)false)]
        public bool UseExponentialScale
        {
            get
            {
                return this.sliders.UseExponentialScale;
            }

            set
            {
                this.sliders.UseExponentialScale = value;
            }
        }

        [PropertyControlProperty(DefaultValue = (object)1.0)]
        public double SliderSmallChangeX
        {
            get
            {
                return this.sliders.SliderSmallChangeX;
            }

            set
            {
                this.sliders.SliderSmallChangeX = value;
            }
        }

        [PropertyControlProperty(DefaultValue = (object)5.0)]
        public double SliderLargeChangeX
        {
            get
            {
                return this.sliders.SliderLargeChangeX;
            }

            set
            {
                this.sliders.SliderLargeChangeX = value;
            }
        }

        [PropertyControlProperty(DefaultValue = (object)1.0)]
        public double UpDownIncrementX
        {
            get
            {
                return this.sliders.UpDownIncrementX;
            }

            set
            {
                this.sliders.UpDownIncrementX = value;
            }
        }

        [PropertyControlProperty(DefaultValue = (object)1.0)]
        public double SliderSmallChangeY
        {
            get
            {
                return this.sliders.SliderSmallChangeY;
            }

            set
            {
                this.sliders.SliderSmallChangeY = value;
            }
        }

        [PropertyControlProperty(DefaultValue = (object)5.0)]
        public double SliderLargeChangeY
        {
            get
            {
                return this.sliders.SliderLargeChangeY;
            }

            set
            {
                this.sliders.SliderLargeChangeY = value;
            }
        }

        [PropertyControlProperty(DefaultValue = (object)1.0)]
        public double UpDownIncrementY
        {
            get
            {
                return this.sliders.UpDownIncrementY;
            }

            set
            {
                this.sliders.UpDownIncrementY = value;
            }
        }

        [PropertyControlProperty(DefaultValue = (object)false)]
        public bool SliderShowTickMarksX
        {
            get
            {
                return this.sliders.SliderShowTickMarksX;
            }

            set
            {
                this.sliders.SliderShowTickMarksX = value;
            }
        }

        [PropertyControlProperty(DefaultValue = (object)false)]
        public bool SliderShowTickMarksY
        {
            get
            {
                return this.sliders.SliderShowTickMarksY;
            }

            set
            {
                this.sliders.SliderShowTickMarksY = value;
            }
        }

        [PropertyControlProperty(DefaultValue = null)]
        public ImageResource StaticImageUnderlay
        {
            get
            {
                return this.panControl.StaticImageUnderlay;
            }

            set
            {
                this.panControl.StaticImageUnderlay = value;

                if (value == null)
                {
                    this.panControl.BorderStyle = BorderStyle.FixedSingle;
                }
                else
                {
                    this.panControl.BorderStyle = BorderStyle.None;
                }
            }
        }

        protected override void OnPropertyReadOnlyChanged()
        {
            this.panControl.Enabled = !this.Property.ReadOnly;
            this.textDescription.Enabled = !this.Property.ReadOnly;
        }

        protected override void OnPropertyValueChanged()
        {
            PointF newPos = new PointF((float)this.Property.ValueX, (float)this.Property.ValueY);
            this.panControl.Position = newPos;
        }

        private void PanControl_PositionChanged(object sender, EventArgs e)
        {
            PointF pos = this.panControl.Position;

            PointF clampedPos = new PointF(
                Utility.Clamp(pos.X, (float)this.Property.MinValueX, (float)this.Property.MaxValueX),
                Utility.Clamp(pos.Y, (float)this.Property.MinValueY, (float)this.Property.MaxValueY));

            this.panControl.Position = clampedPos;

            Pair<double, double> pairValue = Pair.Create((double)clampedPos.X, (double)clampedPos.Y);

            this.Property.Value = pairValue;
        }

        protected override void OnDisplayNameChanged()
        {
            this.header.Text = this.DisplayName;
            base.OnDisplayNameChanged();
        }

        protected override void OnDescriptionChanged()
        {
            this.textDescription.Text = this.Description;
            base.OnDescriptionChanged();
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            int vMargin = UI.ScaleHeight(4);
            int hMargin = UI.ScaleWidth(4);

            this.header.Location = new Point(0, 0);
            this.header.Size = 
                string.IsNullOrEmpty(DisplayName) ? 
                    new Size(ClientSize.Width, 0) : 
                    this.header.GetPreferredSize(new Size(ClientSize.Width, 0));

            int panControlLength = Math.Min(this.panControl.Width, this.panControl.Height);
            int tries = 2;

            while (tries > 0)
            {
                --tries;

                this.panControl.Location = new Point(0, this.header.Bottom + vMargin);
                this.panControl.Size = new Size(panControlLength, panControlLength);
                this.panControl.PerformLayout();

                this.sliders.Location = new Point(this.panControl.Right + hMargin, this.header.Bottom + vMargin);
                this.sliders.Width = ClientSize.Width - this.sliders.Left;
                this.sliders.PerformLayout();

                // put some padding to ensure the thumbnail is a little larger
                int sliderBottomPlusReserve = this.sliders.Bottom + UI.ScaleHeight(20 + (SliderShowTickMarksX ? 0 : 4) + (SliderShowTickMarksY ? 0 : 4));

                this.textDescription.Location = new Point(
                    0,
                    (string.IsNullOrEmpty(this.Description) ? 0 : vMargin) + 
                        Math.Max(this.panControl.Bottom, /*this.sliders.Bottom*/ sliderBottomPlusReserve));

                this.textDescription.Width = ClientSize.Width;
                this.textDescription.Height = string.IsNullOrEmpty(this.Description) ? 0 :
                    this.textDescription.GetPreferredSize(new Size(this.textDescription.Width, 1)).Height;

                ClientSize = new Size(ClientSize.Width, this.textDescription.Bottom);

                panControlLength = (this.textDescription.Top - this.panControl.Top - vMargin);
                panControlLength |= 1;
            }

            base.OnLayout(levent);
        }

        public DoubleVectorPanAndSliderPropertyControl(PropertyControlInfo propInfo)
            : base(propInfo)
        {
            SuspendLayout();

            this.header = new HeaderLabel();
            this.header.Name = "header";
            this.header.RightMargin = 0;
            this.header.Text = this.DisplayName;

            this.panControl = new PaintDotNet.Core.PanControl();
            this.panControl.Name = "panControl";
            this.panControl.StaticImageUnderlay = (ImageResource)propInfo.ControlProperties[ControlInfoPropertyNames.StaticImageUnderlay].Value;
            this.panControl.PositionChanged += new EventHandler(PanControl_PositionChanged);
            this.panControl.Size = new Size(1, 1);

            this.sliders = new DoubleVectorSliderPropertyControl(propInfo);
            this.sliders.Name = "sliders";
            this.sliders.DisplayName = "";
            this.sliders.Description = "";

            this.textDescription = new Label();
            this.textDescription.Name = "textDescription";
            this.textDescription.Text = Description;

            this.Controls.AddRange(
                new Control[]
                {
                    this.header,
                    this.panControl,
                    this.sliders,
                    this.textDescription
                });

            ResumeLayout(false);
        }

        protected override bool OnFirstSelect()
        {
            return ((IFirstSelection)this.sliders).FirstSelect();
        }
    }
}
