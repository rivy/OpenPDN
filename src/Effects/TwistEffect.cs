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
using PaintDotNet.Effects;
using PaintDotNet.PropertySystem;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace PaintDotNet.Effects
{
    public sealed class TwistEffect
        : InternalPropertyBasedEffect
    {
        public static Image StaticImage
        {
            get
            {
                return PdnResources.GetImageResource("Icons.TwistEffect.png").Reference;
            }
        }

        public static string StaticName
        {
            get
            {
                return PdnResources.GetString("TwistEffect.Name");
            }
        }

        public static string StaticSubMenuName
        {
            get
            {
                return SubmenuNames.Distort;
            }
        }

        public enum PropertyNames
        {
            Amount = 0,
            Size = 1,
            Offset = 2,
            Quality = 3
        }

        public TwistEffect()
            : base(StaticName, StaticImage, StaticSubMenuName, EffectFlags.Configurable)
        {
        }

        protected override PropertyCollection OnCreatePropertyCollection()
        {
            List<Property> props = new List<Property>();

            props.Add(new DoubleProperty(PropertyNames.Amount, 30.0, -200.0, 200.0));
            props.Add(new DoubleProperty(PropertyNames.Size, 1.0, 0.01, 2.0));

            props.Add(new DoubleVectorProperty(
                PropertyNames.Offset,
                Pair.Create(0.0, 0.0),
                Pair.Create(-2.0, -2.0),
                Pair.Create(+2.0, +2.0)));

            props.Add(new Int32Property(PropertyNames.Quality, 2, 1, 5));

            return new PropertyCollection(props);
        }

        protected override ControlInfo OnCreateConfigUI(PropertyCollection props)
        {
            ControlInfo configUI = CreateDefaultConfigUI(props);

            configUI.SetPropertyControlValue(PropertyNames.Amount, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("TwistEffect.TwistAmount.Text"));
            configUI.SetPropertyControlValue(PropertyNames.Amount, ControlInfoPropertyNames.UseExponentialScale, true);

            configUI.SetPropertyControlValue(PropertyNames.Size, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("TwistEffect.TwistSize.Text"));
            configUI.SetPropertyControlValue(PropertyNames.Size, ControlInfoPropertyNames.SliderSmallChange, 0.05);
            configUI.SetPropertyControlValue(PropertyNames.Size, ControlInfoPropertyNames.SliderLargeChange, 0.25);
            configUI.SetPropertyControlValue(PropertyNames.Size, ControlInfoPropertyNames.UpDownIncrement, 0.01);

            configUI.SetPropertyControlValue(PropertyNames.Offset, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("TwistEffect.Offset.Text"));
            configUI.SetPropertyControlValue(PropertyNames.Offset, ControlInfoPropertyNames.SliderSmallChangeX, 0.05);
            configUI.SetPropertyControlValue(PropertyNames.Offset, ControlInfoPropertyNames.SliderLargeChangeX, 0.25);
            configUI.SetPropertyControlValue(PropertyNames.Offset, ControlInfoPropertyNames.UpDownIncrementX, 0.01);
            configUI.SetPropertyControlValue(PropertyNames.Offset, ControlInfoPropertyNames.SliderSmallChangeY, 0.05);
            configUI.SetPropertyControlValue(PropertyNames.Offset, ControlInfoPropertyNames.SliderLargeChangeY, 0.25);
            configUI.SetPropertyControlValue(PropertyNames.Offset, ControlInfoPropertyNames.UpDownIncrementY, 0.01);

            Surface sourceSurface = this.EnvironmentParameters.SourceSurface;
            Bitmap bitmap = sourceSurface.CreateAliasedBitmap();
            ImageResource imageResource = ImageResource.FromImage(bitmap);
            configUI.SetPropertyControlValue(PropertyNames.Offset, ControlInfoPropertyNames.StaticImageUnderlay, imageResource);

            configUI.SetPropertyControlValue(PropertyNames.Quality, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("TwistEffect.Antialias.Text"));

            return configUI;
        }

        private double inv100 = 1.0 / 100.0;

        private double amount;
        private double size;
        private int quality;
        private Pair<double, double> offset;

        protected override void OnSetRenderInfo(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            this.amount = -newToken.GetProperty<DoubleProperty>(PropertyNames.Amount).Value;
            this.size = 1.0 / newToken.GetProperty<DoubleProperty>(PropertyNames.Size).Value;
            this.quality = newToken.GetProperty<Int32Property>(PropertyNames.Quality).Value;
            this.offset = newToken.GetProperty<DoubleVectorProperty>(PropertyNames.Offset).Value;

            base.OnSetRenderInfo(newToken, dstArgs, srcArgs);
        }

        protected unsafe override void OnRender(Rectangle[] rois, int startIndex, int length)
        {
            double twist = this.amount * this.amount * Math.Sign(this.amount);

            Surface dst = DstArgs.Surface;
            Surface src = SrcArgs.Surface;

            float hw = dst.Width / 2.0f;
            hw += (float)(hw * this.offset.First);
            float hh = dst.Height / 2.0f;
            hh += (float)(hh * this.offset.Second);

            //*double maxrad = Math.Min(dst.Width / 2.0, dst.Height / 2.0);
            double invmaxrad = 1.0 / Math.Min(dst.Width / 2.0, dst.Height / 2.0);

            int aaLevel = this.quality;
            int aaSamples = aaLevel * aaLevel;
            PointF* aaPoints = stackalloc PointF[aaSamples];
            Utility.GetRgssOffsets(aaPoints, aaSamples, aaLevel);

            ColorBgra* samples = stackalloc ColorBgra[aaSamples];

            for (int n = startIndex; n < startIndex + length; ++n)
            {
                Rectangle rect = rois[n];

                for (int y = rect.Top; y < rect.Bottom; y++)
                {
                    float j = y - hh;
                    ColorBgra* dstPtr = dst.GetPointAddressUnchecked(rect.Left, y);
                    ColorBgra* srcPtr = src.GetPointAddressUnchecked(rect.Left, y);

                    for (int x = rect.Left; x < rect.Right; x++)
                    {
                        float i = x - hw;

                        int sampleCount = 0;

                        for (int p = 0; p < aaSamples; ++p)
                        {
                            float u = i + aaPoints[p].X;
                            float v = j + aaPoints[p].Y;

                            double rad = Math.Sqrt(u * u + v * v);
                            double theta = Math.Atan2(v, u);

                            double t = 1 - ((rad * this.size) * invmaxrad);

                            t = (t < 0) ? 0 : (t * t * t);

                            theta += (t * twist) * inv100;

                            float sampleX = (hw + (float)(rad * Math.Cos(theta)));
                            float sampleY = (hh + (float)(rad * Math.Sin(theta)));

                            samples[sampleCount] = src.GetBilinearSampleClamped(sampleX, sampleY);
                            ++sampleCount;
                        }

                        *dstPtr = ColorBgra.Blend(samples, sampleCount);


                        ++dstPtr;
                        ++srcPtr;
                    }
                }
            }
        }
    }
}
