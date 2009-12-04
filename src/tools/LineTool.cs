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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;

namespace PaintDotNet.Tools
{
    internal class LineTool
        : ShapeTool 
    {
        private const int controlPointCount = 4;
        private const float flattenConstant = 0.1f;
        private Cursor lineToolCursor;
        private Cursor lineToolMouseDownCursor;
        private string statusTextFormat = PdnResources.GetString("LineTool.StatusText.Format");
        private ImageResource lineToolIcon;
        private MoveNubRenderer[] moveNubs;
        private bool inCurveMode = false;
        private int draggingNubIndex = -1;
        private CurveType curveType;

        private enum CurveType
        {
            NotDecided,
            Bezier,
            Spline
        }

        private PointF[] LineToSpline(PointF a, PointF b, int points)
        {
            PointF[] spline = new PointF[points];

            for (int i = 0; i < spline.Length; ++i)
            {
                float frac = (float)i / (float)(spline.Length - 1);
                PointF mid = Utility.Lerp(a, b, frac);
                spline[i] = mid;
            }

            return spline;
        }

        protected override List<PointF> TrimShapePath(List<PointF> points)
        {
            if (this.inCurveMode)
            {
                return points;
            }
            else
            {
                List<PointF> array = new List<PointF>();

                if (points.Count > 0)
                {
                    array.Add(points[0]);

                    if (points.Count > 1)
                    {
                        array.Add(points[points.Count - 1]);
                    }
                }

                return array;
            }
        }

        private void ConstrainPoints(ref PointF a, ref PointF b)
        {
            PointF dir = new PointF(b.X - a.X, b.Y - a.Y);
            double theta = Math.Atan2(dir.Y, dir.X);
            double len = Math.Sqrt(dir.X * dir.X + dir.Y * dir.Y);

            theta = Math.Round(12 * theta / Math.PI) * Math.PI / 12;
            b = new PointF((float)(a.X + len * Math.Cos(theta)), (float)(a.Y + len * Math.Sin(theta)));
        }

        protected override PdnGraphicsPath CreateShapePath(PointF[] points)
        {
            if (points.Length >= 4)
            {
                PdnGraphicsPath path = new PdnGraphicsPath();

                switch (this.curveType)
                {
                    default:
                    case CurveType.Spline:
                        path.AddCurve(points);
                        break;

                    case CurveType.Bezier:
                        path.AddBezier(points[0], points[1], points[2], points[3]);
                        break;
                }

                path.Flatten(Utility.IdentityMatrix, flattenConstant);
                return path;
            }
            else //if (points.Length <= 2)
            {
                PointF a = points[0];
                PointF b = points[points.Length - 1];
            
                if (0 != (ModifierKeys & Keys.Shift) && a != b)
                {
                    ConstrainPoints(ref a, ref b);
                }

                double angle = -180.0 * Math.Atan2(b.Y - a.Y, b.X - a.X) / Math.PI;
                MeasurementUnit units = AppWorkspace.Units;
                double offsetXPhysical = Document.PixelToPhysicalX(b.X - a.X, units);
                double offsetYPhysical = Document.PixelToPhysicalY(b.Y - a.Y, units);
                double offsetLengthPhysical = Math.Sqrt(offsetXPhysical * offsetXPhysical + offsetYPhysical * offsetYPhysical);

                string numberFormat;
                string unitsAbbreviation;

                if (units != MeasurementUnit.Pixel)
                {
                    string unitsAbbreviationName = "MeasurementUnit." + units.ToString() + ".Abbreviation";
                    unitsAbbreviation = PdnResources.GetString(unitsAbbreviationName);
                    numberFormat = "F2";
                }
                else
                {
                    unitsAbbreviation = string.Empty;
                    numberFormat = "F0";
                }

                string unitsString = PdnResources.GetString("MeasurementUnit." + units.ToString() + ".Plural");

                string statusText = string.Format(
                    this.statusTextFormat,
                    offsetXPhysical.ToString(numberFormat),
                    unitsAbbreviation,
                    offsetYPhysical.ToString(numberFormat),
                    unitsAbbreviation,
                    offsetLengthPhysical.ToString("F2"),
                    unitsString,
                    angle.ToString("F2"));

                SetStatus(this.lineToolIcon, statusText);

                if (a == b)
                {
                    return null;
                }
                else
                {
                    PdnGraphicsPath path = new PdnGraphicsPath();
                    PointF[] spline = LineToSpline(a, b, controlPointCount);
                    path.AddCurve(spline);
                    path.Flatten(Utility.IdentityMatrix, flattenConstant);
                    return path;
                }
            }
        }

        public override PixelOffsetMode GetPixelOffsetMode()
        {
            return PixelOffsetMode.None;
        }

        protected override void OnPulse()
        {
            if (this.moveNubs != null)
            {
                for (int i = 0; i < this.moveNubs.Length; ++i)
                {
                    if (!this.moveNubs[i].Visible)
                    {
                        continue;
                    }

                    // Oscillate between 25% and 100% alpha over a period of 2 seconds
                    // Alpha value of 100% is sustained for a large duration of this period
                    const int period = 10000 * 2000; // 10000 ticks per ms, 2000ms per second
                    long tick = (DateTime.Now.Ticks % period) + (i * (period / this.moveNubs.Length));;
                    double sin = Math.Sin(((double)tick / (double)period) * (2.0 * Math.PI));
                    // sin is [-1, +1]

                    sin = Math.Min(0.5, sin);
                    // sin is [-1, +0.5]

                    sin += 1.0;
                    // sin is [0, 1.5]

                    sin /= 2.0;
                    // sin is [0, 0.75]

                    sin += 0.25;
                    // sin is [0.25, 1]

                    int newAlpha = (int)(sin * 255.0);
                    int clampedAlpha = Utility.Clamp(newAlpha, 0, 255);
                    this.moveNubs[i].Alpha = clampedAlpha;
                }
            }
            
            base.OnPulse();
        }

        private const int toggleStartCapOrdinal = 0;
        private const int toggleDashOrdinal = 1;
        private const int toggleEndCapOrdinal = 2;

        protected override bool OnWildShortcutKey(int ordinal)
        {
            switch (ordinal)
            {
                case toggleStartCapOrdinal:
                    AppWorkspace.Widgets.ToolConfigStrip.CyclePenStartCap();
                    return true;

                case toggleDashOrdinal:
                    AppWorkspace.Widgets.ToolConfigStrip.CyclePenDashStyle();
                    return true;

                case toggleEndCapOrdinal:
                    AppWorkspace.Widgets.ToolConfigStrip.CyclePenEndCap();
                    return true;
            }

            return base.OnWildShortcutKey(ordinal);
        }

        private bool controlKeyDown = false;
        private DateTime controlKeyDownTime = DateTime.MinValue;
        private readonly TimeSpan controlKeyDownThreshold = new TimeSpan(0, 0, 0, 0, 400);

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.ControlKey:
                    if (!this.controlKeyDown)
                    {
                        this.controlKeyDown = true;
                        this.controlKeyDownTime = DateTime.Now;
                    }

                    break;
            }

            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.ControlKey:
                    TimeSpan heldDuration = (DateTime.Now - this.controlKeyDownTime);

