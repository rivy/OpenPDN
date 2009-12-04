/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.PropertySystem;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaintDotNet
{
    [Serializable]
    public sealed class PropertyBasedSaveConfigToken
        : SaveConfigToken
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

        public PropertyBasedSaveConfigToken(PropertyCollection props)
        {
            this.properties = props.Clone();
        }

        private PropertyBasedSaveConfigToken(PropertyBasedSaveConfigToken copyMe)
            : base(copyMe)
        {
            this.properties = copyMe.properties.Clone();
        }

        public override object Clone()
        {
            return new PropertyBasedSaveConfigToken(this);
        }
    }

}
