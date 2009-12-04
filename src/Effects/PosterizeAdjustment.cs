/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

// Copyright (c) 2007,2008 Ed Harvey 
//
// MIT License: http://www.opensource.org/licenses/mit-license.php
//
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal 
// in the Software without restriction, including without limitation the rights 
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions: 
//
// The above copyright notice and this permission notice shall be included in 
// all copies or substantial portions of the Software. 
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
// THE SOFTWARE. 
//

using System.Collections.Generic;
using System.Drawing;
using PaintDotNet.IndirectUI;
using PaintDotNet.PropertySystem;

namespace PaintDotNet.Effects
{
    [EffectCategory(EffectCategory.Adjustment)]
    public sealed class PosterizeAdjustment
        : InternalPropertyBasedEffect
    {
        private class PosterizePixelOp
            : UnaryPixelOp
        {
            private byte[] redLevels;
            private byte[] greenLevels;
            private byte[] blueLevels;

            public PosterizePixelOp(int red, int green, int blue)
            {
                this.redLevels = CalcLevels(red);
                this.greenLevels = CalcLevels(green);
                this.blueLevels = CalcLevels(blue);
            }

            private static byte[] CalcLevels(int levelCount)
            {
                byte[] t1 = new byte[levelCount];

                for (int i = 1; i < levelCount; i++)
                {
                    t1[i] = (byte)((255 * i) / (levelCount - 1));
                }

                byte[] levels = new byte[256];

                int j = 0;
                int k = 0;

                for (int i = 0; i < 256; i++)
                {
                    levels[i] = t1[j];

                    k += levelCount;

                    if (k > 255)
                    {
                        k -= 255;
                        j++;
                    }
                }

                return levels;
            }

            public override ColorBgra Apply(ColorBgra color)
            {
                return ColorBgra.FromBgra(blueLevels[color.B], greenLevels[color.G], redLevels[color.R], color.A);
            }

            public unsafe override void Apply(ColorBgra* ptr, int length)
            {
                while (length > 0)
                {
                    ptr->B = this.blueLevels[ptr->B];
                    ptr->G = this.greenLevels[ptr->G];
                    ptr->R = this.redLevels[ptr->R];

                    ++ptr;
                    --length;
                }
            }

            public unsafe override void Apply(ColorBgra* dst, ColorBgra* src, int length)
            {
                while (length > 0)
                {
                    dst->B = this.blueLevels[src->B];
                    dst->G = this.greenLevels[src->G];
                    dst->R = this.redLevels[src->R];
                    dst->A = src->A;

                    ++dst;
                    ++src;
                    --length;
                }
            }
        }

        private UnaryPixelOp op;

        public PosterizeAdjustment()
            : base(PdnResources.GetString("PosterizeAdjustment.Name"),
                   PdnResources.GetImageResource("Icons.PosterizeEffectIcon.png").Reference,
                   null,
                   EffectFlags.Configurable)
        {
        }

        public enum PropertyNames
        {
            RedLevels = 0,
            GreenLevels = 1,
            BlueLevels = 2,
            LinkLevels = 3
        }

        protected override PropertyCollection OnCreatePropertyCollection()
        {
            List<Property> props = new List<Property>();

            props.Add(new Int32Property(PropertyNames.RedLevels, 16, 2, 64));
            props.Add(new Int32Property(PropertyNames.GreenLevels, 16, 2, 64));
            props.Add(new Int32Property(PropertyNames.BlueLevels, 16, 2, 64));
            props.Add(new BooleanProperty(PropertyNames.LinkLevels, true));

            List<PropertyCollectionRule> rules = new List<PropertyCollectionRule>();

            rules.Add(new LinkValuesBasedOnBooleanRule<int, Int32Property>(
                new object[] { PropertyNames.RedLevels, PropertyNames.GreenLevels, PropertyNames.BlueLevels }, 
                PropertyNames.LinkLevels, 
                false));

            return new PropertyCollection(props, rules);
        }

        protected override ControlInfo OnCreateConfigUI(PropertyCollection props)
        {
            ControlInfo info = PropertyBasedEffect.CreateDefaultConfigUI(props);

            info.SetPropertyControlValue(
                PropertyNames.RedLevels, 
                ControlInfoPropertyNames.DisplayName, 
                PdnResources.GetString("PosterizeAdjustment.ConfigDialog.RedLevels.DisplayName"));

            info.SetPropertyControlValue(
                PropertyNames.GreenLevels, 
                ControlInfoPropertyNames.DisplayName,
                PdnResources.GetString("PosterizeAdjustment.ConfigDialog.GreenLevels.DisplayName"));

            info.SetPropertyControlValue(
                PropertyNames.BlueLevels, 
                ControlInfoPropertyNames.DisplayName,
                PdnResources.GetString("PosterizeAdjustment.ConfigDialog.BlueLevels.DisplayName"));

            info.SetPropertyControlValue(
                PropertyNames.LinkLevels,
                ControlInfoPropertyNames.DisplayName,
                string.Empty);

            info.SetPropertyControlValue(
                PropertyNames.LinkLevels,
                ControlInfoPropertyNames.Description,
                PdnResources.GetString("PosterizeAdjustment.ConfigDialog.LinkLevels.Description"));

            return info;
        }

        protected override void OnSetRenderInfo(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            int red = newToken.GetProperty<Int32Property>(PropertyNames.RedLevels).Value;
            int green = newToken.GetProperty<Int32Property>(PropertyNames.GreenLevels).Value;
            int blue = newToken.GetProperty<Int32Property>(PropertyNames.BlueLevels).Value;

            this.op = new PosterizePixelOp(red, green, blue);

            base.OnSetRenderInfo(newToken, dstArgs, srcArgs);
        }

        protected unsafe override void OnRender(Rectangle[] renderRects, int startIndex, int length)
        {
            this.op.Apply(DstArgs.Surface, SrcArgs.Surface, renderRects, startIndex, length);
        }
    }
}
