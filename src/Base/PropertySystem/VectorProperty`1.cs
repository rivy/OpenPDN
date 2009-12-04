/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) Rick Brewster, Tom Jackson, and past contributors.            //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;

namespace PaintDotNet.PropertySystem
{
    public abstract class VectorProperty<T>
        : Property<Pair<T, T>>
          where T : struct, IComparable<T>
    {
        private Pair<T, T> minValues;
        private Pair<T, T> maxValues;

        public Pair<T, T> MinValues
        {
            get
            {
                return this.minValues;
            }
        }

        public Pair<T, T> MaxValues
        {
            get
            {
                return this.maxValues;
            }
        }

        public T MinValueX
        {
            get
            {
                return MinValues.First;
            }
        }

        public T MaxValueX
        {
            get
            {
                return MaxValues.First;
            }
        }

        public T MinValueY
        {
            get
            {
                return MinValues.Second;
            }
        }

        public T MaxValueY
        {
            get
            {
                return MaxValues.Second;
            }
        }

        public T DefaultValueX
        {
            get
            {
                return DefaultValue.First;
            }
        }

        public T DefaultValueY
        {
            get
            {
                return DefaultValue.Second;
            }
        }

        public T ValueX
        {
            get
            {
                return Value.First;
            }

            set
            {
                Value = Pair.Create(value, ValueY);
            }
        }

        public T ValueY
        {
            get
            {
                return Value.Second;
            }

            set
            {
                Value = Pair.Create(ValueX, value);
            }
        }

        public bool IsEqualTo(VectorProperty<T> rhs)
        {
            return IsEqualTo(this, rhs);
        }

        public static bool IsEqualTo(VectorProperty<T> lhs, VectorProperty<T> rhs)
        {
            return IsEqualTo(lhs.Value, rhs.Value);
        }

        public static bool IsEqualTo(Pair<T, T> lhs, Pair<T, T> rhs)
        {
            return (lhs.First.CompareTo(rhs.First) == 0) && (lhs.Second.CompareTo(rhs.Second) == 0);
        }

        internal VectorProperty(object name, Pair<T, T> defaultValues, Pair<T, T> minValues, Pair<T, T> maxValues)
            : this(name, defaultValues, minValues, maxValues, false)
        {
        }

        internal VectorProperty(object name, Pair<T, T> defaultValues, Pair<T, T> minValues, Pair<T, T> maxValues, bool readOnly)
            : this(name, defaultValues, minValues, maxValues, readOnly, DefaultValueValidationFailureResult)
        {
        }

        internal VectorProperty(object name, Pair<T, T> defaultValues, Pair<T, T> minValues, Pair<T, T> maxValues, bool readOnly, ValueValidationFailureResult vvfResult)
            : base(name, defaultValues, readOnly, vvfResult)
        {
            this.minValues = minValues;
            this.maxValues = maxValues;
        }

        internal VectorProperty(VectorProperty<T> cloneMe, VectorProperty<T> sentinelNotUsed)
            : base(cloneMe, sentinelNotUsed)
        {
            this.minValues = cloneMe.minValues;
            this.maxValues = cloneMe.maxValues;
        }

        public T ClampPotentialValueX(T newValue)
        {
            return ScalarProperty<T>.Clamp(newValue, this.MinValueX, this.MaxValueX);
        }

        public T ClampPotentialValueY(T newValue)
        {
            return ScalarProperty<T>.Clamp(newValue, this.MinValueY, this.MaxValueY);
        }

        public Pair<T, T> ClampPotentialValue(Pair<T, T> newValue)
        {
            return Pair.Create(ClampPotentialValueX(newValue.First), ClampPotentialValueY(newValue.Second));
        }

        protected override Pair<T, T> OnClampNewValueT(Pair<T, T> newValue)
        {
            return ClampPotentialValue(newValue);
        }
    }
}
