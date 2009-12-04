/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet;
using PaintDotNet.PropertySystem;
using PaintDotNet.IndirectUI;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace PaintDotNet.Effects
{
    public sealed class ReduceNoiseEffect
        : LocalHistogramEffect
    {
        private int radius;
        private double strength;

        public ReduceNoiseEffect()
            : base(StaticName, StaticImage, SubmenuNames.Noise, EffectFlags.Configurable)
        {
        }

        public static string StaticName
        {
            get
            {
                return PdnResources.GetString("ReduceNoiseEffect.Name");
            }
        }

        public static Image StaticImage
        {
            get
            {
                return PdnResources.GetImageResource("Icons.ReduceNoiseEffectIcon.png").Reference;
            }
        }

        public enum PropertyNames
        {
            Radius = 0,
            Strength = 1
        }

        protected override PropertyCollection OnCreatePropertyCollection()
        {
            List<Property> props = new List<Property>();

            props.Add(new Int32Property(PropertyNames.Radius, 10, 0, 200));
            props.Add(new DoubleProperty(PropertyNames.Strength, 0.4, 0, 1));

            return new PropertyCollection(props);
        }

        protected override ControlInfo OnCreateConfigUI(PropertyCollection props)
        {
            ControlInfo configUI = CreateDefaultConfigUI(props);

            configUI.SetPropertyControlValue(PropertyNames.Radius, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("ReduceNoise.Radius.DisplayName"));
            configUI.SetPropertyControlValue(PropertyNames.Strength, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("ReduceNoise.Strength.DisplayName"));

            PropertyControlInfo strengthControlInfo = configUI.FindControlForPropertyName(PropertyNames.Strength);
            configUI.SetPropertyControlValue(PropertyNames.Strength, ControlInfoPropertyNames.UpDownIncrement, 0.01);
            configUI.SetPropertyControlValue(PropertyNames.Strength, ControlInfoPropertyNames.SliderSmallChange, 0.01);
            configUI.SetPropertyControlValue(PropertyNames.Strength, ControlInfoPropertyNames.SliderLargeChange, 0.1);

            return configUI;
        }

        protected override void OnSetRenderInfo(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            this.radius = newToken.GetProperty<Int32Property>(PropertyNames.Radius).Value;
            this.strength = -0.2 * newToken.GetProperty<DoubleProperty>(PropertyNames.Strength).Value;

            base.OnSetRenderInfo(newToken, dstArgs, srcArgs);
        }

        protected override void OnRender(Rectangle[] renderRects, int startIndex, int length)
        {
            for (int i = startIndex; i < startIndex + length; ++i)
            {
                RenderRect(radius, this.SrcArgs.Surface, this.DstArgs.Surface, renderRects[i]);
            }
        }

        public override unsafe ColorBgra Apply(ColorBgra color, int area, int* hb, int* hg, int* hr, int* ha)
        {
            ColorBgra normalized = GetPercentileOfColor(color, area, hb, hg, hr, ha);
            double lerp = strength * (1 - 0.75 * color.GetIntensity());

            return ColorBgra.Lerp(color, normalized, lerp);
        }

        private static unsafe ColorBgra GetPercentileOfColor(ColorBgra color, int area, int* hb, int* hg, int* hr, int* ha)
        {
            int rc = 0;
            int gc = 0;
            int bc = 0;

            for (int i = 0; i < color.R; ++i)
            {
                rc += hr[i];
            }

            for (int i = 0; i < color.G; ++i)
            {
                gc += hg[i];
            }

            for (int i = 0; i < color.B; ++i)
            {
                bc += hb[i];
            }

            rc = (rc * 255) / area;
            gc = (gc * 255) / area;
            bc = (bc * 255) / area;

            return ColorBgra.FromBgr((byte)bc, (byte)gc, (byte)rc);
        }
    }
}
