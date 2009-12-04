/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) Rick Brewster, Tom Jackson, and past contributors.            //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet;
using PaintDotNet.PropertySystem;
using PaintDotNet.SystemLayer;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace PaintDotNet.IndirectUI
{
    [PropertyControlInfo(typeof(DoubleVectorProperty), PropertyControlType.Slider)]
    internal sealed class DoubleVectorSliderPropertyControl
        : VectorSliderPropertyControl<double>
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
                ResetUIRanges();
            }
        }

        [PropertyControlProperty(DefaultValue = (object)1.0)]
        public new double SliderSmallChangeX
        {
            get
            {
                return base.SliderSmallChangeX;
            }

            set
            {
                base.SliderSmallChangeX = value;
            }
        }

        [PropertyControlProperty(DefaultValue = (object)5.0)]
        public new double SliderLargeChangeX
        {
            get
            {
                return base.SliderLargeChangeX;
            }

            set
            {
                base.SliderLargeChangeX = value;
            }
        }

        [PropertyControlProperty(DefaultValue = (object)1.0)]
        public new double UpDownIncrementX
        {
            get
            {
                return base.UpDownIncrementX;
            }

            set
            {
                base.UpDownIncrementX = value;
            }
        }

        [PropertyControlProperty(DefaultValue = (object)1.0)]
        public new double SliderSmallChangeY
        {
            get
            {
                return base.SliderSmallChangeY;
            }

            set
            {
                base.SliderSmallChangeY = value;
            }
        }

        [PropertyControlProperty(DefaultValue = (object)5.0)]
        public new double SliderLargeChangeY
        {
            get
            {
                return base.SliderLargeChangeY;
            }

            set
            {
                base.SliderLargeChangeY = value;
            }
        }

        [PropertyControlProperty(DefaultValue = (object)1.0)]
        public new double UpDownIncrementY
        {
            get
            {
                return base.UpDownIncrementY;
            }

            set
            {
                base.UpDownIncrementY = value;
            }
        }

        public DoubleVectorSliderPropertyControl(PropertyControlInfo propInfo)
            : base(propInfo)
        {
            DecimalPlaces = (int)propInfo.ControlProperties[ControlInfoPropertyNames.DecimalPlaces].Value;
            SliderSmallChangeX = (double)propInfo.ControlProperties[ControlInfoPropertyNames.SliderSmallChangeX].Value;
            SliderLargeChangeX = (double)propInfo.ControlProperties[ControlInfoPropertyNames.SliderLargeChangeX].Value;
            UpDownIncrementX = (double)propInfo.ControlProperties[ControlInfoPropertyNames.UpDownIncrementX].Value;
            SliderSmallChangeY = (double)propInfo.ControlProperties[ControlInfoPropertyNames.SliderSmallChangeY].Value;
            SliderLargeChangeY = (double)propInfo.ControlProperties[ControlInfoPropertyNames.SliderLargeChangeY].Value;
            UpDownIncrementY = (double)propInfo.ControlProperties[ControlInfoPropertyNames.UpDownIncrementY].Value;
            UseExponentialScale = (bool)propInfo.ControlProperties[ControlInfoPropertyNames.UseExponentialScale].Value;
            ResetUIRanges();
        }

        protected override double RoundPropertyValue(double value)
        {
            double multScale = Math.Pow(10, DecimalPlaces + 1);
            double divScale = Math.Pow(10, -(DecimalPlaces + 1));

            double roundMe = value * multScale;
            double rounded = Math.Round(roundMe);
            double valueResult = rounded * divScale;

            return valueResult;
        }

        private decimal ToNudValue(double propertyValue)
        {
            return (decimal)propertyValue;
        }

        private double FromNudValue(decimal nudValue)
        {
            return (double)nudValue;
        }

        protected override int ToSliderValueX(double propertyValue)
        {
            if (this.useExponentialScale)
            {
                return PropertyControlUtil.ToSliderValueExp(propertyValue, Property.MinValueX, Property.MaxValueX, DecimalPlaces);
            }
            else
            {
                double toIntScale = Math.Pow(10, DecimalPlaces);
                double sliderValueDouble = propertyValue * toIntScale;
                int sliderValueInt = (int)sliderValueDouble;
                return sliderValueInt;
            }
        }

        protected override double FromSliderValueX(int sliderValue)
        {
            if (this.useExponentialScale)
            {
                return PropertyControlUtil.FromSliderValueExp(sliderValue, Property.MinValueX, Property.MaxValueX, DecimalPlaces);
            }
            else
            {
                double fromIntScale = Math.Pow(10, -DecimalPlaces);
                double valueDouble = (double)sliderValue * fromIntScale;
                return valueDouble;
            }
        }

        protected override decimal ToNudValueX(double propertyValue)
        {
            return ToNudValue(propertyValue);
        }

        protected override double FromNudValueX(decimal nudValue)
        {
            return FromNudValue(nudValue);
        }

        protected override int ToSliderValueY(double propertyValue)
        {
            if (this.useExponentialScale)
            {
                return PropertyControlUtil.ToSliderValueExp(propertyValue, Property.MinValueY, Property.MaxValueY, DecimalPlaces);
            }
            else
            {
                double toIntScale = Math.Pow(10, DecimalPlaces);
                double sliderValueDouble = propertyValue * toIntScale;
                int sliderValueInt = (int)sliderValueDouble;
                return sliderValueInt;
            }
        }

        protected override double FromSliderValueY(int sliderValue)
        {
            if (this.useExponentialScale)
            {
                return PropertyControlUtil.FromSliderValueExp(sliderValue, Property.MinValueY, Property.MaxValueY, DecimalPlaces);
            }
            else
            {
                double fromIntScale = Math.Pow(10, -DecimalPlaces);
                double valueDouble = (double)sliderValue * fromIntScale;
                return valueDouble;
            }
        }

        protected override decimal ToNudValueY(double propertyValue)
        {
            return ToNudValue(propertyValue);
        }

        protected override double FromNudValueY(decimal nudValue)
        {
            return FromNudValue(nudValue);
        }
    }
}
