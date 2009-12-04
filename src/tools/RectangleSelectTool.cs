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
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace PaintDotNet.Tools
{
    internal class RectangleSelectTool
        : SelectionTool
    {
        protected override List<Point> TrimShapePath(System.Collections.Generic.List<Point> tracePoints)
        {
            List<Point> array = new List<Point>();

            if (tracePoints.Count > 0)
            {
                array.Add(tracePoints[0]);

                if (tracePoints.Count > 1)
                {
                    array.Add(tracePoints[tracePoints.Count - 1]);
                }
            }

            return array;
        }

        protected override List<PointF> CreateShape(List<Point> tracePoints)
        {
            Point a = tracePoints[0];
            Point b = tracePoints[tracePoints.Count - 1];

            Rectangle rect;

            SelectionDrawModeInfo sdmInfo = AppEnvironment.SelectionDrawModeInfo;
            switch (sdmInfo.DrawMode)
            {
                case SelectionDrawMode.Normal:
                    if ((ModifierKeys & Keys.Shift) != 0)
                    {
                        rect = Utility.PointsToConstrainedRectangle(a, b);
                    }
                    else
                    {
                        rect = Utility.PointsToRectangle(a, b);
                    }
                    break;

                case SelectionDrawMode.FixedRatio:
                    try
                    {
                        int drawnWidth = b.X - a.X;
                        int drawnHeight = b.Y - a.Y;

                        double drawnWidthScale = (double)drawnWidth / (double)sdmInfo.Width;
                        double drawnWidthSign = Math.Sign(drawnWidthScale);
                        double drawnHeightScale = (double)drawnHeight / (double)sdmInfo.Height;
                        double drawnHeightSign = Math.Sign(drawnHeightScale);

                        double aspect = (double)sdmInfo.Width / (double)sdmInfo.Height;

                        if (drawnWidthScale < drawnHeightScale)
                        {
                            rect = Utility.PointsToRectangle(
                                new Point(a.X, a.Y),
                                new Point(a.X + drawnWidth, a.Y + (int)(drawnHeightSign * Math.Abs((double)drawnWidth / aspect))));
                        }
                        else
                        {
                            rect = Utility.PointsToRectangle(
                                new Point(a.X, a.Y),
                                new Point(a.X + (int)(drawnWidthSign * Math.Abs((double)drawnHeight * aspect)), a.Y + drawnHeight));
                        }
                    }

                    catch (ArithmeticException)
                    {
                        rect = new Rectangle(a.X, a.Y, 0, 0);
                    }

                    break;

                case SelectionDrawMode.FixedSize:
                    double pxWidth = Document.ConvertMeasurement(sdmInfo.Width, sdmInfo.Units, this.Document.DpuUnit, this.Document.DpuX, MeasurementUnit.Pixel);
                    double pxHeight = Document.ConvertMeasurement(sdmInfo.Height, sdmInfo.Units, this.Document.DpuUnit, this.Document.DpuY, MeasurementUnit.Pixel);

                    rect = new Rectangle(b.X, b.Y, (int)pxWidth, (int)pxHeight);

                    break;

                default:
                    throw new InvalidEnumArgumentException();
            }

            rect.Intersect(DocumentWorkspace.Document.Bounds);

            List<PointF> shape;

            if (rect.Width > 0 && rect.Height > 0)
            {
                shape = new List<PointF>(5);

                shape.Add(new PointF(rect.Left, rect.Top));
                shape.Add(new PointF(rect.Right, rect.Top));
                shape.Add(new PointF(rect.Right, rect.Bottom));
                shape.Add(new PointF(rect.Left, rect.Bottom));
                shape.Add(shape[0]);
            }
            else
            {
                shape = new List<PointF>(0);
            }

            return shape;
        }

        protected override void OnActivate()
        {
            SetCursors(
                "Cursors.RectangleSelectToolCursor.cur",
                "Cursors.RectangleSelectToolCursorMinus.cur",
                "Cursors.RectangleSelectToolCursorPlus.cur",
                "Cursors.RectangleSelectToolCursorMouseDown.cur");

            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate();
        }

        public RectangleSelectTool(DocumentWorkspace documentWorkspace)
            : base(documentWorkspace,
                   PdnResources.GetImageResource("Icons.RectangleSelectToolIcon.png"),
                   PdnResources.GetString("RectangleSelectTool.Name"),
                   PdnResources.GetString("RectangleSelectTool.HelpText"),
                   's',
                   ToolBarConfigItems.SelectionDrawMode)
        {
        }
    }
}
