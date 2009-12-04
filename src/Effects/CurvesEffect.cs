/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PaintDotNet.Effects
{
    [EffectCategory(EffectCategory.Adjustment)]
    public sealed class CurvesEffect
        : Effect
    {
        public CurvesEffect()
            : base(PdnResources.GetString("CurvesEffect.Name"),
                   PdnResources.GetImageResource("Icons.CurvesEffect.png").Reference,
                   EffectFlags.Configurable)
        {
        }

        public override void Render(EffectConfigToken parameters, RenderArgs dstArgs, RenderArgs srcArgs, Rectangle[] rois, int startIndex, int length)
        {
            CurvesEffectConfigToken token = parameters as CurvesEffectConfigToken;

            if (token != null)
            {
                UnaryPixelOp uop = token.Uop;

                for (int i = startIndex; i < startIndex + length; ++i)
                {
                    uop.Apply(dstArgs.Surface, srcArgs.Surface, rois[i]);
                }
            }
        }

        public override EffectConfigDialog CreateConfigDialog()
        {
            return new CurvesEffectConfigDialog();
        }
    }
}
