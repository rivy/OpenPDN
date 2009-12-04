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

namespace PaintDotNet.Effects
{
    public sealed class RedEyeRemoveEffect
        : InternalPropertyBasedEffect
    {
        public enum PropertyNames
        {
            Tolerance = 0,
            Saturation = 1
        }

        private int tolerance;
        private int saturation;
        private PixelOp redEyeOp;

        protected override PropertyCollection OnCreatePropertyCollection()
        {
            List<Property> props = new List<Property>();

            props.Add(new Int32Property(PropertyNames.Tolerance, 70, 0, 100));
            props.Add(new Int32Property(PropertyNames.Saturation, 90, 0, 100));

            return new PropertyCollection(props);
        }

        protected override ControlInfo OnCreateConfigUI(PropertyCollection props)
        {
            ControlInfo configUI = CreateDefaultConfigUI(props);

            configUI.SetPropertyControlValue(PropertyNames.Tolerance, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("RedEyeRemoveEffect.ConfigDialog.Amount1Label"));
            configUI.SetPropertyControlValue(PropertyNames.Saturation, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("RedEyeRemoveEffect.ConfigDialog.Amount2Label"));
            configUI.SetPropertyControlValue(PropertyNames.Saturation, ControlInfoPropertyNames.Description, PdnResources.GetString("RedEyeRemoveEffectDialog.UsageHintLabel.Text"));

            return configUI;
        }

        protected override void OnSetRenderInfo(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            this.tolerance = newToken.GetProperty<Int32Property>(PropertyNames.Tolerance).Value;
            this.saturation = newToken.GetProperty<Int32Property>(PropertyNames.Saturation).Value;

            this.redEyeOp = new UnaryPixelOps.RedEyeRemove(this.tolerance, this.saturation);

            base.OnSetRenderInfo(newToken, dstArgs, srcArgs);
        }

        protected override void OnRender(Rectangle[] rois, int startIndex, int length)
        {
            this.redEyeOp.Apply(DstArgs.Surface, SrcArgs.Surface, rois, startIndex, length);
        }

        public RedEyeRemoveEffect()
            : base(PdnResources.GetString("RedEyeRemoveEffect.Name"),
                   PdnResources.GetImageResource("Icons.RedEyeRemoveEffect.png").Reference,
                   SubmenuNames.Photo,
                   EffectFlags.Configurable)
        {
        }
    }
}