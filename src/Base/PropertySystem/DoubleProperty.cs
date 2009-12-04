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
    public sealed class DoubleProperty
        : ScalarProperty<double>
    {
        public DoubleProperty(object name)
            : this(name, 0)
        {
        }

        public DoubleProperty(object name, double defaultValue)
            : this(name, defaultValue, double.MinValue, double.MaxValue)
        {
        }

        public DoubleProperty(object name, double defaultValue, double minValue, double maxValue)
            : this(name, defaultValue, minValue, maxValue, false)
        {
        }

        public DoubleProperty(object name, double defaultValue, double minValue, double maxValue, bool readOnly)
            : this(name, defaultValue, minValue, maxValue, readOnly, DefaultValueValidationFailureResult)
        {
        }

        public DoubleProperty(object name, double defaultValue, double minValue, double maxValue, bool readOnly, ValueValidationFailureResult vvfResult)
            : base(name, defaultValue, minValue, maxValue, readOnly, vvfResult)
        {
        }

        private DoubleProperty(DoubleProperty copyMe, DoubleProperty sentinelNotUsed)
            : base(copyMe, sentinelNotUsed)
        {
        }

        public override Property Clone()
        {
            return new DoubleProperty(this, this);
        }
    }
}