/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

// Copyright (c) 2007,2008 Ed Harvey 
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
    public sealed class VignetteEffect
        : InternalPropertyBasedEffect
    {
        public VignetteEffect()
            : base(PdnResources.GetString("VignetteEffect.Name"),
                   PdnResources.GetImageResource("Icons.VignetteEffectIcon.png").Reference,
                   SubmenuNames.Photo,
                   EffectFlags.Configurable)
        {
        }

        public enum PropertyNames
        {
            Offset,
            Amount,
            Radius,
        }

        protected override PropertyCollection OnCreatePropertyCollection()
        {
            List<Property> properties = new List<Property>();

            properties.Add(new DoubleVectorProperty(PropertyNames.Offset, 
                Pair.Create(0.0, 0.0), Pair.Create(-1.0, -1.0), Pair.Create(1.0, 1.0)));

            //somewhat arbitary limits...
            properties.Add(new DoubleProperty(PropertyNames.Radius, 0.5, 0.1, 4.0));

            properties.Add(new DoubleProperty(PropertyNames.Amount, 1.0, 0.0, 1.0));

            return new PropertyCollection(properties);
        }

        protected override ControlInfo OnCreateConfigUI(PropertyCollection props)
        {
            ControlInfo info = PropertyBasedEffect.CreateDefaultConfigUI(props);

            info.SetPropertyControlValue(
                PropertyNames.Offset, 
                ControlInfoPropertyNames.DisplayName, 
                PdnResources.GetString("VignetteEffect.ConfigDialog.CenterLabel"));

            info.SetPropertyControlValue(PropertyNames.Offset, ControlInfoPropertyNames.SliderSmallChangeX, 0.05);
            info.SetPropertyControlValue(PropertyNames.Offset, ControlInfoPropertyNames.SliderLargeChangeX, 0.25);
            info.SetPropertyControlValue(PropertyNames.Offset, ControlInfoPropertyNames.UpDownIncrementX, 0.01);
            info.SetPropertyControlValue(PropertyNames.Offset, ControlInfoPropertyNames.SliderSmallChangeY, 0.05);
            info.SetPropertyControlValue(PropertyNames.Offset, ControlInfoPropertyNames.SliderLargeChangeY, 0.25);
            info.SetPropertyControlValue(PropertyNames.Offset, ControlInfoPropertyNames.UpDownIncrementY, 0.01);

            // thumbnail/preview
            Rectangle selection = this.EnvironmentParameters.GetSelection(base.EnvironmentParameters.SourceSurface.Bounds).GetBoundsInt();
            ImageResource propertyValue = ImageResource.FromImage(base.EnvironmentParameters.SourceSurface.CreateAliasedBitmap(selection));
            info.SetPropertyControlValue(PropertyNames.Offset, ControlInfoPropertyNames.StaticImageUnderlay, propertyValue);

            info.SetPropertyControlValue(
                PropertyNames.Radius, 
                ControlInfoPropertyNames.DisplayName, 
                PdnResources.GetString("VignetteEffect.ConfigDialog.RadiusLabel"));

            info.SetPropertyControlValue(PropertyNames.Radius, ControlInfoPropertyNames.UseExponentialScale, true);
            info.SetPropertyControlValue(PropertyNames.Radius, ControlInfoPropertyNames.SliderLargeChange, 0.25);
            info.SetPropertyControlValue(PropertyNames.Radius, ControlInfoPropertyNames.SliderSmallChange, 0.05);
            info.SetPropertyControlValue(PropertyNames.Radius, ControlInfoPropertyNames.UpDownIncrement, 0.01);

            info.SetPropertyControlValue(
                PropertyNames.Amount, 
                ControlInfoPropertyNames.DisplayName, 
                PdnResources.GetString("VignetteEffect.ConfigDialog.DensityLabel"));

            info.SetPropertyControlValue(PropertyNames.Amount, ControlInfoPropertyNames.UseExponentialScale, true);
            info.SetPropertyControlValue(PropertyNames.Amount, ControlInfoPropertyNames.SliderLargeChange, 0.25);
            info.SetPropertyControlValue(PropertyNames.Amount, ControlInfoPropertyNames.SliderSmallChange, 0.05);
            info.SetPropertyControlValue(PropertyNames.Amount, ControlInfoPropertyNames.UpDownIncrement, 0.01);

            return info;
        }

        protected override void OnSetRenderInfo(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            this.offset = newToken.GetProperty<DoubleVectorProperty>(PropertyNames.Offset).Value;
            this.amount = newToken.GetProperty<DoubleProperty>(PropertyNames.Amount).Value;
            this.amount1 = 1d - amount;

            Rectangle bounds = this.EnvironmentParameters.GetSelection(srcArgs.Surface.Bounds).GetBoundsInt();

            double width = bounds.Width;
            double height = bounds.Height;
            this.xCenterOffset = bounds.Left + (width * (1d + offset.First) * 0.5d);
            this.yCenterOffset = bounds.Top + (height * (1d + offset.Second) * 0.5d);

            this.radius = Math.Max(width, height) * 0.5d;
            this.radius *= newToken.GetProperty<DoubleProperty>(PropertyNames.Radius).Value;
            this.radius *= radius;
            this.radiusR = Math.PI / (8 * radius);

            base.OnSetRenderInfo(newToken, dstArgs, srcArgs);
        }

        private double amount;
        private double amount1;
        private double radius;
        private double radiusR;

        private double xCenterOffset;
        private double yCenterOffset;

        private Pair<double, double> offset = new Pair<double, double>(0, 0);

        protected unsafe override void OnRender(Rectangle[] renderRects, int startIndex, int length)
        {
            for (int ri = startIndex; ri < startIndex + length; ++ri)
            {
                Rectangle rect = renderRects[ri];

                for (int y = rect.Top; y < rect.Bottom; ++y)
                {
                    double iy2 = y - this.yCenterOffset;
                    iy2 *= iy2;

                    ColorBgra* srcPtr = SrcArgs.Surface.GetPointAddress(rect.Left, y);
                    ColorBgra* dstPtr = DstArgs.Surface.GetPointAddress(rect.Left, y);

                    for (int x = rect.Left; x < rect.Right; ++x)
                    {
                        double ix = x - this.xCenterOffset;
                        double d = (iy2 + (ix * ix)) * radiusR;
                        double factor = Math.Cos(d);

                        if (factor <= 0 || d > Math.PI)
                        {
                            dstPtr->R = (byte)(0.5 + (255 * SrgbUtility.ToSrgbClamped(SrgbUtility.ToLinear(srcPtr->R) * amount1)));
                            dstPtr->G = (byte)(0.5 + (255 * SrgbUtility.ToSrgbClamped(SrgbUtility.ToLinear(srcPtr->G) * amount1)));
                            dstPtr->B = (byte)(0.5 + (255 * SrgbUtility.ToSrgbClamped(SrgbUtility.ToLinear(srcPtr->B) * amount1)));
                            dstPtr->A = srcPtr->A;
                        }
                        else
                        {
                            factor *= factor;
                            factor *= factor;
                            factor = amount1 + (amount * factor);
                            dstPtr->R = (byte)(0.5 + (255 * SrgbUtility.ToSrgbClamped(SrgbUtility.ToLinear(srcPtr->R) * factor)));
                            dstPtr->G = (byte)(0.5 + (255 * SrgbUtility.ToSrgbClamped(SrgbUtility.ToLinear(srcPtr->G) * factor)));
                            dstPtr->B = (byte)(0.5 + (255 * SrgbUtility.ToSrgbClamped(SrgbUtility.ToLinear(srcPtr->B) * factor)));
                            dstPtr->A = srcPtr->A;
                        }

                        ++dstPtr;
                        ++srcPtr;
                    }
                }
            }
        }
    }
}
