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
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PaintDotNet.Effects
{
    [EffectCategory(EffectCategory.Adjustment)]
    public sealed class AutoLevelEffect
        : InternalPropertyBasedEffect
    {
        private UnaryPixelOps.Level levels = null;

        protected override PropertyCollection OnCreatePropertyCollection()
        {
            return PropertyCollection.CreateEmpty();
        }

        protected override void OnSetRenderInfo(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            HistogramRgb histogram = new HistogramRgb();
            histogram.UpdateHistogram(srcArgs.Surface, this.EnvironmentParameters.GetSelection(dstArgs.Bounds));
            this.levels = histogram.MakeLevelsAuto();

            base.OnSetRenderInfo(newToken, dstArgs, srcArgs);
        }

        protected override void OnRender(Rectangle[] rois, int startIndex, int length)
        {
            if (this.levels.isValid)
            {
                this.levels.Apply(DstArgs.Surface, SrcArgs.Surface, rois, startIndex, length);
            }
        }

        public AutoLevelEffect()
            : base(PdnResources.GetString("AutoLevel.Name"),
                   PdnResources.GetImageResource("Icons.AutoLevel.png").Reference,
                   null,
                   EffectFlags.None)
        {
        }
    }
}
