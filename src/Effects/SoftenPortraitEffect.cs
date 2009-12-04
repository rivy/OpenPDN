/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

// This effect was graciously provided by David Issel, aka BoltBait. His original
// copyright and license (MIT License) are reproduced below.

/*
PortraitEffect.cs 
Copyright (c) 2007 David Issel 
Contact Info: BoltBait@hotmail.com http://www.BoltBait.com 

Permission is hereby granted, free of charge, to any person obtaining a copy 
of this software and associated documentation files (the "Software"), to deal 
in the Software without restriction, including without limitation the rights 
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
copies of the Software, and to permit persons to whom the Software is 
furnished to do so, subject to the following conditions: 

The above copyright notice and this permission notice shall be included in 
all copies or substantial portions of the Software. 

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
THE SOFTWARE. 
*/

using PaintDotNet;
using PaintDotNet.Effects;
using PaintDotNet.IndirectUI;
using PaintDotNet.PropertySystem;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace PaintDotNet.Effects
{
    public sealed class SoftenPortraitEffect
        : InternalPropertyBasedEffect
    {
        public enum PropertyNames
        {
            Softness = 0,
            Lighting = 1,
            Warmth = 2
        }

        public static string StaticName
        {
            get
            {
                return PdnResources.GetString("SoftenPortraitEffect.Name");
            }
        }

        public static Image StaticIcon
        {
            get
            {
                return PdnResources.GetImageResource("Icons.SoftenPortraitEffectIcon.png").Reference;
            }
        }

        protected override PropertyCollection OnCreatePropertyCollection()
        {
 	        List<Property> props = new List<Property>();

            props.Add(new Int32Property(PropertyNames.Softness, 5, 0, 10));
            props.Add(new Int32Property(PropertyNames.Lighting, 0, -20, +20));
            props.Add(new Int32Property(PropertyNames.Warmth, 10, 0, 20));

            return new PropertyCollection(props);
        }

        protected override ControlInfo OnCreateConfigUI(PropertyCollection props)
        {
            ControlInfo configUI = CreateDefaultConfigUI(props);

            configUI.SetPropertyControlValue(PropertyNames.Softness, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("SoftenPortraitEffect.ConfigDialog.SoftnessLabel"));
            configUI.SetPropertyControlValue(PropertyNames.Lighting, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("SoftenPortraitEffect.ConfigDialog.LightingLabel"));
            configUI.SetPropertyControlValue(PropertyNames.Warmth, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("SoftenPortraitEffect.ConfigDialog.WarmthLabel"));

 	        return configUI;
        }

        private GaussianBlurEffect blurEffect;
        private PropertyCollection blurProps;
        private UnaryPixelOps.Desaturate desaturateOp;
        private BrightnessAndContrastAdjustment bacAdjustment;
        private PropertyCollection bacProps;
        private UserBlendOps.OverlayBlendOp overlayOp;

        private int softness;
        private int lighting;
        private int warmth;

        protected override void OnSetRenderInfo(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            this.softness = newToken.GetProperty<Int32Property>(PropertyNames.Softness).Value;
            this.lighting = newToken.GetProperty<Int32Property>(PropertyNames.Lighting).Value;
            this.warmth = newToken.GetProperty<Int32Property>(PropertyNames.Warmth).Value;

            PropertyBasedEffectConfigToken blurToken = new PropertyBasedEffectConfigToken(this.blurProps);
            blurToken.SetPropertyValue(GaussianBlurEffect.PropertyNames.Radius, this.softness * 3);
            this.blurEffect.SetRenderInfo(blurToken, dstArgs, srcArgs);

            PropertyBasedEffectConfigToken bacToken = new PropertyBasedEffectConfigToken(this.bacProps);
            bacToken.SetPropertyValue(BrightnessAndContrastAdjustment.PropertyNames.Brightness, this.lighting);
            bacToken.SetPropertyValue(BrightnessAndContrastAdjustment.PropertyNames.Contrast, -this.lighting / 2);
            this.bacAdjustment.SetRenderInfo(bacToken, dstArgs, dstArgs);

            base.OnSetRenderInfo(newToken, dstArgs, srcArgs);
        }

        protected override unsafe void OnRender(Rectangle[] rois, int startIndex, int length)
        {
            float redAdjust = 1.0f + (this.warmth / 100.0f);
            float blueAdjust = 1.0f - (this.warmth / 100.0f);

            this.blurEffect.Render(rois, startIndex, length);
            this.bacAdjustment.Render(rois, startIndex, length);

            for (int i = startIndex; i < startIndex + length; ++i)
            {
                Rectangle roi = rois[i];

                for (int y = roi.Top; y < roi.Bottom; ++y)
                {
                    ColorBgra* srcPtr = SrcArgs.Surface.GetPointAddress(roi.X, y);
                    ColorBgra* dstPtr = DstArgs.Surface.GetPointAddress(roi.X, y);

                    for (int x = roi.Left; x < roi.Right; ++x)
                    {
                        ColorBgra srcGrey = this.desaturateOp.Apply(*srcPtr);

                        srcGrey.R = Utility.ClampToByte((int)((float)srcGrey.R * redAdjust));
                        srcGrey.B = Utility.ClampToByte((int)((float)srcGrey.B * blueAdjust));

                        ColorBgra mypixel = this.overlayOp.Apply(srcGrey, *dstPtr);
                        *dstPtr = mypixel;

                        ++srcPtr;
                        ++dstPtr;
                    }
                }
            }
        }

        public SoftenPortraitEffect()
            : base(StaticName, StaticIcon, SubmenuNames.Photo, EffectFlags.Configurable)
        {
            this.blurEffect = new GaussianBlurEffect();
            this.blurProps = this.blurEffect.CreatePropertyCollection();

            this.desaturateOp = new UnaryPixelOps.Desaturate();

            this.bacAdjustment = new BrightnessAndContrastAdjustment();
            this.bacProps = this.bacAdjustment.CreatePropertyCollection();

            this.overlayOp = new UserBlendOps.OverlayBlendOp();
        }
    }
}