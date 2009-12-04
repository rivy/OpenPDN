/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet;
using PaintDotNet.HistoryMementos;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Collections;

namespace PaintDotNet.Tools
{
    /// <summary>
    /// Ctrl left-click to select an origin, left click to place it
    /// </summary>
    internal class CloneStampTool
        : Tool
    {
        private class StaticData
        {
            public Point takeFrom;
            public Point lastMoved;
            public bool updateSrcPreview;
            public WeakReference wr;
        }

        private new StaticData GetStaticData()
        {
            object staticData = base.GetStaticData();

            if (staticData == null)
            {
                staticData = new StaticData();
                base.SetStaticData(staticData);
            }

            return (StaticData)staticData;
        }

        private BitmapLayer takeFromLayer;

        private bool switchedTo = false;
        private Rectangle undoRegion = Rectangle.Empty;
        private PdnRegion savedRegion;
        private RenderArgs ra;
        private bool mouseUp = true;
        private Vector<Rectangle> historyRects;
        private bool antialiasing;
        private PdnRegion clipRegion;

        private BrushPreviewRenderer rendererDst;
        private BrushPreviewRenderer rendererSrc;

        // private bool added by MK for "clone source" cursor transition
        private bool mouseDownSettingCloneSource;

        private Cursor cursorMouseDown, cursorMouseUp, cursorMouseDownSetSource;

        private bool IsShiftDown()
        {
            return ModifierKeys == Keys.Shift;
        }

        private bool IsCtrlDown()
        {
            return ModifierKeys == Keys.Control;
        }

        /// <summary>
        /// Button down mouse left.  Returns true if only the left mouse button is depressed.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private bool IsMouseLeftDown(MouseEventArgs e)
        {
            return e.Button == MouseButtons.Left;
        }

        /// <summary>
        /// Button down mouse right.  Returns true if only the right mouse is depressed.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private bool IsMouseRightDown(MouseEventArgs e)
        {
            return e.Button == MouseButtons.Right;
        }

        protected override void OnMouseEnter()
        {
            this.rendererDst.Visible = true;
            base.OnMouseEnter();
        }

        protected override void OnMouseLeave()
        {
            this.rendererDst.Visible = false;
            base.OnMouseLeave();
        }

        public CloneStampTool(DocumentWorkspace documentWorkspace) 
            : base(documentWorkspace,
                   PdnResources.GetImageResource("Icons.CloneStampToolIcon.png"),
                   PdnResources.GetString("CloneStampTool.Name"),
                   PdnResources.GetString("CloneStampTool.HelpText"),
                   'l',
                   false,
                   ToolBarConfigItems.Pen | ToolBarConfigItems.Antialiasing)
        {
        }

        protected override void OnPulse()
        {
            double time = (double)new SystemLayer.Timing().GetTickCount();
            double sin = Math.Sin(time / 300.0);
            int alpha = (int)Math.Ceiling((((sin + 1.0) / 2.0) * 224.0) + 31.0);
            this.rendererSrc.BrushAlpha = alpha;
            base.OnPulse();
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            
            cursorMouseDown = new Cursor(PdnResources.GetResourceStream("Cursors.GenericToolCursorMouseDown.cur"));
            cursorMouseDownSetSource = new Cursor(PdnResources.GetResourceStream("Cursors.CloneStampToolCursorSetSource.cur"));
            cursorMouseUp = new Cursor(PdnResources.GetResourceStream("Cursors.CloneStampToolCursor.cur"));
            this.Cursor = cursorMouseUp;

            this.rendererDst = new BrushPreviewRenderer(this.RendererList);
            this.RendererList.Add(this.rendererDst, false);

            this.rendererSrc = new BrushPreviewRenderer(this.RendererList);
            this.rendererSrc.BrushLocation = GetStaticData().takeFrom;
            this.rendererSrc.BrushSize = AppEnvironment.PenInfo.Width / 2.0f;
            this.rendererSrc.Visible = (GetStaticData().takeFrom != Point.Empty);
            this.RendererList.Add(this.rendererSrc, false);

            if (ActiveLayer != null)
            {
                switchedTo = true;
                historyRects = new Vector<Rectangle>();

                if (GetStaticData().wr != null && GetStaticData().wr.IsAlive)
                {
                    takeFromLayer = (BitmapLayer)GetStaticData().wr.Target;
                }
                else
                {
                    takeFromLayer = null;
                }
            }

            AppEnvironment.PenInfoChanged += new EventHandler(Environment_PenInfoChanged);
        }

        protected override void OnDeactivate()
        {
            if (!this.mouseUp)
            {
                StaticData sd = GetStaticData();
                Point lastXY = Point.Empty;

                if (sd != null)
                {
                    lastXY = sd.lastMoved;
                }

                OnMouseUp(new MouseEventArgs(MouseButtons.Left, 0, lastXY.X, lastXY.Y, 0));
            } 
            
            AppEnvironment.PenInfoChanged -= new EventHandler(Environment_PenInfoChanged);

            this.RendererList.Remove(this.rendererDst);
            this.rendererDst.Dispose();
            this.rendererDst = null;

            this.RendererList.Remove(this.rendererSrc);
            this.rendererSrc.Dispose();
            this.rendererSrc = null;

            if (cursorMouseDown != null)
            {
                cursorMouseDown.Dispose();
                cursorMouseDown = null;
            }

            if (cursorMouseUp != null)
            {
                cursorMouseUp.Dispose();
                cursorMouseUp = null;
            }

            if (cursorMouseDownSetSource != null)
            {
                cursorMouseDownSetSource.Dispose();
                cursorMouseDownSetSource = null;
            }
            
            base.OnDeactivate();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (IsCtrlDown() && mouseUp)
            {
                Cursor = cursorMouseDownSetSource;
                mouseDownSettingCloneSource = true;
            }

            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            // this isn't likely the best way to check to see if
            // the CTRL key has been let up.  If it's not, version
            // 2.1 can address the discrepancy.
            if (!IsCtrlDown() && mouseDownSettingCloneSource)
            {
                Cursor = cursorMouseUp;
                mouseDownSettingCloneSource = false;
            }

            base.OnKeyUp(e);
        }
                
        protected override void OnMouseUp(MouseEventArgs e)
        {
            mouseUp = true;

            if (!mouseDownSettingCloneSource)
            {
                Cursor = cursorMouseUp; 
            }

            if (IsMouseLeftDown(e))
            {
                this.rendererDst.Visible = true;

                if (savedRegion != null)
                {
                    //RestoreRegion(this.savedRegion);
                    ActiveLayer.Invalidate(this.savedRegion.GetBoundsInt());
                    savedRegion.Dispose();
                    savedRegion = null;
                    Update();
                }

                if (GetStaticData().takeFrom == Point.Empty || GetStaticData().lastMoved == Point.Empty)
                {
                    return;
                }

                if (historyRects.Count > 0)
                {
                    PdnRegion saveMeRegion;

                    Rectangle[] rectsRO;
                    int rectsROLength;
                    this.historyRects.GetArrayReadOnly(out rectsRO, out rectsROLength);
                    saveMeRegion = Utility.RectanglesToRegion(rectsRO, 0, rectsROLength);

                    PdnRegion simplifiedRegion = Utility.SimplifyAndInflateRegion(saveMeRegion);
                    SaveRegion(simplifiedRegion, simplifiedRegion.GetBoundsInt());

                    historyRects = new Vector<Rectangle>();

                    HistoryMemento ha = new BitmapHistoryMemento(Name, Image, DocumentWorkspace, ActiveLayerIndex, 
                        simplifiedRegion, this.ScratchSurface);

                    HistoryStack.PushNewMemento(ha);
                    this.ClearSavedMemory();
                }
            }
        }

        private unsafe void DrawACircle(PointF pt, Surface srfSrc, Surface srfDst, Point difference, Rectangle rect) 
        {
            float bw = AppEnvironment.PenInfo.Width / 2;
            float envAlpha = AppEnvironment.PrimaryColor.A / 255.0f;

            rect.Intersect(new Rectangle(difference, srfSrc.Size));
            rect.Intersect(srfDst.Bounds);

            if (rect.Width == 0 || rect.Height == 0)
            {
                return;
            }

            // envAlpha = envAlpha^4
            envAlpha *= envAlpha;
            envAlpha *= envAlpha;

            for (int y = rect.Top; y < rect.Bottom; y++) 
            {
                ColorBgra *srcRow = srfSrc.GetRowAddressUnchecked(y - difference.Y);
                ColorBgra *dstRow = srfDst.GetRowAddressUnchecked(y);

                for (int x = rect.Left; x < rect.Right; x++) 
                {
                    ColorBgra *srcPtr = unchecked(srcRow + x - difference.X);
                    ColorBgra *dstPtr = unchecked(dstRow + x);
                    float distFromRing = 0.5f + bw - Utility.Distance(pt, new PointF(x, y));

                    if (distFromRing > 0)
                    {
                        float alpha = antialiasing ? Utility.Clamp(distFromRing * envAlpha, 0, 1) : 1;
                        alpha *= srcPtr->A / 255.0f;
                        dstPtr->A = (byte)(255 - (255 - dstPtr->A) * (1 - alpha));

                        if (0 == (alpha + (1 - alpha) * dstPtr->A / 255))
                        {
                            dstPtr->Bgra = 0;
                        }
                        else
                        {
                            dstPtr->R = (byte)((srcPtr->R * alpha + dstPtr->R * (1 - alpha) * dstPtr->A / 255) / (alpha + (1 - alpha) * dstPtr->A / 255));
                            dstPtr->G = (byte)((srcPtr->G * alpha + dstPtr->G * (1 - alpha) * dstPtr->A / 255) / (alpha + (1 - alpha) * dstPtr->A / 255));
                            dstPtr->B = (byte)((srcPtr->B * alpha + dstPtr->B * (1 - alpha) * dstPtr->A / 255) / (alpha + (1 - alpha) * dstPtr->A / 255));
                        }
                    }
                }
            }

            rect.Inflate(1, 1);
            Document.Invalidate(rect);
        }

        private void DrawCloneLine(Point currentMouse, Point lastMoved, Point lastTakeFrom, Surface surfaceSource, Surface surfaceDest)
        {
            Rectangle[] rectSelRegions;
            Rectangle rectBrushArea;
            int penWidth = (int)AppEnvironment.PenInfo.Width;
            int ceilingPenWidth = (int)Math.Ceiling((double)penWidth);

            if (mouseUp || switchedTo)
            {
                lastMoved = currentMouse;
                lastTakeFrom = GetStaticData().takeFrom;
                mouseUp = false;
                switchedTo = false;
            }

            Point difference = new Point(currentMouse.X - GetStaticData().takeFrom.X, currentMouse.Y - GetStaticData().takeFrom.Y);
            Point direction = new Point(currentMouse.X - lastMoved.X, currentMouse.Y - lastMoved.Y);
            float length = Utility.Magnitude(direction);
            float bw = 1 + AppEnvironment.PenInfo.Width / 2;
                        
            rectSelRegions = this.clipRegion.GetRegionScansReadOnlyInt();
        
            Rectangle rect = Utility.PointsToRectangle(lastMoved, currentMouse);
            rect.Inflate(penWidth / 2 + 1, penWidth / 2 + 1);
            rect.Intersect(new Rectangle(difference, surfaceSource.Size));
            rect.Intersect(surfaceDest.Bounds);
            
            if (rect.Width == 0 || rect.Height == 0)
            {
                return;
            }

            SaveRegion(null, rect);
            historyRects.Add(rect);

            // Follow the line to draw the clone... line
            float fInc;

            try
            {
                fInc = (float)Math.Sqrt(bw) / length;
            }

            catch (DivideByZeroException)
            {
                // See bug #1796
                return;
            } 
            
            for (float f = 0; f < 1; f += fInc) 
            {
                // Do intersects with each of the rectangles in a selection
                foreach (Rectangle rectSel in rectSelRegions)
                {
                    PointF p = new PointF(currentMouse.X * (1 - f) + f * lastMoved.X,
                        currentMouse.Y * (1 - f) + f * lastMoved.Y);

                    rectBrushArea = new Rectangle((int)(p.X - bw), (int)(p.Y - bw), (int)(bw * 2 + 1), (int)(bw * 2 + 1));
                    
                    Rectangle rectBrushArea2 = new Rectangle(
                        rectBrushArea.X - difference.X,
                        rectBrushArea.Y - difference.Y,
                        rectBrushArea.Width,
                        rectBrushArea.Height);

                    if (rectBrushArea.IntersectsWith(rectSel))
                    {
                        rectBrushArea.Intersect(rectSel);
                        SaveRegion(null, rectBrushArea);
                        SaveRegion(null, rectBrushArea2);
                        DrawACircle(p, surfaceSource, surfaceDest, difference, rectBrushArea);
                    }
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {           
            base.OnMouseMove(e);

            this.rendererDst.BrushLocation = new Point(e.X, e.Y);
            this.rendererDst.BrushSize = AppEnvironment.PenInfo.Width / 2.0f;

            if (!(ActiveLayer is BitmapLayer) || (takeFromLayer == null))
            {
                return;
            }

            if (GetStaticData().updateSrcPreview)
            {
                Point currentMouse = new Point(e.X, e.Y);
                Point difference = new Point(currentMouse.X - GetStaticData().lastMoved.X, currentMouse.Y - GetStaticData().lastMoved.Y);
                this.rendererSrc.BrushLocation = new Point(GetStaticData().takeFrom.X + difference.X, GetStaticData().takeFrom.Y + difference.Y);;
                this.rendererSrc.BrushSize = AppEnvironment.PenInfo.Width / 2.0f;
            }
            
            if (IsMouseLeftDown(e) && 
                (GetStaticData().takeFrom != Point.Empty) && 
                !IsCtrlDown())
            {
                Point currentMouse = new Point(e.X, e.Y);
                Point lastTakeFrom = Point.Empty;

                lastTakeFrom = GetStaticData().takeFrom;
                if (GetStaticData().lastMoved != Point.Empty)
                {
                    Point difference = new Point(currentMouse.X - GetStaticData().lastMoved.X, currentMouse.Y - GetStaticData().lastMoved.Y);
                    GetStaticData().takeFrom = new Point(GetStaticData().takeFrom.X + difference.X, GetStaticData().takeFrom.Y + difference.Y);
                }
                else
                {
                    GetStaticData().lastMoved = currentMouse;
                }

                int penWidth = (int)AppEnvironment.PenInfo.Width;
                Rectangle rect;

                if (penWidth != 1)
                {
                    rect = new Rectangle(new Point(GetStaticData().takeFrom.X - penWidth / 2, GetStaticData().takeFrom.Y - penWidth / 2), new Size(penWidth + 1, penWidth + 1));
                }
                else
                {
                    rect = new Rectangle(new Point(GetStaticData().takeFrom.X - penWidth, GetStaticData().takeFrom.Y - penWidth), new Size(1 + (2 * penWidth), 1 + (2 * penWidth)));
                }

                Rectangle boundRect = new Rectangle(GetStaticData().takeFrom, new Size(1, 1));

                // If the takeFrom area escapes the boundary
                if (!ActiveLayer.Bounds.Contains(boundRect))
                {
                    GetStaticData().lastMoved = currentMouse;
                    lastTakeFrom = GetStaticData().takeFrom;
                }

                if (this.savedRegion != null)
                {
                    ActiveLayer.Invalidate(savedRegion.GetBoundsInt());
                    this.savedRegion.Dispose();
                    this.savedRegion = null;
                }
                
                rect.Intersect(takeFromLayer.Surface.Bounds);

                if (rect.Width == 0 || rect.Height == 0)
                {
                    return;
                }

                this.savedRegion = new PdnRegion(rect);
                SaveRegion(this.savedRegion, rect);

                // Draw that clone line
                Surface takeFromSurface;
                if (object.ReferenceEquals(takeFromLayer, ActiveLayer))
                {
                    takeFromSurface = this.ScratchSurface;
                }
                else
                {
                    takeFromSurface = takeFromLayer.Surface;
                }

                if (this.clipRegion == null)
                {
                    this.clipRegion = Selection.CreateRegion();
                }

                DrawCloneLine(currentMouse, GetStaticData().lastMoved, lastTakeFrom, 
                    takeFromSurface, ((BitmapLayer)ActiveLayer).Surface);

                this.rendererSrc.BrushLocation = GetStaticData().takeFrom;

                ActiveLayer.Invalidate(rect);
                Update();
                
                GetStaticData().lastMoved = currentMouse;
            }
        }

        protected override void OnSelectionChanged()
        {
            if (this.clipRegion != null)
            {
                this.clipRegion.Dispose();
                this.clipRegion = null;
            }

            base.OnSelectionChanged();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (!(ActiveLayer is BitmapLayer))
            {
                return;
            }

            Cursor = cursorMouseDown;

            if (IsMouseLeftDown(e))
            {
                this.rendererDst.Visible = false;

                if (IsCtrlDown())
                {
                    GetStaticData().takeFrom = new Point(e.X, e.Y);

                    this.rendererSrc.BrushLocation = new Point(e.X, e.Y);
                    this.rendererSrc.BrushSize = AppEnvironment.PenInfo.Width / 2.0f;
                    this.rendererSrc.Visible = true;
                    GetStaticData().updateSrcPreview = false;

                    GetStaticData().wr = new WeakReference(((BitmapLayer)ActiveLayer));
                    takeFromLayer = (BitmapLayer)(GetStaticData().wr.Target);
                    GetStaticData().lastMoved = Point.Empty;
                    ra = new RenderArgs(((BitmapLayer)ActiveLayer).Surface);
                }
                else
                {
                    GetStaticData().updateSrcPreview = true;

                    // Determine if there is something to work if, if there isn't return
                    if (GetStaticData().takeFrom == Point.Empty)
                    {
                    }
                    else if (!GetStaticData().wr.IsAlive || takeFromLayer == null)
                    {
                        GetStaticData().takeFrom = Point.Empty;
                        GetStaticData().lastMoved = Point.Empty;
                    }
                    // Make sure the layer is still there!
                    else if (takeFromLayer != null && !Document.Layers.Contains(takeFromLayer))
                    {   
                        GetStaticData().takeFrom = Point.Empty;
                        GetStaticData().lastMoved = Point.Empty;
                    }
                    else
                    {
                        this.antialiasing = AppEnvironment.AntiAliasing;
                        this.ra = new RenderArgs(((BitmapLayer)ActiveLayer).Surface);
                        this.ra.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        OnMouseMove(e);
                    }
                }
            }
        }

        private void Environment_PenInfoChanged(object sender, EventArgs e)
        {
            this.rendererSrc.BrushSize = AppEnvironment.PenInfo.Width / 2.0f;
            this.rendererDst.BrushSize = AppEnvironment.PenInfo.Width / 2.0f;
        }
    }
}
