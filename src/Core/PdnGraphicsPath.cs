/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.SystemLayer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.Serialization;

namespace PaintDotNet
{ 
    [Serializable]
    public sealed class PdnGraphicsPath
        : MarshalByRefObject,
          ICloneable,
          IDisposable,
          ISerializable
    { 
        private GraphicsPath gdiPath;
        private bool tooComplex = false;
        internal PdnRegion regionCache = null;

        public static implicit operator GraphicsPath(PdnGraphicsPath convert)
        {
            return convert.gdiPath;
        }

        internal PdnRegion GetRegionCache()
        {
            if (regionCache == null)
            {
                regionCache = new PdnRegion(this.gdiPath);
            }

            return regionCache;
        }

        private GraphicsPath GdiPath
        {
            get 
            { 
                return gdiPath; 
            }
        }

        private void Changed()
        {
            if (regionCache != null)
            {
                lock (regionCache.SyncRoot)
                {
                    regionCache.Dispose();
                    regionCache = null;
                }
            }
        }

        public PdnGraphicsPath()
        {
            Changed();
            gdiPath = new GraphicsPath();
        }

        public PdnGraphicsPath(GraphicsPath wrapMe)
        {
            Changed();
            gdiPath = wrapMe;
        }

        public PdnGraphicsPath(FillMode fillMode)
        {
            Changed();
            gdiPath = new GraphicsPath(fillMode);
        }

        public PdnGraphicsPath(Point[] pts, byte[] types)
        {
            Changed();
            gdiPath = new GraphicsPath(pts, types);
        }

        public PdnGraphicsPath(PointF[] pts, byte[] types)
        {
            Changed();
            gdiPath = new GraphicsPath(pts, types);
        }

        public PdnGraphicsPath(Point[] pts, byte[] types, FillMode fillMode)
        {
            Changed();
            gdiPath = new GraphicsPath(pts, types, fillMode);
        }

        public PdnGraphicsPath(PointF[] pts, byte[] types, FillMode fillMode)
        {
            Changed();
            gdiPath = new GraphicsPath(pts, types, fillMode);
        }

        public PdnGraphicsPath(SerializationInfo info, StreamingContext context)
        {
            int ptCount = info.GetInt32("ptCount");

            PointF[] pts;
            byte[] types;
            
            if (ptCount == 0)
            {
                pts = new PointF[0];
                types = new byte[0];
            }
            else
            {
                pts = (PointF[])info.GetValue("pts", typeof(PointF[]));
                types = (byte[])info.GetValue("types", typeof(byte[]));
            }
            
            FillMode fillMode = (FillMode)info.GetValue("fillMode", typeof(FillMode));
            Changed();

            if (ptCount == 0)
            {
                gdiPath = new GraphicsPath();
            }
            else
            {
                gdiPath = new GraphicsPath(pts, types, fillMode);
            }

            this.tooComplex = false;
            this.regionCache = null;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            lock (this) // avoid race condition with Dispose()
            {
                info.AddValue("ptCount", this.gdiPath.PointCount);

                if (this.gdiPath.PointCount > 0)
                {
                    info.AddValue("pts", this.gdiPath.PathPoints);
                    info.AddValue("types", this.gdiPath.PathTypes);
                }

                info.AddValue("fillMode", this.gdiPath.FillMode);
            }
        }

        public static PdnGraphicsPath FromRegion(PdnRegion region)
        {
            Rectangle[] scans = region.GetRegionScansReadOnlyInt();

            if (scans.Length == 1)
            {
                PdnGraphicsPath path = new PdnGraphicsPath();
                path.AddRectangle(scans[0]);
                path.CloseFigure();
                return path;
            }
            else
            {
                Rectangle bounds = region.GetBoundsInt();
                BitVector2D stencil = new BitVector2D(bounds.Width, bounds.Height);

                for (int i = 0; i < scans.Length; ++i)
                {
                    Rectangle rect = scans[i];
                    rect.X -= bounds.X;
                    rect.Y -= bounds.Y;

                    stencil.SetUnchecked(rect, true);
                }

                PdnGraphicsPath path = PathFromStencil(stencil, new Rectangle(0, 0, stencil.Width, stencil.Height));

                using (Matrix matrix = new Matrix())
                {
                    matrix.Reset();
                    matrix.Translate(bounds.X, bounds.Y);
                    path.Transform(matrix);
                }

                return path;
            }
        }

        public static PdnGraphicsPath FromRegions(PdnRegion lhs, CombineMode combineMode, PdnRegion rhs)
        {
            Rectangle lhsBounds = lhs.GetBoundsInt();
            Rectangle rhsBounds = rhs.GetBoundsInt();
            int left = Math.Min(lhsBounds.Left, rhsBounds.Left);
            int top = Math.Min(lhsBounds.Top, rhsBounds.Top);
            int right = Math.Max(lhsBounds.Right, rhsBounds.Right);
            int bottom = Math.Max(lhsBounds.Bottom, rhsBounds.Bottom);
            Rectangle bounds = Rectangle.FromLTRB(left, top, right, bottom);
            BitVector2D stencil = new BitVector2D(bounds.Width, bounds.Height);
            Rectangle[] lhsScans = lhs.GetRegionScansReadOnlyInt();
            Rectangle[] rhsScans = rhs.GetRegionScansReadOnlyInt();

            switch (combineMode)
            {
                case CombineMode.Complement:
                case CombineMode.Intersect:
                case CombineMode.Replace:
                    throw new ArgumentException("combineMode can't be Complement, Intersect, or Replace");

                default:
                    break;
            }

            for (int i = 0; i < lhsScans.Length; ++i)
            {
                Rectangle rect = lhsScans[i];
                rect.X -= bounds.X;
                rect.Y -= bounds.Y;

                stencil.SetUnchecked(rect, true);
            }

            for (int i = 0; i < rhsScans.Length; ++i)
            {
                Rectangle rect = rhsScans[i];
                rect.X -= bounds.X;
                rect.Y -= bounds.Y;

                switch (combineMode)
                {
                    case CombineMode.Xor:
                        stencil.InvertUnchecked(rect);
                        break;

                    case CombineMode.Union:
                        stencil.SetUnchecked(rect, true);
                        break;
                    
                    case CombineMode.Exclude:
                        stencil.SetUnchecked(rect, false);
                        break;
                }
            }

            PdnGraphicsPath path = PathFromStencil(stencil, new Rectangle(0, 0, stencil.Width, stencil.Height));

            using (Matrix matrix = new Matrix())
            {
                matrix.Reset();
                matrix.Translate(bounds.X, bounds.Y);
                path.Transform(matrix);
            }

            return path;
        }

        public unsafe static Point[][] PolygonSetFromStencil(IBitVector2D stencil, Rectangle bounds, int translateX, int translateY)
        {
            List<Point[]> polygons = new List<Point[]>();

            if (!stencil.IsEmpty)
            {
                Point start = bounds.Location;
                List<Point> pts = new List<Point>();
                int count = 0;

                // find all islands
                while (true) 
                {
                    bool startFound = false;

                    while (true)
                    {
                        if (stencil[start])
                        {
                            startFound = true;
                            break;
                        }

                        ++start.X;

                        if (start.X >= bounds.Right)
                        {
                            ++start.Y;
                            start.X = bounds.Left;

                            if (start.Y >= bounds.Bottom)
                            {
                                break;
                            }
                        }
                    }
            
                    if (!startFound)
                    {
                        break;
                    }

                    pts.Clear();
                    Point last = new Point(start.X, start.Y + 1);
                    Point curr = new Point(start.X, start.Y);
                    Point next = curr;
                    Point left = Point.Empty;
                    Point right = Point.Empty;
            
                    // trace island outline
                    while (true)
                    {
                        left.X = ((curr.X - last.X) + (curr.Y - last.Y) + 2) / 2 + curr.X - 1;
                        left.Y = ((curr.Y - last.Y) - (curr.X - last.X) + 2) / 2 + curr.Y - 1;

                        right.X = ((curr.X - last.X) - (curr.Y - last.Y) + 2) / 2 + curr.X - 1;
                        right.Y = ((curr.Y - last.Y) + (curr.X - last.X) + 2) / 2 + curr.Y - 1;

                        if (bounds.Contains(left) && stencil[left])
                        {
                            // go left
                            next.X += curr.Y - last.Y;
                            next.Y -= curr.X - last.X;
                        }
                        else if (bounds.Contains(right) && stencil[right])
                        {
                            // go straight
                            next.X += curr.X - last.X;
                            next.Y += curr.Y - last.Y;
                        }
                        else
                        {
                            // turn right
                            next.X -= curr.Y - last.Y;
                            next.Y += curr.X - last.X;
                        }

                        if (Math.Sign(next.X - curr.X) != Math.Sign(curr.X - last.X) ||
                            Math.Sign(next.Y - curr.Y) != Math.Sign(curr.Y - last.Y))
                        {
                            pts.Add(curr);
                            ++count;
                        }

                        last = curr;
                        curr = next;

                        if (next.X == start.X && next.Y == start.Y)
                        {
                            break;
                        }
                    }

                    Point[] points = pts.ToArray();
                    Scanline[] scans = Utility.GetScans(points);

                    foreach (Scanline scan in scans)
                    {
                        stencil.Invert(scan);
                    }

                    Utility.TranslatePointsInPlace(points, translateX, translateY);
                    polygons.Add(points);
                }
            }

            Point[][] returnVal = polygons.ToArray();
            return returnVal;
        }

        /// <summary>
        /// Creates a graphics path from the given stencil buffer. It should be filled with 'true' values
        /// to indicate the areas that should be outlined.
        /// </summary>
        /// <param name="stencil">The stencil buffer to read from. NOTE: The contents of this will be destroyed when this method returns.</param>
        /// <param name="bounds">The bounding box within the stencil buffer to limit discovery to.</param>
        /// <returns>A PdnGraphicsPath with traces that outline the various areas from the given stencil buffer.</returns>
        public unsafe static PdnGraphicsPath PathFromStencil(IBitVector2D stencil, Rectangle bounds)
        {
            if (stencil.IsEmpty)
            {
                return new PdnGraphicsPath();
            }

            PdnGraphicsPath ret = new PdnGraphicsPath();
            Point start = bounds.Location;
            Vector<Point> pts = new Vector<Point>();
            int count = 0;

            // find all islands
            while (true) 
            {
                bool startFound = false;

                while (true)
                {
                    if (stencil[start])
                    {
                        startFound = true;
                        break;
                    }

                    ++start.X;

                    if (start.X >= bounds.Right)
                    {
                        ++start.Y;
                        start.X = bounds.Left;

                        if (start.Y >= bounds.Bottom)
                        {
                            break;
                        }
                    }
                }
            
                if (!startFound)
                {
                    break;
                }

                pts.Clear();
                Point last = new Point(start.X, start.Y + 1);
                Point curr = new Point(start.X, start.Y);
                Point next = curr;
                Point left = Point.Empty;
                Point right = Point.Empty;
            
                // trace island outline
                while (true)
                {
                    left.X = ((curr.X - last.X) + (curr.Y - last.Y) + 2) / 2 + curr.X - 1;
                    left.Y = ((curr.Y - last.Y) - (curr.X - last.X) + 2) / 2 + curr.Y - 1;

                    right.X = ((curr.X - last.X) - (curr.Y - last.Y) + 2) / 2 + curr.X - 1;
                    right.Y = ((curr.Y - last.Y) + (curr.X - last.X) + 2) / 2 + curr.Y - 1;

                    if (bounds.Contains(left) && stencil[left])
                    {
                        // go left
                        next.X += curr.Y - last.Y;
                        next.Y -= curr.X - last.X;
                    }
                    else if (bounds.Contains(right) && stencil[right])
                    {
                        // go straight
                        next.X += curr.X - last.X;
                        next.Y += curr.Y - last.Y;
                    }
                    else
                    {
                        // turn right
                        next.X -= curr.Y - last.Y;
                        next.Y += curr.X - last.X;
                    }

                    if (Math.Sign(next.X - curr.X) != Math.Sign(curr.X - last.X) ||
                        Math.Sign(next.Y - curr.Y) != Math.Sign(curr.Y - last.Y))
                    {
                        pts.Add(curr);
                        ++count;
                    }

                    last = curr;
                    curr = next;

                    if (next.X == start.X && next.Y == start.Y)
                    {
                        break;
                    }
                }

                Point[] points = pts.ToArray();
                Scanline[] scans = Utility.GetScans(points);

                foreach (Scanline scan in scans)
                {
                    stencil.Invert(scan);
                }

                ret.AddLines(points);
                ret.CloseFigure();
            }

            return ret;
        }

        public static PdnGraphicsPath Combine(PdnGraphicsPath subjectPath, CombineMode combineMode, PdnGraphicsPath clipPath)
        {
            switch (combineMode)
            {
                case CombineMode.Complement:
                    return Combine(clipPath, CombineMode.Exclude, subjectPath);

                case CombineMode.Replace:
                    return clipPath.Clone();

                case CombineMode.Xor:
                case CombineMode.Intersect:
                case CombineMode.Union:
                case CombineMode.Exclude:
                    if (subjectPath.IsEmpty && clipPath.IsEmpty)
                    {
                        return new PdnGraphicsPath(); // empty path
                    }
                    else if (subjectPath.IsEmpty)
                    {
                        switch (combineMode)
                        {
                            case CombineMode.Xor:
                            case CombineMode.Union:
                                return clipPath.Clone();

                            case CombineMode.Intersect:
                            case CombineMode.Exclude:
                                return new PdnGraphicsPath();

                            default:
                                throw new InvalidEnumArgumentException();
                        }
                    }
                    else if (clipPath.IsEmpty)
                    {
                        switch (combineMode)
                        {
                            case CombineMode.Exclude:
                            case CombineMode.Xor:
                            case CombineMode.Union:
                                return subjectPath.Clone();

                            case CombineMode.Intersect:
                                return new PdnGraphicsPath();

                            default:
                                throw new InvalidEnumArgumentException();
                        }
                    }
                    else
                    {
                        GraphicsPath resultPath = PdnGraphics.ClipPath(subjectPath, combineMode, clipPath);
                        return new PdnGraphicsPath(resultPath);
                    }

                default:
                    throw new InvalidEnumArgumentException();
            }
        }


        ~PdnGraphicsPath()
        {
            Changed();
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        { 
            if (disposing)
            {
                lock (this) // avoid race condition with GetObjectData()
                {
                    if (gdiPath != null)
                    {
                        gdiPath.Dispose();
                        gdiPath = null;
                    }

                    if (regionCache != null)
                    {
                        regionCache.Dispose();
                        regionCache = null;
                    }
                }
            }
        }

        public FillMode FillMode
        { 
            get 
            { 
                return gdiPath.FillMode; 
            }

            set 
            { 
                Changed(); 
                gdiPath.FillMode = value; 
            }
        }

        public PathData PathData
        { 
            get 
            { 
                return gdiPath.PathData; 
            }
        }

        public PointF[] PathPoints
        { 
            get 
            { 
                return gdiPath.PathPoints; 
            }
        }

        public byte[] PathTypes
        { 
            get 
            { 
                return gdiPath.PathTypes; 
            }
        }

        public int PointCount
        { 
            get 
            { 
                return gdiPath.PointCount; 
            }
        }

        public void AddArc(Rectangle rect, float startAngle, float sweepAngle)
        {
            Changed();
            gdiPath.AddArc(rect, startAngle, sweepAngle);
        }

        public void AddArc(RectangleF rectF, float startAngle, float sweepAngle)
        {
            Changed();
            gdiPath.AddArc(rectF, startAngle, sweepAngle);
        }

        public void AddArc(int x, int y, int width, int height, float startAngle, float sweepAngle)
        {
            Changed();
            gdiPath.AddArc(x, y, width, height, startAngle, sweepAngle);
        }

        public void AddArc(float x, float y, float width, float height, float startAngle, float sweepAngle)
        {
            Changed();
            gdiPath.AddArc(x, y, width, height, startAngle, sweepAngle);
        }

        public void AddBezier(Point pt1, Point pt2, Point pt3, Point pt4)
        {
            Changed();
            gdiPath.AddBezier(pt1, pt2, pt3, pt4);
        }

        public void AddBezier(PointF pt1, PointF pt2, PointF pt3, PointF pt4)
        {
            Changed();
            gdiPath.AddBezier(pt1, pt2, pt3, pt4);
        }

        public void AddBezier(int x1, int y1, int x2, int y2, int x3, int y3, int x4, int y4)
        {
            Changed();
            gdiPath.AddBezier(x1, y1, x2, y2, x3, y3, x4, y4);
        }

        public void AddBezier(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
        {
            Changed();
            gdiPath.AddBezier(x1, y1, x2, y2, x3, y3, x4, y4);
        }

        public void AddBeziers(Point[] points)
        {
            Changed();
            gdiPath.AddBeziers(points);
        }

        public void AddBeziers(PointF[] points)
        {
            Changed();
            gdiPath.AddBeziers(points);
        }

        public void AddClosedCurve(Point[] points)
        {
            Changed();
            gdiPath.AddClosedCurve(points);
        }

        public void AddClosedCurve(PointF[] points)
        {
            Changed();
            gdiPath.AddClosedCurve(points);
        }

        public void AddClosedCurve(Point[] points, float tension)
        {
            Changed();
            gdiPath.AddClosedCurve(points, tension);
        }

        public void AddClosedCurve(PointF[] points, float tension)
        {
            Changed();
            gdiPath.AddClosedCurve(points, tension);
        }

        public void AddCurve(Point[] points)
        {
            Changed();
            gdiPath.AddCurve(points);
        }

        public void AddCurve(PointF[] points)
        {
            Changed();
            gdiPath.AddCurve(points);
        }

        public void AddCurve(Point[] points, float tension)
        {
            Changed();
            gdiPath.AddCurve(points, tension);
        }

        public void AddCurve(PointF[] points, float tension)
        {
            Changed();
            gdiPath.AddCurve(points, tension);
        }

        public void AddCurve(Point[] points, int offset, int numberOfSegments, float tension)
        {
            Changed();
            gdiPath.AddCurve(points, offset, numberOfSegments, tension);
        }

        public void AddCurve(PointF[] points, int offset, int numberOfSegments, float tension)
        {
            Changed();
            gdiPath.AddCurve(points, offset, numberOfSegments, tension);
        }

        public void AddEllipse(Rectangle rect)
        {
            Changed();
            gdiPath.AddEllipse(rect);
        }

        public void AddEllipse(RectangleF rectF)
        {
            Changed();
            gdiPath.AddEllipse(rectF);
        }

        public void AddEllipse(int x, int y, int width, int height)
        {
            Changed();
            gdiPath.AddEllipse(x, y, width, height);
        }

        public void AddEllipse(float x, float y, float width, float height)
        {
            Changed();
            gdiPath.AddEllipse(x, y, width, height);
        }

        public void AddLine(Point pt1, Point pt2)
        {
            Changed();
            gdiPath.AddLine(pt1, pt2);
        }

        public void AddLine(PointF pt1, PointF pt2)
        {
            Changed();
            gdiPath.AddLine(pt1, pt2);
        }

        public void AddLine(int x1, int y1, int x2, int y2)
        {
            Changed();
            gdiPath.AddLine(x1, y1, x2, y2);
        }

        public void AddLine(float x1, float y1, float x2, float y2)
        {
            Changed();
            gdiPath.AddLine(x1, y1, x2, y2);
        }

        public void AddLines(Point[] points)
        {
            Changed();
            gdiPath.AddLines(points);
        }

        public void AddLines(PointF[] points)
        {
            Changed();
            gdiPath.AddLines(points);
        }

        public void AddPath(GraphicsPath addingPath, bool connect)
        {
            if (addingPath.PointCount != 0)
            {
                Changed();
                gdiPath.AddPath(addingPath, connect);
            }
        }

        public void AddPie(Rectangle rect, float startAngle, float sweepAngle)
        {
            Changed();
            gdiPath.AddPie(rect, startAngle, sweepAngle);
        }

        public void AddPie(int x, int y, int width, int height, float startAngle, float sweepAngle)
        {
            Changed();
            gdiPath.AddPie(x, y, width, height, startAngle, sweepAngle);
        }

        public void AddPie(float x, float y, float width, float height, float startAngle, float sweepAngle)
        {
            Changed();
            gdiPath.AddPie(x, y, width, height, startAngle, sweepAngle);
        }

        public void AddPolygon(Point[] points)
        {
            Changed();
            gdiPath.AddPolygon(points);
        }

        public void AddPolygon(PointF[] points)
        {
            Changed();
            gdiPath.AddPolygon(points);
        }

        public void AddPolygons(PointF[][] polygons)
        {
            foreach (PointF[] polygon in polygons)
            {
                AddPolygon(polygon);
                CloseFigure();
            }
        }

        public void AddPolygons(Point[][] polygons)
        {
            foreach (Point[] polygon in polygons)
            {
                AddPolygon(polygon);
                CloseFigure();
            }
        }

        public void AddRectangle(Rectangle rect)
        {
            Changed();
            gdiPath.AddRectangle(rect);
        }

        public void AddRectangle(RectangleF rectF)
        {
            Changed();
            gdiPath.AddRectangle(rectF);
        }

        public void AddRectangles(Rectangle[] rects)
        {
            Changed();
            gdiPath.AddRectangles(rects);
        }

        public void AddRectangles(RectangleF[] rectsF)
        {
            Changed();
            gdiPath.AddRectangles(rectsF);
        }

        public void AddString(string s, FontFamily family, int style, float emSize, Point origin, StringFormat format)
        {
            Changed();
            gdiPath.AddString(s, family, style, emSize, origin, format);
        }

        public void AddString(string s, FontFamily family, int style, float emSize, PointF origin, StringFormat format)
        {
            Changed();
            gdiPath.AddString(s, family, style, emSize, origin, format);
        }

        public void AddString(string s, FontFamily family, int style, float emSize, Rectangle layoutRect, StringFormat format)
        {
            Changed();
            gdiPath.AddString(s, family, style, emSize, layoutRect, format);
        }

        public void AddString(string s, FontFamily family, int style, float emSize, RectangleF layoutRect, StringFormat format)
        {
            Changed();
            gdiPath.AddString(s, family, style, emSize, layoutRect, format);
        }

        public void ClearMarkers()
        {
            Changed();
            gdiPath.ClearMarkers();
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        public PdnGraphicsPath Clone()
        {
            PdnGraphicsPath path = new PdnGraphicsPath((GraphicsPath)gdiPath.Clone());
            path.tooComplex = this.tooComplex;
            return path;
        }

        public void CloseAllFigures()
        {
            Changed();
            gdiPath.CloseAllFigures();
        }

        public void CloseFigure()
        {
            Changed();
            gdiPath.CloseFigure();
        }

        public void Draw(Graphics g, Pen pen)
        {
            Draw(g, pen, false);
        }

        /// <summary>
        /// Draws the path to the given Graphics context using the given Pen.
        /// </summary>
        /// <param name="g">The Graphics context to draw to.</param>
        /// <param name="pen">The Pen to draw with.</param>
        /// <param name="presentationIntent">
        /// If true, gives a hint that the path is being drawn to be presented to the user.
        /// </param>
        /// <remarks>
        /// If the path is "too complex," and if presentationIntent is true, then the path will
        /// not be drawn. To force the path to be drawn, set presentationIntent to false.
        /// </remarks>
        public void Draw(Graphics g, Pen pen, bool presentationIntent)
        {
            try
            {
                if (!tooComplex || !presentationIntent)
                {
                    int start = Environment.TickCount;
                    g.DrawPath(pen, this.gdiPath);
                    int end = Environment.TickCount;

                    if ((end - start) > 1000)
                    {
                        tooComplex = true;
                    }
                }
            }

            catch (OutOfMemoryException ex)
            {
                tooComplex = true;
                Tracing.Ping("DrawPath exception: " + ex);
            }
        }

        public void Flatten()
        {
            Changed();
            gdiPath.Flatten();
        }

        public void Flatten(Matrix matrix)
        {
            Changed();
            gdiPath.Flatten(matrix);
        }

        public void Flatten(Matrix matrix, float flatness)
        {
            Changed();
            gdiPath.Flatten(matrix, flatness);
        }

        public RectangleF GetBounds2()
        {
            if (this.PointCount == 0)
            {
                return RectangleF.Empty;
            }

            PointF[] points = this.PathPoints;

            if (points.Length == 0)
            {
                return RectangleF.Empty;
            }

            float left = points[0].X;
            float right = points[0].X;
            float top = points[0].Y;
            float bottom = points[0].Y;

            for (int i = 1; i < points.Length; ++i)
            {
                if (points[i].X < left)
                {
                    left = points[i].X;
                }

                if (points[i].Y < top)
                {
                    top = points[i].Y;
                }

                if (points[i].X > right)
                {
                    right = points[i].X;
                }

                if (points[i].Y > bottom)
                {
                    bottom = points[i].Y;
                }
            }

            return RectangleF.FromLTRB(left, top, right, bottom);
        }

        public RectangleF GetBounds()
        {
            return gdiPath.GetBounds();
        }

        public RectangleF GetBounds(Matrix matrix)
        {
            return gdiPath.GetBounds(matrix);
        }

        public RectangleF GetBounds(Matrix matrix, Pen pen)
        {
            return gdiPath.GetBounds(matrix, pen);
        }

        public PointF GetLastPoint()
        {
            return gdiPath.GetLastPoint();
        }

        public bool IsEmpty
        {
            get
            {
                return this.PointCount == 0;
            }
        }

        public bool IsOutlineVisible(Point point, Pen pen)
        {
            return gdiPath.IsOutlineVisible(point, pen);
        }

        public bool IsOutlineVisible(PointF point, Pen pen)
        {
            return gdiPath.IsOutlineVisible(point, pen);
        }

        public bool IsOutlineVisible(int x, int y, Pen pen)
        {
            return gdiPath.IsOutlineVisible(x, y, pen);
        }

        public bool IsOutlineVisible(Point point, Pen pen, Graphics g)
        {
            return gdiPath.IsOutlineVisible(point, pen, g);
        }

        public bool IsOutlineVisible(PointF point, Pen pen, Graphics g)
        {
            return gdiPath.IsOutlineVisible(point, pen, g);
        }

        public bool IsOutlineVisible(float x, float y, Pen pen)
        {
            return gdiPath.IsOutlineVisible(x, y, pen);
        }

        public bool IsOutlineVisible(int x, int y, Pen pen, Graphics g)
        {
            return gdiPath.IsOutlineVisible(x, y, pen, g);
        }

        public bool IsOutlineVisible(float x, float y, Pen pen, Graphics g)
        {
            return gdiPath.IsOutlineVisible(x, y, pen, g);
        }

        public bool IsVisible(Point point)
        {
            return gdiPath.IsVisible(point);
        }

        public bool IsVisible(PointF point)
        {
            return gdiPath.IsVisible(point);
        }

        public bool IsVisible(int x, int y)
        {
            return gdiPath.IsVisible(x, y);
        }

        public bool IsVisible(Point point, Graphics g)
        {
            return gdiPath.IsVisible(point, g);
        }

        public bool IsVisible(PointF point, Graphics g)
        {
            return gdiPath.IsVisible(point, g);
        }

        public bool IsVisible(float x, float y)
        {
            return gdiPath.IsVisible(x, y);
        }

        public bool IsVisible(int x, int y, Graphics g)
        {
            return gdiPath.IsVisible(x, y, g);
        }

        public bool IsVisible(float x, float y, Graphics g)
        {
            return gdiPath.IsVisible(x, y, g);
        }
        
        public void Reset()
        {
            Changed();
            this.tooComplex = false;
            gdiPath.Reset();
        }

        public void Reverse()
        {
            Changed();
            gdiPath.Reverse();
        }

        public void SetMarkers()
        {
            Changed();
            gdiPath.SetMarkers();
        }

        public void StartFigure()
        {
            Changed();
            gdiPath.StartFigure();
        }

        public void Transform(Matrix matrix)
        {
            Changed();
            gdiPath.Transform(matrix);
        }

        public void Warp(PointF[] destPoints, RectangleF srcRect)
        {
            Changed();
            gdiPath.Warp(destPoints, srcRect);
        }

        public void Warp(PointF[] destPoints, RectangleF srcRect, Matrix matrix)
        {
            Changed();
            gdiPath.Warp(destPoints, srcRect, matrix);
        }

        public void Warp(PointF[] destPoints, RectangleF srcRect, Matrix matrix, WarpMode warpMode)
        {
            Changed();
            gdiPath.Warp(destPoints, srcRect, matrix, warpMode);
        }

        public void Warp(PointF[] destPoints, RectangleF srcRect, Matrix matrix, WarpMode warpMode, float flatness)
        {
            Changed();
            gdiPath.Warp(destPoints, srcRect, matrix, warpMode, flatness);
        }

        public void Widen(Pen pen)
        {
            Changed();
            gdiPath.Widen(pen);
        }

        public void Widen(Pen pen, Matrix matrix)
        {
            Changed();
            gdiPath.Widen(pen, matrix);
        }

        public void Widen(Pen pen, Matrix matrix, float flatness)
        {
            Changed();
            gdiPath.Widen(pen, matrix, flatness);
        }
    }
}
