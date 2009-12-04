/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.Serialization;

namespace PaintDotNet
{
    /// <summary>
    /// Carries information about the subset of Pen configuration details that we support.
    /// Does not carry color information.
    /// </summary>
    [Serializable]
    internal sealed class PenInfo
        : ICloneable,
          ISerializable
    {
        public const DashStyle DefaultDashStyle = DashStyle.Solid;
        public const LineCap2 DefaultLineCap = LineCap2.Flat;
        public const float DefaultCapScale = 1.0f;
        public const float MinCapScale = 1.0f;
        public const float MaxCapScale = 5.0f;

        private DashStyle dashStyle;
        public DashStyle DashStyle
        {
            get
            {
                return this.dashStyle;
            }

            set
            {
                this.dashStyle = value;
            }
        }

        private float width;
        public float Width
        {
            get
            {
                return this.width;
            }

            set
            {
                this.width = value;
            }
        }

        private LineCap2 startCap;
        public LineCap2 StartCap
        {
            get
            {
                return this.startCap;
            }

            set
            {
                this.startCap = value;
            }
        }

        private LineCap2 endCap;
        public LineCap2 EndCap
        {
            get
            {
                return this.endCap;
            }

            set
            {
                this.endCap = value;
            }
        }

        private float capScale;
        private float CapScale
        {
            get
            {
                return Utility.Clamp(this.capScale, MinCapScale, MaxCapScale);
            }

            set
            {
                this.capScale = value;
            }
        }

        public static bool operator==(PenInfo lhs, PenInfo rhs)
        {
            return (
                lhs.dashStyle == rhs.dashStyle && 
                lhs.width == rhs.width &&
                lhs.startCap == rhs.startCap &&
                lhs.endCap == rhs.endCap &&
                lhs.capScale == rhs.capScale);
        }

        public static bool operator!=(PenInfo lhs, PenInfo rhs)
        {
            return !(lhs == rhs);
        }

        public override bool Equals(object obj)
        {
            PenInfo rhs = obj as PenInfo;

            if (rhs == null)
            {
                return false;
            }

            return this == rhs;
        }

        public override int GetHashCode()
        {
            return 
                this.dashStyle.GetHashCode() ^ 
                this.width.GetHashCode() ^ 
                this.startCap.GetHashCode() ^ 
                this.endCap.GetHashCode() ^
                this.capScale.GetHashCode();
        }

        private void LineCapToLineCap2(LineCap2 cap2, out LineCap capResult, out CustomLineCap customCapResult)
        {
            switch (cap2)
            {
                case LineCap2.Flat:
                    capResult = LineCap.Flat;
                    customCapResult = null;
                    break;

                case LineCap2.Arrow:
                    capResult = LineCap.ArrowAnchor;
                    customCapResult = new AdjustableArrowCap(5.0f * this.capScale, 5.0f * this.capScale, false);
                    break;

                case LineCap2.ArrowFilled:
                    capResult = LineCap.ArrowAnchor;
                    customCapResult = new AdjustableArrowCap(5.0f * this.capScale, 5.0f * this.capScale, true);
                    break;

                case LineCap2.Rounded:
                    capResult = LineCap.Round;
                    customCapResult = null;
                    break;

                default:
                    throw new InvalidEnumArgumentException();
            }
        }

        public Pen CreatePen(BrushInfo brushInfo, Color foreColor, Color backColor)
        {
            Pen pen;

            if (brushInfo.BrushType == BrushType.None)
            {
                pen = new Pen(foreColor, width);
            }
            else
            {
                pen = new Pen(brushInfo.CreateBrush(foreColor, backColor), width);
            }

            LineCap startLineCap;
            CustomLineCap startCustomLineCap;
            LineCapToLineCap2(this.startCap, out startLineCap, out startCustomLineCap);

            if (startCustomLineCap != null)
            {
                pen.CustomStartCap = startCustomLineCap;
            }
            else
            {
                pen.StartCap = startLineCap;
            }

            LineCap endLineCap;
            CustomLineCap endCustomLineCap;
            LineCapToLineCap2(this.endCap, out endLineCap, out endCustomLineCap);

            if (endCustomLineCap != null)
            {
                pen.CustomEndCap = endCustomLineCap;
            }
            else
            {
                pen.EndCap = endLineCap;
            }

            pen.DashStyle = this.dashStyle;

            return pen;
        }

        public PenInfo(DashStyle dashStyle, float width, LineCap2 startCap, LineCap2 endCap, float capScale)
        {
            this.dashStyle = dashStyle;
            this.width = width;
            this.capScale = capScale;
            this.startCap = startCap;
            this.endCap = endCap;
        }

        private PenInfo(SerializationInfo info, StreamingContext context)
        {
            this.dashStyle = (DashStyle)info.GetValue("dashStyle", typeof(DashStyle));
            this.width = info.GetSingle("width");

            // Save the caps as integers because we want to change the "LineCap2" name.
            // Just not feeling very creative right now I guess.
            try
            {
                this.startCap = (LineCap2)info.GetInt32("startCap");
            }

            catch (SerializationException)
            {
                this.startCap = DefaultLineCap;
            }

            try
            {
                this.endCap = (LineCap2)info.GetInt32("endCap");
            }

            catch (SerializationException)
            {
                this.endCap = DefaultLineCap;
            }

            try
            {
                float loadedCapScale = info.GetSingle("capScale");
                this.capScale = Utility.Clamp(loadedCapScale, MinCapScale, MaxCapScale);
            }

            catch (SerializationException)
            {
                this.capScale = DefaultCapScale;
            }
        }

        public PenInfo Clone()
        {
            return new PenInfo(this.dashStyle, this.width, this.startCap, this.endCap, this.capScale);
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("dashStyle", this.dashStyle);
            info.AddValue("width", this.width);
            info.AddValue("startCap", (int)this.startCap);
            info.AddValue("endCap", (int)this.endCap);
            info.AddValue("capScale", this.capScale);
        }
    }
}
