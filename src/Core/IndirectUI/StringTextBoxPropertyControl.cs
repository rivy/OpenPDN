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
    [PropertyControlInfo(typeof(StringProperty), PropertyControlType.TextBox, IsDefault = true)]
    internal sealed class StringTextBoxPropertyControl
        : PropertyControl<string, StringProperty>
    {
        private HeaderLabel header;
        private TextBox textBox;
        private Label description;
        private int baseTextBoxHeight;

        [PropertyControlProperty(DefaultValue = false)]
        public bool Multiline
        {
            get
            {
                return this.textBox.Multiline;
            }

            set
            {
                this.textBox.Multiline = value;
                this.textBox.AcceptsReturn = value;
                PerformLayout();
            }
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            int vMargin = UI.ScaleHeight(4);
            int hMargin = UI.ScaleWidth(4);

            this.header.Location = new Point(0, 0);
            this.header.Size = string.IsNullOrEmpty(DisplayName) ?
                new Size(ClientSize.Width, 0) :
                this.header.GetPreferredSize(new Size(ClientSize.Width, 1));

            this.textBox.Location = new Point(0, this.header.Bottom + hMargin);
            this.textBox.Width = ClientSize.Width;
            this.textBox.Height = this.textBox.Multiline ? this.baseTextBoxHeight * 4 : this.baseTextBoxHeight;

            this.description.Location = new Point(0,
                (string.IsNullOrEmpty(this.Description) ? 0 : vMargin) + this.textBox.Bottom);

            this.description.Width = ClientSize.Width;
            this.description.Height = string.IsNullOrEmpty(this.description.Text) ? 0 :
                this.description.GetPreferredSize(new Size(this.description.Width, 1)).Height;

            ClientSize = new Size(ClientSize.Width, this.description.Bottom);

            base.OnLayout(levent);
        }

        public StringTextBoxPropertyControl(PropertyControlInfo propInfo)
            : base(propInfo)
        {
            SuspendLayout();

            this.header = new HeaderLabel();
            this.textBox = new TextBox();
            this.description = new Label();

            this.header.Name = "header";
            this.header.Text = DisplayName;
            this.header.RightMargin = 0;

            this.description.Name = "description";
            this.description.Text = this.Description;

            this.textBox.Name = "textBox";
            this.textBox.TextChanged += new EventHandler(TextBox_TextChanged);
            this.textBox.MaxLength = Property.MaxLength;
            this.baseTextBoxHeight = this.textBox.Height;
            this.Multiline = (bool)propInfo.ControlProperties[ControlInfoPropertyNames.Multiline].Value;

            this.Controls.AddRange(
                new Control[]
                {
                    this.header,
                    this.textBox,
                    this.description
                });

            ResumeLayout(false);
        }

        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            string newValue;

            if (this.textBox.Text.Length > Property.MaxLength)
            {
                newValue = this.textBox.Text.Substring(Property.MaxLength);
            }
            else
            {
                newValue = this.textBox.Text;
            }

            if (Property.Value != newValue)
            {
                Property.Value = newValue;
            }
        }

        protected override void OnDisplayNameChanged()
        {
            this.header.Text = DisplayName;
            base.OnDisplayNameChanged();
        }

        protected override void OnDescriptionChanged()
        {
            this.description.Text = Description;
            base.OnDescriptionChanged();
        }

        protected override void OnPropertyReadOnlyChanged()
        {
            this.textBox.Enabled = !Property.ReadOnly;
            this.textBox.ReadOnly = Property.ReadOnly;
            this.description.Enabled = !Property.ReadOnly;
        }

        protected override void OnPropertyValueChanged()
        {
            if (this.textBox.Text != Property.Value)
            {
                this.textBox.Text = Property.Value;
            }
        }

        protected override bool OnFirstSelect()
        {
            this.textBox.Select();
            return true;
        }
    }
}