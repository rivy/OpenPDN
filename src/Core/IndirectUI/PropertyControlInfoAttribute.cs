/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) Rick Brewster, Tom Jackson, and past contributors.            //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.Core;
using PaintDotNet.PropertySystem;
using System;

namespace PaintDotNet.IndirectUI
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    internal class PropertyControlInfoAttribute
        : Attribute
    {
        private Type propertyType;
        private PropertyControlType controlType;
        private bool isDefault;

        public Type PropertyType
        {
            get
            {
                return this.propertyType;
            }
        }

        public PropertyControlType ControlType
        {
            get
            {
                return this.controlType;
            }
        }

        public bool IsDefault
        {
            get
            {
                return this.isDefault;
            }

            set
            {
                this.isDefault = value;
            }
        }

        public PropertyControlInfoAttribute(Type propertyType, PropertyControlType controlType)
        {
            if (!typeof(Property).IsAssignableFrom(propertyType))
            {
                throw new ArgumentException("propertyType must be a type that derives from Property");
            }

            this.propertyType = propertyType;
            this.controlType = controlType;
        }
    }
}
