/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.PropertySystem;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PaintDotNet.Effects
{
    [EffectCategory(EffectCategory.Adjustment)]
    public sealed class DesaturateEffect
        : InternalPropertyBasedEffect
    {
        private UnaryPixelOps.Desaturate desaturateOp;

        protected override PropertyCollection OnCreatePropertyCollection()
        {
            return PropertyCollection.CreateEmpty();
        }

        protected override void OnRender(Rectangle[] rois, int startIndex, int length)
        {
            this.desaturateOp.Apply(DstArgs.Surface, SrcArgs.Surface, rois, startIndex, length);
        }

        public DesaturateEffect()
            : base(PdnResources.GetString("DesaturateEffect.Name"),
                   PdnResources.GetImageResource("Icons.DesaturateEffect.png").Reference,
                   null,
                   EffectFlags.None)
        {
            this.desaturateOp = new UnaryPixelOps.Desaturate();
        }
    }
}
