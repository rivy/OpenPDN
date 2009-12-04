/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.SystemLayer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace PaintDotNet
{
    internal class DocumentStrip
        : ImageStrip,
          IDocumentList
    {
        private object thumbsLock = new object();

        private List<DocumentWorkspace> documents = new List<DocumentWorkspace>();
        private List<ImageStrip.Item> documentButtons = new List<ImageStrip.Item>();
        private Dictionary<DocumentWorkspace, ImageStrip.Item> dw2button = new Dictionary<DocumentWorkspace, ImageStrip.Item>();
        private Dictionary<DocumentWorkspace, RenderArgs> thumbs = new Dictionary<DocumentWorkspace, RenderArgs>();
        private ThumbnailManager thumbnailManager;
        private DocumentWorkspace selectedDocument = null;
        private bool ensureSelectedIsVisible = true;
        private int suspendThumbnailUpdates = 0;

        public void SuspendThumbnailUpdates()
        {
            ++this.suspendThumbnailUpdates;
        }

        public void ResumeThumbnailUpdates()
        {
            --this.suspendThumbnailUpdates;
        }

        public void SyncThumbnails()
        {
            if (!this.thumbnailManager.IsDisposed)
            {
                using (new WaitCursorChanger(this))
                {
                    this.thumbnailManager.DrainQueue();
                }

                Refresh();
            }
        }

        public int ThumbnailUpdateLatency
        {
            get
            {
                return this.thumbnailManager.UpdateLatency;
            }

            set
            {
                this.thumbnailManager.UpdateLatency = value;
            }
        }

        public bool EnsureSelectedIsVisible
        {
            get
            {
                return this.ensureSelectedIsVisible;
            }

            set
            {
                if (this.ensureSelectedIsVisible != value)
                {
                    this.ensureSelectedIsVisible = value;
                    PerformLayout();
                }
            }
        }

        public DocumentWorkspace[] DocumentList
        {
            get
            {
                return this.documents.ToArray();
            }
        }

        public Image[] DocumentThumbnails
        {
            get
            {
                SyncThumbnails();

                Image[] thumbnails = new Image[this.documents.Count];

                for (int i = 0; i < thumbnails.Length; ++i)
                {
                    DocumentWorkspace dw = this.documents[i];
                    RenderArgs ra;

                    if (!this.thumbs.TryGetValue(dw, out ra))
                    {
                        thumbnails[i] = null;
                    }
                    else
                    {
                        thumbnails[i] = ra.Bitmap;
                    }
                }

                return thumbnails;
            }
        }

        public int DocumentCount
        {
            get
            {
                return this.documents.Count;
            }
        }

        public event EventHandler DocumentListChanged;
        protected virtual void OnDocumentListChanged()
        {
            if (DocumentListChanged != null)
            {
                DocumentListChanged(this, EventArgs.Empty);
            }
        }

        public DocumentWorkspace SelectedDocument
        {
            get
            {
                return this.selectedDocument;
            }

            set
            {
                if (!this.documents.Contains(value))
                {
                    throw new ArgumentException("DocumentWorkspace isn't being tracked by this instance of DocumentStrip");
                }

                if (this.selectedDocument != value)
                {
                    SelectDocumentWorkspace(value);
                    OnDocumentClicked(value, DocumentClickAction.Select);
                    Refresh();
                }
            }
        }

        public int SelectedDocumentIndex
        {
            get
            {
                return this.documents.IndexOf(this.selectedDocument);
            }
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            Size itemSize = ItemSize;
            int preferredWidth;

            if (this.ItemCount == 0)
            {
                preferredWidth = 0;
            }
            else
            {
                preferredWidth = itemSize.Width * DocumentCount;
            }

            Size preferredSize = new Size(preferredWidth, itemSize.Height);
            return preferredSize;
        }

        private bool OnDigitHotKeyPressed(Keys keys)
        {
            // strip off the Alt and Control stuff
            keys &= ~Keys.Alt;
            keys &= ~Keys.Control;

            if (keys < Keys.D0 || keys > Keys.D9)
            {
                return false;
            }

            int digit = (int)keys - (int)Keys.D0;
            int index;

            if (digit == 0)
            {
                index = 9;
            }
            else
            {
                index = digit - 1;
            }

            if (index < this.documents.Count)
            {
                PerformItemClick(index, ItemPart.Image, MouseButtons.Left);
                return true;
            }
            else
            {
                return false;
            }
        }

        public DocumentStrip()
        {
            PdnBaseForm.RegisterFormHotKey(Keys.Control | Keys.Tab, OnNextTabHotKeyPressed);
            PdnBaseForm.RegisterFormHotKey(Keys.Control | Keys.PageDown, OnNextTabHotKeyPressed);
            PdnBaseForm.RegisterFormHotKey(Keys.Control | Keys.Shift | Keys.Tab, OnPreviousTabHotKeyPressed);
            PdnBaseForm.RegisterFormHotKey(Keys.Control | Keys.PageUp, OnPreviousTabHotKeyPressed);

            thumbnailManager = new ThumbnailManager(this); // allow for a 1px black border

            InitializeComponent();

            for (int i = 0; i <= 9; ++i)
            {
                Keys digit = Utility.LetterOrDigitCharToKeys((char)(i + '0'));
                PdnBaseForm.RegisterFormHotKey(Keys.Control | digit, OnDigitHotKeyPressed);
                PdnBaseForm.RegisterFormHotKey(Keys.Alt | digit, OnDigitHotKeyPressed);
            }

            this.ShowCloseButtons = true;
        }

        private bool OnNextTabHotKeyPressed(Keys keys)
        {
            bool processed = NextTab();
            return processed;
        }

        private bool OnPreviousTabHotKeyPressed(Keys keys)
        {
            bool processed = PreviousTab();
            return processed;
        }

        public bool NextTab()
        {
            bool changed = false;

            if (this.selectedDocument != null)
            {
                int currentIndex = this.documents.IndexOf(this.selectedDocument);
                int newIndex = (currentIndex + 1) % this.documents.Count;
                SelectedDocument = this.documents[newIndex];
                changed = true;
            }

            return changed;
        }

        public bool PreviousTab()
        {
            bool changed = false;

            if (this.selectedDocument != null)
            {
                int currentIndex = this.documents.IndexOf(this.selectedDocument);
                int newIndex = (currentIndex + (this.documents.Count - 1)) % this.documents.Count;
                SelectedDocument = this.documents[newIndex];
                changed = true;
            }

            return changed;
        }

        private void InitializeComponent()
        {
            this.Name = "DocumentStrip";
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                while (this.documents.Count > 0)
                {
                    RemoveDocumentWorkspace(this.documents[this.documents.Count - 1]);
                }

                if (this.thumbnailManager != null)
                {
                    this.thumbnailManager.Dispose();
                    this.thumbnailManager = null;
                }

                foreach (DocumentWorkspace dw in this.thumbs.Keys)
                {
                    RenderArgs ra = this.thumbs[dw];
                    ra.Dispose();
                }

                this.thumbs.Clear();
                this.thumbs = null;
            }

            base.Dispose(disposing);
        }

        protected override void OnScrollArrowClicked(ArrowDirection arrowDirection)
        {
            int sign = 0;

            switch (arrowDirection)
            {
                case ArrowDirection.Left:
                    sign = -1;
                    break;

                case ArrowDirection.Right:
                    sign = +1;
                    break;
            }

            int delta = ItemSize.Width;

            ScrollOffset += sign * delta;

            base.OnScrollArrowClicked(arrowDirection);
        }
        
        protected override void OnItemClicked(Item item, ItemPart itemPart, MouseButtons mouseButtons)
        {
            DocumentWorkspace dw = item.Tag as DocumentWorkspace;

            if (dw != null)
            {
                switch (itemPart)
                {
                    case ItemPart.None:
                        // do nothing
                        break;

                    case ItemPart.CloseButton:
                        if (mouseButtons == MouseButtons.Left)
                        {
                            OnDocumentClicked(dw, DocumentClickAction.Close);
                        }
                        break;

                    case ItemPart.Image:
                        if (mouseButtons == MouseButtons.Left)
                        {
                            SelectedDocument = dw;
                        }
                        else if (mouseButtons == MouseButtons.Right)
                        {
                            // TODO: right click menu
                        }
                        break;

                    default:
                        throw new InvalidEnumArgumentException();
                }
            }

            base.OnItemClicked(item, itemPart, mouseButtons);
        }

        public event EventHandler<EventArgs<Pair<DocumentWorkspace, DocumentClickAction>>> DocumentClicked;
        protected virtual void OnDocumentClicked(DocumentWorkspace dw, DocumentClickAction action)
        {
            if (DocumentClicked != null)
            {
                DocumentClicked(this, new EventArgs<Pair<DocumentWorkspace, DocumentClickAction>>(
                    Pair.Create(dw, action)));
            }
        }

        public void UnlockDocumentWorkspaceDirtyValue(DocumentWorkspace unlockMe)
        {
            Item docItem = this.dw2button[unlockMe];
            docItem.UnlockDirtyValue();
        }

        public void LockDocumentWorkspaceDirtyValue(DocumentWorkspace lockMe, bool forceDirtyValue)
        {
            Item docItem = this.dw2button[lockMe];
            docItem.LockDirtyValue(forceDirtyValue);
        }

        public void AddDocumentWorkspace(DocumentWorkspace addMe)
        {
            this.documents.Add(addMe);

            ImageStrip.Item docButton = new ImageStrip.Item();
            docButton.Image = null;
            docButton.Tag = addMe;

            AddItem(docButton);
            this.documentButtons.Add(docButton);

            addMe.CompositionUpdated += Workspace_CompositionUpdated;

            this.dw2button.Add(addMe, docButton);

            if (addMe.Document != null)
            {
                QueueThumbnailUpdate(addMe);
                docButton.Dirty = addMe.Document.Dirty;
                addMe.Document.DirtyChanged += Document_DirtyChanged;
            }

            addMe.DocumentChanging += Workspace_DocumentChanging;
            addMe.DocumentChanged += Workspace_DocumentChanged;

            OnDocumentListChanged();
        }

        private void Workspace_DocumentChanging(object sender, EventArgs<Document> e)
        {
            if (e.Data != null)
            {
                e.Data.DirtyChanged -= Document_DirtyChanged;
            }
        }

        private void Workspace_DocumentChanged(object sender, EventArgs e)
        {
            DocumentWorkspace dw = (DocumentWorkspace)sender;
            ImageStrip.Item docButton = this.dw2button[dw];

            if (dw.Document != null)
            {
                docButton.Dirty = dw.Document.Dirty;
                dw.Document.DirtyChanged += Document_DirtyChanged;
            }
            else
            {
                docButton.Dirty = false;
            }
        }

        private void Document_DirtyChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < this.documents.Count; ++i)
            {
                if (object.ReferenceEquals(sender, this.documents[i].Document))
                {
                    ImageStrip.Item docButton = this.dw2button[this.documents[i]];
                    docButton.Dirty = ((Document)sender).Dirty;
                }
            }
        }

        private void Workspace_CompositionUpdated(object sender, EventArgs e)
        {
            DocumentWorkspace dw = (DocumentWorkspace)sender;
            QueueThumbnailUpdate(dw);
        }

        public void RemoveDocumentWorkspace(DocumentWorkspace removeMe)
        {
            removeMe.CompositionUpdated -= Workspace_CompositionUpdated;

            if (this.selectedDocument == removeMe)
            {
                this.selectedDocument = null;
            }

            removeMe.DocumentChanging -= Workspace_DocumentChanging;
            removeMe.DocumentChanged -= Workspace_DocumentChanged;

            if (removeMe.Document != null)
            {
                removeMe.Document.DirtyChanged -= Document_DirtyChanged;
            }

            this.documents.Remove(removeMe);
            this.thumbnailManager.RemoveFromQueue(removeMe);

            ImageStrip.Item docButton = this.dw2button[removeMe];
            this.RemoveItem(docButton);
            this.dw2button.Remove(removeMe);
            this.documentButtons.Remove(docButton);

            if (this.thumbs.ContainsKey(removeMe))
            {
                RenderArgs thumbRA = this.thumbs[removeMe];
                Surface surface = thumbRA.Surface;
                thumbRA.Dispose();
                this.thumbs.Remove(removeMe);
                surface.Dispose();
            }

            OnDocumentListChanged();
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);

            if (this.ensureSelectedIsVisible && 
                (!Focused && !LeftScrollButton.Focused && !RightScrollButton.Focused))
            {
                int index = this.documents.IndexOf(this.selectedDocument);
                EnsureItemFullyVisible(index);
            }
        }

        public void SelectDocumentWorkspace(DocumentWorkspace selectMe)
        {
            UI.SuspendControlPainting(this);

            this.selectedDocument = selectMe;

            if (this.thumbs.ContainsKey(selectMe))
            {
                RenderArgs thumb = this.thumbs[selectMe];
                Bitmap bitmap = thumb.Bitmap;
            }
            else
            {
                QueueThumbnailUpdate(selectMe);
            }

            foreach (ImageStrip.Item docItem in this.documentButtons)
            {
                if ((docItem.Tag as DocumentWorkspace) == selectMe)
                {
                    EnsureItemFullyVisible(docItem);
                    docItem.Checked = true;
                }
                else
                {
                    docItem.Checked = false;
                }
            }

            UI.ResumeControlPainting(this);
            Invalidate(true);
        }

        public void RefreshThumbnail(DocumentWorkspace dw)
        {
            if (this.documents.Contains(dw))
            {
                QueueThumbnailUpdate(dw);
            }
        }

        public void RefreshAllThumbnails()
        {
            foreach (DocumentWorkspace dw in this.documents)
            {
                QueueThumbnailUpdate(dw);
            }
        }

        private void OnThumbnailUpdated(DocumentWorkspace dw)
        {
            // We must double check that the DW is still around, because there's a chance
            // that the DW was been removed while the thumbnail was being rendered.
            if (this.dw2button.ContainsKey(dw))
            {
                ImageStrip.Item docButton = this.dw2button[dw];
                RenderArgs docRA = this.thumbs[dw];
                docButton.Image = docRA.Bitmap;
                docButton.Update();
            }
        }

        public void QueueThumbnailUpdate(DocumentWorkspace dw)
        {
            if (this.suspendThumbnailUpdates <= 0)
            {
                this.thumbnailManager.QueueThumbnailUpdate(dw, PreferredImageSize.Width - 2, OnThumbnailRendered);
            }
        }

        private void OnThumbnailRendered(object sender, EventArgs<Pair<IThumbnailProvider, Surface>> e)
        {
            RenderArgs ra = null;
            DocumentWorkspace dw = (DocumentWorkspace)e.Data.First;

            Size desiredSize = new Size(e.Data.Second.Width + 2, e.Data.Second.Height + 2);

            if (this.thumbs.ContainsKey(dw))
            {
                ra = this.thumbs[dw];

                if (ra.Size != desiredSize)
                {
                    ra.Dispose();
                    ra = null;

                    this.thumbs.Remove(dw);
                }
            }
            
            if (ra == null)
            {
                Surface surface = new Surface(desiredSize);
                ra = new RenderArgs(surface);
                this.thumbs.Add(dw, ra);
            }

            ra.Surface.Clear(ColorBgra.Black);
            ra.Surface.CopySurface(e.Data.Second, new Point(1, 1));
            e.Data.Second.Dispose();

            OnThumbnailUpdated(dw);
        }
    }
}
