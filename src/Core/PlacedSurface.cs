/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace PaintDotNet
{
    /// <summary>
    /// Encapsulates a surface ("what") along with a pixel offset ("where") which 
    /// defines where the surface would be drawn on to another surface.
    /// Instances of this object are immutable -- once you create it, you can not
    /// change it.
    /// </summary>
    [Serializable]
    public sealed class PlacedSurface
        : ISurfaceDraw,
          IDisposable,
          ICloneable
    {
        Point where;
        Surface what;

        public Point Where
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("PlacedSurface");
                }

                return where;
            }
        }

        public Surface What
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("PlacedSurface");
                }

                return what;
            }
        }

        public Size Size
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("PlacedSurface");
                }

                return What.Size;
            }
        }

        public Rectangle Bounds
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("PlacedSurface");
                }

                return new Rectangle(Where, What.Size);
            }
        }

        public void Draw(Surface dst)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("PlacedSurface");
            }

            dst.CopySurface(what, where);
        }

        public void Draw(Surface dst, IPixelOp pixelOp)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("PlacedSurface");
            }

            Rectangle dstRect = Bounds;
            Rectangle dstClip = Rectangle.Intersect(dstRect, dst.Bounds);

            if (dstClip.Width > 0 && dstClip.Height > 0)
            {
                int dtX = dstClip.X - where.X;
                int dtY = dstClip.Y - where.Y;

                pixelOp.Apply(dst, dstClip.Location, what, new Point(dtX, dtY), dstClip.Size);
            }
        }

        public void Draw(Surface dst, int tX, int tY)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("PlacedSurface");
            }

            Point oldWhere = where;

            try
            {
                where.X += tX;
                where.Y += tY;
                Draw(dst);
            }

            finally
            {
                where = oldWhere;
            }
        }

        public void Draw(Surface dst, int tX, int tY, IPixelOp pixelOp)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("PlacedSurface");
            }

            Point oldWhere = where;

            try
            {
                where.X += tX;
                where.Y += tY;
                Draw(dst, pixelOp);
            }

            finally
            {
                where = oldWhere;
            }
        }

        public PlacedSurface (Surface source, Rectangle roi)
        {
            where = roi.Location;

            Surface window = source.CreateWindow(roi);
            what = new Surface(window.Size);
            what.CopySurface(window);
            window.Dispose();
        }

        private PlacedSurface (PlacedSurface ps)
        {
            where = ps.Where;
            what = ps.What.Clone();
        }

        private PlacedSurface()
        {
        }

        ~PlacedSurface()
        {
            Dispose(false);
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
            if (!disposed)
            {
                disposed = true;

                if (disposing)
                {
                    what.Dispose();
                    what = null;
                }
            }
        }
        #endregion

        #region ICloneable Members

        public object Clone()
        {
            if (disposed)
            {
                throw new ObjectDisposedException("PlacedSurface");
            }

            return new PlacedSurface(this);
        }

        #endregion
    }
}
