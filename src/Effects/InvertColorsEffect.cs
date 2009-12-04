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
using System.Drawing;
using System.Windows.Forms;

namespace PaintDotNet.Effects
{
    [EffectCategory(EffectCategory.Adjustment)]
    public sealed class InvertColorsEffect
        : InternalPropertyBasedEffect
    {
        private UnaryPixelOps.Invert invertOp;

        protected override PropertyCollection OnCreatePropertyCollection()
        {
            return PropertyCollection.CreateEmpty();
        }

        protected override void OnRender(Rectangle[] rois, int startIndex, int length)
        {
            this.invertOp.Apply(DstArgs.Surface, SrcArgs.Surface, rois, startIndex, length);
        }

        public InvertColorsEffect()
            : base(PdnResources.GetString("InvertColorsEffect.Name"),
                   PdnResources.GetImageResource("Icons.InvertColorsEffect.png").Reference,
                   null,
                   EffectFlags.None)
        {
            this.invertOp = new UnaryPixelOps.Invert();
        }
    }
}
