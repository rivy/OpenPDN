/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) Rick Brewster, Tom Jackson, and past contributors.            //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PaintDotNet.PropertySystem
{
    public class StaticListChoiceProperty
        : Property<object>
    {
        private object[] valueChoices;

        public object[] ValueChoices
        {
            get
            {
                return (object[])this.valueChoices.Clone();
            }
        }

        public StaticListChoiceProperty(object name, object[] valueChoices)
            : this(name, valueChoices, 0)
        {
        }

        public StaticListChoiceProperty(object name, object[] valueChoices, int defaultChoiceIndex)
            : this(name, valueChoices, defaultChoiceIndex, false)
        {
        }

        public StaticListChoiceProperty(object name, object[] valueChoices, int defaultChoiceIndex, bool readOnly)
            : this(name, valueChoices, defaultChoiceIndex, readOnly, DefaultValueValidationFailureResult)
        {
        }

        public StaticListChoiceProperty(object name, object[] valueChoices, int defaultChoiceIndex, bool readOnly, ValueValidationFailureResult vvfResult)
            : base(name, valueChoices[defaultChoiceIndex], readOnly, vvfResult)
        {
            if (defaultChoiceIndex < 0 || defaultChoiceIndex >= valueChoices.Length)
            {
                throw new ArgumentOutOfRangeException("defaultChoiceIndex", "must be in the range [0, valueChoices.Length) (actual value: " + defaultChoiceIndex.ToString() + ")");
            }

            this.valueChoices = (object[])valueChoices.Clone();
        }

        protected StaticListChoiceProperty(StaticListChoiceProperty cloneMe, StaticListChoiceProperty sentinelNotUsed)
            : base(cloneMe, sentinelNotUsed)
        {
            this.valueChoices = (object[])cloneMe.valueChoices.Clone();
        }

        public override Property Clone()
        {
            return new StaticListChoiceProperty(this, this);
        }

        protected override bool ValidateNewValueT(object newValue)
        {
            int index = Array.IndexOf(this.valueChoices, newValue);
            return (index != -1);
        }

        public static StaticListChoiceProperty CreateForEnum<TEnum>(object name, TEnum defaultValue, bool readOnly)
            where TEnum : struct
        {
            return CreateForEnum(typeof(TEnum), name, defaultValue, readOnly);
        }

        public static StaticListChoiceProperty CreateForEnum(Type enumType, object name, object defaultValue, bool readOnly)
        {
            // [Flags] enums aren't currently supported
            object[] flagsAttributes = enumType.GetCustomAttributes(typeof(FlagsAttribute), true);

            if (flagsAttributes.Length > 0)
            {
                throw new ArgumentOutOfRangeException("Enums with [Flags] are not currently supported");
            }

            Array enumChoices = Enum.GetValues(enumType);
            int defaultChoiceIndex = Array.IndexOf(enumChoices, defaultValue);

            if (defaultChoiceIndex == -1)
            {
                throw new ArgumentOutOfRangeException(string.Format(
                    "defaultValue ({0}) is not a valid enum value for {1}",
                    defaultValue.ToString(),
                    enumType.FullName));
            }

            object[] enumChoicesObj = new object[enumChoices.Length];
            enumChoices.CopyTo(enumChoicesObj, 0);

            StaticListChoiceProperty enumProperty = new StaticListChoiceProperty(name, enumChoicesObj, defaultChoiceIndex, readOnly);
            return enumProperty;
        }

        protected override object OnClampNewValueT(object newValue)
        {
            object clampedValue = newValue;
            int newValueIndex = Array.IndexOf(this.valueChoices, newValue);

            if (newValueIndex == -1)
            {
                clampedValue = this.DefaultValue;
            }

            return clampedValue;
        }
    }
}
