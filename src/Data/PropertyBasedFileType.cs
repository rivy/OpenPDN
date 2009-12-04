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
using System.Collections.Generic;
using System.IO;

namespace PaintDotNet
{
    public abstract class PropertyBasedFileType
        : FileType<PropertyBasedSaveConfigToken, PropertyBasedSaveConfigWidget>
    {
        public abstract PropertyCollection OnCreateSavePropertyCollection();

        public PropertyCollection CreateSavePropertyCollection()
        {
            PropertyCollection props = OnCreateSavePropertyCollection();

            // Perform any necessary validation here. Right now there's nothing special to do.

            return props.Clone();
        }

        public static ControlInfo CreateDefaultSaveConfigUI(PropertyCollection props)
        {
            PanelControlInfo configUI = new PanelControlInfo();

            foreach (Property property in props)
            {
                PropertyControlInfo propertyControlInfo = PropertyControlInfo.CreateFor(property);

                foreach (Property controlProperty in propertyControlInfo.ControlProperties)
                {
                    if (0 == string.Compare(controlProperty.Name, ControlInfoPropertyNames.DisplayName.ToString(), StringComparison.InvariantCulture))
                    {
                        controlProperty.Value = property.Name;
                    }
                    else if (0 == string.Compare(controlProperty.Name, ControlInfoPropertyNames.ShowResetButton.ToString(), StringComparison.InvariantCulture))
                    {
                        controlProperty.Value = false;
                    }
                }

                configUI.AddChildControl(propertyControlInfo);
            }

            return configUI;
        }

        public virtual ControlInfo OnCreateSaveConfigUI(PropertyCollection props)
        {
            return CreateDefaultSaveConfigUI(props);
        }

        public ControlInfo CreateSaveConfigUI(PropertyCollection props)
        {
            ControlInfo configUI = OnCreateSaveConfigUI(props);

            // Perform any necessary validation here. Right now there's nothing special to do.

            return configUI;
        }

        protected override sealed PropertyBasedSaveConfigToken OnCreateDefaultSaveConfigTokenT()
        {
            PropertyCollection props = CreateSavePropertyCollection();
            PropertyCollection props1 = props.Clone();

            PropertyBasedSaveConfigToken token = new PropertyBasedSaveConfigToken(props1);

            return token;
        }

        protected override sealed PropertyBasedSaveConfigWidget OnCreateSaveConfigWidgetT()
        {
            PropertyCollection props1 = CreateSavePropertyCollection();
            PropertyCollection props2 = props1.Clone();
            PropertyCollection props3 = props1.Clone();

            ControlInfo configUI1 = CreateSaveConfigUI(props2);
            ControlInfo configUI2 = configUI1.Clone();

            PropertyBasedSaveConfigWidget widget = new PropertyBasedSaveConfigWidget(this, props3, configUI2);

            return widget;
        }

        protected override PropertyBasedSaveConfigToken GetSaveConfigTokenFromSerializablePortionT(object portion)
        {
            PropertyCollection props1 = CreateSavePropertyCollection();
            PropertyCollection props2 = props1.Clone();

            Pair<string, object>[] nameValues = (Pair<string, object>[])portion;

            foreach (Pair<string, object> nameValue in nameValues)
            {
                Property property = props2[nameValue.First];

                if (property.ReadOnly)
                {
                    property.ReadOnly = false;
                    property.Value = nameValue.Second;
                    property.ReadOnly = true;
                }
                else
                {
                    property.Value = nameValue.Second;
                }
            }

            PropertyBasedSaveConfigToken newToken = CreateDefaultSaveConfigToken();

            newToken.Properties.CopyCompatibleValuesFrom(props2, true);

            return newToken;
        }

        protected override object GetSerializablePortionOfSaveConfigToken(PropertyBasedSaveConfigToken token)
        {
            // We do not want to save the schema, just the [name, value] pairs
            int propCount = token.Properties.Count;

            Pair<string, object>[] nameValues = new Pair<string, object>[propCount];

            int index = 0;
            foreach (string propertyName in token.PropertyNames)
            {
                Property property = token.GetProperty(propertyName);
                object value = property.Value;
                Pair<string, object> nameValue = Pair.Create(propertyName, value);
                nameValues[index] = nameValue;
                ++index;
            }

            return nameValues;
        }

        public PropertyBasedFileType(string name, FileTypeFlags flags, string[] extensions)
            : base(name, flags, extensions)
        {
        }
    }
}
