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
using System.Collections.Generic;
using System.Drawing;

namespace PaintDotNet.Effects
{
    public sealed class SharpenEffect
        : LocalHistogramEffect
    {
        public static string StaticName
        {
            get
            {
                return PdnResources.GetString("SharpenEffect.Name");
            }
        }

        public static ImageResource StaticImage
        {
            get
            {
                return PdnResources.GetImageResource("Icons.SharpenEffect.png");
            }
        }

        public enum PropertyNames
        {
            Amount = 0
        }

        public SharpenEffect()
            : base(StaticName, StaticImage.Reference, SubmenuNames.Photo, EffectFlags.Configurable)
        {
        }

        protected override PropertyCollection OnCreatePropertyCollection()
        {
            List<Property> props = new List<Property>();

            props.Add(new Int32Property(PropertyNames.Amount, 2, 1, 20));

            return new PropertyCollection(props);
        }

        protected override ControlInfo OnCreateConfigUI(PropertyCollection props)
        {
            ControlInfo configUI = CreateDefaultConfigUI(props);

            configUI.SetPropertyControlValue(PropertyNames.Amount, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("SharpenEffect.ConfigDialog.SliderLabel"));

            return configUI;
        }

        private int amount;

        protected override void OnSetRenderInfo(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            this.amount = newToken.GetProperty<Int32Property>(PropertyNames.Amount).Value;
            base.OnSetRenderInfo(newToken, dstArgs, srcArgs);
        }

        public unsafe override ColorBgra Apply(ColorBgra src, int area, int* hb, int* hg, int* hr, int* ha)
        {
            ColorBgra median = GetPercentile(50, area, hb, hg, hr, ha);
            return ColorBgra.Lerp(src, median, -0.5f);
        }

        protected unsafe override void OnRender(Rectangle[] rois, int startIndex, int length)
        {
            foreach (Rectangle rect in rois)
            {
                RenderRect(this.amount, SrcArgs.Surface, DstArgs.Surface, rect);
            }
        }
    }
}
