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
    public sealed class PencilSketchEffect
        : InternalPropertyBasedEffect
    {
        public enum PropertyNames
        {
            PencilTipSize = 0,
            ColorRange = 1
        }

        private static string StaticName
        {
            get
            {
                return PdnResources.GetString("PencilSketchEffect.Name");
            }
        }

        private static ImageResource StaticIcon
        {
            get
            {
                return PdnResources.GetImageResource("Icons.PencilSketchEffectIcon.png");
            }
        }

        private GaussianBlurEffect blurEffect;
        private PropertyCollection blurProps;

        private UnaryPixelOps.Desaturate desaturateOp = new UnaryPixelOps.Desaturate();

        private DesaturateEffect desaturateEffect = new DesaturateEffect();
        private InvertColorsEffect invertEffect = new InvertColorsEffect();

        private BrightnessAndContrastAdjustment bacAdjustment = new BrightnessAndContrastAdjustment();
        private PropertyCollection bacProps;

        private UserBlendOps.ColorDodgeBlendOp colorDodgeOp = new UserBlendOps.ColorDodgeBlendOp();

        protected override PropertyCollection OnCreatePropertyCollection()
        {
            List<Property> props = new List<Property>();

            props.Add(new Int32Property(PropertyNames.PencilTipSize, 2, 1, 20));
            props.Add(new Int32Property(PropertyNames.ColorRange, 0, -20, +20));

            return new PropertyCollection(props);
        }

        protected override ControlInfo OnCreateConfigUI(PropertyCollection props)
        {
            ControlInfo configUI = CreateDefaultConfigUI(props);

            configUI.SetPropertyControlValue(PropertyNames.PencilTipSize, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("PencilSketchEffect.ConfigDialog.PencilTipSizeLabel"));
            configUI.SetPropertyControlValue(PropertyNames.ColorRange, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("PencilSketchEffect.ConfigDialog.RangeLabel"));

            return configUI;
        }

        private int pencilTipSize;
        private int colorRange;

        protected override void OnSetRenderInfo(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            this.pencilTipSize = newToken.GetProperty<Int32Property>(PropertyNames.PencilTipSize).Value;
            this.colorRange = newToken.GetProperty<Int32Property>(PropertyNames.ColorRange).Value;

            PropertyBasedEffectConfigToken blurToken = new PropertyBasedEffectConfigToken(this.blurProps);
            blurToken.SetPropertyValue(GaussianBlurEffect.PropertyNames.Radius, this.pencilTipSize);
            this.blurEffect.SetRenderInfo(blurToken, dstArgs, srcArgs);

            PropertyBasedEffectConfigToken bacToken = new PropertyBasedEffectConfigToken(this.bacProps);
            bacToken.SetPropertyValue(BrightnessAndContrastAdjustment.PropertyNames.Brightness, this.colorRange);
            bacToken.SetPropertyValue(BrightnessAndContrastAdjustment.PropertyNames.Contrast, -this.colorRange);
            this.bacAdjustment.SetRenderInfo(bacToken, dstArgs, dstArgs);

            this.desaturateEffect.SetRenderInfo(null, dstArgs, dstArgs);

            this.invertEffect.SetRenderInfo(null, dstArgs, dstArgs);

            base.OnSetRenderInfo(newToken, dstArgs, srcArgs);
        }

        protected override unsafe void OnRender(Rectangle[] rois, int startIndex, int length)
        {
            this.blurEffect.Render(rois, startIndex, length);
            this.bacAdjustment.Render(rois, startIndex, length);
            this.invertEffect.Render(rois, startIndex, length);
            this.desaturateEffect.Render(rois, startIndex, length);

            for (int i = startIndex; i < startIndex + length; ++i)
            {
                Rectangle roi = rois[i];

                for (int y = roi.Top; y < roi.Bottom; ++y)
                {
                    ColorBgra* srcPtr = SrcArgs.Surface.GetPointAddressUnchecked(roi.X, y);
                    ColorBgra* dstPtr = DstArgs.Surface.GetPointAddressUnchecked(roi.X, y);

                    for (int x = roi.Left; x < roi.Right; ++x)
                    {
                        ColorBgra srcGrey = this.desaturateOp.Apply(*srcPtr);
                        ColorBgra sketched = this.colorDodgeOp.Apply(srcGrey, *dstPtr);
                        *dstPtr = sketched;

                        ++srcPtr;
                        ++dstPtr;
                    }
                }
            }
        }

        public PencilSketchEffect()
            : base(StaticName, StaticIcon.Reference, SubmenuNames.Artistic, EffectFlags.Configurable)
        {
            this.blurEffect = new GaussianBlurEffect();
            this.blurProps = this.blurEffect.CreatePropertyCollection();

            this.bacAdjustment = new BrightnessAndContrastAdjustment();
            this.bacProps = this.bacAdjustment.CreatePropertyCollection();
        }
    }
}
