/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.HistoryMementos;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;

namespace PaintDotNet.Tools
{
    /// <summary>
    /// Allows the user to draw a shape that can be defined using two points on the canvas.
    /// The user clicks and drags between two points to define the area that bounds the shape.
    /// </summary>
    internal abstract class ShapeTool
        : Tool
    {
        private const char defaultShortcut = 'o';
        private bool moveOriginMode;
        private PointF lastXY;
        private bool mouseDown;
        private MouseButtons mouseButton;
        private BitmapLayer bitmapLayer;
        private RenderArgs renderArgs;
        private PdnRegion interiorSaveRegion;
        private PdnRegion outlineSaveRegion;
        private List<PointF> points;
        private PdnRegion lastDrawnRegion = null;
        private Cursor cursorMouseUp;
        private Cursor cursorMouseDown;
        private bool shapeWasCommited = true;
        private CompoundHistoryMemento chaAlreadyOnStack = null;
        private bool useDashStyle = false; // if set to false, then the DashStyle will always be forced to DashStyle.Flat
        private bool forceShapeType = false;
        private ShapeDrawType forcedShapeDrawType = ShapeDrawType.Both;

        protected override bool SupportsInk
        {
            get
            {
                return true;
            }
        }

        // This is for shapes that should only be draw in one ShapeDrawType
        // The line shape, for instance, should only ever be drawn in ShapeDrawType.Outline
        protected bool ForceShapeDrawType
        {
            get
            {
                return this.forceShapeType;
            }

            set
            {
                this.forceShapeType = value;
            }
        }

        protected ShapeDrawType ForcedShapeDrawType
        {
            get
            {
                return this.forcedShapeDrawType;
            }

            set
            {
                this.forcedShapeDrawType = value;
            }
        }

        protected bool UseDashStyle
        {
            get
            {
                return this.useDashStyle;
            }

            set
            {
                this.useDashStyle = value;
            }
        }
        
        /// <summary>
        /// Different shapes may not require all the points given to them, and as such
        /// if the user is drawing for a long time there may be lots of memory that's
        /// allocated that doesn't need to be. So before CreateShapePath is called,
        /// this method is called first.
        /// For example, the LineTool would return a new array containing only the
        /// first and last points.
        /// It is ok to return the same array that was passed in, even if it is modified.
        /// </summary>
        /// <param name="points">A list containing PointF instances.</param>
        /// <returns></returns>
        protected virtual List<PointF> TrimShapePath(List<PointF> trimThesePoints)
        {
            return trimThesePoints;
        }

        /// <summary>
        /// Override this function to return an "optimized" region that encompasses 
        /// the shape's outline. For example, a circle would return a list of rectangles
        /// that traces the outline. This is necessary because normally simplification
        /// will produce a region that, for a circle's outline, encompasses its
        /// interior as well. If you return null, then the default simplification
        /// algorithm will be used.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        protected virtual RectangleF[] GetOptimizedShapeOutlineRegion(PointF[] optimizeThesePoints, PdnGraphicsPath path)
        {
            return null;
        }

        // Implement this!
        protected abstract PdnGraphicsPath CreateShapePath(PointF[] shapePoints);

        protected override void OnActivate()
        {
            base.OnActivate();

            outlineSaveRegion = null;
            interiorSaveRegion = null;

            // creates a bitmap layer from the active layer
            bitmapLayer = (BitmapLayer)ActiveLayer;

            // create Graphics object
            renderArgs = new RenderArgs(bitmapLayer.Surface);

            lastDrawnRegion = new PdnRegion();
            lastDrawnRegion.MakeEmpty();
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate();

            if (mouseDown)
            {
                PointF lastPoint = (PointF)points[points.Count - 1];
                OnStylusUp(new StylusEventArgs(mouseButton, 0, lastPoint.X, lastPoint.Y, 0));
            }

            if (!this.shapeWasCommited)
            {
                CommitShape();
            }

            bitmapLayer = null;

            if (renderArgs != null)
            {
                renderArgs.Dispose();
                renderArgs = null;
            }

            if (outlineSaveRegion != null)
            {
                outlineSaveRegion.Dispose();
                outlineSaveRegion = null;
            }

            if (interiorSaveRegion != null)
            {
                interiorSaveRegion.Dispose();
                interiorSaveRegion = null;
            }

            points = null;
        }

        protected virtual void OnShapeBegin()
        {
        }

        /// <summary>
        /// Called when the shape is finished being traced by the default input handlers.
        /// </summary>
        /// <remarks>Do not call the base implementation of this method if you are overriding it.</remarks>
        /// <returns>true to commit the shape immediately</returns>
        protected virtual bool OnShapeEnd()
        {
            return true;
        }

        protected override void OnStylusDown(StylusEventArgs  e)
        {
            base.OnStylusDown(e);

            if (!this.shapeWasCommited)
            {
                CommitShape();
            }
            
            this.ClearSavedMemory();
            this.ClearSavedRegion();

            cursorMouseUp = Cursor;
            Cursor = cursorMouseDown;

            if (mouseDown && e.Button == mouseButton)
            {
                return;
            }

            if (mouseDown)
            {
                moveOriginMode = true;
                lastXY = new PointF(e.Fx, e.Fy);
                OnStylusMove(e);
            }
            else if (((e.Button & MouseButtons.Left) == MouseButtons.Left) ||
                     ((e.Button & MouseButtons.Right) == MouseButtons.Right))
            {
                // begin new shape
                this.shapeWasCommited = false;

                OnShapeBegin();

                mouseDown = true;
                mouseButton = e.Button;

                using (PdnRegion clipRegion = Selection.CreateRegion())
                {
                    renderArgs.Graphics.SetClip(clipRegion.GetRegionReadOnly(), CombineMode.Replace);
                }

                // reset the points we're drawing!
                points = new List<PointF>();

                OnStylusMove(e);
            }
        }

        protected override void OnStylusMove(StylusEventArgs e)
        {
            base.OnStylusMove (e);

            if (moveOriginMode)
            {
                SizeF delta = new SizeF(e.Fx - lastXY.X, e.Fy - lastXY.Y);

                for (int i = 0; i < points.Count; ++i)
                {
                    PointF ptF = (PointF)points[i];
                    ptF.X += delta.Width;
                    ptF.Y += delta.Height;
                    points[i] = ptF;
                }

                lastXY = new PointF(e.Fx, e.Fy);
            }
            else if (mouseDown && ((e.Button & mouseButton) != MouseButtons.None))
            {
                PointF mouseXY = new PointF(e.Fx, e.Fy);
                points.Add(mouseXY);
            }
        }

        public virtual PixelOffsetMode GetPixelOffsetMode()
        {
            return PixelOffsetMode.Half;
        }

        protected List<PointF> GetTrimmedShapePath()
        {
            List<PointF> pointsCopy = new List<PointF>(this.points);
            pointsCopy = TrimShapePath(pointsCopy);
            return pointsCopy;
        }

        protected void SetShapePath(List<PointF> newPoints)
        {
            this.points = newPoints;
        }

        protected void RenderShape()
        {
            // create the Pen we will use to draw with
            Pen outlinePen = null;
            Brush interiorBrush = null;
            PenInfo pi = AppEnvironment.PenInfo;
            BrushInfo bi = AppEnvironment.BrushInfo;

            ColorBgra primary = AppEnvironment.PrimaryColor;
            ColorBgra secondary = AppEnvironment.SecondaryColor;

            if (!ForceShapeDrawType && AppEnvironment.ShapeDrawType == ShapeDrawType.Interior)
            {
                Utility.Swap(ref primary, ref secondary);
            }

            // Initialize pens and brushes to the correct colors
            if ((mouseButton & MouseButtons.Left) == MouseButtons.Left)
            {
                outlinePen = pi.CreatePen(AppEnvironment.BrushInfo, primary.ToColor(), secondary.ToColor());
                interiorBrush = bi.CreateBrush(secondary.ToColor(), primary.ToColor());
            }
            else if ((mouseButton & MouseButtons.Right) == MouseButtons.Right)
            {
                outlinePen = pi.CreatePen(AppEnvironment.BrushInfo, secondary.ToColor(), primary.ToColor());
                interiorBrush = bi.CreateBrush(primary.ToColor(), secondary.ToColor());
            }

            if (!this.useDashStyle)
            {
                outlinePen.DashStyle = DashStyle.Solid;
            }

            outlinePen.LineJoin = LineJoin.MiterClipped;
            outlinePen.MiterLimit = 2;

            // redraw the old saveSurface
            if (interiorSaveRegion != null)
            {
                RestoreRegion(interiorSaveRegion);
                interiorSaveRegion.Dispose();
                interiorSaveRegion = null;
            }

            if (outlineSaveRegion != null)
            {
                RestoreRegion(outlineSaveRegion);
                outlineSaveRegion.Dispose();
                outlineSaveRegion = null;
            }

            // anti-aliasing? Don't mind if I do
            if (AppEnvironment.AntiAliasing)
            {
                renderArgs.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            }
            else
            {
                renderArgs.Graphics.SmoothingMode = SmoothingMode.None;
            }

            // also set the pixel offset mode
            renderArgs.Graphics.PixelOffsetMode = GetPixelOffsetMode();

            // figure out how we're going to draw
            ShapeDrawType drawType;

            if (ForceShapeDrawType)
            {
                drawType = ForcedShapeDrawType;
            }
            else
            {
                drawType = AppEnvironment.ShapeDrawType;
            }

            // get the region we want to save
            points = this.TrimShapePath(points);
            PointF[] pointsArray = points.ToArray();
            PdnGraphicsPath shapePath = CreateShapePath(pointsArray);

            if (shapePath != null)
            {
                // create non-optimized interior region
                PdnRegion interiorRegion = new PdnRegion(shapePath);

                // create non-optimized outline region
                PdnRegion outlineRegion;

                using (PdnGraphicsPath outlinePath = (PdnGraphicsPath)shapePath.Clone())
                {
                    try
                    {
                        outlinePath.Widen(outlinePen);
                        outlineRegion = new PdnRegion(outlinePath);
                    }

                    // Sometimes GDI+ gets cranky if we have a very small shape (e.g. all points
                    // are coincident). 
                    catch (OutOfMemoryException)
                    {
                        outlineRegion = new PdnRegion(shapePath);
                    }
                }

                // create optimized outlineRegion for purposes of rendering, if it is possible to do so
                // shapes will often provide an "optimized" region that circumvents the fact that
                // we'd otherwise get a region that encompasses the outline *and* the interior, thus
                // slowing rendering significantly in many cases.
                RectangleF[] optimizedOutlineRegion = GetOptimizedShapeOutlineRegion(pointsArray, shapePath);
                PdnRegion invalidOutlineRegion;

                if (optimizedOutlineRegion != null)
                {
                    Utility.InflateRectanglesInPlace(optimizedOutlineRegion, (int)(outlinePen.Width + 2));
                    invalidOutlineRegion = Utility.RectanglesToRegion(optimizedOutlineRegion);
                }
                else
                {
                    invalidOutlineRegion = Utility.SimplifyAndInflateRegion(outlineRegion, Utility.DefaultSimplificationFactor, (int)(outlinePen.Width + 2));
                }

                // create optimized interior region
                PdnRegion invalidInteriorRegion = Utility.SimplifyAndInflateRegion(interiorRegion, Utility.DefaultSimplificationFactor, 3);

                PdnRegion invalidRegion = new PdnRegion();
                invalidRegion.MakeEmpty();

                // set up alpha blending
                renderArgs.Graphics.CompositingMode = AppEnvironment.GetCompositingMode();

                SaveRegion(invalidOutlineRegion, invalidOutlineRegion.GetBoundsInt());
                this.outlineSaveRegion = invalidOutlineRegion;
                if ((drawType & ShapeDrawType.Outline) != 0)
                {
                    shapePath.Draw(renderArgs.Graphics, outlinePen);
                }

                invalidRegion.Union(invalidOutlineRegion);

                // draw shape
                if ((drawType & ShapeDrawType.Interior) != 0)
                {
                    SaveRegion(invalidInteriorRegion, invalidInteriorRegion.GetBoundsInt());
                    this.interiorSaveRegion = invalidInteriorRegion;
                    renderArgs.Graphics.FillPath(interiorBrush, shapePath);
                    invalidRegion.Union(invalidInteriorRegion);
                }
                else
                {
                    invalidInteriorRegion.Dispose();
                    invalidInteriorRegion = null;
                }

                bitmapLayer.Invalidate(invalidRegion);

                invalidRegion.Dispose();
                invalidRegion = null;

                outlineRegion.Dispose();
                outlineRegion = null;

                interiorRegion.Dispose();
                interiorRegion = null;
            }

            Update();

            if (shapePath != null)
            {
                shapePath.Dispose();
                shapePath = null;
            }

            outlinePen.Dispose();
            interiorBrush.Dispose();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            // if mouse button not down then leave function
            if (mouseDown && ((e.Button & mouseButton) != MouseButtons.None))
            {
                RenderShape();
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (mouseDown)
            {
                RenderShape();
            }

            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (mouseDown)
            {
                RenderShape();
            }

            base.OnKeyUp(e);
        }

        protected virtual void OnShapeCommitting()
        {
        }

        protected void CommitShape()
        {
            OnShapeCommitting();

            mouseDown = false;

            ArrayList has = new ArrayList();
            PdnRegion activeRegion = Selection.CreateRegion();

            if (outlineSaveRegion != null)
            {
                using (PdnRegion clipTest = activeRegion.Clone())
                {
                    clipTest.Intersect(outlineSaveRegion);
                    
                    if (!clipTest.IsEmpty())
                    {
                        BitmapHistoryMemento bha = new BitmapHistoryMemento(Name, Image, this.DocumentWorkspace, 
                            ActiveLayerIndex, outlineSaveRegion, this.ScratchSurface);

                        has.Add(bha);
                        outlineSaveRegion.Dispose();
                        outlineSaveRegion = null;
                    }
                }
            }

            if (interiorSaveRegion != null)
            {
                using (PdnRegion clipTest = activeRegion.Clone())
                {
                    clipTest.Intersect(interiorSaveRegion);
                        
                    if (!clipTest.IsEmpty())
                    {
                        BitmapHistoryMemento bha = new BitmapHistoryMemento(Name, Image, this.DocumentWorkspace,
                            ActiveLayerIndex, interiorSaveRegion, this.ScratchSurface);

                        has.Add(bha);
                        interiorSaveRegion.Dispose();
                        interiorSaveRegion = null;
                    }
                }
            }

            if (has.Count > 0)
            {
                CompoundHistoryMemento cha = new CompoundHistoryMemento(Name, Image, (HistoryMemento[])has.ToArray(typeof(HistoryMemento)));

                if (this.chaAlreadyOnStack == null)
                {
                    HistoryStack.PushNewMemento(cha);
                }
                else
                {
                    this.chaAlreadyOnStack.PushNewAction(cha);
                    this.chaAlreadyOnStack = null;
                }
            }

            activeRegion.Dispose();
            points = null;
            Update();
            this.shapeWasCommited = true;
        }

        protected override void OnStylusUp(StylusEventArgs e)
        {
            base.OnStylusUp(e);

            Cursor = cursorMouseUp;

            if (moveOriginMode)
            {
                moveOriginMode = false;
            }
            else if (mouseDown)
            {
                bool doCommit = OnShapeEnd();

                if (doCommit)
                {
                    CommitShape();
                }
                else
                {
                    // place a 'sentinel' history action on the stack that will be filled in later
                    CompoundHistoryMemento cha = new CompoundHistoryMemento(Name, Image, new List<HistoryMemento>());
                    HistoryStack.PushNewMemento(cha);
                    this.chaAlreadyOnStack = cha;
                }
            }
        }

        public ShapeTool(DocumentWorkspace documentWorkspace,
                         ImageResource toolBarImage,
                         string name,
                         string helpText)
            : this(documentWorkspace,
                   toolBarImage,
                   name,
                   helpText,
                   defaultShortcut,
                   ToolBarConfigItems.None,
                   ToolBarConfigItems.None)
        {
        }

        public ShapeTool(DocumentWorkspace documentWorkspace,
                         ImageResource toolBarImage,
                         string name,
                         string helpText,
                         ToolBarConfigItems toolBarConfigItemsInclude,
                         ToolBarConfigItems toolBarConfigItemsExclude)
            : this(documentWorkspace,
                   toolBarImage,
                   name,
                   helpText,
                   defaultShortcut,
                   toolBarConfigItemsInclude,
                   toolBarConfigItemsExclude)
        {
        }

        public ShapeTool(DocumentWorkspace documentWorkspace,
                         ImageResource toolBarImage,
                         string name,
                         string helpText,
                         char hotKey,
                         ToolBarConfigItems toolBarConfigItemsInclude,
                         ToolBarConfigItems toolBarConfigItemsExclude)
            : base(documentWorkspace,
                   toolBarImage,
                   name,
                   helpText,
                   hotKey,
                   false,
                   (toolBarConfigItemsInclude |
                       (ToolBarConfigItems.Brush | 
                        ToolBarConfigItems.Pen | 
                        ToolBarConfigItems.ShapeType | 
                        ToolBarConfigItems.Antialiasing | 
                        ToolBarConfigItems.AlphaBlending)) &
                       ~(toolBarConfigItemsExclude))
        {
            this.mouseDown = false;
            this.points = null;
            this.cursorMouseUp = new Cursor(PdnResources.GetResourceStream("Cursors.ShapeToolCursor.cur"));
            this.cursorMouseDown = new Cursor(PdnResources.GetResourceStream("Cursors.ShapeToolCursorMouseDown.cur"));
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose (disposing);

            if (disposing)
            {
                if (cursorMouseUp != null)
                {
                    cursorMouseUp.Dispose();
                    cursorMouseUp = null;
                }

                if (cursorMouseDown != null)
                {
                    cursorMouseDown.Dispose();
                    cursorMouseDown = null;
                }
            }
        }
    }
}
