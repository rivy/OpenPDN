/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.IndirectUI;
using PaintDotNet.PropertySystem;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PaintDotNet.Effects
{
    public sealed class RadialBlurEffect
        : InternalPropertyBasedEffect
    {
        public static string StaticName
        {
            get
            {
                return PdnResources.GetString("RadialBlurEffect.Name");
            }
        }

        public RadialBlurEffect()
            : base(StaticName,
                   PdnResources.GetImageResource("Icons.RadialBlurEffect.png").Reference,
                   SubmenuNames.Blurs,
                   EffectFlags.Configurable)
        {
        }

        public enum PropertyNames
        {
            Angle = 0,
            Offset = 1,
            Quality = 2
        }

        protected override PropertyCollection OnCreatePropertyCollection()
        {
            List<Property> props = new List<Property>();

            props.Add(new DoubleProperty(PropertyNames.Angle, 2, 0, 360));

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

            configUI.SetPropertyControlValue(PropertyNames.Angle, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("RadialBlurEffect.ConfigDialog.RadialLabel"));
            configUI.FindControlForPropertyName(PropertyNames.Angle).ControlType.Value = PropertyControlType.AngleChooser;

            configUI.SetPropertyControlValue(PropertyNames.Offset, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("RadialBlurEffect.ConfigDialog.OffsetLabel"));
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

            configUI.SetPropertyControlValue(PropertyNames.Quality, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("RadialBlurEffect.ConfigDialog.QualityLabel"));
            configUI.SetPropertyControlValue(PropertyNames.Quality, ControlInfoPropertyNames.Description, PdnResources.GetString("RadialBlurEffect.ConfigDialog.QualityDescription"));

            return configUI;
        }

        private static void Rotate(ref int fx, ref int fy, int fr)
        {
            int cx = fx;
            int cy = fy;

            //sin(x) ~~ x
            //cos(x)~~ 1 - x^2/2
            fx = cx - ((cy >> 8) * fr >> 8) - ((cx >> 14) * (fr * fr >> 11) >> 8);
            fy = cy + ((cx >> 8) * fr >> 8) - ((cy >> 14) * (fr * fr >> 11) >> 8);
        }

        private double angle;
        private double offsetX;
        private double offsetY;
        private int quality;

        protected override void OnSetRenderInfo(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            this.angle = newToken.GetProperty<DoubleProperty>(PropertyNames.Angle).Value;
            this.offsetX = newToken.GetProperty<DoubleVectorProperty>(PropertyNames.Offset).ValueX;
            this.offsetY = newToken.GetProperty<DoubleVectorProperty>(PropertyNames.Offset).ValueY;

            this.quality = newToken.GetProperty<Int32Property>(PropertyNames.Quality).Value;

            base.OnSetRenderInfo(newToken, dstArgs, srcArgs);
        }

        protected unsafe override void OnRender(Rectangle[] rois, int startIndex, int length)
        {
            Surface src = SrcArgs.Surface;
            Surface dst = DstArgs.Surface;
            int w = dst.Width;
            int h = dst.Height;
            int fcx = (w << 15) + (int)(this.offsetX * (w << 15));
            int fcy = (h << 15) + (int)(this.offsetY * (h << 15));

            int n = (this.quality * this.quality) * (30 + this.quality * this.quality);

            int fr = (int)(this.angle * Math.PI * 65536.0 / 181.0);

            for (int r = startIndex; r < startIndex + length; ++r)
            {
                Rectangle rect = rois[r];

                for (int y = rect.Top; y < rect.Bottom; ++y)
                {
                    ColorBgra* dstPtr = dst.GetPointAddressUnchecked(rect.Left, y);
                    ColorBgra* srcPtr = src.GetPointAddressUnchecked(rect.Left, y);

                    for (int x = rect.Left; x < rect.Right; ++x)
                    {
                        int fx = (x << 16) - fcx;
                        int fy = (y << 16) - fcy;

                        int fsr = fr / n;

                        int sr = 0;
                        int sg = 0;
                        int sb = 0;
                        int sa = 0;
                        int sc = 0;

                        sr += srcPtr->R * srcPtr->A;
                        sg += srcPtr->G * srcPtr->A;
                        sb += srcPtr->B * srcPtr->A;
                        sa += srcPtr->A;
                        ++sc;

                        int ox1 = fx;
                        int ox2 = fx;
                        int oy1 = fy;
                        int oy2 = fy;

                        for (int i = 0; i < n; ++i)
                        {
                            Rotate(ref ox1, ref oy1, fsr);
                            Rotate(ref ox2, ref oy2, -fsr);

                            int u1 = ox1 + fcx + 32768 >> 16;
                            int v1 = oy1 + fcy + 32768 >> 16;

                            if (u1 > 0 && v1 > 0 && u1 < w && v1 < h)
                            {
                                ColorBgra *sample = src.GetPointAddressUnchecked(u1, v1);

                                sr += sample->R * sample->A;
                                sg += sample->G * sample->A;
                                sb += sample->B * sample->A;
                                sa += sample->A;
                                ++sc;
                            }

                            int u2 = ox2 + fcx + 32768 >> 16;
                            int v2 = oy2 + fcy + 32768 >> 16;

                            if (u2 > 0 && v2 > 0 && u2 < w && v2 < h)
                            {
                                ColorBgra* sample = src.GetPointAddressUnchecked(u2, v2);

                                sr += sample->R * sample->A;
                                sg += sample->G * sample->A;
                                sb += sample->B * sample->A;
                                sa += sample->A;
                                ++sc;
                            }
                        }

                        if (sa > 0)
                        {
                            *dstPtr = ColorBgra.FromBgra(
                                Utility.ClampToByte(sb / sa),
                                Utility.ClampToByte(sg / sa),
                                Utility.ClampToByte(sr / sa),
                                Utility.ClampToByte(sa / sc));
                        }
                        else
                        {
                            dstPtr->Bgra = 0;
                        }

                        ++dstPtr;
                        ++srcPtr;
                    }
                }
            }
        }
    }
}
