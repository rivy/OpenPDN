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
    public sealed class Int32Property
        : ScalarProperty<int>
    {
        public Int32Property(object name)
            : this(name, 0)
        {
        }

        public Int32Property(object name, int defaultValue)
            : this(name, defaultValue, int.MinValue, int.MaxValue)
        {
        }

        public Int32Property(object name, int defaultValue, int minValue, int maxValue)
            : this(name, defaultValue, minValue, maxValue, false)
        {
        }

        public Int32Property(object name, int defaultValue, int minValue, int maxValue, bool readOnly)
            : this(name, defaultValue, minValue, maxValue, readOnly, DefaultValueValidationFailureResult)
        {
        }

        public Int32Property(object name, int defaultValue, int minValue, int maxValue, bool readOnly, ValueValidationFailureResult vvfResult)
            : base(name, defaultValue, minValue, maxValue, readOnly, vvfResult)
        {
        }

        private Int32Property(Int32Property copyMe, Int32Property sentinelNotUsed)
            : base(copyMe, sentinelNotUsed)
        {
        }

        protected override int OnCoerceValueT(object newValue)
        {
            if (newValue is double)
            {
                return (int)(double)newValue;
            }
            else if (newValue is float)
            {
                return (int)(float)newValue;
            }

            return base.OnCoerceValueT(newValue);
        }

        public override Property Clone()
        {
            return new Int32Property(this, this);
        }
    }
}