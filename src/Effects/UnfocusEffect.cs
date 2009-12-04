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
using System.Text;

namespace PaintDotNet.Effects
{
    public sealed class UnfocusEffect
        : LocalHistogramEffect
    {
        public static string StaticName
        {
            get
            {
                return PdnResources.GetString("UnfocusEffect.Name");
            }
        }

        public static ImageResource StaticImage
        {
            get
            {
                return PdnResources.GetImageResource("Icons.UnfocusEffectIcon.png");
            }
        }

        public UnfocusEffect() 
            : base(StaticName, 
                   StaticImage.Reference,
                   SubmenuNames.Blurs,
                   EffectFlags.Configurable)
        { 
        }

        public enum PropertyNames
        {
            Radius = 0
        }

        protected override PropertyCollection OnCreatePropertyCollection()
        {
            List<Property> props = new List<Property>();

            props.Add(new Int32Property(PropertyNames.Radius, 4, 1, 200));

            return new PropertyCollection(props);
        }

        protected override ControlInfo OnCreateConfigUI(PropertyCollection props)
        {
            ControlInfo configUI = CreateDefaultConfigUI(props);

            configUI.SetPropertyControlValue(PropertyNames.Radius, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("UnfocusEffect.ConfigDialog.AmountLabel"));

            // TODO: units label
            //acd.SliderUnitsName = PdnResources.GetString("UnfocusEffect.ConfigDialog.UnitsLabel");

            return configUI;
        }

        private int radius;

        protected override void OnSetRenderInfo(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            this.radius = newToken.GetProperty<Int32Property>(PropertyNames.Radius).Value;
            base.OnSetRenderInfo(newToken, dstArgs, srcArgs);
        }

        public unsafe override ColorBgra ApplyWithAlpha(ColorBgra src, int area, int sum, int* hb, int* hg, int* hr)
        {
            //each slot of the histgram can contain up to area * 255. This will overflow an int when area > 32k
            if (area < 32768)
            {
                int b = 0;
                int g = 0;
                int r = 0;

                for (int i = 1; i < 256; ++i)
                {
                    b += i * hb[i];
                    g += i * hg[i];
                    r += i * hr[i];
                }

                int alpha = sum / area;
                int div = area * 255;

                return ColorBgra.FromBgraClamped(b / div, g / div, r / div, alpha);
            }
            else //use a long if an int will overflow.
            {
                long b = 0;
                long g = 0;
                long r = 0;

                for (long i = 1; i < 256; ++i)
                {
                    b += i * hb[i];
                    g += i * hg[i];
                    r += i * hr[i];
                }

                int alpha = sum / area;
                int div = area * 255;

                return ColorBgra.FromBgraClamped(b / div, g / div, r / div, alpha);
            }
        }

        protected unsafe override void OnRender(Rectangle[] rois, int startIndex, int length)
        {
            foreach (Rectangle rect in rois)
            {
                RenderRectWithAlpha(this.radius, SrcArgs.Surface, DstArgs.Surface, rect);
            }
        }
    }
}
