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
using System.Drawing;
using System.Windows.Forms;

namespace PaintDotNet.IndirectUI
{
    internal abstract class SliderPropertyControl<TValue>
         : PropertyControl<TValue, ScalarProperty<TValue>>
           where TValue : struct, IComparable<TValue>
    {
        private HeaderLabel header;
        private TrackBar slider;
        private PdnNumericUpDown numericUpDown;
        private Button resetButton;
        private Label descriptionText;

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

        protected int DecimalPlaces
        {
            get
            {
                return this.numericUpDown.DecimalPlaces;
            }

            set
            {
                this.numericUpDown.DecimalPlaces = value;
                ResetUIRanges();
            }
        }

        [PropertyControlProperty(DefaultValue = (object)false)]
        public bool SliderShowTickMarks
        {
            get
            {
                return this.slider.TickStyle != TickStyle.None;
            }

            set
            {
                this.slider.TickStyle = value ? TickStyle.BottomRight : TickStyle.None;
            }
        }

        protected TValue SliderSmallChange
        {
            get
            {
                return FromSliderValue(this.slider.SmallChange);
            }

            set
            {
                this.slider.SmallChange = ToSliderValue(value);
            }
        }

        protected TValue SliderLargeChange
        {
            get
            {
                return FromSliderValue(this.slider.LargeChange);
            }

            set
            {
                this.slider.LargeChange = ToSliderValue(value);
            }
        }

        protected TValue UpDownIncrement
        {
            get
            {
                return FromNudValue(this.numericUpDown.Increment);
            }

            set
            {
                this.numericUpDown.Increment = ToNudValue(value);
            }
        }

        protected abstract int ToSliderValue(TValue propertyValue);
        protected abstract TValue FromSliderValue(int sliderValue);

        protected abstract decimal ToNudValue(TValue propertyValue);
        protected abstract TValue FromNudValue(decimal nudValue);

        protected override void OnLayout(LayoutEventArgs levent)
        {
            int vMargin = UI.ScaleHeight(4);
            int hMargin = UI.ScaleWidth(4);

            this.header.Location = new Point(0, 0);
            this.header.Width = ClientSize.Width;
            this.header.Height = string.IsNullOrEmpty(DisplayName) ? 0 :
                this.header.GetPreferredSize(new Size(this.header.Width, 0)).Height;

            this.resetButton.Width = UI.ScaleWidth(20);
            this.resetButton.Location = new Point(
                ClientSize.Width - this.resetButton.Width,
                this.header.Bottom + vMargin);

            int nudWidth = UI.ScaleWidth(70);

            this.numericUpDown.PerformLayout();
            this.numericUpDown.Width = nudWidth;
            this.numericUpDown.Location = new Point(
                (this.resetButton.Visible ? (this.resetButton.Left - hMargin) : ClientSize.Width) - this.numericUpDown.Width,
                this.header.Bottom + vMargin);

            this.resetButton.Height = this.numericUpDown.Height;

            this.slider.Location = new Point(0, this.header.Bottom + vMargin);
            this.slider.Size = new Size(
                this.numericUpDown.Left - hMargin,
                PropertyControlUtil.GetGoodSliderHeight(this.slider));

            this.descriptionText.Location = new Point(
                0,
                (string.IsNullOrEmpty(this.Description) ? 0 : vMargin) + Utility.Max(this.resetButton.Bottom, this.slider.Bottom, this.numericUpDown.Bottom));

            this.descriptionText.Width = ClientSize.Width;
            this.descriptionText.Height = string.IsNullOrEmpty(this.descriptionText.Text) ? 0 : 
                this.descriptionText.GetPreferredSize(new Size(this.descriptionText.Width, 1)).Height;

            ClientSize = new Size(ClientSize.Width, this.descriptionText.Bottom);

            base.OnLayout(levent);
        }

        protected override void OnDisplayNameChanged()
        {
            this.header.Text = this.DisplayName;
            base.OnDisplayNameChanged();
        }

        protected override void OnDescriptionChanged()
        {
            this.descriptionText.Text = this.Description;
            base.OnDescriptionChanged();
        }

        private void ValidateUIRanges()
        {
            try
            {
                int value1 = ToSliderValue(Property.MinValue);
                int value2 = ToSliderValue(Property.MaxValue);

                decimal value3 = ToNudValue(Property.MinValue);
                decimal value4 = ToNudValue(Property.MaxValue);

                TValue value5 = FromSliderValue(ToSliderValue(Property.MinValue));
                TValue value6 = FromSliderValue(ToSliderValue(Property.MaxValue));

                TValue value7 = FromNudValue(ToNudValue(Property.MinValue));
                TValue value8 = FromNudValue(ToNudValue(Property.MaxValue));
            }

            catch (Exception ex)
            {
                string message = string.Format(
                    "The property's range, [{0}, {1}], cannot be accomodated. Try a smaller range, or a smaller value for DecimalPlaces.",
                    Property.MinValue, 
                    Property.MaxValue);

                throw new PdnException(message, ex);
            }
        }

        protected void ResetUIRanges()
        {
            this.numericUpDown.Minimum = ToNudValue(Property.MinValue);
            this.numericUpDown.Maximum = ToNudValue(Property.MaxValue);
            this.slider.Minimum = ToSliderValue(Property.MinValue);
            this.slider.Maximum = ToSliderValue(Property.MaxValue);
            this.slider.TickFrequency = PropertyControlUtil.GetGoodSliderTickFrequency(this.slider);
        }

        public SliderPropertyControl(PropertyControlInfo propInfo)
            : base(propInfo)
        {
            this.header = new HeaderLabel();
            this.slider = new TrackBar();
            this.numericUpDown = new PdnNumericUpDown();
            this.resetButton = new Button();
            this.descriptionText = new Label();

            this.slider.BeginInit();

            SuspendLayout();

            this.header.Name = "header";
            this.header.RightMargin = 0;
            this.header.Text = this.DisplayName;

            this.numericUpDown.DecimalPlaces = 0;
            this.numericUpDown.Name = "numericUpDown";
            this.numericUpDown.TextAlign = HorizontalAlignment.Right;
            this.numericUpDown.TabIndex = 1;

            this.slider.Name = "slider";
            this.slider.AutoSize = false;
            this.slider.Orientation = Orientation.Horizontal;
            this.slider.TabIndex = 0;
            this.SliderShowTickMarks = (bool)propInfo.ControlProperties[ControlInfoPropertyNames.SliderShowTickMarks].Value;

            this.resetButton.AutoSize = false;
            this.resetButton.Name = "resetButton";
            this.resetButton.FlatStyle = FlatStyle.Standard;
            this.resetButton.Click += new EventHandler(ResetButton_Click);
            this.resetButton.Image = PdnResources.GetImageResource("Icons.ResetIcon.png").Reference;
            this.resetButton.TabIndex = 2;
            this.resetButton.Visible = (bool)propInfo.ControlProperties[ControlInfoPropertyNames.ShowResetButton].Value;
            this.ToolTip.SetToolTip(this.resetButton, PdnResources.GetString("Form.ResetButton.Text").Replace("&", ""));

            this.descriptionText.Name = "descriptionText";
            this.descriptionText.AutoSize = false;
            this.descriptionText.Text = this.Description;

            // In order to make sure that setting the ranges on the controls doesn't affect the property in weird ways,
            // we don't set up our ValueChanged handlers until after we set up the controls.
            ValidateUIRanges();
            ResetUIRanges();

            this.numericUpDown.ValueChanged += new EventHandler(NumericUpDown_ValueChanged);
            this.slider.ValueChanged += new EventHandler(Slider_ValueChanged);

            Controls.AddRange(
                new Control[]
                {
                    this.header,
                    this.slider,
                    this.numericUpDown,
                    this.resetButton,
                    this.descriptionText
                });

            this.slider.EndInit();

            ResumeLayout(false);
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            Property.Value = (TValue)Property.DefaultValue;
        }

        protected override void OnPropertyValueChanged()
        {
            if (this.numericUpDown.Value != ToNudValue(Property.Value))
            {
                decimal newNudValue = ToNudValue(Property.Value);
                this.numericUpDown.Value = newNudValue;
            }

            if (this.slider.Value != ToSliderValue(Property.Value))
            {
                int newSliderValue = ToSliderValue(Property.Value);
                int clampedValue = Utility.Clamp(newSliderValue, this.slider.Minimum, this.slider.Maximum);
                this.slider.Value = clampedValue;
            }
        }

        protected override void OnPropertyReadOnlyChanged()
        {
            this.numericUpDown.Enabled = !Property.ReadOnly;
            this.slider.Enabled = !Property.ReadOnly;
            this.resetButton.Enabled = !Property.ReadOnly;
            this.descriptionText.Enabled = !Property.ReadOnly;
        }

        private void Slider_ValueChanged(object sender, EventArgs e)
        {
            if (ToSliderValue(Property.Value) != ToSliderValue(FromSliderValue(this.slider.Value)))
            {
                TValue fromSliderValue = FromSliderValue(this.slider.Value);
                TValue clampedValue = Property.ClampPotentialValue(fromSliderValue);
                Property.Value = clampedValue;
            }
        }

        private void NumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (ToNudValue(Property.Value) != ToNudValue(FromNudValue(this.numericUpDown.Value)))
            {
                TValue fromNudValue = FromNudValue(this.numericUpDown.Value);
                TValue clampedValue = Property.ClampPotentialValue(fromNudValue);
                Property.Value = clampedValue;
            }
        }

        protected override bool OnFirstSelect()
        {
            this.numericUpDown.Select();
            return true;
        }
    }
}
