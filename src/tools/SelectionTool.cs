/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.Actions;
using PaintDotNet.HistoryMementos;
using PaintDotNet.HistoryFunctions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PaintDotNet.Tools
{
    internal class SelectionTool
        : Tool
    {
        private bool tracking = false;
        private bool moveOriginMode = false;
        private Point lastXY;
        private SelectionHistoryMemento undoAction;
        private CombineMode combineMode;
        private List<Point> tracePoints = null;
        private DateTime startTime;
        private bool hasMoved = false;
        private bool append = false;
        private bool wasNotEmpty = false;

        private Selection newSelection;
        private SelectionRenderer newSelectionRenderer;

        private Cursor cursorMouseUp;
        private Cursor cursorMouseUpMinus;
        private Cursor cursorMouseUpPlus;
        private Cursor cursorMouseDown;

        protected CombineMode SelectionMode
        {
            get
            {
                return this.combineMode;
            }
        }

        protected void SetCursors(
            string cursorMouseUpResName,
            string cursorMouseUpMinusResName,
            string cursorMouseUpPlusResName,
            string cursorMouseDownResName)
        {
            if (this.cursorMouseUp != null)
            {
                this.cursorMouseUp.Dispose();
                this.cursorMouseUp = null;
            }

            if (cursorMouseUpResName != null)
            {
                this.cursorMouseUp = new Cursor(PdnResources.GetResourceStream(cursorMouseUpResName));
            }

            if (this.cursorMouseUpMinus != null)
            {
                this.cursorMouseUpMinus.Dispose();
                this.cursorMouseUpMinus = null;
            }

            if (cursorMouseUpMinusResName != null)
            {
                this.cursorMouseUpMinus = new Cursor(PdnResources.GetResourceStream(cursorMouseUpMinusResName));
            }

            if (this.cursorMouseUpPlus != null)
            {
                this.cursorMouseUpPlus.Dispose();
                this.cursorMouseUpPlus = null;
            }

            if (cursorMouseUpPlusResName != null)
            {
                this.cursorMouseUpPlus = new Cursor(PdnResources.GetResourceStream(cursorMouseUpPlusResName));
            }

            if (this.cursorMouseDown != null)
            {
                this.cursorMouseDown.Dispose();
                this.cursorMouseDown = null;
            }

            if (cursorMouseDownResName != null)
            {
                this.cursorMouseDown = new Cursor(PdnResources.GetResourceStream(cursorMouseDownResName));
            }
        }

        private Cursor GetCursor(bool mouseDown, bool ctrlDown, bool altDown)
        {
            Cursor cursor;

            if (mouseDown)
            {
                cursor = this.cursorMouseDown;
            }
            else if (ctrlDown)
            {
                cursor = this.cursorMouseUpPlus;
            }
            else if (altDown)
            {
                cursor = this.cursorMouseUpMinus;
            }
            else
            {
                cursor = this.cursorMouseUp;
            }

            return cursor;
        }

        private Cursor GetCursor()
        {
            return GetCursor(IsMouseDown, (ModifierKeys & Keys.Control) != 0, (ModifierKeys & Keys.Alt) != 0);
        }

        protected override void OnActivate()
        {
            // Assume that SetCursors() has been called by now

            this.Cursor = GetCursor();
            DocumentWorkspace.EnableSelectionTinting = true;

            this.newSelection = new Selection();
            this.newSelectionRenderer = new SelectionRenderer(this.RendererList, this.newSelection, this.DocumentWorkspace);
            this.newSelectionRenderer.EnableSelectionTinting = false;
            this.newSelectionRenderer.EnableOutlineAnimation = false;
            this.newSelectionRenderer.Visible = false;
            this.RendererList.Add(this.newSelectionRenderer, true);

            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            DocumentWorkspace.EnableSelectionTinting = false;

            if (this.tracking)
            {
                Done();
            }

            base.OnDeactivate();

            SetCursors(null, null, null, null); // dispose 'em

            this.RendererList.Remove(this.newSelectionRenderer);
            this.newSelectionRenderer.Dispose();
            this.newSelectionRenderer = null;
            this.newSelection = null;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            this.Cursor = GetCursor();

            if (tracking)
            {
                moveOriginMode = true;
                lastXY = new Point(e.X, e.Y);
                OnMouseMove(e);
            }
            else if ((e.Button & MouseButtons.Left) == MouseButtons.Left ||
                (e.Button & MouseButtons.Right) == MouseButtons.Right)
            {
                tracking = true;
                hasMoved = false;
                startTime = DateTime.Now;

                tracePoints = new List<Point>();
                tracePoints.Add(new Point(e.X, e.Y));

                undoAction = new SelectionHistoryMemento("sentinel", this.Image, DocumentWorkspace);

                wasNotEmpty = !Selection.IsEmpty;

                // Determine this.combineMode

                if ((ModifierKeys & Keys.Control) != 0 && e.Button == MouseButtons.Left)
                {
                    this.combineMode = CombineMode.Union;
                }
                else if ((ModifierKeys & Keys.Alt) != 0 && e.Button == MouseButtons.Left)
                {
                    this.combineMode = CombineMode.Exclude;
                }
                else if ((ModifierKeys & Keys.Control) != 0 && e.Button == MouseButtons.Right)
                {
                    this.combineMode = CombineMode.Xor;
                }
                else if ((ModifierKeys & Keys.Alt) != 0 && e.Button == MouseButtons.Right)
                {
                    this.combineMode = CombineMode.Intersect;
                }
                else
                {
                    this.combineMode = AppEnvironment.SelectionCombineMode;
                }


                DocumentWorkspace.EnableSelectionOutline = false;

                this.newSelection.Reset();
                PdnGraphicsPath basePath = Selection.CreatePath();
                this.newSelection.SetContinuation(basePath, CombineMode.Replace, true);
                this.newSelection.CommitContinuation();

                bool newSelectionRendererVisible = true;

                // Act on this.combineMode
                switch (this.combineMode)
                {
                    case CombineMode.Xor:
                        append = true;
                        Selection.ResetContinuation();
                        break;

                    case CombineMode.Union:
                        append = true;
                        Selection.ResetContinuation();
                        break;

                    case CombineMode.Exclude:
                        append = true;
                        Selection.ResetContinuation();
                        break;

                    case CombineMode.Replace:
                        append = false;
                        Selection.Reset();
                        break;

                    case CombineMode.Intersect:
                        append = true;
                        Selection.ResetContinuation();
                        break;

                    default:
                        throw new InvalidEnumArgumentException();
                }

                this.newSelectionRenderer.Visible = newSelectionRendererVisible;
            }
        }

        protected virtual List<Point> TrimShapePath(List<Point> trimTheseTracePoints)
        {
            return trimTheseTracePoints;
        }

        protected virtual List<PointF> CreateShape(List<Point> inputTracePoints)
        {
            List<PointF> points = Utility.PointListToPointFList(inputTracePoints);
            return points;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (moveOriginMode)
            {
                Size delta = new Size(e.X - lastXY.X, e.Y - lastXY.Y);
                
                for (int i = 0; i < tracePoints.Count; ++i)
                {
                    Point pt = (Point)tracePoints[i];
                    pt.X += delta.Width;
                    pt.Y += delta.Height;
                    tracePoints[i] = pt;
                }

                lastXY = new Point(e.X, e.Y);
                Render();
            }
            else if (tracking)
            {
                Point mouseXY = new Point(e.X, e.Y);

                if (mouseXY != (Point)tracePoints[tracePoints.Count - 1])
                {
                    tracePoints.Add(mouseXY);
                }
                
                hasMoved = true;
                Render();
            }
        }

        private PointF[] CreateSelectionPolygon()
        {
            List<Point> trimmedTrace = this.TrimShapePath(tracePoints);
            List<PointF> shapePoints = CreateShape(trimmedTrace);
            List<PointF> polygon;

            switch (this.combineMode)
            {
                case CombineMode.Xor:
                case CombineMode.Exclude:
                    polygon = shapePoints;
                    break;

                default:
                case CombineMode.Complement:
                case CombineMode.Intersect:
                case CombineMode.Replace:
                case CombineMode.Union:
                    polygon = Utility.SutherlandHodgman(DocumentWorkspace.Document.Bounds, shapePoints);
                    break;
            }

            return polygon.ToArray();
        }

        private void Render()
        {
            if (tracePoints != null && tracePoints.Count > 2)
            {
                PointF[] polygon = CreateSelectionPolygon();

                if (polygon.Length > 2)
                {
                    DocumentWorkspace.ResetOutlineWhiteOpacity();
                    this.newSelectionRenderer.ResetOutlineWhiteOpacity();

                    Selection.SetContinuation(polygon, this.combineMode);

                    CombineMode cm;

                    if (SelectionMode == CombineMode.Replace)
                    {
                        cm = CombineMode.Replace;
                    }
                    else
                    {
                        cm = CombineMode.Xor;
                    }

                    this.newSelection.SetContinuation(polygon, cm);

                    Update();
                }
            }
        }

        protected override void OnPulse()
        {
            if (this.tracking)
            {
                DocumentWorkspace.ResetOutlineWhiteOpacity();
                this.newSelectionRenderer.ResetOutlineWhiteOpacity();
            }

            base.OnPulse();
        }

        private enum WhatToDo
        {
            Clear,
            Emit,
            Reset,
        }

        private void Done()
        {
            if (tracking)
            {
                // Truth table for what we should do based on three flags:
                //  append  | moved | tooQuick | result                             | optimized expression to yield true
                // ---------+-------+----------+-----------------------------------------------------------------------
                //     F    |   T   |    T     | clear selection                    | !append && (!moved || tooQuick)
                //     F    |   T   |    F     | emit new selected area             | !append && moved && !tooQuick
                //     F    |   F   |    T     | clear selection                    | !append && (!moved || tooQuick)
                //     F    |   F   |    F     | clear selection                    | !append && (!moved || tooQuick)
                //     T    |   T   |    T     | append to selection                | append && moved
                //     T    |   T   |    F     | append to selection                | append && moved
                //     T    |   F   |    T     | reset selection                    | append && !moved
                //     T    |   F   |    F     | reset selection                    | append && !moved
                //
                // append   --> If the user was holding control, then true. Else false.
                // moved    --> If they never moved the mouse, false. Else true.
                // tooQuick --> If they held the mouse button down for more than 50ms, false. Else true.
                //
                // "Clear selection" means to result in no selected area. If the selection area was previously empty,
                //    then no HistoryMemento is emitted. Otherwise a Deselect HistoryMemento is emitted.
                //
                // "Reset selection" means to reset the selected area to how it was before interaction with the tool,
                //    without a HistoryMemento.

                PointF[] polygon = CreateSelectionPolygon();
                this.hasMoved &= (polygon.Length > 1);

                // They were "too quick" if they weren't doing a selection for more than 50ms
                // This takes care of the case where someone wants to click to deselect, but accidentally moves
                // the mouse. This happens VERY frequently.
                bool tooQuick = Utility.TicksToMs((DateTime.Now - startTime).Ticks) <= 50;

                // If their selection was completedly out of bounds, it will be clipped
                bool clipped = (polygon.Length == 0);

                // What the user drew had no effect on the slection, e.g. subtraction where there was nothing in the first place
                bool noEffect = false;

                WhatToDo whatToDo;

                // If their selection gets completely clipped (i.e. outside the image canvas),
                // then result in a no-op
                if (append)
                {
                    if (!hasMoved || clipped || noEffect)
                    {   
                        whatToDo = WhatToDo.Reset;
                    }
                    else
                    {   
                        whatToDo = WhatToDo.Emit;
                    }
                }
                else
                {
                    if (hasMoved && !tooQuick && !clipped && !noEffect)
                    {   
                        whatToDo = WhatToDo.Emit;
                    }
                    else
                    {   
                        whatToDo = WhatToDo.Clear;
                    }
                }

                switch (whatToDo)
                {
                    case WhatToDo.Clear:
                        if (wasNotEmpty)
                        {
                            // emit a deselect history action
                            undoAction.Name = DeselectFunction.StaticName;
                            undoAction.Image = DeselectFunction.StaticImage;
                            HistoryStack.PushNewMemento(undoAction);
                        }

                        Selection.Reset();
                        break;

                    case WhatToDo.Emit:
                        // emit newly selected area
                        undoAction.Name = this.Name;
                        HistoryStack.PushNewMemento(undoAction);
                        Selection.CommitContinuation();
                        break;

                    case WhatToDo.Reset:
                        // reset selection, no HistoryMemento
                        Selection.ResetContinuation();
                        break;
                }

                DocumentWorkspace.ResetOutlineWhiteOpacity();
                this.newSelectionRenderer.ResetOutlineWhiteOpacity();
                this.newSelection.Reset();
                this.newSelectionRenderer.Visible = false;

                this.tracking = false;

                DocumentWorkspace.EnableSelectionOutline = true;
                DocumentWorkspace.InvalidateSurface(Utility.RoundRectangle(DocumentWorkspace.VisibleDocumentRectangleF));
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            OnMouseMove(e);

            if (moveOriginMode)
            {
                moveOriginMode = false;
            }
            else
            {
                Done();
            }

            base.OnMouseUp(e);

            Cursor = GetCursor();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (tracking)
            {
                Render();
            }

            Cursor = GetCursor();
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            if (tracking)
            {
                Render();
            }

            Cursor = GetCursor();
        }

        protected override void OnClick()
        {
            base.OnClick();
            
            if (!moveOriginMode)
            {
                Done();
            }
        }

        public SelectionTool(
            DocumentWorkspace documentWorkspace,
            ImageResource toolBarImage,
            string name,
            string helpText,
            char hotKey,
            ToolBarConfigItems toolBarConfigItems)
            : base(documentWorkspace,
                   toolBarImage,
                   name,
                   helpText,
                   hotKey,
                   false,
                   toolBarConfigItems | ToolBarConfigItems.SelectionCombineMode)
        {
            this.tracking = false;
        }
    }
}
