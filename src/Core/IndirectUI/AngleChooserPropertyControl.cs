/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) Rick Brewster, Tom Jackson, and past contributors.            //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet;
using PaintDotNet.Core;
using PaintDotNet.PropertySystem;
using PaintDotNet.SystemLayer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PaintDotNet.IndirectUI
{
    [PropertyControlInfo(typeof(DoubleProperty), PropertyControlType.AngleChooser)]
    internal sealed class AngleChooserPropertyControl
        : PropertyControl<double, DoubleProperty>
    {
        private HeaderLabel header;
        private AngleChooserControl angleChooser;
        private NumericUpDown valueNud;
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
                PerformLayout();
            }
        }

        protected override void OnPropertyReadOnlyChanged()
        {
            this.angleChooser.Enabled = !Property.ReadOnly;
            this.valueNud.Enabled = !Property.ReadOnly;
            this.resetButton.Enabled = !Property.ReadOnly;
            this.description.Enabled = !Property.ReadOnly;
        }

        private double FromAngleChooserValue(double angleChooserValue)
        {
            if (this.Property.MinValue == -180)
            {
                // property value's range is [-180, +180]
                return angleChooserValue;
            }
            else
            {
                // property value's range is [0, 360]
                if (angleChooserValue > 0)
                {
                    return angleChooserValue;
                }
                else
                {
                    return angleChooserValue + 360;
                }
            }
        }

        private double ToAngleChooserValue(double nudValue)
        {
            if (this.Property.MinValue == -180)
            {
                // property value's range is [-180, +180]
                return nudValue;
            }
            else
            {
                // property value's range is [0, 360]
                if (nudValue <= 180.0)
                {
                    return nudValue;
                }
                else
                {
                    return nudValue - 360;
                }
            }
        }

        protected override void OnPropertyValueChanged()
        {
            if (this.angleChooser.ValueDouble != ToAngleChooserValue(Property.Value))
            {
                this.angleChooser.ValueDouble = ToAngleChooserValue(Property.Value);
            }

            if (this.valueNud.Value != (decimal)Property.Value)
            {
                this.valueNud.Value = (decimal)Property.Value;
            }
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            int vMargin = UI.ScaleHeight(4);
            int hMargin = UI.ScaleWidth(4);

            this.header.Location = new Point(0, 0);
            this.header.Size = string.IsNullOrEmpty(DisplayName) ? new Size(ClientSize.Width, 0) :
                this.header.GetPreferredSize(new Size(ClientSize.Width, 1));
            this.header.Visible = !string.IsNullOrEmpty(DisplayName);

            this.resetButton.Width = UI.ScaleWidth(20);
            this.resetButton.Location = new Point(
                ClientSize.Width - this.resetButton.Width, 
                this.header.Bottom + vMargin);

            int baseNudWidth = UI.ScaleWidth(70);
            this.valueNud.PerformLayout();
            this.valueNud.Width = baseNudWidth;
            this.valueNud.Location = new Point(
                this.resetButton.Left - hMargin - this.valueNud.Width, 
                this.header.Bottom + vMargin);

            this.resetButton.Height = this.valueNud.Height;
            
            this.angleChooser.Size = UI.ScaleSize(new Size(60, 60));
            int angleChooserMinLeft = hMargin;
            int angleChooserMaxRight = this.valueNud.Left - hMargin;
            double angleChooserCenter = (double)(angleChooserMinLeft + angleChooserMaxRight) / 2.0;
            int angleChooserLeft = (int)(angleChooserCenter - ((double)this.angleChooser.Width / 2.0));
            this.angleChooser.Location = new Point(angleChooserLeft, this.header.Bottom + vMargin);

            this.description.Location = new Point(0, Math.Max(this.valueNud.Bottom, Math.Max(this.resetButton.Bottom, this.angleChooser.Bottom)));
            this.description.Width = ClientSize.Width;
            this.description.Height = string.IsNullOrEmpty(this.description.Text) ? 0 :
                this.description.GetPreferredSize(new Size(this.description.Width, 1)).Height;

            ClientSize = new Size(ClientSize.Width, description.Bottom);

            base.OnLayout(levent);
        }

        protected override void OnDisplayNameChanged()
        {
            this.header.Text = this.DisplayName;
            base.OnDisplayNameChanged();
        }

        protected override void OnDescriptionChanged()
        {
            this.description.Text = this.Description;
            base.OnDescriptionChanged();
        }

        public AngleChooserPropertyControl(PropertyControlInfo propInfo)
            : base(propInfo)
        {
            DoubleProperty doubleProp = (DoubleProperty)propInfo.Property;
            if (!((doubleProp.MinValue == -180 && doubleProp.MaxValue == +180) ||
                (doubleProp.MinValue == 0 && doubleProp.MaxValue == 360)))
            {
                throw new ArgumentException("Only two min/max ranges are allowed for the AngleChooser control type: [-180, +180] and [0, 360]");
            }

            this.header = new HeaderLabel();
            this.header.Name = "header";
            this.header.RightMargin = 0;
            this.header.Text = this.DisplayName;

            this.angleChooser = new AngleChooserControl();
            this.angleChooser.Name = "angleChooser";
            this.angleChooser.ValueChanged += new EventHandler(AngleChooser_ValueChanged);

            this.valueNud = new NumericUpDown();
            this.valueNud.Name = "numericUpDown";
            this.valueNud.Minimum = (decimal)Property.MinValue;
            this.valueNud.Maximum = (decimal)Property.MaxValue;
            this.valueNud.DecimalPlaces = 2;
            this.valueNud.ValueChanged += new EventHandler(ValueNud_ValueChanged);
            this.valueNud.TextAlign = HorizontalAlignment.Right;

            this.resetButton = new Button();
            this.resetButton.Name = "resetButton";
            this.resetButton.FlatStyle = FlatStyle.Standard;
            this.resetButton.Click += new EventHandler(ResetButton_Click);
            this.resetButton.Image = PdnResources.GetImageResource("Icons.ResetIcon.png").Reference;
            this.resetButton.Visible = (bool)propInfo.ControlProperties[ControlInfoPropertyNames.ShowResetButton].Value; 
            this.ToolTip.SetToolTip(this.resetButton, PdnResources.GetString("Form.ResetButton.Text").Replace("&", ""));

            this.description = new Label();
            this.description.Name = "descriptionText";
            this.description.AutoSize = false;
            this.description.Text = this.Description;

            SuspendLayout();

            this.Controls.AddRange(
                new Control[]
                {
                    this.header,
                    this.angleChooser,
                    this.valueNud,
                    this.resetButton,
                    this.description
                });

            ResumeLayout(false);
            PerformLayout();
        }

        private void AngleChooser_ValueChanged(object sender, EventArgs e)
        {
            if (Property.Value != FromAngleChooserValue(this.angleChooser.ValueDouble))
            {
                Property.Value = FromAngleChooserValue(this.angleChooser.ValueDouble);
            }
        }

        private void ValueNud_ValueChanged(object sender, EventArgs e)
        {
            if (Property.Value != (double)this.valueNud.Value)
            {
                Property.Value = (double)this.valueNud.Value;
            }
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            Property.Value = (double)Property.DefaultValue;
        }

        protected override bool OnFirstSelect()
        {
            this.valueNud.Select();
            return true;
        }
    }
}
