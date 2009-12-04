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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PaintDotNet
{
    internal class SelectionRenderer
        : SurfaceBoxGraphicsRenderer
    {
        private const int dancingAntsInterval = 60;
        private const double maxCpuTime = 0.2; // max 20% CPU time
        private Color tintColor = Color.FromArgb(32, 32, 32, 255);
        private static Pen outlinePen1 = null;
        private static Pen outlinePen2 = null;
        private UserControl2 ownerControl;
        private System.Windows.Forms.Timer selectionTimer;
        private bool enableOutlineAnimation = true;
        private System.ComponentModel.IContainer components = null;
        private bool invertedTinting = false;
        private bool render = true; // when false, we do not Render()
        private PdnGraphicsPath selectedPath;
        private Selection selection;
        private PdnGraphicsPath zoomInSelectedPath;
        private int dancingAntsT = 0;
        private int whiteOpacity = 255;
        private int lastTickMod = 0;
        private Rectangle[] simplifiedRegionForTimer = null;
        private double coolOffTimeTickCount = 0.0;

        /// <summary>
        /// This variable is used to accumulate an invalidation region. It is initialized
        /// upon responding to the SelectedPathChanging event that is raised by the
        /// DocumentEnvironment. Then, when the SelectedPathChanged event is raised, the
        /// full region that needs to be redrawn is accounted for.
        /// </summary>
        private PdnRegion selectionRedrawInterior = PdnRegion.CreateEmpty();
        private PdnGraphicsPath selectionRedrawOutline = new PdnGraphicsPath();
        private DateTime lastFullInvalidate = DateTime.Now;

        protected override void OnVisibleChanged()
        {
            this.selectionTimer.Enabled = this.Visible;

            if (this.selection != null)
            {
                Rectangle rect = this.selection.GetBounds();
                Invalidate(rect);
            }
        }

        public override void OnDestinationSizeChanged()
        {
            lock (SyncRoot)
            {
                this.simplifiedRegionForTimer = null;
            }

            base.OnDestinationSizeChanged();
        }

        public override void OnSourceSizeChanged()
        {
            lock (SyncRoot)
            {
                this.simplifiedRegionForTimer = null;
            }

            base.OnSourceSizeChanged();
        }

        public bool EnableOutlineAnimation
        {
            get
            {
                return this.enableOutlineAnimation;
            }

            set
            {
                if (this.enableOutlineAnimation != value)
                {
                    this.enableOutlineAnimation = value;
                    Invalidate();
                }
            }
        }

        public bool InvertedTinting
        {
            get
            {
                return this.invertedTinting;
            }

            set
            {
                if (this.invertedTinting != value)
                {
                    this.invertedTinting = value;
                    Invalidate();
                }
            }
        }

        public Color TintColor
        {
            get
            {
                return this.tintColor;
            }

            set
            {
                if (value != this.tintColor)
                {
                    this.tintColor = value;

                    if (this.interiorBrush != null)
                    {
                        this.interiorBrush.Dispose();
                        this.interiorBrush = null;
                    }

                    Invalidate();
                }
            }
        }

        private void OnSelectionChanging(object sender, EventArgs e)
        {
            this.render = false;

            if (!this.selectionTimer.Enabled)
            {
                this.selectionTimer.Enabled = true;
            }
        }

        private void OnSelectionChanged(object sender, EventArgs e)
        {
            this.render = true;
            PdnGraphicsPath path = this.selection.CreatePath(); //this.selection.GetPathReadOnly();

            if (this.selectedPath == null)
            {
                Invalidate();
            }
            else
            {
                this.selectedPath.Dispose(); //
                this.selectedPath = null;
            }

            bool fullInvalidate = false;
            this.selectedPath = path;

            // HACK: Sometimes the selection leaves behind artifacts. So do a full invalidate
            //       every 1 second.
            if (this.selectedPath.PointCount > 10 && (DateTime.Now - lastFullInvalidate > new TimeSpan(0, 0, 0, 1, 0)))
            {
                fullInvalidate = true;
            }

            // if we're moving to a simpler selection region ...
            if (this.selectedPath == null)// || this.selectedPath.PointCount == 0)
            {   
                // then invalidate everything
                fullInvalidate = true;
            }
            else
            {   
                // otherwise, be intelligent about it and only redraw the 'new' area
                PdnRegion xorMe = new PdnRegion(this.selectedPath);
                selectionRedrawInterior.Xor(xorMe);
                xorMe.Dispose();
            }

            float ratio = 1.0f / (float)OwnerList.ScaleFactor.Ratio;
            int ratioInt = (int)Math.Ceiling(ratio);

            if (this.Visible && (this.EnableSelectionOutline || this.EnableSelectionTinting))
            {
                using (PdnRegion simplified = Utility.SimplifyAndInflateRegion(selectionRedrawInterior, Utility.DefaultSimplificationFactor, 2 * ratioInt))
                {
                    Invalidate(simplified);
                }
            }

            if (fullInvalidate)
            {
                Rectangle rect = Rectangle.Inflate(Rectangle.Truncate(selectionRedrawOutline.GetBounds2()), 1, 1);
                Invalidate(rect);
                lastFullInvalidate = DateTime.Now;
            }
            

            this.selectionRedrawInterior.Dispose();
            this.selectionRedrawInterior = null;

            if (this.zoomInSelectedPath != null)
            {
                this.zoomInSelectedPath.Dispose();
                this.zoomInSelectedPath = null;
            }

            this.simplifiedRegionForTimer = null;

            // prepare for next call
            if (this.selectedPath != null && !this.selectedPath.IsEmpty)
            {
                this.selectionRedrawOutline = (PdnGraphicsPath)this.selectedPath.Clone();
                this.selectionRedrawInterior = new PdnRegion(this.selectedPath);
            }
            else
            {
                if (invertedTinting)
                {
                    this.selectionRedrawInterior = new PdnRegion(new Rectangle(0, 0, this.SourceSize.Width, this.SourceSize.Height));
                }
                else
                {
                    this.selectionRedrawInterior = new PdnRegion();
                    this.selectionRedrawInterior.MakeEmpty();
                }

                Invalidate();
                this.selectionRedrawOutline = new PdnGraphicsPath();
            }
        }

        /// <summary>
        /// When we zoom in, we want to "stair-step" the selected path.
        /// </summary>
        /// <returns></returns>
        private PdnGraphicsPath GetZoomInPath()
        {
            lock (this.SyncRoot)
            {
                if (this.zoomInSelectedPath == null)
                {
                    if (this.selectedPath == null)
                    {
                        this.zoomInSelectedPath = new PdnGraphicsPath();
                    }
                    else
                    {
                        this.zoomInSelectedPath = this.selection.CreatePixelatedPath();
                    }
                }

                return this.zoomInSelectedPath;
            }
        }

        private PdnGraphicsPath GetAppropriateRenderPath()
        {
            if (OwnerList.ScaleFactor.Ratio >= 1.01)
            {
                return GetZoomInPath();
            }
            else
            {
                return this.selectedPath;
            }
        }

        private Timing timer = new Timing();
        private double renderTime = 0.0;

        public override bool ShouldRender()
        {
            return (this.render && (this.EnableSelectionOutline || this.EnableSelectionTinting));            
        }

        public override void RenderToGraphics(Graphics g, Point offset)
        {
            double start = timer.GetTickCountDouble();

            lock (SyncRoot)
            {
                PdnGraphicsPath path = GetAppropriateRenderPath();

                if (path == null || path.IsEmpty)
                {
                    this.render = false; // will be reset next time selection changes
                }
                else
                {
                    g.TranslateTransform(-offset.X, -offset.Y);
                    DrawSelection(g, path);
                }

                double end = timer.GetTickCountDouble();
                this.renderTime += (end - start);
            }
        }

        public SelectionRenderer(SurfaceBoxRendererList ownerList, Selection selection)
            : this(ownerList, selection, null)
        {
        }

        public SelectionRenderer(SurfaceBoxRendererList ownerList, Selection selection, UserControl2 ownerControl)
            : base(ownerList)
        {
            this.ownerControl = ownerControl;
            this.selection = selection;
            this.selection.Changing += new EventHandler(OnSelectionChanging);
            this.selection.Changed += new EventHandler(OnSelectionChanged);
            this.components = new System.ComponentModel.Container();
            this.selectionTimer = new System.Windows.Forms.Timer(this.components);
            this.selectionTimer.Enabled = true;
            this.selectionTimer.Interval = dancingAntsInterval / 2;
            this.selectionTimer.Tick += new System.EventHandler(this.SelectionTimer_Tick);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.components != null) 
                {
                    this.components.Dispose();
                    this.components = null;
                }

                if (this.selectionTimer != null)
                {
                    this.selectionTimer.Dispose();
                    this.selectionTimer = null;
                }

                if (this.zoomInSelectedPath != null)
                {
                    this.zoomInSelectedPath.Dispose();
                    this.zoomInSelectedPath = null;
                }
            }

            base.Dispose (disposing);
        }

        private Brush interiorBrush;
        private Brush InteriorBrush
        {
            get
            {
                if (interiorBrush == null)
                {
                    interiorBrush =  new SolidBrush(tintColor);
                }

                return interiorBrush;
            }
        }

        private bool enableSelectionOutline = true;
        public bool EnableSelectionOutline
        {
            get
            {
                return enableSelectionOutline;
            }

            set
            {
                if (this.enableSelectionOutline != value)
                {
                    enableSelectionOutline = value;
                    Invalidate();
                }
            }
        }

        private bool enableSelectionTinting = true;
        public bool EnableSelectionTinting
        {
            get
            {
                return enableSelectionTinting;
            }

            set
            {
                if (enableSelectionTinting != value)
                {
                    enableSelectionTinting = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// This is a silly function name.
        /// </summary>
        public void ResetOutlineWhiteOpacity()
        {
            if (this.whiteOpacity > 0)
            {
                Invalidate();
            }

            this.whiteOpacity = 0;
        }

        private void DrawSelectionOutline(Graphics g, PdnGraphicsPath outline)
        {
            if (outline == null)
            {
                return;
            }

            if (outlinePen1 == null)
            {
                outlinePen1 = new Pen(Color.FromArgb(160, Color.Black), 1.0f);
                outlinePen1.Alignment = PenAlignment.Outset;
                outlinePen1.LineJoin = LineJoin.Bevel;
                outlinePen1.Width = -1;
            }

            if (outlinePen2 == null)
            {
                outlinePen2 = new Pen(Color.White, 1.0f);
                outlinePen2.Alignment = PenAlignment.Outset;
                outlinePen2.LineJoin = LineJoin.Bevel;
                outlinePen2.MiterLimit = 2;
                outlinePen2.Width = -1;
                outlinePen2.DashStyle = DashStyle.Dash;
                outlinePen2.DashPattern = new float[] { 4, 4 };
                outlinePen2.Color = Color.White;
                outlinePen2.DashOffset = 4.0f;
            }

            PixelOffsetMode oldPOM = g.PixelOffsetMode;
            g.PixelOffsetMode = PixelOffsetMode.None;
            
            SmoothingMode oldSM = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            outline.Draw(g, outlinePen1);

            float offset = (float)((double)dancingAntsT / OwnerList.ScaleFactor.Ratio);
            outlinePen2.DashOffset += offset;

            if (whiteOpacity != 0)
            {
                outlinePen2.Color = Color.FromArgb(whiteOpacity, Color.White);
                outline.Draw(g, outlinePen2);
            }

            outlinePen2.DashOffset -= offset;

            g.SmoothingMode = oldSM;
            g.PixelOffsetMode = oldPOM;
        }

        private void DrawSelectionTinting(Graphics g, PdnGraphicsPath outline)
        {
            if (outline == null)
            {
                return;
            }

            CompositingMode oldCM = g.CompositingMode;
            g.CompositingMode = CompositingMode.SourceOver;

            SmoothingMode oldSM = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            PixelOffsetMode oldPOM = g.PixelOffsetMode;
            g.PixelOffsetMode = PixelOffsetMode.None;

            Region oldClipRegion = null;
            RectangleF outlineBounds = outline.GetBounds();

            if (outlineBounds.Left < 0 ||
                outlineBounds.Top < 0 ||
                outlineBounds.Right >= this.SourceSize.Width ||
                outlineBounds.Bottom >= this.SourceSize.Height)
            {
                oldClipRegion = g.Clip;

                Region newClipRegion = oldClipRegion.Clone();
                newClipRegion.Intersect(new Rectangle(0, 0, this.SourceSize.Width, this.SourceSize.Height));
                g.Clip = newClipRegion;
                newClipRegion.Dispose();
            }
             
            g.FillPath(InteriorBrush, outline);

            if (oldClipRegion != null)
            {
                g.Clip = oldClipRegion;
                oldClipRegion.Dispose();
            }

            g.PixelOffsetMode = oldPOM;
            g.SmoothingMode = oldSM;
            g.CompositingMode = oldCM;
        }

        private void DrawSelection(Graphics gdiG, PdnGraphicsPath outline)
        {
            if (outline == null)
            {
                return;
            }

            float ratio = (float)OwnerList.ScaleFactor.Ratio;
            gdiG.ScaleTransform(ratio, ratio);

            if (EnableSelectionTinting)
            {
                PdnGraphicsPath outline2;

                if (invertedTinting)
                {
                    outline2 = (PdnGraphicsPath)outline.Clone();
                    outline2.AddRectangle(new Rectangle(-1, -1, this.SourceSize.Width + 1, this.SourceSize.Height + 1));
                    outline2.CloseAllFigures();
                }
                else
                {
                    outline2 = outline;
                }

                DrawSelectionTinting(gdiG, outline2);

                if (invertedTinting)
                {
                    outline2.Dispose();
                }
            }

            if (EnableSelectionOutline)
            {
                DrawSelectionOutline(gdiG, outline);
            }

            gdiG.ScaleTransform(1 / ratio, 1 / ratio);
        }

        private void SelectionTimer_Tick(object sender, System.EventArgs e)
        {
            if (this.IsDisposed || this.ownerControl.IsDisposed)
            {
                return;
            }

            if (this.selectedPath == null || this.selectedPath.IsEmpty)
            {
                this.selectionTimer.Enabled = false;
                return;
            }

            if (!this.enableOutlineAnimation)
            {
                return;
            }

            if (this.timer.GetTickCountDouble() < this.coolOffTimeTickCount)
            {
                return;
            }

            if (this.ownerControl != null && this.ownerControl.IsMouseCaptured())
            {
                return;
            }

            Form form = this.ownerControl.FindForm();
            if (form != null && form.WindowState == FormWindowState.Minimized)
            {
                return;
            }

            int presentTickMod = (int)((Utility.GetTimeMs() / dancingAntsInterval) % 2);

            if (presentTickMod != lastTickMod)
            {
                lastTickMod = presentTickMod;
                dancingAntsT = unchecked(dancingAntsT + 1);

                if (this.simplifiedRegionForTimer == null)
                {
                    using (PdnGraphicsPath invalidPath = (PdnGraphicsPath)selectedPath.Clone())
                    {
                        invalidPath.CloseAllFigures();

                        float ratio = 1.0f / (float)OwnerList.ScaleFactor.Ratio;
                        int inflateAmount = (int)Math.Ceiling(ratio);

                        this.simplifiedRegionForTimer = Utility.SimplifyTrace(invalidPath, 50);
                        Utility.InflateRectanglesInPlace(this.simplifiedRegionForTimer, inflateAmount);
                    }
                }

                try
                {
                    foreach (Rectangle rect in this.simplifiedRegionForTimer)
                    {
                        Invalidate(rect);
                    }
                }

                catch (ObjectDisposedException)
                {
                    try
                    {
                        this.selectionTimer.Enabled = false;
                    }

                    catch (Exception)
                    {
                        // Ignore error
                    }
                }

                if (this.ownerControl == null || (this.ownerControl != null && !this.ownerControl.IsMouseCaptured()))
                {
                    whiteOpacity = Math.Min(whiteOpacity + 16, 255);
                }
            }

            // If it takes "too long" to render the dancing ants, then we institute
            // a cooling-off period during which we will not render the ants.
            // This will curb the CPU usage by a few percent, which will avoid us
            // monopolizing the CPU.
            double maxRenderTime = (double)dancingAntsInterval * maxCpuTime;

            if (renderTime > maxRenderTime)
            {
                double coolOffTime = renderTime / maxRenderTime;
                this.coolOffTimeTickCount = timer.GetTickCountDouble() + coolOffTime;
            }

            this.renderTime = 0.0;
        }
    }
}
