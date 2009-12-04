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
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Threading;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// Renders a Surface to the screen.
    /// </summary>
    public sealed class SurfaceBox 
        : Control
    {
        public const int MaxSideLength = 32767;

        private int justPaintWhite = 0; // when this is non-zero, we just paint white (startup optimization)
        private ScaleFactor scaleFactor;
        private PaintDotNet.Threading.ThreadPool threadPool = new PaintDotNet.Threading.ThreadPool();
        private SurfaceBoxRendererList rendererList;
        private SurfaceBoxBaseRenderer baseRenderer;
        private Surface surface;

        // Each concrete instance of SurfaceBox holds a strong reference to a single
        // double buffer Surface. Statically, we store a weak reference to it. This way
        // when there are no more SurfaceBox instances, the double buffer Surface can
        // be cleaned up by the GC.
        private static WeakReference<Surface> doubleBufferSurfaceWeakRef = null;
        private Surface doubleBufferSurface = null;

        public SurfaceBoxRendererList RendererList
        {
            get
            {
                return this.rendererList;
            }
        }

        public Surface Surface
        {
            get
            {
                return this.surface;
            }

            set
            {
                this.surface = value;
                this.baseRenderer.Source = value;

                if (this.surface != null)
                {
                    // Maintain the scalefactor
                    this.Size = this.scaleFactor.ScaleSize(surface.Size);
                    this.rendererList.SourceSize = this.surface.Size;
                    this.rendererList.DestinationSize = this.Size;
                }

                Invalidate();
            }
        }

        [Obsolete("This functionality was moved to the DocumentView class", true)]
        public bool DrawGrid 
        {
            get 
            {
                return false;
            }

            set 
            {
            }
        }

        public void FitToSize(Size fit)
        {
            ScaleFactor newSF = ScaleFactor.Min(fit.Width, surface.Width,
                                                fit.Height, surface.Height,
                                                ScaleFactor.MinValue);

            this.scaleFactor = newSF;
            this.Size = this.scaleFactor.ScaleSize(surface.Size);
        }

        public void RenderTo(Surface dst)
        {
            dst.Clear(ColorBgra.Transparent);

            if (this.surface != null)
            {
                SurfaceBoxRendererList sbrl = new SurfaceBoxRendererList(this.surface.Size, dst.Size);
                SurfaceBoxBaseRenderer sbbr = new SurfaceBoxBaseRenderer(sbrl, this.surface);
                sbrl.Add(sbbr, true);
                sbrl.Render(dst, new Point(0, 0));
                sbrl.Remove(sbbr);
            }
        }

        /// <summary>
        /// Increments the "just paint white" counter. When this counter is non-zero,
        /// the OnPaint() method will only paint white. This is used as an optimization
        /// during Paint.NET's startup so that it doesn't have to touch all the pages
        /// of the blank document's layer.
        /// </summary>
        public void IncrementJustPaintWhite()
        {
            ++this.justPaintWhite;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            // This code fixes the size of the surfaceBox as necessary to 
            // maintain the aspect ratio of the surface. Keeping the mouse
            // within 32767 is delegated to the new overflow-checking code
            // in Tool.cs.

            Size mySize = this.Size;
            if (this.Width == MaxSideLength && surface != null)
            { 
                // Windows forms probably clamped this control's width, so we have to fix the height.
                mySize.Height = (int)(((long)(MaxSideLength + 1) * (long)surface.Height) / (long)surface.Width);
            }
            else if (mySize.Width == 0)
            {
                mySize.Width = 1;
            }

            if (this.Width == MaxSideLength && surface != null)
            { 
                // Windows forms probably clamped this control's height, so we have to fix the width.
                mySize.Width = (int)(((long)(MaxSideLength + 1) * (long)surface.Width) / (long)surface.Height);
            }
            else if (mySize.Height == 0) 
            {
                mySize.Height = 1;
            }

            if (mySize != this.Size) 
            {
                this.Size = mySize;
            }
           
            if (surface == null)
            {
                this.scaleFactor = ScaleFactor.OneToOne;
            }
            else
            {
                ScaleFactor newSF = ScaleFactor.Max(this.Width, surface.Width,
                                                    this.Height, surface.Height,
                                                    ScaleFactor.OneToOne);

                this.scaleFactor = newSF;
            }

            this.rendererList.DestinationSize = this.Size;
        }

        public ScaleFactor ScaleFactor
        {
            get
            {
                return this.scaleFactor;
            }
        }

        public SurfaceBox()
        {
            InitializeComponent();
            this.scaleFactor = ScaleFactor.OneToOne;

            this.rendererList = new SurfaceBoxRendererList(this.Size, this.Size);
            this.rendererList.Invalidated += new InvalidateEventHandler(Renderers_Invalidated);
            this.baseRenderer = new SurfaceBoxBaseRenderer(this.rendererList, null);
            this.rendererList.Add(this.baseRenderer, false);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.baseRenderer != null)
                {
                    this.rendererList.Remove(this.baseRenderer);
                    this.baseRenderer.Dispose();
                    this.baseRenderer = null;
                }

                if (this.doubleBufferSurface != null)
                {
                    this.doubleBufferSurface.Dispose();
                    this.doubleBufferSurface = null;
                }
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// This event is raised after painting has been performed. This is required because
        /// the normal Paint event is raised *before* painting has been performed.
        /// </summary>
        public event PaintEventHandler2 Painted;
        private void OnPainted(PaintEventArgs2 e)
        {
            if (Painted != null)
            {
                Painted(this, e);
            }
        }

        public event PaintEventHandler2 PrePaint;
        private void OnPrePaint(PaintEventArgs2 e)
        {
            if (PrePaint != null)
            {
                PrePaint(this, e);
            }
        }

        private Surface GetDoubleBuffer(Size size)
        {
            Surface localDBSurface = null;
            Size oldSize = new Size(0, 0);

            // If we already have a double buffer surface reference, but if that surface
            // is already disposed then don't worry about it.
            if (this.doubleBufferSurface != null && this.doubleBufferSurface.IsDisposed)
            {
                oldSize = this.doubleBufferSurface.Size;
                this.doubleBufferSurface = null;
            }

            // If we already have a double buffer surface reference, but if that surface
            // is too small, then nuke it.
            if (this.doubleBufferSurface != null && 
                (this.doubleBufferSurface.Width < size.Width || this.doubleBufferSurface.Height < size.Height))
            {
                oldSize = this.doubleBufferSurface.Size;
                this.doubleBufferSurface.Dispose();
                this.doubleBufferSurface = null;
                doubleBufferSurfaceWeakRef = null;
            }

            // If we don't have a double buffer, then we'd better get one.
            if (this.doubleBufferSurface != null)
            {
                // Got one!
                localDBSurface = this.doubleBufferSurface;
            }
            else if (doubleBufferSurfaceWeakRef != null)
            {
                // First, try to get the one that's already shared amongst all SurfaceBox instances.
                localDBSurface = doubleBufferSurfaceWeakRef.Target;

                // If it's disposed, then forget about it.
                if (localDBSurface != null && localDBSurface.IsDisposed)
                {
                    oldSize = localDBSurface.Size;
                    localDBSurface = null;
                    doubleBufferSurfaceWeakRef = null;
                }
            }

            // Make sure the surface is big enough.
            if (localDBSurface != null && (localDBSurface.Width < size.Width || localDBSurface.Height < size.Height))
            {
                oldSize = localDBSurface.Size;
                localDBSurface.Dispose();
                localDBSurface = null;
                doubleBufferSurfaceWeakRef = null;
            }

            // So, do we have a surface? If not then we'd better make one.
            if (localDBSurface == null)
            {
                Size newSize = new Size(Math.Max(size.Width, oldSize.Width), Math.Max(size.Height, oldSize.Height));
                localDBSurface = new Surface(newSize.Width, newSize.Height);
                doubleBufferSurfaceWeakRef = new WeakReference<Surface>(localDBSurface);
            }

            this.doubleBufferSurface = localDBSurface;
            Surface window = localDBSurface.CreateWindow(0, 0, size.Width, size.Height);
            return window;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (this.surface != null)
            {
                PdnRegion clipRegion = null;
                Rectangle[] rects = this.realUpdateRects;
                
                if (rects == null)
                {
                    clipRegion = new PdnRegion(e.Graphics.Clip, true);
                    clipRegion.Intersect(e.ClipRectangle);
                    rects = clipRegion.GetRegionScansReadOnlyInt();
                }

                if (this.justPaintWhite > 0)
                {
                    PdnGraphics.FillRectangles(e.Graphics, Color.White, rects);
                }
                else
                {
                    foreach (Rectangle rect in rects)
                    {
                        if (e.Graphics.IsVisible(rect))
                        {
                            PaintEventArgs2 e2 = new PaintEventArgs2(e.Graphics, rect);
                            OnPaintImpl(e2);
                        }
                    }
                }

                if (clipRegion != null)
                {
                    clipRegion.Dispose();
                    clipRegion = null;
                }
            }

            if (this.justPaintWhite > 0)
            {
                --this.justPaintWhite;
            }

            base.OnPaint(e);
        }

        private void OnPaintImpl(PaintEventArgs2 e)
        {
            using (Surface doubleBuffer = GetDoubleBuffer(e.ClipRectangle.Size))
            {
                using (RenderArgs renderArgs = new RenderArgs(doubleBuffer))
                {
                    OnPrePaint(e);
                    DrawArea(renderArgs, e.ClipRectangle.Location);
                    OnPainted(e);

                    IntPtr tracking;
                    Point childOffset;
                    Size parentSize;
                    doubleBuffer.GetDrawBitmapInfo(out tracking, out childOffset, out parentSize);

                    PdnGraphics.DrawBitmap(e.Graphics, e.ClipRectangle, e.Graphics.Transform,
                        tracking, childOffset.X, childOffset.Y);
                }
            }
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            // do nothing so as to avoid flicker
            // tip: for debugging, uncomment the next line!
            //base.OnPaintBackground(pevent);
        }

        private class RenderContext
        {
            public Surface[] windows;
            public Point[] offsets;
            public Rectangle[] rects;
            public SurfaceBox owner;
            public WaitCallback waitCallback;

            public void RenderThreadMethod(object indexObject)
            {
                int index = (int)indexObject;
                this.owner.rendererList.Render(windows[index], offsets[index]);
                this.windows[index].Dispose();
                this.windows[index] = null;
            }
        }

        private RenderContext renderContext;

        /// <summary>
        /// Draws an area of the SurfaceBox.
        /// </summary>
        /// <param name="ra">The rendering surface object to draw to.</param>
        /// <param name="offset">The virtual offset of ra, in client (destination) coordinates.</param>
        /// <remarks>
        /// If drawing to ra.Surface or ra.Bitmap, copy the roi of the source surface to (0,0) of ra.Surface or ra.Bitmap
        /// If drawing to ra.Graphics, copy the roi of the surface to (roi.X, roi.Y) of ra.Graphics
        /// </remarks>
        private unsafe void DrawArea(RenderArgs ra, Point offset)
        {
            if (surface == null)
            {
                return;
            }

            if (renderContext == null || (renderContext.windows != null && renderContext.windows.Length != Processor.LogicalCpuCount))
            {
                renderContext = new RenderContext();
                renderContext.owner = this;
                renderContext.waitCallback = new WaitCallback(renderContext.RenderThreadMethod);
                renderContext.windows = new Surface[Processor.LogicalCpuCount];
                renderContext.offsets = new Point[Processor.LogicalCpuCount];
                renderContext.rects = new Rectangle[Processor.LogicalCpuCount];
            }

            Utility.SplitRectangle(ra.Bounds, renderContext.rects);

            for (int i = 0; i < renderContext.rects.Length; ++i)
            {
                if (renderContext.rects[i].Width > 0 && renderContext.rects[i].Height > 0)
                {
                    renderContext.offsets[i] = new Point(renderContext.rects[i].X + offset.X, renderContext.rects[i].Y + offset.Y);
                    renderContext.windows[i] = ra.Surface.CreateWindow(renderContext.rects[i]);
                }
                else
                {
                    renderContext.windows[i] = null;
                }
            }
            
            for (int i = 0; i < renderContext.windows.Length; ++i)
            {
                if (renderContext.windows[i] != null)
                {
                    this.threadPool.QueueUserWorkItem(renderContext.waitCallback, BoxedConstants.GetInt32(i));
                }
            }

            this.threadPool.Drain();
        }

        private Rectangle[] realUpdateRects = null;
        protected override void WndProc(ref Message m)
        {
            IntPtr preR = m.Result;

            // Ignore focus
            if (m.Msg == 7 /* WM_SETFOCUS */)
            {
                return;
            }
            else if (m.Msg == 0x000f /* WM_PAINT */)
            {
                this.realUpdateRects = UI.GetUpdateRegion(this);

                if (this.realUpdateRects != null &&
                    this.realUpdateRects.Length >= 5) // '5' chosen arbitrarily
                {
                    this.realUpdateRects = null;
                }

                base.WndProc(ref m);
            }
            else
            {
                base.WndProc (ref m);
            }
        }

        /// <summary>
        /// Converts from control client coordinates to surface coordinates
        /// This is useful when this.Bounds != surface.Bounds (i.e. some sort of zooming is in effect)
        /// </summary>
        /// <param name="clientPt"></param>
        /// <returns></returns>
        public PointF ClientToSurface(PointF clientPt)
        {
            return ScaleFactor.UnscalePoint(clientPt);
        }

        public Point ClientToSurface(Point clientPt)
        {
            return ScaleFactor.UnscalePoint(clientPt);
        }

        public SizeF ClientToSurface(SizeF clientSize)
        {
            return ScaleFactor.UnscaleSize(clientSize);
        }

        public Size ClientToSurface(Size clientSize)
        {
            return Size.Round(ClientToSurface((SizeF)clientSize));
        }

        public RectangleF ClientToSurface(RectangleF clientRect)
        {
            return new RectangleF(ClientToSurface(clientRect.Location), ClientToSurface(clientRect.Size));
        }

        public Rectangle ClientToSurface(Rectangle clientRect)
        {
            return new Rectangle(ClientToSurface(clientRect.Location), ClientToSurface(clientRect.Size));
        }

        public PointF SurfaceToClient(PointF surfacePt)
        {
            return ScaleFactor.ScalePoint(surfacePt);
        }

        public Point SurfaceToClient(Point surfacePt)
        {
            return ScaleFactor.ScalePoint(surfacePt);
        }

        public SizeF SurfaceToClient(SizeF surfaceSize)
        {
            return ScaleFactor.ScaleSize(surfaceSize);
        }

        public Size SurfaceToClient(Size surfaceSize)
        {
            return Size.Round(SurfaceToClient((SizeF)surfaceSize));
        }

        public RectangleF SurfaceToClient(RectangleF surfaceRect)
        {
            return new RectangleF(SurfaceToClient(surfaceRect.Location), SurfaceToClient(surfaceRect.Size));
        }        

        public Rectangle SurfaceToClient(Rectangle surfaceRect)
        {
            return new Rectangle(SurfaceToClient(surfaceRect.Location), SurfaceToClient(surfaceRect.Size));
        }

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
        }

        private void Renderers_Invalidated(object sender, InvalidateEventArgs e)
        {
            Rectangle rect = SurfaceToClient(e.InvalidRect);
            rect.Inflate(1, 1);
            Invalidate(rect);
        }
    }
}

