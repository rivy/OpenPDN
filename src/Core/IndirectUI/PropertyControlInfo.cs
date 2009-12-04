/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) Rick Brewster, Tom Jackson, and past contributors.            //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.PropertySystem;
using PaintDotNet.SystemLayer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;

namespace PaintDotNet.IndirectUI
{
    public sealed class PropertyControlInfo
        : ControlInfo,
          IPropertyRef
    {
        // NOTE: In these descriptions, do not confuse Property.GetType() with Property.ValueType.

        // Maps from Property.GetType() to set of PropertyControlType
        private static Dictionary<Type, Set<PropertyControlType>> propertyTypeToControlType;

        // Maps from Property.GetType() to the default PropertyControlType (some property types get more than 1)
        private static Dictionary<Type, PropertyControlType> propertyTypeToDefaultControlType;

        // Maps from {Property.GetType(), PropertyControlType} to PropertyControl.GetType()
        private static Dictionary<Pair<Type, PropertyControlType>, Type> controlTypeToPropertyControlType;

        // Maps from {Property.GetType(), PropertyControlType} to its PropertyControlInfo properties
        private static Dictionary<Pair<Type, PropertyControlType>, PropertyCollection> controlTypeToProperties;

        static PropertyControlInfo()
        {
            Tracing.Enter();

            try
            {
                BuildStaticMaps();
            }

            finally
            {
                Tracing.Leave();
            }
        }

        private static void BuildStaticMaps()
        {
            propertyTypeToControlType = new Dictionary<Type,Set<PropertyControlType>>();
            propertyTypeToDefaultControlType = new Dictionary<Type, PropertyControlType>();
            controlTypeToPropertyControlType = new Dictionary<Pair<Type,PropertyControlType>,Type>();
            controlTypeToProperties = new Dictionary<Pair<Type, PropertyControlType>, PropertyCollection>();

            Assembly thisAssembly = Assembly.GetExecutingAssembly();
            Type[] types = thisAssembly.GetTypes();

            foreach (Type propertyControlType in types)
            {
                if (propertyControlType.IsAbstract)
                {
                    continue;
                }                

                if (!propertyControlType.IsSubclassOf(typeof(PropertyControl)))
                {
                    continue;
                }

                object[] infoAttributes = propertyControlType.GetCustomAttributes(typeof(PropertyControlInfoAttribute), false);
                PropertyControlInfoAttribute infoAttribute;

                if (infoAttributes.Length == 1)
                {
                    infoAttribute = (PropertyControlInfoAttribute)infoAttributes[0];

                    if (!propertyTypeToControlType.ContainsKey(infoAttribute.PropertyType))
                    {
                        propertyTypeToControlType.Add(infoAttribute.PropertyType, new Set<PropertyControlType>());
                    }

                    propertyTypeToControlType[infoAttribute.PropertyType].Add(infoAttribute.ControlType);
                    controlTypeToPropertyControlType[Pair.Create(infoAttribute.PropertyType, infoAttribute.ControlType)] = propertyControlType;

                    if (!propertyTypeToDefaultControlType.ContainsKey(infoAttribute.PropertyType) ||
                        infoAttribute.IsDefault)
                    {
                        propertyTypeToDefaultControlType[infoAttribute.PropertyType] = infoAttribute.ControlType;
                    }

                    List<Property> controlProps = new List<Property>();
                    PropertyInfo[] props = propertyControlType.GetProperties();

                    foreach (PropertyInfo prop in props)
                    {
                        object[] propAttributes = prop.GetCustomAttributes(typeof(PropertyControlPropertyAttribute), true);

                        if (propAttributes.Length == 1)
                        {
                            string propName = prop.Name;
                            Type propType = prop.PropertyType;
                            object propDefault = ((PropertyControlPropertyAttribute)propAttributes[0]).DefaultValue;
                            Property propProp = Property.Create(propType, propName, propDefault);
                            controlProps.Add(propProp);
                        }
                    }

                    PropertyCollection controlProps2 = new PropertyCollection(controlProps);
                    controlTypeToProperties[Pair.Create(infoAttribute.PropertyType, infoAttribute.ControlType)] = controlProps2;
                }
            }
        }

        private Property property;
        private StaticListChoiceProperty controlType;
        private Dictionary<object, string> valueDisplayNames = new Dictionary<object, string>();

        public Property Property
        {
            get
            {
                return this.property;
            }
        }

        public StaticListChoiceProperty ControlType
        {
            get
            {
                return this.controlType;
            }
        }

        public void SetValueDisplayName(object value, string displayName)
        {
            this.valueDisplayNames[value] = displayName;
        }

        public string GetValueDisplayName(object value)
        {
            string valueDisplayName;
            this.valueDisplayNames.TryGetValue(value, out valueDisplayName);
            return valueDisplayName ?? value.ToString();
        }

        private PropertyControlInfo(Property property)
            : base()
        {
            this.property = property;
            PropertyControlType defaultControlType = propertyTypeToDefaultControlType[this.property.GetType()];
            this.controlType = StaticListChoiceProperty.CreateForEnum<PropertyControlType>(ControlInfoPropertyNames.ControlType, defaultControlType, false);
            this.controlType.ValueChanged += new EventHandler(ControlType_ValueChanged);
            this.ControlProperties = controlTypeToProperties[Pair.Create(property.GetType(), (PropertyControlType)this.controlType.Value)].Clone();
        }

        private PropertyControlInfo(PropertyControlInfo cloneMe)
            : base(cloneMe)
        {
            this.property = cloneMe.property;
            this.controlType = (StaticListChoiceProperty)cloneMe.controlType.Clone();
            this.valueDisplayNames = new Dictionary<object, string>(cloneMe.valueDisplayNames);
        }

        private void ControlType_ValueChanged(object sender, EventArgs e)
        {
            PropertyCollection newProps = controlTypeToProperties[Pair.Create(this.Property.GetType(), 
                (PropertyControlType)this.controlType.Value)].Clone();

            newProps.CopyCompatibleValuesFrom(this.ControlProperties);

            this.ControlProperties = newProps;
        }

        public static PropertyControlInfo CreateFor(Property property)
        {
            return new PropertyControlInfo(property);
        }

        internal override Control CreateWinFormsControl()
        {
            Type propertyControlType = controlTypeToPropertyControlType[Pair.Create(
                Property.GetType(), (PropertyControlType)this.controlType.Value)];

            PropertyControl propertyControl = (PropertyControl)Activator.CreateInstance(propertyControlType, this);

            return propertyControl;
        }

        public override ControlInfo Clone()
        {
            return new PropertyControlInfo(this);
        }
    }
}
