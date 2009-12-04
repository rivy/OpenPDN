/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) Rick Brewster, Tom Jackson, and past contributors.            //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.Core;
using PaintDotNet.IndirectUI;
using PaintDotNet.PropertySystem;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace PaintDotNet.Effects
{
    public abstract class PropertyBasedEffect
        : Effect<PropertyBasedEffectConfigToken>
    {
        protected abstract PropertyCollection OnCreatePropertyCollection();

        public PropertyCollection CreatePropertyCollection()
        {
            return OnCreatePropertyCollection();
        }

        protected virtual ControlInfo OnCreateConfigUI(PropertyCollection props)
        {
            return CreateDefaultConfigUI(props);
        }

        public ControlInfo CreateConfigUI(PropertyCollection props)
        {
            PropertyCollection props2 = props.Clone();

            using (props2.__Internal_BeginEventAddMoratorium())
            {
                ControlInfo configUI1 = OnCreateConfigUI(props2);
                ControlInfo configUI2 = configUI1.Clone();
                return configUI2;
            }
        }

        public static ControlInfo CreateDefaultConfigUI(IEnumerable<Property> props)
        {
            PanelControlInfo configUI = new PanelControlInfo();

            foreach (Property property in props)
            {
                PropertyControlInfo propertyControlInfo = PropertyControlInfo.CreateFor(property);
                propertyControlInfo.ControlProperties[ControlInfoPropertyNames.DisplayName].Value = property.Name;
                configUI.AddChildControl(propertyControlInfo);
            }

            return configUI;
        }

        private string GetConfigDialogTitle()
        {
            return this.Name;
        }

        private Icon GetConfigDialogIcon()
        {
            Image image = this.Image;

            Icon icon = null;

            if (image != null)
            {
                icon = Utility.ImageToIcon(image);
            }

            return icon;
        }

        protected virtual void OnCustomizeConfigUIWindowProperties(PropertyCollection props)
        {
            return;
        }

        public override sealed EffectConfigDialog CreateConfigDialog()
        {
            PropertyCollection props1 = CreatePropertyCollection();
            PropertyCollection props2 = props1.Clone();
            PropertyCollection props3 = props1.Clone();

            ControlInfo configUI1 = CreateConfigUI(props2);
            ControlInfo configUI2 = configUI1.Clone();

            PropertyCollection windowProps = PropertyBasedEffectConfigDialog.CreateWindowProperties();
            windowProps[ControlInfoPropertyNames.WindowTitle].Value = this.Name;
            OnCustomizeConfigUIWindowProperties(windowProps);
            PropertyCollection windowProps2 = windowProps.Clone();

            PropertyBasedEffectConfigDialog pbecd = new PropertyBasedEffectConfigDialog(props3, configUI2, windowProps2);

            pbecd.Icon = GetConfigDialogIcon();

            return pbecd;
        }

        protected PropertyBasedEffect(string name, Image image, string subMenuName, EffectFlags flags)
            : base(name, image, subMenuName, flags)
        {
        }
    }
}
