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
    [PropertyControlInfo(typeof(BooleanProperty), PropertyControlType.CheckBox)]
    internal sealed class BooleanCheckBoxPropertyControl
        : PropertyControl<bool, BooleanProperty>
    {
        private HeaderLabel header;
        private CheckBox checkBox;

        protected override void OnDisplayNameChanged()
        {
            this.header.Text = this.DisplayName;
            this.checkBox.Text = string.IsNullOrEmpty(this.Description) ? this.DisplayName : this.Description;
            base.OnDisplayNameChanged();
        }

        protected override void OnDescriptionChanged()
        {
            this.checkBox.Text = string.IsNullOrEmpty(this.Description) ? this.DisplayName : this.Description;
            base.OnDescriptionChanged();
        }

        public BooleanCheckBoxPropertyControl(PropertyControlInfo propInfo)
            : base(propInfo)
        {
            SuspendLayout();

            this.header = new HeaderLabel();
            this.header.Name = "header";
            this.header.RightMargin = 0;
            this.header.Text = this.DisplayName;

            this.checkBox = new CheckBox();
            this.checkBox.Name = "checkBox";
            this.checkBox.CheckedChanged += new EventHandler(CheckBox_CheckedChanged);
            this.checkBox.FlatStyle = FlatStyle.System;
            this.checkBox.Text = string.IsNullOrEmpty(this.Description) ? this.DisplayName : this.Description;

            this.Controls.AddRange(
                new Control[]
                {
                    this.header,
                    this.checkBox
                });

            ResumeLayout(false);
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            int vSpacing = UI.ScaleHeight(4);

            this.header.Location = new Point(0, 0);
            this.header.Width = ClientSize.Width;
            this.header.Height = string.IsNullOrEmpty(this.header.Text) ? 0 : 
                this.header.GetPreferredSize(new Size(ClientSize.Width, 1)).Height;

            this.checkBox.Location = new Point(0, this.header.Bottom + vSpacing);

            LayoutUtility.PerformAutoLayout(
                this.checkBox,
                AutoSizeStrategy.ExpandHeightToContentAndKeepWidth, 
                EdgeSnapOptions.SnapLeftEdgeToContainerLeftEdge | 
                    EdgeSnapOptions.SnapRightEdgeToContainerRightEdge);

            ClientSize = new Size(ClientSize.Width, this.checkBox.Bottom);

            base.OnLayout(levent);
        }

        private void CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (Property.Value != this.checkBox.Checked)
            {
                Property.Value = this.checkBox.Checked;
            }
        }

        protected override void OnPropertyValueChanged()
        {
            this.checkBox.Checked = this.Property.Value;
        }

        protected override void OnPropertyReadOnlyChanged()
        {
            this.checkBox.Enabled = !Property.ReadOnly;
        }

        protected override bool OnFirstSelect()
        {
            this.checkBox.Select();
            return true;
        }
    }
}