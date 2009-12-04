/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

// Leave uncommented to always use bilinear rendering. Otherwise nearest neighbor
// is used while interacting with the selection via the mouse, for better performance.
//#define ALWAYSHIGHQUALITY

using PaintDotNet.Actions;
using PaintDotNet.HistoryMementos;
using PaintDotNet.Threading;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.Serialization;
using System.Windows.Forms;

namespace PaintDotNet.Tools
{
    internal class MoveTool
        : MoveToolBase
    {
        public static string StaticName
        {
            get
            {
                return PdnResources.GetString("MoveTool.Name");
            }
        }

        // if this equals false, then Render() will always use NearestNeighbor, regardless of AppEnvironment.ResamplingAlgorithm
        private bool fullQuality = false;

        private BitmapLayer activeLayer;
        private RenderArgs renderArgs;
        private bool didPaste = false;

        private MoveToolContext ourContext
        {
            get
            {
                return (MoveToolContext)this.context;
            }
        }

        [Serializable]
        private sealed class MoveToolContext
            : MoveToolBase.Context
        {
            [NonSerialized]
            private MaskedSurface liftedPixels;

            [NonSerialized]
            public PersistedObject<MaskedSurface> poLiftedPixels;

            public Guid poLiftedPixelsGuid;

            public MaskedSurface LiftedPixels
            {
                get
                {
                    if (this.liftedPixels == null)
                    {
                        if (this.poLiftedPixels != null)
                        {
                            this.liftedPixels = (MaskedSurface)poLiftedPixels.Object;
                        }
                    }

                    return this.liftedPixels;
                }

                set
                {
                    if (value == null)
                    {
                        this.poLiftedPixels = null;
                        this.liftedPixels = null;
                    }
                    else
                    {
                        this.poLiftedPixels = new PersistedObject<MaskedSurface>(value, true);
                        this.poLiftedPixelsGuid = PersistedObjectLocker.Add(this.poLiftedPixels);
                        this.liftedPixels = null;
                    }
                }
            }

            public override void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                base.GetObjectData(info, context);
                info.AddValue("poLiftedPixelsGuid", this.poLiftedPixelsGuid);
            }

            public MoveToolContext(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {
                this.poLiftedPixelsGuid = (Guid)info.GetValue("poLiftedPixelsGuid", typeof(Guid));
                this.poLiftedPixels = PersistedObjectLocker.Get<MaskedSurface>(this.poLiftedPixelsGuid);
            }

            public MoveToolContext(MoveToolContext cloneMe)
                : base(cloneMe)
            {
                this.poLiftedPixelsGuid = cloneMe.poLiftedPixelsGuid;
                this.poLiftedPixels = cloneMe.poLiftedPixels; // do not clone
                this.liftedPixels = cloneMe.liftedPixels; // do not clone
            }

            public MoveToolContext()
            {
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                }

                base.Dispose(disposing);
            }

            public override object Clone()
            {
                return new MoveToolContext(this);
            }
        }

        private class ContextHistoryMemento
            : ToolHistoryMemento
        {
            private int layerIndex;
            private object liftedPixelsRef; // prevent this from being GC'd
            
            [Serializable]
            private class OurContextHistoryMementoData
                : HistoryMementoData
            {
                public MoveToolContext context;

                public OurContextHistoryMementoData(Context context)
                {
                    this.context = (MoveToolContext)context.Clone();
                }
            }

            protected override HistoryMemento OnToolUndo()
            {
                MoveTool moveTool = DocumentWorkspace.Tool as MoveTool;

                if (moveTool == null)
                {
                    throw new InvalidOperationException("Current Tool is not the MoveTool");
                }

                ContextHistoryMemento cha = new ContextHistoryMemento(DocumentWorkspace, moveTool.ourContext, this.Name, this.Image);
                OurContextHistoryMementoData ohad = (OurContextHistoryMementoData)this.Data;
                Context newContext = ohad.context;

                if (moveTool.ActiveLayerIndex != this.layerIndex)
                {
                    bool oldDOLC = moveTool.deactivateOnLayerChange;
                    moveTool.deactivateOnLayerChange = false;
                    moveTool.ActiveLayerIndex = this.layerIndex;
                    moveTool.deactivateOnLayerChange = oldDOLC;
                    moveTool.activeLayer = (BitmapLayer)moveTool.ActiveLayer;
                    moveTool.renderArgs = new RenderArgs(moveTool.activeLayer.Surface);
                    moveTool.ClearSavedMemory();
                }

                moveTool.context.Dispose();
                moveTool.context = newContext;

                moveTool.DestroyNubs();

                if (moveTool.context.lifted)
                {
                    moveTool.PositionNubs(moveTool.context.currentMode);
                }

                return cha;
            }

            public ContextHistoryMemento(DocumentWorkspace documentWorkspace, MoveToolContext context, string name, ImageResource image)
                : base(documentWorkspace, name, image)
            {
                this.Data = new OurContextHistoryMementoData(context);
                this.layerIndex = this.DocumentWorkspace.ActiveLayerIndex;
                this.liftedPixelsRef = context.poLiftedPixels;
            }
        }

