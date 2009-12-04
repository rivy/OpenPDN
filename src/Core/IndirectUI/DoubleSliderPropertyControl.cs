/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) Rick Brewster, Tom Jackson, and past contributors.            //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet;
using PaintDotNet.Core;
using PaintDotNet.PropertySystem;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaintDotNet.IndirectUI
{
    [PropertyControlInfo(typeof(DoubleProperty), PropertyControlType.Slider, IsDefault = true)]
    internal sealed class DoubleSliderPropertyControl
        : SliderPropertyControl<double>
    {
        private bool useExponentialScale = false;

        [PropertyControlProperty(DefaultValue = (object)2)]
        public new int DecimalPlaces
        {
            get
            {
                return base.DecimalPlaces;
            }

            set
            {
                base.DecimalPlaces = value;
            }
        }

        [PropertyControlProperty(DefaultValue = (object)false)]
        public bool UseExponentialScale
        {
            get
            {
                return this.useExponentialScale;
            }

            set
            {
                this.useExponentialScale = value;
                base.SliderShowTickMarks &= value;
                ResetUIRanges();
            }
        }

        [PropertyControlProperty(DefaultValue = (object)1.0)]
        public new double SliderSmallChange
        {
            get
            {
                return base.SliderSmallChange;
            }

            set
            {
                base.SliderSmallChange = value;
            }
        }

        [PropertyControlProperty(DefaultValue = (object)5.0)]
        public new double SliderLargeChange
        {
            get
            {
                return base.SliderLargeChange;
            }

            set
            {
                base.SliderLargeChange = value;
            }
        }

        [PropertyControlProperty(DefaultValue = (object)1.0)]
        public new double UpDownIncrement
        {
            get
            {
                return base.UpDownIncrement;
            }

            set
            {
                base.UpDownIncrement = value;
            }
        }

        public DoubleSliderPropertyControl(PropertyControlInfo propInfo)
            : base(propInfo)
        {
            SuspendLayout();
            DecimalPlaces = (int)propInfo.ControlProperties[ControlInfoPropertyNames.DecimalPlaces].Value;
            UseExponentialScale = (bool)propInfo.ControlProperties[ControlInfoPropertyNames.UseExponentialScale].Value;
            SliderSmallChange = (double)propInfo.ControlProperties[ControlInfoPropertyNames.SliderSmallChange].Value;
            SliderLargeChange = (double)propInfo.ControlProperties[ControlInfoPropertyNames.SliderLargeChange].Value;
            UpDownIncrement = (double)propInfo.ControlProperties[ControlInfoPropertyNames.UpDownIncrement].Value;
            ResumeLayout(false);
        }
        
        protected override int ToSliderValue(double propertyValue)
        {
            if (this.useExponentialScale)
            {
                return PropertyControlUtil.ToSliderValueExp(propertyValue, Property.MinValue, Property.MaxValue, DecimalPlaces);
            }
            else
            {
                double toIntScale = Math.Pow(10, DecimalPlaces);
                double sliderValueDouble = propertyValue * toIntScale;
                int sliderValueInt = (int)sliderValueDouble;
                return sliderValueInt;
            }
        }

        protected override double FromSliderValue(int sliderValue)
        {
            if (this.useExponentialScale)
            {
                return PropertyControlUtil.FromSliderValueExp(sliderValue, Property.MinValue, Property.MaxValue, DecimalPlaces);
            }
            else
            {
                double fromIntScale = Math.Pow(10, -DecimalPlaces);
                double valueDouble = (double)sliderValue * fromIntScale;
                return valueDouble;
            }
        }

        protected override decimal ToNudValue(double propertyValue)
        {
            return (decimal)propertyValue;
        }

        protected override double FromNudValue(decimal nudValue)
        {
            return (double)nudValue;
        }
    }
}
