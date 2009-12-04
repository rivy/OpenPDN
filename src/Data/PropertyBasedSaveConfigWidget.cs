/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.IndirectUI;
using PaintDotNet.PropertySystem;
using System;
using System.Windows.Forms;

namespace PaintDotNet
{
    public sealed class PropertyBasedSaveConfigWidget
        : SaveConfigWidget<PropertyBasedFileType, PropertyBasedSaveConfigToken>
    {
        private PropertyCollection originalProps;
        private ControlInfo configUI;
        private Control configUIControl;

        protected override void InitWidgetFromToken(PropertyBasedSaveConfigToken sourceToken)
        {
            foreach (string propertyName in sourceToken.PropertyNames)
            {
                Property srcProperty = sourceToken.GetProperty(propertyName);
                PropertyControlInfo dstPropertyControlInfo = this.configUI.FindControlForPropertyName(propertyName);

                if (dstPropertyControlInfo != null)
                {
                    Property dstProperty = dstPropertyControlInfo.Property;

                    if (dstProperty.ReadOnly)
                    {
                        dstProperty.ReadOnly = false;
                        dstProperty.Value = srcProperty.Value;
                        dstProperty.ReadOnly = true;
                    }
                    else
                    {
                        dstProperty.Value = srcProperty.Value;
                    }
                }
            }
        }

        protected override PropertyBasedSaveConfigToken CreateTokenFromWidget()
        {
            PropertyCollection props = this.originalProps.Clone();

            foreach (string propertyName in props.PropertyNames)
            {
                PropertyControlInfo srcPropertyControlInfo = this.configUI.FindControlForPropertyName(propertyName);

                if (srcPropertyControlInfo != null)
                {
                    Property srcProperty = srcPropertyControlInfo.Property;
                    Property dstProperty = props[propertyName];

                    if (dstProperty.ReadOnly)
                    {
                        dstProperty.ReadOnly = false;
                        dstProperty.Value = srcProperty.Value;
                        dstProperty.ReadOnly = true;
                    }
                    else
                    {
                        dstProperty.Value = srcProperty.Value;
                    }
                }
            }

            PropertyBasedSaveConfigToken pbsct = new PropertyBasedSaveConfigToken(props);

            return pbsct;
        }

        public PropertyBasedSaveConfigWidget(PropertyBasedFileType fileType, PropertyCollection props, ControlInfo configUI)
            : base(fileType)
        {
            this.originalProps = props.Clone();
            this.configUI = configUI.Clone();

            // Make sure that the properties in props and configUI are not the same objects
            foreach (Property property in props)
            {
                PropertyControlInfo pci = this.configUI.FindControlForPropertyName(property.Name);

                if (pci != null)
                {
                    if (object.ReferenceEquals(property, pci.Property))
                    {
                        throw new ArgumentException("Property references in propertyCollection must not be the same as those in configUI");
                    }
                }
            }

            SuspendLayout();

            this.configUIControl = (Control)this.configUI.CreateConcreteControl(this);
            this.configUIControl.SuspendLayout();

            this.configUIControl.TabIndex = 0;

            // Set up data binding
            foreach (Property property in this.originalProps)
            {
                PropertyControlInfo pci = this.configUI.FindControlForPropertyName(property.Name);

                if (pci == null)
                {
                    throw new InvalidOperationException("Every property must have a control associated with it");
                }
                else
                {
                    Property controlsProperty = pci.Property;

                    // ASSUMPTION: We assume that the concrete WinForms Control holds a reference to
                    //             the same Property instance as the ControlInfo it was created from.

                    controlsProperty.ValueChanged += ControlsProperty_ValueChanged;
                }
            }

            this.Controls.Add(this.configUIControl);

            this.configUIControl.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            this.configUIControl.Width = this.ClientSize.Width;
            this.configUIControl.PerformLayout();
            this.ClientSize = this.configUIControl.Size;
            base.OnLayout(e);
        }

        private void ControlsProperty_ValueChanged(object sender, EventArgs e)
        {
            UpdateToken();
        }
    }
}