        protected override void OnActivate()
        {
            AppEnvironment.ResamplingAlgorithmChanged += AppEnvironment_ResamplingAlgorithmChanged;

            this.moveToolCursor = new Cursor(PdnResources.GetResourceStream("Cursors.MoveToolCursor.cur"));
            this.Cursor = this.moveToolCursor;

            this.context.lifted = false;
            this.ourContext.LiftedPixels = null;
            this.context.offset = new Point(0, 0);
            this.context.liftedBounds = Selection.GetBoundsF();
            this.activeLayer = (BitmapLayer)ActiveLayer;

            if (this.renderArgs != null)
            {
                this.renderArgs.Dispose();
                this.renderArgs = null;
            }

            if (this.activeLayer == null)
            {
                this.renderArgs = null;
            }
            else
            {
                this.renderArgs = new RenderArgs(this.activeLayer.Surface);
            }

            this.tracking = false;
            PositionNubs(this.context.currentMode);

#if ALWAYSHIGHQUALITY
            this.fullQuality = true;
#endif

            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            AppEnvironment.ResamplingAlgorithmChanged -= AppEnvironment_ResamplingAlgorithmChanged;

            if (this.moveToolCursor != null)
            {
                this.moveToolCursor.Dispose();
                this.moveToolCursor = null;
            }

            if (context.lifted)
            {   
                Drop();
            }

            this.activeLayer = null;

            if (this.renderArgs != null)
            {
                this.renderArgs.Dispose();
                this.renderArgs = null;
            }

            this.tracking = false;
            DestroyNubs();
            base.OnDeactivate();
        }

        private void AppEnvironment_ResamplingAlgorithmChanged(object sender, EventArgs e)
        {
            if (this.ourContext.LiftedPixels != null)
            {
                bool oldHQ = this.fullQuality;
                this.fullQuality = true;
                PreRender();
                Render(this.context.offset, true);
                Update();
                this.fullQuality = oldHQ;
            }
        }

        protected override void Drop()
        {
            RestoreSavedRegion();

            PdnRegion regionCopy = Selection.CreateRegion();

            using (PdnRegion simplifiedRegion = Utility.SimplifyAndInflateRegion(regionCopy, 
                       Utility.DefaultSimplificationFactor, 2))
            {
                HistoryMemento bitmapAction2 = new BitmapHistoryMemento(Name, Image, DocumentWorkspace, 
                    ActiveLayerIndex, simplifiedRegion);

                bool oldHQ = this.fullQuality;
                this.fullQuality = true;
                Render(this.context.offset, true);
                this.fullQuality = oldHQ;
                this.currentHistoryMementos.Add(bitmapAction2);

                activeLayer.Invalidate(simplifiedRegion);
                Update();
            }

            regionCopy.Dispose();
            regionCopy = null;

            ContextHistoryMemento cha = new ContextHistoryMemento(this.DocumentWorkspace, this.ourContext, this.Name, this.Image);
            this.currentHistoryMementos.Add(cha);

            string name;
            ImageResource image;

            if (didPaste)
            {
                name = EnumLocalizer.EnumValueToLocalizedName(typeof(CommonAction), CommonAction.Paste);
                image = PdnResources.GetImageResource("Icons.MenuEditPasteIcon.png");
            }
            else
            {
                name = this.Name;
                image = this.Image;
            }

            didPaste = false;

            SelectionHistoryMemento sha = new SelectionHistoryMemento(this.Name, this.Image, this.DocumentWorkspace);
            this.currentHistoryMementos.Add(sha);

            this.context.Dispose();
            this.context = new MoveToolContext();

            this.FlushHistoryMementos(PdnResources.GetString("MoveTool.HistoryMemento.DropPixels"));
        }

        protected override void OnSelectionChanging()
        {
            base.OnSelectionChanging();

            if (!dontDrop)
            {
                if (context.lifted)
                {
                    Drop();
                }

                if (tracking)
                {
                    tracking = false;
                }
            }
        }

        protected override void OnSelectionChanged()
        {
            if (!context.lifted)
            {
                DestroyNubs();
                PositionNubs(this.context.currentMode);
            }

            base.OnSelectionChanged();
        }

