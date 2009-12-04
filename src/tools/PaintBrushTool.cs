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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;

namespace PaintDotNet.Tools
{
    internal class PaintBrushTool
        : Tool 
    {
        private bool mouseDown;
        private Brush brush;
        private MouseButtons mouseButton;
        private List<Rectangle> savedRects;
        private PointF lastMouseXY;
        private PointF lastNorm;
        private PointF lastDir;
        private RenderArgs renderArgs;
        private BitmapLayer bitmapLayer;
        private Cursor cursorMouseDown;
        private Cursor cursorMouseUp;
        private BrushPreviewRenderer previewRenderer;

        protected override bool SupportsInk
        {
            get
            {
                return true;
            }
        }

        protected override void OnMouseEnter()
        {
            this.previewRenderer.Visible = true;
            base.OnMouseEnter();
        }

        protected override void OnMouseLeave()
        {
            this.previewRenderer.Visible = false;
            base.OnMouseLeave();
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            cursorMouseUp = new Cursor(PdnResources.GetResourceStream("Cursors.PaintBrushToolCursor.cur"));
            cursorMouseDown = new Cursor(PdnResources.GetResourceStream("Cursors.PaintBrushToolCursorMouseDown.cur"));
            Cursor = cursorMouseUp;
            
            this.savedRects = new List<Rectangle>();

            if (ActiveLayer != null)
            {
                bitmapLayer = (BitmapLayer)ActiveLayer;
                renderArgs = new RenderArgs(bitmapLayer.Surface);
            }
            else
            {
                bitmapLayer = null;
                renderArgs = null;
            }

            this.previewRenderer = new BrushPreviewRenderer(this.RendererList);
            this.RendererList.Add(this.previewRenderer, false);

            mouseDown = false;
        }

        protected override void OnDeactivate()
        {
            if (mouseDown)
            {
                OnStylusUp(new StylusEventArgs(mouseButton, 0, lastMouseXY.X, lastMouseXY.Y, 0));
            }

            this.RendererList.Remove(this.previewRenderer);
            this.previewRenderer.Dispose();
            this.previewRenderer = null;

            this.savedRects = null;

            if (renderArgs != null)
            {
                renderArgs.Dispose();
                renderArgs = null;
            }

            bitmapLayer = null;

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

            base.OnDeactivate();
        }

        private float GetWidth(float Pressure) 
        {
            return Pressure * Pressure * AppEnvironment.PenInfo.Width * 0.5f;
        }

        protected override void OnStylusDown(StylusEventArgs e)
        {
            base.OnStylusDown(e);

            if (mouseDown)
            {
                return;
            }

            ClearSavedMemory();

            this.previewRenderer.Visible = false;

            Cursor = cursorMouseDown;

            if (((e.Button & MouseButtons.Left) == MouseButtons.Left) ||
                ((e.Button & MouseButtons.Right) == MouseButtons.Right))
            {
                mouseButton = e.Button;

                if ((mouseButton & MouseButtons.Left) == MouseButtons.Left)
                {
                    brush = AppEnvironment.CreateBrush(false);
                }
                else if ((mouseButton & MouseButtons.Right) == MouseButtons.Right)
                {
                    brush = AppEnvironment.CreateBrush(true);
                }

                lastMouseXY.X = e.Fx;
                lastMouseXY.Y = e.Fy;

                mouseDown = true;
                mouseButton = e.Button;

                using (PdnRegion clipRegion = Selection.CreateRegion())
                {
                    renderArgs.Graphics.SetClip(clipRegion.GetRegionReadOnly(), CombineMode.Replace);
                }

                this.OnStylusMove(new StylusEventArgs(e.Button, e.Clicks, unchecked(e.Fx + 0.01f), e.Fy, e.Delta, e.Pressure));
            }
        }

        private PointF[] MakePolygon(PointF a, PointF b, PointF c, PointF d) 
        {
            PointF dirA = new PointF(a.X - b.X, a.Y - b.Y);
            PointF dirB = new PointF(c.X - d.X, c.Y - d.Y);

            // Swap points as necessary to keep the polygon winding one direction
            if (dirA.X * dirB.X + dirA.Y * dirB.Y > 0)
            {
                return new PointF[] { a, b, d, c };
            } 
            else
            {   
                return new PointF[] { a, b, c, d };
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (this.mouseDown && e.Button != MouseButtons.None) 
            {
                // This is done so that if drawing falls behind due to a
                // large queue of stylus inputs, it won't do any updates
                // until it's done. This is accomplished by only updating
                // when a MouseMove is caught   
                Update();
            }

            base.OnMouseMove(e);
        }

        protected override void OnStylusMove(StylusEventArgs e)
        {
            base.OnStylusMove(e);
            PointF currMouseXY = new PointF(e.Fx, e.Fy);

            if (mouseDown && ((e.Button & mouseButton) != MouseButtons.None))
            {
                float pressure = GetWidth(e.Pressure);
                float length;
                PointF a = lastMouseXY;
                PointF b = currMouseXY;
                PointF dir = new PointF(b.X - a.X, b.Y - a.Y);
                PointF norm;
                PointF[] poly;
                RectangleF dotRect = Utility.RectangleFromCenter(currMouseXY, pressure);

                if (pressure > 0.5f)
                {
                    renderArgs.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
                }
                else
                {
                    renderArgs.Graphics.PixelOffsetMode = PixelOffsetMode.None;
                }

                // save direction before normalizing
                lastDir = dir;

                // normalize
                length = Utility.Magnitude(dir);
                dir.X /= length;
                dir.Y /= length;
                
                // compute normal vector, calculate perpendicular offest from stroke for width
                norm = new PointF(dir.Y, -dir.X);
                norm.X *= pressure;
                norm.Y *= pressure;

                a.X -= dir.X * 0.1666f;
                a.Y -= dir.Y * 0.1666f;

                lastNorm = norm;

                poly = MakePolygon(
                    new PointF(a.X - lastNorm.X, a.Y - lastNorm.Y),
                    new PointF(a.X + lastNorm.X, a.Y + lastNorm.Y),
                    new PointF(b.X + norm.X, b.Y + norm.Y),
                    new PointF(b.X - norm.X, b.Y - norm.Y));

                RectangleF saveRect = RectangleF.Union(
                    dotRect,
                    RectangleF.Union(
                        Utility.PointsToRectangle(poly[0], poly[1]),
                        Utility.PointsToRectangle(poly[2], poly[3])));

                saveRect.Inflate(2.0f, 2.0f); // account for anti-aliasing

                saveRect.Intersect(ActiveLayer.Bounds);

                // drawing outside of the canvas is a no-op, so don't do anything in that case!
                // also make sure we're within the clip region
                if (saveRect.Width > 0 && saveRect.Height > 0 && renderArgs.Graphics.IsVisible(saveRect))
                {
                    Rectangle saveRectRounded = Utility.RoundRectangle(saveRect);
                    saveRectRounded.Intersect(ActiveLayer.Bounds);

                    if (saveRectRounded.Width > 0 && saveRectRounded.Height > 0)
                    {
                        SaveRegion(null, saveRectRounded);
                        this.savedRects.Add(saveRectRounded);

                        if (AppEnvironment.AntiAliasing)
                        {
                            renderArgs.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        }
                        else
                        {
                            renderArgs.Graphics.SmoothingMode = SmoothingMode.None;
                        }

                        renderArgs.Graphics.CompositingMode = AppEnvironment.GetCompositingMode();

                        renderArgs.Graphics.FillEllipse(brush, dotRect);

                        // bail out early if the mouse hasn't even moved. If we don't bail out, we'll get a 0-distance move, which will result in a div-by-0
                        if (lastMouseXY != currMouseXY)
                        {
                            renderArgs.Graphics.FillPolygon(brush, poly, FillMode.Winding);
                        }
                    }

                    bitmapLayer.Invalidate(saveRectRounded);
                }

                lastNorm = norm;
                lastMouseXY = currMouseXY;
            }
            else
            {
                lastMouseXY = currMouseXY;
                lastNorm = PointF.Empty;
                lastDir = PointF.Empty;
                this.previewRenderer.BrushSize = AppEnvironment.PenInfo.Width / 2.0f;
            }

            this.previewRenderer.BrushLocation = currMouseXY;
        }

        protected override void OnStylusUp(StylusEventArgs e)
        {
            base.OnStylusUp(e);

            Cursor = cursorMouseUp;

            if (mouseDown)
            {
                this.previewRenderer.Visible = true;
                mouseDown = false;

                if (this.savedRects.Count > 0)
                {
                    PdnRegion saveMeRegion = Utility.RectanglesToRegion(this.savedRects.ToArray());
                    HistoryMemento ha = new BitmapHistoryMemento(Name, Image, DocumentWorkspace, 
                        ActiveLayerIndex, saveMeRegion, this.ScratchSurface);
                    HistoryStack.PushNewMemento(ha);
                    saveMeRegion.Dispose();
                    this.savedRects.Clear();
                    this.ClearSavedMemory();
                }

                this.brush.Dispose();
                this.brush = null;
            }
        }

        public PaintBrushTool(DocumentWorkspace documentWorkspace)
            : base(documentWorkspace,
                   PdnResources.GetImageResource("Icons.PaintBrushToolIcon.png"),
                   PdnResources.GetString("PaintBrushTool.Name"),
                   PdnResources.GetString("PaintBrushTool.HelpText"),
                   'b',
                   false,
                   ToolBarConfigItems.Brush | ToolBarConfigItems.Pen | ToolBarConfigItems.Antialiasing | ToolBarConfigItems.AlphaBlending)
        {
            // initialize any state information you need
            mouseDown = false;
        }
    }
}
