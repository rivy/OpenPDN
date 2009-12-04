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
    public sealed class SepiaEffect
        : InternalPropertyBasedEffect
    {
        private UnaryPixelOp levels;
        private UnaryPixelOp desaturate;

        protected override PropertyCollection OnCreatePropertyCollection()
        {
            return PropertyCollection.CreateEmpty();
        }

        protected override void OnRender(Rectangle[] rois, int startIndex, int length)
        {
            this.desaturate.Apply(DstArgs.Surface, SrcArgs.Surface, rois, startIndex, length);
            this.levels.Apply(DstArgs.Surface, DstArgs.Surface, rois, startIndex, length);
        }

        public SepiaEffect()
            : base(PdnResources.GetString("SepiaEffect.Name"),
                   PdnResources.GetImageResource("Icons.SepiaEffect.png").Reference,
                   null,
                   EffectFlags.None)
        {
            this.desaturate = new UnaryPixelOps.Desaturate();

            this.levels = new UnaryPixelOps.Level(
                ColorBgra.Black, 
                ColorBgra.White,
                new float[] { 1.2f, 1.0f, 0.8f },
                ColorBgra.Black,
                ColorBgra.White);
        }
    }
}
