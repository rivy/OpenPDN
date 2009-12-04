/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

// Copyright (C) 2006-2008 Ed Harvey
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

using PaintDotNet.IndirectUI;
using PaintDotNet.PropertySystem;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace PaintDotNet.Effects
{
    public sealed class DentsEffect 
        : WarpEffectBase
    {
        public DentsEffect()
            : base(PdnResources.GetString("DentsEffect.Name"),
                   PdnResources.GetImageResource("Icons.DentsEffectIcon.png").Reference,
                   SubmenuNames.Distort,
                   EffectFlags.Configurable)
        {
            EdgeBehavior = WarpEdgeBehavior.Reflect;
        }

        // This is so that each repetition of the effect shows up differently.
        private byte instanceSeed = unchecked((byte)DateTime.Now.Ticks);

        private double scaleR;
        private double refractionScale;
        private double theta;
        private double roughness;
        private double detail;
        private byte seed;

        public enum PropertyNames
        {
            Scale = 0,
            Refraction = 1,
            Roughness = 2,
            Tension = 3,
            Quality = 4,
            Seed = 5
        }

        protected override PropertyCollection OnCreatePropertyCollection()
        {
            List<Property> props = new List<Property>();

            props.Add(new DoubleProperty(PropertyNames.Scale, 25, 1, 200));
            props.Add(new DoubleProperty(PropertyNames.Refraction, 50, 0, 200));
            props.Add(new DoubleProperty(PropertyNames.Roughness, 10, 0, 100));
            props.Add(new DoubleProperty(PropertyNames.Tension, 10, 0, 100));

            props.Add(new Int32Property(PropertyNames.Quality, 2, 1, 5));
            props.Add(new Int32Property(PropertyNames.Seed, 0, 0, 255));

            return new PropertyCollection(props);
        }

        protected override ControlInfo OnCreateConfigUI(PropertyCollection props)
        {
            ControlInfo info = base.OnCreateConfigUI(props);

            info.SetPropertyControlValue(
                PropertyNames.Scale, 
                ControlInfoPropertyNames.DisplayName, 
                PdnResources.GetString("DentsEffect.ConfigDialog.ScaleLabel"));

            info.SetPropertyControlValue(PropertyNames.Scale, ControlInfoPropertyNames.UseExponentialScale, true);

            info.SetPropertyControlValue(
                PropertyNames.Refraction, 
                ControlInfoPropertyNames.DisplayName, 
                PdnResources.GetString("DentsEffect.ConfigDialog.RefractionLabel"));

            info.SetPropertyControlValue(PropertyNames.Refraction, ControlInfoPropertyNames.UseExponentialScale, true);

            info.SetPropertyControlValue(
                PropertyNames.Roughness, 
                ControlInfoPropertyNames.DisplayName, 
                PdnResources.GetString("DentsEffect.ConfigDialog.RoughnessLabel"));

            info.SetPropertyControlValue(
                PropertyNames.Tension, 
                ControlInfoPropertyNames.DisplayName, 
                PdnResources.GetString("DentsEffect.ConfigDialog.TensionLabel"));

            info.SetPropertyControlValue(PropertyNames.Tension, ControlInfoPropertyNames.UseExponentialScale, true);

            info.SetPropertyControlValue(
                PropertyNames.Quality, 
                ControlInfoPropertyNames.DisplayName, 
                PdnResources.GetString("DentsEffect.ConfigDialog.QualityLabel"));

            info.SetPropertyControlType(
                PropertyNames.Seed, 
                PropertyControlType.IncrementButton);

            info.SetPropertyControlValue(
                PropertyNames.Seed,
                ControlInfoPropertyNames.DisplayName,
                PdnResources.GetString("DentsEffect.ConfigDialog.SeedLabel"));

            info.SetPropertyControlValue(
                PropertyNames.Seed,
                ControlInfoPropertyNames.ButtonText,
                PdnResources.GetString("DentsEffect.ConfigDialog.SeedButtonText"));

            return info;
        }

        protected override void OnSetRenderInfo2(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            double scale = newToken.GetProperty<DoubleProperty>(PropertyNames.Scale).Value;

            double refraction = newToken.GetProperty<DoubleProperty>(PropertyNames.Refraction).Value;
            double detail1 = newToken.GetProperty<DoubleProperty>(PropertyNames.Roughness).Value;
            double detail2 = detail1;
            double roughness = detail2;

            double turbulence = newToken.GetProperty<DoubleProperty>(PropertyNames.Tension).Value;

            int quality = newToken.GetProperty<Int32Property>(PropertyNames.Quality).Value;
            byte newSeed = (byte)newToken.GetProperty<Int32Property>(PropertyNames.Seed).Value;

            this.seed = (byte)(this.instanceSeed ^ newSeed);

            this.scaleR = (400.0 / base.DefaultRadius) / scale;
            this.refractionScale = (refraction / 100.0) / scaleR;
            this.theta = Math.PI * 2.0 * turbulence / 10.0;
            this.roughness = roughness / 100.0;

            double detail3 = 1.0 + (detail2 / 10.0);

            // we don't want the perlin noise frequency components exceeding
            // the nyquist limit, so we will limit 'detail' appropriately
            double maxDetail = Math.Floor(Math.Log(this.scaleR) / Math.Log(0.5));

            if (detail3 > maxDetail && maxDetail >= 1.0)
            {
                this.detail = maxDetail;
            }
            else
            {
                this.detail = detail3;
            }

            base.Quality = quality;

            base.OnSetRenderInfo2(newToken, dstArgs, srcArgs);
        }

        protected override void InverseTransform(ref TransformData data)
        {
            double x = data.X;
            double y = data.Y;

            double ix = x * scaleR;
            double iy = y * scaleR;

            double bumpAngle = this.theta * PerlinNoise2D.Noise(ix, iy, this.detail, this.roughness, this.seed);

            data.X = x + (this.refractionScale * Math.Sin(-bumpAngle));
            data.Y = y + (this.refractionScale * Math.Cos(bumpAngle));
        }
    }
}
