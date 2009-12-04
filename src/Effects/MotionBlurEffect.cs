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
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PaintDotNet.Effects
{
    public sealed class MotionBlurEffect
        : InternalPropertyBasedEffect
    {
        public MotionBlurEffect()
            : base(PdnResources.GetString("MotionBlurEffect.Name"),
                   PdnResources.GetImageResource("Icons.MotionBlurEffect.png").Reference,
                   SubmenuNames.Blurs,
                   EffectFlags.Configurable)
        {
        }

        public enum PropertyNames
        {
            Angle = 0,
            Distance = 1,
            Centered = 2
        }

        protected override PropertyCollection OnCreatePropertyCollection()
        {
            List<Property> props = new List<Property>();

            props.Add(new DoubleProperty(PropertyNames.Angle, 25, -180, +180));
            props.Add(new BooleanProperty(PropertyNames.Centered, true));
            props.Add(new Int32Property(PropertyNames.Distance, 10, 1, 200));

            return new PropertyCollection(props);
        }

        protected override ControlInfo OnCreateConfigUI(PropertyCollection props)
        {
            ControlInfo configUI = CreateDefaultConfigUI(props);

            configUI.SetPropertyControlValue(PropertyNames.Angle, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("MotionBlurEffectConfigDialog.AngleHeader.Text"));
            configUI.SetPropertyControlType(PropertyNames.Angle, PropertyControlType.AngleChooser);
            configUI.SetPropertyControlValue(PropertyNames.Centered, ControlInfoPropertyNames.DisplayName, string.Empty);
            configUI.SetPropertyControlValue(PropertyNames.Centered, ControlInfoPropertyNames.Description, PdnResources.GetString("MotionBlurEffectConfigDialog.CenteredCheckBox.Text"));
            configUI.SetPropertyControlValue(PropertyNames.Distance, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("MotionBlurEffectConfigDialog.DistanceHeader.Text"));

            return configUI;
        }

        private double angle;
        private int distance;
        private bool centered;
        private PointF[] points;

        protected override void OnSetRenderInfo(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            this.angle = newToken.GetProperty<DoubleProperty>(PropertyNames.Angle).Value;
            this.distance = newToken.GetProperty<Int32Property>(PropertyNames.Distance).Value;
            this.centered = newToken.GetProperty<BooleanProperty>(PropertyNames.Centered).Value;

            PointF start = new PointF(0, 0);
            double theta = ((double)(this.angle + 180) * 2 * Math.PI) / 360.0;
            double alpha = (double)distance;
            double x = alpha * Math.Cos(theta);
            double y = alpha * Math.Sin(theta);
            PointF end = new PointF((float)x, (float)(-y));

            if (this.centered)
            {
                start.X = -end.X / 2.0f;
                start.Y = -end.Y / 2.0f;

                end.X /= 2.0f;
                end.Y /= 2.0f;
            }

            this.points = new PointF[((1 + this.distance) * 3) / 2];

            if (this.points.Length == 1)
            {
                this.points[0] = new PointF(0, 0);
            }
            else
            {
                for (int i = 0; i < this.points.Length; ++i)
                {
                    float frac = (float)i / (float)(this.points.Length - 1);
                    this.points[i] = Utility.Lerp(start, end, frac);
                }
            }

            base.OnSetRenderInfo(newToken, dstArgs, srcArgs);
        }

        protected override unsafe void OnRender(Rectangle[] rois, int startIndex, int length)
        {
            Surface dst = DstArgs.Surface;
            Surface src = SrcArgs.Surface;

            ColorBgra* samples = stackalloc ColorBgra[this.points.Length];

            for (int i = startIndex; i < startIndex + length; ++i)
            {
                Rectangle rect = rois[i];

                for (int y = rect.Top; y < rect.Bottom; ++y)
                {
                    ColorBgra *dstPtr = dst.GetPointAddressUnchecked(rect.Left, y);

                    for (int x = rect.Left; x < rect.Right; ++x)
                    {
                        int sampleCount = 0;

                        PointF a = new PointF((float)x + points[0].X, (float)y + points[0].Y);
                        PointF b = new PointF((float)x + points[points.Length - 1].X, (float)y + points[points.Length - 1].Y);

                        for (int j = 0; j < this.points.Length; ++j)
                        {
                            PointF pt = new PointF(this.points[j].X + (float)x, this.points[j].Y + (float)y);

                            if (pt.X >= 0 && pt.Y >= 0 && pt.X <= (src.Width - 1) && pt.Y <= (src.Height - 1))
                            {
                                samples[sampleCount] = src.GetBilinearSample(pt.X, pt.Y);
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