                    // If the user taps Ctrl, then we should toggle the visiblity of the moveNubs
                    if (heldDuration < this.controlKeyDownThreshold)
                    {
                        for (int i = 0; i < this.moveNubs.Length; ++i)
                        {
                            this.moveNubs[i].Visible = this.inCurveMode && !this.moveNubs[i].Visible;
                        }
                    }

                    this.controlKeyDown = false;
                    break;
            }

            base.OnKeyUp(e); base.OnKeyUp(e);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (this.inCurveMode)
            {
                switch (e.KeyChar)
                {
                    case '\r': // Enter
                        e.Handled = true;
                        CommitShape();
                        break;

                    case (char)27: // Escape
                        // Only recognize if the user is not pressing Ctrl.
                        // Reason for this is that Ctrl+[ ends up being sent
                        // to us as (char)27 as well, but the user probably
                        // wants to use that for the decrease brush size
                        // shortcut, not cancel :)
                        if ((ModifierKeys & Keys.Control) == 0)
                        {
                            e.Handled = true;
                            HistoryStack.StepBackward();
                        }
                        break;
                }
            }

            base.OnKeyPress(e);
        }

        protected override void OnShapeCommitting()
        {
            for (int i = 0; i < this.moveNubs.Length; ++i)
            {
                this.moveNubs[i].Visible = false;
            }
            
            this.inCurveMode = false;
            this.curveType = CurveType.NotDecided;
            this.Cursor = this.lineToolCursor;
            this.draggingNubIndex = -1;

            DocumentWorkspace.UpdateStatusBarToToolHelpText();
        }

        protected override bool OnShapeEnd()
        {
            // init move nubs
            List<PointF> points = GetTrimmedShapePath();

            if (points.Count < 2)
            {
                return true;
            }
            else
            {
                PointF a = (PointF)points[0];
                PointF b = (PointF)points[points.Count - 1];

                if (0 != (ModifierKeys & Keys.Shift) && a != b)
                {
                    ConstrainPoints(ref a, ref b);
                }

                PointF[] spline = LineToSpline(a, b, controlPointCount);
                List<PointF> newPoints = new List<PointF>();

                this.inCurveMode = true;
                for (int i = 0; i < this.moveNubs.Length; ++i)
                {
                    this.moveNubs[i].Location = spline[i];
                    this.moveNubs[i].Visible = true;
                    newPoints.Add(spline[i]);
                }

                string helpText2 = PdnResources.GetString("LineTool.PreCurveHelpText");
                this.SetStatus(null, helpText2);
                SetShapePath(newPoints);
                return false;
            }
        }

        protected override void OnStylusDown(StylusEventArgs e)
        {
            bool callBase = false;

            if (!this.inCurveMode)
            {
                callBase = true;
            }
            else
            {
                PointF mousePtF = new PointF(e.Fx, e.Fy);
                Point mousePt = Point.Truncate(mousePtF);
                float minDistance = float.MaxValue;

                for (int i = 0; i < this.moveNubs.Length; ++i)
                {
                    if (this.moveNubs[i].IsPointTouching(mousePt, true))
                    {
                        float distance = Utility.Distance(mousePtF, this.moveNubs[i].Location);

                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            this.draggingNubIndex = i;
                        }
                    }
                }

                if (this.draggingNubIndex == -1)
                {
                    callBase = true;
                }
                else
                {
                    this.Cursor = this.handCursorMouseDown;

                    if (this.curveType == CurveType.NotDecided)
                    {
                        if (e.Button == MouseButtons.Right)
                        {
                            this.curveType = CurveType.Bezier;
                        }
                        else
                        {
                            this.curveType = CurveType.Spline;
                        }
                    }

                    for (int i = 0; i < this.moveNubs.Length; ++i)
                    {
                        this.moveNubs[i].Visible = false;
                    }

                    string helpText2 = PdnResources.GetString("LineTool.CurvingHelpText");
                    SetStatus(null, helpText2);
                    OnStylusMove(e);
                }
            }

            if (callBase)
            {
                base.OnStylusDown(e);
                Cursor = this.lineToolMouseDownCursor;
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (!this.inCurveMode)
            {
                base.OnMouseDown(e);
            }
        }

        protected override void OnStylusUp(StylusEventArgs e)
        {
            if (!this.inCurveMode)
            {
                base.OnStylusUp(e);
            }
            else
            {
                if (this.draggingNubIndex != -1)
                {
                    OnStylusMove(e);
                    this.draggingNubIndex = -1;
                    this.Cursor = this.lineToolCursor;

                    for (int i = 0; i < this.moveNubs.Length; ++i)
                    {
                        this.moveNubs[i].Visible = true;
                    }
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (!this.inCurveMode)
            {
                base.OnMouseUp(e);
            }
        }

        protected override void OnStylusMove(StylusEventArgs e)
        {
            if (!this.inCurveMode)
            {
                base.OnStylusMove(e);
            }
            else if (this.draggingNubIndex != -1)
            {
                PointF mousePt = new PointF(e.Fx, e.Fy);
                this.moveNubs[this.draggingNubIndex].Location = mousePt;
                List<PointF> points = GetTrimmedShapePath();
                points[this.draggingNubIndex] = mousePt;
                SetShapePath(points);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (this.draggingNubIndex != -1)
            {
                RenderShape();
                Update();
            }
            else
            {
                Point mousePt = new Point(e.X, e.Y);
                bool hot = false;

                for (int i = 0; i < this.moveNubs.Length; ++i)
                {
                    if (this.moveNubs[i].Visible && this.moveNubs[i].IsPointTouching(Point.Truncate(mousePt), true))
                    {
                        this.Cursor = this.handCursor;
                        hot = true;
                        break;
                    }
                }

                if (!hot)
                {
                    if (IsMouseDown)
                    {
                        Cursor = this.lineToolMouseDownCursor;
                    }
                    else
                    {
                        Cursor = this.lineToolCursor;
                    }
                }
            }

            base.OnMouseMove(e);
        }

        protected override void OnActivate()
        {
            this.lineToolCursor = new Cursor(PdnResources.GetResourceStream("Cursors.LineToolCursor.cur"));
            this.lineToolMouseDownCursor = new Cursor(PdnResources.GetResourceStream("Cursors.GenericToolCursorMouseDown.cur"));
            this.Cursor = this.lineToolCursor;
            this.lineToolIcon = this.Image;

            this.moveNubs = new MoveNubRenderer[controlPointCount];
            for (int i = 0; i < this.moveNubs.Length; ++i)
            {
                this.moveNubs[i] = new MoveNubRenderer(this.RendererList);
                this.moveNubs[i].Visible = false;
                this.RendererList.Add(this.moveNubs[i], false);
            }

            AppEnvironment.PrimaryColorChanged += new EventHandler(RenderShapeBecauseOfEvent);
            AppEnvironment.SecondaryColorChanged += new EventHandler(RenderShapeBecauseOfEvent);
            AppEnvironment.AntiAliasingChanged += new EventHandler(RenderShapeBecauseOfEvent);
            AppEnvironment.AlphaBlendingChanged += new EventHandler(RenderShapeBecauseOfEvent);
            AppEnvironment.BrushInfoChanged += new EventHandler(RenderShapeBecauseOfEvent);
            AppEnvironment.PenInfoChanged += new EventHandler(RenderShapeBecauseOfEvent);
            AppWorkspace.UnitsChanged += new EventHandler(RenderShapeBecauseOfEvent);

            base.OnActivate();
        }

        private void RenderShapeBecauseOfEvent(object sender, EventArgs e)
        {
            if (this.inCurveMode)
            {
                RenderShape();
            }
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate();

            AppEnvironment.PrimaryColorChanged -= new EventHandler(RenderShapeBecauseOfEvent);
            AppEnvironment.SecondaryColorChanged -= new EventHandler(RenderShapeBecauseOfEvent);
            AppEnvironment.AntiAliasingChanged -= new EventHandler(RenderShapeBecauseOfEvent);
            AppEnvironment.AlphaBlendingChanged -= new EventHandler(RenderShapeBecauseOfEvent);
            AppEnvironment.BrushInfoChanged -= new EventHandler(RenderShapeBecauseOfEvent);
            AppEnvironment.PenInfoChanged -= new EventHandler(RenderShapeBecauseOfEvent);
            AppWorkspace.UnitsChanged -= new EventHandler(RenderShapeBecauseOfEvent);

            for (int i = 0; i < this.moveNubs.Length; ++i)
            {
                this.RendererList.Remove(this.moveNubs[i]);
                this.moveNubs[i].Dispose();
                this.moveNubs[i] = null;
            }

            this.moveNubs = null;

            if (this.lineToolCursor != null)
            {
                this.lineToolCursor.Dispose();
                this.lineToolCursor = null;
            }

            if (this.lineToolMouseDownCursor != null)
            {
                this.lineToolMouseDownCursor.Dispose();
                this.lineToolMouseDownCursor = null;
            }
        }

        public LineTool(DocumentWorkspace documentWorkspace)
            : base(documentWorkspace,
                   PdnResources.GetImageResource("Icons.LineToolIcon.png"),
                   PdnResources.GetString("LineTool.Name"),
                   PdnResources.GetString("LineTool.HelpText"),
                   ToolBarConfigItems.None | ToolBarConfigItems.PenCaps, 
                   ToolBarConfigItems.ShapeType)
        {
            this.ForceShapeDrawType = true;
            this.ForcedShapeDrawType = ShapeDrawType.Outline;
            this.UseDashStyle = true;
        }
    }
}
