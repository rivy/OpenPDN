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
using System.Text;

namespace PaintDotNet.Effects
{
    public sealed class PropertyBasedEffectConfigToken
        : EffectConfigToken
    {
        private PropertyCollection properties;

        public PropertyCollection Properties
        {
            get
            {
                return this.properties;
            }
        }

        public IEnumerable<string> PropertyNames
        {
            get
            {
                return this.properties.PropertyNames;
            }
        }

        public Property GetProperty(object propertyName)
        {
            return this.properties[propertyName];
        }

        public T GetProperty<T>(object propertyName)
            where T : Property
        {
            return (T)this.properties[propertyName];
        }

        public bool SetPropertyValue(object propertyName, object newValue)
        {
            try
            {
                Property property = this.properties[propertyName];
                property.Value = newValue;
            }

            catch (Exception ex)
            {
                if (ex is KeyNotFoundException ||
                    ex is ReadOnlyException)
                {
                    return false;
                }
                else
                {
                    throw;
                }
            }

            return true;
        }

        public PropertyBasedEffectConfigToken(PropertyCollection propertyCollection)
        {
            Initialize(propertyCollection);
        }

        private PropertyBasedEffectConfigToken(PropertyBasedEffectConfigToken cloneMe)
            : base(cloneMe)
        {
            Initialize(cloneMe.properties);
        }

        private void Initialize(PropertyCollection propertyCollection)
        {
            this.properties = propertyCollection.Clone();
        }

        public override object Clone()
        {
            return new PropertyBasedEffectConfigToken(this);
        }
    }
}
