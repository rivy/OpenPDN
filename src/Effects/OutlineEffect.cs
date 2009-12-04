/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.IndirectUI;
using PaintDotNet.PropertySystem;
using System;
using System.Drawing;
using System.Collections.Generic;

namespace PaintDotNet.Effects
{
    public sealed class OutlineEffect
        : LocalHistogramEffect
    {
        public static string StaticName
        {
            get
            {
                return PdnResources.GetString("OutlineEffect.Name");
            }
        }

        public static ImageResource StaticImage
        {
            get
            {
                return PdnResources.GetImageResource("Icons.OutlineEffectIcon.png");
            }
        }

        public enum PropertyNames
        {
            Thickness = 0,
            Intensity = 1
        }

        private int thickness;
        private int intensity;

        public OutlineEffect()
            : base(StaticName, StaticImage.Reference, SubmenuNames.Stylize, EffectFlags.Configurable)
        {
        }

        protected override PropertyCollection OnCreatePropertyCollection()
        {
            List<Property> props = new List<Property>();

            props.Add(new Int32Property(PropertyNames.Thickness, 3, 1, 200));
            props.Add(new Int32Property(PropertyNames.Intensity, 50, 0, 100));

            return new PropertyCollection(props);
        }

        protected override ControlInfo OnCreateConfigUI(PropertyCollection props)
        {
            ControlInfo configUI = CreateDefaultConfigUI(props);

            configUI.SetPropertyControlValue(PropertyNames.Thickness, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("OutlineEffect.ConfigDialog.ThicknessLabel"));
            configUI.SetPropertyControlValue(PropertyNames.Intensity, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("OutlineEffect.ConfigDialog.IntensityLabel"));

            return configUI;
        }

        protected override void OnSetRenderInfo(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            this.thickness = newToken.GetProperty<Int32Property>(PropertyNames.Thickness).Value;
            this.intensity = newToken.GetProperty<Int32Property>(PropertyNames.Intensity).Value;

            base.OnSetRenderInfo(newToken, dstArgs, srcArgs);
        }

        public unsafe override ColorBgra Apply(ColorBgra src, int area, int* hb, int* hg, int* hr, int* ha)
        {
            int minCount1 = area * (100 - this.intensity) / 200;
            int minCount2 = area * (100 + this.intensity) / 200;

            int bCount = 0;
            int b1 = 0;
            while (b1 < 255 && hb[b1] == 0)
            {
                ++b1;
            }

            while (b1 < 255 && bCount < minCount1)
            {
                bCount += hb[b1];
                ++b1;
            }

            int b2 = b1;
            while (b2 < 255 && bCount < minCount2)
            {
                bCount += hb[b2];
                ++b2;
            }

            int gCount = 0;
            int g1 = 0;
            while (g1 < 255 && hg[g1] == 0)
            {
                ++g1;
            }

            while (g1 < 255 && gCount < minCount1)
            {
                gCount += hg[g1];
                ++g1;
            }

            int g2 = g1;
            while (g2 < 255 && gCount < minCount2)
            {
                gCount += hg[g2];
                ++g2;
            }

            int rCount = 0;
            int r1 = 0;
            while (r1 < 255 && hr[r1] == 0)
            {
                ++r1;
            }

            while (r1 < 255 && rCount < minCount1)
            {
                rCount += hr[r1];
                ++r1;
            }

            int r2 = r1;
            while (r2 < 255 && rCount < minCount2)
            {
                rCount += hr[r2];
                ++r2;
            }

            int aCount = 0;
            int a1 = 0;
            while (a1 < 255 && hb[a1] == 0)
            {
                ++a1;
            }

            while (a1 < 255 && aCount < minCount1)
            {
                aCount += ha[a1];
                ++a1;
            }

            int a2 = a1;
            while (a2 < 255 && aCount < minCount2)
            {
                aCount += ha[a2];
                ++a2;
            }

            return ColorBgra.FromBgra(
                (byte)(255 - (b2 - b1)),
                (byte)(255 - (g2 - g1)),
                (byte)(255 - (r2 - r1)),
                (byte)(a2));
        }

        protected unsafe override void OnRender(Rectangle[] rois, int startIndex, int length)
        {
            foreach (Rectangle rect in rois)
            {
                RenderRect(this.thickness, SrcArgs.Surface, DstArgs.Surface, rect);
            }
        }
    }
}
