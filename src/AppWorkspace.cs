/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.Actions;
using PaintDotNet.Effects;
using PaintDotNet.HistoryFunctions;
using PaintDotNet.HistoryMementos;
using PaintDotNet.SystemLayer;
using PaintDotNet.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Text;
using System.Windows.Forms;

namespace PaintDotNet
{
    internal class AppWorkspace
        : UserControl,
          ISnapObstacleHost
    {
        private readonly string cursorInfoStatusBarFormat = PdnResources.GetString("StatusBar.CursorInfo.Format");
        private readonly string imageInfoStatusBarFormat = PdnResources.GetString("StatusBar.Size.Format");

        private Type defaultToolTypeChoice;

        private Type globalToolTypeChoice = null;
        private bool globalRulersChoice = false;

        private AppEnvironment appEnvironment;
        private DocumentWorkspace activeDocumentWorkspace;

        // if a new workspace is added, and this workspace is not dirty, then it will be removed. 
        // This keeps track of the last workspace added via CreateBlankDocumentInNewWorkspace (if 
        // true was passed for its 2nd parameter)
        private DocumentWorkspace initialWorkspace; 

        private List<DocumentWorkspace> documentWorkspaces = new List<DocumentWorkspace>();
        private WorkspaceWidgets widgets;

        private Panel workspacePanel;
        private PdnToolBar toolBar;
        private PdnStatusBar statusBar;

        private ToolsForm mainToolBarForm;
        private LayerForm layerForm;
        private HistoryForm historyForm;
        private ColorsForm colorsForm;

        private MostRecentFiles mostRecentFiles = null;
        private const int defaultMostRecentFilesMax = 8;

        private SnapObstacleController snapObstacle;
        private bool addedToSnapManager = false;
        private int ignoreUpdateSnapObstacle = 0;
        private int suspendThumbnailUpdates = 0;

        public void CheckForUpdates()
        {
            this.toolBar.MainMenu.CheckForUpdates();
        }

        public IDisposable SuspendThumbnailUpdates()
        {
            CallbackOnDispose resumeFn = new CallbackOnDispose(ResumeThumbnailUpdates);

            ++this.suspendThumbnailUpdates;

            if (this.suspendThumbnailUpdates == 1)
            {
                Widgets.DocumentStrip.SuspendThumbnailUpdates();
                Widgets.LayerControl.SuspendLayerPreviewUpdates();
            }

            return resumeFn;
        }

        private void ResumeThumbnailUpdates()
        {
            --this.suspendThumbnailUpdates;

            if (this.suspendThumbnailUpdates == 0)
            {
                Widgets.DocumentStrip.ResumeThumbnailUpdates();
                Widgets.LayerControl.ResumeLayerPreviewUpdates();
            }
        }

        public Type DefaultToolType
        {
            get
            {
                return this.defaultToolTypeChoice;
            }

            set
            {
                this.defaultToolTypeChoice = value;
                Settings.CurrentUser.SetString(SettingNames.DefaultToolTypeName, value.Name);
            }
        }

        public Type GlobalToolTypeChoice
        {
            get
            {
                return this.globalToolTypeChoice;
            }

            set
            {
                this.globalToolTypeChoice = value;

                if (ActiveDocumentWorkspace != null)
                {
                    ActiveDocumentWorkspace.SetToolFromType(value);
                }
            }
        }

        public DocumentWorkspace InitialWorkspace
        {
            set
            {
                this.initialWorkspace = value;
            }
        }

        public event EventHandler RulersEnabledChanged;
        protected virtual void OnRulersEnabledChanged()
        {
            if (RulersEnabledChanged != null)
            {
                RulersEnabledChanged(this, EventArgs.Empty);
            }
        }

        public bool RulersEnabled
        {
            get
            {
                return this.globalRulersChoice;
            }

            set
            {
                if (this.globalRulersChoice != value)
                {
                    this.globalRulersChoice = value;

                    if (ActiveDocumentWorkspace != null)
                    {
                        ActiveDocumentWorkspace.RulersEnabled = value;
                    }

                    OnRulersEnabledChanged();
                }
            }
        }

        private void DocumentWorkspace_DrawGridChanged(object sender, EventArgs e)
        {
            DrawGrid = this.activeDocumentWorkspace.DrawGrid;
        }

        private void ViewConfigStrip_DrawGridChanged(object sender, EventArgs e)
        {
            DrawGrid = ((ViewConfigStrip)sender).DrawGrid;
        }

        private bool DrawGrid
        {
            get
            {
                return this.Widgets.ViewConfigStrip.DrawGrid;
            }

            set
            {
                if (this.Widgets.ViewConfigStrip.DrawGrid != value)
                {
                    this.Widgets.ViewConfigStrip.DrawGrid = value;
                }

                if (this.activeDocumentWorkspace != null && this.activeDocumentWorkspace.DrawGrid != value)
                {
                    this.activeDocumentWorkspace.DrawGrid = value;
                }

                Settings.CurrentUser.SetBoolean(SettingNames.DrawGrid, this.DrawGrid);
            }
        }

        public event EventHandler UnitsChanged;
        protected virtual void OnUnitsChanged()
        {
            if (UnitsChanged != null)
            {
                UnitsChanged(this, EventArgs.Empty);
            }
        }

        public MeasurementUnit Units
        {
            get
            {
                return this.widgets.ViewConfigStrip.Units;
            }

            set
            {
                this.widgets.ViewConfigStrip.Units = value;
            }
        }

        public SnapObstacle SnapObstacle
        {
            get
            {
                if (this.snapObstacle == null)
                {
                    // HACK: for some reason retrieving the ClientRectangle can raise a VisibleChanged event
                    //       so we initially pass in Rectangle.Empty for the rectangle bounds
                    this.snapObstacle = new SnapObstacleController(
                        this.Name,
                        Rectangle.Empty,
                        SnapRegion.Interior,
                        true);

                    this.snapObstacle.EnableSave = false;

                    PdnBaseForm pdbForm = FindForm() as PdnBaseForm;
                    pdbForm.Moving += new MovingEventHandler(ParentForm_Moving);
                    pdbForm.Move += new EventHandler(ParentForm_Move);
                    pdbForm.ResizeEnd += new EventHandler(ParentForm_ResizeEnd);
                    pdbForm.Layout += new LayoutEventHandler(ParentForm_Layout);
                    pdbForm.SizeChanged += new EventHandler(ParentForm_SizeChanged);

                    UpdateSnapObstacle();
                }

                return this.snapObstacle;
            }
        }

        private void ParentForm_Move(object sender, EventArgs e)
        {
            UpdateSnapObstacle();
        }

        private void ParentForm_SizeChanged(object sender, EventArgs e)
        {
            UpdateSnapObstacle();
        }

        private void ParentForm_Layout(object sender, LayoutEventArgs e)
        {
            UpdateSnapObstacle();
        }

        private void ParentForm_ResizeEnd(object sender, EventArgs e)
        {
            UpdateSnapObstacle();
        }

        private void ParentForm_Moving(object sender, MovingEventArgs e)
        {
            UpdateSnapObstacle();
        }

        private void SuspendUpdateSnapObstacle()
        {
            ++this.ignoreUpdateSnapObstacle;
        }

        private void ResumeUpdateSnapObstacle()
        {
            --this.ignoreUpdateSnapObstacle;
        }

        private void UpdateSnapObstacle()
        {
            if (this.ignoreUpdateSnapObstacle > 0)
            {
                return;
            }

            if (this.snapObstacle == null)
            {
                return;
            }

            if (!this.addedToSnapManager)
            {
                SnapManager sm = SnapManager.FindMySnapManager(this);

                if (sm != null)
                {
                    SnapObstacle so = this.SnapObstacle;

                    if (!this.addedToSnapManager)
                    {
                        sm.AddSnapObstacle(this.SnapObstacle);
                        this.addedToSnapManager = true;

                        FindForm().Shown += new EventHandler(AppWorkspace_Shown);
                    }
                }
            }

            if (this.snapObstacle != null)
            {
                Rectangle clientRect;

                if (ActiveDocumentWorkspace != null)
                {
                    clientRect = ActiveDocumentWorkspace.VisibleViewRectangle;
                }
                else
                {
                    clientRect = this.workspacePanel.ClientRectangle;
                }

                Rectangle screenRect = this.workspacePanel.RectangleToScreen(clientRect);
                this.snapObstacle.SetBounds(screenRect);
                this.snapObstacle.Enabled = this.Visible && this.Enabled;
            }
        }

        private void AppWorkspace_Shown(object sender, EventArgs e)
        {
            UpdateSnapObstacle();
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            UpdateSnapObstacle();
            base.OnLayout(levent);
        }

        protected override void OnLocationChanged(EventArgs e)
        {
            UpdateSnapObstacle();
            base.OnLocationChanged(e);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            UpdateSnapObstacle();
            base.OnSizeChanged(e);
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            UpdateSnapObstacle();
            base.OnEnabledChanged(e);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            UpdateSnapObstacle();
            base.OnVisibleChanged(e);
        }

        public void ResetFloatingForms()
        {
            ResetFloatingForm(Widgets.ToolsForm);
            ResetFloatingForm(Widgets.HistoryForm);
            ResetFloatingForm(Widgets.LayerForm);
            ResetFloatingForm(Widgets.ColorsForm);
        }

        public void ResetFloatingForm(FloatingToolForm ftf)
        {
            SnapManager sm = SnapManager.FindMySnapManager(this);

            if (ftf == Widgets.ToolsForm)
            {
                sm.ParkObstacle(Widgets.ToolsForm, this, HorizontalSnapEdge.Top, VerticalSnapEdge.Left);
            }
            else if (ftf == Widgets.HistoryForm)
            {
                sm.ParkObstacle(Widgets.HistoryForm, this, HorizontalSnapEdge.Top, VerticalSnapEdge.Right);
            }
            else if (ftf == Widgets.LayerForm)
            {
                sm.ParkObstacle(Widgets.LayerForm, this, HorizontalSnapEdge.Bottom, VerticalSnapEdge.Right);
            }
            else if (ftf == Widgets.ColorsForm)
            {
                sm.ParkObstacle(Widgets.ColorsForm, this, HorizontalSnapEdge.Bottom, VerticalSnapEdge.Left);
            }
            else
            {
                throw new ArgumentException();
            }
        }

        private Set<Triple<Assembly, Type, Exception>> effectLoadErrors = new Set<Triple<Assembly, Type, Exception>>();

        public void ReportEffectLoadError(Triple<Assembly, Type, Exception> error)
        {
            lock (this.effectLoadErrors)
            {
                if (!this.effectLoadErrors.Contains(error))
                {
                    this.effectLoadErrors.Add(error);
                }
            }
        }

        public static string GetLocalizedEffectErrorMessage(Assembly assembly, Type type, Exception exception)
        {
            IPluginSupportInfo supportInfo;
            string typeName;

            if (type != null)
            {
                typeName = type.FullName;
                supportInfo = PluginSupportInfo.GetPluginSupportInfo(type);
            }
            else if (exception is TypeLoadException)
            {
                TypeLoadException asTlex = exception as TypeLoadException;
                typeName = asTlex.TypeName;
                supportInfo = PluginSupportInfo.GetPluginSupportInfo(assembly);
            }
            else
            {
                supportInfo = PluginSupportInfo.GetPluginSupportInfo(assembly);
                typeName = null;
            }

            return GetLocalizedEffectErrorMessage(assembly, typeName, supportInfo, exception);
        }

        public static string GetLocalizedEffectErrorMessage(Assembly assembly, string typeName, Exception exception)
        {
            IPluginSupportInfo supportInfo = PluginSupportInfo.GetPluginSupportInfo(assembly);
            return GetLocalizedEffectErrorMessage(assembly, typeName, supportInfo, exception);
        }

        private static string GetLocalizedEffectErrorMessage(Assembly assembly, string typeName, IPluginSupportInfo supportInfo, Exception exception)
        {
            string fileName = assembly.Location;
            string shortErrorFormat = PdnResources.GetString("EffectErrorMessage.ShortFormat");
            string fullErrorFormat = PdnResources.GetString("EffectErrorMessage.FullFormat");
            string notSuppliedText = PdnResources.GetString("EffectErrorMessage.InfoNotSupplied");

            string errorText;

            if (supportInfo == null)
            {
                errorText = string.Format(
                    shortErrorFormat,
                    fileName ?? notSuppliedText,
                    typeName ?? notSuppliedText,
                    exception.ToString());
            }
            else
            {
                errorText = string.Format(
                    fullErrorFormat,
                    fileName ?? notSuppliedText,
                    typeName ?? supportInfo.DisplayName ?? notSuppliedText,
                    (supportInfo.Version ?? new Version()).ToString(),
                    supportInfo.Author ?? notSuppliedText,
                    supportInfo.Copyright ?? notSuppliedText,
                    (supportInfo.WebsiteUri == null ? notSuppliedText : supportInfo.WebsiteUri.ToString()),
                    exception.ToString());
            }

            return errorText;
        }

        public IList<Triple<Assembly, Type, Exception>> GetEffectLoadErrors()
        {
            return this.effectLoadErrors.ToArray();
        }

        public void RunEffect(Type effectType)
        {
            // TODO: this is kind of a hack
            this.toolBar.MainMenu.RunEffect(effectType);
        }

        public PdnToolBar ToolBar
        {
            get
            {
                return this.toolBar;
            }
        }

        private ImageResource FileNewIcon
        {
            get
            {
                return PdnResources.GetImageResource("Icons.MenuFileNewIcon.png");
            }
        }

        private ImageResource ImageFromDiskIcon
        {
            get
            {
                return PdnResources.GetImageResource("Icons.ImageFromDiskIcon.png");
            }
        }

        public MostRecentFiles MostRecentFiles
        {
            get
            {
                if (this.mostRecentFiles == null)
                {
                    this.mostRecentFiles = new MostRecentFiles(defaultMostRecentFilesMax);
                }

                return this.mostRecentFiles;
            }
        }

        private void DocumentWorkspace_DocumentChanging(object sender, EventArgs<Document> e)
        {
            UI.SuspendControlPainting(this);
        }

        private void DocumentWorkspace_DocumentChanged(object sender, EventArgs e)
        {
            UpdateDocInfoInStatusBar();

            UI.ResumeControlPainting(this);
            Invalidate(true);
        }

        private void CoordinatesToStrings(int x, int y, out string xString, out string yString, out string unitsString)
        {
            this.activeDocumentWorkspace.Document.CoordinatesToStrings(this.Units, x, y, out xString, out yString, out unitsString);
        }

        private void UpdateCursorInfoInStatusBar(int cursorX, int cursorY)
        {
            SuspendLayout();

            if (this.activeDocumentWorkspace == null ||
                this.activeDocumentWorkspace.Document == null)
            {
                this.statusBar.CursorInfoText = string.Empty;
            }
            else
            {
                string xString;
                string yString;
                string units;

                CoordinatesToStrings(cursorX, cursorY, out xString, out yString, out units);

                string cursorText = string.Format(
                    CultureInfo.InvariantCulture,
                    this.cursorInfoStatusBarFormat,
                    xString,
                    units,
                    yString,
                    units);

                this.statusBar.CursorInfoText = cursorText;
            }

            ResumeLayout(false);
        }

        private void UpdateDocInfoInStatusBar()
        {
            if (this.activeDocumentWorkspace == null ||
                this.activeDocumentWorkspace.Document == null)
            {
                this.statusBar.ImageInfoStatusText = string.Empty;
            }
            else if (this.activeDocumentWorkspace != null &&
                     this.activeDocumentWorkspace.Document != null)
            {
                string widthString;
                string heightString;
                string units;

                CoordinatesToStrings(
                    this.activeDocumentWorkspace.Document.Width,
                    this.activeDocumentWorkspace.Document.Height,
                    out widthString,
                    out heightString,
                    out units);

                string imageText = string.Format(
                    CultureInfo.InvariantCulture,
                    this.imageInfoStatusBarFormat,
                    widthString,
                    units,
                    heightString,
                    units);

                this.statusBar.ImageInfoStatusText = imageText;
            }
        }

        [Browsable(false)]
        public WorkspaceWidgets Widgets
        {
            get
            {
                return this.widgets;
            }
        }

        [Browsable(false)]
        public AppEnvironment AppEnvironment
        {
            get
            {
                return this.appEnvironment;
            }
        }

        [Browsable(false)]
        public DocumentWorkspace ActiveDocumentWorkspace
        {
            get
            {
                return this.activeDocumentWorkspace;
            }

            set
            {
                if (value != this.activeDocumentWorkspace)
                {
                    if (value != null &&
                        this.documentWorkspaces.IndexOf(value) == -1)
                    {
                        throw new ArgumentException("DocumentWorkspace was not created with AddNewDocumentWorkspace");
                    }

                    bool focused = false;
                    if (this.activeDocumentWorkspace != null)
                    {
                        focused = this.activeDocumentWorkspace.Focused;
                    }

                    UI.SuspendControlPainting(this);
                    OnActiveDocumentWorkspaceChanging();
                    this.activeDocumentWorkspace = value;
                    OnActiveDocumentWorkspaceChanged();
                    UI.ResumeControlPainting(this);

                    Refresh();

                    if (value != null)
                    {
                        value.Focus();
                    }
                }
            }
        }

        private void ActiveDocumentWorkspace_FirstInputAfterGotFocus(object sender, EventArgs e)
        {
            this.toolBar.DocumentStrip.EnsureItemFullyVisible(this.toolBar.DocumentStrip.SelectedDocumentIndex);
        }

        public DocumentWorkspace[] DocumentWorkspaces
        {
            get
            {
                return this.documentWorkspaces.ToArray();
            }
        }

        public DocumentWorkspace AddNewDocumentWorkspace()
        {
            if (this.initialWorkspace != null)
            {
                if (this.initialWorkspace.Document == null || !this.initialWorkspace.Document.Dirty)
                {
                    this.globalToolTypeChoice = this.initialWorkspace.GetToolType();
                    RemoveDocumentWorkspace(this.initialWorkspace);
                    this.initialWorkspace = null;
                }
            }

            DocumentWorkspace dw = new DocumentWorkspace();

            dw.AppWorkspace = this;
            this.documentWorkspaces.Add(dw);
            this.toolBar.DocumentStrip.AddDocumentWorkspace(dw);

            return dw;
        }

        public Image GetDocumentWorkspaceThumbnail(DocumentWorkspace dw)
        {
            this.toolBar.DocumentStrip.SyncThumbnails();
            Image[] images = this.toolBar.DocumentStrip.DocumentThumbnails;
            DocumentWorkspace[] documents = this.toolBar.DocumentStrip.DocumentList;

            for (int i = 0; i < documents.Length; ++i)
            {
                if (documents[i] == dw)
                {
                    return images[i];
                }
            }

            throw new ArgumentException("The requested DocumentWorkspace doesn't exist in this AppWorkspace");
        }

        public void RemoveDocumentWorkspace(DocumentWorkspace documentWorkspace)
        {
            int dwIndex = this.documentWorkspaces.IndexOf(documentWorkspace);

            if (dwIndex == -1)
            {
                throw new ArgumentException("DocumentWorkspace was not created with AddNewDocumentWorkspace");
            }

            bool removingCurrentDW;
            if (this.ActiveDocumentWorkspace == documentWorkspace)
            {
                removingCurrentDW = true;
                this.globalToolTypeChoice = documentWorkspace.GetToolType();
            }
            else
            {
                removingCurrentDW = false;
            }

            documentWorkspace.SetTool(null);

            // Choose new active DW if removing the current DW
            if (removingCurrentDW)
            {
                if (this.documentWorkspaces.Count == 1)
                {
                    this.ActiveDocumentWorkspace = null;
                }
                else if (dwIndex == 0)
                {
                    this.ActiveDocumentWorkspace = this.documentWorkspaces[1];
                }
                else
                {
                    this.ActiveDocumentWorkspace = this.documentWorkspaces[dwIndex - 1];
                }
            }

            this.documentWorkspaces.Remove(documentWorkspace);
            this.toolBar.DocumentStrip.RemoveDocumentWorkspace(documentWorkspace);

            if (this.initialWorkspace == documentWorkspace)
            {
                this.initialWorkspace = null;
            }

            // Clean up the DocumentWorkspace
            Document document = documentWorkspace.Document;

            documentWorkspace.Document = null;
            document.Dispose();

            documentWorkspace.Dispose();
            documentWorkspace = null;
        }

        private void UpdateHistoryButtons()
        {
            if (ActiveDocumentWorkspace == null)
            {
                widgets.CommonActionsStrip.SetButtonEnabled(CommonAction.Undo, false);
                widgets.CommonActionsStrip.SetButtonEnabled(CommonAction.Redo, false);
            }
            else
            {
                if (ActiveDocumentWorkspace.History.UndoStack.Count > 1)
                {
                    widgets.CommonActionsStrip.SetButtonEnabled(CommonAction.Undo, true);
                }
                else
                {
                    widgets.CommonActionsStrip.SetButtonEnabled(CommonAction.Undo, false);
                }

                if (ActiveDocumentWorkspace.History.RedoStack.Count > 0)
                {
                    widgets.CommonActionsStrip.SetButtonEnabled(CommonAction.Redo, true);
                }
                else
                {
                    widgets.CommonActionsStrip.SetButtonEnabled(CommonAction.Redo, false);
                }
            }
        }

        private void HistoryChangedHandler(object sender, EventArgs e)
        {
            UpdateHistoryButtons();

            // some actions change the document size: make sure we update our status bar panel
            // TODO: shouldn't this be handled by our DocumentWorkspace.DocumentChanged handler...?
            UpdateDocInfoInStatusBar();
        }

        public event EventHandler ActiveDocumentWorkspaceChanging;
        protected virtual void OnActiveDocumentWorkspaceChanging()
        {
            SuspendUpdateSnapObstacle();

            if (ActiveDocumentWorkspaceChanging != null)
            {
                ActiveDocumentWorkspaceChanging(this, EventArgs.Empty);
            }

            if (this.activeDocumentWorkspace != null)
            {
                this.activeDocumentWorkspace.FirstInputAfterGotFocus +=
                    ActiveDocumentWorkspace_FirstInputAfterGotFocus;

                this.activeDocumentWorkspace.RulersEnabledChanged -= this.DocumentWorkspace_RulersEnabledChanged;
                this.activeDocumentWorkspace.DocumentMouseEnter -= this.DocumentMouseEnterHandler;
                this.activeDocumentWorkspace.DocumentMouseLeave -= this.DocumentMouseLeaveHandler;
                this.activeDocumentWorkspace.DocumentMouseMove -= this.DocumentMouseMoveHandler;
                this.activeDocumentWorkspace.DocumentMouseDown -= this.DocumentMouseDownHandler;
                this.activeDocumentWorkspace.Scroll -= this.DocumentWorkspace_Scroll;
                this.activeDocumentWorkspace.Layout -= this.DocumentWorkspace_Layout;
                this.activeDocumentWorkspace.DrawGridChanged -= this.DocumentWorkspace_DrawGridChanged;
                this.activeDocumentWorkspace.DocumentClick -= this.DocumentClick;
                this.activeDocumentWorkspace.DocumentMouseUp -= this.DocumentMouseUpHandler;
                this.activeDocumentWorkspace.DocumentKeyPress -= this.DocumentKeyPress;
                this.activeDocumentWorkspace.DocumentKeyUp -= this.DocumenKeyUp;
                this.activeDocumentWorkspace.DocumentKeyDown -= this.DocumentKeyDown;

                this.activeDocumentWorkspace.History.Changed -= HistoryChangedHandler;
                this.activeDocumentWorkspace.StatusChanged -= OnDocumentWorkspaceStatusChanged;
                this.activeDocumentWorkspace.DocumentChanging -= DocumentWorkspace_DocumentChanging;
                this.activeDocumentWorkspace.DocumentChanged -= DocumentWorkspace_DocumentChanged;
                this.activeDocumentWorkspace.Selection.Changing -= SelectedPathChangingHandler;
                this.activeDocumentWorkspace.Selection.Changed -= SelectedPathChangedHandler;
                this.activeDocumentWorkspace.ScaleFactorChanged -= ZoomChangedHandler;
                this.activeDocumentWorkspace.ZoomBasisChanged -= DocumentWorkspace_ZoomBasisChanged;

                this.activeDocumentWorkspace.Visible = false;
                this.historyForm.HistoryControl.HistoryStack = null;

                this.activeDocumentWorkspace.ToolChanging -= this.ToolChangingHandler;
                this.activeDocumentWorkspace.ToolChanged -= this.ToolChangedHandler;

                if (this.activeDocumentWorkspace.Tool != null)
                {
                    while (this.activeDocumentWorkspace.Tool.IsMouseEntered)
                    {
                        this.activeDocumentWorkspace.Tool.PerformMouseLeave();
                    }
                }

                Type toolType = this.activeDocumentWorkspace.GetToolType();

                if (toolType != null)
                {
                    this.globalToolTypeChoice = this.activeDocumentWorkspace.GetToolType();
                }
            }

            ResumeUpdateSnapObstacle();
            UpdateSnapObstacle();
        }

        public event EventHandler ActiveDocumentWorkspaceChanged;
        protected virtual void OnActiveDocumentWorkspaceChanged()
        {
            SuspendUpdateSnapObstacle();

            if (this.activeDocumentWorkspace == null)
            {
                this.toolBar.CommonActionsStrip.SetButtonEnabled(CommonAction.Print, false);
                this.toolBar.CommonActionsStrip.SetButtonEnabled(CommonAction.Save, false);
            }
            else
            {
                this.activeDocumentWorkspace.SuspendLayout();

                this.toolBar.CommonActionsStrip.SetButtonEnabled(CommonAction.Print, true);
                this.toolBar.CommonActionsStrip.SetButtonEnabled(CommonAction.Save, true);

                this.activeDocumentWorkspace.BackColor = System.Drawing.SystemColors.ControlDark;
                this.activeDocumentWorkspace.Dock = System.Windows.Forms.DockStyle.Fill;
                this.activeDocumentWorkspace.DrawGrid = this.DrawGrid;
                this.activeDocumentWorkspace.PanelAutoScroll = true;
                this.activeDocumentWorkspace.RulersEnabled = this.globalRulersChoice;
                this.activeDocumentWorkspace.TabIndex = 0;
                this.activeDocumentWorkspace.TabStop = false;
                this.activeDocumentWorkspace.RulersEnabledChanged += this.DocumentWorkspace_RulersEnabledChanged;
                this.activeDocumentWorkspace.DocumentMouseEnter += this.DocumentMouseEnterHandler;
                this.activeDocumentWorkspace.DocumentMouseLeave += this.DocumentMouseLeaveHandler;
                this.activeDocumentWorkspace.DocumentMouseMove += this.DocumentMouseMoveHandler;
                this.activeDocumentWorkspace.DocumentMouseDown += this.DocumentMouseDownHandler;
                this.activeDocumentWorkspace.Scroll += this.DocumentWorkspace_Scroll;
                this.activeDocumentWorkspace.DrawGridChanged += this.DocumentWorkspace_DrawGridChanged;
                this.activeDocumentWorkspace.DocumentClick += this.DocumentClick;
                this.activeDocumentWorkspace.DocumentMouseUp += this.DocumentMouseUpHandler;
                this.activeDocumentWorkspace.DocumentKeyPress += this.DocumentKeyPress;
                this.activeDocumentWorkspace.DocumentKeyUp += this.DocumenKeyUp;
                this.activeDocumentWorkspace.DocumentKeyDown += this.DocumentKeyDown;

                if (this.workspacePanel.Controls.Contains(this.activeDocumentWorkspace))
                {
                    this.activeDocumentWorkspace.Visible = true;
                }
                else
                {
                    this.activeDocumentWorkspace.Dock = DockStyle.Fill;
                    this.workspacePanel.Controls.Add(this.activeDocumentWorkspace);
                }

                this.activeDocumentWorkspace.Layout += this.DocumentWorkspace_Layout;
                this.toolBar.ViewConfigStrip.ScaleFactor = this.activeDocumentWorkspace.ScaleFactor;
                this.toolBar.ViewConfigStrip.ZoomBasis = this.activeDocumentWorkspace.ZoomBasis;

                this.activeDocumentWorkspace.AppWorkspace = this;
                this.activeDocumentWorkspace.History.Changed += HistoryChangedHandler;
                this.activeDocumentWorkspace.StatusChanged += OnDocumentWorkspaceStatusChanged;
                this.activeDocumentWorkspace.DocumentChanging += DocumentWorkspace_DocumentChanging;
                this.activeDocumentWorkspace.DocumentChanged += DocumentWorkspace_DocumentChanged;
                this.activeDocumentWorkspace.Selection.Changing += SelectedPathChangingHandler;
                this.activeDocumentWorkspace.Selection.Changed += SelectedPathChangedHandler;
                this.activeDocumentWorkspace.ScaleFactorChanged += ZoomChangedHandler;
                this.activeDocumentWorkspace.ZoomBasisChanged += DocumentWorkspace_ZoomBasisChanged;

                this.activeDocumentWorkspace.Units = this.widgets.ViewConfigStrip.Units;

                this.historyForm.HistoryControl.HistoryStack = this.ActiveDocumentWorkspace.History;

                this.activeDocumentWorkspace.ToolChanging += this.ToolChangingHandler;
                this.activeDocumentWorkspace.ToolChanged += this.ToolChangedHandler;

                this.toolBar.ViewConfigStrip.RulersEnabled = this.activeDocumentWorkspace.RulersEnabled;
                this.toolBar.DocumentStrip.SelectDocumentWorkspace(this.activeDocumentWorkspace);

                this.activeDocumentWorkspace.SetToolFromType(this.globalToolTypeChoice);

                UpdateSelectionToolbarButtons();
                UpdateHistoryButtons();
                UpdateDocInfoInStatusBar();

                this.activeDocumentWorkspace.ResumeLayout();
                this.activeDocumentWorkspace.PerformLayout();

                this.activeDocumentWorkspace.FirstInputAfterGotFocus +=
                    ActiveDocumentWorkspace_FirstInputAfterGotFocus;
            }

            if (ActiveDocumentWorkspaceChanged != null)
            {
                ActiveDocumentWorkspaceChanged(this, EventArgs.Empty);
            }

            UpdateStatusBarContextStatus();
            ResumeUpdateSnapObstacle();
            UpdateSnapObstacle();
        }

        public AppWorkspace()
        {
            SuspendLayout();

            // initialize!
            InitializeComponent();
            InitializeFloatingForms();

            this.mainToolBarForm.ToolsControl.SetTools(DocumentWorkspace.ToolInfos);
            this.mainToolBarForm.ToolsControl.ToolClicked += new ToolClickedEventHandler(this.MainToolBar_ToolClicked);

            this.toolBar.ToolChooserStrip.SetTools(DocumentWorkspace.ToolInfos);
            this.toolBar.ToolChooserStrip.ToolClicked += new ToolClickedEventHandler(this.MainToolBar_ToolClicked);

            this.toolBar.AppWorkspace = this;

            // init the Widgets container
            this.widgets = new WorkspaceWidgets(this);
            this.widgets.ViewConfigStrip = this.toolBar.ViewConfigStrip;
            this.widgets.CommonActionsStrip = this.toolBar.CommonActionsStrip;
            this.widgets.ToolConfigStrip = this.toolBar.ToolConfigStrip;
            this.widgets.ToolsForm = this.mainToolBarForm;
            this.widgets.LayerForm = this.layerForm;
            this.widgets.HistoryForm = this.historyForm;
            this.widgets.ColorsForm = this.colorsForm;
            this.widgets.StatusBarProgress = this.statusBar;
            this.widgets.DocumentStrip = this.toolBar.DocumentStrip;

            // Load our settings and initialize the AppEnvironment
            LoadSettings();

            // hook into Environment *Changed events
            AppEnvironment.PrimaryColorChanged += PrimaryColorChangedHandler;
            AppEnvironment.SecondaryColorChanged += SecondaryColorChangedHandler;
            AppEnvironment.ShapeDrawTypeChanged += ShapeDrawTypeChangedHandler;
            AppEnvironment.GradientInfoChanged += GradientInfoChangedHandler;
            AppEnvironment.ToleranceChanged += OnEnvironmentToleranceChanged;
            AppEnvironment.AlphaBlendingChanged += AlphaBlendingChangedHandler;
            AppEnvironment.FontInfo = this.toolBar.ToolConfigStrip.FontInfo;
            AppEnvironment.TextAlignment = this.toolBar.ToolConfigStrip.FontAlignment;
            AppEnvironment.AntiAliasingChanged += Environment_AntiAliasingChanged;
            AppEnvironment.FontInfoChanged += Environment_FontInfoChanged;
            AppEnvironment.FontSmoothingChanged += Environment_FontSmoothingChanged;
            AppEnvironment.TextAlignmentChanged += Environment_TextAlignmentChanged;
            AppEnvironment.PenInfoChanged += Environment_PenInfoChanged;
            AppEnvironment.BrushInfoChanged += Environment_BrushInfoChanged;
            AppEnvironment.ColorPickerClickBehaviorChanged += Environment_ColorPickerClickBehaviorChanged;
            AppEnvironment.ResamplingAlgorithmChanged += Environment_ResamplingAlgorithmChanged;
            AppEnvironment.SelectionCombineModeChanged += Environment_SelectionCombineModeChanged;
            AppEnvironment.FloodModeChanged += Environment_FloodModeChanged;
            AppEnvironment.SelectionDrawModeInfoChanged += Environment_SelectionDrawModeInfoChanged;

            this.toolBar.DocumentStrip.RelinquishFocus += RelinquishFocusHandler;

            this.toolBar.ToolConfigStrip.ToleranceChanged += OnToolBarToleranceChanged;
            this.toolBar.ToolConfigStrip.FontAlignmentChanged += ToolConfigStrip_TextAlignmentChanged;
            this.toolBar.ToolConfigStrip.FontInfoChanged += ToolConfigStrip_FontTextChanged;
            this.toolBar.ToolConfigStrip.FontSmoothingChanged += ToolConfigStrip_FontSmoothingChanged;
            this.toolBar.ToolConfigStrip.RelinquishFocus += RelinquishFocusHandler2;

            this.toolBar.CommonActionsStrip.RelinquishFocus += OnToolStripRelinquishFocus;
            this.toolBar.CommonActionsStrip.MouseWheel += OnToolStripMouseWheel;
            this.toolBar.CommonActionsStrip.ButtonClick += CommonActionsStrip_ButtonClick;

            this.toolBar.ViewConfigStrip.DrawGridChanged += ViewConfigStrip_DrawGridChanged;
            this.toolBar.ViewConfigStrip.RulersEnabledChanged += ViewConfigStrip_RulersEnabledChanged;
            this.toolBar.ViewConfigStrip.ZoomBasisChanged += ViewConfigStrip_ZoomBasisChanged;
            this.toolBar.ViewConfigStrip.ZoomScaleChanged += ViewConfigStrip_ZoomScaleChanged;
            this.toolBar.ViewConfigStrip.ZoomIn += ViewConfigStrip_ZoomIn;
            this.toolBar.ViewConfigStrip.ZoomOut += ViewConfigStrip_ZoomOut;
            this.toolBar.ViewConfigStrip.UnitsChanged += ViewConfigStrip_UnitsChanged;
            this.toolBar.ViewConfigStrip.RelinquishFocus += OnToolStripRelinquishFocus;
            this.toolBar.ViewConfigStrip.MouseWheel += OnToolStripMouseWheel;

            this.toolBar.ToolConfigStrip.BrushInfoChanged += DrawConfigStrip_BrushChanged;
            this.toolBar.ToolConfigStrip.ShapeDrawTypeChanged += DrawConfigStrip_ShapeDrawTypeChanged;
            this.toolBar.ToolConfigStrip.PenInfoChanged += DrawConfigStrip_PenChanged;
            this.toolBar.ToolConfigStrip.GradientInfoChanged += ToolConfigStrip_GradientInfoChanged;
            this.toolBar.ToolConfigStrip.AlphaBlendingChanged += OnDrawConfigStripAlphaBlendingChanged;
            this.toolBar.ToolConfigStrip.AntiAliasingChanged += DrawConfigStrip_AntiAliasingChanged;
            this.toolBar.ToolConfigStrip.RelinquishFocus += OnToolStripRelinquishFocus;
            this.toolBar.ToolConfigStrip.ColorPickerClickBehaviorChanged += ToolConfigStrip_ColorPickerClickBehaviorChanged;
            this.toolBar.ToolConfigStrip.ResamplingAlgorithmChanged += ToolConfigStrip_ResamplingAlgorithmChanged;
            this.toolBar.ToolConfigStrip.SelectionCombineModeChanged += ToolConfigStrip_SelectionCombineModeChanged;
            this.toolBar.ToolConfigStrip.FloodModeChanged += ToolConfigStrip_FloodModeChanged;
            this.toolBar.ToolConfigStrip.SelectionDrawModeInfoChanged += ToolConfigStrip_SelectionDrawModeInfoChanged;
            this.toolBar.ToolConfigStrip.SelectionDrawModeUnitsChanging += ToolConfigStrip_SelectionDrawModeUnitsChanging;

            this.toolBar.ToolConfigStrip.MouseWheel += OnToolStripMouseWheel;

            this.toolBar.DocumentStrip.RelinquishFocus += OnToolStripRelinquishFocus;
            this.toolBar.DocumentStrip.DocumentClicked += DocumentStrip_DocumentTabClicked;
            this.toolBar.DocumentStrip.DocumentListChanged += DocumentStrip_DocumentListChanged;

            // Synchronize
            AppEnvironment.PerformAllChanged();

            this.globalToolTypeChoice = this.defaultToolTypeChoice;
            this.toolBar.ToolConfigStrip.ToolBarConfigItems = ToolBarConfigItems.None;

            this.layerForm.LayerControl.AppWorkspace = this;

            ResumeLayout();
            PerformLayout();
        }

        private void ToolConfigStrip_ColorPickerClickBehaviorChanged(object sender, EventArgs e)
        {
            this.appEnvironment.ColorPickerClickBehavior = this.widgets.ToolConfigStrip.ColorPickerClickBehavior;
        }

        private void Environment_ColorPickerClickBehaviorChanged(object sender, EventArgs e)
        {
            this.widgets.ToolConfigStrip.ColorPickerClickBehavior = this.appEnvironment.ColorPickerClickBehavior;
        }

        private void ToolConfigStrip_ResamplingAlgorithmChanged(object sender, EventArgs e)
        {
            this.appEnvironment.ResamplingAlgorithm = this.widgets.ToolConfigStrip.ResamplingAlgorithm;
        }

        private void Environment_ResamplingAlgorithmChanged(object sender, EventArgs e)
        {
            this.widgets.ToolConfigStrip.ResamplingAlgorithm = this.appEnvironment.ResamplingAlgorithm;
        }

        private void ToolConfigStrip_SelectionCombineModeChanged(object sender, EventArgs e)
        {
            this.appEnvironment.SelectionCombineMode = this.widgets.ToolConfigStrip.SelectionCombineMode;
        }

        private void Environment_SelectionCombineModeChanged(object sender, EventArgs e)
        {
            this.widgets.ToolConfigStrip.SelectionCombineMode = this.appEnvironment.SelectionCombineMode;
        }

        private void ToolConfigStrip_FloodModeChanged(object sender, EventArgs e)
        {
            this.appEnvironment.FloodMode = this.widgets.ToolConfigStrip.FloodMode;
        }

        private void Environment_FloodModeChanged(object sender, EventArgs e)
        {
            this.widgets.ToolConfigStrip.FloodMode = this.appEnvironment.FloodMode;
        }

        private void ToolConfigStrip_SelectionDrawModeInfoChanged(object sender, EventArgs e)
        {
            this.appEnvironment.SelectionDrawModeInfo = this.widgets.ToolConfigStrip.SelectionDrawModeInfo;
        }

        private void Environment_SelectionDrawModeInfoChanged(object sender, EventArgs e)
        {
            this.widgets.ToolConfigStrip.SelectionDrawModeInfo = this.appEnvironment.SelectionDrawModeInfo;
        }

        private sealed class ToolConfigStrip_SelectionDrawModeUnitsChangeHandler
        {
            private ToolConfigStrip toolConfigStrip;
            private Document activeDocument;
            private MeasurementUnit oldUnits;

            public ToolConfigStrip_SelectionDrawModeUnitsChangeHandler(ToolConfigStrip toolConfigStrip, Document activeDocument)
            {
                this.toolConfigStrip = toolConfigStrip;
                this.activeDocument = activeDocument;
                this.oldUnits = toolConfigStrip.SelectionDrawModeInfo.Units;
            }

            public void Initialize()
            {
                this.toolConfigStrip.SelectionDrawModeUnitsChanged += ToolConfigStrip_SelectionDrawModeUnitsChanged;
            }

            public void ToolConfigStrip_SelectionDrawModeUnitsChanged(object sender, EventArgs e)
            {
                try
                {
                    SelectionDrawModeInfo sdmi = this.toolConfigStrip.SelectionDrawModeInfo;
                    MeasurementUnit newUnits = sdmi.Units;

                    double oldWidth = sdmi.Width;
                    double oldHeight = sdmi.Height;

                    double newWidth;
                    double newHeight;

                    newWidth = Document.ConvertMeasurement(oldWidth, this.oldUnits, this.activeDocument.DpuUnit, this.activeDocument.DpuX, newUnits);
                    newHeight = Document.ConvertMeasurement(oldHeight, this.oldUnits, this.activeDocument.DpuUnit, this.activeDocument.DpuY, newUnits);

                    SelectionDrawModeInfo newSdmi = sdmi.CloneWithNewWidthAndHeight(newWidth, newHeight);
                    this.toolConfigStrip.SelectionDrawModeInfo = newSdmi;
                }

                finally
                {
                    this.toolConfigStrip.SelectionDrawModeUnitsChanged -= ToolConfigStrip_SelectionDrawModeUnitsChanged;
                }
            }
        }

        private void ToolConfigStrip_SelectionDrawModeUnitsChanging(object sender, EventArgs e)
        {
            if (this.ActiveDocumentWorkspace != null && this.ActiveDocumentWorkspace.Document != null)
            {
                ToolConfigStrip_SelectionDrawModeUnitsChangeHandler tcsSdmuch = new ToolConfigStrip_SelectionDrawModeUnitsChangeHandler(
                    this.toolBar.ToolConfigStrip, this.ActiveDocumentWorkspace.Document);

                tcsSdmuch.Initialize();
            }
        }

        private void DocumentStrip_DocumentListChanged(object sender, EventArgs e)
        {
            bool enableThem = (this.widgets.DocumentStrip.DocumentCount != 0);

            this.widgets.ToolsForm.Enabled = enableThem;
            this.widgets.HistoryForm.Enabled = enableThem;
            this.widgets.LayerForm.Enabled = enableThem;
            this.widgets.ColorsForm.Enabled = enableThem;
            this.widgets.CommonActionsStrip.SetButtonEnabled(CommonAction.Paste, enableThem);

            UpdateHistoryButtons();
            UpdateDocInfoInStatusBar();
            UpdateCursorInfoInStatusBar(0, 0);
        }

        public void SaveSettings()
        {
            Settings.CurrentUser.SetBoolean(SettingNames.Rulers, this.globalRulersChoice);
            Settings.CurrentUser.SetBoolean(SettingNames.DrawGrid, this.DrawGrid);
            Settings.CurrentUser.SetString(SettingNames.DefaultToolTypeName, this.defaultToolTypeChoice.Name);
            this.MostRecentFiles.SaveMruList();
        }

        private void LoadDefaultToolType()
        {
            string defaultToolTypeName = Settings.CurrentUser.GetString(SettingNames.DefaultToolTypeName, Tool.DefaultToolType.Name);

            ToolInfo[] tis = DocumentWorkspace.ToolInfos;
            ToolInfo ti = Array.Find(
                tis,
                delegate(ToolInfo check)
                {
                    if (string.Compare(defaultToolTypeName, check.ToolType.Name, StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                });

            if (ti == null)
            {
                this.defaultToolTypeChoice = Tool.DefaultToolType;
            }
            else
            {
                this.defaultToolTypeChoice = ti.ToolType;
            }
        }

        public void LoadSettings()
        {
            try
            {
                LoadDefaultToolType();

                this.globalToolTypeChoice = this.defaultToolTypeChoice;
                this.globalRulersChoice = Settings.CurrentUser.GetBoolean(SettingNames.Rulers, false);
                this.DrawGrid = Settings.CurrentUser.GetBoolean(SettingNames.DrawGrid, false);

                this.appEnvironment = AppEnvironment.GetDefaultAppEnvironment();

                this.widgets.ViewConfigStrip.Units = (MeasurementUnit)Enum.Parse(typeof(MeasurementUnit),
                    Settings.CurrentUser.GetString(SettingNames.Units, MeasurementUnit.Pixel.ToString()), true);
            }

            catch (Exception)
            {
                this.appEnvironment = new AppEnvironment();
                this.appEnvironment.SetToDefaults();

                try
                {
                    Settings.CurrentUser.Delete(
                        new string[] 
                        {    
                            SettingNames.Rulers, 
                            SettingNames.DrawGrid, 
                            SettingNames.Units,
                            SettingNames.DefaultAppEnvironment,
                            SettingNames.DefaultToolTypeName,
                        });
                }

                catch (Exception)
                {
                }
            }

            try
            {
                this.toolBar.ToolConfigStrip.LoadFromAppEnvironment(this.appEnvironment);
            }

            catch (Exception)
            {
                this.appEnvironment = new AppEnvironment();
                this.appEnvironment.SetToDefaults();
                this.toolBar.ToolConfigStrip.LoadFromAppEnvironment(this.appEnvironment);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            if (this.ActiveDocumentWorkspace != null)
            {
                this.ActiveDocumentWorkspace.Select();
            }

            UpdateSnapObstacle();

            base.OnLoad(e);
        }

        public void RefreshTool()
        {
            Type toolType = activeDocumentWorkspace.GetToolType();
            Widgets.ToolsControl.SelectTool(toolType);
        }

        private void GradientInfoChangedHandler(object sender, EventArgs e)
        {
            if (widgets.ToolConfigStrip.GradientInfo != AppEnvironment.GradientInfo)
            {
                widgets.ToolConfigStrip.GradientInfo = AppEnvironment.GradientInfo;
            }
        }

        private void ToolConfigStrip_GradientInfoChanged(object sender, EventArgs e)
        {
            if (AppEnvironment.GradientInfo != widgets.ToolConfigStrip.GradientInfo)
            {
                AppEnvironment.GradientInfo = widgets.ToolConfigStrip.GradientInfo;
            }
        }

        /// <summary>
        /// Keeps the Environment's ShapeDrawType and the corresponding widget synchronized
        /// </summary>
        private void ShapeDrawTypeChangedHandler(object sender, EventArgs e)
        {
            if (widgets.ToolConfigStrip.ShapeDrawType != AppEnvironment.ShapeDrawType)
            {
                widgets.ToolConfigStrip.ShapeDrawType = AppEnvironment.ShapeDrawType;
            }
        }

        /// <summary>
        /// Keeps the Environment's alpha blending value and the corresponding widget synchronized
        /// </summary>
        private void AlphaBlendingChangedHandler(object sender, EventArgs e)
        {
            if (widgets.ToolConfigStrip.AlphaBlending != AppEnvironment.AlphaBlending)
            {
                widgets.ToolConfigStrip.AlphaBlending = AppEnvironment.AlphaBlending;
            }
        }

        private void ColorDisplay_UserPrimaryAndSecondaryColorsChanged(object sender, EventArgs e)
        {
            // We need to make sure that we don't change which user color is selected (primary vs. secondary)
            // To do this we choose the ordering based on which one is currently active (primary vs. secondary)
            if (widgets.ColorsForm.WhichUserColor == WhichUserColor.Primary)
            {
                widgets.ColorsForm.SetColorControlsRedraw(false);
                SecondaryColorChangedHandler(sender, e);
                PrimaryColorChangedHandler(sender, e);
                widgets.ColorsForm.SetColorControlsRedraw(true);
                widgets.ColorsForm.WhichUserColor = WhichUserColor.Primary;
            }
            else //if (widgets.ColorsForm.WhichUserColor == WhichUserColor.Background)
            {
                widgets.ColorsForm.SetColorControlsRedraw(false);
                PrimaryColorChangedHandler(sender, e);
                SecondaryColorChangedHandler(sender, e);
                widgets.ColorsForm.SetColorControlsRedraw(true);
                widgets.ColorsForm.WhichUserColor = WhichUserColor.Secondary;
            }
        }
 
        private void PrimaryColorChangedHandler(object sender, EventArgs e)
        {
            if (sender == appEnvironment)
            {
                widgets.ColorsForm.UserPrimaryColor = AppEnvironment.PrimaryColor;
            }
        }

        private void OnToolBarToleranceChanged(object sender, EventArgs e)
        {
            AppEnvironment.Tolerance = widgets.ToolConfigStrip.Tolerance;
            this.Focus();
        }

        private void OnEnvironmentToleranceChanged(object sender, EventArgs e)
        {
            widgets.ToolConfigStrip.Tolerance = AppEnvironment.Tolerance;
            this.Focus();
        }

        private void SecondaryColorChangedHandler(object sender, EventArgs e)
        {
            if (sender == appEnvironment)
            {
                widgets.ColorsForm.UserSecondaryColor = AppEnvironment.SecondaryColor;
            }
        }

        private void RelinquishFocusHandler(object sender, EventArgs e)
        {
            this.Focus();
        }

        private void RelinquishFocusHandler2(object sender, EventArgs e)
        {
            if (this.activeDocumentWorkspace != null)
            {
                this.activeDocumentWorkspace.Focus();
            }
        }

        private void ColorsForm_UserPrimaryColorChanged(object sender, ColorEventArgs e)
        {
            ColorsForm cf = (ColorsForm)sender;
            AppEnvironment.PrimaryColor = e.Color;
        }

        private void ColorsForm_UserSecondaryColorChanged(object sender, ColorEventArgs e)
        {
            ColorsForm cf = (ColorsForm)sender;
            AppEnvironment.SecondaryColor = e.Color;
        }

        /// <summary>
        /// Handles the SelectedPathChanging event that is raised by the AppEnvironment.
        /// </summary>
        private void SelectedPathChangingHandler(object sender, EventArgs e)
        {   
        }

        private void UpdateSelectionToolbarButtons()
        {
            if (ActiveDocumentWorkspace == null || ActiveDocumentWorkspace.Selection.IsEmpty)
            {
                widgets.CommonActionsStrip.SetButtonEnabled(CommonAction.Cut, false);
                widgets.CommonActionsStrip.SetButtonEnabled(CommonAction.Copy, false);
                widgets.CommonActionsStrip.SetButtonEnabled(CommonAction.Deselect, false);
                widgets.CommonActionsStrip.SetButtonEnabled(CommonAction.CropToSelection, false);
            }
            else
            {
                widgets.CommonActionsStrip.SetButtonEnabled(CommonAction.Cut, true);
                widgets.CommonActionsStrip.SetButtonEnabled(CommonAction.Copy, true);
                widgets.CommonActionsStrip.SetButtonEnabled(CommonAction.Deselect, true);
                widgets.CommonActionsStrip.SetButtonEnabled(CommonAction.CropToSelection, true);
            }
        }

        /// <summary>
        /// Handles the SelectedPathChanged event that is raised by the AppEnvironment.
        /// </summary>
        private void SelectedPathChangedHandler(object sender, EventArgs e)
        {
            UpdateSelectionToolbarButtons();
        }

        private void ZoomChangedHandler(object sender, EventArgs e)
        {
            ScaleFactor sf = this.activeDocumentWorkspace.ScaleFactor;
            this.toolBar.ViewConfigStrip.SuspendEvents();
            this.toolBar.ViewConfigStrip.ZoomBasis = this.activeDocumentWorkspace.ZoomBasis;
            this.toolBar.ViewConfigStrip.ScaleFactor = sf;
            this.toolBar.ViewConfigStrip.ResumeEvents();
        }

        private void InitializeComponent()
        {
            this.toolBar = new PdnToolBar();
            this.statusBar = new PdnStatusBar();
            this.workspacePanel = new Panel();
            this.workspacePanel.SuspendLayout();
            this.statusBar.SuspendLayout();
            this.SuspendLayout();
            //
            // toolBar
            //
            this.toolBar.Name = "toolBar";
            this.toolBar.Dock = DockStyle.Top;
            //
            // statusBar
            //
            this.statusBar.Name = "statusBar";
            //
            // workspacePanel
            //
            this.workspacePanel.Name = "workspacePanel";
            this.workspacePanel.Dock = DockStyle.Fill;
            // 
            // AppWorkspace
            // 
            this.Controls.Add(this.workspacePanel);
            this.Controls.Add(this.statusBar);
            this.Controls.Add(this.toolBar);
            this.Name = "AppWorkspace";
            this.Size = new System.Drawing.Size(872, 640);
            this.workspacePanel.ResumeLayout(false);
            this.statusBar.ResumeLayout(false);
            this.statusBar.PerformLayout();
            this.ResumeLayout(false);
        }

        private void DocumentStrip_DocumentTabClicked(
            object sender, 
            EventArgs<Pair<DocumentWorkspace, DocumentClickAction>> e)
        {
            switch (e.Data.Second)
            {
                case DocumentClickAction.Select:
                    this.ActiveDocumentWorkspace = e.Data.First;
                    break;

                case DocumentClickAction.Close:
                    CloseWorkspaceAction cwa = new CloseWorkspaceAction(e.Data.First);
                    PerformAction(cwa);
                    break;

                default:
                    throw new NotImplementedException("Code for DocumentClickAction." + e.Data.Second.ToString() + " not implemented");
            }

            Update();
        }

        private void OnToolStripMouseWheel(object sender, MouseEventArgs e)
        {
            if (this.activeDocumentWorkspace != null)
            {
                this.activeDocumentWorkspace.PerformMouseWheel((Control)sender, e);
            }
        }

        private void OnToolStripRelinquishFocus(object sender, EventArgs e)
        {
            if (this.activeDocumentWorkspace != null)
            {
                this.activeDocumentWorkspace.Focus();
            }
        }

        // The Document* events are raised by the Document class, handled here,
        // and relayed as necessary. For instance, for the DocumentMouse* events, 
        // these are all relayed to the active tool.

        private void DocumentMouseEnterHandler(object sender, EventArgs e)
        {
            if (ActiveDocumentWorkspace.Tool != null)
            {
                ActiveDocumentWorkspace.Tool.PerformMouseEnter();
            }
        }

        private void DocumentMouseLeaveHandler(object sender, EventArgs e)
        {
            if (ActiveDocumentWorkspace.Tool != null)
            {
                ActiveDocumentWorkspace.Tool.PerformMouseLeave();
            }
        }

        private void DocumentMouseUpHandler(object sender, MouseEventArgs e)
        {
            if (ActiveDocumentWorkspace.Tool != null)
            {
                ActiveDocumentWorkspace.Tool.PerformMouseUp(e);
            }
        }

        private void DocumentMouseDownHandler(object sender, MouseEventArgs e)
        {
            if (ActiveDocumentWorkspace.Tool != null)
            {
                ActiveDocumentWorkspace.Tool.PerformMouseDown(e);
            }
        }

        private void DocumentMouseMoveHandler(object sender, MouseEventArgs e)
        {
            if (ActiveDocumentWorkspace.Tool != null)
            {
                ActiveDocumentWorkspace.Tool.PerformMouseMove(e);
            }

            UpdateCursorInfoInStatusBar(e.X, e.Y);
        }

        private void DocumentClick(object sender, EventArgs e)
        {
            if (ActiveDocumentWorkspace.Tool != null)
            {
                ActiveDocumentWorkspace.Tool.PerformClick();
            }
        }

        private void DocumentKeyPress(object sender, KeyPressEventArgs e)
        {
            if (ActiveDocumentWorkspace.Tool != null)
            {
                ActiveDocumentWorkspace.Tool.PerformKeyPress(e);
            }
        }

        private void DocumentKeyDown(object sender, KeyEventArgs e)
        {
            if (ActiveDocumentWorkspace.Tool != null)
            {
                ActiveDocumentWorkspace.Tool.PerformKeyDown(e);
            }
        }

        private void DocumenKeyUp(object sender, KeyEventArgs e)
        {
            if (ActiveDocumentWorkspace.Tool != null)
            {
                ActiveDocumentWorkspace.Tool.PerformKeyUp(e);
            }
        }

        private void InitializeFloatingForms()
        {
            // MainToolBarForm
            mainToolBarForm = new ToolsForm();
            mainToolBarForm.RelinquishFocus += RelinquishFocusHandler;
            mainToolBarForm.ProcessCmdKeyEvent += OnToolFormProcessCmdKeyEvent;

            // LayerForm
            layerForm = new LayerForm();
            layerForm.LayerControl.AppWorkspace = this;
            layerForm.LayerControl.ClickedOnLayer += LayerControl_ClickedOnLayer;
            layerForm.NewLayerButtonClick += LayerForm_NewLayerButtonClicked;
            layerForm.DeleteLayerButtonClick += LayerForm_DeleteLayerButtonClicked;
            layerForm.DuplicateLayerButtonClick += LayerForm_DuplicateLayerButtonClick;
            layerForm.MergeLayerDownClick += new EventHandler(LayerForm_MergeLayerDownClick);
            layerForm.MoveLayerUpButtonClick += LayerForm_MoveLayerUpButtonClicked;
            layerForm.MoveLayerDownButtonClick += LayerForm_MoveLayerDownButtonClicked;
            layerForm.PropertiesButtonClick += LayerForm_PropertiesButtonClick;
            layerForm.RelinquishFocus += RelinquishFocusHandler;
            layerForm.ProcessCmdKeyEvent += OnToolFormProcessCmdKeyEvent;
            
            // HistoryForm
            historyForm = new HistoryForm();
            historyForm.RewindButtonClicked += HistoryForm_RewindButtonClicked;
            historyForm.UndoButtonClicked += HistoryForm_UndoButtonClicked;
            historyForm.RedoButtonClicked += HistoryForm_RedoButtonClicked;
            historyForm.FastForwardButtonClicked += HistoryForm_FastForwardButtonClicked;
            historyForm.RelinquishFocus += RelinquishFocusHandler;
            historyForm.ProcessCmdKeyEvent += OnToolFormProcessCmdKeyEvent;

            // ColorsForm
            colorsForm = new ColorsForm();
            colorsForm.PaletteCollection = new PaletteCollection();
            colorsForm.WhichUserColor = WhichUserColor.Primary;
            colorsForm.UserPrimaryColorChanged += ColorsForm_UserPrimaryColorChanged;
            colorsForm.UserSecondaryColorChanged += ColorsForm_UserSecondaryColorChanged;
            colorsForm.RelinquishFocus += RelinquishFocusHandler;
            colorsForm.ProcessCmdKeyEvent += OnToolFormProcessCmdKeyEvent;
        }

        // TODO: put at correct scope
        public event CmdKeysEventHandler ProcessCmdKeyEvent;

        private bool OnToolFormProcessCmdKeyEvent(object sender, ref Message msg, Keys keyData)
        {
            if (ProcessCmdKeyEvent != null)
            {
                return ProcessCmdKeyEvent(sender, ref msg, keyData);
            }
            else
            {
                return false;
            }
        }

        public void PerformActionAsync(AppWorkspaceAction performMe)
        {
            this.BeginInvoke(new Procedure<AppWorkspaceAction>(PerformAction), new object[] { performMe });
        }

        public void PerformAction(AppWorkspaceAction performMe)
        {
            Update();

            using (new WaitCursorChanger(this))
            {
                performMe.PerformAction(this);
            }

            Update();
        }

        private void MainToolBar_ToolClicked(object sender, ToolClickedEventArgs e)
        {
            if (ActiveDocumentWorkspace != null)
            {
                ActiveDocumentWorkspace.Focus();
                ActiveDocumentWorkspace.SetToolFromType(e.ToolType);
            }
        }

        private void ToolChangingHandler(object sender, EventArgs e)
        {
            UI.SuspendControlPainting(this.toolBar);

            if (ActiveDocumentWorkspace.Tool != null)
            {
                // unregister for events here (none at this time)
            }
        }

        private void ToolChangedHandler(object sender, EventArgs e)
        {
            if (ActiveDocumentWorkspace.Tool != null)
            {
                this.widgets.ToolsControl.SelectTool(ActiveDocumentWorkspace.GetToolType(), false);
                this.toolBar.ToolChooserStrip.SelectTool(ActiveDocumentWorkspace.GetToolType(), false);
                this.toolBar.ToolConfigStrip.Visible = true; // HACK: see bug #2702
                this.toolBar.ToolConfigStrip.ToolBarConfigItems = ActiveDocumentWorkspace.Tool.ToolBarConfigItems;
                this.globalToolTypeChoice = ActiveDocumentWorkspace.GetToolType();
            }

            UpdateStatusBarContextStatus();

            UI.ResumeControlPainting(this.toolBar);
            this.toolBar.Refresh();
        }

        private void DrawConfigStrip_AntiAliasingChanged(object sender, System.EventArgs e)
        {
            AppEnvironment.AntiAliasing = ((ToolConfigStrip)sender).AntiAliasing;
        }

        private void DrawConfigStrip_PenChanged(object sender, System.EventArgs e)
        {
            AppEnvironment.PenInfo = this.toolBar.ToolConfigStrip.PenInfo;
        }

        private void DrawConfigStrip_BrushChanged(object sender, System.EventArgs e)
        {
            AppEnvironment.BrushInfo = this.toolBar.ToolConfigStrip.BrushInfo;
        }

        private void LayerControl_ClickedOnLayer(object sender, EventArgs<Layer> ce)
        {
            if (ActiveDocumentWorkspace != null)
            {
                if (ce.Data != ActiveDocumentWorkspace.ActiveLayer)
                {
                    ActiveDocumentWorkspace.ActiveLayer = ce.Data;
                }
            }

            this.RelinquishFocusHandler(sender, EventArgs.Empty);
        }

        private void LayerForm_NewLayerButtonClicked(object sender, System.EventArgs e)
        {
            if (ActiveDocumentWorkspace != null)
            {
                ActiveDocumentWorkspace.ExecuteFunction(new AddNewBlankLayerFunction());
            }
        }

        private void LayerForm_DeleteLayerButtonClicked(object sender, System.EventArgs e)
        {
            if (ActiveDocumentWorkspace != null && ActiveDocumentWorkspace.Document.Layers.Count > 1)
            {
                ActiveDocumentWorkspace.ExecuteFunction(new DeleteLayerFunction(ActiveDocumentWorkspace.ActiveLayerIndex));
            }
        }

        private void LayerForm_MergeLayerDownClick(object sender, EventArgs e)
        {
            if (ActiveDocumentWorkspace != null)
            {
                if (ActiveDocumentWorkspace != null &&
                    ActiveDocumentWorkspace.ActiveLayerIndex > 0)
                {
                    // TODO: keep this in sync with LayersMenu. not appropriate to refactor into an Action for a 'dot' release
                    int newLayerIndex = Utility.Clamp(
                        ActiveDocumentWorkspace.ActiveLayerIndex - 1,
                        0,
                        ActiveDocumentWorkspace.Document.Layers.Count - 1);

                    ActiveDocumentWorkspace.ExecuteFunction(
                        new MergeLayerDownFunction(ActiveDocumentWorkspace.ActiveLayerIndex));

                    ActiveDocumentWorkspace.ActiveLayerIndex = newLayerIndex;
                }
            }
        }

        private void LayerForm_DuplicateLayerButtonClick(object sender, System.EventArgs e)
        {
            if (ActiveDocumentWorkspace != null)
            {
                ActiveDocumentWorkspace.ExecuteFunction(new DuplicateLayerFunction(ActiveDocumentWorkspace.ActiveLayerIndex));
            }
        }

        private void LayerForm_MoveLayerUpButtonClicked(object sender, System.EventArgs e)
        {
            if (ActiveDocumentWorkspace != null && ActiveDocumentWorkspace.Document.Layers.Count >= 2)
            {
                ActiveDocumentWorkspace.PerformAction(new MoveActiveLayerUpAction());
            }
        }
        
        private void LayerForm_MoveLayerDownButtonClicked(object sender, System.EventArgs e)
        {
            if (ActiveDocumentWorkspace != null && ActiveDocumentWorkspace.Document.Layers.Count >= 2)
            {
                ActiveDocumentWorkspace.PerformAction(new MoveActiveLayerDownAction());
            }
        }

        private void DrawConfigStrip_ShapeDrawTypeChanged(object sender, System.EventArgs e)
        {
            if (AppEnvironment.ShapeDrawType != widgets.ToolConfigStrip.ShapeDrawType)
            {
                AppEnvironment.ShapeDrawType = widgets.ToolConfigStrip.ShapeDrawType;
            }
        }

        private void HistoryForm_UndoButtonClicked(object sender, System.EventArgs e)
        {
            if (ActiveDocumentWorkspace != null)
            {
                ActiveDocumentWorkspace.PerformAction(new HistoryUndoAction());
            }
        }

        private void HistoryForm_RedoButtonClicked(object sender, System.EventArgs e)
        {
            if (ActiveDocumentWorkspace != null)
            {
                ActiveDocumentWorkspace.PerformAction(new HistoryRedoAction());
            }
        }
        
        private void ViewConfigStrip_RulersEnabledChanged(object sender, System.EventArgs e)
        {
            if (ActiveDocumentWorkspace != null)
            {
                ActiveDocumentWorkspace.RulersEnabled = this.toolBar.ViewConfigStrip.RulersEnabled;
            }
        }

        private void HistoryForm_RewindButtonClicked(object sender, EventArgs e)
        {
            if (ActiveDocumentWorkspace != null)
            {
                ActiveDocumentWorkspace.PerformAction(new HistoryRewindAction());
            }
        }
        
        private void HistoryForm_FastForwardButtonClicked(object sender, EventArgs e)
        {
            if (ActiveDocumentWorkspace != null)
            {
                ActiveDocumentWorkspace.PerformAction(new HistoryFastForwardAction());
            }
        }

        private void LayerForm_PropertiesButtonClick(object sender, EventArgs e)
        {
            if (ActiveDocumentWorkspace != null)
            {
                ActiveDocumentWorkspace.PerformAction(new OpenActiveLayerPropertiesAction());
            }
        }

        private void Environment_FontInfoChanged(object sender, EventArgs e)
        {
            this.widgets.ToolConfigStrip.FontInfo = AppEnvironment.FontInfo;
        }

        private void Environment_FontSmoothingChanged(object sender, EventArgs e)
        {
            this.widgets.ToolConfigStrip.FontSmoothing = AppEnvironment.FontSmoothing;
        }

        private void Environment_TextAlignmentChanged(object sender, EventArgs e)
        {
            this.widgets.ToolConfigStrip.FontAlignment = AppEnvironment.TextAlignment;
        }

        private void Environment_PenInfoChanged(object sender, EventArgs e)
        {
            this.widgets.ToolConfigStrip.PenInfo = AppEnvironment.PenInfo;
        }

        private void Environment_BrushInfoChanged(object sender, EventArgs e)
        {
        }

        private void ToolConfigStrip_TextAlignmentChanged(object sender, EventArgs e)
        {
            AppEnvironment.TextAlignment = widgets.ToolConfigStrip.FontAlignment;
        }

        private void ToolConfigStrip_FontTextChanged(object sender, EventArgs e)
        {
            AppEnvironment.FontInfo = widgets.ToolConfigStrip.FontInfo;
        }

        private void ToolConfigStrip_FontSmoothingChanged(object sender, EventArgs e)
        {
            AppEnvironment.FontSmoothing = widgets.ToolConfigStrip.FontSmoothing;
        }

        protected override void OnResize(EventArgs e)
        {
            UpdateSnapObstacle();

            base.OnResize(e);

            if (ParentForm != null && this.ActiveDocumentWorkspace != null)
            {
                if (ParentForm.WindowState == FormWindowState.Minimized)
                {
                    this.ActiveDocumentWorkspace.EnableToolPulse = false;
                }
                else
                {
                    this.ActiveDocumentWorkspace.EnableToolPulse = true;
                }
            }
        }

        private void DocumentWorkspace_Scroll(object sender, System.Windows.Forms.ScrollEventArgs e)
        {
            OnScroll(e);
        }

        private void DocumentWorkspace_Layout(object sender, LayoutEventArgs e)
        {
            UpdateSnapObstacle();
        }

        private void ViewConfigStrip_ZoomBasisChanged(object sender, EventArgs e)
        {
            if (ActiveDocumentWorkspace != null)
            {
                if (ActiveDocumentWorkspace.ZoomBasis != this.toolBar.ViewConfigStrip.ZoomBasis)
                {
                    ActiveDocumentWorkspace.ZoomBasis = this.toolBar.ViewConfigStrip.ZoomBasis;
                }
            }
        }

        private void DocumentWorkspace_ZoomBasisChanged(object sender, EventArgs e)
        {
            if (this.toolBar.ViewConfigStrip.ZoomBasis != this.ActiveDocumentWorkspace.ZoomBasis)
            {
                this.toolBar.ViewConfigStrip.ZoomBasis = this.ActiveDocumentWorkspace.ZoomBasis;
            }
        }

        private void ViewConfigStrip_ZoomScaleChanged(object sender, EventArgs e)
        {
            if (ActiveDocumentWorkspace != null)
            {
                if (this.toolBar.ViewConfigStrip.ZoomBasis == ZoomBasis.ScaleFactor)
                {
                    this.activeDocumentWorkspace.ScaleFactor = this.toolBar.ViewConfigStrip.ScaleFactor;
                }
            }
        }

        private void DocumentWorkspace_RulersEnabledChanged(object sender, EventArgs e)
        {
            this.toolBar.ViewConfigStrip.RulersEnabled = this.activeDocumentWorkspace.RulersEnabled;
            this.globalRulersChoice = this.activeDocumentWorkspace.RulersEnabled;
            PerformLayout();
            ActiveDocumentWorkspace.UpdateRulerSelectionTinting();

            Settings.CurrentUser.SetBoolean(SettingNames.Rulers, this.activeDocumentWorkspace.RulersEnabled);
        }

        private void Environment_AntiAliasingChanged(object sender, EventArgs e)
        {
            this.toolBar.ToolConfigStrip.AntiAliasing = AppEnvironment.AntiAliasing;
        }

        private void ViewConfigStrip_ZoomIn(object sender, EventArgs e)
        {
            if (this.ActiveDocumentWorkspace != null)
            {
                this.ActiveDocumentWorkspace.ZoomIn();
            }
        }

        private void ViewConfigStrip_ZoomOut(object sender, EventArgs e)
        {
            if (this.ActiveDocumentWorkspace != null)
            {
                this.ActiveDocumentWorkspace.ZoomOut();
            }
        }

        private void ViewConfigStrip_UnitsChanged(object sender, EventArgs e)
        {
            if (this.toolBar.ViewConfigStrip.Units != MeasurementUnit.Pixel)
            {
                Settings.CurrentUser.SetString(SettingNames.LastNonPixelUnits, this.toolBar.ViewConfigStrip.Units.ToString());
            }

            if (this.activeDocumentWorkspace != null)
            {
                this.activeDocumentWorkspace.Units = this.Units;
            }

            Settings.CurrentUser.SetString(SettingNames.Units, this.toolBar.ViewConfigStrip.Units.ToString());

            UpdateDocInfoInStatusBar();
            this.statusBar.CursorInfoText = string.Empty;

            OnUnitsChanged();
        }

        private void OnDrawConfigStripAlphaBlendingChanged(object sender, EventArgs e)
        {
            if (AppEnvironment.AlphaBlending != widgets.ToolConfigStrip.AlphaBlending)
            {
                AppEnvironment.AlphaBlending = widgets.ToolConfigStrip.AlphaBlending;
            }
        }

        public event EventHandler StatusChanged;
        private void OnStatusChanged()
        {
            if (StatusChanged != null)
            {
                StatusChanged(this, EventArgs.Empty);
            }
        }

        private void OnDocumentWorkspaceStatusChanged(object sender, EventArgs e)
        {
            OnStatusChanged();
            UpdateStatusBarContextStatus();
        }

        private void UpdateStatusBarContextStatus()
        {
            if (ActiveDocumentWorkspace != null)
            {
                this.statusBar.ContextStatusText = this.activeDocumentWorkspace.StatusText;
                this.statusBar.ContextStatusImage = this.activeDocumentWorkspace.StatusIcon;
            }
            else
            {
                this.statusBar.ContextStatusText = string.Empty;
                this.statusBar.ContextStatusImage = null;
            }
        }

        private static bool NullGetThumbnailImageAbort()
        {
            return false;
        }
        
        /// <summary>
        /// Creates a blank document of the given size in a new workspace, and activates that workspace.
        /// </summary>
        /// <remarks>
        /// If isInitial=true, then last workspace added by this method is kept track of, and if it is not modified by
        /// the time the next workspace is added, then it will be removed.
        /// </remarks>
        /// <returns>true if everything was successful, false if there wasn't enough memory</returns>
        public bool CreateBlankDocumentInNewWorkspace(Size size, MeasurementUnit dpuUnit, double dpu, bool isInitial)
        {
            DocumentWorkspace dw1 = this.activeDocumentWorkspace;
            if (dw1 != null)
            {
                dw1.SuspendRefresh();
            }

            try
            {
                Document untitled = new Document(size.Width, size.Height);
                untitled.DpuUnit = dpuUnit;
                untitled.DpuX = dpu;
                untitled.DpuY = dpu;

                BitmapLayer bitmapLayer;

                try
                {
                    using (new WaitCursorChanger(this))
                    {
                        bitmapLayer = Layer.CreateBackgroundLayer(size.Width, size.Height);
                    }
                }

                catch (OutOfMemoryException)
                {
                    Utility.ErrorBox(this, PdnResources.GetString("NewImageAction.Error.OutOfMemory"));
                    return false;
                }

                using (new WaitCursorChanger(this))
                {
                    bool focused = false;

                    if (this.ActiveDocumentWorkspace != null && this.ActiveDocumentWorkspace.Focused)
                    {
                        focused = true;
                    }

                    untitled.Layers.Add(bitmapLayer);

                    DocumentWorkspace dw = this.AddNewDocumentWorkspace();
                    this.Widgets.DocumentStrip.LockDocumentWorkspaceDirtyValue(dw, false);
                    dw.SuspendRefresh();

                    try
                    {
                        dw.Document = untitled;
                    }

                    catch (OutOfMemoryException)
                    {
                        Utility.ErrorBox(this, PdnResources.GetString("NewImageAction.Error.OutOfMemory"));
                        RemoveDocumentWorkspace(dw);
                        untitled.Dispose();
                        return false;
                    }

                    dw.ActiveLayer = (Layer)dw.Document.Layers[0];

                    this.ActiveDocumentWorkspace = dw;

                    dw.SetDocumentSaveOptions(null, null, null);
                    dw.History.ClearAll();
                    dw.History.PushNewMemento(
                        new NullHistoryMemento(PdnResources.GetString("NewImageAction.Name"), 
                        this.FileNewIcon));

                    dw.Document.Dirty = false;
                    dw.ResumeRefresh();

                    if (isInitial)
                    {
                        this.initialWorkspace = dw;
                    }

                    if (focused)
                    {
                        this.ActiveDocumentWorkspace.Focus();
                    }

                    this.Widgets.DocumentStrip.UnlockDocumentWorkspaceDirtyValue(dw);
                }
            }

            finally
            {
                if (dw1 != null)
                {
                    dw1.ResumeRefresh();
                }
            }

            return true;
        }

        public bool OpenFilesInNewWorkspace(string[] fileNames)
        {
            if (IsDisposed)
            {
                return false;
            }

            bool result = true;

            foreach (string fileName in fileNames)
            {
                result &= OpenFileInNewWorkspace(fileName);

                if (!result)
                {
                    break;
                }
            }

            return result;
        }

        public bool OpenFileInNewWorkspace(string fileName)
        {
            return OpenFileInNewWorkspace(fileName, true);
        }

        public bool OpenFileInNewWorkspace(string fileName, bool addToMruList)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }

            if (fileName.Length == 0)
            {
                throw new ArgumentOutOfRangeException("fileName.Length == 0");
            }

            PdnBaseForm.UpdateAllForms();

            FileType fileType;
            Document document;

            this.widgets.StatusBarProgress.ResetProgressStatusBar();

            ProgressEventHandler progressCallback =
                delegate(object sender, ProgressEventArgs e)
                {
                    this.widgets.StatusBarProgress.SetProgressStatusBar(e.Percent);
                };

            document = DocumentWorkspace.LoadDocument(this, fileName, out fileType, progressCallback);
            this.widgets.StatusBarProgress.EraseProgressStatusBar();

            if (document == null)
            {
                this.Cursor = Cursors.Default;
            }
            else
            {
                using (new WaitCursorChanger(this))
                {
                    DocumentWorkspace dw = AddNewDocumentWorkspace();
                    Widgets.DocumentStrip.LockDocumentWorkspaceDirtyValue(dw, false);

                    try
                    {
                        dw.Document = document;
                    }

                    catch (OutOfMemoryException)
                    {
                        Utility.ErrorBox(this, PdnResources.GetString("LoadImage.Error.OutOfMemoryException"));
                        RemoveDocumentWorkspace(dw);
                        document.Dispose();
                        return false;
                    } 

                    dw.ActiveLayer = (Layer)document.Layers[0];

                    dw.SetDocumentSaveOptions(fileName, fileType, null);

                    this.ActiveDocumentWorkspace = dw;

                    dw.History.ClearAll();

                    dw.History.PushNewMemento(
                        new NullHistoryMemento(
                            PdnResources.GetString("OpenImageAction.Name"),
                            this.ImageFromDiskIcon));

                    document.Dirty = false;
                    Widgets.DocumentStrip.UnlockDocumentWorkspaceDirtyValue(dw);
                }

                if (document != null)
                {
                    ActiveDocumentWorkspace.ZoomBasis = ZoomBasis.FitToWindow;
                }

                // add to MRU list
                if (addToMruList)
                {
                    ActiveDocumentWorkspace.AddToMruList();
                }

                this.toolBar.DocumentStrip.SyncThumbnails();

                WarnAboutSavedWithVersion(document.SavedWithVersion);
            }

            if (ActiveDocumentWorkspace != null)
            {
                ActiveDocumentWorkspace.Focus();
            }

            return document != null;
        }

        private void WarnAboutSavedWithVersion(Version savedWith)
        {
            // warn about version?
            // 2.1 Build 1897 signifies when the file format changed and broke backwards compatibility (for saving)
            // 2.1 Build 1921 signifies when MemoryBlock was upgraded to support 64-bits, which broke it again
            // 2.1 Build 1924 upgraded to "unimportant ordering" for MemoryBlock serialization so we can to faster multiproc saves
            //                (in v2.5 we always save in order, although that doesn't change the file format's laxness)
            // 2.5 Build 2105 changed the way PropertyItems are serialized
            // 2.6 Build      upgrade to .NET 2.0, does not appear to be compatible with 2.5 and earlier files as a result
            if (savedWith < new Version(2, 6, 0))
            {
                Version ourVersion = PdnInfo.GetVersion();
                Version ourVersion2 = new Version(ourVersion.Major, ourVersion.Minor);
                Version ourVersion3 = new Version(ourVersion.Major, ourVersion.Minor, ourVersion.Build);

                int fields;

                if (savedWith < ourVersion2)
                {
                    fields = 2;
                }
                else
                {
                    fields = 3;
                }

                string format = PdnResources.GetString("SavedWithOlderVersion.Format");
                string text = string.Format(format, savedWith.ToString(fields), ourVersion.ToString(fields));

                // TODO: should we even bother to inform them? It is probably more annoying than not,
                //       especially since older versions will say "Hey this file is corrupt OR saved with a newer version"
                //Utility.InfoBox(this, text);
            }
        }

        /// <summary>
        /// Computes what the size of a new document should be. If the screen is in a normal,
        /// wider-than-tall (landscape) mode then it returns 800x600. If the screen is in a
        /// taller-than-wide (portrait) mode then it retusn 600x800. If the screen is square
        /// then it returns 800x600.
        /// </summary>
        public Size GetNewDocumentSize()
        {
            PdnBaseForm findForm = this.FindForm() as PdnBaseForm;

            if (findForm != null && findForm.ScreenAspect < 1.0)
            {
                return new Size(600, 800);
            }
            else
            {
                return new Size(800, 600);
            }
        }

        private void CommonActionsStrip_ButtonClick(object sender, EventArgs<CommonAction> e)
        {
            CommonAction ca = e.Data;

            switch (ca)
            {
                case CommonAction.New:
                    PerformAction(new NewImageAction());
                    break;

                case CommonAction.Open:
                    PerformAction(new OpenFileAction());
                    break;

                case CommonAction.Save:
                    if (ActiveDocumentWorkspace != null)
                    {
                        ActiveDocumentWorkspace.DoSave();
                    }
                    break;

                case CommonAction.Print:
                    if (ActiveDocumentWorkspace != null)
                    {
                        PrintAction pa = new PrintAction();
                        ActiveDocumentWorkspace.PerformAction(pa);
                    }
                    break;

                case CommonAction.Cut:
                    if (ActiveDocumentWorkspace != null)
                    {
                        CutAction cutAction = new CutAction();
                        cutAction.PerformAction(ActiveDocumentWorkspace);
                    }

                    break;

                case CommonAction.Copy:
                    if (ActiveDocumentWorkspace != null)
                    {
                        CopyToClipboardAction ctca = new CopyToClipboardAction(ActiveDocumentWorkspace);
                        ctca.PerformAction();
                    }
                    break;

                case CommonAction.Paste:
                    if (ActiveDocumentWorkspace != null)
                    {
                        PasteAction pa = new PasteAction(ActiveDocumentWorkspace);
                        pa.PerformAction();
                    }

                    break;

                case CommonAction.CropToSelection:
                    if (ActiveDocumentWorkspace != null)
                    {
                        using (new PushNullToolMode(ActiveDocumentWorkspace))
                        {
                            ActiveDocumentWorkspace.ExecuteFunction(new CropToSelectionFunction());
                        }
                    }

                    break;

                case CommonAction.Deselect:
                    if (ActiveDocumentWorkspace != null)
                    {
                        ActiveDocumentWorkspace.ExecuteFunction(new DeselectFunction());
                    }
                    break;

                case CommonAction.Undo:
                    if (ActiveDocumentWorkspace != null)
                    {
                        ActiveDocumentWorkspace.PerformAction(new HistoryUndoAction());
                    }
                    break;

                case CommonAction.Redo:
                    if (ActiveDocumentWorkspace != null)
                    {
                        ActiveDocumentWorkspace.PerformAction(new HistoryRedoAction());
                    }
                    break;

                default:
                    throw new InvalidEnumArgumentException("e.Data");
            }

            if (ActiveDocumentWorkspace != null)
            {
                ActiveDocumentWorkspace.Focus();
            }
        }
    }
}
