/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.Data;
using PaintDotNet.Effects;
using PaintDotNet.HistoryFunctions;
using PaintDotNet.HistoryMementos;
using PaintDotNet.SystemLayer;
using PaintDotNet.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// Builds on DocumentView by adding application-specific elements.
    /// </summary>
    internal class DocumentWorkspace
        : DocumentView,
          IHistoryWorkspace,
          IThumbnailProvider
    {
        public static readonly DateTime NeverSavedDateTime = DateTime.MinValue;

        private static Type[] tools; // TODO: move to Tool class?
        private static ToolInfo[] toolInfos;

        private ZoomBasis zoomBasis;
        private AppWorkspace appWorkspace;
        private string filePath = null;
        private FileType fileType = null;
        private SaveConfigToken saveConfigToken = null;
        private Selection selection = new Selection();
        private Surface scratchSurface = null;
        private SelectionRenderer selectionRenderer;
        private Hashtable staticToolData = Hashtable.Synchronized(new Hashtable());
        private Tool activeTool;
        private Type previousActiveToolType;
        private Type preNullTool = null;
        private int nullToolCount = 0;
        private int zoomChangesCount = 0;
        private HistoryStack history;
        private Layer activeLayer;
        private System.Windows.Forms.Timer toolPulseTimer;
        private DateTime lastSaveTime = NeverSavedDateTime;
        private int suspendToolCursorChanges = 0;
        private ImageResource statusIcon = null;
        private string statusText = null;

        private readonly string contextStatusBarWithAngleFormat = PdnResources.GetString("StatusBar.Context.SelectedArea.Text.WithAngle.Format");
        private readonly string contextStatusBarFormat = PdnResources.GetString("StatusBar.Context.SelectedArea.Text.Format");

        public void SuspendToolCursorChanges()
        {
            ++this.suspendToolCursorChanges;
        }

        public void ResumeToolCursorChanges()
        {
            --this.suspendToolCursorChanges;

            if (this.suspendToolCursorChanges <= 0 && this.activeTool != null)
            {
                Cursor = this.activeTool.Cursor;
            }
        }
        
        public ImageResource StatusIcon
        {
            get
            {
                return this.statusIcon;
            }
        }

        public string StatusText
        {
            get
            {
                return this.statusText;
            }
        }

        public void SetStatus(string newStatusText, ImageResource newStatusIcon)
        {
            this.statusText = newStatusText;
            this.statusIcon = newStatusIcon;
            OnStatusChanged();
        }

        public event EventHandler StatusChanged;
        protected virtual void OnStatusChanged()
        {
            if (StatusChanged != null)
            {
                StatusChanged(this, EventArgs.Empty);
            }
        }

        static DocumentWorkspace()
        {
            InitializeTools();
            InitializeToolInfos();
        }

        public DateTime LastSaveTime
        {
            get
            {
                return this.lastSaveTime;
            }
        }

        public bool IsZoomChanging
        {
            get
            {
                return (this.zoomChangesCount > 0);
            }
        }

        private void BeginZoomChanges()
        {
            ++this.zoomChangesCount;
        }

        private void EndZoomChanges()
        {
            --this.zoomChangesCount;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            PerformLayout();
            base.OnSizeChanged(e);
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            if (this.zoomBasis == ZoomBasis.FitToWindow)
            {
                ZoomToWindow();

                // This bizarre ordering of setting PanelAutoScroll prevents some very weird layout/scroll-without-scrollbars stuff.
                PanelAutoScroll = true;
                PanelAutoScroll = false;
            }

            base.OnLayout(e);
        }

        protected override void OnResize(EventArgs e)
        {
            if (this.zoomBasis == ZoomBasis.FitToWindow)
            {
                PerformLayout();
            }

            base.OnResize(e);
        }

        public DocumentWorkspace()
        {
            this.activeLayer = null;
            this.history = new HistoryStack(this);

            InitializeComponent();

            // hook the DocumentWorkspace with its selectedPath ...
            this.selectionRenderer = new SelectionRenderer(this.RendererList, this.Selection, this);
            this.RendererList.Add(this.selectionRenderer, true);
            this.selectionRenderer.EnableOutlineAnimation = true;
            this.selectionRenderer.EnableSelectionTinting = false;
            this.selectionRenderer.EnableSelectionOutline = true;

            this.selection.Changed += new EventHandler(Selection_Changed);

            this.zoomBasis = ZoomBasis.FitToWindow;
        }

        protected override void OnUnitsChanged()
        {
            if (!Selection.IsEmpty)
            {
                UpdateSelectionInfoInStatusBar();
            }

            base.OnUnitsChanged();
        }

        public void UpdateStatusBarToToolHelpText(Tool tool)
        {
            if (tool == null)
            {
                SetStatus(string.Empty, null);
            }
            else
            {
                string toolName = tool.Name;
                string helpText = tool.HelpText;

                string contextFormat = PdnResources.GetString("StatusBar.Context.Help.Text.Format");
                string contextText = string.Format(contextFormat, toolName, helpText);

                SetStatus(contextText, PdnResources.GetImageResource("Icons.MenuHelpHelpTopicsIcon.png"));
            }
        }

        public void UpdateStatusBarToToolHelpText()
        {
            UpdateStatusBarToToolHelpText(this.activeTool);
        }

        private void UpdateSelectionInfoInStatusBar()
        {
            if (Selection.IsEmpty)
            {
                UpdateStatusBarToToolHelpText();
            }
            else
            {
                string newStatusText;

                int area = 0;
                Rectangle bounds;

                using (PdnRegion tempSelection = Selection.CreateRegionRaw())
                {
                    tempSelection.Intersect(Document.Bounds);
                    bounds = Utility.GetRegionBounds(tempSelection);
                    area = tempSelection.GetArea();
                }

                string unitsAbbreviationXY;
                string xString;
                string yString;
                string unitsAbbreviationWH;
                string widthString;
                string heightString;

                Document.CoordinatesToStrings(Units, bounds.X, bounds.Y, out xString, out yString, out unitsAbbreviationXY);
                Document.CoordinatesToStrings(Units, bounds.Width, bounds.Height, out widthString, out heightString, out unitsAbbreviationWH);

                NumberFormatInfo nfi = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();

                string areaString;
                if (this.Units == MeasurementUnit.Pixel)
                {
                    nfi.NumberDecimalDigits = 0;
                    areaString = area.ToString("N", nfi);
                }
                else
                {
                    nfi.NumberDecimalDigits = 2;
                    double areaD = Document.PixelAreaToPhysicalArea(area, this.Units);
                    areaString = areaD.ToString("N", nfi);
                }

                string pluralUnits = PdnResources.GetString("MeasurementUnit." + this.Units.ToString() + ".Plural");
                MoveToolBase moveTool = Tool as MoveToolBase;

                if (moveTool != null && moveTool.HostShouldShowAngle)
                {
                    NumberFormatInfo nfi2 = (NumberFormatInfo)nfi.Clone();
                    nfi2.NumberDecimalDigits = 2;
                    float angle = moveTool.HostAngle;

                    while (angle > 180.0f)
                    {
                        angle -= 360.0f;
                    }

                    while (angle < -180.0f)
                    {
                        angle += 360.0f;
                    }

                    newStatusText = string.Format(
                        contextStatusBarWithAngleFormat,
                        xString,
                        unitsAbbreviationXY,
                        yString,
                        unitsAbbreviationXY,
                        widthString,
                        unitsAbbreviationWH,
                        heightString,
                        unitsAbbreviationWH,
                        areaString,
                        pluralUnits.ToLower(),
                        moveTool.HostAngle.ToString("N", nfi2));
                }
                else
                {
                    newStatusText = string.Format(
                        contextStatusBarFormat,
                        xString,
                        unitsAbbreviationXY,
                        yString,
                        unitsAbbreviationXY,
                        widthString,
                        unitsAbbreviationWH,
                        heightString,
                        unitsAbbreviationWH,
                        areaString,
                        pluralUnits.ToLower());
                }

                SetStatus(newStatusText, PdnResources.GetImageResource("Icons.SelectionIcon.png"));
            }
        }

        private void Selection_Changed(object sender, EventArgs e)
        {
            UpdateRulerSelectionTinting();
            UpdateSelectionInfoInStatusBar();
        }

        private void InitializeComponent()
        {
            this.toolPulseTimer = new System.Windows.Forms.Timer();
            this.toolPulseTimer.Interval = 16;
            this.toolPulseTimer.Tick += new EventHandler(this.ToolPulseTimer_Tick);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.activeTool != null)
                {
                    this.activeTool.Dispose();
                    this.activeTool = null;
                }
            }

            base.Dispose(disposing);
        }

        public void PerformActionAsync(DocumentWorkspaceAction action)
        {
            BeginInvoke(new Procedure<DocumentWorkspaceAction>(PerformAction), new object[] { action });
        }

        public void PerformAction(DocumentWorkspaceAction action)
        {
            bool nullTool = false;

            if ((action.ActionFlags & ActionFlags.KeepToolActive) != ActionFlags.KeepToolActive)
            {
                PushNullTool();
                Update();
                nullTool = true;
            }

            try
            {
                using (new WaitCursorChanger(this))
                {
                    HistoryMemento ha = action.PerformAction(this);

                    if (ha != null)
                    {
                        History.PushNewMemento(ha);
                    }
                }
            }

            finally
            {
                if (nullTool)
                {
                    PopNullTool();
                }
            }
        }

        /// <summary>
        /// Executes a HistoryFunction in the context of this DocumentWorkspace.
        /// </summary>
        /// <param name="function">The HistoryFunction to execute.</param>
        /// <remarks>
        /// Depending on the HistoryFunction, the currently active tool may be refreshed.
        /// </remarks>
        public HistoryFunctionResult ExecuteFunction(HistoryFunction function)
        {
            HistoryFunctionResult result;

            bool nullTool = false;

            if ((function.ActionFlags & ActionFlags.KeepToolActive) != ActionFlags.KeepToolActive)
            {
                PushNullTool();
                Update();
                nullTool = true;
            }

            try
            {
                using (new WaitCursorChanger(this))
                {
                    HistoryMemento hm = null;
                    string errorText;

                    try
                    {
                        bool cancelled = false;

                        if ((function.ActionFlags & ActionFlags.ReportsProgress) != ActionFlags.ReportsProgress)
                        {
                            hm = function.Execute(this);
                        }
                        else
                        {
                            ProgressDialog pd = new ProgressDialog();
                            bool pdLoaded = false;
                            bool closeAtLoad = false;

                            EventHandler loadCallback =
                                delegate(object sender, EventArgs e)
                                {
                                    pdLoaded = true;

                                    if (closeAtLoad)
                                    {
                                        pd.Close();
                                    }
                                };

                            ProgressEventHandler progressCallback =
                                delegate(object sender, ProgressEventArgs e)
                                {
                                    if (pdLoaded)
                                    {
                                        double newValue = Utility.Clamp(e.Percent, 0.0, 100.0);
                                        pd.Value = newValue;
                                    }
                                };

                            EventHandler<EventArgs<HistoryMemento>> finishedCallback =
                                delegate(object sender, EventArgs<HistoryMemento> e)
                                {
                                    hm = e.Data;

                                    if (pdLoaded)
                                    {
                                        // TODO: fix ProgressDialog's very weird interface
                                        pd.ExternalFinish();
                                        pd.Close();
                                    }
                                    else
                                    {
                                        closeAtLoad = true;
                                    }
                                };

                            EventHandler cancelClickCallback =
                                delegate(object sender, EventArgs e)
                                {
                                    cancelled = true;
                                    function.RequestCancel();
                                    //pd.Cancellable = false;
                                };

                            pd.Text = PdnInfo.GetBareProductName();
                            pd.Description = PdnResources.GetString("ExecuteFunction.ProgressDialog.Description.Text");
                            pd.Load += loadCallback;
                            pd.Cancellable = false; //function.Cancellable;
                            pd.CancelClick += cancelClickCallback;
                            function.Progress += progressCallback;
                            function.BeginExecute(this, this, finishedCallback);
                            pd.ShowDialog(this);
                            pd.Dispose();
                        }

                        if (hm == null && !cancelled)
                        {
                            result = HistoryFunctionResult.SuccessNoOp;
                        }
                        else if (hm == null && cancelled)
                        {
                            result = HistoryFunctionResult.Cancelled;
                        }
                        else
                        {
                            result = HistoryFunctionResult.Success;
                        }

                        errorText = null;
                    }

                    catch (HistoryFunctionNonFatalException hfnfex)
                    {
                        if (hfnfex.InnerException is OutOfMemoryException)
                        {
                            result = HistoryFunctionResult.OutOfMemory;
                        }
                        else
                        {
                            result = HistoryFunctionResult.NonFatalError;
                        }

                        if (hfnfex.LocalizedErrorText != null)
                        {
                            errorText = hfnfex.LocalizedErrorText;
                        }
                        else
                        {
                            if (hfnfex.InnerException is OutOfMemoryException)
                            {
                                errorText = PdnResources.GetString("ExecuteFunction.GenericOutOfMemory");
                            }
                            else
                            {
                                errorText = PdnResources.GetString("ExecuteFunction.GenericError");
                            }
                        }
                    }

                    if (errorText != null)
                    {
                        Utility.ErrorBox(this, errorText);
                    }

                    if (hm != null)
                    {
                        History.PushNewMemento(hm);
                    }
                }
            }

            finally
            {
                if (nullTool)
                {
                    PopNullTool();
                }
            }

            return result;
        }

        public override void ZoomIn()
        {
            this.ZoomBasis = ZoomBasis.ScaleFactor;
            base.ZoomIn();
        }

        public override void ZoomIn(double factor)
        {
            this.ZoomBasis = ZoomBasis.ScaleFactor;
            base.ZoomIn(factor);
        }

        public override void ZoomOut()
        {
            this.ZoomBasis = ZoomBasis.ScaleFactor;
            base.ZoomOut();
        }

        public override void ZoomOut(double factor)
        {
            this.ZoomBasis = ZoomBasis.ScaleFactor;
            base.ZoomOut(factor);
        }

        // TODO:
        /// <summary>
        /// Same as PerformAction(Type) except it lets you rename the HistoryMemento's name.
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="newName"></param>
        public void PerformAction(Type actionType, string newName, ImageResource icon)
        {
            using (new WaitCursorChanger(this))
            {
                ConstructorInfo ci = actionType.GetConstructor(new Type[] { typeof(DocumentWorkspace) });
                object actionAsObject = ci.Invoke(new object[] { this });
                DocumentWorkspaceAction action = actionAsObject as DocumentWorkspaceAction;

                if (action != null)
                {
                    bool nullTool = false;

                    if ((action.ActionFlags & ActionFlags.KeepToolActive) != ActionFlags.KeepToolActive)
                    {
                        PushNullTool();
                        Update();
                        nullTool = true;
                    }

                    try
                    {
                        HistoryMemento ha = action.PerformAction(this);

                        if (ha != null)
                        {
                            ha.Name = newName;
                            ha.Image = icon;
                            History.PushNewMemento(ha);
                        }
                    }

                    finally
                    {
                        if (nullTool)
                        {
                            PopNullTool();
                        }
                    }
                }
            }
        }
        
        public event EventHandler ZoomBasisChanging;
        protected virtual void OnZoomBasisChanging()
        {
            if (ZoomBasisChanging != null)
            {
                ZoomBasisChanging(this, EventArgs.Empty);
            }
        }

        public event EventHandler ZoomBasisChanged;
        protected virtual void OnZoomBasisChanged()
        {
            if (ZoomBasisChanged != null)
            {
                ZoomBasisChanged(this, EventArgs.Empty);
            }
        }

        public ZoomBasis ZoomBasis
        {
            get
            {
                return this.zoomBasis;
            }

            set
            {
                if (this.zoomBasis != value)
                {
                    OnZoomBasisChanging();
                    this.zoomBasis = value;

                    switch (this.zoomBasis)
                    {
                        case ZoomBasis.FitToWindow:
                            ZoomToWindow();

                            // Enable PanelAutoScroll only long enough to recenter the view
                            PanelAutoScroll = true;
                            PanelAutoScroll = false;

                            // this would be unset by the scalefactor changes in ZoomToWindow
                            this.zoomBasis = ZoomBasis.FitToWindow;
                            break;

                        case ZoomBasis.ScaleFactor:
                            PanelAutoScroll = true;
                            break;

                        default:
                            throw new InvalidEnumArgumentException();
                    }

                    OnZoomBasisChanged();
                }
            }
        }

        public void ZoomToSelection()
        {
            if (Selection.IsEmpty)
            {
                ZoomToWindow();
            }
            else
            {
                using (PdnRegion region = Selection.CreateRegion())
                {
                    ZoomToRectangle(region.GetBoundsInt());
                }
            }
        }

        public void ZoomToRectangle(Rectangle selectionBounds)
        {
            PointF selectionCenter = new PointF((selectionBounds.Left + selectionBounds.Right + 1) / 2,
                (selectionBounds.Top + selectionBounds.Bottom + 1) / 2);

            PointF cornerPosition;

            ScaleFactor zoom = ScaleFactor.Min(ClientRectangleMin.Width, selectionBounds.Width + 2,
                                               ClientRectangleMin.Height, selectionBounds.Height + 2,
                                               ScaleFactor.MinValue);

            // Zoom out to fit the image
            ZoomBasis = ZoomBasis.ScaleFactor;
            ScaleFactor = zoom;

            cornerPosition = new PointF(selectionCenter.X - (VisibleDocumentRectangleF.Width / 2),
                selectionCenter.Y - (VisibleDocumentRectangleF.Height / 2));

            DocumentScrollPositionF = cornerPosition;
        }

        protected override void HandleMouseWheel(Control sender, MouseEventArgs e)
        {
            if (Control.ModifierKeys == Keys.Control)
            {
                double mouseDelta = (double)e.Delta / 120.0f;
                Rectangle visibleDocBoundsStart = this.VisibleDocumentBounds;
                Point mouseDocPt = this.MouseToDocument(sender, new Point(e.X, e.Y));
                RectangleF visibleDocDocRect1 = this.VisibleDocumentRectangleF;

                PointF mouseNPt = new PointF(
                    (mouseDocPt.X - visibleDocDocRect1.X) / visibleDocDocRect1.Width,
                    (mouseDocPt.Y - visibleDocDocRect1.Y) / visibleDocDocRect1.Height);

                const double factor = 1.12;
                double mouseFactor = Math.Pow(factor, Math.Abs(mouseDelta));

                if (e.Delta > 0)
                {
                    this.ZoomIn(mouseFactor);
                }
                else if (e.Delta < 0)
                {
                    this.ZoomOut(mouseFactor);
                }

                RectangleF visibleDocDocRect2 = this.VisibleDocumentRectangleF;

                PointF scrollPt2 = new PointF(
                    mouseDocPt.X - visibleDocDocRect2.Width * mouseNPt.X,
                    mouseDocPt.Y - visibleDocDocRect2.Height * mouseNPt.Y);

                this.DocumentScrollPositionF = scrollPt2;

                Rectangle visibleDocBoundsEnd = this.VisibleDocumentBounds;

                if (visibleDocBoundsEnd != visibleDocBoundsStart)
                {
                    // Make sure the screen updates, otherwise it can get a little funky looking
                    this.Update();
                }
            }

            base.HandleMouseWheel(sender, e);
        }

        public void SelectClosestVisibleLayer(Layer layer)
        {
            int oldLayerIndex = this.Document.Layers.IndexOf(layer);
            int newLayerIndex = oldLayerIndex;

            // find the closest layer that is still visible
            for (int i = 0; i < this.Document.Layers.Count; ++i)
            {
                int lower = oldLayerIndex - i;
                int upper = oldLayerIndex + i;

                if (lower >= 0 && lower < this.Document.Layers.Count && ((Layer)this.Document.Layers[lower]).Visible)
                {
                    newLayerIndex = lower;
                    break;
                }

                if (upper >= 0 && upper < this.Document.Layers.Count && ((Layer)this.Document.Layers[upper]).Visible)
                {
                    newLayerIndex = upper;
                    break;
                }
            }

            if (newLayerIndex != oldLayerIndex)
            {
                this.ActiveLayer = (Layer)Document.Layers[newLayerIndex];
            }
        }

        public void UpdateRulerSelectionTinting()
        {
            if (this.RulersEnabled)
            {
                Rectangle bounds = this.Selection.GetBounds();
                this.SetHighlightRectangle(bounds);
            }
        }

        private void LayerRemovingHandler(object sender, IndexEventArgs e)
        {
            Layer layer = (Layer)this.Document.Layers[e.Index];
            layer.PropertyChanging -= LayerPropertyChangingHandler;
            layer.PropertyChanged -= LayerPropertyChangedHandler;

            // pick a new valid layer!
            int newLayerIndex;

            if (e.Index == this.Document.Layers.Count - 1)
            {
                newLayerIndex = e.Index - 1;
            }
            else
            {
                newLayerIndex = e.Index + 1;
            }

            if (newLayerIndex >= 0 && newLayerIndex < this.Document.Layers.Count)
            {
                this.ActiveLayer = (Layer)this.Document.Layers[newLayerIndex];
            }
            else
            {
                if (this.Document.Layers.Count == 0)
                {
                    this.ActiveLayer = null;
                }
                else
                {
                    this.ActiveLayer = (Layer)this.Document.Layers[0];
                }
            }
        }

        private void LayerRemovedHandler(object sender, IndexEventArgs e)
        {
        }

        private void LayerInsertedHandler(object sender, IndexEventArgs e)
        {
            Layer layer = (Layer)this.Document.Layers[e.Index];
            this.ActiveLayer = layer;
            layer.PropertyChanging += LayerPropertyChangingHandler;
            layer.PropertyChanged += LayerPropertyChangedHandler;
        }

        private void LayerPropertyChangingHandler(object sender, PropertyEventArgs e)
        {
            string nameFormat = PdnResources.GetString("LayerPropertyChanging.HistoryMementoNameFormat");
            string haName = string.Format(nameFormat, e.PropertyName);

            LayerPropertyHistoryMemento lpha = new LayerPropertyHistoryMemento(
                haName,
                PdnResources.GetImageResource("Icons.MenuLayersLayerPropertiesIcon.png"),
                this,
                this.Document.Layers.IndexOf(sender));

            this.History.PushNewMemento(lpha);
        }

        private void LayerPropertyChangedHandler(object sender, PropertyEventArgs e)
        {
            Layer layer = (Layer)sender;

            if (!layer.Visible && 
                layer == this.ActiveLayer && 
                this.Document.Layers.Count > 1 &&
                !History.IsExecutingMemento)
            {
                SelectClosestVisibleLayer(layer);
            }
        }

        private void ToolPulseTimer_Tick(object sender, EventArgs e)
        {
            if (FindForm() == null || FindForm().WindowState == FormWindowState.Minimized)
            {
                return;
            }

            if (this.Tool != null && this.Tool.Active)
            {
                this.Tool.PerformPulse();
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            if (this.appWorkspace == null)
            {
                throw new InvalidOperationException("Must set the Workspace property");
            }

            base.OnLoad(e);
        }

        public event EventHandler ActiveLayerChanging;
        protected void OnLayerChanging()
        {
            if (ActiveLayerChanging != null)
            {
                ActiveLayerChanging(this, EventArgs.Empty);
            }
        }

        public event EventHandler ActiveLayerChanged;
        protected void OnLayerChanged()
        {
            this.Focus();

            if (ActiveLayerChanged != null)
            {
                ActiveLayerChanged(this, EventArgs.Empty);
            }
        }

        public Layer ActiveLayer
        {
            get
            {
                return this.activeLayer;
            }

            set
            {
                OnLayerChanging();

                bool deactivateTool;

                if (this.Tool != null)
                {
                    deactivateTool = this.Tool.DeactivateOnLayerChange;
                }
                else
                {
                    deactivateTool = false;
                }

                if (deactivateTool)
                {
                    PushNullTool();
                    this.EnableToolPulse = false;
                }

                try
                {
                    // Verify that the layer is in the document (sanity checking)
                    if (this.Document != null)
                    {
                        if (value != null && !this.Document.Layers.Contains(value))
                        {
                            throw new InvalidOperationException("ActiveLayer was changed to a layer that is not contained within the Document");
                        }
                    }
                    else
                    {   
                        // Document == null
                        if (value != null)
                        {
                            throw new InvalidOperationException("ActiveLayer was set to non-null while Document was null");
                        }
                    }

                    // Finally, set the field.
                    this.activeLayer = value;
                }

                finally
                {
                    if (deactivateTool)
                    {
                        PopNullTool();
                        this.EnableToolPulse = true;
                    }
                }

                OnLayerChanged();
            }
        }

        public int ActiveLayerIndex
        {
            get
            {
                return Document.Layers.IndexOf(ActiveLayer);
            }

            set
            {
                this.ActiveLayer = (Layer)Document.Layers[value];
            }
        }

        public bool EnableToolPulse
        {
            get
            {
                return this.toolPulseTimer.Enabled;
            }

            set
            {
                this.toolPulseTimer.Enabled = value;
            }
        }

        public HistoryStack History
        {
            get
            {
                return this.history;
            }
        }

        public Tool Tool
        {
            get
            {
                return this.activeTool;
            }
        }

        public Type GetToolType()
        {
            if (Tool != null)
            {
                return Tool.GetType();
            }
            else
            {
                return null;
            }
        }

        public void SetToolFromType(Type toolType)
        {
            if (toolType == GetToolType())
            {
                return;
            }
            else if (toolType == null)
            {
                SetTool(null);
            }
            else
            {
                Tool newTool = CreateTool(toolType);
                SetTool(newTool);
            }
        }

        public void PushNullTool()
        {
            if (this.nullToolCount == 0)
            {
                this.preNullTool = GetToolType();
                this.SetTool(null);
                this.nullToolCount = 1;
            }
            else
            {
                ++this.nullToolCount;
            }
        }

        public void PopNullTool()
        {
            --this.nullToolCount;

            if (this.nullToolCount == 0)
            {
                this.SetToolFromType(this.preNullTool);
                this.preNullTool = null;
            }
            else if (this.nullToolCount < 0)
            {
                throw new InvalidOperationException("PopNullTool() call was not matched with PushNullTool()");
            }
        }

        public Type PreviousActiveToolType
        {
            get
            {
                return this.previousActiveToolType;
            }
        }

        public void SetTool(Tool copyMe)
        {
            OnToolChanging();

            if (this.activeTool != null)
            {
                this.previousActiveToolType = this.activeTool.GetType();
                this.activeTool.CursorChanged -= ToolCursorChangedHandler;
                this.activeTool.PerformDeactivate();
                this.activeTool.Dispose();
                this.activeTool = null;
            }

            if (copyMe == null)
            {
                EnableToolPulse = false;
            }
            else
            {
                Tracing.LogFeature("SetTool(" + copyMe.GetType().FullName + ")");
                this.activeTool = CreateTool(copyMe.GetType());
                this.activeTool.PerformActivate();
                this.activeTool.CursorChanged += ToolCursorChangedHandler;

                if (this.suspendToolCursorChanges <= 0)
                {
                    Cursor = this.activeTool.Cursor;
                }

                EnableToolPulse = true;
            }

            OnToolChanged();
        }

        public Tool CreateTool(Type toolType)
        {
            return DocumentWorkspace.CreateTool(toolType, this);
        }

        private static Tool CreateTool(Type toolType, DocumentWorkspace dc)
        {
            ConstructorInfo ci = toolType.GetConstructor(new Type[] { typeof(DocumentWorkspace) });
            Tool tool = (Tool)ci.Invoke(new object[] { dc });
            return tool;
        }

        private static void InitializeTools()
        {
            // add all the tools
            tools = new Type[] 
            {
                typeof(RectangleSelectTool),
                typeof(MoveTool),
                typeof(LassoSelectTool),
                typeof(MoveSelectionTool),

                typeof(EllipseSelectTool),
                typeof(ZoomTool),

                typeof(MagicWandTool),
                typeof(PanTool),

                typeof(PaintBucketTool),
                typeof(GradientTool),

                typeof(PaintBrushTool),
                typeof(EraserTool),
                typeof(PencilTool),
                typeof(ColorPickerTool),
                typeof(CloneStampTool), 
                typeof(RecolorTool),
                typeof(TextTool),

                typeof(LineTool),
                typeof(RectangleTool),
                typeof(RoundedRectangleTool),
                typeof(EllipseTool),
                typeof(FreeformShapeTool),
            };
        }

        private static void InitializeToolInfos()
        {
            int i = 0;
            toolInfos = new ToolInfo[tools.Length];

            foreach (Type toolType in tools)
            {
                using (Tool tool = DocumentWorkspace.CreateTool(toolType, null))
                {
                    toolInfos[i] = tool.Info;
                    ++i;
                }
            }
        }

        public static Type[] Tools
        {
            get
            {
                return (Type[])tools.Clone();
            }
        }

        public static ToolInfo[] ToolInfos
        {
            get
            {
                return (ToolInfo[])toolInfos.Clone();
            }
        }

        public event EventHandler ToolChanging;
        protected void OnToolChanging()
        {
            if (ToolChanging != null)
            {
                ToolChanging(this, EventArgs.Empty);
            }
        }

        public event EventHandler ToolChanged;
        protected void OnToolChanged()
        {
            if (ToolChanged != null)
            {
                ToolChanged(this, EventArgs.Empty);
            }
        }

        private void ToolCursorChangedHandler(object sender, EventArgs e)
        {
            if (this.suspendToolCursorChanges <= 0)
            {
                Cursor = this.activeTool.Cursor;
            }
        }

        // Note: static tool data is removed whenever the Document changes
        // TODO: shouldn't this be moved to the Tool class somehow?
        public object GetStaticToolData(Type toolType)
        {
            return staticToolData[toolType];
        }

        public void SetStaticToolData(Type toolType, object data)
        {
            staticToolData[toolType] = data;
        }

        public AppWorkspace AppWorkspace
        {
            get
            {
                return this.appWorkspace;
            }

            set
            {
                this.appWorkspace = value;
            }
        }

        public Selection Selection
        {
            get
            {
                return this.selection;
            }
        }

        public bool EnableOutlineAnimation
        {
            get
            {
                return this.selectionRenderer.EnableOutlineAnimation;
            }

            set
            {
                this.selectionRenderer.EnableOutlineAnimation = value;
            }
        }

        public bool EnableSelectionOutline
        {
            get
            {
                return this.selectionRenderer.EnableSelectionOutline;
            }

            set
            {
                this.selectionRenderer.EnableSelectionOutline = value;
            }
        }

        public bool EnableSelectionTinting
        {
            get
            {
                return this.selectionRenderer.EnableSelectionTinting;
            }

            set
            {
                this.selectionRenderer.EnableSelectionTinting = value;
            }
        }

        public void ResetOutlineWhiteOpacity()
        {
            this.selectionRenderer.ResetOutlineWhiteOpacity();
        }

        public event EventHandler FilePathChanged;
        protected virtual void OnFilePathChanged()
        {
            if (FilePathChanged != null)
            {
                FilePathChanged(this, EventArgs.Empty);
            }
        }

        public string FilePath
        {
            get
            {
                return this.filePath;
            }
        }

        public string GetFriendlyName()
        {
            string friendlyName;

            if (this.filePath != null)
            {
                friendlyName = Path.GetFileName(this.filePath);
            }
            else
            {
                friendlyName = PdnResources.GetString("Untitled.FriendlyName");
            }

            return friendlyName;
        }

        public FileType FileType
        {
            get
            {
                return this.fileType;
            }
        }

        public SaveConfigToken SaveConfigToken
        {
            get
            {
                if (this.saveConfigToken == null)
                {
                    return null;
                }
                else
                {
                    return (SaveConfigToken)this.saveConfigToken.Clone();
                }
            }
        }

        public event EventHandler SaveOptionsChanged;
        protected virtual void OnSaveOptionsChanged()
        {
            if (SaveOptionsChanged != null)
            {
                SaveOptionsChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Sets the FileType and SaveConfigToken parameters that are used if the
        /// user chooses "Save" from the File menu. These are not used by the
        /// DocumentControl class and should be used by whoever actually goes
        /// to save the Document instance.
        /// </summary>
        /// <param name="fileType"></param>
        /// <param name="saveParameters"></param>
        public void SetDocumentSaveOptions(string newFilePath, FileType newFileType, SaveConfigToken newSaveConfigToken)
        {
            this.filePath = newFilePath;
            OnFilePathChanged();

            this.fileType = newFileType;

            if (newSaveConfigToken == null)
            {
                this.saveConfigToken = null;
            }
            else
            {
                this.saveConfigToken = (SaveConfigToken)newSaveConfigToken.Clone();
            }

            OnSaveOptionsChanged();
        }

        public void GetDocumentSaveOptions(out string filePathResult, out FileType fileTypeResult, out SaveConfigToken saveConfigTokenResult)
        {
            filePathResult = this.filePath;
            fileTypeResult = this.fileType;

            if (this.saveConfigToken == null)
            {
                saveConfigTokenResult = null;
            }
            else
            {
                saveConfigTokenResult = (SaveConfigToken)this.saveConfigToken.Clone();
            }
        }

        private bool isScratchSurfaceBorrowed = false;
        private string borrowScratchSurfaceReason = string.Empty;

        /// The scratch, stencil, accumulation, whatever buffer. This is used by many parts
        /// of Paint.NET as a temporary area for which to store data.
        /// This surface is 'owned' by any Tool that is active. If you want to use this you
        /// must first deactivate the Tool using PushNullTool() and then reactivate it when 
        /// you are finished by calling PopNullTool().
        /// Tools should use Tool.ScratchSurface instead of these API's.

        public Surface BorrowScratchSurface(string reason)
        {
            if (this.isScratchSurfaceBorrowed)
            {
                throw new InvalidOperationException(
                    "ScratchSurface already borrowed: '" + 
                    this.borrowScratchSurfaceReason + 
                    "' (trying to borrow for: '" + reason + "')");
            }

            Tracing.Ping("Borrowing scratchSurface: " + reason);
            this.isScratchSurfaceBorrowed = true;
            this.borrowScratchSurfaceReason = reason;
            return this.scratchSurface;
        }

        public void ReturnScratchSurface(Surface borrowedScratchSurface)
        {
            if (!this.isScratchSurfaceBorrowed)
            {
                throw new InvalidOperationException("ScratchSurface wasn't borrowed");
            }

            if (this.scratchSurface != borrowedScratchSurface)
            {
                throw new InvalidOperationException("returned ScratchSurface doesn't match the real one");
            }

            Tracing.Ping("Returning scratchSurface: " + this.borrowScratchSurfaceReason);
            this.isScratchSurfaceBorrowed = false;
            this.borrowScratchSurfaceReason = string.Empty;
        }

        /// <summary>
        /// Updates any pertinent EXIF tags, such as "Creation Software", to be
        /// relevant or up-to-date.
        /// </summary>
        /// <param name="document"></param>
        private void UpdateExifTags(Document document)
        {
            // We want it to say "Creation Software: Paint.NET vX.Y"
            // I have verified that other image editing software overwrites this tag,
            // and does not just add it when it does not exist.
            PropertyItem pi = Exif.CreateAscii(ExifTagID.Software, PdnInfo.GetProductName(false));
            document.Metadata.ReplaceExifValues(ExifTagID.Software, new PropertyItem[1] { pi });
        }
        
        private ZoomBasis savedZb;
        private ScaleFactor savedSf;
        private int savedAli;
        protected override void OnDocumentChanging(Document newDocument)
        {
            base.OnDocumentChanging(newDocument);

            this.savedZb = this.ZoomBasis;
            this.savedSf = ScaleFactor;

            if (this.ActiveLayer != null)
            {
                this.savedAli = ActiveLayerIndex;
            }
            else
            {
                this.savedAli = -1;
            }

            if (newDocument != null)
            {
                UpdateExifTags(newDocument);
            }

            if (this.Document != null)
            {
                foreach (Layer layer in this.Document.Layers)
                {
                    layer.PropertyChanging -= LayerPropertyChangingHandler;
                    layer.PropertyChanged -= LayerPropertyChangedHandler;
                }

                this.Document.Layers.RemovingAt -= LayerRemovingHandler;
                this.Document.Layers.RemovedAt -= LayerRemovedHandler;
                this.Document.Layers.Inserted -= LayerInsertedHandler;
            }
            
            this.staticToolData.Clear();

            PushNullTool(); // matching Pop is in OnDocumetChanged()
            ActiveLayer = null;

            if (this.scratchSurface != null)
            {
                if (this.isScratchSurfaceBorrowed)
                {
                    throw new InvalidOperationException("scratchSurface is currently borrowed: " + this.borrowScratchSurfaceReason);
                }

                if (newDocument == null || newDocument.Size != this.scratchSurface.Size)
                {
                    this.scratchSurface.Dispose();
                    this.scratchSurface = null;
                }
            }

            if (!Selection.IsEmpty)
            {
                Selection.Reset();
            }
        }

        protected override void OnDocumentChanged()
        {
            // if the ActiveLayer is not in this new document, then
            // we try to set ActiveLayer to the first layer in this
            // new document. But if the document contains no layers,
            // or is null, we just null the ActiveLayer.
            if (this.Document == null)
            {
                this.ActiveLayer = null;
            }
            else
            {
                if (this.activeTool != null)
                {
                    throw new InvalidOperationException("Tool was not deactivated while Document was being changed");
                }

                if (this.scratchSurface != null)
                {
                    if (this.isScratchSurfaceBorrowed)
                    {
                        throw new InvalidOperationException("scratchSurface is currently borrowed: " + this.borrowScratchSurfaceReason);
                    }

                    if (Document == null || this.scratchSurface.Size != Document.Size)
                    {
                        this.scratchSurface.Dispose();
                        this.scratchSurface = null;
                    }
                }

                this.scratchSurface = new Surface(this.Document.Size);

                this.Selection.ClipRectangle = this.Document.Bounds;

                foreach (Layer layer in this.Document.Layers)
                {
                    layer.PropertyChanging += LayerPropertyChangingHandler;
                    layer.PropertyChanged += LayerPropertyChangedHandler;
                }

                this.Document.Layers.RemovingAt += LayerRemovingHandler;
                this.Document.Layers.RemovedAt += LayerRemovedHandler;
                this.Document.Layers.Inserted += LayerInsertedHandler;

                if (!this.Document.Layers.Contains(this.ActiveLayer))
                {
                    if (this.Document.Layers.Count > 0)
                    {
                        if (savedAli >= 0 && savedAli < this.Document.Layers.Count)
                        {
                            this.ActiveLayer = (Layer)this.Document.Layers[savedAli];
                        }
                        else
                        {
                            this.ActiveLayer = (Layer)this.Document.Layers[0];
                        }
                    }
                    else
                    {
                        this.ActiveLayer = null;
                    }
                }

                // we invalidate each layer so that the layer previews refresh themselves
                foreach (Layer layer in this.Document.Layers)
                {
                    layer.Invalidate();
                }

                bool oldDirty = this.Document.Dirty;
                this.Document.Invalidate();
                this.Document.Dirty = oldDirty;

                this.ZoomBasis = this.savedZb;
                if (this.savedZb == ZoomBasis.ScaleFactor)
                {
                    ScaleFactor = this.savedSf;
                }
            }

            PopNullTool();
            AutoScrollPosition = new Point(0, 0);

            base.OnDocumentChanged();
        }

        /// <summary>
        /// Takes the current Document from this DocumentWorkspace instance and adds it to the MRU list.
        /// </summary>
        /// <param name="fileName"></param>
        public void AddToMruList()
        {
            using (new PushNullToolMode(this))
            {
                string fullFileName = Path.GetFullPath(this.FilePath);
                int edgeLength = AppWorkspace.MostRecentFiles.IconSize;
                Surface thumb1 = RenderThumbnail(edgeLength, true, true);

                // Put it inside a square bitmap
                Surface thumb = new Surface(4 + edgeLength, 4 + edgeLength);

                thumb.Clear(ColorBgra.Transparent);

                Rectangle dstRect = new Rectangle((thumb.Width - thumb1.Width) / 2,
                    (thumb.Height - thumb1.Height) / 2, thumb1.Width, thumb1.Height);

                thumb.CopySurface(thumb1, dstRect.Location);

                using (RenderArgs ra = new RenderArgs(thumb))
                {
                    // Draw black border
                    Rectangle borderRect = new Rectangle(dstRect.Left - 1, dstRect.Top - 1, dstRect.Width + 2, dstRect.Height + 2);
                    --borderRect.Width;
                    --borderRect.Height;
                    ra.Graphics.DrawRectangle(Pens.Black, borderRect);

                    Rectangle shadowRect = Rectangle.Inflate(borderRect, 1, 1);
                    ++shadowRect.Width;
                    ++shadowRect.Height;
                    Utility.DrawDropShadow1px(ra.Graphics, shadowRect);

                    thumb1.Dispose();
                    thumb1 = null;

                    MostRecentFile mrf = new MostRecentFile(fullFileName, Utility.FullCloneBitmap(ra.Bitmap));

                    if (AppWorkspace.MostRecentFiles.Contains(fullFileName))
                    {
                        AppWorkspace.MostRecentFiles.Remove(fullFileName);
                    }

                    AppWorkspace.MostRecentFiles.Add(mrf);
                    AppWorkspace.MostRecentFiles.SaveMruList();
                }
            }
        }

        /// <summary>
        /// Shows an OpenFileDialog or SaveFileDialog and populates the InitialDirectory from the global
        /// settings repository if possible.
        /// </summary>
        /// <param name="fd">The FileDialog to show.</param>
        /// <remarks>
        /// The FileDialog should already have its InitialDirectory populated as a suggestion of where to start.
        /// </remarks>
        public static DialogResult ShowFileDialog(Control owner, IFileDialog fd)
        {
            string initialDirectory = Settings.CurrentUser.GetString(SettingNames.LastFileDialogDirectory, fd.InitialDirectory);

            // TODO: spawn this in a background thread, if it doesn't respond within ~500ms?, assume the dir doesn't exist
            bool dirExists = false;

            try 
            {
                DirectoryInfo dirInfo = new DirectoryInfo(initialDirectory);

                using (new WaitCursorChanger(owner))
                {
                    dirExists = dirInfo.Exists;

                    if (!dirInfo.Exists)
                    {
                        initialDirectory = fd.InitialDirectory;
                    }
                }
            }

            catch (Exception)
            {
                initialDirectory = fd.InitialDirectory;
            }

            fd.InitialDirectory = initialDirectory;

            OurFileDialogUICallbacks ouc = new OurFileDialogUICallbacks();
            DialogResult result = fd.ShowDialog(owner, ouc);

            if (result == DialogResult.OK)
            {
                string fileName;
                
                if (fd is IFileOpenDialog)
                {
                    string[] fileNames = ((IFileOpenDialog)fd).FileNames;

                    if (fileNames.Length > 0)
                    {
                        fileName = fileNames[0];
                    }
                    else
                    {
                        fileName = null;
                    }
                }
                else if (fd is IFileSaveDialog)
                {
                    fileName = ((IFileSaveDialog)fd).FileName;
                }
                else
                {
                    throw new InvalidOperationException();
                }

                if (fileName != null)
                {
                    string newDir = Path.GetDirectoryName(fileName);
                    Settings.CurrentUser.SetString(SettingNames.LastFileDialogDirectory, newDir);
                }
                else
                {
                    throw new FileNotFoundException();
                }
            }

            return result;
        }

        private sealed class OurFileDialogUICallbacks
            : IFileDialogUICallbacks
        {
            public FileOverwriteAction ShowOverwritePrompt(IWin32Window owner, string pathName)
            {
                FileOverwriteAction returnVal;

                string title = PdnResources.GetString("SaveAs.OverwriteConfirmation.Title");
                string textFormat = PdnResources.GetString("SaveAs.OverwriteConfirmation.Text.Format");
                string fileName;

                try
                {
                    fileName = Path.GetFileName(pathName);
                }

                catch (Exception)
                {
                    fileName = pathName;
                }

                string text = string.Format(textFormat, fileName);

                DialogResult result = MessageBox.Show(owner, text, title, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);

                switch (result)
                {
                    case DialogResult.Yes:
                        returnVal = FileOverwriteAction.Overwrite;
                        break;

                    case DialogResult.No:
                        returnVal = FileOverwriteAction.Cancel;
                        break;

                    default:
                        throw new InvalidEnumArgumentException();
                }

                return returnVal;
            }

            public bool ShowError(IWin32Window owner, string filePath, Exception ex)
            {
                if (ex is PathTooLongException)
                {
                    string title = PdnInfo.GetBareProductName();
                    string message = PdnResources.GetString("FileDialog.PathTooLongException.Message");

                    MessageBox.Show(owner, message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return true;
                }
                else
                {
                    return false;
                }
            }

            public IFileTransferProgressEvents CreateFileTransferProgressEvents()
            {
                return new OurProgressEvents();
            }
        }

        private sealed class OurProgressEvents
            : IFileTransferProgressEvents
        {
            private TransferProgressDialog progressDialog;
            private ICancelable cancelSink;
            private int itemCount = 0;
            private int itemOrdinal = 0;
            private string itemName = string.Empty;
            private long totalWork;
            private long totalProgress;
            private const int maxPBValue = 200; // granularity of progress bar. 100 means 1%, 200 means 0.5%, etc.
            private bool cancelRequested = false;

            private ManualResetEvent operationEnded = new ManualResetEvent(false);

            public OurProgressEvents()
            {
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1500:VariableNamesShouldNotMatchFieldNames", MessageId = "cancelSink")]
            public void BeginOperation(IWin32Window owner, EventHandler callWhenUIShown, ICancelable cancelSink)
            {
                if (this.progressDialog != null)
                {
                    throw new InvalidOperationException("Operation already in progress");
                }

                this.progressDialog = new TransferProgressDialog();
                this.progressDialog.Text = PdnResources.GetString("DocumentWorkspace.ShowFileDialog.TransferProgress.Title");
                this.progressDialog.Icon = Utility.ImageToIcon(PdnResources.GetImageResource("Icons.MenuFileOpenIcon.png").Reference);
                this.progressDialog.ItemText = PdnResources.GetString("DocumentWorkspace.ShowFileDialog.ItemText.Initializing");
                this.progressDialog.ProgressBar.Style = ProgressBarStyle.Marquee;
                this.progressDialog.ProgressBar.Maximum = maxPBValue;

                this.progressDialog.CancelClicked +=
                    delegate(object sender, EventArgs e)
                    {
                        this.cancelRequested = true;
                        this.cancelSink.RequestCancel();
                        UpdateUI();
                    };

                EventHandler progressDialog_Shown =
                    delegate(object sender, EventArgs e)
                    {
                        callWhenUIShown(this, EventArgs.Empty);
                    };

                this.cancelSink = cancelSink;
                this.itemOrdinal = 0;
                this.cancelRequested = false;
                this.itemName = string.Empty;
                this.itemCount = 0;
                this.itemOrdinal = 0;
                this.totalProgress = 0;
                this.totalWork = 0;

                this.progressDialog.Shown += progressDialog_Shown;
                this.progressDialog.ShowDialog(owner);
                this.progressDialog.Shown -= progressDialog_Shown;

                this.progressDialog.Dispose();
                this.progressDialog = null;
                this.cancelSink = null;
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1500:VariableNamesShouldNotMatchFieldNames", MessageId = "itemCount")]
            public void SetItemCount(int itemCount)
            {
                if (this.progressDialog.InvokeRequired)
                {
                    this.progressDialog.BeginInvoke(new Procedure<int>(SetItemCount), new object[] { itemCount });
                }
                else
                {
                    this.itemCount = itemCount;
                    UpdateUI();
                }
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1500:VariableNamesShouldNotMatchFieldNames", MessageId = "itemOrdinal")]
            public void SetItemOrdinal(int itemOrdinal)
            {
                if (this.progressDialog.InvokeRequired)
                {
                    this.progressDialog.BeginInvoke(new Procedure<int>(SetItemOrdinal), new object[] { itemOrdinal });
                }
                else
                {
                    this.itemOrdinal = itemOrdinal;
                    this.totalWork = 0;
                    this.totalProgress = 0;
                    UpdateUI();
                }
            }

            public void SetItemInfo(string itemInfo)
            {
                if (this.progressDialog.InvokeRequired)
                {
                    this.progressDialog.BeginInvoke(new Procedure<string>(SetItemInfo), new object[] { itemInfo });
                }
                else
                {
                    this.itemName = itemInfo;
                    UpdateUI();
                }
            }

            public void BeginItem()
            {
                if (this.progressDialog.InvokeRequired)
                {
                    this.progressDialog.BeginInvoke(new Procedure(BeginItem), null);
                }
                else
                {
                    this.progressDialog.ProgressBar.Style = ProgressBarStyle.Continuous;
                }
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1500:VariableNamesShouldNotMatchFieldNames", MessageId = "totalWork")]
            public void SetItemWorkTotal(long totalWork)
            {
                if (this.progressDialog.InvokeRequired)
                {
                    this.progressDialog.BeginInvoke(new Procedure<long>(SetItemWorkTotal), new object[] { totalWork });
                }
                else
                {
                    this.totalWork = totalWork;
                    UpdateUI();
                }
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1500:VariableNamesShouldNotMatchFieldNames", MessageId = "totalProgress")]
            public void SetItemWorkProgress(long totalProgress)
            {
                if (this.progressDialog.InvokeRequired)
                {
                    this.progressDialog.BeginInvoke(new Procedure<long>(SetItemWorkProgress), new object[] { totalProgress });
                }
                else
                {
                    this.totalProgress = totalProgress;
                    UpdateUI();
                }
            }

            public void EndItem(WorkItemResult result)
            {
                if (this.progressDialog.InvokeRequired)
                {
                    this.progressDialog.BeginInvoke(new Procedure<WorkItemResult>(EndItem), new object[] { result });
                }
                else
                {
                }
            }

            public void EndOperation(OperationResult result)
            {
                if (this.progressDialog.InvokeRequired)
                {
                    this.progressDialog.BeginInvoke(new Procedure<OperationResult>(EndOperation), new object[] { result });
                }
                else
                {
                    this.progressDialog.Close();
                }
            }

            public WorkItemFailureAction ReportItemFailure(Exception ex)
            {
                if (this.progressDialog.InvokeRequired)
                {
                    object result = this.progressDialog.Invoke(
                        new Function<WorkItemFailureAction, Exception>(ReportItemFailure), 
                        new object[] { ex });

                    return (WorkItemFailureAction)result;
                }
                else
                {
                    WorkItemFailureAction result;
                    result = ShowFileTransferFailedDialog(ex);
                    return result;
                }
            }

            private WorkItemFailureAction ShowFileTransferFailedDialog(Exception ex)
            {
                WorkItemFailureAction result;
                Icon formIcon = this.progressDialog.Icon;

                string formTitle = PdnResources.GetString("DocumentWorkspace.ShowFileDialog.ItemFailureDialog.Title");

                Image taskImage = PdnResources.GetImageResource("Icons.WarningIcon.png").Reference;

                string introTextFormat = PdnResources.GetString("DocumentWorkspace.ShowFileDialog.ItemFailureDialog.IntroText.Format");
                string introText = string.Format(introTextFormat, ex.Message);

                TaskButton retryTB = new TaskButton(
                    PdnResources.GetImageResource("Icons.MenuImageRotate90CWIcon.png").Reference,
                    PdnResources.GetString("DocumentWorkspace.ShowFileDialog.RetryTB.ActionText"),
                    PdnResources.GetString("DocumentWorkspace.ShowFileDialog.RetryTB.ExplanationText"));

                TaskButton skipTB = new TaskButton(
                    PdnResources.GetImageResource("Icons.HistoryFastForwardIcon.png").Reference,
                    PdnResources.GetString("DocumentWorkspace.ShowFileDialog.SkipTB.ActionText"),
                    PdnResources.GetString("DocumentWorkspace.ShowFileDialog.SkipTB.ExplanationText"));

                TaskButton cancelTB = new TaskButton(
                    PdnResources.GetImageResource("Icons.CancelIcon.png").Reference,
                    PdnResources.GetString("DocumentWorkspace.ShowFileDialog.CancelTB.ActionText"),
                    PdnResources.GetString("DocumentWorkspace.ShowFileDialog.CancelTB.ExplanationText"));

                List<TaskButton> taskButtons = new List<TaskButton>();
                taskButtons.Add(retryTB);

                // Only have the Skip button if there is more than 1 item being transferred.
                // If only 1 item is begin transferred, Skip and Cancel are essentially synonymous.
                if (this.itemCount > 1)
                {
                    taskButtons.Add(skipTB);
                }

                taskButtons.Add(cancelTB);

                int width96 = (TaskDialog.DefaultPixelWidth96Dpi * 4) / 3; // 33% wider

                TaskButton clickedTB = TaskDialog.Show(
                    this.progressDialog,
                    formIcon,
                    formTitle,
                    taskImage,
                    true,
                    introText,
                    taskButtons.ToArray(),
                    retryTB,
                    cancelTB,
                    width96);

                if (clickedTB == retryTB)
                {
                    result = WorkItemFailureAction.RetryItem;
                }
                else if (clickedTB == skipTB)
                {
                    result = WorkItemFailureAction.SkipItem;
                }
                else
                {
                    result = WorkItemFailureAction.CancelOperation;
                }

                return result;
            }

            private void UpdateUI()
            {
                int itemCount2 = Math.Max(1, this.itemCount);

                double startValue = (double)this.itemOrdinal / (double)itemCount2;
                double endValue = (double)(this.itemOrdinal + 1) / (double)itemCount2;

                long totalWork2 = Math.Max(1, this.totalWork);
                double lerp = (double)this.totalProgress / (double)totalWork2;

                double newValue = Utility.Lerp(startValue, endValue, lerp);
                int newValueInt = (int)Math.Ceiling(maxPBValue * newValue);

                if (this.cancelRequested)
                {
                    this.progressDialog.CancelEnabled = false;
                    this.progressDialog.ItemText = PdnResources.GetString("DocumentWorkspace.ShowFileDialog.ItemText.Canceling");
                    this.progressDialog.OperationProgress = string.Empty;
                    this.progressDialog.ProgressBar.Style = ProgressBarStyle.Marquee;
                }
                else
                {
                    this.progressDialog.CancelEnabled = true;
                    this.progressDialog.ItemText = this.itemName;
                    string progressFormat = PdnResources.GetString("DocumentWorkspace.ShowFileDialog.ProgressText.Format");
                    string progressText = string.Format(progressFormat, this.itemOrdinal + 1, this.itemCount);
                    this.progressDialog.OperationProgress = progressText;
                    this.progressDialog.ProgressBar.Style = ProgressBarStyle.Continuous;
                    this.progressDialog.ProgressBar.Value = newValueInt;
                }
            }
        }

        public static DialogResult ChooseFile(Control parent, out string fileName)
        {
            return ChooseFile(parent, out fileName, null);
        }

        public static DialogResult ChooseFile(Control parent, out string fileName, string startingDir)
        {
            string[] fileNames;
            DialogResult result = ChooseFiles(parent, out fileNames, false, startingDir);

            if (result == DialogResult.OK)
            {
                fileName = fileNames[0];
            }
            else
            {
                fileName = null;
            }

            return result;
        }

        public static DialogResult ChooseFiles(Control owner, out string[] fileNames, bool multiselect)
        {
            return ChooseFiles(owner, out fileNames, multiselect, null);
        }

        public static DialogResult ChooseFiles(Control owner, out string[] fileNames, bool multiselect, string startingDir)
        {
            FileTypeCollection fileTypes = FileTypes.GetFileTypes();

            using (IFileOpenDialog ofd = SystemLayer.CommonDialogs.CreateFileOpenDialog())
            {
                if (startingDir != null)
                {
                    ofd.InitialDirectory = startingDir;
                }
                else
                {
                    ofd.InitialDirectory = GetDefaultSavePath();
                }

                ofd.CheckFileExists = true;
                ofd.CheckPathExists = true;
                ofd.Multiselect = multiselect;

                ofd.Filter = fileTypes.ToString(true, PdnResources.GetString("FileDialog.Types.AllImages"), false, true);
                ofd.FilterIndex = 0;

                DialogResult result = ShowFileDialog(owner, ofd);

                if (result == DialogResult.OK)
                {
                    fileNames = ofd.FileNames;
                }
                else
                {
                    fileNames = new string[0];
                }

                return result;
            }
        }

        /// <summary>
        /// Use this to get a save config token. You should already know the filename and file type.
        /// An existing save config token is optional and will be used to pre-populate the config dialog.
        /// </summary>
        /// <param name="fileType"></param>
        /// <param name="saveConfigToken"></param>
        /// <param name="newSaveConfigToken"></param>
        /// <returns>false if the user cancelled, otherwise true</returns>
        private bool GetSaveConfigToken(
            FileType currentFileType, 
            SaveConfigToken currentSaveConfigToken, 
            out SaveConfigToken newSaveConfigToken, 
            Surface saveScratchSurface)
        {
            if (currentFileType.SupportsConfiguration)
            {
                using (SaveConfigDialog scd = new SaveConfigDialog())
                {
                    scd.ScratchSurface = saveScratchSurface;

                    ProgressEventHandler peh = delegate(object sender, ProgressEventArgs e)
                    {
                        if (e.Percent < 0 || e.Percent >= 100)
                        {
                            AppWorkspace.Widgets.StatusBarProgress.ResetProgressStatusBar();
                            AppWorkspace.Widgets.StatusBarProgress.EraseProgressStatusBar();
                        }
                        else
                        {
                            AppWorkspace.Widgets.StatusBarProgress.SetProgressStatusBar(e.Percent);
                        }
                    };

                    //if (currentFileType.SavesWithProgress)
                    {
                        scd.Progress += peh;
                    }

                    scd.Document = Document;
                    scd.FileType = currentFileType;

                    SaveConfigToken token = currentFileType.GetLastSaveConfigToken();
                    if (currentSaveConfigToken != null &&
                        token.GetType() == currentSaveConfigToken.GetType())
                    {
                        scd.SaveConfigToken = currentSaveConfigToken;
                    }

                    scd.EnableInstanceOpacity = false;

                    // show configuration/preview dialog
                    DialogResult dr = scd.ShowDialog(this);

                    //if (currentFileType.SavesWithProgress)
                    {
                        scd.Progress -= peh;
                        AppWorkspace.Widgets.StatusBarProgress.ResetProgressStatusBar();
                        AppWorkspace.Widgets.StatusBarProgress.EraseProgressStatusBar();
                    }

                    if (dr == DialogResult.OK)
                    {
                        newSaveConfigToken = scd.SaveConfigToken;
                        return true;
                    }
                    else
                    {
                        newSaveConfigToken = null;
                        return false;
                    }
                }
            }
            else
            {
                newSaveConfigToken = currentFileType.GetLastSaveConfigToken();
                return true;
            }
        }

        /// <summary>
        /// Used to set the file name, file type, and save config token
        /// </summary>
        /// <param name="newFileName"></param>
        /// <param name="newFileType"></param>
        /// <param name="newSaveConfigToken"></param>
        /// <returns>true if the user clicked through and accepted, or false if they cancelled at any point</returns>
        private bool DoSaveAsDialog(
            out string newFileName, 
            out FileType newFileType, 
            out SaveConfigToken newSaveConfigToken, 
            Surface saveScratchSurface)
        {
            FileTypeCollection fileTypes = FileTypes.GetFileTypes();

            using (IFileSaveDialog sfd = SystemLayer.CommonDialogs.CreateFileSaveDialog())
            {
                sfd.AddExtension = true;
                sfd.CheckPathExists = true;
                sfd.OverwritePrompt = true;
                string filter = fileTypes.ToString(false, null, true, false);
                sfd.Filter = filter;

                string localFileName;
                FileType localFileType;
                SaveConfigToken localSaveConfigToken;
                GetDocumentSaveOptions(out localFileName, out localFileType, out localSaveConfigToken);

                if (Document.Layers.Count > 1 && 
                    localFileType != null && 
                    !localFileType.SupportsLayers)
                {
                    localFileType = null;
                }

                if (localFileType == null)
                {
                    if (Document.Layers.Count == 1)
                    {
                        localFileType = PdnFileTypes.Png;
                    }
                    else
                    {
                        localFileType = PdnFileTypes.Pdn;
                    }

                    localFileName = Path.ChangeExtension(localFileName, localFileType.DefaultExtension);
                }

                if (localFileName == null)
                {
                    string name = GetDefaultSaveName();
                    string newName = Path.ChangeExtension(name, localFileType.DefaultExtension);
                    localFileName = Path.Combine(GetDefaultSavePath(), newName);
                }

                // If the filename is only an extension (i.e. ".lmnop") then we must treat it specially
                string fileNameOnly = Path.GetFileName(localFileName);
                if (fileNameOnly.Length >= 1 && fileNameOnly[0] == '.')
                {
                    sfd.FileName = localFileName;
                }
                else
                {
                    sfd.FileName = Path.ChangeExtension(localFileName, null);
                }

                sfd.FilterIndex = 1 + fileTypes.IndexOfFileType(localFileType);
                sfd.InitialDirectory = Path.GetDirectoryName(localFileName);
                sfd.Title = PdnResources.GetString("SaveAsDialog.Title");

                DialogResult dr1 = ShowFileDialog(this, sfd);
                bool result;

                if (dr1 != DialogResult.OK)
                {
                    result = false;
                }
                else
                {
                    localFileName = sfd.FileName;
                    FileType fileType2 = fileTypes[sfd.FilterIndex - 1];
                    result = GetSaveConfigToken(fileType2, localSaveConfigToken, out localSaveConfigToken, saveScratchSurface);
                    localFileType = fileType2;
                }

                if (result)
                {
                    newFileName = localFileName;
                    newFileType = localFileType;
                    newSaveConfigToken = localSaveConfigToken;
                }
                else
                {
                    newFileName = null;
                    newFileType = null;
                    newSaveConfigToken = null;
                }

                return result;
            }
        }

        /// <summary>
        /// Warns the user that we need to flatten the image.
        /// </summary>
        /// <returns>Returns DialogResult.Yes if they want to proceed or DialogResult.No if they don't.</returns>
        private DialogResult WarnAboutFlattening()
        {
            Icon formIcon = Utility.ImageToIcon(PdnResources.GetImageResource("Icons.MenuFileSaveIcon.png").Reference);
            string formTitle = PdnResources.GetString("WarnAboutFlattening.Title");

            string introText = PdnResources.GetString("WarnAboutFlattening.IntroText");
            Image taskImage = null;

            TaskButton flattenTB = new TaskButton(
                PdnResources.GetImageResource("Icons.MenuImageFlattenIcon.png").Reference,
                PdnResources.GetString("WarnAboutFlattening.FlattenTB.ActionText"),
                PdnResources.GetString("WarnAboutFlattening.FlattenTB.ExplanationText"));

            TaskButton cancelTB = new TaskButton(
                TaskButton.Cancel.Image,
                PdnResources.GetString("WarnAboutFlattening.CancelTB.ActionText"),
                PdnResources.GetString("WarnAboutFlattening.CancelTB.ExplanationText"));

            TaskButton clickedTB = TaskDialog.Show(
                AppWorkspace,
                formIcon,
                formTitle,
                taskImage,
                true,
                introText,
                new TaskButton[] { flattenTB, cancelTB },
                flattenTB,
                cancelTB,
                (TaskDialog.DefaultPixelWidth96Dpi * 5) / 4);

            if (clickedTB == flattenTB)
            {
                return DialogResult.Yes;
            }
            else
            {
                return DialogResult.No;
            }
        }
        
        private static string GetDefaultSaveName()
        {
            return PdnResources.GetString("Untitled.FriendlyName");
        }

        private static string GetDefaultSavePath()
        {
            string myPics;

            try
            {
                myPics = Shell.GetVirtualPath(VirtualFolderName.UserPictures, false);
                DirectoryInfo dirInfo = new DirectoryInfo(myPics); // validate
            }

            catch (Exception)
            {
                myPics = "";
            }

            string dir = Settings.CurrentUser.GetString(SettingNames.LastFileDialogDirectory, null);

            if (dir == null)
            {
                dir = myPics;
            }
            else
            {
                try
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(dir);

                    if (!dirInfo.Exists)
                    {
                        dir = myPics;
                    }
                }

                catch (Exception)
                {
                    dir = myPics;
                }
            }

            return dir;
        }

        public bool DoSave()
        {
            return DoSave(false);
        }

        /// <summary>
        /// Does the dirty work for a File->Save operation. If any of the "Save Options" in the
        /// DocumentWorkspace are null, this will call DoSaveAs(). If the image has more than 1
        /// layer but the file type they want to save with does not support layers, then it will
        /// ask the user about flattening the image.
        /// </summary>
        /// <param name="tryToFlatten">
        /// If true, will ask the user about flattening if the workspace's saveFileType does not 
        /// support layers and the image has more than 1 layer.
        /// If false, then DoSaveAs will be called and the fileType will be prepopulated with
        /// the .PDN type.
        /// </param>
        /// <returns><b>true</b> if the file was saved, <b>false</b> if the user cancelled</returns>
        protected bool DoSave(bool tryToFlatten)
        {
            using (new PushNullToolMode(this))
            {
                string newFileName;
                FileType newFileType;
                SaveConfigToken newSaveConfigToken;

                GetDocumentSaveOptions(out newFileName, out newFileType, out newSaveConfigToken);

                // if they haven't specified a filename, then revert to "Save As" behavior
                if (newFileName == null)
                {
                    return DoSaveAs();
                }

                // if we have a filename but no file type, try to infer the file type
                if (newFileType == null)
                {
                    FileTypeCollection fileTypes = FileTypes.GetFileTypes();
                    string ext = Path.GetExtension(newFileName);
                    int index = fileTypes.IndexOfExtension(ext);
                    FileType inferredFileType = fileTypes[index];
                    newFileType = inferredFileType;
                }

                // if the image has more than 1 layer but is saving with a file type that
                // does not support layers, then we must ask them if we may flatten the
                // image first
                if (Document.Layers.Count > 1 && !newFileType.SupportsLayers)
                {
                    if (!tryToFlatten)
                    {
                        return DoSaveAs();
                    }
                    else
                    {
                        DialogResult dr = WarnAboutFlattening();

                        if (dr == DialogResult.Yes)
                        {
                            ExecuteFunction(new FlattenFunction());
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

                // get the configuration!
                if (newSaveConfigToken == null)
                {
                    Surface scratch = BorrowScratchSurface(this.GetType().Name + ".DoSave() calling GetSaveConfigToken()");

                    bool result;
                    try
                    {
                        result = GetSaveConfigToken(newFileType, newSaveConfigToken, out newSaveConfigToken, scratch);
                    }

                    finally
                    {
                        ReturnScratchSurface(scratch);
                    }

                    if (!result)
                    {
                        return false;
                    }
                }

                // At this point fileName, fileType, and saveConfigToken must all be non-null

                // if the document supports custom headers, embed a thumbnail in there
                if (newFileType.SupportsCustomHeaders)
                {
                    using (new WaitCursorChanger(this))
                    {
                        Utility.GCFullCollect();
                        const int maxDim = 256;

                        Surface thumb;
                        Surface flattened = BorrowScratchSurface(this.GetType().Name + ".DoSave() preparing embedded thumbnail");

                        try
                        {
                            Document.Flatten(flattened);

                            if (Document.Width > maxDim || Document.Height > maxDim)
                            {
                                int width;
                                int height;

                                if (Document.Width > Document.Height)
                                {
                                    width = maxDim;
                                    height = (Document.Height * maxDim) / Document.Width;
                                }
                                else
                                {
                                    height = maxDim;
                                    width = (Document.Width * maxDim) / Document.Height;
                                }

                                int thumbWidth = Math.Max(1, width);
                                int thumbHeight = Math.Max(1, height);

                                thumb = new Surface(thumbWidth, thumbHeight);
                                thumb.SuperSamplingFitSurface(flattened);
                            }
                            else
                            {
                                thumb = new Surface(flattened.Size);
                                thumb.CopySurface(flattened);
                            }
                        }

                        finally
                        {
                            ReturnScratchSurface(flattened);
                        }

                        Document thumbDoc = new Document(thumb.Width, thumb.Height);
                        BitmapLayer thumbLayer = new BitmapLayer(thumb);
                        BitmapLayer backLayer = new BitmapLayer(thumb.Width, thumb.Height);
                        backLayer.Surface.Clear(ColorBgra.Transparent);
                        thumb.Dispose();
                        thumbDoc.Layers.Add(backLayer);
                        thumbDoc.Layers.Add(thumbLayer);
                        MemoryStream thumbPng = new MemoryStream();
                        PropertyBasedSaveConfigToken pngToken = PdnFileTypes.Png.CreateDefaultSaveConfigToken();
                        PdnFileTypes.Png.Save(thumbDoc, thumbPng, pngToken, null, null, false);
                        byte[] thumbBytes = thumbPng.ToArray();

                        string thumbString = Convert.ToBase64String(thumbBytes, Base64FormattingOptions.None);
                        thumbDoc.Dispose();

                        string thumbXml = "<thumb png=\"" + thumbString + "\" />";
                        Document.CustomHeaders = thumbXml;
                    }
                }

                // save!
                bool success = false;
                Stream stream = null;

                try
                {
                    stream = (Stream)new FileStream(newFileName, FileMode.Create, FileAccess.Write);

                    using (new WaitCursorChanger(this))
                    {
                        Utility.GCFullCollect();

                        SaveProgressDialog sd = new SaveProgressDialog(this);
                        Surface scratch = BorrowScratchSurface(this.GetType().Name + ".DoSave() handing off scratch surface to SaveProgressDialog.Save()");

                        try
                        {
                            sd.Save(stream, Document, newFileType, newSaveConfigToken, scratch);
                        }

                        finally
                        {
                            ReturnScratchSurface(scratch);
                        }

                        success = true;

                        this.lastSaveTime = DateTime.Now;

                        stream.Close();
                        stream = null;
                    }
                }

                catch (UnauthorizedAccessException)
                {
                    Utility.ErrorBox(this, PdnResources.GetString("SaveImage.Error.UnauthorizedAccessException"));
                }

                catch (SecurityException)
                {
                    Utility.ErrorBox(this, PdnResources.GetString("SaveImage.Error.SecurityException"));
                }

                catch (DirectoryNotFoundException)
                {
                    Utility.ErrorBox(this, PdnResources.GetString("SaveImage.Error.DirectoryNotFoundException"));
                }

                catch (IOException)
                {
                    Utility.ErrorBox(this, PdnResources.GetString("SaveImage.Error.IOException"));
                }

                catch (OutOfMemoryException)
                {
                    Utility.ErrorBox(this, PdnResources.GetString("SaveImage.Error.OutOfMemoryException"));
                }

#if !DEBUG
                catch (Exception)
                {
                    Utility.ErrorBox(this, PdnResources.GetString("SaveImage.Error.Exception"));
                }
#endif

                finally
                {
                    if (stream != null)
                    {
                        stream.Close();
                        stream = null;
                    }
                }

                if (success)
                {
                    Shell.AddToRecentDocumentsList(newFileName);
                }
                else
                {
                    return false;
                }

                // reset the dirty bit so they won't be asked to save on quitting
                Document.Dirty = false;

                // some misc. book keeping ...
                AddToMruList();

                // and finally, shout happiness by way of ...
                return true;
            }
        }

        /// <summary>
        /// Does the grunt work to do a File->Save As operation.
        /// </summary>
        /// <returns><b>true</b> if the file was saved correctly, <b>false</b> if the user cancelled</returns>
        public bool DoSaveAs()
        {
            using (new PushNullToolMode(this))
            {
                string newFileName;
                FileType newFileType;
                SaveConfigToken newSaveConfigToken;

                Surface scratch = BorrowScratchSurface(this.GetType() + ".DoSaveAs() handing off scratch surface to DoSaveAsDialog()");

                bool result;
                try
                {
                    result = DoSaveAsDialog(out newFileName, out newFileType, out newSaveConfigToken, scratch);
                }

                finally
                {
                    ReturnScratchSurface(scratch);
                }

                if (result)
                {
                    string oldFileName;
                    FileType oldFileType;
                    SaveConfigToken oldSaveConfigToken;

                    GetDocumentSaveOptions(out oldFileName, out oldFileType, out oldSaveConfigToken);
                    SetDocumentSaveOptions(newFileName, newFileType, newSaveConfigToken);

                    bool result2 = DoSave(true);

                    if (!result2)
                    {
                        SetDocumentSaveOptions(oldFileName, oldFileType, oldSaveConfigToken);
                    }

                    return result2;
                }
                else
                {
                    return false;
                }
            }
        }

        public static Document LoadDocument(Control owner, string fileName, out FileType fileTypeResult, ProgressEventHandler progressCallback)
        {
            FileTypeCollection fileTypes;
            int ftIndex;
            FileType fileType;

            fileTypeResult = null;

            try
            {
                fileTypes = FileTypes.GetFileTypes();
                ftIndex = fileTypes.IndexOfExtension(Path.GetExtension(fileName));

                if (ftIndex == -1)
                {
                    Utility.ErrorBox(owner, PdnResources.GetString("LoadImage.Error.ImageTypeNotRecognized"));
                    return null;
                }

                fileType = fileTypes[ftIndex];
                fileTypeResult = fileType;
            }

            catch (ArgumentException)
            {
                string format = PdnResources.GetString("LoadImage.Error.InvalidFileName.Format");
                string error = string.Format(format, fileName);
                Utility.ErrorBox(owner, error);
                return null;
            }

            Document document = null;

            using (new WaitCursorChanger(owner))
            {
                Utility.GCFullCollect();
                Stream stream = null;

                try
                {
                    try
                    {
                        stream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                        long totalBytes = 0;

                        SiphonStream siphonStream = new SiphonStream(stream);

                        IOEventHandler ioEventHandler = null;
                        ioEventHandler =
                            delegate(object sender, IOEventArgs e)
                            {
                                if (progressCallback != null)
                                {
                                    totalBytes += (long)e.Count;
                                    double percent = Utility.Clamp(100.0 * ((double)totalBytes / (double)siphonStream.Length), 0, 100);
                                    progressCallback(null, new ProgressEventArgs(percent));
                                }
                            };

                        siphonStream.IOFinished += ioEventHandler;

                        using (new WaitCursorChanger(owner))
                        {
                            document = fileType.Load(siphonStream);

                            if (progressCallback != null)
                            {
                                progressCallback(null, new ProgressEventArgs(100.0));
                            }
                        }

                        siphonStream.IOFinished -= ioEventHandler;
                        siphonStream.Close();
                    }

                    catch (WorkerThreadException ex)
                    {
                        Type innerExType = ex.InnerException.GetType();
                        ConstructorInfo ci = innerExType.GetConstructor(new Type[] { typeof(string), typeof(Exception) });

                        if (ci == null)
                        {
                            throw;
                        }
                        else
                        {
                            Exception ex2 = (Exception)ci.Invoke(new object[] { "Worker thread threw an exception of this type", ex.InnerException });
                            throw ex2;
                        }
                    }
                }

                catch (ArgumentException)
                {
                    if (fileName.Length == 0)
                    {
                        Utility.ErrorBox(owner, PdnResources.GetString("LoadImage.Error.BlankFileName"));
                    }
                    else
                    {
                        Utility.ErrorBox(owner, PdnResources.GetString("LoadImage.Error.ArgumentException"));
                    }
                }

                catch (UnauthorizedAccessException)
                {
                    Utility.ErrorBox(owner, PdnResources.GetString("LoadImage.Error.UnauthorizedAccessException"));
                }

                catch (SecurityException)
                {
                    Utility.ErrorBox(owner, PdnResources.GetString("LoadImage.Error.SecurityException"));
                }

                catch (FileNotFoundException)
                {
                    Utility.ErrorBox(owner, PdnResources.GetString("LoadImage.Error.FileNotFoundException"));
                }

                catch (DirectoryNotFoundException)
                {
                    Utility.ErrorBox(owner, PdnResources.GetString("LoadImage.Error.DirectoryNotFoundException"));
                }

                catch (PathTooLongException)
                {
                    Utility.ErrorBox(owner, PdnResources.GetString("LoadImage.Error.PathTooLongException"));
                }

                catch (IOException)
                {
                    Utility.ErrorBox(owner, PdnResources.GetString("LoadImage.Error.IOException"));
                }

                catch (SerializationException)
                {
                    Utility.ErrorBox(owner, PdnResources.GetString("LoadImage.Error.SerializationException"));
                }

                catch (OutOfMemoryException)
                {
                    Utility.ErrorBox(owner, PdnResources.GetString("LoadImage.Error.OutOfMemoryException"));
                }

                catch (Exception)
                {
                    Utility.ErrorBox(owner, PdnResources.GetString("LoadImage.Error.Exception"));
                }

                finally
                {
                    if (stream != null)
                    {
                        stream.Close();
                        stream = null;
                    }
                }
            }

            return document;
        }

        public Surface RenderThumbnail(int maxEdgeLength, bool highQuality, bool forceUpToDate)
        {
            if (Document == null)
            {
                Surface ret = new Surface(maxEdgeLength, maxEdgeLength);
                ret.Clear(ColorBgra.Transparent);
                return ret;
            }

            Size thumbSize = Utility.ComputeThumbnailSize(Document.Size, maxEdgeLength);
            Surface thumb = new Surface(thumbSize);
            thumb.Clear(ColorBgra.Transparent);

            RenderCompositionTo(thumb, highQuality, forceUpToDate);

            return thumb;
        }

        Surface IThumbnailProvider.RenderThumbnail(int maxEdgeLength)
        {
            return RenderThumbnail(maxEdgeLength, true, false);
        }
    }
}
