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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.Serialization;
using System.Threading;

namespace PaintDotNet
{
    /// <summary>
    /// Defines a surface that is irregularly shaped, defined by a Region.
    /// Works by encapsulating a Surface that that is the size of the region's
    /// bounding box, and then storing the region to mask drawing operations.
    /// Similar to IrregularImage, makes working with transformations much
    /// easier.
    /// Instances of this class are immutable once created.
    /// This class is not thread-safe, and its properties and fields must only
    /// be executing in one thread at a time. However, it may be serialized
    /// by one thread while being accessed in the aforementioned manner by
    /// another thread. It may not be serialized by more than one thread at once.
    /// </summary>
    [Serializable]
    public sealed class MaskedSurface
        : ICloneable,
          IDisposable,
          IDeserializationCallback
    {
        private bool disposed = false;
        private Surface surface;

        // Use one of these
        private PdnRegion region;

        // Whenever you set this field, you must Clone() it and store that into shadowPath -- 
        // in other words, use SetPathField() and never set this field directly
        // And, never call methods or properties of the path field's object, always use shadowPath.
        // (it is fine to do a null check on path)
        private PdnGraphicsPath path;

        private void SetPathField(PdnGraphicsPath newPath)
        {
            this.path = newPath;
            this.shadowPath = newPath.Clone();
        }

        // We create one copy of the path so that when we go to Draw(), we can Clone() it without
        // running into a race condition. PdnGraphicsPath is not thread safe, and the MoveTool
        // is going the performant route of serializing data in the background while continuing
        // on with its work. This work includes calling our Draw() method which then Clone()s the
        // path while the path is still being serialized. Since GDI+ is fussy about cross-thread 
        // use of its objects, it throws an exception.
        // This does not introduce thread safety into PdnGraphicsPath, but it does relieve a certain
        // amount of transitive responsibility from users of this class.
        [NonSerialized]
        private PdnGraphicsPath shadowPath;

        [NonSerialized]
        private static PaintDotNet.Threading.ThreadPool threadPool = new PaintDotNet.Threading.ThreadPool();

        /// <summary>
        /// Do not modify the surface. Treat it as immutable.
        /// </summary>
        public Surface SurfaceReadOnly
        {
            get
            {
                return this.surface;
            }
        }

        public bool IsDisposed
        {
            get
            {
                return this.disposed;
            }
        }

        private PdnRegion GetRegion()
        {
            if (this.region == null)
            {
                this.region = new PdnRegion(this.shadowPath);
            }

            return this.region;
        }

        public PdnRegion CreateRegion()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("MaskedSurface");
            }

