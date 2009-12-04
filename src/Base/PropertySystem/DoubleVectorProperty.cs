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
    public sealed class DoubleVectorProperty
        : VectorProperty<double>
    {
        public DoubleVectorProperty(object name)
            : this(name, Pair.Create(0.0, 0.0))
        {
        }

        public DoubleVectorProperty(object name, Pair<double, double> defaultValues)
            : this(name, defaultValues, Pair.Create(double.MinValue, double.MinValue), Pair.Create(double.MaxValue, double.MaxValue))
        {
        }

        public DoubleVectorProperty(object name, Pair<double, double> defaultValues, Pair<double, double> minValues, Pair<double, double> maxValues)
            : this(name, defaultValues, minValues, maxValues, false)
        {
        }

        public DoubleVectorProperty(object name, Pair<double, double> defaultValues, Pair<double, double> minValues, Pair<double, double> maxValues, bool readOnly)
            : this(name, defaultValues, minValues, maxValues, readOnly, DefaultValueValidationFailureResult)
        {
        }

        public DoubleVectorProperty(object name, Pair<double, double> defaultValues, Pair<double, double> minValues, Pair<double, double> maxValues, bool readOnly, ValueValidationFailureResult vvfResult)
            : base(name, defaultValues, minValues, maxValues, readOnly, vvfResult)
        {
        }

        private DoubleVectorProperty(DoubleVectorProperty cloneMe, DoubleVectorProperty sentinelNotUsed)
            : base(cloneMe, sentinelNotUsed)
        {
        }

        public override Property Clone()
        {
            return new DoubleVectorProperty(this, this);
        }
    }
}
