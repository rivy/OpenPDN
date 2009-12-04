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
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PaintDotNet.Tools
{
    /// <summary>
    /// Allows the user to click on the image to zoom to that location
    /// </summary>
    internal class ZoomTool
        : Tool
    {
        private bool moveOffsetMode = false;
        private MouseButtons mouseDown;
        private Point downPt;
        private Point lastPt;
        private Rectangle rect = Rectangle.Empty;
        private Cursor cursorZoomIn;
        private Cursor cursorZoomOut;
        private Cursor cursorZoom;
        private Cursor cursorZoomPan;
        private SelectionRenderer outlineRenderer;
        private Selection outline;

        public ZoomTool(DocumentWorkspace documentWorkspace)
            : base(documentWorkspace,
                   PdnResources.GetImageResource("Icons.ZoomToolIcon.png"),
                   PdnResources.GetString("ZoomTool.Name"),
                   PdnResources.GetString("ZoomTool.HelpText"),
                   'z',
                   false,
                   ToolBarConfigItems.None)
        {
            this.mouseDown = MouseButtons.None;
        }

        protected override void OnActivate()
        {
            this.cursorZoom = new Cursor(PdnResources.GetResourceStream("Cursors.ZoomToolCursor.cur"));
            this.cursorZoomIn = new Cursor(PdnResources.GetResourceStream("Cursors.ZoomInToolCursor.cur"));
            this.cursorZoomOut = new Cursor(PdnResources.GetResourceStream("Cursors.ZoomOutToolCursor.cur"));
            this.cursorZoomPan = new Cursor(PdnResources.GetResourceStream("Cursors.ZoomOutToolCursor.cur"));
            this.Cursor = this.cursorZoom;

            base.OnActivate();
            
            this.outline = new Selection();
            this.outlineRenderer = new SelectionRenderer(this.RendererList, this.outline, this.DocumentWorkspace);
            this.outlineRenderer.InvertedTinting = true;
            this.outlineRenderer.TintColor = Color.FromArgb(128, 255, 255, 255);
            this.outlineRenderer.ResetOutlineWhiteOpacity();
            this.RendererList.Add(this.outlineRenderer, true);
        }

        protected override void OnDeactivate()
        {
            if (cursorZoom != null)
            {
                cursorZoom.Dispose();
                cursorZoom = null;
            }

            if (cursorZoomIn != null)
            {
                cursorZoomIn.Dispose();
                cursorZoomIn = null;
            }

            if (cursorZoomOut != null)
            {
                cursorZoomOut.Dispose();
                cursorZoomOut = null;
            }

            if (cursorZoomPan != null)
            {
                cursorZoomPan.Dispose();
                cursorZoomPan = null;
            }

            this.RendererList.Remove(this.outlineRenderer);
            this.outlineRenderer.Dispose();
            this.outlineRenderer = null;
            
            base.OnDeactivate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (mouseDown != MouseButtons.None)
            {
                this.moveOffsetMode = true;
            }
            else
            {
                switch (e.Button) 
                {
                    case MouseButtons.Left:
                        Cursor = cursorZoomIn;
                        break;

                    case MouseButtons.Middle:
                        Cursor = cursorZoomPan;
                        break;

                    case MouseButtons.Right:
                        Cursor = cursorZoomOut;
                        break;
                }

                mouseDown = e.Button;
                lastPt = new Point(e.X, e.Y);
                downPt = lastPt;
                OnMouseMove(e);
            }
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (!e.Handled)
            {
                if (this.mouseDown != MouseButtons.None)
                {
                    e.Handled = true;
                }
            }

            base.OnKeyPress(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove (e);

            Point thisPt = new Point(e.X, e.Y);

            if (this.moveOffsetMode)
            {
                Size delta = new Size(thisPt.X - lastPt.X, thisPt.Y - lastPt.Y);
                downPt.X += delta.Width;
                downPt.Y += delta.Height;
            }

            if ((e.Button == MouseButtons.Left && 
                 mouseDown == MouseButtons.Left && 
                 Utility.Distance(thisPt, downPt) > 10) ||  // if they've moved the mouse more than 10 pixels since they clicked
                 !rect.IsEmpty) //don't undraw the rectangle
            {
                rect = Utility.PointsToRectangle(downPt, thisPt);
                rect.Intersect(ActiveLayer.Bounds);
                UpdateDrawnRect();
            } 
            else if (e.Button == MouseButtons.Middle && mouseDown == MouseButtons.Middle)
            {
                PointF lastScrollPosition = DocumentWorkspace.DocumentScrollPositionF;
                lastScrollPosition.X += thisPt.X - lastPt.X;
                lastScrollPosition.Y += thisPt.Y - lastPt.Y;
                DocumentWorkspace.DocumentScrollPositionF = lastScrollPosition;
                Update();
            }
            else
            {
                rect = Rectangle.Empty;
            }

            lastPt = thisPt;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp (e);
            OnMouseMove(e);
            bool resetMouseDown = true;

            Cursor = cursorZoom;

            if (this.moveOffsetMode)
            {
                this.moveOffsetMode = false;
                resetMouseDown = false;
            } 
            else if (mouseDown == MouseButtons.Left || mouseDown == MouseButtons.Right) 
            {
                Rectangle zoomTo = rect;

                rect = Rectangle.Empty;
                UpdateDrawnRect();

                if (e.Button == MouseButtons.Left) 
                {
                    if (Utility.Magnitude(new PointF(zoomTo.Width, zoomTo.Height)) < 10) 
                    {
                        DocumentWorkspace.ZoomIn();
                        DocumentWorkspace.RecenterView(new Point(e.X, e.Y));
                    } 
                    else
                    {
                        DocumentWorkspace.ZoomToRectangle(zoomTo);
                    }
                }
                else
                {
                    DocumentWorkspace.ZoomOut();
                    DocumentWorkspace.RecenterView(new Point(e.X, e.Y));
                }

                this.outline.Reset();
            }

            if (resetMouseDown)
            {
                mouseDown = MouseButtons.None;
            }
        }

        private void UpdateDrawnRect() 
        {
            if (!rect.IsEmpty)
            {
                this.outline.PerformChanging();
                this.outline.Reset();
                this.outline.SetContinuation(rect, CombineMode.Replace);
                this.outlineRenderer.ResetOutlineWhiteOpacity();
                this.outline.CommitContinuation();
                this.outline.PerformChanged();
                Update();
            }
        }
    }
}
