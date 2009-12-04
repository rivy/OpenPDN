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
InkSketchEffect.cs 
Copyright (c) 2007 David Issel 
Contact info: BoltBait@hotmail.com http://www.BoltBait.com 

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
    public sealed class InkSketchEffect
        : InternalPropertyBasedEffect
    {
        public enum PropertyNames
        {
            InkOutline = 0,
            Coloring = 1
        }

        public static string StaticName
        {
            get
            {
                return PdnResources.GetString("InkSketchEffect.Name");
            }
        }

        public static Image StaticIcon
        {
            get
            {
                return PdnResources.GetImageResource("Icons.InkSketchEffectIcon.png").Reference;
            }
        }

        public InkSketchEffect()
            : base(StaticName, StaticIcon, SubmenuNames.Artistic, EffectFlags.Configurable)
        {
            this.glowEffect = new GlowEffect();
            this.glowProps = this.glowEffect.CreatePropertyCollection();
        }

        static InkSketchEffect()
        {
            conv = new int[5][];

            for (int i = 0; i < conv.Length; ++i)
            {
                conv[i] = new int[5];
            }

            conv[0] = new int[] { -1, -1, -1, -1, -1 };
            conv[1] = new int[] { -1, -1, -1, -1, -1 };
            conv[2] = new int[] { -1, -1, 30, -1, -1 };
            conv[3] = new int[] { -1, -1, -1, -1, -1 };
            conv[4] = new int[] { -1, -1, -5, -1, -1 };
        }

        private static readonly int[][] conv;
        private const int size = 5;
        private const int radius = (size - 1) / 2;

        private UnaryPixelOps.Desaturate desaturateOp = new UnaryPixelOps.Desaturate();
        private GlowEffect glowEffect;
        private PropertyCollection glowProps;
        private UserBlendOps.DarkenBlendOp darkenOp = new UserBlendOps.DarkenBlendOp();

        private int inkOutline;
        private int coloring;

        protected override PropertyCollection OnCreatePropertyCollection()
        {
            List<Property> props = new List<Property>();

            props.Add(new Int32Property(PropertyNames.InkOutline, 50, 0, 99));
            props.Add(new Int32Property(PropertyNames.Coloring, 50, 0, 100));

            return new PropertyCollection(props);
        }

        protected override ControlInfo OnCreateConfigUI(PropertyCollection props)
        {
            ControlInfo configUI = CreateDefaultConfigUI(props);

            configUI.SetPropertyControlValue(PropertyNames.InkOutline, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("InkSketchEffect.ConfigDialog.InkOutlineLabel"));
            configUI.SetPropertyControlValue(PropertyNames.Coloring, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("InkSketchEffect.ConfigDialog.ColoringLabel"));

            return configUI;
        }

        protected override void OnSetRenderInfo(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            this.inkOutline = newToken.GetProperty<Int32Property>(PropertyNames.InkOutline).Value;
            this.coloring = newToken.GetProperty<Int32Property>(PropertyNames.Coloring).Value;

            PropertyBasedEffectConfigToken glowToken = new PropertyBasedEffectConfigToken(this.glowProps);
            glowToken.SetPropertyValue(GlowEffect.PropertyNames.Radius, 6);
            glowToken.SetPropertyValue(GlowEffect.PropertyNames.Brightness, -(this.coloring - 50) * 2);
            glowToken.SetPropertyValue(GlowEffect.PropertyNames.Contrast, -(this.coloring - 50) * 2);
            this.glowEffect.SetRenderInfo(glowToken, dstArgs, srcArgs);

            base.OnSetRenderInfo(newToken, dstArgs, srcArgs);
        }

        protected override unsafe void OnRender(Rectangle[] rois, int startIndex, int length)
        {
            // Glow backgound 
            this.glowEffect.Render(rois, startIndex, length);

            // Create black outlines by finding the edges of objects 

            for (int i = startIndex; i < startIndex + length; ++i)
            {
                Rectangle roi = rois[i];

                for (int y = roi.Top; y < roi.Bottom; ++y)
                {
                    int top = y - radius;
                    int bottom = y + radius + 1;

                    if (top < 0)
                    {
                        top = 0;
                    }

                    if (bottom > DstArgs.Height)
                    {
                        bottom = DstArgs.Height;
                    }

                    ColorBgra* srcPtr = SrcArgs.Surface.GetPointAddress(roi.X, y);
                    ColorBgra* dstPtr = DstArgs.Surface.GetPointAddress(roi.X, y);

                    for (int x = roi.Left; x < roi.Right; ++x)
                    {
                        int left = x - radius;
                        int right = x + radius + 1;

                        if (left < 0)
                        {
                            left = 0;
                        }

                        if (right > DstArgs.Width)
                        {
                            right = DstArgs.Width;
                        }

                        int r = 0;
                        int g = 0;
                        int b = 0;

                        for (int v = top; v < bottom; v++)
                        {
                            ColorBgra* pRow = SrcArgs.Surface.GetRowAddress(v);
                            int j = v - y + radius;

                            for (int u = left; u < right; u++)
                            {
                                int i1 = u - x + radius;
                                int w = conv[j][i1];

                                ColorBgra* pRef = pRow + u;

                                r += pRef->R * w;
                                g += pRef->G * w;
                                b += pRef->B * w;
                            }
                        }

                        ColorBgra topLayer = ColorBgra.FromBgr(
                            Utility.ClampToByte(b),
                            Utility.ClampToByte(g),
                            Utility.ClampToByte(r));

                        // Desaturate 
                        topLayer = this.desaturateOp.Apply(topLayer);

                        // Adjust Brightness and Contrast 
                        if (topLayer.R > (this.inkOutline * 255 / 100))
                        {
                            topLayer = ColorBgra.FromBgra(255, 255, 255, topLayer.A);
                        }
                        else
                        {
                            topLayer = ColorBgra.FromBgra(0, 0, 0, topLayer.A);
                        }

                        // Change Blend Mode to Darken 
                        ColorBgra myPixel = this.darkenOp.Apply(topLayer, *dstPtr);
                        *dstPtr = myPixel;

                        ++srcPtr;
                        ++dstPtr;
                    }
                }
            }
        }
    }
}