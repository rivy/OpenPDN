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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;

namespace PaintDotNet
{
    /// <summary>
    /// Designed as a proxy to the GDI+ Region class, while allowing for a
    /// replacement that won't break code. The main reason for having this
    /// right now is to work around some bugs in System.Drawing.Region,
    /// especially the memory leak in GetRegionScans().
    /// </summary>
    [Serializable]
    public sealed class PdnRegion
        : ISerializable,
          IDisposable
    {
        private object lockObject = new object();
        private Region gdiRegion;
        private bool changed = true;
        private int cachedArea = -1;
        private Rectangle cachedBounds = Rectangle.Empty;
        private RectangleF[] cachedRectsF = null;
        private Rectangle[] cachedRects = null;

        public object SyncRoot
        {
            get
            {
                return lockObject;
            }
        }

        public int GetArea()
        {
            lock (SyncRoot)
            {
                int theCachedArea = cachedArea;

                if (theCachedArea == -1)
                {
                    int ourCachedArea = 0;

                    foreach (Rectangle rect in GetRegionScansReadOnlyInt())
                    {
                        try
                        {
                            ourCachedArea += rect.Width * rect.Height;
                        }

                        catch (System.OverflowException)
                        {
                            ourCachedArea = int.MaxValue;
                            break;
                        }
                    }

                    cachedArea = ourCachedArea;
                    return ourCachedArea;
                }
                else
                {
                    return theCachedArea;
                }
            }
        }

        private bool IsChanged()
        {
            return this.changed;
        }

        private void Changed()
        {
            lock (SyncRoot)
            {
                this.changed = true;
                this.cachedArea = -1;
                this.cachedBounds = Rectangle.Empty;
            }
        }

        private void ResetChanged()
        {
            lock (SyncRoot)
            {
                this.changed = false;
            }
        }

        public PdnRegion()
        {
            this.gdiRegion = new Region();
        }

        public PdnRegion(GraphicsPath path)
        {
            this.gdiRegion = new Region(path);
        }

        public PdnRegion(PdnGraphicsPath pdnPath)
            : this(pdnPath.GetRegionCache())
        {
        }

        public PdnRegion(Rectangle rect)
        {
            this.gdiRegion = new Region(rect);
        }

        public PdnRegion(RectangleF rectF)
        {
            this.gdiRegion = new Region(rectF);
        }

        public PdnRegion(RegionData regionData)
        {
            this.gdiRegion = new Region(regionData);
        }

        public PdnRegion(Region region, bool takeOwnership)
        {
            if (takeOwnership)
            {
                this.gdiRegion = region;
            }
            else
            {
                this.gdiRegion = region.Clone();
            }
        }

        public PdnRegion(Region region)
            : this(region, false)
        {
        }

        private PdnRegion(PdnRegion pdnRegion)
        {
            lock (pdnRegion.SyncRoot)
            {
                this.gdiRegion = pdnRegion.gdiRegion.Clone();
                this.changed = pdnRegion.changed;
                this.cachedArea = pdnRegion.cachedArea;
                this.cachedRectsF = pdnRegion.cachedRectsF; 
                this.cachedRects = pdnRegion.cachedRects;
            }
        }

        // This constructor is used by WrapRegion. The boolean parameter is just
        // there because we already have a parameterless contructor
        private PdnRegion(bool sentinel)
        {
        }

        public static PdnRegion CreateEmpty()
        {
            PdnRegion region = new PdnRegion();
            region.MakeEmpty();
            return region;
        }

        public static PdnRegion WrapRegion(Region region)
        {
            PdnRegion pdnRegion = new PdnRegion(false);
            pdnRegion.gdiRegion = region;
            return pdnRegion;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("PdnRegion");
            }

            RegionData regionData;

            lock (SyncRoot)
            {
                regionData = this.gdiRegion.GetRegionData();
            }

            byte[] data = regionData.Data;
            info.AddValue("data", data);
        }

        public PdnRegion(SerializationInfo info, StreamingContext context)
        {
            byte[] data = (byte[])info.GetValue("data", typeof(byte[]));

            using (Region region = new Region())
            {
                RegionData regionData = region.GetRegionData();
                regionData.Data = data;
                this.gdiRegion = new Region(regionData);
            }

            this.lockObject = new object();
            this.cachedArea = -1;
            this.cachedBounds = Rectangle.Empty;
            this.changed = true;
            this.cachedRects = null;
            this.cachedRectsF = null;
        }

        public PdnRegion Clone()
        {
            return new PdnRegion(this);
        }

