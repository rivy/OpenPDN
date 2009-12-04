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
using System.Drawing;
using System.Runtime.Serialization;

namespace PaintDotNet
{
    /// <summary>
    /// Defines a surface that is irregularly shaped, defined by a Region.
    /// Works by containing an array of PlacedSurface instances.
    /// Similar to IrregularImage, but works with Surface objects instead.
    /// Instances of this class are immutable once created.
    /// </summary>
    [Serializable]
    public sealed class IrregularSurface
        : ISurfaceDraw,
          IDisposable,
          ICloneable,
          IDeserializationCallback
    {
        private ArrayList placedSurfaces;

        [NonSerialized]
        private PdnRegion region;

        /// <summary>
        /// The Region that the irregular image fills.
        /// </summary>
        public PdnRegion Region
        {
            get
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException("IrregularSurface");
                }

                return this.region;
            }
        }

        /// <summary>
        /// Constructs an IrregularSurface by copying the given region-of-interest from an Image.
        /// </summary>
        /// <param name="source">The Surface to copy pixels from.</param>
        /// <param name="roi">Defines the Region from which to copy pixels from the Image.</param>
        public IrregularSurface (Surface source, PdnRegion roi)
        {   
            PdnRegion roiClipped = (PdnRegion)roi.Clone();
            roiClipped.Intersect(source.Bounds);

            Rectangle[] rects = roiClipped.GetRegionScansReadOnlyInt();
            this.placedSurfaces = new ArrayList(rects.Length);

            foreach (Rectangle rect in rects)
            {
                this.placedSurfaces.Add(new PlacedSurface(source, rect));
            }

            this.region = roiClipped;
        }

        public IrregularSurface (Surface source, RectangleF[] roi)
        {
            this.placedSurfaces = new ArrayList(roi.Length);

            foreach (RectangleF rectF in roi)
            {
                RectangleF ri = RectangleF.Intersect(source.Bounds, rectF);

                if (!ri.IsEmpty)
                {
                    this.placedSurfaces.Add(new PlacedSurface(source, Rectangle.Truncate(ri)));
                }
            }

            this.region = Utility.RectanglesToRegion(roi);
            this.region.Intersect(source.Bounds);
        }

        public IrregularSurface(Surface source, Rectangle[] roi)
        {
            this.placedSurfaces = new ArrayList(roi.Length);

            foreach (Rectangle rect in roi)
            {
                Rectangle ri = Rectangle.Intersect(source.Bounds, rect);

                if (!ri.IsEmpty)
                {
                    this.placedSurfaces.Add(new PlacedSurface(source, ri));
                }
            }

            this.region = Utility.RectanglesToRegion(roi);
            this.region.Intersect(source.Bounds);
        }

        /// <summary>
        /// Constructs an IrregularSurface by copying the given rectangle-of-interest from an Image.
        /// </summary>
        /// <param name="source">The Surface to copy pixels from.</param>
        /// <param name="roi">Defines the Rectangle from which to copy pixels from the Image.</param>
        public IrregularSurface (Surface source, Rectangle roi)
        {
            this.placedSurfaces = new ArrayList();
            this.placedSurfaces.Add(new PlacedSurface(source, roi));
            this.region = new PdnRegion(roi);
        }

        private IrregularSurface (IrregularSurface cloneMe)
        {
            this.placedSurfaces = new ArrayList(cloneMe.placedSurfaces.Count);

            foreach (PlacedSurface ps in cloneMe.placedSurfaces)
            {
                this.placedSurfaces.Add(ps.Clone());
            }

            this.region = (PdnRegion)cloneMe.Region.Clone();
        }

        ~IrregularSurface()
        {
            Dispose(false);
        }

        /// <summary>
        /// Draws the IrregularSurface on to the given Surface.
        /// </summary>
        /// <param name="dst">The Surface to draw to.</param>
        public void Draw(Surface dst)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("IrregularSurface");
            }

            foreach (PlacedSurface ps in placedSurfaces)
            {
                ps.Draw(dst);
            }
        }

        public void Draw(Surface dst, IPixelOp pixelOp)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("IrregularSurface");
            }

            foreach (PlacedSurface ps in this.placedSurfaces)
            {
                ps.Draw(dst, pixelOp);
            }
        }

        /// <summary>
        /// Draws the IrregularSurface on to the given Surface starting at the given (x,y) offset.
        /// </summary>
        /// <param name="g">The Surface to draw to.</param>
        /// <param name="transformX">The value to be added to every X coordinate that is used for drawing.</param>
        /// <param name="transformY">The value to be added to every Y coordinate that is used for drawing.</param>
        public void Draw(Surface dst, int tX, int tY)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("IrregularSurface");
            }

            foreach (PlacedSurface ps in this.placedSurfaces)
            {
                ps.Draw(dst, tX, tY);
            }
        }

        public void Draw(Surface dst, int tX, int tY, IPixelOp pixelOp)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("IrregularSurface");
            }

            foreach (PlacedSurface ps in this.placedSurfaces)
            {
                ps.Draw(dst, tX, tY, pixelOp);
            }
        }

        #region IDisposable Members
        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                // TODO: FXCOP: call Dispose() on this.region

                this.disposed = true;

                if (disposing)
                {
                    foreach (PlacedSurface ps in this.placedSurfaces)
                    {
                        ps.Dispose();
                    }

                    this.placedSurfaces.Clear();
                    this.placedSurfaces = null;
                }
            }
        }
        #endregion

        #region ICloneable Members

        /// <summary>
        /// Clones the IrregularSurface.
        /// </summary>
        /// <returns>A copy of the current state of this PlacedSurface.</returns>
        public object Clone()
        {
            if (disposed)
            {
                throw new ObjectDisposedException("IrregularSurface");
            }

            return new IrregularSurface(this);
        }
        #endregion

        #region IDeserializationCallback Members

        public void OnDeserialization(object sender)
        {
            region = PdnRegion.CreateEmpty();

            Rectangle[] rects = new Rectangle[placedSurfaces.Count];

            for (int i = 0; i < placedSurfaces.Count; ++i)
            {
                rects[i] = ((PlacedSurface)placedSurfaces[i]).Bounds;
            }

            region = Utility.RectanglesToRegion(rects);
        }

        #endregion
    }
}