        /// <summary>
        /// Provided as a special entry point so that Paste can work well.
        /// </summary>
        /// <param name="surface">What you want to paste.</param>
        /// <param name="offset">Where you want to paste it.</param>
        public void PasteMouseDown(SurfaceForClipboard sfc, Point offset)
        {
            if (this.context.lifted)
            {
                Drop();
            }

            MaskedSurface pixels = sfc.MaskedSurface;
            PdnGraphicsPath pastePath = pixels.CreatePath();

            PdnRegion pasteRegion = new PdnRegion(pastePath);

            PdnRegion simplifiedPasteRegion = Utility.SimplifyAndInflateRegion(pasteRegion);

            HistoryMemento bitmapAction = new BitmapHistoryMemento(Name, Image, 
                DocumentWorkspace, ActiveLayerIndex, simplifiedPasteRegion); // SLOW (110ms)

            this.currentHistoryMementos.Add(bitmapAction);

            PushContextHistoryMemento();

            this.context.seriesGuid = Guid.NewGuid();
            this.context.currentMode = Mode.Translate;
            this.context.startEdge = Edge.None;
            this.context.startAngle = 0.0f;

            this.ourContext.LiftedPixels = pixels;
            this.context.lifted = true;
            this.context.liftTransform = new Matrix();
            this.context.liftTransform.Reset();
            this.context.deltaTransform = new Matrix();
            this.context.deltaTransform.Reset();
            this.context.offset = new Point(0, 0);

            bool oldDD = this.dontDrop;
            this.dontDrop = true;

            SelectionHistoryMemento sha = new SelectionHistoryMemento(null, null, DocumentWorkspace);
            this.currentHistoryMementos.Add(sha);

            Selection.PerformChanging();
            Selection.Reset();
            Selection.SetContinuation(pastePath, CombineMode.Replace, true);
            pastePath = null;
            Selection.CommitContinuation();
            Selection.PerformChanged();

            PushContextHistoryMemento();

            this.context.liftedBounds = Selection.GetBoundsF(false);
            this.context.startBounds = this.context.liftedBounds;
            this.context.baseTransform = new Matrix();
            this.context.baseTransform.Reset();
            this.tracking = true;

            this.dontDrop = oldDD;
            this.didPaste = true;

            this.tracking = true;

            DestroyNubs();
            PositionNubs(this.context.currentMode);

            // we use the value 70,000 to simulate mouse input because that's guaranteed to be out of bounds of where
            // the mouse can actually be -- PDN is limited to 65536 x 65536 images by design
            MouseEventArgs mea1 = new MouseEventArgs(MouseButtons.Left, 0, 70000, 70000, 0);
            MouseEventArgs mea2 = new MouseEventArgs(MouseButtons.Left, 0, 70000 + offset.X, 70000 + offset.Y, 0);
            this.context.startMouseXY = new Point(70000, 70000);

            OnMouseDown(mea1);
            OnMouseMove(mea2); // SLOW (200ms)
            OnMouseUp(mea2);
        }

        protected override void OnLift(MouseEventArgs e)
        {
            PdnGraphicsPath liftPath = Selection.CreatePath();
            PdnRegion liftRegion = Selection.CreateRegion();

            this.ourContext.LiftedPixels = new MaskedSurface(activeLayer.Surface, liftPath);

            HistoryMemento bitmapAction = new BitmapHistoryMemento(
                Name, 
                Image, 
                DocumentWorkspace, 
                ActiveLayerIndex, 
                this.ourContext.poLiftedPixelsGuid);

            this.currentHistoryMementos.Add(bitmapAction);
            
            // If the user is holding down the control key, we want to *copy* the pixels
            // and not "lift and erase"
            if ((ModifierKeys & Keys.Control) == Keys.None)
            {
                ColorBgra fill = AppEnvironment.SecondaryColor;
                fill.A = 0;
                UnaryPixelOp op = new UnaryPixelOps.Constant(fill);
                op.Apply(this.renderArgs.Surface, liftRegion);
            }

            liftRegion.Dispose();
            liftRegion = null;

            liftPath.Dispose();
            liftPath = null;
        }

        protected override void PushContextHistoryMemento()
        {
            ContextHistoryMemento cha = new ContextHistoryMemento(this.DocumentWorkspace, this.ourContext, null, null);
            this.currentHistoryMementos.Add(cha);
        }

        protected override void Render(Point newOffset, bool useNewOffset)
        {
            Render(newOffset, useNewOffset, true);
        }

