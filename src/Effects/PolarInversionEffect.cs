/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

// Copyright (c) 2006-2008 Ed Harvey 
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

using System.Collections.Generic;
using System.Drawing;
using PaintDotNet.IndirectUI;
using PaintDotNet.PropertySystem;

namespace PaintDotNet.Effects
{
    public sealed class PolarInversionEffect 
        : WarpEffectBase
    {
        public PolarInversionEffect()
            : base(PdnResources.GetString("PolarInversion.Name"),
                   PdnResources.GetImageResource("Icons.PolarInversionEffect.png").Reference,
                   SubmenuNames.Distort, 
                   EffectFlags.Configurable)
        {
        }

        public enum PropertyNames
        {
            Amount = 0,
            Offset = 1,
            EdgeBehavior = 2,
            Quality = 3
        }

        private double amount;

        protected override PropertyCollection OnCreatePropertyCollection()
        {
            List<Property> properties = new List<Property>();

            properties.Add(new DoubleProperty(PropertyNames.Amount, 1, -4, 4));
            properties.Add(new DoubleVectorProperty(PropertyNames.Offset, Pair.Create<double, double>(0, 0), Pair.Create<double, double>(-2, -2), Pair.Create<double, double>(2, 2)));
            properties.Add(new StaticListChoiceProperty(PropertyNames.EdgeBehavior, new object[] { WarpEdgeBehavior.Clamp, WarpEdgeBehavior.Reflect, WarpEdgeBehavior.Wrap }, 2));
            properties.Add(new Int32Property(PropertyNames.Quality, 2, 1, 5));

            return new PropertyCollection(properties);
        }

        protected override ControlInfo OnCreateConfigUI(PropertyCollection props)
        {
            ControlInfo configUI = base.OnCreateConfigUI(props);

            configUI.SetPropertyControlValue(PropertyNames.Amount, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("PolarInversion.ConfigUI.Amount.DisplayName"));
            configUI.SetPropertyControlValue(PropertyNames.Amount, ControlInfoPropertyNames.UseExponentialScale, true);
            configUI.SetPropertyControlValue(PropertyNames.Amount, ControlInfoPropertyNames.SliderLargeChange, 0.25);
            configUI.SetPropertyControlValue(PropertyNames.Amount, ControlInfoPropertyNames.SliderSmallChange, 0.05);
            configUI.SetPropertyControlValue(PropertyNames.Amount, ControlInfoPropertyNames.UpDownIncrement, 0.01);

            configUI.SetPropertyControlValue(PropertyNames.Offset, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("PolarInversion.ConfigUI.Offset.DisplayName"));
            configUI.SetPropertyControlValue(PropertyNames.Offset, ControlInfoPropertyNames.SliderSmallChangeX, 0.05);
            configUI.SetPropertyControlValue(PropertyNames.Offset, ControlInfoPropertyNames.SliderLargeChangeX, 0.25);
            configUI.SetPropertyControlValue(PropertyNames.Offset, ControlInfoPropertyNames.UpDownIncrementX, 0.01);
            configUI.SetPropertyControlValue(PropertyNames.Offset, ControlInfoPropertyNames.SliderSmallChangeY, 0.05);
            configUI.SetPropertyControlValue(PropertyNames.Offset, ControlInfoPropertyNames.SliderLargeChangeY, 0.25);
            configUI.SetPropertyControlValue(PropertyNames.Offset, ControlInfoPropertyNames.UpDownIncrementY, 0.01);

            Rectangle selection = this.EnvironmentParameters.GetSelection(base.EnvironmentParameters.SourceSurface.Bounds).GetBoundsInt();
            ImageResource propertyValue = ImageResource.FromImage(base.EnvironmentParameters.SourceSurface.CreateAliasedBitmap(selection));
            configUI.SetPropertyControlValue(PropertyNames.Offset, ControlInfoPropertyNames.StaticImageUnderlay, propertyValue);

            configUI.SetPropertyControlValue(PropertyNames.EdgeBehavior, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("PolarInversion.ConfigUI.EdgeBehavior.DisplayName"));

            PropertyControlInfo edgeBehaviorPCI = configUI.FindControlForPropertyName(PropertyNames.EdgeBehavior);
            edgeBehaviorPCI.SetValueDisplayName(WarpEdgeBehavior.Clamp, PdnResources.GetString("PolarInversion.ConfigUI.EdgeBehavior.Clamp.DisplayName"));
            edgeBehaviorPCI.SetValueDisplayName(WarpEdgeBehavior.Reflect, PdnResources.GetString("PolarInversion.ConfigUI.EdgeBehavior.Reflect.DisplayName"));
            edgeBehaviorPCI.SetValueDisplayName(WarpEdgeBehavior.Wrap, PdnResources.GetString("PolarInversion.ConfigUI.EdgeBehavior.Wrap.DisplayName"));

            configUI.SetPropertyControlValue(PropertyNames.Quality, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("PolarInversion.ConfigUI.Quality.DisplayName"));

            return configUI;
        }

        protected override void OnSetRenderInfo2(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            this.amount = newToken.GetProperty<DoubleProperty>(PropertyNames.Amount).Value;
            base.Offset = newToken.GetProperty<DoubleVectorProperty>(PropertyNames.Offset).Value;
            base.EdgeBehavior = (WarpEdgeBehavior)newToken.GetProperty<StaticListChoiceProperty>(PropertyNames.EdgeBehavior).Value;
            base.Quality = newToken.GetProperty<Int32Property>(PropertyNames.Quality).Value;
        }

        protected override void InverseTransform(ref TransformData data)
        {
            double x = data.X;
            double y = data.Y;

            // NOTE: when x and y are zero, this will divide by zero and return NaN
            double invertDistance = Utility.Lerp(1d, DefaultRadius2 / ((x * x) + (y * y)), amount);

            data.X = x * invertDistance;
            data.Y = y * invertDistance;
        }
    }
}
