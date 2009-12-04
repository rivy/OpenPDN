/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace PaintDotNet.Effects
{
    public class CurvesEffectConfigToken
        : EffectConfigToken
    {
        private SortedList<int, int>[] controlPoints;
        public SortedList<int, int>[] ControlPoints
        {
            get
            {
                return this.controlPoints;
            }

            set
            {
                this.uop = null;
                this.controlPoints = value;
            }
        }

        private ColorTransferMode colorTransferMode;
        public ColorTransferMode ColorTransferMode
        {
            get
            {
                return this.colorTransferMode;
            }

            set
            {
                this.uop = null;
                this.colorTransferMode = value;
            }
        }

        [NonSerialized]
        private UnaryPixelOp uop;

        public UnaryPixelOp Uop
        {
            get
            {
                if (this.uop == null)
                {
                    this.uop = MakeUop();
                }

                return uop;
            }
        }

        private UnaryPixelOp MakeUop()
        {
            UnaryPixelOp uopRet;
            byte[][] transferCurves;
            int entries;

            switch (colorTransferMode)
            {
                case ColorTransferMode.Rgb:
                    UnaryPixelOps.ChannelCurve cc = new UnaryPixelOps.ChannelCurve();
                    transferCurves = new byte[][] { cc.CurveR, cc.CurveG, cc.CurveB };
                    entries = 256;
                    uopRet = cc;
                    break;

                case ColorTransferMode.Luminosity:
                    UnaryPixelOps.LuminosityCurve lc = new UnaryPixelOps.LuminosityCurve();
                    transferCurves = new byte[][] { lc.Curve };
                    entries = 256;
                    uopRet = lc;
                    break;

                default:
                    throw new InvalidEnumArgumentException();
            }

            
            int channels = transferCurves.Length;

            for (int channel = 0; channel < channels; ++channel)
            {
                SortedList<int, int> channelControlPoints = controlPoints[channel];
                IList<int> xa = channelControlPoints.Keys;
                IList<int> ya = channelControlPoints.Values;
                SplineInterpolator interpolator = new SplineInterpolator();
                int length = channelControlPoints.Count;

                for (int i = 0; i < length; ++i)
                {
                    interpolator.Add(xa[i], ya[i]);
                }

                for (int i = 0; i < entries; ++i)
                {
                    transferCurves[channel][i] = Utility.ClampToByte(interpolator.Interpolate(i));
                }
            }

            return uopRet;
        }

        public override object Clone()
        {
            return new CurvesEffectConfigToken(this);
        }

        public CurvesEffectConfigToken()
        {
            controlPoints = new SortedList<int, int>[1];

            for (int i = 0; i < this.controlPoints.Length; ++i)
            {
                SortedList<int, int> newList = new SortedList<int, int>();

                newList.Add(0, 0);
                newList.Add(255, 255);
                controlPoints[i] = newList;
            }

            colorTransferMode = ColorTransferMode.Luminosity;
        }

        protected CurvesEffectConfigToken(CurvesEffectConfigToken copyMe)
            : base(copyMe)
        {
            this.uop = copyMe.Uop;
            this.colorTransferMode = copyMe.ColorTransferMode;
            this.controlPoints = (SortedList<int, int>[])copyMe.controlPoints.Clone();
        }
    }
}
