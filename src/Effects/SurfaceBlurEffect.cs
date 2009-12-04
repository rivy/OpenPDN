/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

// Copyright (c) 2007, 2008 Ed Harvey 
//
// MIT License: http://www.opensource.org/licenses/mit-license.php
//
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal 
// in the Software without restriction, including without limitation the rights 
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions: 
//
// The above copyright notice and this permission notice shall be included in 
// all copies or substantial portions of the Software. 
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
// THE SOFTWARE. 
//

using System;
using System.Collections.Generic;
using System.Drawing;
using PaintDotNet.IndirectUI;
using PaintDotNet.PropertySystem;

namespace PaintDotNet.Effects
{
    public sealed class SurfaceBlurEffect
        : LocalHistogramEffect
    {
        public SurfaceBlurEffect()
            : base(PdnResources.GetString("SurfaceBlurEffect.Name"),
                   PdnResources.GetImageResource("Icons.SurfaceBlurEffectIcon.png").Reference,
                   SubmenuNames.Blurs, 
                   EffectFlags.Configurable)
        {
        }

        private int radius;
        private int threshold;
        private int[] intensityFunction;

        public enum PropertyName
        {
            Radius = 0,
            Threshold = 1,
        }

        protected override PropertyCollection OnCreatePropertyCollection()
        {
            List<Property> properties = new List<Property>();

            properties.Add(new Int32Property(PropertyName.Radius, 6, 1, 100));
            properties.Add(new Int32Property(PropertyName.Threshold, 15, 1, 100));

            return new PropertyCollection(properties);
        }

        protected override ControlInfo OnCreateConfigUI(PropertyCollection props)
        {
            ControlInfo info = PropertyBasedEffect.CreateDefaultConfigUI(props);

            info.SetPropertyControlValue(
                PropertyName.Radius, 
                ControlInfoPropertyNames.DisplayName, 
                PdnResources.GetString("SurfaceBlurEffect.ConfigDialog.RadiusLabel"));

            info.SetPropertyControlValue(
                PropertyName.Threshold, 
                ControlInfoPropertyNames.DisplayName, 
                PdnResources.GetString("SurfaceBlurEffect.ConfigDialog.ThresholdLabel"));

            // don't need to add custom increments - the defaults are fine

            return info;
        }

        protected override void OnSetRenderInfo(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            this.radius = newToken.GetProperty<Int32Property>(PropertyName.Radius).Value;
            this.threshold = newToken.GetProperty<Int32Property>(PropertyName.Threshold).Value;
            this.intensityFunction = PrecalculateIntensityFunction(this.threshold);

            base.OnSetRenderInfo(newToken, dstArgs, srcArgs);
        }

        // rather than a fancy function such as a gaussian, 
        // currently using a trigular function as this seems to be 'good-enough'
        private static int[] PrecalculateIntensityFunction(int threshold)
        {
            int[] factors = new int[256];

            double slope = 96d / threshold;

            for (int i = 0; i < 256; i++)
            {
                int factor = (int)Math.Round(255 - (i * slope), MidpointRounding.AwayFromZero);

                if (factor < 0)
                {
                    factor = 0;
                }

                factors[i] = factor;
            }

            return factors;
        }

        protected override void OnRender(Rectangle[] renderRects, int startIndex, int length)
        {
            foreach (Rectangle rect in renderRects)
            {
                RenderRect(this.radius, SrcArgs.Surface, DstArgs.Surface, rect);
            }
        }

        public override unsafe ColorBgra Apply(ColorBgra src, int area, int* hb, int* hg, int* hr, int* ha)
        {
            int resultB = BlurChannel(src.B, hb);
            int resultG = BlurChannel(src.G, hg);
            int resultR = BlurChannel(src.R, hr);

            // there is no way we can deal with pre-multiplied alphas; the correlation 
            // between channels no longer exists by this point in the algorithm... 
            // so, just use the alpha from the source pixel.

            ColorBgra result = ColorBgra.FromBgra((byte)resultB, (byte)resultG, (byte)resultR, src.A);
            return result;
        }

        private unsafe int BlurChannel(int current, int* histogram)
        {
            // note to self: pointers are passed by-value...
            //               incrementing passed pointer - no effect outside current scope
            int sum = 0;
            int divisor = 0;
            int result = current;

            for (int bin = 0; bin < 256; bin++)
            {
                if (*histogram > 0)
                {
                    int diff;

                    if (bin > current)
                    {
                        diff = bin - current;
                    }
                    else
                    {
                        diff = current - bin;
                    }

                    int intensity = this.intensityFunction[diff];

                    if (intensity > 0)
                    {
                        int t = (*histogram) * intensity;
                        sum += (t * bin);
                        divisor += t;
                    }
                }

                ++histogram;
            }

            if (divisor > 0)
            {
                // 1/2 LSB for integer rounding
                int roundingTerm = divisor >> 1;
                result = (sum + roundingTerm) / divisor;
            }

            return result;
        }
    }
}