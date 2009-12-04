/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet;
using PaintDotNet.Effects;
using PaintDotNet.IndirectUI;
using PaintDotNet.PropertySystem;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace PaintDotNet.Effects
{
    public sealed class GlowEffect
        : InternalPropertyBasedEffect
    {
        public static string StaticName
        {
            get
            {
                return PdnResources.GetString("GlowEffect.Name");
            }
        }

        public static Image StaticImage
        {
            get
            {
                return PdnResources.GetImageResource("Icons.GlowEffect.png").Reference;
            }
        }

        public enum PropertyNames
        {
            Radius = 0,
            Brightness = 1,
            Contrast = 2
        }

        private GaussianBlurEffect blurEffect = new GaussianBlurEffect();
        private PropertyCollection blurProps;
        private BrightnessAndContrastAdjustment bcAdjustment = new BrightnessAndContrastAdjustment();
        private PropertyCollection bcProps;

        private UserBlendOps.ScreenBlendOp screenBlendOp = new UserBlendOps.ScreenBlendOp();

        protected override PropertyCollection OnCreatePropertyCollection()
        {
            List<Property> props = new List<Property>();

            props.Add(new Int32Property(PropertyNames.Radius, 6, 1, 20));
            props.Add(new Int32Property(PropertyNames.Brightness, 10, -100, +100));
            props.Add(new Int32Property(PropertyNames.Contrast, 10, -100, +100));

            return new PropertyCollection(props);
        }

        protected override void OnSetRenderInfo(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            PropertyCollection blurValues = this.blurProps.Clone();
            blurValues[GaussianBlurEffect.PropertyNames.Radius].Value = newToken.GetProperty<Int32Property>(PropertyNames.Radius).Value;
            PropertyBasedEffectConfigToken blurToken = new PropertyBasedEffectConfigToken(blurValues);
            this.blurEffect.SetRenderInfo(blurToken, dstArgs, srcArgs);

            PropertyCollection bcValues = this.bcProps.Clone();

            bcValues[BrightnessAndContrastAdjustment.PropertyNames.Brightness].Value =
                newToken.GetProperty<Int32Property>(PropertyNames.Brightness).Value;

            bcValues[BrightnessAndContrastAdjustment.PropertyNames.Contrast].Value =
                newToken.GetProperty<Int32Property>(PropertyNames.Contrast).Value;

            PropertyBasedEffectConfigToken bcToken = new PropertyBasedEffectConfigToken(bcValues);
            this.bcAdjustment.SetRenderInfo(bcToken, dstArgs, dstArgs); // have to do adjustment in place, hence dstArgs for both 'args' parameters

            base.OnSetRenderInfo(newToken, dstArgs, srcArgs);
        }

        protected override unsafe void OnRender(
            Rectangle[] rois, 
            int startIndex, 
            int length)
        {
            // First we blur the source, and write the result to the destination surface
            // Then we apply Brightness/Contrast with the input as the dst, and the output as the dst
            // Third, we apply the Screen blend operation so that dst = dst OVER src

            this.blurEffect.Render(rois, startIndex, length);
            this.bcAdjustment.Render(rois, startIndex, length);

            for (int i = startIndex; i < startIndex + length; ++i)
            {
                Rectangle roi = rois[i];

                for (int y = roi.Top; y < roi.Bottom; ++y)
                {
                    ColorBgra* dstPtr = DstArgs.Surface.GetPointAddressUnchecked(roi.Left, y);
                    ColorBgra* srcPtr = SrcArgs.Surface.GetPointAddressUnchecked(roi.Left, y);

                    screenBlendOp.Apply(dstPtr, srcPtr, dstPtr, roi.Width);
                }
            }
        }

        protected override ControlInfo OnCreateConfigUI(PropertyCollection props)
        {
            ControlInfo configUI = CreateDefaultConfigUI(props);

            configUI.SetPropertyControlValue(PropertyNames.Radius, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("GlowEffect.Amount1.Name"));
            configUI.SetPropertyControlValue(PropertyNames.Brightness, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("GlowEffect.Amount2.Name"));
            configUI.SetPropertyControlValue(PropertyNames.Contrast, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("GlowEffect.Amount3.Name"));

            return configUI;
        }

        public GlowEffect()
            : base(StaticName, StaticImage, SubmenuNames.Photo, EffectFlags.Configurable)
        {
            this.blurEffect = new GaussianBlurEffect();
            this.blurProps = this.blurEffect.CreatePropertyCollection();

            this.bcAdjustment = new BrightnessAndContrastAdjustment();
            this.bcProps = this.bcAdjustment.CreatePropertyCollection();

            this.screenBlendOp = new UserBlendOps.ScreenBlendOp();
        }
    }
}
