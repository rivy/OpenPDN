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
using System.Text;

namespace PaintDotNet.PropertySystem
{
    public sealed class StringProperty
        : Property<string>
    {
        private int maxLength;

        public static int MaxMaxLength
        {
            get
            {
                return 32767;
            }
        }

        public int MaxLength
        {
            get
            {
                return this.maxLength;
            }
        }

        public StringProperty(object name)
            : this(name, string.Empty)
        {
        }

        public StringProperty(object name, string defaultValue)
            : this(name, defaultValue, MaxMaxLength)
        {
        }

        public StringProperty(object name, string defaultValue, int maxLength)
            : this(name, defaultValue, maxLength, false)
        {
        }

        public StringProperty(object name, string defaultValue, int maxLength, bool readOnly)
            : this(name, defaultValue, maxLength, readOnly, DefaultValueValidationFailureResult)
        {
        }

        public StringProperty(object name, string defaultValue, int maxLength, bool readOnly, ValueValidationFailureResult vvfResult)
            : base(name, defaultValue, readOnly, vvfResult)
        {
            if (defaultValue == null)
            {
                throw new ArgumentNullException("defaultValue must not be null");
            }

            if (maxLength < 0 || maxLength > MaxMaxLength)
            {
                throw new ArgumentOutOfRangeException(string.Format(
                    "maxLength was {0} but it must be greater than 0, and less than StringProperty.MaxMaxLength",
                    maxLength));
            }

            this.maxLength = maxLength;
        }

        private StringProperty(StringProperty cloneMe, StringProperty sentinelNotUsed)
            : base(cloneMe, sentinelNotUsed)
        {
            this.maxLength = cloneMe.maxLength;
        }

        public override Property Clone()
        {
            return new StringProperty(this, this);
        }

        protected override string PropertyValueToStringT(string value)
        {
            return value;
        }

        protected override bool ValidateNewValueT(string newValue)
        {
            return (newValue.Length <= this.maxLength);
        }

        protected override string OnClampNewValueT(string newValue)
        {
            string clampedValue = newValue;

            if (clampedValue.Length > this.MaxLength)
            {
                clampedValue = clampedValue.Substring(this.MaxLength);
            }

            return clampedValue;
        }
    }
}
