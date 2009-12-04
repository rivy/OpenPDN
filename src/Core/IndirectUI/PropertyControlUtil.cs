/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) Rick Brewster, Tom Jackson, and past contributors.            //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.PropertySystem;
using PaintDotNet.SystemLayer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;

namespace PaintDotNet.IndirectUI
{
    internal static class PropertyControlUtil
    {
        public static int GetGoodSliderHeight(TrackBar slider)
        {
            if (slider.AutoSize)
            {
                return slider.Height;
            }
            else if (slider.TickStyle == TickStyle.BottomRight || slider.TickStyle == TickStyle.TopLeft)
            {
                return UI.ScaleHeight(35); // determined experimentally
            }
            else if (slider.TickStyle == TickStyle.None)
            {
                return UI.ScaleHeight(25); // determined experimentally
            }
            else if (slider.TickStyle == TickStyle.Both)
            {
                return UI.ScaleHeight(45); // pulled from default Height value when AutoSize=true
            }
            else
            {
                throw new InvalidEnumArgumentException();
            }
        }

        public static int GetGoodSliderTickFrequency(TrackBar slider)
        {
            int delta = Math.Abs(slider.Maximum - slider.Minimum);
            int goodTicks = Math.Max(1, delta / 20);
            return goodTicks;
        }

        public static int ToSliderValueExpCore(double propertyValue, double minValue, double maxValue, int scaleLog10)
        {
            int scaleLog10Plus1 = 1 + scaleLog10;

            int toIntScale = (int)Math.Pow(10, scaleLog10Plus1);
            double toDoubleScale = Math.Pow(10, -scaleLog10Plus1);

            double lerp = Math.Abs((propertyValue - minValue) / (maxValue - minValue));
            double lerp2 = Math.Sqrt(lerp);
            double newPropertyValue = minValue + (lerp2 * (maxValue - minValue));
            double clampedNewPropertyValue = Utility.Clamp(newPropertyValue, minValue, maxValue);
            double newSliderValueDouble = clampedNewPropertyValue * toIntScale;
            long newSliderValueLong = (long)newSliderValueDouble;
            int newSliderValueInt = (int)newSliderValueLong;

            return newSliderValueInt;
        }

        public static int ToSliderValueExp(double propertyValue, double minValue, double maxValue, int scaleLog10)
        {
            if (propertyValue == 0)
            {
                // Zero is always zero.
                return 0;
            }
            else if (minValue >= 0 && maxValue >= 0)
            {
                // All positive range: normal math
                return ToSliderValueExpCore(propertyValue, minValue, maxValue, scaleLog10);
            }
            else if (minValue <= 0 && maxValue <= 0)
            {
                // All negative range: negate values, convert, then negate back over to positve range again
                return -ToSliderValueExpCore(-propertyValue, -minValue, -maxValue, scaleLog10);
            }
            else if (propertyValue > 0)
            {
                return ToSliderValueExpCore(propertyValue, 0, maxValue, scaleLog10);
            }
            else if (propertyValue < 0)
            {
                return -ToSliderValueExpCore(-propertyValue, 0, -minValue, scaleLog10);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public static double FromSliderValueExpCore(int sliderValue, double minValue, double maxValue, int scaleLog10)
        {
            int scaleLog10Plus1 = 1 + scaleLog10;

            int toIntScale = (int)Math.Pow(10, scaleLog10Plus1);
            double toDoubleScale = Math.Pow(10, -scaleLog10Plus1);

            double newPropertyValue = (double)sliderValue * toDoubleScale;
            double lerp2 = (newPropertyValue - minValue) / (maxValue - minValue);
            double lerp = lerp2 * lerp2;
            double propertyValue = minValue + (lerp * (maxValue - minValue));
            return Utility.Clamp(propertyValue, minValue, maxValue);
        }

        public static double FromSliderValueExp(int sliderValue, double minValue, double maxValue, int scaleLog10)
        {
            if (sliderValue == 0)
            {
                // Zero is always zero.
                return 0;
            }
            else if (minValue >= 0 && maxValue >= 0)
            {
                // All positive range: normal math
                return FromSliderValueExpCore(sliderValue, minValue, maxValue, scaleLog10);
            }
            else if (minValue <= 0 && maxValue <= 0)
            {
                // All negative range: negate values, convert, then negate back over to positve range again
                return -FromSliderValueExpCore(-sliderValue, -minValue, -maxValue, scaleLog10);
            }
            else if (sliderValue > 0)
            {
                // Split range, and value is in the positive side. 
                return FromSliderValueExpCore(sliderValue, 0, maxValue, scaleLog10);
            }
            else if (sliderValue < 0)
            {
                // Split range, and value is in the negative side.
                return -FromSliderValueExpCore(-sliderValue, 0, -minValue, scaleLog10);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }
}
