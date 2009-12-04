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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;

namespace PaintDotNet.Tools
{
    internal class RectangleTool
        : ShapeTool 
    {
        private ImageResource rectangleToolIcon;
        private string statusTextFormat = PdnResources.GetString("RectangleTool.StatusText.Format");
        private Cursor rectangleToolCursor;

        protected override List<PointF> TrimShapePath(List<PointF> points)
        {
            List<PointF> array = new List<PointF>();

            if (points.Count > 0)
            {
                array.Add(points[0]);

                if (points.Count > 1)
                {
                    array.Add(points[points.Count - 1]);
                }
            }

            return array;
        }

        public override PixelOffsetMode GetPixelOffsetMode()
        {
            if (AppEnvironment.PenInfo.Width == 1.0f)
            {
                return PixelOffsetMode.None;
            }

            return base.GetPixelOffsetMode();
        }

        protected override PdnGraphicsPath CreateShapePath(PointF[] points)
        {
            PointF a = points[0];
            PointF b = points[points.Length - 1];
            RectangleF rect;

            if ((ModifierKeys & Keys.Shift) != 0)
            {
                rect = Utility.PointsToConstrainedRectangle(a, b);
            }
            else
            {
                rect = Utility.PointsToRectangle(a, b);
            }

            PdnGraphicsPath path = new PdnGraphicsPath();
            path.AddRectangle(rect);
            path.CloseFigure();
            path.Reverse();

            MeasurementUnit units = AppWorkspace.Units;
            double widthPhysical = Math.Abs(Document.PixelToPhysicalX(rect.Width, units));
            double heightPhysical = Math.Abs(Document.PixelToPhysicalY(rect.Height, units));
            double areaPhysical = widthPhysical * heightPhysical;
            
            string numberFormat;
            string unitsAbbreviation;

            if (units != MeasurementUnit.Pixel)
            {
                string unitsAbbreviationName = "MeasurementUnit." + units.ToString() + ".Abbreviation";
                unitsAbbreviation = PdnResources.GetString(unitsAbbreviationName);
                numberFormat = "F2";
            }
            else
            {
                unitsAbbreviation = string.Empty;
                numberFormat = "F0";
            }

            string unitsString = PdnResources.GetString("MeasurementUnit." + units.ToString() + ".Plural");

            string statusText = string.Format(
                this.statusTextFormat,
                widthPhysical.ToString(numberFormat),
                unitsAbbreviation,
                heightPhysical.ToString(numberFormat),
                unitsAbbreviation,
                areaPhysical.ToString(numberFormat),
                unitsString);

            this.SetStatus(this.rectangleToolIcon, statusText);
            return path;
        }

        protected override void OnActivate()
        {
            rectangleToolCursor = new Cursor(PdnResources.GetResourceStream("Cursors.RectangleToolCursor.cur"));
            this.rectangleToolIcon = this.Image;
            this.Cursor = rectangleToolCursor;
            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            if (this.rectangleToolCursor != null)
            {
                this.rectangleToolCursor.Dispose();
                this.rectangleToolCursor = null;
            }

            base.OnDeactivate();
        }

        public RectangleTool(DocumentWorkspace documentWorkspace)
            : base(documentWorkspace,
                   PdnResources.GetImageResource("Icons.RectangleToolIcon.png"),
                   PdnResources.GetString("RectangleTool.Name"),
                   PdnResources.GetString("RectangleTool.HelpText"))
        {
        }
    }
}