            return GetRegion().Clone();
        }

        private PdnGraphicsPath GetPath()
        {
            if (this.path == null)
            {
                // TODO: FromRegion() is a VERY expensive call!
                PdnGraphicsPath newPath = PdnGraphicsPath.FromRegion(this.region);
                SetPathField(newPath);
            }

            return this.shadowPath;
        }

        public PdnGraphicsPath CreatePath()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("MaskedSurface");
            }

            return GetPath().Clone();
        }

        private MaskedSurface()
        {
        }

        /// <summary>
        /// Constructs a MaskSurface by copying the given region-of-interest from an Image.
        /// </summary>
        /// <param name="source">The Surface to copy pixels from.</param>
        /// <param name="roi">Defines the Region from which to copy pixels from the Image.</param>
        public MaskedSurface(Surface source, PdnRegion roi)
        {
            PdnRegion roiClipped = (PdnRegion)roi.Clone();
            roiClipped.Intersect(source.Bounds);

            Rectangle boundsClipped = roiClipped.GetBoundsInt();
            this.surface = new Surface(boundsClipped.Size);
            this.surface.Clear(ColorBgra.FromUInt32(0x00ffffff));

            Rectangle rect = boundsClipped;
            Point dstOffset = new Point(rect.X - boundsClipped.X, rect.Y - boundsClipped.Y);
            this.surface.CopySurface(source, dstOffset, rect);

            this.region = roiClipped;
            // TODO: FromRegion() is a VERY expensive call for what we are doing!
            PdnGraphicsPath newPath = PdnGraphicsPath.FromRegion(this.region);
            SetPathField(newPath);
        }

        public MaskedSurface(Surface source, PdnGraphicsPath path)
        {
            RectangleF boundsF = path.GetBounds();
            Rectangle bounds = Utility.RoundRectangle(boundsF);

            Rectangle boundsClipped = Rectangle.Intersect(bounds, source.Bounds);
            Rectangle boundsRead;

            if (bounds != boundsClipped)
            {
                PdnRegion region = new PdnRegion(path);
                region.Intersect(source.Bounds);
                SetPathField(PdnGraphicsPath.FromRegion(region));
                this.region = region;
                boundsRead = region.GetBoundsInt();
            }
            else
            {
                SetPathField(path.Clone());
                this.region = new PdnRegion(this.path);
                boundsRead = boundsClipped;
            }

            if (boundsRead.Width > 0 && boundsRead.Height > 0)
            {
                this.surface = new Surface(boundsRead.Size);
                this.surface.CopySurface(source, boundsRead);
            }
            else
            {
                this.surface = null;
            }
        }

        public void OnDeserialization(object sender)
        {
            threadPool = new PaintDotNet.Threading.ThreadPool();

            if (this.path != null)
            {
                this.shadowPath = this.path.Clone();
            }
        }

        public MaskedSurface Clone()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("MaskedSurface");
            }

            MaskedSurface ms = new MaskedSurface();

            if (this.region != null)
            {
                ms.region = this.region.Clone();
            }

            if (this.path != null)
            {
                ms.SetPathField(this.shadowPath.Clone());
            }

            if (this.surface != null)
            {
                ms.surface = this.surface.Clone();
            }

            return ms;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        ~MaskedSurface()
        {
            Dispose(false);
        }

        // Constants that define fixed-point precision and arithmetic
        private const int fp_ShiftFactor = 14;
        private const float fp_MultFactor = (float)(1 << fp_ShiftFactor);
        private const float fp_MaxValue = (float)((1 << (31 - fp_ShiftFactor)) - 1);
        private const int fp_RoundFactor = ((1 << fp_ShiftFactor) >> 1) - 1;

        private class DrawContext
        {
            public Surface src;
            public Surface dst;
            public float dsxddx;
            public float dsyddx;
            public float dsxddy;
            public float dsyddy;
            public int fp_dsxddx;
            public int fp_dsyddx;
            public int fp_dsxddy;
            public int fp_dsyddy;
            public Rectangle[] dstScans;
            public Matrix[] inverses;
            public int boundsX;
            public int boundsY;

            private static int Clamp(int x, int min, int max)
            {
                if (x < min)
                {
                    return min;
                }
                else if (x > max)
                {
                    return max;
                }
                else
                {
                    return x;
                }
            }

            public unsafe void DrawScansNearestNeighbor(object cpuNumberObj)
            {
                int cpuNumber = (int)cpuNumberObj;
                int inc = Processor.LogicalCpuCount;
                void* scan0 = src.Scan0.VoidStar;
                int stride = src.Stride;
                PointF[] pts = new PointF[1];

                for (int i = cpuNumber; i < this.dstScans.Length; i += inc)
                {
                    Rectangle dstRect = this.dstScans[i];

                    dstRect.Intersect(dst.Bounds);

                    if (dstRect.Width == 0 || dstRect.Height == 0)
                    {
                        continue;
                    }

                    pts[0] = new PointF(dstRect.Left, dstRect.Top);

                    this.inverses[cpuNumber].TransformPoints(pts);

                    pts[0].X -= this.boundsX;
                    pts[0].Y -= this.boundsY;

                    int fp_srcPtRowX = (int)(pts[0].X * fp_MultFactor);
                    int fp_srcPtRowY = (int)(pts[0].Y * fp_MultFactor);

                    for (int dstY = dstRect.Top; dstY < dstRect.Bottom; ++dstY)
                    {
                        int fp_srcPtColX = fp_srcPtRowX;
                        int fp_srcPtColY = fp_srcPtRowY;
                        fp_srcPtRowX += this.fp_dsxddy;
                        fp_srcPtRowY += this.fp_dsyddy;

                        if (dstY >= 0)
                        {
                            // We render the left side, then the right side, then the in-between pixels.
                            // The reason for this is that the left and right sides have the chance that,
                            // due to lack of enough precision, we will dive off the end of the surface
                            // and into memory that we can't actually read from. To solve this, the left
                            // and right sides will clamp the pixel coordinates that they read from,
                            // and then keep track of when they were able to stomp clamping these values.
                            // Then, rendering the middle part of the scanline is able to be completed
                            // without the expensive clamping operation.

                            int dstX = dstRect.Left;
                            ColorBgra* dstPtr = dst.GetPointAddress(dstX, dstY);
                            ColorBgra* dstPtrEnd = dstPtr + dstRect.Width;
                            int fp_srcPtColLastX = fp_srcPtColX + (this.fp_dsxddx * (dstRect.Width - 1));
                            int fp_srcPtColLastY = fp_srcPtColY + (this.fp_dsyddx * (dstRect.Width - 1));

                            // Left side
                            while (dstPtr < dstPtrEnd)
                            {
                                int srcPtColX = (fp_srcPtColX + fp_RoundFactor) >> fp_ShiftFactor;
                                int srcPtColY = (fp_srcPtColY + fp_RoundFactor) >> fp_ShiftFactor;

                                int srcX = Clamp(srcPtColX, 0, src.Width - 1);
                                int srcY = Clamp(srcPtColY, 0, src.Height - 1);
                                *dstPtr = this.src.GetPointUnchecked(srcX, srcY);

                                ++dstPtr;
                                fp_srcPtColX += this.fp_dsxddx;
                                fp_srcPtColY += this.fp_dsyddx;

                                if (srcX == srcPtColX && srcY == srcPtColY)
                                {
                                    break;
                                }
                            }

                            ColorBgra* startFastPtr = dstPtr;
                            dstPtr = dstPtrEnd - 1;

                            // Right side
                            while (dstPtr >= startFastPtr)
                            {
                                int srcPtColX = (fp_srcPtColLastX + fp_RoundFactor) >> fp_ShiftFactor;
                                int srcPtColY = (fp_srcPtColLastY + fp_RoundFactor) >> fp_ShiftFactor;

                                int srcX = Clamp(srcPtColX, 0, src.Width - 1);
                                int srcY = Clamp(srcPtColY, 0, src.Height - 1);
                                *dstPtr = this.src.GetPointUnchecked(srcX, srcY);

                                if (srcX == srcPtColX && srcY == srcPtColY)
                                {
                                    break;
                                }

                                --dstPtr;
                                fp_srcPtColLastX -= this.fp_dsxddx;
                                fp_srcPtColLastY -= this.fp_dsyddx;
                            }

                            ColorBgra* endFastPtr = dstPtr;

                            // Middle
                            while (startFastPtr < endFastPtr)
                            {
                                int srcPtColX = (fp_srcPtColX + fp_RoundFactor) >> fp_ShiftFactor;
                                int srcPtColY = (fp_srcPtColY + fp_RoundFactor) >> fp_ShiftFactor;

                                // This is GetPointUnchecked inlined -- avoid the overhead, especially, of the call to MemoryBlock.VoidStar
                                // which has the potential to throw an exception which is then NOT inlined.
                                startFastPtr->Bgra = (unchecked(srcPtColX + (ColorBgra *)(((byte *)scan0) + (srcPtColY * stride))))->Bgra;

                                ++startFastPtr;
                                fp_srcPtColX += this.fp_dsxddx;
                                fp_srcPtColY += this.fp_dsyddx;
                            }
                        }
                    }
                }
            }

            public unsafe void DrawScansBilinear(object cpuNumberObj)
            {
                int cpuNumber = (int)cpuNumberObj;
                int inc = Processor.LogicalCpuCount;
                PointF[] pts2 = new PointF[1];

                for (int i = cpuNumber; i < this.dstScans.Length; i += inc)
                {
                    Rectangle dstRect = this.dstScans[i];
                    dstRect.Intersect(dst.Bounds);

                    pts2[0] = new PointF(dstRect.Left, dstRect.Top);

                    this.inverses[cpuNumber].TransformPoints(pts2);

                    pts2[0].X -= this.boundsX;
                    pts2[0].Y -= this.boundsY;

                    // Sometimes pts2 ends up being infintessimally small (1 x 10^-5 or -6) but negative
                    // This throws off GetBilinearSample and it returns transparent colors, which looks horribly wrong.
                    // So we fix that!
                    if (pts2[0].X < 0)
                    {
                        pts2[0].X = 0;
                    }

                    if (pts2[0].Y < 0)
                    {
                        pts2[0].Y = 0;
                    }

                    PointF srcPtRow = pts2[0];

                    for (int dstY = dstRect.Top; dstY < dstRect.Bottom; ++dstY)
                    {
                        PointF srcPtCol = srcPtRow;
                        srcPtRow.X += this.dsxddy;
                        srcPtRow.Y += this.dsyddy;

                        if (dstY >= 0)
                        {
                            int dstX = dstRect.Left;

                            while (dstX < dstRect.Right)
                            {
                                ColorBgra srcPixel = this.src.GetBilinearSample(srcPtCol.X, srcPtCol.Y);
                                ColorBgra* dstPtr = dst.GetPointAddressUnchecked(dstX, dstY);
                                *dstPtr = srcPixel;

                                srcPtCol.X += this.dsxddx;
                                srcPtCol.Y += this.dsyddx;
                                ++dstX;
                            }
                        }
                    }
                }
            }
        }

        public void Draw(Surface dst)
        {
            Draw(dst, 0, 0);
        }

        public void Draw(Surface dst, int tX, int tY)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("MaskedSurface");
            }

            using (Matrix m = new Matrix())
            {
                m.Reset();
                m.Translate(tX, tY, MatrixOrder.Append);
                Draw(dst, m, ResamplingAlgorithm.Bilinear);
            }
        }

        public unsafe void Draw(Surface dst, Matrix transform, ResamplingAlgorithm sampling)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("MaskedSurface");
            }

            if (this.surface == null || !transform.IsInvertible)
            {
                return;
            }

            PdnRegion theRegion;
            Rectangle regionBounds;

            if (this.path == null)
            {
                theRegion = this.region.Clone();
                regionBounds = this.region.GetBoundsInt();
                theRegion.Transform(transform);
            }
            else
            {
                using (PdnGraphicsPath mPath = this.shadowPath.Clone())
                {
                    regionBounds = Rectangle.Truncate(mPath.GetBounds());
                    mPath.Transform(transform);
                    theRegion = new PdnRegion(mPath);
                }
            }

            DrawContext dc = new DrawContext();

            dc.boundsX = regionBounds.X;
            dc.boundsY = regionBounds.Y;

            Matrix inverse = transform.Clone();
            inverse.Invert();

            dc.inverses = new Matrix[Processor.LogicalCpuCount];
            for (int i = 0; i < dc.inverses.Length; ++i)
            {
                dc.inverses[i] = inverse.Clone();
            }

            // change in source-[X|Y] w.r.t. destination-[X|Y]
            PointF[] pts = new PointF[] {
                                            new PointF(1, 0),
                                            new PointF(0, 1)
                                        };

            inverse.TransformVectors(pts);
            inverse.Dispose();
            inverse = null;

            dc.dsxddx = pts[0].X;

            if (Math.Abs(dc.dsxddx) > fp_MaxValue)
            {
                dc.dsxddx = 0.0f;
            }

            dc.dsyddx = pts[0].Y;

            if (Math.Abs(dc.dsyddx) > fp_MaxValue)
            {
                dc.dsyddx = 0.0f;
            }

            dc.dsxddy = pts[1].X;

            if (Math.Abs(dc.dsxddy) > fp_MaxValue)
            {
                dc.dsxddy = 0.0f;
            }

            dc.dsyddy = pts[1].Y;

            if (Math.Abs(dc.dsyddy) > fp_MaxValue)
            {
                dc.dsyddy = 0.0f;
            }

            dc.fp_dsxddx = (int)(dc.dsxddx * fp_MultFactor);
            dc.fp_dsyddx = (int)(dc.dsyddx * fp_MultFactor);
            dc.fp_dsxddy = (int)(dc.dsxddy * fp_MultFactor);
            dc.fp_dsyddy = (int)(dc.dsyddy * fp_MultFactor);

            dc.dst = dst;
            dc.src = this.surface;
            Rectangle[] scans = theRegion.GetRegionScansReadOnlyInt();

            if (scans.Length == 1)
            {
                dc.dstScans = new Rectangle[Processor.LogicalCpuCount];
                Utility.SplitRectangle(scans[0], dc.dstScans);
            }
            else
            {
                dc.dstScans = scans;
            }

            WaitCallback wc;

            switch (sampling)
            {
                case ResamplingAlgorithm.NearestNeighbor:
                    wc = new WaitCallback(dc.DrawScansNearestNeighbor);
                    break;

                case ResamplingAlgorithm.Bilinear:
                    wc = new WaitCallback(dc.DrawScansBilinear);
                    break;

                default:
                    throw new System.ComponentModel.InvalidEnumArgumentException();
            }

            for (int i = 0; i < Processor.LogicalCpuCount; ++i)
            {
                if (i == Processor.LogicalCpuCount - 1)
                {
                    // Don't queue the last work item into a separate thread
                    wc(BoxedConstants.GetInt32(i));
                }
                else
                {
                    threadPool.QueueUserWorkItem(wc, BoxedConstants.GetInt32(i));
                }
            }

            threadPool.Drain();

            for (int i = 0; i < Processor.LogicalCpuCount; ++i)
            {
                dc.inverses[i].Dispose();
                dc.inverses[i] = null;
            }

            dc.src = null;

            theRegion.Dispose();
            theRegion = null;
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
                if (this.surface != null)
                {
                    this.surface.Dispose();
                    this.surface = null;
                }

                if (this.region != null)
                {
                    this.region.Dispose();
                    this.region = null;
                }

                if (this.path != null)
                {
                    this.path.Dispose();
                    this.path = null;
                }

                if (this.shadowPath != null)
                {
                    this.shadowPath.Dispose();
                    this.shadowPath = null;
                }
            }

            this.disposed = true;
        }
    }
}
