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
    public abstract class ScalarProperty<T>
        : Property<T>
          where T : struct, IComparable<T>
    {
        private T minValue;
        private T maxValue;

        public T MinValue
        {
            get
            {
                return this.minValue;
            }
        }

        public T MaxValue
        {
            get
            {
                return this.maxValue;
            }
        }

        public bool IsLessThan(ScalarProperty<T> rhs)
        {
            return IsLessThan(this, rhs);
        }

        public static bool IsLessThan(ScalarProperty<T> lhs, ScalarProperty<T> rhs)
        {
            return IsLessThan(lhs.Value, rhs.Value);
        }

        public static bool IsLessThan(T lhs, T rhs)
        {
            return lhs.CompareTo(rhs) < 0;
        }

        public bool IsGreaterThan(ScalarProperty<T> rhs)
        {
            return IsGreaterThan(this, rhs);
        }

        public static bool IsGreaterThan(ScalarProperty<T> lhs, ScalarProperty<T> rhs)
        {
            return IsGreaterThan(lhs.Value, rhs.Value);
        }

        public static bool IsGreaterThan(T lhs, T rhs)
        {
            return lhs.CompareTo(rhs) > 0;
        }

        public bool IsEqualTo(ScalarProperty<T> rhs)
        {
            return IsEqualTo(this, rhs);
        }

        public static bool IsEqualTo(ScalarProperty<T> lhs, ScalarProperty<T> rhs)
        {
            return IsEqualTo(lhs.Value, rhs.Value);
        }

        public static bool IsEqualTo(T lhs, T rhs)
        {
            return lhs.CompareTo(rhs) == 0;
        }

        public static T Clamp(T value, T min, T max)
        {
            T newValue = value;

            if (IsGreaterThan(min, max))
            {
                throw new ArgumentOutOfRangeException("min must be less than or equal to max");
            }

            if (IsGreaterThan(value, max))
            {
                newValue = max;
            }

            if (IsLessThan(value, min))
            {
                newValue = min;
            }

            return newValue;
        }

        public T ClampPotentialValue(T newValue)
        {
            return Clamp(newValue, this.minValue, this.maxValue);
        }

        internal ScalarProperty(object name, T defaultValue, T minValue, T maxValue)
            : this(name, defaultValue, minValue, maxValue, false)
        {
        }

        internal ScalarProperty(object name, T defaultValue, T minValue, T maxValue, bool readOnly)
            : this(name, defaultValue, minValue, maxValue, readOnly, DefaultValueValidationFailureResult)
        {
        }

        internal ScalarProperty(object name, T defaultValue, T minValue, T maxValue, bool readOnly, ValueValidationFailureResult vvfResult)
            : base(name, defaultValue, readOnly, vvfResult)
        {
            if (IsLessThan(maxValue, minValue))
            {
                throw new ArgumentOutOfRangeException("maxValue < minValue");
            }

            if (IsLessThan(defaultValue, minValue))
            {
                throw new ArgumentOutOfRangeException("defaultValue < minValue");
            }

            if (IsGreaterThan(defaultValue, maxValue))
            {
                throw new ArgumentOutOfRangeException("defaultValue > maxValue");
            }

            this.minValue = minValue;
            this.maxValue = maxValue;
        }

        internal ScalarProperty(ScalarProperty<T> copyMe, ScalarProperty<T> sentinelNotUsed)
            : base(copyMe, sentinelNotUsed)
        {
            this.minValue = copyMe.minValue;
            this.maxValue = copyMe.maxValue;
        }

        protected override T OnClampNewValueT(T newValue)
        {
            return ClampPotentialValue(newValue);
        }

        protected override bool ValidateNewValueT(T newValue)
        {
            if (IsLessThan(newValue, this.minValue))
            {
                return false;
            }

            if (IsGreaterThan(newValue, this.maxValue))
            {
                return false;
            }

            return base.ValidateNewValueT(newValue);
        }
    }
}
