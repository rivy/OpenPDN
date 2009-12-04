/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) Rick Brewster, Tom Jackson, and past contributors.            //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.PropertySystem;
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace PaintDotNet.IndirectUI
{
    internal abstract class PropertyControl
        : UserControl,
          IPropertyRef,
          IFirstSelection
    {
        private Property property;
        private string displayName;
        private string description;
        private ToolTip toolTip;

        public Property Property
        {
            get
            {
                return this.property;
            }
        }

        protected ToolTip ToolTip
        {
            get
            {
                if (this.toolTip == null)
                {
                    this.toolTip = new ToolTip();
                }

                return this.toolTip;
            }
        }

        protected virtual void OnDisplayNameChanged()
        {
        }

        [PropertyControlProperty(DefaultValue = "")]
        public string DisplayName
        {
            get
            {
                return this.displayName;
            }

            set
            {
                if (this.displayName != value)
                {
                    this.displayName = value;
                    OnDisplayNameChanged();
                    PerformLayout();
                }
            }
        }

        protected virtual void OnDescriptionChanged()
        {
        }

        [PropertyControlProperty(DefaultValue = "")]
        public string Description
        {
            get
            {
                return this.description;
            }

            set
            {
                this.description = value;
                OnDescriptionChanged();
                PerformLayout();
            }
        }

        internal PropertyControl(PropertyControlInfo propInfo)
        {
            this.property = propInfo.Property;
            this.property.ValueChanged += new EventHandler(Property_ValueChanged);
            this.property.ReadOnlyChanged += new EventHandler(Property_ReadOnlyChanged);
            this.displayName = (string)propInfo.ControlProperties[ControlInfoPropertyNames.DisplayName].Value;
            this.description = (string)propInfo.ControlProperties[ControlInfoPropertyNames.Description].Value;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.toolTip != null)
                {
                    this.toolTip.Dispose();
                    this.toolTip = null;
                }
            }

            base.Dispose(disposing);
        }

        protected override void OnLoad(EventArgs e)
        {
            OnPropertyValueChanged();
            OnPropertyReadOnlyChanged();
            base.OnLoad(e);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
        }

        protected abstract void OnPropertyReadOnlyChanged();

        private void Property_ReadOnlyChanged(object sender, EventArgs e)
        {
            OnPropertyReadOnlyChanged();
        }

        protected abstract void OnPropertyValueChanged();

        private void Property_ValueChanged(object sender, EventArgs e)
        {
            OnPropertyValueChanged();
        }

        protected abstract bool OnFirstSelect();

        bool IFirstSelection.FirstSelect()
        {
            return OnFirstSelect();
        }
    }
}