        protected void Render(Point newOffset, bool useNewOffset, bool saveRegion)
        {
            Rectangle saveBounds = Selection.GetBounds();
            PdnRegion selectedRegion = Selection.CreateRegion();
            PdnRegion simplifiedRegion = Utility.SimplifyAndInflateRegion(selectedRegion);

            if (saveRegion)
            {
                SaveRegion(simplifiedRegion, saveBounds);
            }

            WaitCursorChanger wcc = null;

            if (this.fullQuality && AppEnvironment.ResamplingAlgorithm == ResamplingAlgorithm.Bilinear)
            {
                wcc = new WaitCursorChanger(DocumentWorkspace);
            }

            this.ourContext.LiftedPixels.Draw(
                this.renderArgs.Surface, 
                this.context.deltaTransform, 
                this.fullQuality ? 
                    AppEnvironment.ResamplingAlgorithm :
                    ResamplingAlgorithm.NearestNeighbor);

            if (wcc != null)
            {
                wcc.Dispose();
                wcc = null;
            }

            activeLayer.Invalidate(simplifiedRegion);
            PositionNubs(this.context.currentMode);
            
            simplifiedRegion.Dispose();
            selectedRegion.Dispose();
        }

        protected override void PreRender()
        {
            RestoreSavedRegion();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (!tracking)
            {
                return;
            }

            this.fullQuality = true;
            OnMouseMove(e);

#if !ALWAYSHIGHQUALITY
            this.fullQuality = false;
#endif

            this.rotateNub.Visible = false;
            tracking = false;
            PositionNubs(this.context.currentMode);

            string resourceName;
            switch (this.context.currentMode)
            {
                default:
                    throw new InvalidEnumArgumentException();

                case Mode.Rotate:
                    resourceName = "MoveTool.HistoryMemento.Rotate";
                    break;

                case Mode.Scale:
                    resourceName = "MoveTool.HistoryMemento.Scale";
                    break;

                case Mode.Translate:
                    resourceName = "MoveTool.HistoryMemento.Translate";
                    break;
            }

            this.context.startAngle += this.angleDelta;

            if (this.context.liftTransform == null)
            {
                this.context.liftTransform = new Matrix();
            }

            this.context.liftTransform.Reset();
            this.context.liftTransform.Multiply(this.context.deltaTransform, MatrixOrder.Append);
            
            string actionName = PdnResources.GetString(resourceName);
            FlushHistoryMementos(actionName);
        }

        private void FlushHistoryMementos(string name)
        {
            if (this.currentHistoryMementos.Count > 0)
            {
                CompoundHistoryMemento cha = new CompoundHistoryMemento(null, null,
                    this.currentHistoryMementos.ToArray());

                string haName;
                ImageResource image;

                if (this.didPaste)
                {
                    haName = PdnResources.GetString("CommonAction.Paste");
                    image = PdnResources.GetImageResource("Icons.MenuEditPasteIcon.png");
                    this.didPaste = false;
                }
                else
                {
                    if (name == null)
                    {
                        haName = this.Name;
                    }
                    else
                    {
                        haName = name;
                    }

                    image = this.Image;
                }

                CompoundToolHistoryMemento ctha = new CompoundToolHistoryMemento(cha, this.DocumentWorkspace, haName, image);

                ctha.SeriesGuid = context.seriesGuid;
                HistoryStack.PushNewMemento(ctha);

                this.currentHistoryMementos.Clear();
            }
        }

        public MoveTool(DocumentWorkspace documentWorkspace)
            : base(documentWorkspace,
                   PdnResources.GetImageResource("Icons.MoveToolIcon.png"),
                   MoveTool.StaticName,
                   PdnResources.GetString("MoveTool.HelpText"), // "Click and drag to move a selected region",
                   'm',
                   false,
                   ToolBarConfigItems.Resampling)
        {
            this.context = new MoveToolContext();
            this.enableOutline = false;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                DestroyNubs();

                if (this.renderArgs != null)
                {
                    this.renderArgs.Dispose();
                    this.renderArgs = null;
                }

                if (this.context != null)
                {
                    this.context.Dispose();
                    this.context = null;
                }
            }
        }

        protected override void OnExecutingHistoryMemento(ExecutingHistoryMementoEventArgs e)
        {
            this.dontDrop = true;

            RestoreSavedRegion();
            ClearSavedMemory();

            if (e.MayAlterSuspendTool)
            {
                e.SuspendTool = false;
            }
        }

        protected override void OnExecutedHistoryMemento(ExecutedHistoryMementoEventArgs e)
        {
            if (context.lifted)
            {
                bool oldHQ = this.fullQuality;
                this.fullQuality = false;
                Render(context.offset, true);
                ClearSavedMemory();
                this.fullQuality = oldHQ;
            }
            else
            {
                DestroyNubs();
                PositionNubs(this.context.currentMode);
            }

            this.dontDrop = false;
        }

        protected override void OnFinishedHistoryStepGroup()
        {
            if (context.lifted)
            {
                bool oldHQ = this.fullQuality;
                this.fullQuality = true;
                Render(context.offset, true, false);
                this.fullQuality = oldHQ;
            }

            base.OnFinishedHistoryStepGroup();
        }
    }
}
