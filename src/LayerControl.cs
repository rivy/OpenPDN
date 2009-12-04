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
using System.Windows.Forms;
using System.Diagnostics;

namespace PaintDotNet
{
    internal class LayerControl 
        : UserControl
    {
        private class PanelWithLayout
            : PanelEx
        {
            private LayerControl parentLayerControl;
            public LayerControl ParentLayerControl
            {
                get
                {
                    return this.parentLayerControl;
                }

                set
                {
                    this.parentLayerControl = value;
                }
            }

            public PanelWithLayout()
            {
                this.HideHScroll = true;
            }

            public void PositionLayers()
            {
                if (this.parentLayerControl != null &&
                    this.parentLayerControl.layerControls != null)
                {
                    int cursor = this.AutoScrollPosition.Y;
                    int newWidth = this.ClientRectangle.Width;

                    for (int i = this.parentLayerControl.layerControls.Count - 1; i >= 0; --i)
                    {
                        LayerElement lec = this.parentLayerControl.layerControls[i];
                        lec.Width = newWidth;
                        lec.Top = cursor;
                        cursor += lec.Height;
                    }
                }
            }

            protected override void OnResize(EventArgs eventargs)
            {
                SystemLayer.UI.SuspendControlPainting(this);
                PositionLayers();
                this.AutoScrollPosition = new Point(0, -this.AutoScrollOffset.Y);
                base.OnResize(eventargs);
                SystemLayer.UI.ResumeControlPainting(this);
                Invalidate(true);
            }

            protected override void OnLayout(LayoutEventArgs levent)
            {
                PositionLayers();
                base.OnLayout(levent);
            }
        }

        public void PositionLayers()
        {
            this.layerControlPanel.PositionLayers();
        }

        private EventHandler elementClickDelegate;
        private EventHandler elementDoubleClickDelegate;
        private EventHandler documentChangedDelegate;
        private EventHandler<EventArgs<Document>> documentChangingDelegate;
        private EventHandler layerChangedDelegate;
        private KeyEventHandler keyUpDelegate;
        private IndexEventHandler layerInsertedDelegate;
        private IndexEventHandler layerRemovedDelegate;
        
        private int elementHeight;
        private int thumbnailSize;
        
        private AppWorkspace appWorkspace;
        private Document document;

        private List<LayerElement> layerControls;
        private PanelWithLayout layerControlPanel;

        private ThumbnailManager thumbnailManager;

        [Browsable(false)]
        public LayerElement[] Layers
        {
            get
            {
                if (layerControls == null)
                {
                    return new LayerElement[0];
                }
                else
                {
                    return this.layerControls.ToArray();
                }
            }
        }

        public Layer ActiveLayer
        {
            get
            {
                int[] selected = SelectedLayerIndexes;

                if (selected.Length == 1)
                {
                    return this.Layers[selected[0]].Layer;
                }
                else
                {
                    return null;
                }
            }
        }

        public int ActiveLayerIndex
        {
            get
            {
                int[] selected = SelectedLayerIndexes;

                if (selected.Length == 1)
                {
                    return selected[0];
                }
                else
                {
                    return -1;
                }
            }
        }

        private int[] SelectedLayerIndexes
        {
            get
            {
                LayerElement[] layers = this.Layers;
                List<int> layerIndexes = new List<int>();

                for (int i = 0; i < layers.Length; ++i)
                {
                    if (layers[i].IsSelected)
                    {
                        layerIndexes.Add(i);
                    }
                }

                return layerIndexes.ToArray();
            }
        }

        public void ClearLayerSelection()
        {
            LayerElement[] layers = this.Layers;

            for (int i = 0; i < layers.Length; ++i)
            {
                layers[i].IsSelected = false;
            }
        }

        public new BorderStyle BorderStyle
        {
            get
            {
                return layerControlPanel.BorderStyle;
            }

            set
            {
                layerControlPanel.BorderStyle = value;
            }
        }

        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public LayerControl()
        {
            this.elementHeight = 6 + SystemLayer.UI.ScaleWidth(LayerElement.ThumbSizePreScaling);

            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            elementClickDelegate = new EventHandler(ElementClickHandler);
            elementDoubleClickDelegate = new EventHandler(ElementDoubleClickHandler);
            documentChangedDelegate = new EventHandler(DocumentChangedHandler);
            documentChangingDelegate = DocumentChangingHandler;
            layerInsertedDelegate = new IndexEventHandler(LayerInsertedHandler);
            layerRemovedDelegate = new IndexEventHandler(LayerRemovedHandler);
            layerChangedDelegate = new EventHandler(LayerChangedHandler);
            keyUpDelegate = new KeyEventHandler(KeyUpHandler);

            this.thumbnailManager = new ThumbnailManager(this);
            this.thumbnailSize = SystemLayer.UI.ScaleWidth(LayerElement.ThumbSizePreScaling);
            layerControls = new List<LayerElement>();
        }

        private void SetupNewDocument(Document newDocument)
        {
            //this.thumbnailManager.ClearQueue();

            // Subscribe to the eevents
            this.document = newDocument;
            this.document.Layers.Inserted += layerInsertedDelegate;
            this.document.Layers.RemovedAt += layerRemovedDelegate;

            SystemLayer.UI.SuspendControlPainting(this.layerControlPanel);

            for (int i = 0; i < this.document.Layers.Count; ++i)
            {
                this.LayerInsertedHandler(this, new IndexEventArgs(i));
            }

            if (this.appWorkspace != null)
            {
                foreach (LayerElement lec in layerControls)
                {
                    if (lec.Layer == appWorkspace.ActiveDocumentWorkspace.ActiveLayer)
                    {
                        lec.IsSelected = true;
                    }
                    else
                    {
                        lec.IsSelected = false;
                    }
                }
            }

            SystemLayer.UI.ResumeControlPainting(this.layerControlPanel);
            this.layerControlPanel.Invalidate(true);

            OnActiveLayerChanged(ActiveLayer);
        }

        private void TearDownOldDocument()
        {
            SuspendLayout();

            foreach (LayerElement lec in this.layerControls)
            {
                lec.Click -= elementClickDelegate;
                lec.DoubleClick -= elementDoubleClickDelegate;
                lec.KeyUp -= keyUpDelegate;
                lec.Layer = null;
                layerControlPanel.Controls.Remove(lec);
                lec.Dispose();
            }

            ResumeLayout(true);

            this.layerControls.Clear();

            //this.thumbnailManager.ClearQueue();

            // Unsubscribe to the Events
            if (this.document != null)
            {
                this.document.Layers.Inserted -= layerInsertedDelegate;
                this.document.Layers.RemovedAt -= layerRemovedDelegate;
                this.document = null;
            }
        }
        
        private void DocumentChangingHandler(object sender, EventArgs<Document> e)
        {
            TearDownOldDocument();
        }

        private void DocumentChangedHandler(object sender, EventArgs e)
        {
            SetupNewDocument(appWorkspace.ActiveDocumentWorkspace.Document);
        }

        private void LayerRemovedHandler(object sender, IndexEventArgs e)
        {
            LayerElement lec = layerControls[e.Index];
            this.thumbnailManager.RemoveFromQueue(lec.Layer);
            lec.Click -= this.elementClickDelegate;
            lec.DoubleClick -= this.elementDoubleClickDelegate;
            lec.KeyUp -= keyUpDelegate;
            lec.Layer = null;
            layerControls.Remove(lec);
            layerControlPanel.Controls.Remove(lec);
            lec.Dispose();
            PerformLayout();
        }

        private void InitializeLayerElement(LayerElement lec, Layer l)
        {
            lec.Height = elementHeight;
            lec.Layer = l;
            lec.Click += elementClickDelegate;
            lec.DoubleClick += elementDoubleClickDelegate;
            lec.KeyUp += keyUpDelegate;
            lec.IsSelected = false;
        }

        private void SetActive(LayerElement lec)
        {
            SetActive(lec.Layer);
        }

        private void SetActive(Layer layer)
        {
            foreach (LayerElement lec in layerControls)
            {
                bool active = (lec.Layer == layer);
                lec.IsSelected = active;

                if (active)
                {
                    OnActiveLayerChanged(lec.Layer);
                    layerControlPanel.ScrollControlIntoView(lec);
                    lec.Select();
                    Update();
                }
            }
        }

        private void LayerInsertedHandler(object sender, IndexEventArgs e)
        {
            this.SuspendLayout();
            this.layerControlPanel.SuspendLayout();
            Layer layer = (Layer)this.document.Layers[e.Index];
            LayerElement lec = new LayerElement();
            lec.ThumbnailManager = this.thumbnailManager;
            lec.ThumbnailSize = this.thumbnailSize;
            InitializeLayerElement(lec, layer);
            layerControls.Insert(e.Index, lec);
            layerControlPanel.Controls.Add(lec);
            layerControlPanel.ScrollControlIntoView(lec);
            lec.Select();
            SetActive(lec);
            lec.RefreshPreview();
            this.layerControlPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.layerControlPanel.PerformLayout();
            PerformLayout();

            Refresh();
        }

        public void RefreshPreviews()
        {
            for (int i = 0; i < this.layerControls.Count; ++i)
            {
                this.layerControls[i].RefreshPreview();
            }
        }

        public event EventHandler RelinquishFocus;
        protected void OnRelinquishFocus()
        {
            if (RelinquishFocus != null)
            {
                RelinquishFocus(this, EventArgs.Empty);
            }
        }

        protected override void OnClick(EventArgs e)
        {
            OnRelinquishFocus();
            base.OnClick(e);
        }

        /// <summary>
        /// This event is raised whenever the user clicks on a layer within the
        /// LayerControl to activate it.
        /// </summary>
        public event EventHandler<EventArgs<Layer>> ClickedOnLayer;
        private void OnClickedOnLayer(Layer layer)
        {
            if (ClickedOnLayer != null)
            {
                ClickedOnLayer(this, new EventArgs<Layer>(layer));
            }
        }

        /// <summary>
        /// This event is raised whenever the selected layer is changed. Note that
        /// this can occur without user intervention, which distinguishes this event
        /// from ClickedOnLayer.
        /// </summary>
        public event EventHandler<EventArgs<Layer>> ActiveLayerChanged;
        private void OnActiveLayerChanged(Layer layer)
        {
            if (ActiveLayerChanged != null)
            {
                ActiveLayerChanged(this, new EventArgs<Layer>(layer));
            }
        }

        public event EventHandler<EventArgs<Layer>> DoubleClickedOnLayer;
        private void OnDoubleClickedOnLayer(Layer layer)
        {
            if (DoubleClickedOnLayer != null)
            {
                DoubleClickedOnLayer(this, new EventArgs<Layer>(layer));
            }
        }

        private void ElementClickHandler(object sender, EventArgs e)
        {
            LayerElement lec = (LayerElement)sender;

            if (Control.ModifierKeys == Keys.Control)
            {
                lec.IsSelected = !lec.IsSelected;
            }
            else
            {
                ClearLayerSelection();
                lec.IsSelected = true;
            }

            SetActive(lec);
            OnClickedOnLayer(lec.Layer);
        }

        private void ElementDoubleClickHandler(object sender, EventArgs e)
        {
            OnDoubleClickedOnLayer(((LayerElement)sender).Layer);
        }
    
        private void LayerChangedHandler(object sender, EventArgs e)
        {
            SetActive(appWorkspace.ActiveDocumentWorkspace.ActiveLayer);
        }

        public void SuspendLayerPreviewUpdates()
        {
            foreach (LayerElement element in this.layerControls)
            {
                element.SuspendPreviewUpdates();
            }
        }

        public void ResumeLayerPreviewUpdates()
        {
            foreach (LayerElement element in this.layerControls)
            {
                element.ResumePreviewUpdates();
            }
        }

        private void KeyUpHandler(object sender, KeyEventArgs e)
        {
            this.OnKeyUp(e);
        }
    
        [Browsable(false)]
        public AppWorkspace AppWorkspace
        {
            get
            {
                return this.appWorkspace;
            }

            set
            {
                if (this.appWorkspace != value)
                {
                    if (this.appWorkspace != null)
                    {
                        TearDownOldDocument();

                        this.appWorkspace.ActiveDocumentWorkspaceChanging -= Workspace_ActiveDocumentWorkspaceChanging;
                        this.appWorkspace.ActiveDocumentWorkspaceChanged -= Workspace_ActiveDocumentWorkspaceChanged;
                    }

                    this.appWorkspace = value;

                    if (this.appWorkspace != null)
                    {
                        this.appWorkspace.ActiveDocumentWorkspaceChanging += Workspace_ActiveDocumentWorkspaceChanging;
                        this.appWorkspace.ActiveDocumentWorkspaceChanged += Workspace_ActiveDocumentWorkspaceChanged;

                        if (this.appWorkspace.ActiveDocumentWorkspace != null)
                        {
                            SetupNewDocument(this.appWorkspace.ActiveDocumentWorkspace.Document);
                        }
                    }
                }
            }
        }

        private void Workspace_ActiveDocumentWorkspaceChanging(object sender, EventArgs e)
        {
            TearDownOldDocument();

            if (this.appWorkspace.ActiveDocumentWorkspace != null)
            {
                this.appWorkspace.ActiveDocumentWorkspace.DocumentChanging -= documentChangingDelegate;
                this.appWorkspace.ActiveDocumentWorkspace.DocumentChanged -= documentChangedDelegate;
                this.appWorkspace.ActiveDocumentWorkspace.ActiveLayerChanged -= layerChangedDelegate;
            }
        }

        private void Workspace_ActiveDocumentWorkspaceChanged(object sender, EventArgs e)
        {
            if (this.appWorkspace.ActiveDocumentWorkspace != null)
            {
                appWorkspace.ActiveDocumentWorkspace.DocumentChanging += documentChangingDelegate;
                appWorkspace.ActiveDocumentWorkspace.DocumentChanged += documentChangedDelegate;
                appWorkspace.ActiveDocumentWorkspace.ActiveLayerChanged += layerChangedDelegate;

                if (appWorkspace.ActiveDocumentWorkspace.Document != null)
                {
                    SetupNewDocument(appWorkspace.ActiveDocumentWorkspace.Document);
                }
            }
        }

        [Browsable(false)]
        public Document Document
        {
            get
            {
                return this.document;
            }

            set
            {
                if (this.appWorkspace != null)
                {
                    throw new InvalidOperationException("Workspace property is already set");
                }

                if (this.document != null)
                {
                    TearDownOldDocument();
                }

                if (value != null)
                {
                    SetupNewDocument(value);
                }
            }
        }
        
        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                    components = null;
                }

                if (this.thumbnailManager != null)
                {
                    this.thumbnailManager.Dispose();
                    this.thumbnailManager = null;
                }
            }

            base.Dispose(disposing);
        }

        private void LayerControlPanel_Click(object sender, EventArgs e)
        {
            OnRelinquishFocus();
        }

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.layerControlPanel = new PanelWithLayout();
            this.SuspendLayout();
            // 
            // layerControlPanel
            // 
            this.layerControlPanel.AutoScroll = true;
            this.layerControlPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layerControlPanel.Location = new System.Drawing.Point(0, 0);
            this.layerControlPanel.Name = "layerControlPanel";
            this.layerControlPanel.ParentLayerControl = this;
            this.layerControlPanel.Size = new System.Drawing.Size(150, 150);
            this.layerControlPanel.TabIndex = 2;
            this.layerControlPanel.Click += new EventHandler(LayerControlPanel_Click);
            // 
            // LayerControl
            // 
            this.Controls.Add(this.layerControlPanel);
            this.Name = "LayerControl";
            this.ResumeLayout(false);

        }

        #endregion
    }
}
