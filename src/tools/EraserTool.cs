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
using System.Diagnostics;
using System.Windows.Forms;

namespace PaintDotNet.Tools
{
    internal class EraserTool 
        : Tool
    {
        private bool mouseDown;
        private MouseButtons mouseButton;
        private List<Rectangle> savedRects;
        private Point lastMouseXY;
        private RenderArgs renderArgs;
        private BitmapLayer bitmapLayer;
        private Cursor cursorMouseDown;
        private Cursor cursorMouseUp;
        private BrushPreviewRenderer previewRenderer;

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

            // cursor-transitions
            this.cursorMouseUp = new Cursor(PdnResources.GetResourceStream("Cursors.EraserToolCursor.cur"));
            this.cursorMouseDown = new Cursor(PdnResources.GetResourceStream("Cursors.EraserToolCursorMouseDown.cur"));
            this.Cursor = cursorMouseUp;

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
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate();

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
            
            if (mouseDown)
            {
                OnMouseUp(new MouseEventArgs(mouseButton, 0, lastMouseXY.X, lastMouseXY.Y, 0));
            }

            this.RendererList.Remove(this.previewRenderer);
            this.previewRenderer.Dispose();
            this.previewRenderer = null;

            this.savedRects.Clear();

            if (renderArgs != null)
            {
                renderArgs.Dispose();
                renderArgs = null;
            }

            bitmapLayer = null;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown (e);

            this.Cursor = this.cursorMouseDown;

            if (((e.Button & MouseButtons.Left) == MouseButtons.Left) ||
                ((e.Button & MouseButtons.Right) == MouseButtons.Right))
            {
                this.previewRenderer.Visible = false;

                mouseDown = true;
                mouseButton = e.Button;

                lastMouseXY.X = e.X;
                lastMouseXY.Y = e.Y;

                PdnRegion clipRegion = Selection.CreateRegion();
                renderArgs.Graphics.SetClip(clipRegion.GetRegionReadOnly(), CombineMode.Replace);
                clipRegion.Dispose();

                OnMouseMove(e);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp (e);

            Cursor = cursorMouseUp;

            if (mouseDown)
            {
                OnMouseMove(e);
                this.previewRenderer.Visible = true;
                mouseDown = false;

                if (this.savedRects.Count > 0)
                {
                    Rectangle[] savedScans = this.savedRects.ToArray();
                    PdnRegion saveMeRegion = Utility.RectanglesToRegion(savedScans);

                    HistoryMemento ha = new BitmapHistoryMemento(Name, Image, DocumentWorkspace, 
                        ActiveLayerIndex, saveMeRegion, this.ScratchSurface);

                    HistoryStack.PushNewMemento(ha);
                    saveMeRegion.Dispose();
                    this.savedRects.Clear();
                    ClearSavedMemory();
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (mouseDown && ((e.Button & mouseButton) != MouseButtons.None))
            {
                int alpha;

                if (e.Button == MouseButtons.Left)
                {
                    alpha = AppEnvironment.PrimaryColor.A;
                }
                else
                {
                    alpha = AppEnvironment.SecondaryColor.A;
                }

                Pen pen = AppEnvironment.PenInfo.CreatePen(AppEnvironment.BrushInfo,
                    Color.FromArgb(alpha, 0, 0, 0), Color.FromArgb(alpha, 0, 0, 0));

                Point a = lastMouseXY;
                Point b = new Point(e.X, e.Y);

                Rectangle saveRect = Utility.PointsToRectangle(a, b);

                saveRect.Inflate((int)Math.Ceiling(pen.Width), (int)Math.Ceiling(pen.Width));

                if (renderArgs.Graphics.SmoothingMode == SmoothingMode.AntiAlias)
                {
                    saveRect.Inflate(1, 1);
                }

                saveRect.Intersect(ActiveLayer.Bounds);

                // drawing outside of the canvas is a no-op, so don't do anything in that case!
                // also make sure we're within the clip region
                if (saveRect.Width > 0 && saveRect.Height > 0 && renderArgs.Graphics.IsVisible(saveRect))
                {
                    SaveRegion(null, saveRect);
                    this.savedRects.Add(saveRect);

                    if (AppEnvironment.AntiAliasing)
                    {
                        renderArgs.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    }
                    else
                    {
                        renderArgs.Graphics.SmoothingMode = SmoothingMode.None;
                    }

                    new UnaryPixelOps.InvertWithAlpha().Apply(renderArgs.Surface, saveRect);
                    renderArgs.Graphics.CompositingMode = CompositingMode.SourceOver;

                    if (pen.Width > 1)
                    {
                        renderArgs.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
                    }
                    else
                    {
                        renderArgs.Graphics.PixelOffsetMode = PixelOffsetMode.None;
                    }

                    pen.EndCap = LineCap.Round;
                    pen.StartCap = LineCap.Round;
                    renderArgs.Graphics.DrawLine(pen, a, b);
                    renderArgs.Graphics.FillEllipse(pen.Brush, a.X - pen.Width / 2.0f, a.Y - pen.Width / 2.0f, pen.Width, pen.Width);

                    new UnaryPixelOps.InvertWithAlpha().Apply(renderArgs.Surface, saveRect);

                    new BinaryPixelOps.SetColorChannels().Apply(renderArgs.Surface, saveRect.Location, 
                        ScratchSurface, saveRect.Location, saveRect.Size);

                    bitmapLayer.Invalidate(saveRect);
                    Update();
                }

                lastMouseXY = b;
                pen.Dispose();
            }
            else
            {
                this.previewRenderer.BrushLocation = new Point(e.X, e.Y);
                this.previewRenderer.BrushSize = AppEnvironment.PenInfo.Width / 2.0f;
            }
        }

        public EraserTool(DocumentWorkspace documentWorkspace) 
            : base(documentWorkspace,
                   PdnResources.GetImageResource("Icons.EraserToolIcon.png"),
                   PdnResources.GetString("EraserTool.Name"),
                   PdnResources.GetString("EraserTool.HelpText"), //"Click and drag to erase a portion of the image",
                   'e',
                   false,
                   ToolBarConfigItems.Pen | ToolBarConfigItems.Antialiasing)
        {
        }
    }
}
