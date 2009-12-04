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

using System;
using System.Collections.Generic;
using System.Drawing;
using PaintDotNet.IndirectUI;
using PaintDotNet.PropertySystem;

namespace PaintDotNet.Effects
{
    public sealed class FragmentEffect
        : InternalPropertyBasedEffect
    {
        public FragmentEffect()
            : base(PdnResources.GetString("FragmentEffect.Name"), 
                   PdnResources.GetImageResource("Icons.FragmentEffectIcon.png").Reference, 
                   SubmenuNames.Blurs, 
                   EffectFlags.Configurable)
        {
        }

        public enum PropertyNames
        {
            Fragments = 0,
            Rotation = 1,
            Distance = 2
        }

        protected override PropertyCollection OnCreatePropertyCollection()
        {
            List<Property> properties = new List<Property>();

            properties.Add(new Int32Property(PropertyNames.Fragments, 4, 2, 50));
            properties.Add(new Int32Property(PropertyNames.Distance, 8, 0, 100));
            properties.Add(new DoubleProperty(PropertyNames.Rotation, 0, 0, 360));

            return new PropertyCollection(properties);
        }

        protected override ControlInfo OnCreateConfigUI(PropertyCollection props)
        {
            ControlInfo configUI = PropertyBasedEffect.CreateDefaultConfigUI(props);

            configUI.SetPropertyControlValue(
                PropertyNames.Fragments, 
                ControlInfoPropertyNames.DisplayName, 
                PdnResources.GetString("FragmentEffect.ConfigDialog.Fragments.DisplayName"));

            configUI.SetPropertyControlValue(
                PropertyNames.Distance, 
                ControlInfoPropertyNames.DisplayName,
                PdnResources.GetString("FragmentEffect.ConfigDialog.Distance.DisplayName"));

            configUI.SetPropertyControlValue(
                PropertyNames.Rotation,
                ControlInfoPropertyNames.DisplayName,
                PdnResources.GetString("FragmentEffect.ConfigDialog.Rotation.DisplayName"));

            configUI.SetPropertyControlType(PropertyNames.Rotation, PropertyControlType.AngleChooser);

            return configUI;
        }

        protected override void OnSetRenderInfo(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            int fragments = newToken.GetProperty<Int32Property>(PropertyNames.Fragments).Value;
            double rotation = newToken.GetProperty<DoubleProperty>(PropertyNames.Rotation).Value;
            int distance = newToken.GetProperty<Int32Property>(PropertyNames.Distance).Value;

            RecalcPointOffsets(fragments, rotation, distance);

            base.OnSetRenderInfo(newToken, dstArgs, srcArgs);
        }

        private void RecalcPointOffsets(int fragments, double rotationAngle, int distance)
        {
            double pointStep = 2 * Math.PI / (double)fragments;
            double rotationRadians = ((rotationAngle - 90.0) * Math.PI) / 180.0;
            double offsetAngle = pointStep;

            this.pointOffsets = new Point[fragments];

            for (int i = 0; i < fragments; i++)
            {
                double currentRadians = rotationRadians + (pointStep * i);

                this.pointOffsets[i] = new Point(
                    (int)Math.Round(distance * -Math.Sin(currentRadians), MidpointRounding.AwayFromZero),
                    (int)Math.Round(distance * -Math.Cos(currentRadians), MidpointRounding.AwayFromZero));
            }
        }

        private Point[] pointOffsets = null;

        protected override unsafe void OnRender(Rectangle[] renderRects, int startIndex, int length)
        {
            Surface dst = DstArgs.Surface;
            Surface src = SrcArgs.Surface;

            int poLength = this.pointOffsets.Length;
            Point *pointOffsets = stackalloc Point[poLength];
            for (int i = 0; i < poLength; ++i)
            {
                pointOffsets[i] = this.pointOffsets[i];
            }

            ColorBgra* samples = stackalloc ColorBgra[poLength];

            for (int n = startIndex; n < startIndex + length; ++n)
            {
                Rectangle rect = renderRects[n];

                for (int y = rect.Top; y < rect.Bottom; y++)
                {
                    ColorBgra* dstPtr = dst.GetPointAddressUnchecked(rect.Left, y);

                    for (int x = rect.Left; x < rect.Right; x++)
                    {
                        int sampleCount = 0;

                        for (int i = 0; i < poLength; ++i)
                        {
                            int u = x - pointOffsets[i].X;
                            int v = y - pointOffsets[i].Y;

                            if (u >= 0 && u < src.Bounds.Width && v >= 0 && v < src.Bounds.Height)
                            {
                                samples[sampleCount] = src.GetPointUnchecked(u, v);
                                ++sampleCount;
                            }
                        }

                        *dstPtr = ColorBgra.Blend(samples, sampleCount);
                        ++dstPtr;
                    }
                }
            }
        }
    }
}
