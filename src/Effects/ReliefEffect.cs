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
using PaintDotNet.Effects;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace PaintDotNet.Effects
{
    public sealed class ReliefEffect
        : ColorDifferenceEffect
    {
        public ReliefEffect()
            : base(PdnResources.GetString("ReliefEffect.Name"),
                   PdnResources.GetImageResource("Icons.ReliefEffect.png").Reference,
                   SubmenuNames.Stylize,
                   EffectFlags.Configurable)
        {
        }

        public enum PropertyNames
        {
            Angle = 0
        }

        protected override PropertyCollection OnCreatePropertyCollection()
        {
            List<Property> props = new List<Property>();

            props.Add(new DoubleProperty(PropertyNames.Angle, 45.0, -180.0, +180.0));

            return new PropertyCollection(props);
        }

        protected override ControlInfo OnCreateConfigUI(PropertyCollection props)
        {
            ControlInfo configUI = CreateDefaultConfigUI(props);

            configUI.SetPropertyControlValue(PropertyNames.Angle, ControlInfoPropertyNames.DisplayName, PdnResources.GetString("AngleChooserConfigDialog.AngleHeader.Text"));
            configUI.SetPropertyControlType(PropertyNames.Angle, PropertyControlType.AngleChooser);

            return configUI;
        }

        private double angle;
        private double[][] weights;

        protected override void OnSetRenderInfo(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            this.angle = newToken.GetProperty<DoubleProperty>(PropertyNames.Angle).Value;

            this.weights = new double[3][];
            for (int i = 0; i < this.weights.Length; ++i)
            {
                this.weights[i] = new double[3];
            }

            // adjust and convert angle to radians
            double r = (double)this.angle * 2.0 * Math.PI / 360.0;

            // angle delta for each weight
            double dr = Math.PI / 4.0;

            // for r = 0 this builds an Relief filter pointing straight left
            this.weights[0][0] = Math.Cos(r + dr);
            this.weights[0][1] = Math.Cos(r + 2.0 * dr);
            this.weights[0][2] = Math.Cos(r + 3.0 * dr);

            this.weights[1][0] = Math.Cos(r);
            this.weights[1][1] = 1;
            this.weights[1][2] = Math.Cos(r + 4.0 * dr);

            this.weights[2][0] = Math.Cos(r - dr);
            this.weights[2][1] = Math.Cos(r - 2.0 * dr);
            this.weights[2][2] = Math.Cos(r - 3.0 * dr);

            base.OnSetRenderInfo(newToken, dstArgs, srcArgs);
        }

        protected override void OnRender(Rectangle[] rois, int startIndex, int length)
        {
            base.RenderColorDifferenceEffect(this.weights, DstArgs, SrcArgs, rois, startIndex, length);
        }

    }
}
