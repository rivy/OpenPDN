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
using PaintDotNet.SystemLayer;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace PaintDotNet.Tools
{
    internal sealed class GradientTool
        : Tool
    {
        public static string StaticName
        {
            get
            {
                return PdnResources.GetString("GradientTool.Name");
            }
        }

        public static ImageResource StaticImage
        {
            get
            {
                return PdnResources.GetImageResource("Icons.GradientToolIcon.png");
            }
        }

        private Cursor toolCursor;
        private Cursor toolMouseDownCursor;
        private ImageResource toolIcon;

        private MoveNubRenderer[] moveNubs;
        private MoveNubRenderer startNub;
        private MoveNubRenderer endNub;
        private MoveNubRenderer mouseNub; // which nub the mouse is manipulating, null for neither
        private MouseButtons mouseButton = MouseButtons.None;
        private PointF startPoint;
        private PointF endPoint;

        private string helpTextInitial = PdnResources.GetString("GradientTool.HelpText");
        private string helpTextWhileAdjustingFormat = PdnResources.GetString("GradientTool.HelpText.WhileAdjusting.Format");
        private string helpTextAdjustable = PdnResources.GetString("GradientTool.HelpText.Adjustable");

        private bool shouldMoveBothNubs = false;
        private bool shouldConstrain = false;
        private bool shouldSwapColors = false;
        private bool gradientActive = false; // we are drawing or adjusting a gradient

        private CompoundHistoryMemento historyMemento = null;

        private void ConstrainPoints(PointF a, ref PointF b)
        {
            PointF dir = new PointF(b.X - a.X, b.Y - a.Y);
            double theta = Math.Atan2(dir.Y, dir.X);
            double len = Math.Sqrt(dir.X * dir.X + dir.Y * dir.Y);

            theta = Math.Round(12 * theta / Math.PI) * Math.PI / 12;
            b = new PointF((float)(a.X + len * Math.Cos(theta)), (float)(a.Y + len * Math.Sin(theta)));
        }

        protected override void OnPulse()
        {
            if (this.gradientActive && this.moveNubs != null)
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
                    long tick = (DateTime.Now.Ticks % period) + (i * (period / this.moveNubs.Length)); ;
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

                case Keys.ShiftKey:
                    bool oldShouldConstrain = this.shouldConstrain;
                    this.shouldConstrain = true;

                    if (this.gradientActive &&
                        this.mouseButton != MouseButtons.None && 
                        !oldShouldConstrain)
                    {
                        RenderGradient();
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
                            this.moveNubs[i].Visible = this.gradientActive && !this.moveNubs[i].Visible;
                        }
                    }

                    this.controlKeyDown = false;
                    break;

                case Keys.ShiftKey:
                    this.shouldConstrain = false;

                    if (this.gradientActive && 
                        this.mouseButton != MouseButtons.None)
                    {
                        RenderGradient();
                    }
                    break;
            }

            base.OnKeyUp(e);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (this.gradientActive)
            {
                switch (e.KeyChar)
                {
                    case '\r': // Enter
                        e.Handled = true;
                        CommitGradient();
                        break;

                    case (char)27: // Escape
                        e.Handled = true;
                        CommitGradient();
                        HistoryStack.StepBackward();
                        break;
                }
            }

            base.OnKeyPress(e);
        }


        private sealed class RenderContext
        {
            public Surface surface;
            public Rectangle[] rois;
            public GradientRenderer renderer;

            public void Render(object cpuIndexObj)
            {
                int cpuIndex = (int)cpuIndexObj;
                int start = (this.rois.Length * cpuIndex) / Processor.LogicalCpuCount;
                int end = (this.rois.Length * (cpuIndex + 1)) / Processor.LogicalCpuCount;

                renderer.Render(this.surface, this.rois, start, end - start);
            }
        }

        private void RenderGradient(Surface surface, PdnRegion clipRegion, CompositingMode compositingMode,
            PointF startPointF, ColorBgra startColor, PointF endPointF, ColorBgra endColor)
        {
            GradientRenderer gr = AppEnvironment.GradientInfo.CreateGradientRenderer();

            gr.StartColor = startColor;
            gr.EndColor = endColor;
            gr.StartPoint = startPointF;
            gr.EndPoint = endPointF;
            gr.AlphaBlending = (compositingMode == CompositingMode.SourceOver);
            gr.BeforeRender();

            Rectangle[] oldRois = clipRegion.GetRegionScansReadOnlyInt();
            Rectangle[] newRois;

            if (oldRois.Length == 1)
            {
                newRois = new Rectangle[Processor.LogicalCpuCount];
                Utility.SplitRectangle(oldRois[0], newRois);
            }
            else
            {
                newRois = oldRois;
            }

            RenderContext rc = new RenderContext();
            rc.surface = surface;
            rc.rois = newRois;
            rc.renderer = gr;

            WaitCallback wc = new WaitCallback(rc.Render);

            for (int i = 0; i < Processor.LogicalCpuCount; ++i)
            {
                if (i == Processor.LogicalCpuCount - 1)
                {
                    wc(BoxedConstants.GetInt32(i));
                }
                else
                {
                    PaintDotNet.Threading.ThreadPool.Global.QueueUserWorkItem(wc, BoxedConstants.GetInt32(i));
                }
            }

            PaintDotNet.Threading.ThreadPool.Global.Drain();
        }

        private void RenderGradient()
        {
            ColorBgra startColor = AppEnvironment.PrimaryColor;
            ColorBgra endColor = AppEnvironment.SecondaryColor;

            if (this.shouldSwapColors)
            {
                if (AppEnvironment.GradientInfo.AlphaOnly)
                {
                    // In transparency mode, the color values don't matter. We just need to reverse
                    // and invert the alpha values.
                    byte startAlpha = startColor.A;
                    startColor.A = (byte)(255 - endColor.A);
                    endColor.A = (byte)(255 - startAlpha);
                }
                else
                {
                    Utility.Swap(ref startColor, ref endColor);
                }
            }

            PointF startPointF = this.startPoint;
            PointF endPointF = this.endPoint;

            if (this.shouldConstrain)
            {
                if (this.mouseNub == this.startNub)
                {
                    ConstrainPoints(endPointF, ref startPointF);
                }
                else
                {
                    ConstrainPoints(startPointF, ref endPointF);
                }
            }

            RestoreSavedRegion();

            Surface surface = ((BitmapLayer)DocumentWorkspace.ActiveLayer).Surface;
            PdnRegion clipRegion = DocumentWorkspace.Selection.CreateRegion();

            SaveRegion(clipRegion, clipRegion.GetBoundsInt());

            RenderGradient(surface, clipRegion, AppEnvironment.GetCompositingMode(), startPointF, startColor, endPointF, endColor);

            using (PdnRegion simplified = Utility.SimplifyAndInflateRegion(clipRegion, Utility.DefaultSimplificationFactor, 0))
            {
                DocumentWorkspace.ActiveLayer.Invalidate(simplified);
            }

            clipRegion.Dispose();

            // Set up status bar text
            double angle = -180.0 * Math.Atan2(endPointF.Y - startPointF.Y, endPointF.X - startPointF.X) / Math.PI;
            MeasurementUnit units = AppWorkspace.Units;
            double offsetXPhysical = Document.PixelToPhysicalX(endPointF.X - startPointF.X, units);
            double offsetYPhysical = Document.PixelToPhysicalY(endPointF.Y - startPointF.Y, units);
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
                this.helpTextWhileAdjustingFormat,
                offsetXPhysical.ToString(numberFormat),
                unitsAbbreviation,
                offsetYPhysical.ToString(numberFormat),
                unitsAbbreviation,
                offsetLengthPhysical.ToString("F2"),
                unitsString,
                angle.ToString("F2"));

            SetStatus(this.toolIcon, statusText);

            // Make sure everything is on screen.
            Update();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            PointF mousePt = new PointF(e.X, e.Y);
            MoveNubRenderer mouseCursorNub = PointToNub(mousePt);

            if (this.mouseButton != MouseButtons.None)
            {
                this.shouldMoveBothNubs = !this.shouldMoveBothNubs;
            }
            else
            {
                bool startNewGradient = true;
                this.mouseButton = e.Button;

                if (!this.gradientActive)
                {
                    this.shouldSwapColors = (this.mouseButton == MouseButtons.Right);
                }
                else
                {
                    this.shouldMoveBothNubs = false;

                    // We are already in the process of drawing or adjusting a gradient.
                    // Determine if they clicked to drag one of the nubs for adjusting.

                    if (mouseCursorNub == null)
                    {
                        // No. Commit the old gradient and begin a new one.
                        CommitGradient();
                        startNewGradient = true;
                        this.shouldSwapColors = (this.mouseButton == MouseButtons.Right);
                    }
                    else
                    {
                        // Yes. Continue adjusting the old gradient.
                        Cursor = this.handCursorMouseDown;

                        this.mouseNub = mouseCursorNub;
                        this.mouseNub.Location = mousePt;

                        if (this.mouseNub == this.startNub)
                        {
                            this.startPoint = mousePt;
                        }
                        else
                        {
                            this.endPoint = mousePt;
                        }

                        if (this.mouseButton == MouseButtons.Right)
                        {
                            this.shouldSwapColors = !this.shouldSwapColors;
                        }

                        RenderGradient();
                        startNewGradient = false;
                    }
                }

                if (startNewGradient)
                {
                    // Brand new gradient. Set everything up.
                    this.startPoint = mousePt;
                    this.startNub.Location = mousePt;
                    this.startNub.Visible = true;

                    this.endNub.Location = mousePt;
                    this.endNub.Visible = true;
                    this.endPoint = mousePt;

                    this.mouseNub = mouseCursorNub;

                    Cursor = this.toolMouseDownCursor;

                    this.gradientActive = true;

                    ClearSavedRegion();
                    RenderGradient();

                    this.historyMemento = new CompoundHistoryMemento(StaticName, StaticImage);
                    HistoryStack.PushNewMemento(this.historyMemento); // this makes it so they can push Esc to undo
                }
            }

            base.OnMouseDown(e);
        }

        private MoveNubRenderer PointToNub(PointF mousePtF)
        {
            float startDistance = Utility.Distance(mousePtF, this.startNub.Location);
            float endDistance = Utility.Distance(mousePtF, this.endNub.Location);

            if (this.startNub.Visible &&
                startDistance < endDistance && 
                this.startNub.IsPointTouching(mousePtF, true))
            {
                return this.startNub;
            }
            else if (this.endNub.Visible &&
                     this.endNub.IsPointTouching(mousePtF, true))
            {
                return this.endNub;
            }
            else
            {
                return null;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            PointF mousePtF = new PointF(e.X, e.Y);
            MoveNubRenderer mouseCursorNub = PointToNub(mousePtF);

            if (this.mouseButton == MouseButtons.None)
            {
                // No mouse button dragging is being tracked.
                this.mouseNub = mouseCursorNub;

                if (this.mouseNub == this.startNub || this.mouseNub == this.endNub)
                {
                    Cursor = this.handCursor;
                }
                else
                {
                    Cursor = this.toolCursor;
                }
            }
            else
            {
                if (this.mouseNub == this.startNub)
                {
                    // Dragging the start nub
                    if (this.shouldConstrain && !this.shouldMoveBothNubs)
                    {
                        ConstrainPoints(this.endPoint, ref mousePtF);
                    }

                    this.startNub.Location = mousePtF;

                    SizeF delta = new SizeF(
                        this.startNub.Location.X - this.startPoint.X,
                        this.startNub.Location.Y - this.startPoint.Y);

                    this.startPoint = mousePtF;

                    if (this.shouldMoveBothNubs)
                    {
                        this.endNub.Location += delta;
                        this.endPoint += delta;
                    }
                }
                else if (this.mouseNub == this.endNub)
                {
                    // Dragging the ending nub
                    if (this.shouldConstrain && !this.shouldMoveBothNubs)
                    {
                        ConstrainPoints(this.startPoint, ref mousePtF);
                    }

                    this.endNub.Location = mousePtF;

                    SizeF delta = new SizeF(
                        this.endNub.Location.X - this.endPoint.X,
                        this.endNub.Location.Y - this.endPoint.Y);

                    this.endPoint = mousePtF;

                    if (this.shouldMoveBothNubs)
                    {
                        this.startNub.Location += delta;
                        this.startPoint += delta;
                    }
                }
                else
                {
                    // Initial drawing
                    if (this.shouldMoveBothNubs)
                    {
                        SizeF delta = new SizeF(
                            this.endNub.Location.X - mousePtF.X,
                            this.endNub.Location.Y - mousePtF.Y);

                        this.startNub.Location -= delta;
                        this.startPoint -= delta;
                    }
                    else if (this.shouldConstrain)
                    {
                        ConstrainPoints(this.startPoint, ref mousePtF);
                    }

                    this.endNub.Location = mousePtF;
                    this.endPoint = mousePtF;
                }

                RenderGradient();
            }

            base.OnMouseMove(e);
        }

        private void CommitGradient()
        {
            if (!this.gradientActive)
            {
                throw new InvalidOperationException("CommitGradient() called when a gradient was not active");
            }

            RenderGradient();

            using (PdnRegion clipRegion = DocumentWorkspace.Selection.CreateRegion())
            {
                BitmapHistoryMemento bhm = new BitmapHistoryMemento(
                    StaticName,
                    StaticImage,
                    DocumentWorkspace,
                    DocumentWorkspace.ActiveLayerIndex,
                    clipRegion,
                    this.ScratchSurface);

                this.historyMemento.PushNewAction(bhm);

                // We assume this.historyMemento has already been pushed on to the HistoryStack

                this.historyMemento = null;
            }

            this.startNub.Visible = false;
            this.endNub.Visible = false;

            ClearSavedRegion();
            ClearSavedMemory();
            this.gradientActive = false;

            SetStatus(this.toolIcon, this.helpTextInitial);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            PointF mousePt = new PointF(e.X, e.Y);

            if (!this.gradientActive)
            {
                // do nothing
            }
            else if (e.Button != this.mouseButton)
            {
                this.shouldMoveBothNubs = !this.shouldMoveBothNubs;
            }
            else
            {
                if (this.mouseNub == this.startNub)
                {
                    // We were adjusting the start nub.
                    if (this.shouldConstrain)
                    {
                        ConstrainPoints(this.endPoint, ref mousePt);
                    }

                    this.startNub.Location = mousePt;
                    this.startPoint = mousePt;
                }
                else if (this.mouseNub == this.endNub)
                {
                    // We were adjusting the ending nub.
                    if (this.shouldConstrain)
                    {
                        ConstrainPoints(this.startPoint, ref mousePt);
                    }

                    this.endNub.Location = mousePt;
                    this.endPoint = mousePt;
                }
                else
                {
                    // We were drawing a brand new gradient.
                    if (this.shouldConstrain)
                    {
                        ConstrainPoints(this.startPoint, ref mousePt);
                    }

                    this.endNub.Location = mousePt;
                    this.endPoint = mousePt;
                }

                // In any event, make sure the nubs are visible and other state adjusted accordingly.
                this.startNub.Visible = true;
                this.endNub.Visible = true;
                this.mouseButton = MouseButtons.None;
                this.gradientActive = true;
                SetStatus(this.toolIcon, this.helpTextAdjustable);
            }

            base.OnMouseUp(e);
        }

        private void RenderBecauseOfEvent(object sender, EventArgs e)
        {
            if (this.gradientActive)
            {
                RenderGradient();
            }
        }

        protected override void OnActivate()
        {
            this.toolCursor = new Cursor(PdnResources.GetResourceStream("Cursors.GenericToolCursor.cur"));
            this.toolMouseDownCursor = new Cursor(PdnResources.GetResourceStream("Cursors.GenericToolCursorMouseDown.cur"));
            this.Cursor = this.toolCursor;
            this.toolIcon = this.Image;

            this.startNub = new MoveNubRenderer(RendererList);
            this.startNub.Visible = false;
            this.startNub.Shape = MoveNubShape.Circle;
            RendererList.Add(this.startNub, false);

            this.endNub = new MoveNubRenderer(RendererList);
            this.endNub.Visible = false;
            this.endNub.Shape = MoveNubShape.Circle;
            RendererList.Add(this.endNub, false);

            this.moveNubs = 
                new MoveNubRenderer[] 
                { 
                    this.startNub, 
                    this.endNub 
                };

            AppEnvironment.PrimaryColorChanged += new EventHandler(RenderBecauseOfEvent);
            AppEnvironment.SecondaryColorChanged += new EventHandler(RenderBecauseOfEvent);
            AppEnvironment.GradientInfoChanged += new EventHandler(RenderBecauseOfEvent);
            AppEnvironment.AlphaBlendingChanged += new EventHandler(RenderBecauseOfEvent);
            AppWorkspace.UnitsChanged += new EventHandler(RenderBecauseOfEvent);

            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            AppEnvironment.PrimaryColorChanged -= new EventHandler(RenderBecauseOfEvent);
            AppEnvironment.SecondaryColorChanged -= new EventHandler(RenderBecauseOfEvent);
            AppEnvironment.GradientInfoChanged -= new EventHandler(RenderBecauseOfEvent);
            AppEnvironment.AlphaBlendingChanged -= new EventHandler(RenderBecauseOfEvent);
            AppWorkspace.UnitsChanged -= new EventHandler(RenderBecauseOfEvent);

            if (this.gradientActive)
            {
                CommitGradient();
                this.mouseButton = MouseButtons.None;
            }

            if (this.startNub != null)
            {
                RendererList.Remove(this.startNub);
                this.startNub.Dispose();
                this.startNub = null;
            }

            if (this.endNub != null)
            {
                RendererList.Remove(this.endNub);
                this.endNub.Dispose();
                this.endNub = null;
            }

            this.moveNubs = null;

            if (this.toolCursor != null)
            {
                this.toolCursor.Dispose();
                this.toolCursor = null;
            }

            if (this.toolMouseDownCursor != null)
            {
                this.toolMouseDownCursor.Dispose();
                this.toolMouseDownCursor = null;
            }

            base.OnDeactivate();
        }
    
        public GradientTool(DocumentWorkspace documentWorkspace)
            : base(documentWorkspace,
                   StaticImage,
                   StaticName,
                   PdnResources.GetString("GradientTool.HelpText"),
                   'g',
                   false,
                   ToolBarConfigItems.Gradient | ToolBarConfigItems.AlphaBlending)
        {
        }
    }
}
