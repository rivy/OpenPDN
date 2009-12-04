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
using System.Windows.Forms;

namespace PaintDotNet.IndirectUI
{
    [PropertyControlInfo(typeof(StaticListChoiceProperty), PropertyControlType.DropDown)]
    internal sealed class StaticListDropDownPropertyControl
        : PropertyControl<object, StaticListChoiceProperty>
    {
        private HeaderLabel header;
        private ComboBox comboBox;
        private Label descriptionText;

        public StaticListDropDownPropertyControl(PropertyControlInfo propInfo)
            : base(propInfo)
        {
            SuspendLayout();

            this.header = new HeaderLabel();
            this.header.Name = "header";
            this.header.RightMargin = 0;
            this.header.Text = this.DisplayName;

            this.comboBox = new ComboBox();
            this.comboBox.Name = "comboBox";
            this.comboBox.FlatStyle = FlatStyle.System;
            this.comboBox.SelectedIndexChanged += new EventHandler(ComboBox_SelectedIndexChanged);
            this.comboBox.DropDownStyle = ComboBoxStyle.DropDownList;

            foreach (object choice in Property.ValueChoices)
            {
                string valueText = propInfo.GetValueDisplayName(choice);
                this.comboBox.Items.Add(valueText);
            }

            this.descriptionText = new Label();
            this.descriptionText.Name = "descriptionText";
            this.descriptionText.AutoSize = false;
            this.descriptionText.Text = this.Description;

            this.Controls.AddRange(
                new Control[]
                {
                    this.header,
                    this.comboBox,
                    this.descriptionText
                });

            ResumeLayout(false);
            PerformLayout();
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            int vSpacing = UI.ScaleHeight(4);

            this.header.Location = new Point(0, 0);
            this.header.Size = string.IsNullOrEmpty(DisplayName) ?
                new Size(ClientSize.Width, 0) :
                this.header.GetPreferredSize(new Size(ClientSize.Width, 1));

            this.comboBox.Location = new Point(0, this.header.Bottom + vSpacing);
            this.comboBox.Width = ClientSize.Width;
            this.comboBox.PerformLayout();

            this.descriptionText.Location = new Point(
                0,
                this.comboBox.Bottom + (string.IsNullOrEmpty(this.descriptionText.Text) ? 0 : vSpacing));

            this.descriptionText.Width = ClientSize.Width;
            this.descriptionText.Height = string.IsNullOrEmpty(this.descriptionText.Text) ? 0 :
                this.descriptionText.GetPreferredSize(new Size(this.descriptionText.Width, 1)).Height;

            ClientSize = new Size(ClientSize.Width, this.descriptionText.Bottom);

            base.OnLayout(levent);
        }

        private void ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            int valueIndex = Array.IndexOf(Property.ValueChoices, Property.Value);

            if (valueIndex != this.comboBox.SelectedIndex)
            {
                Property.Value = Property.ValueChoices[this.comboBox.SelectedIndex];
            }
        }

        protected override void OnPropertyReadOnlyChanged()
        {
            this.header.Enabled = !Property.ReadOnly;
            this.comboBox.Enabled = !Property.ReadOnly;
        }

        protected override void OnPropertyValueChanged()
        {
            int valueIndex = Array.IndexOf(Property.ValueChoices, Property.Value);

            if (this.comboBox.SelectedIndex != valueIndex)
            {
                this.comboBox.SelectedIndex = valueIndex;
            }
        }

        protected override bool OnFirstSelect()
        {
            this.comboBox.Select();
            return true;
        }
    }
}
