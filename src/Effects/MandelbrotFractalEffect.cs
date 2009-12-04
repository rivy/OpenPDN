/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.Core;
using PaintDotNet.IndirectUI;
using PaintDotNet.PropertySystem;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace PaintDotNet.Effects
{
    public sealed class MandelbrotFractalEffect
        : InternalPropertyBasedEffect
    {
        public static string StaticName
        {
            get
            {
                return PdnResources.GetString("MandelbrotFractalEffect.Name");
            }
        }

        public static Image StaticImage
        {
            get
            {
                return PdnResources.GetImageResource("Icons.MandelbrotFractalEffectIcon.png").Reference;
            }
        }

        public enum PropertyNames
        {
            Factor = 0,
            Quality = 1,
            Zoom = 2,
            Angle = 3,
            InvertColors = 4
        }

        public MandelbrotFractalEffect()
            : base(StaticName,
                   StaticImage, 
                   SubmenuNames.Render, 
                   EffectFlags.Configurable)
        {
        }

        protected override PropertyCollection OnCreatePropertyCollection()
        {
            List<Property> props = new List<Property>();

            props.Add(new Int32Property(PropertyNames.Factor, 1, 1, 10));
            props.Add(new DoubleProperty(PropertyNames.Zoom, 10, 0, 100));
            props.Add(new DoubleProperty(PropertyNames.Angle, 0.0, -180.0, +180.0));
            props.Add(new Int32Property(PropertyNames.Quality, 2, 1, 5));
            props.Add(new BooleanProperty(PropertyNames.InvertColors));

            return new PropertyCollection(props);
        }

        protected override ControlInfo OnCreateConfigUI(PropertyCollection props)
        {
            ControlInfo configUI = CreateDefaultConfigUI(props);

            configUI.SetPropertyControlValue(PropertyNames.Factor, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("MandelbrotFractalEffect.ConfigDialog.Factor.DisplayName"));
            configUI.SetPropertyControlValue(PropertyNames.Zoom, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("MandelbrotFractalEffect.ConfigDialog.Zoom.DisplayName"));
            configUI.SetPropertyControlValue(PropertyNames.Angle, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("MandelbrotFractalEffect.ConfigDialog.Angle.DisplayName"));
            configUI.SetPropertyControlType(PropertyNames.Angle, PropertyControlType.AngleChooser);
            configUI.SetPropertyControlValue(PropertyNames.Quality, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("MandelbrotFractalEffect.ConfigDialog.Quality.DisplayName"));
            configUI.SetPropertyControlValue(PropertyNames.InvertColors, ControlInfoPropertyNames.DisplayName, string.Empty);
            configUI.SetPropertyControlValue(PropertyNames.InvertColors, ControlInfoPropertyNames.Description, PdnResources.GetString("MandelbrotFractalEffect.ConfigDialog.InvertColors.Description"));

            return configUI;
        }

        protected override void OnSetRenderInfo(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            this.zoom = 1 + zoomFactor * newToken.GetProperty<DoubleProperty>(PropertyNames.Zoom).Value;
            this.factor = newToken.GetProperty<Int32Property>(PropertyNames.Factor).Value;
            this.quality = newToken.GetProperty<Int32Property>(PropertyNames.Quality).Value;
            this.angle = newToken.GetProperty<DoubleProperty>(PropertyNames.Angle).Value;
            this.invertColors = newToken.GetProperty<BooleanProperty>(PropertyNames.InvertColors).Value;
            this.angleTheta = (this.angle * 2 * Math.PI) / 360;

            base.OnSetRenderInfo(newToken, dstArgs, srcArgs);
        }

        private static double zoomFactor = 20.0;
        private double zoom;

        private int factor;
        private int quality = 1;
        private double angle = 0;

        private double angleTheta;

        private const double xOffsetBasis = -0.7;
        private double xOffset = xOffsetBasis;

        private const double yOffsetBasis = -0.29;
        private double yOffset = yOffsetBasis;

        private bool invertColors;

        private const double max = 100000;
        private static readonly double invLogMax = 1.0 / Math.Log(max);

        private static double Mandelbrot(double r, double i, int factor)
        {
            int c = 0;
            double x = 0;
            double y = 0;

            while ((c * factor) < 1024 && 
                   ((x * x) + (y * y)) < max)
            {
                double t = x;

                x = x * x - y * y + r;
                y = 2 * t * y + i;

                ++c;
            }

            return c - Math.Log(y * y + x * x) * invLogMax;
        }

        protected override unsafe void OnRender(Rectangle[] renderRects, int startIndex, int length)
        {
            int w = DstArgs.Width;
            int h = DstArgs.Height;

            double wDiv2 = (double)w / 2;
            double hDiv2 = (double)h / 2;

            double invH = 1.0 / h;
            double invZoom = 1.0 / this.zoom;

            double invQuality = 1.0 / (double)this.quality;

            int count = this.quality * this.quality + 1;
            double invCount = 1.0 / (double)count;

            for (int ri = startIndex; ri < startIndex + length; ++ri)
            {
                Rectangle rect = renderRects[ri];

                for (int y = rect.Top; y < rect.Bottom; y++)
                {
                    ColorBgra* dstPtr = DstArgs.Surface.GetPointAddressUnchecked(rect.Left, y);

                    for (int x = rect.Left; x < rect.Right; x++)
                    {
                        int r = 0;
                        int g = 0;
                        int b = 0;
                        int a = 0;

                        for (double i = 0; i < count; i++)
                        {
                            double u = (2.0 * x - w + (i * invCount)) * invH;
                            double v = (2.0 * y - h + ((i * invQuality) % 1)) * invH;

                            double radius = Math.Sqrt((u * u) + (v * v));
                            double radiusP = radius;
                            double theta = Math.Atan2(v, u);
                            double thetaP = theta + this.angleTheta;

                            double uP = radiusP * Math.Cos(thetaP);
                            double vP = radiusP * Math.Sin(thetaP);

                            double m = Mandelbrot(
                                (uP * invZoom) + this.xOffset, 
                                (vP * invZoom) + this.yOffset, 
                                this.factor);

                            double c = 64 + this.factor * m;

                            r += Utility.ClampToByte(c - 768);
                            g += Utility.ClampToByte(c - 512);
                            b += Utility.ClampToByte(c - 256);
                            a += Utility.ClampToByte(c - 0);
                        }

                        *dstPtr = ColorBgra.FromBgra(
                            Utility.ClampToByte(b / count),
                            Utility.ClampToByte(g / count),
                            Utility.ClampToByte(r / count),
                            Utility.ClampToByte(a / count));

                        ++dstPtr;
                    }
                }

                if (this.invertColors)
                {
                    for (int y = rect.Top; y < rect.Bottom; y++)
                    {
                        ColorBgra* dstPtr = DstArgs.Surface.GetPointAddressUnchecked(rect.Left, y);

                        for (int x = rect.Left; x < rect.Right; ++x)
                        {
                            ColorBgra c = *dstPtr;

                            c.B = (byte)(255 - c.B);
                            c.G = (byte)(255 - c.G);
                            c.R = (byte)(255 - c.R);

                            *dstPtr = c;
                            ++dstPtr;
                        }
                    }
                }
            }            
        }
    }
}