        ~PdnRegion()
        {
            Dispose(false);
        }

        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    lock (SyncRoot)
                    {
                        gdiRegion.Dispose();
                        gdiRegion = null;
                    }
                }

                disposed = true;
            }
        }

        public Region GetRegionReadOnly()
        {
            return this.gdiRegion;
        }

        public void Complement(GraphicsPath path)
        {
            lock (SyncRoot)
            {
                Changed();
                gdiRegion.Complement(path);
            }
        }

        public void Complement(Rectangle rect)
        {
            lock (SyncRoot)
            {
                Changed();
                gdiRegion.Complement(rect);
            }
        }

        public void Complement(RectangleF rectF)
        {
            lock (SyncRoot)
            {
                Changed();
                gdiRegion.Complement(rectF);
            }
        }

        public void Complement(Region region)
        {
            lock (SyncRoot)
            {
                Changed();
                gdiRegion.Complement(region);
            }
        }

        public void Complement(PdnRegion region2)
        {
            lock (SyncRoot)
            {
                Changed();
                gdiRegion.Complement(region2.gdiRegion);
            }
        }

        public void Exclude(GraphicsPath path)
        {
            lock (SyncRoot)
            {
                gdiRegion.Exclude(path);
            }
        }

        public void Exclude(Rectangle rect)
        {
            lock (SyncRoot)
            {
                gdiRegion.Exclude(rect);
            }
        }

        public void Exclude(RectangleF rectF)
        {
            lock (SyncRoot)
            {
                gdiRegion.Exclude(rectF);
            }
        }

        public void Exclude(Region region)
        {
            lock (SyncRoot)
            {
                gdiRegion.Exclude(region);
            }
        }

        public void Exclude(PdnRegion region2)
        {
            lock (SyncRoot)
            {
                gdiRegion.Exclude(region2.gdiRegion);
            }
        }

        public RectangleF GetBounds(Graphics g)
        {
            lock (SyncRoot)
            {
                return gdiRegion.GetBounds(g);
            }
        }

        public RectangleF GetBounds()
        {
            lock (SyncRoot)
            {
                using (SystemLayer.NullGraphics nullGraphics = new SystemLayer.NullGraphics())
                {
                    return gdiRegion.GetBounds(nullGraphics.Graphics);
                }
            }
        }

        public Rectangle GetBoundsInt()
        {
            Rectangle bounds;

            lock (SyncRoot)
            {
                bounds = this.cachedBounds;

                if (bounds == Rectangle.Empty)
                {
                    Rectangle[] rects = GetRegionScansReadOnlyInt();
            
                    if (rects.Length == 0)
                    {
                        return Rectangle.Empty;
                    }

                    bounds = rects[0];

                    for (int i = 1; i < rects.Length; ++i)
                    {
                        bounds = Rectangle.Union(bounds, rects[i]);
                    }

                    this.cachedBounds = bounds;
                }
            }

            return bounds;
        }

        public RegionData GetRegionData()
        {
            lock (SyncRoot)
            {
                return gdiRegion.GetRegionData();
            }
        }

        public RectangleF[] GetRegionScans()
        {
            return (RectangleF[])GetRegionScansReadOnly().Clone();
        }

        /// <summary>
        /// This is an optimized version of GetRegionScans that returns a reference to the array
        /// that is used to cache the region scans. This mitigates performance when this array
        /// is requested many times on an unmodified PdnRegion.
        /// Thus, by using this method you are promising to not modify the array that is returned.
        /// </summary>
        /// <returns></returns>
        public RectangleF[] GetRegionScansReadOnly()
        {
            lock (this.SyncRoot)
            {
                if (this.changed)
                {
                    UpdateCachedRegionScans();
                }

                if (this.cachedRectsF == null)
                {
                    this.cachedRectsF = new RectangleF[cachedRects.Length];

                    for (int i = 0; i < this.cachedRectsF.Length; ++i)
                    {
                        this.cachedRectsF[i] = (RectangleF)this.cachedRects[i];
                    }
                }

                return this.cachedRectsF;
            }
        }

        public Rectangle[] GetRegionScansInt()
        {
            return (Rectangle[])GetRegionScansReadOnlyInt().Clone();
        }

        public Rectangle[] GetRegionScansReadOnlyInt()
        {
            lock (this.SyncRoot)
            {
                if (this.changed)
                {
                    UpdateCachedRegionScans();
                }

                return this.cachedRects;
            }
        }

        private unsafe void UpdateCachedRegionScans()
        {
            // Assumes we are in a lock(SyncRoot){} block
            SystemLayer.PdnGraphics.GetRegionScans(this.gdiRegion, out cachedRects, out cachedArea);
            this.cachedRectsF = null; // only update this when specifically asked for it
        }
                
        public void Intersect(GraphicsPath path)
        {
            lock (SyncRoot)
            {
                Changed();
                gdiRegion.Intersect(path);
            }
        }

        public void Intersect(Rectangle rect)
        {
            lock (SyncRoot)
            {
                Changed();
                gdiRegion.Intersect(rect);
            }
        }

        public void Intersect(RectangleF rectF)
        {
            lock (SyncRoot)
            {
                Changed();
                gdiRegion.Intersect(rectF);
            }
        }

        public void Intersect(Region region)
        {
            lock (SyncRoot)
            {
                Changed();
                gdiRegion.Intersect(region);
            }
        }

        public void Intersect(PdnRegion region2)
        {
            lock (SyncRoot)
            {
                Changed();
                gdiRegion.Intersect(region2.gdiRegion);
            }
        }

        public bool IsEmpty(Graphics g)
        {
            lock (SyncRoot)
            {
                return gdiRegion.IsEmpty(g);
            }
        }

        public bool IsEmpty()
        {
            return GetArea() == 0;
        }

        public bool IsInfinite(Graphics g)
        {
            lock (SyncRoot)
            {
                return gdiRegion.IsInfinite(g);
            }
        }

        public bool IsVisible(Point point)
        {
            lock (SyncRoot)
            {
                return gdiRegion.IsVisible(point);
            }
        }

        public bool IsVisible(PointF pointF)
        {
            lock (SyncRoot)
            {
                return gdiRegion.IsVisible(pointF);
            }
        }

        public bool IsVisible(Rectangle rect)
        {
            lock (SyncRoot)
            {
                return gdiRegion.IsVisible(rect);
            }
        }

        public bool IsVisible(RectangleF rectF)
        {
            lock (SyncRoot)
            {
                return gdiRegion.IsVisible(rectF);
            }
        }

        public bool IsVisible(Point point, Graphics g)
        {
            lock (SyncRoot)
            {
                return gdiRegion.IsVisible(point, g);
            }
        }
        
        public bool IsVisible(PointF pointF, Graphics g)
        {
            lock (SyncRoot)
            {
                return gdiRegion.IsVisible(pointF, g);
            }
        }

        public bool IsVisible(Rectangle rect, Graphics g)
        {
            lock (SyncRoot)
            {
                return gdiRegion.IsVisible(rect, g);
            }
        }

        public bool IsVisible(RectangleF rectF, Graphics g)
        {
            lock (SyncRoot)
            {
                return gdiRegion.IsVisible(rectF, g);
            }
        }

        public bool IsVisible(float x, float y)
        {
            lock (SyncRoot)
            {
                return gdiRegion.IsVisible(x, y);
            }
        }

        public bool IsVisible(int x, int y, Graphics g)
        {
            lock (SyncRoot)
            {
                return gdiRegion.IsVisible(x, y, g);
            }
        }

        public bool IsVisible(float x, float y, Graphics g)
        {
            lock (SyncRoot)
            {
                return gdiRegion.IsVisible(x, y, g);
            }
        }

        public bool IsVisible(int x, int y, int width, int height)
        {
            lock (SyncRoot)
            {
                return gdiRegion.IsVisible(x, y, width, height);
            }
        }

        public bool IsVisible(float x, float y, float width, float height)
        {
            lock (SyncRoot)
            {
                return gdiRegion.IsVisible(x, y, width, height);
            }
        }

        public bool IsVisible(int x, int y, int width, int height, Graphics g)
        {
            lock (SyncRoot)
            {
                return gdiRegion.IsVisible(x, y, width, height, g);
            }
        }

        public bool IsVisible(float x, float y, float width, float height, Graphics g)
        {
            lock (SyncRoot)
            {
                return gdiRegion.IsVisible(x, y, width, height, g);
            }
        }

        public void MakeEmpty()
        {
            lock (SyncRoot)
            {
                Changed();
                gdiRegion.MakeEmpty();
            }
        }

        public void MakeInfinite()
        {
            lock (SyncRoot)
            {
                Changed();
                gdiRegion.MakeInfinite();
            }
        }

        public void Transform(Matrix matrix)
        {
            lock (SyncRoot)
            {
                Changed();
                gdiRegion.Transform(matrix);
            }
        }

        public void Union(GraphicsPath path)
        {
            lock (SyncRoot)
            {
                Changed();
                gdiRegion.Union(path);
            }
        }

        public void Union(Rectangle rect)
        {
            lock (SyncRoot)
            {
                Changed();
                gdiRegion.Union(rect);
            }
        }

        public void Union(RectangleF rectF)
        {
            lock (SyncRoot)
            {
                Changed();
                gdiRegion.Union(rectF);
            }
        }

        public void Union(RectangleF[] rectsF)
        {
            lock (SyncRoot)
            {
                Changed();

                using (PdnRegion tempRegion = Utility.RectanglesToRegion(rectsF))
                {
                    this.Union(tempRegion);
                }
            }
        }

        public void Union(Region region)
        {
            lock (SyncRoot)
            {
                Changed();
                gdiRegion.Union(region);
            }
        }

        public void Union(PdnRegion region2)
        {
            lock (SyncRoot)
            {
                Changed();
                gdiRegion.Union(region2.gdiRegion);
            }
        }

        public void Xor(Rectangle rect)
        {
            lock (SyncRoot)
            {
                Changed();
                gdiRegion.Xor(rect);
            }
        }

        public void Xor(RectangleF rectF)
        {
            lock (SyncRoot)
            {
                Changed();
                gdiRegion.Xor(rectF);
            }
        }

        public void Xor(Region region)
        {
            lock (SyncRoot)
            {
                Changed();
                gdiRegion.Xor(region);
            }
        }

        public void Xor(PdnRegion region2)
        {
            lock (SyncRoot)
            {
                Changed();
                gdiRegion.Xor(region2.gdiRegion);
            }
        }
    }
}
