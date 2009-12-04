/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet;
using PaintDotNet.IndirectUI;
using PaintDotNet.PropertySystem;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PaintDotNet.Effects
{
    [EffectCategory(EffectCategory.Adjustment)]
    public sealed class HueAndSaturationAdjustment
        : InternalPropertyBasedEffect
    {
        public static string StaticName
        {
            get
            {
                return PdnResources.GetString("HueAndSaturationAdjustment.Name");
            }
        }

        public static Image StaticImage
        {
            get
            {
                return PdnResources.GetImageResource("Icons.HueAndSaturationAdjustment.png").Reference;
            }
        }

        public HueAndSaturationAdjustment()
            : base(StaticName,
                   StaticImage,
                   null,
                   EffectFlags.Configurable)
        {
        }

        public enum PropertyNames
        {
            Hue = 0,
            Saturation = 1,
            Lightness = 2
        }

        protected override PropertyCollection OnCreatePropertyCollection()
        {
            List<Property> props = new List<Property>();

            props.Add(new Int32Property(PropertyNames.Hue, 0, -180, +180));
            props.Add(new Int32Property(PropertyNames.Saturation, 100, 0, 200));
            props.Add(new Int32Property(PropertyNames.Lightness, 0, -100, +100));

            return new PropertyCollection(props);
        }

        private int hue;
        private int saturation;
        private int lightness;
        private UnaryPixelOp pixelOp;

        protected override void OnSetRenderInfo(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            this.hue = newToken.GetProperty<Int32Property>(PropertyNames.Hue).Value;
            this.saturation = newToken.GetProperty<Int32Property>(PropertyNames.Saturation).Value;
            this.lightness = newToken.GetProperty<Int32Property>(PropertyNames.Lightness).Value;

            // map the range [0,100] -> [0,100] and the range [101,200] -> [103,400]
            if (this.saturation > 100)
            {
                this.saturation = ((this.saturation - 100) * 3) + 100;
            }

            if (this.hue == 0 && this.saturation == 100 && this.lightness == 0)
            {
                this.pixelOp = new UnaryPixelOps.Identity();
            }
            else
            {
                this.pixelOp = new UnaryPixelOps.HueSaturationLightness(this.hue, this.saturation, this.lightness);
            }

            base.OnSetRenderInfo(newToken, dstArgs, srcArgs);
        }

        protected override ControlInfo OnCreateConfigUI(PropertyCollection props)
        {
            ControlInfo configUI = CreateDefaultConfigUI(props);

            configUI.SetPropertyControlValue(PropertyNames.Hue, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("HueAndSaturationAdjustment.Amount1Label"));
            configUI.SetPropertyControlValue(PropertyNames.Saturation, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("HueAndSaturationAdjustment.Amount2Label"));
            configUI.SetPropertyControlValue(PropertyNames.Lightness, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("HueAndSaturationAdjustment.Amount3Label"));

            return configUI;
        }

        protected override void OnRender(Rectangle[] rois, int startIndex, int length)
        {
            Surface dst = DstArgs.Surface;
            Surface src = SrcArgs.Surface;

            this.pixelOp.Apply(dst, src, rois, startIndex, length);
        }
    }
}
