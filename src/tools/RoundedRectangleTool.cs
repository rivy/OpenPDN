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
    internal class RoundedRectangleTool
        : ShapeTool 
    {
        private ImageResource roundedRectangleToolIcon;
        private string statusTextFormat = PdnResources.GetString("RoundedRectangleTool.StatusText.Format");
        private Cursor roundedRectangleCursor;

        protected override RectangleF[] GetOptimizedShapeOutlineRegion(PointF[] points, PdnGraphicsPath path)
        {
            return Utility.SimplifyTrace(path.PathPoints);
        }

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

        protected override PdnGraphicsPath CreateShapePath(PointF[] points)
        {
            PointF a = points[0];
            PointF b = points[points.Length - 1];
            RectangleF rect;
            float radius = 10;

            if ((ModifierKeys & Keys.Shift) != 0)
            {
                rect = Utility.PointsToConstrainedRectangle(a, b);
            }
            else
            {
                rect = Utility.PointsToRectangle(a, b);
            }

            PdnGraphicsPath path = this.GetRoundedRect(rect, radius); 
            path.Flatten();

            if (path.PathPoints[0] != path.PathPoints[path.PathPoints.Length - 1])
            {
                path.AddLine(path.PathPoints[0], path.PathPoints[path.PathPoints.Length - 1]);
                path.CloseFigure();
            }

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

            this.SetStatus(this.roundedRectangleToolIcon, statusText);

            return path;
        }

        protected override void OnActivate()
        {
            this.roundedRectangleCursor = new Cursor(PdnResources.GetResourceStream("Cursors.RoundedRectangleToolCursor.cur"));
            this.Cursor = this.roundedRectangleCursor;
            this.roundedRectangleToolIcon = this.Image;
            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            if (this.roundedRectangleCursor != null)
            {
                this.roundedRectangleCursor.Dispose();
                this.roundedRectangleCursor = null;
            }

            base.OnDeactivate();
        }

        public RoundedRectangleTool(DocumentWorkspace documentWorkspace)
            : base(documentWorkspace,
                   PdnResources.GetImageResource("Icons.RoundedRectangleToolIcon.png"),
                   PdnResources.GetString("RoundedRectangleTool.Name"),
                   PdnResources.GetString("RoundedRectangleTool.HelpText"))
        {
        }

        // credit for the this function is given to Aaron Reginald http://www.codeproject.com/cs/media/ExtendedGraphics.asp
        protected PdnGraphicsPath GetRoundedRect(RectangleF baseRect, float radius) 
        {
            // if corner radius is less than or equal to zero, 
            // return the original rectangle 
            if (radius <= 0.0f) 
            { 
                PdnGraphicsPath mPath = new PdnGraphicsPath(); 
                mPath.AddRectangle(baseRect); 
                mPath.CloseFigure(); 
                return mPath;
            }

            // if the corner radius is greater than or equal to 
            // half the width, or height (whichever is shorter) 
            // then return a capsule instead of a lozenge 
            if (radius >= (Math.Min(baseRect.Width, baseRect.Height)) / 2.0) 
            {
                return GetCapsule(baseRect); 
            }

            // create the arc for the rectangle sides and declare 
            // a graphics path object for the drawing 
            float diameter = radius * 2.0f; 
            SizeF sizeF = new SizeF(diameter, diameter);
            RectangleF arc = new RectangleF(baseRect.Location, sizeF); 
            PdnGraphicsPath path = new PdnGraphicsPath(); 

            // top left arc 
            path.AddArc (arc, 180, 90); 

            // top right arc 
            arc.X = baseRect.Right - diameter; 
            path.AddArc (arc, 270, 90); 

            // bottom right arc 
            arc.Y = baseRect.Bottom - diameter; 
            path.AddArc (arc, 0, 90); 

            // bottom left arc
            arc.X = baseRect.Left;     
            path.AddArc (arc, 90, 90);     

            path.CloseFigure(); 
            return path; 
        } 

        // credit for the this function is given to Aaron Reginald http://www.codeproject.com/cs/media/ExtendedGraphics.asp
        private PdnGraphicsPath GetCapsule(RectangleF baseRect) 
        { 
            float diameter; 
            RectangleF arc; 
            PdnGraphicsPath path = new PdnGraphicsPath(); 

            try 
            { 
                if (baseRect.Width>baseRect.Height) 
                {   
                    // return horizontal capsule 
                    diameter = baseRect.Height; 
                    SizeF sizeF = new SizeF(diameter, diameter);
                    arc = new RectangleF(baseRect.Location, sizeF); 
                    path.AddArc(arc, 90, 180); 
                    arc.X = baseRect.Right-diameter; 
                    path.AddArc(arc, 270, 180); 
                } 
                else if (baseRect.Width < baseRect.Height) 
                {   
                    // return vertical capsule 
                    diameter = baseRect.Width;
                    SizeF sizeF = new SizeF(diameter, diameter);
                    arc = new RectangleF(baseRect.Location, sizeF);
                    path.AddArc(arc, 180, 180); 
                    arc.Y = baseRect.Bottom-diameter; 
                    path.AddArc(arc, 0, 180); 
                } 
                else
                {   // return circle 
                    path.AddEllipse(baseRect); 
                }
            } 

            catch (Exception)
            {
                path.AddEllipse(baseRect);
            } 

            finally 
            { 
                path.CloseFigure(); 
            } 

            return path; 
        } 
    }
}
