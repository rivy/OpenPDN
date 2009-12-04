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
    public abstract class Property<T>
        : Property
    {
        public new T Value
        {
            get
            {
                return (T)base.Value;
            }

            set
            {
                base.Value = value;
            }
        }

        public new T DefaultValue
        {
            get
            {
                return (T)base.DefaultValue;
            }
        }

        internal Property(object name, T defaultValue, bool readOnly, ValueValidationFailureResult vvfResult)
            : base(name, typeof(T), defaultValue, readOnly, vvfResult)
        {
        }

        internal Property(Property<T> cloneMe, Property<T> sentinelNotUsed)
            : base(cloneMe, sentinelNotUsed)
        {
            // sentinelNotUsed is just there so that this constructor can be unambiguous from Property<T>(object)
        }

        protected virtual T OnCoerceValueT(object newValue)
        {
            return (T)newValue;
        }

        protected override sealed object OnCoerceValue(object newValue)
        {
            return OnCoerceValueT(newValue);
        }

        protected override sealed object OnClampNewValue(object newValue)
        {
            return OnClampNewValueT((T)newValue);
        }

        protected abstract T OnClampNewValueT(T newValue);

        protected virtual bool ValidateNewValueT(T newValue)
        {
            return true;
        }

        protected override sealed bool ValidateNewValue(object newValue)
        {
            T newValueT = (T)newValue;
            return ValidateNewValueT(newValueT);
        }

        protected virtual string PropertyValueToStringT(T value)
        {
            return value.ToString();
        }

        protected override sealed string PropertyValueToString(object value)
        {
            return base.PropertyValueToString(value);
        }
    }
}
