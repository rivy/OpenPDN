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

namespace PaintDotNet
{
    /// <summary>
    /// This class handles rendering something to a SurfaceBox.
    /// </summary>
    public abstract class SurfaceBoxRenderer
        : IDisposable
    {
        private bool disposed = false;
        private SurfaceBoxRendererList ownerList;
        private bool visible;

        public const int MinXCoordinate = -131072;
        public const int MaxXCoordinate = +131072;
        public const int MinYCoordinate = -131072;
        public const int MaxYCoordinate = +131072;

        public bool IsDisposed
        {
            get
            {
                return this.disposed;
            }
        }

        public static Rectangle MaxBounds
        {
            get
            {
                return Rectangle.FromLTRB(MinXCoordinate, MinYCoordinate, MaxXCoordinate + 1, MaxYCoordinate + 1);
            }
        }

        protected object SyncRoot
        {
            get
            {
                return OwnerList.SyncRoot;
            }
        }

        protected SurfaceBoxRendererList OwnerList
        {
            get
            {
                return this.ownerList;
            }
        }

        public virtual void OnSourceSizeChanged()
        {
        }

        public virtual void OnDestinationSizeChanged()
        {
        }

        public Size SourceSize
        {
            get
            {
                return this.OwnerList.SourceSize;
            }
        }

        public Size DestinationSize
        {
            get
            {
                return this.OwnerList.DestinationSize;
            }
        }

        protected virtual void OnVisibleChanging()
        {
        }

        protected abstract void OnVisibleChanged();

        public bool Visible
        {
            get
            {
                return this.visible;
            }

            set
            {
                if (this.visible != value)
                {
                    OnVisibleChanging();
                    this.visible = value;
                    OnVisibleChanged();
                }
            }
        }
        
        protected delegate void RenderDelegate(Surface dst, Point offset);

        /// <summary>
        /// Renders, at the appropriate scale, the layer's imagery.
        /// </summary>
        /// <param name="dst">The Surface to render to.</param>
        /// <param name="dstTranslation">The (x,y) location of the upper-left corner of dst within DestinationSize.</param>
        public abstract void Render(Surface dst, Point offset);

        protected virtual void OnInvalidate(Rectangle rect)
        {
            this.OwnerList.Invalidate(rect);
        }

        public void Invalidate(Rectangle rect)
        {
            OnInvalidate(rect);
        }

        public void Invalidate(RectangleF rectF)
        {
            Rectangle rect = Utility.RoundRectangle(rectF);
            Invalidate(rect);
        }

        public void Invalidate(PdnRegion region)
        {
            foreach (Rectangle rect in region.GetRegionScansReadOnlyInt())
            {
                Invalidate(rect);
            }
        }

        public void Invalidate()
        {
            Invalidate(Rectangle.FromLTRB(MinXCoordinate, MinYCoordinate, MaxXCoordinate + 1, MaxYCoordinate + 1));
        }

        public SurfaceBoxRenderer(SurfaceBoxRendererList ownerList)
        {
            this.ownerList = ownerList;
            this.visible = true;
        }

        ~SurfaceBoxRenderer()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            this.disposed = true;
        }
    }
}
