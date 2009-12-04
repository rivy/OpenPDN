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
    [PropertyControlInfo(typeof(Int32Property), PropertyControlType.IncrementButton)]
    internal sealed class Int32IncrementButtonPropertyControl
        : PropertyControl<int, Int32Property>
    {
        private HeaderLabel header;
        private Button incrementButton;
        private Label descriptionText;

        [PropertyControlProperty(DefaultValue = "+")]
        public string ButtonText
        {
            get
            {
                return this.incrementButton.Text;
            }

            set
            {
                this.incrementButton.Text = value;
            }
        }

        public Int32IncrementButtonPropertyControl(PropertyControlInfo propInfo)
            : base(propInfo)
        {
            SuspendLayout();

            this.header = new HeaderLabel();
            this.header.Name = "header";
            this.header.Text = this.DisplayName;
            this.header.RightMargin = 0;

            this.incrementButton = new Button();
            this.incrementButton.Name = "incrementButton";
            this.incrementButton.AutoSize = true;
            this.incrementButton.FlatStyle = FlatStyle.System;
            this.incrementButton.Text = (string)propInfo.ControlProperties[ControlInfoPropertyNames.ButtonText].Value;
            this.incrementButton.Click += new EventHandler(IncrementButton_Click);

            this.descriptionText = new Label();
            this.descriptionText.Name = "descriptionText";
            this.descriptionText.AutoSize = false;
            this.descriptionText.Text = this.Description;

            this.Controls.AddRange(
                new Control[]
                {
                    this.header,
                    this.incrementButton,
                    this.descriptionText
                });

            ResumeLayout(false);
            PerformLayout();
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            int vSpacing = UI.ScaleHeight(4);

            this.header.Location = new Point(0, 0);
            this.header.Width = ClientSize.Width;
            this.header.Height = string.IsNullOrEmpty(this.header.Text) ? 0 : this.header.GetPreferredSize(new Size(this.header.Width, 1)).Height;

            this.incrementButton.PerformLayout();
            this.incrementButton.Location = new Point(
                0,
                this.header.Bottom + (string.IsNullOrEmpty(this.header.Text) ? 0 : vSpacing));

            this.descriptionText.Location = new Point(
                0,
                this.incrementButton.Bottom + (string.IsNullOrEmpty(this.descriptionText.Text) ? 0 : vSpacing));
            this.descriptionText.Width = ClientSize.Width;
            this.descriptionText.Height = string.IsNullOrEmpty(this.descriptionText.Text) ? 0 : 
                this.descriptionText.GetPreferredSize(new Size(this.descriptionText.Width, 1)).Height;

            ClientSize = new Size(ClientSize.Width, this.descriptionText.Bottom);

            base.OnLayout(levent);
        }

        private void IncrementButton_Click(object sender, EventArgs e)
        {
            long minValue = (long)Property.MinValue;
            long maxValue = (long)Property.MaxValue;
            long deltaCount = maxValue - minValue + 1;

            if (deltaCount != 0)
            {
                long value1 = (long)Property.Value;
                long value2 = 1 + value1;
                long value3 = ((value2 - minValue) % deltaCount) + minValue;
                long newValue = value3;

                Property.Value = (int)newValue;
            }
        }

        protected override void OnPropertyReadOnlyChanged()
        {
            this.header.Enabled = !Property.ReadOnly;
            this.incrementButton.Enabled = !Property.ReadOnly;
        }

        protected override void OnPropertyValueChanged()
        {
            // nothing to do really
        }

        protected override bool OnFirstSelect()
        {
            this.incrementButton.Select();
            return true;
        }
    }
}
