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
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PaintDotNet.Effects
{
    public sealed class AddNoiseEffect
        : InternalPropertyBasedEffect
    {
        public static string StaticName
        {
            get
            {
                return PdnResources.GetString("AddNoiseEffect.Name");
            }
        }

        public static Image StaticImage
        {
            get
            {
                return PdnResources.GetImageResource("Icons.AddNoiseEffect.png").Reference;
            }
        }

        static AddNoiseEffect()
        {
            InitLookup();
        }

        public AddNoiseEffect()
            : base(StaticName, StaticImage, SubmenuNames.Noise, EffectFlags.Configurable)
        {
        }

        public enum PropertyNames
        {
            Intensity = 0,
            Saturation = 1,
            Coverage = 2
        }

        protected override PropertyCollection OnCreatePropertyCollection()
        {
            List<Property> props = new List<Property>();

            props.Add(new Int32Property(PropertyNames.Intensity, 64, 0, 100));
            props.Add(new Int32Property(PropertyNames.Saturation, 100, 0, 400));
            props.Add(new DoubleProperty(PropertyNames.Coverage, 100, 0, 100));

            return new PropertyCollection(props);
        }

        protected override ControlInfo OnCreateConfigUI(PropertyCollection props)
        {
            ControlInfo configUI = CreateDefaultConfigUI(props);

            configUI.SetPropertyControlValue(PropertyNames.Intensity, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("AddNoiseEffect.Amount1Label"));
            configUI.SetPropertyControlValue(PropertyNames.Saturation, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("AddNoiseEffect.Amount2Label"));
            configUI.SetPropertyControlValue(PropertyNames.Coverage, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("AddNoiseEffect.Coverage"));
            configUI.SetPropertyControlValue(PropertyNames.Coverage, ControlInfoPropertyNames.UseExponentialScale, true);

            return configUI;
        }

        private int intensity;
        private int saturation;
        private double coverage;

        protected override void OnSetRenderInfo(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            this.intensity = newToken.GetProperty<Int32Property>(PropertyNames.Intensity).Value;
            this.saturation = newToken.GetProperty<Int32Property>(PropertyNames.Saturation).Value;
            this.coverage = 0.01 * newToken.GetProperty<DoubleProperty>(PropertyNames.Coverage).Value;

            base.OnSetRenderInfo(newToken, dstArgs, srcArgs);
        }

        private const int tableSize = 16384;
        private static int[] lookup;

        private static double NormalCurve(double x, double scale)
        {
            return scale * Math.Exp(-x * x / 2);
        }

        private static void InitLookup()
        {
            int[] curve = new int[tableSize];
            int[] integral = new int[tableSize];

            double l = 5;
            double r = 10;
            double scale = 50;
            double sum = 0;

            while (r - l > 0.0000001)
            {
                sum = 0;
                scale = (l + r) * 0.5;

                for (int i = 0; i < tableSize; ++i)
                {
                    sum += NormalCurve(16.0 * ((double)i - tableSize / 2) / tableSize, scale);

                    if (sum > 1000000)
                    {
                        break;
                    }
                }

                if (sum > tableSize)
                {
                    r = scale;
                }
                else if (sum < tableSize)
                {
                    l = scale;
                }
                else
                {
                    break;
                }
            }

            lookup = new int[tableSize];
            sum = 0;
            int roundedSum = 0, lastRoundedSum;

            for (int i = 0; i < tableSize; ++i)
            {
                sum += NormalCurve(16.0 * ((double)i - tableSize / 2) / tableSize, scale);
                lastRoundedSum = roundedSum;
                roundedSum = (int)sum;

                for (int j = lastRoundedSum; j < roundedSum; ++j)
                {
                    lookup[j] = (i - tableSize / 2) * 65536 / tableSize;
                }
            }
        }

        [ThreadStatic]
        private static Random threadRand = new Random();

        protected override unsafe void OnRender(Rectangle[] rois, int startIndex, int length)
        {
            int dev = this.intensity * this.intensity / 4;
            int sat = this.saturation * 4096 / 100;

            if (threadRand == null)
            {
                threadRand = new Random(unchecked(System.Threading.Thread.CurrentThread.GetHashCode() ^ 
                    unchecked((int)DateTime.Now.Ticks)));
            }

            Random localRand = threadRand;
            int[] localLookup = lookup;

            for (int ri = startIndex; ri < startIndex + length; ++ri)
            {
                Rectangle rect = rois[ri];

                for (int y = rect.Top; y < rect.Bottom; ++y)
                {
                    ColorBgra *srcPtr = SrcArgs.Surface.GetPointAddressUnchecked(rect.Left, y);
                    ColorBgra *dstPtr = DstArgs.Surface.GetPointAddressUnchecked(rect.Left, y);

                    for (int x = 0; x < rect.Width; ++x)
                    {
                        if (localRand.NextDouble() > this.coverage)
                        {
                            *dstPtr = *srcPtr;
                        }
                        else
                        {
                            int r;
                            int g;
                            int b;
                            int i;

                            r = localLookup[localRand.Next(tableSize)];
                            g = localLookup[localRand.Next(tableSize)];
                            b = localLookup[localRand.Next(tableSize)];

                            i = (4899 * r + 9618 * g + 1867 * b) >> 14; 

                            r = i + (((r - i) * sat) >> 12);
                            g = i + (((g - i) * sat) >> 12);
                            b = i + (((b - i) * sat) >> 12);

                            dstPtr->R = Utility.ClampToByte(srcPtr->R + ((r * dev + 32768) >> 16));
                            dstPtr->G = Utility.ClampToByte(srcPtr->G + ((g * dev + 32768) >> 16));
                            dstPtr->B = Utility.ClampToByte(srcPtr->B + ((b * dev + 32768) >> 16));
                            dstPtr->A = srcPtr->A;
                        }

                        ++srcPtr;
                        ++dstPtr;
                    }
                }
            }
        }
    }
}