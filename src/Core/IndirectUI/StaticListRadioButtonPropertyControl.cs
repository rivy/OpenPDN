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
using System.Globalization;
using System.Windows.Forms;

namespace PaintDotNet.IndirectUI
{
    [PropertyControlInfo(typeof(StaticListChoiceProperty), PropertyControlType.RadioButton)]
    internal sealed class StaticListRadioButtonPropertyControl
        : PropertyControl<object, StaticListChoiceProperty>
    {
        private HeaderLabel header;

        // radioButtons[i] corresponds to Property.ValueChoices[i]
        private RadioButton[] radioButtons;

        private Label descriptionText;

        public StaticListRadioButtonPropertyControl(PropertyControlInfo propInfo)
            : base(propInfo)
        {
            SuspendLayout();

            this.header = new HeaderLabel();
            this.header.Name = "header";
            this.header.RightMargin = 0;
            this.header.Text = this.DisplayName;

            object[] valueChoices = Property.ValueChoices; // cache this to avoid making N copies
            this.radioButtons = new RadioButton[valueChoices.Length];

            for (int i = 0; i < this.radioButtons.Length; ++i)
            {
                this.radioButtons[i] = new RadioButton();
                this.radioButtons[i].Name = "radioButton" + i.ToString(CultureInfo.InvariantCulture);
                this.radioButtons[i].FlatStyle = FlatStyle.System;
                this.radioButtons[i].CheckedChanged += new EventHandler(RadioButton_CheckedChanged);

                string valueText = propInfo.GetValueDisplayName(valueChoices[i]);
                this.radioButtons[i].Text = valueText;
            }

            this.descriptionText = new Label();
            this.descriptionText.Name = "descriptionText";
            this.descriptionText.AutoSize = false;
            this.descriptionText.Text = this.Description;

            this.Controls.Add(this.header);
            this.Controls.AddRange(this.radioButtons);
            this.Controls.Add(this.descriptionText);

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

            int currentY = this.header.Bottom;
            for (int i = 0; i < this.radioButtons.Length; ++i)
            {
                this.radioButtons[i].Location = new Point(0, currentY + vSpacing);
                this.radioButtons[i].Width = ClientSize.Width;

                LayoutUtility.PerformAutoLayout(
                    this.radioButtons[i],
                    AutoSizeStrategy.ExpandHeightToContentAndKeepWidth,
                    EdgeSnapOptions.SnapLeftEdgeToContainerLeftEdge |
                        EdgeSnapOptions.SnapRightEdgeToContainerRightEdge);

                currentY = this.radioButtons[i].Bottom;
            }

            this.descriptionText.Location = new Point(
                0,
                currentY + (string.IsNullOrEmpty(this.descriptionText.Text) ? 0 : vSpacing));

            this.descriptionText.Width = ClientSize.Width;
            this.descriptionText.Height = string.IsNullOrEmpty(this.descriptionText.Text) ? 0 :
                this.descriptionText.GetPreferredSize(new Size(this.descriptionText.Width, 1)).Height;

            currentY = this.descriptionText.Bottom;

            ClientSize = new Size(ClientSize.Width, currentY);

            base.OnLayout(levent);
        }

        private void RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton senderRB = (RadioButton)sender;

            if (senderRB.Checked)
            {
                int rbIndex = Array.IndexOf(this.radioButtons, senderRB);

                object value = Property.ValueChoices[rbIndex];

                if (!Property.Value.Equals(value))
                {
                    Property.Value = value;
                }
            }
        }

        protected override void OnPropertyReadOnlyChanged()
        {
            this.header.Enabled = !Property.ReadOnly;

            foreach (RadioButton rb in this.radioButtons)
            {
                rb.Enabled = !Property.ReadOnly;
            }
        }

        protected override void OnPropertyValueChanged()
        {
            int valueIndex = Array.IndexOf(Property.ValueChoices, Property.Value);

            if (valueIndex >= 0 && valueIndex < this.radioButtons.Length)
            {
                if (this.radioButtons[valueIndex].Checked != true)
                {
                    this.radioButtons[valueIndex].Checked = true;
                }
            }
        }

        protected override bool OnFirstSelect()
        {
            foreach (RadioButton radioButton in this.radioButtons)
            {
                if (radioButton.Checked)
                {
                    radioButton.Select();
                    return true;
                }
            }

            return false;
        }
    }
}
