/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.PropertySystem;
using PaintDotNet.SystemLayer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PaintDotNet.Effects
{
    [Obsolete("This class is obsolete, and exists only for compatibility with some legacy plugins. Use GaussianBlurEffect instead.")]
    public sealed class BlurEffect
        : Effect
    {
        private GaussianBlurEffect gbEffect;
        private PropertyCollection gbProps;
        private PropertyBasedEffectConfigToken gbToken;

        protected override void OnSetRenderInfo(EffectConfigToken parameters, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            lock (this)
            {
                this.gbToken = new PropertyBasedEffectConfigToken(this.gbProps);
                this.gbToken.SetPropertyValue(GaussianBlurEffect.PropertyNames.Radius, ((AmountEffectConfigToken)parameters).Amount);
                this.gbEffect.SetRenderInfo(this.gbToken, dstArgs, srcArgs);
            }

            base.OnSetRenderInfo(parameters, dstArgs, srcArgs);
        }

        public override void Render(EffectConfigToken parameters, RenderArgs dstArgs, RenderArgs srcArgs, Rectangle[] rois, int startIndex, int length)
        {
            lock (this)
            {
                SetRenderInfo(parameters, dstArgs, srcArgs);
            }

            this.gbEffect.Render(this.gbToken, dstArgs, srcArgs, rois, startIndex, length);
        }

        public override EffectConfigDialog CreateConfigDialog()
        {
            throw new NotImplementedException();
        }

        public BlurEffect()
            : base(GaussianBlurEffect.StaticName + " -- Obsolete",
                   null,
                   null,
                   EffectFlags.Configurable)
        {
            this.gbEffect = new GaussianBlurEffect();
            this.gbProps = this.gbEffect.CreatePropertyCollection();
        }    
    }
}
