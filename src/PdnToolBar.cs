/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.Menus;
using PaintDotNet.SystemLayer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;

namespace PaintDotNet
{
    internal class PdnToolBar 
        : Control,
          IPaintBackground
    {
        private const ToolStripGripStyle toolStripsGripStyle = ToolStripGripStyle.Hidden;
        private DateTime ignoreShowDocumentListUntil = DateTime.MinValue;

        private AppWorkspace appWorkspace;
        private PdnMainMenu mainMenu;
        private ToolStripPanel toolStripPanel;
        private CommonActionsStrip commonActionsStrip;
        private ViewConfigStrip viewConfigStrip;
        private ToolChooserStrip toolChooserStrip;
        private ToolConfigStrip toolConfigStrip;
        private OurDocumentStrip documentStrip;
        private ArrowButton documentListButton;
        private ImageListMenu imageListMenu;
        private OurToolStripRenderer otsr = new OurToolStripRenderer();

        private class OurToolStripRenderer :
            ToolStripProfessionalRenderer
        {
            public OurToolStripRenderer()
            {
                RoundedEdges = false;
            }

            private void PaintBackground(Graphics g, Control control, Rectangle clipRect)
            {
                Control parent = control;
                IPaintBackground asIpb = null;

                while (true)
                {
                    parent = parent.Parent;

                    if (parent == null)
                    {
                        break;
                    }

                    asIpb = parent as IPaintBackground;

                    if (asIpb != null)
                    {
                        break;
                    }
                }

                if (asIpb != null)
                {
                    Rectangle screenRect = control.RectangleToScreen(clipRect);
                    Rectangle parentRect = parent.RectangleToClient(screenRect);

                    int dx = parentRect.Left - clipRect.Left;
                    int dy = parentRect.Top - clipRect.Top;

                    g.TranslateTransform(-dx, -dy, MatrixOrder.Append);
                    asIpb.PaintBackground(g, parentRect);
                    g.TranslateTransform(dx, dy, MatrixOrder.Append);
                }
            }

            protected override void OnRenderToolStripPanelBackground(ToolStripPanelRenderEventArgs e)
            {
                PaintBackground(e.Graphics, e.ToolStripPanel, new Rectangle(new Point(0, 0), e.ToolStripPanel.Size));
                e.Handled = true;
            }

            protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
            {
                if (e.ToolStrip.GetType() != typeof(ToolStrip) &&
                    e.ToolStrip.GetType() != typeof(ToolStripEx) &&
                    e.ToolStrip.GetType() != typeof(PdnMainMenu))
                {
                    base.OnRenderToolStripBackground(e);
                }
                else
                {
                    PaintBackground(e.Graphics, e.ToolStrip, e.AffectedBounds);
                }
            }
        }

        private class OurDocumentStrip :
            DocumentStrip,
            IPaintBackground
        {
            protected override void DrawItemBackground(Graphics g, Item item, Rectangle itemRect)
            {
                PaintBackground(g, itemRect);
            }

            public void PaintBackground(Graphics g, Rectangle clipRect)
            {
                IPaintBackground asIpb = this.Parent as IPaintBackground;

                if (asIpb != null)
                {
                    Rectangle newClipRect = new Rectangle(
                        clipRect.Left + Left, clipRect.Top + Top, 
                        clipRect.Width, clipRect.Height);

                    g.TranslateTransform(-Left, -Top, MatrixOrder.Append);
                    asIpb.PaintBackground(g, newClipRect);
                    g.TranslateTransform(Left, Top, MatrixOrder.Append);
                }
            }
        }

        public void PaintBackground(Graphics g, Rectangle clipRect)
        {
            if (clipRect.Width > 0 && clipRect.Height > 0)
            {
                Color backColor = ProfessionalColors.MenuStripGradientEnd;

                using (SolidBrush brush = new SolidBrush(backColor))
                {
                    g.FillRectangle(brush, clipRect);
                }
            }
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
                this.mainMenu.AppWorkspace = value;
            }
        }

        public PdnMainMenu MainMenu
        {
            get
            {
                return this.mainMenu;
            }
        }

        public ToolStripPanel ToolStripContainer
        {
            get
            {
                return this.toolStripPanel;
            }
        }

        public CommonActionsStrip CommonActionsStrip
        {
            get
            {
                return this.commonActionsStrip;
            }
        }

        public ViewConfigStrip ViewConfigStrip
        {
            get
            {
                return this.viewConfigStrip;
            }
        }

        public ToolChooserStrip ToolChooserStrip
        {
            get
            {
                return this.toolChooserStrip;
            }
        }

        public ToolConfigStrip ToolConfigStrip
        {
            get
            {
                return this.toolConfigStrip;
            }
        }

        public DocumentStrip DocumentStrip
        {
            get
            {
                return this.documentStrip;
            }
        }

        public PdnToolBar()
        {
            SuspendLayout();
            InitializeComponent();

            this.toolChooserStrip.SetTools(DocumentWorkspace.ToolInfos);

            this.otsr = new OurToolStripRenderer();
            this.commonActionsStrip.Renderer = otsr;
            this.viewConfigStrip.Renderer = otsr;
            this.toolStripPanel.Renderer = otsr;
            this.toolChooserStrip.Renderer = otsr;
            this.toolConfigStrip.Renderer = otsr;
            this.mainMenu.Renderer = otsr;

            ResumeLayout(true);
        }

        private bool computedMaxRowHeight = false;
        private int maxRowHeight = -1;

        protected override void OnLayout(LayoutEventArgs e)
        {
            bool plentyWidthBefore =
                (this.mainMenu.Width >= this.mainMenu.PreferredSize.Width) &&
                (this.commonActionsStrip.Width >= this.commonActionsStrip.PreferredSize.Width) &&
                (this.viewConfigStrip.Width >= this.viewConfigStrip.PreferredSize.Width) &&
                (this.toolChooserStrip.Width >= this.toolChooserStrip.PreferredSize.Width) &&
                (this.toolConfigStrip.Width >= this.toolConfigStrip.PreferredSize.Width);

            if (!plentyWidthBefore)
            {
                UI.SuspendControlPainting(this);
            }
            else
            {
                // if we don't do this then we get some terrible flickering of the right scroll arrow
                UI.SuspendControlPainting(this.documentStrip);
            }

            this.mainMenu.Location = new Point(0, 0);
            this.mainMenu.Height = this.mainMenu.PreferredSize.Height;
            this.toolStripPanel.Location = new Point(0, this.mainMenu.Bottom);

            this.toolStripPanel.RowMargin = new Padding(0);
            this.mainMenu.Padding = new Padding(0, this.mainMenu.Padding.Top, 0, this.mainMenu.Padding.Bottom);

            this.commonActionsStrip.Width = this.commonActionsStrip.PreferredSize.Width;
            this.viewConfigStrip.Width = this.viewConfigStrip.PreferredSize.Width;
            this.toolChooserStrip.Width = this.toolChooserStrip.PreferredSize.Width;
            this.toolConfigStrip.Width = this.toolConfigStrip.PreferredSize.Width;

            if (!this.computedMaxRowHeight)
            {
                ToolBarConfigItems oldTbci = this.toolConfigStrip.ToolBarConfigItems;
                this.toolConfigStrip.ToolBarConfigItems = ToolBarConfigItems.All;
                this.toolConfigStrip.PerformLayout();

                this.maxRowHeight =
                    Math.Max(this.commonActionsStrip.PreferredSize.Height,
                    Math.Max(this.viewConfigStrip.PreferredSize.Height,
                    Math.Max(this.toolChooserStrip.PreferredSize.Height, this.toolConfigStrip.PreferredSize.Height)));

                this.toolConfigStrip.ToolBarConfigItems = oldTbci;
                this.toolConfigStrip.PerformLayout();

                this.computedMaxRowHeight = true;
            }

            this.commonActionsStrip.Height = this.maxRowHeight;
            this.viewConfigStrip.Height = this.maxRowHeight;
            this.toolChooserStrip.Height = this.maxRowHeight;
            this.toolConfigStrip.Height = this.maxRowHeight;

            this.commonActionsStrip.Location = new Point(0, 0);
            this.viewConfigStrip.Location = new Point(this.commonActionsStrip.Right, this.commonActionsStrip.Top);
            this.toolChooserStrip.Location = new Point(0, this.viewConfigStrip.Bottom);
            this.toolConfigStrip.Location = new Point(this.toolChooserStrip.Right, this.toolChooserStrip.Top);

            this.toolStripPanel.Height =
                Math.Max(this.commonActionsStrip.Bottom,
                Math.Max(this.viewConfigStrip.Bottom,
                Math.Max(this.toolChooserStrip.Bottom,
                         this.toolConfigStrip.Visible ? this.toolConfigStrip.Bottom : this.toolChooserStrip.Bottom)));

            // Compute how wide the toolStripContainer would like to be
            int widthRow1 =
                this.commonActionsStrip.Left + this.commonActionsStrip.PreferredSize.Width + this.commonActionsStrip.Margin.Horizontal +
                this.viewConfigStrip.PreferredSize.Width + this.viewConfigStrip.Margin.Horizontal;

            int widthRow2 =
                this.toolChooserStrip.Left + this.toolChooserStrip.PreferredSize.Width + this.toolChooserStrip.Margin.Horizontal +
                this.toolConfigStrip.PreferredSize.Width + this.toolConfigStrip.Margin.Horizontal;

            int preferredMinTscWidth = Math.Max(widthRow1, widthRow2);

            // Throw in the documentListButton if necessary
            bool showDlb = this.documentStrip.DocumentCount > 0;

            this.documentListButton.Visible = showDlb;
            this.documentListButton.Enabled = showDlb;

            if (showDlb)
            {
                int documentListButtonWidth = UI.ScaleWidth(15);
                this.documentListButton.Width = documentListButtonWidth;
            }
            else
            {
                this.documentListButton.Width = 0;
            }

            // Figure out the DocumentStrip's size -- we actually make two passes at setting its Width
            // so that we can toss in the documentListButton if necessary
            if (this.documentStrip.DocumentCount == 0)
            {
                this.documentStrip.Width = 0;
            }
            else
            {
                this.documentStrip.Width = Math.Max(
                    this.documentStrip.PreferredMinClientWidth,
                    Math.Min(this.documentStrip.PreferredSize.Width, 
                             ClientSize.Width - preferredMinTscWidth - this.documentListButton.Width));
            }

            this.documentStrip.Location = new Point(ClientSize.Width - this.documentStrip.Width, 0);
            this.documentListButton.Location = new Point(this.documentStrip.Left - this.documentListButton.Width, 0);

            this.imageListMenu.Location = new Point(this.documentListButton.Left, this.documentListButton.Bottom - 1);
            this.imageListMenu.Width = this.documentListButton.Width;
            this.imageListMenu.Height = 0;

            this.documentListButton.Visible = showDlb;
            this.documentListButton.Enabled = showDlb;

            // Finish setting up widths and heights
            int oldDsHeight = this.documentStrip.Height;
            this.documentStrip.Height = this.toolStripPanel.Bottom;
            this.documentListButton.Height = this.documentStrip.Height;

            int tsWidth = ClientSize.Width - (this.documentStrip.Width + this.documentListButton.Width);
            this.mainMenu.Width = tsWidth;
            this.toolStripPanel.Width = tsWidth;

            this.Height = this.toolStripPanel.Bottom;

            // Now get stuff to paint right
            this.documentStrip.PerformLayout();

            if (!plentyWidthBefore)
            {
                UI.ResumeControlPainting(this);
                Invalidate(true);
            }
            else
            {
                UI.ResumeControlPainting(this.documentStrip);
                this.documentStrip.Invalidate(true);
            }

            if (this.documentStrip.Width == 0)
            {
                this.mainMenu.Invalidate();
            }

            if (oldDsHeight != this.documentStrip.Height)
            {
                this.documentStrip.RefreshAllThumbnails();
            }

            base.OnLayout(e);
        }

        protected override void OnResize(EventArgs e)
        {
            PerformLayout();
            base.OnResize(e);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.mainMenu = new PdnMainMenu();
            this.toolStripPanel = new ToolStripPanel();
            this.commonActionsStrip = new CommonActionsStrip();
            this.viewConfigStrip = new ViewConfigStrip();
            this.toolChooserStrip = new ToolChooserStrip();
            this.toolConfigStrip = new ToolConfigStrip();
            this.documentStrip = new OurDocumentStrip();
            this.documentListButton = new ArrowButton();
            this.imageListMenu = new ImageListMenu();
            this.toolStripPanel.BeginInit();
            this.toolStripPanel.SuspendLayout();
            // 
            // mainMenu
            // 
            this.mainMenu.Name = "mainMenu";
            //
            // toolStripContainer
            //
            this.toolStripPanel.AutoSize = true;
            this.toolStripPanel.Name = "toolStripPanel";
            this.toolStripPanel.TabIndex = 0;
            this.toolStripPanel.TabStop = false;
            this.toolStripPanel.Join(this.viewConfigStrip);
            this.toolStripPanel.Join(this.commonActionsStrip);
            this.toolStripPanel.Join(this.toolConfigStrip);
            this.toolStripPanel.Join(this.toolChooserStrip);
            //
            // commonActionsStrip
            //
            this.commonActionsStrip.Name = "commonActionsStrip";
            this.commonActionsStrip.AutoSize = false;
            this.commonActionsStrip.TabIndex = 0;
            this.commonActionsStrip.Dock = DockStyle.None;
            this.commonActionsStrip.GripStyle = toolStripsGripStyle;
            //
            // viewConfigStrip
            //
            this.viewConfigStrip.Name = "viewConfigStrip";
            this.viewConfigStrip.AutoSize = false;
            this.viewConfigStrip.ZoomBasis = PaintDotNet.ZoomBasis.FitToWindow;
            this.viewConfigStrip.TabStop = false;
            this.viewConfigStrip.DrawGrid = false;
            this.viewConfigStrip.TabIndex = 1;
            this.viewConfigStrip.Dock = DockStyle.None;
            this.viewConfigStrip.GripStyle = toolStripsGripStyle;
            //
            // toolChooserStrip
            //
            this.toolChooserStrip.Name = "toolChooserStrip";
            this.toolChooserStrip.AutoSize = false;
            this.toolChooserStrip.TabIndex = 2;
            this.toolChooserStrip.Dock = DockStyle.None;
            this.toolChooserStrip.GripStyle = toolStripsGripStyle;
            this.toolChooserStrip.ChooseDefaultsClicked += new EventHandler(ToolChooserStrip_ChooseDefaultsClicked);
            //
            // toolConfigStrip
            //
            this.toolConfigStrip.Name = "drawConfigStrip";
            this.toolConfigStrip.AutoSize = false;
            this.toolConfigStrip.ShapeDrawType = PaintDotNet.ShapeDrawType.Outline;
            this.toolConfigStrip.TabIndex = 3;
            this.toolConfigStrip.Dock = DockStyle.None;
            this.toolConfigStrip.GripStyle = toolStripsGripStyle;
            this.toolConfigStrip.Layout +=
                delegate(object sender, LayoutEventArgs e)
                {
                    PerformLayout();
                };
            this.toolConfigStrip.SelectionDrawModeInfoChanged +=
                delegate(object sender, EventArgs e)
                {
                    BeginInvoke(new Procedure(PerformLayout));
                };
            //
            // documentStrip
            //
            this.documentStrip.AutoSize = false;
            this.documentStrip.Name = "documentStrip";
            this.documentStrip.TabIndex = 5;
            this.documentStrip.ShowScrollButtons = true;
            this.documentStrip.DocumentListChanged += new EventHandler(DocumentStrip_DocumentListChanged);
            this.documentStrip.DocumentClicked += DocumentStrip_DocumentClicked;
            this.documentStrip.ManagedFocus = true;
            //
            // documentListButton
            //
            this.documentListButton.Name = "documentListButton";
            this.documentListButton.ArrowDirection = ArrowDirection.Down;
            this.documentListButton.ReverseArrowColors = true;
            this.documentListButton.Click += new EventHandler(DocumentListButton_Click);
            //
            // imageListMenu
            //
            this.imageListMenu.Name = "imageListMenu";
            this.imageListMenu.Closed += new EventHandler(ImageListMenu_Closed);
            this.imageListMenu.ItemClicked += ImageListMenu_ItemClicked;
            //
            // PdnToolBar
            //
            this.Controls.Add(this.documentListButton);
            this.Controls.Add(this.documentStrip);
            this.Controls.Add(this.toolStripPanel);
            this.Controls.Add(this.mainMenu);
            this.Controls.Add(this.imageListMenu);
            this.toolStripPanel.ResumeLayout(false);
            this.toolStripPanel.EndInit();
            this.ResumeLayout(false);
        }

        private void ToolChooserStrip_ChooseDefaultsClicked(object sender, EventArgs e)
        {
            PdnBaseForm.UpdateAllForms();

            WaitCursorChanger wcc = new WaitCursorChanger(this);

            using (ChooseToolDefaultsDialog dialog = new ChooseToolDefaultsDialog())
            {
                EventHandler shownDelegate = null;

                shownDelegate =
                    delegate(object sender2, EventArgs e2)
                    {
                        wcc.Dispose();
                        wcc = null;
                        dialog.Shown -= shownDelegate;
                    };

                dialog.Shown += shownDelegate;
                dialog.SetToolBarSettings(this.appWorkspace.GlobalToolTypeChoice, this.appWorkspace.AppEnvironment);

                AppEnvironment defaultAppEnv = AppEnvironment.GetDefaultAppEnvironment();

                try
                {
                    dialog.LoadUIFromAppEnvironment(defaultAppEnv);
                }

                catch (Exception)
                {
                    defaultAppEnv = new AppEnvironment();
                    defaultAppEnv.SetToDefaults();
                    dialog.LoadUIFromAppEnvironment(defaultAppEnv);
                }

                dialog.ToolType = this.appWorkspace.DefaultToolType;

                DialogResult dr = dialog.ShowDialog(this);

                if (dr != DialogResult.Cancel)
                {
                    AppEnvironment newDefaultAppEnv = dialog.CreateAppEnvironmentFromUI();
                    newDefaultAppEnv.SaveAsDefaultAppEnvironment();
                    this.appWorkspace.AppEnvironment.LoadFrom(newDefaultAppEnv);
                    this.appWorkspace.DefaultToolType = dialog.ToolType;
                    this.appWorkspace.GlobalToolTypeChoice = dialog.ToolType;
                }
            }

            if (wcc != null)
            {
                wcc.Dispose();
                wcc = null;
            }
        }

        private void DocumentListButton_Click(object sender, EventArgs e)
        {
            if (this.imageListMenu.IsImageListVisible)
            {
                HideDocumentList();
            }
            else
            {
                ShowDocumentList();
            }
        }

        public void HideDocumentList()
        {
            this.imageListMenu.HideImageList();
        }

        private void ImageListMenu_Closed(object sender, EventArgs e)
        {
            this.documentListButton.ForcedPushedAppearance = false;

            // We set this up because otherwise if the user clicks on the documentListButton,
            // then first the documentListMenu closes, and then the documentClickButton's Click
            // event fires. The behavior we want is to hide the menu when this Click occurs,
            // but since the menu is already hidden we have no way of knowing that we should
            // not show the menu.
            this.ignoreShowDocumentListUntil = DateTime.Now + new TimeSpan(0, 0, 0, 0, 250);
        }

        private void ImageListMenu_ItemClicked(object sender, EventArgs<ImageListMenu.Item> e)
        {
            DocumentWorkspace dw = (DocumentWorkspace)e.Data.Tag;

            if (!dw.IsDisposed)
            {
                this.documentStrip.SelectedDocument = dw;
            }
        }

        public void ShowDocumentList()
        {
            if (this.documentStrip.DocumentCount < 1)
            {
                return;
            }

            if (DateTime.Now < this.ignoreShowDocumentListUntil)
            {
                return;
            }

            if (this.imageListMenu.IsImageListVisible)
            {
                return;
            }

            DocumentWorkspace[] documents = this.documentStrip.DocumentList;
            Image[] thumbnails = this.documentStrip.DocumentThumbnails;

            ImageListMenu.Item[] items = new ImageListMenu.Item[this.documentStrip.DocumentCount];

            for (int i = 0; i < items.Length; ++i)
            {
                bool selected = (documents[i] == this.documentStrip.SelectedDocument);

                items[i] = new ImageListMenu.Item(
                    thumbnails[i],
                    documents[i].GetFriendlyName(),
                    selected);

                items[i].Tag = documents[i];
            }

            Cursor.Current = Cursors.Default;

            this.documentListButton.ForcedPushedAppearance = true;
            this.imageListMenu.ShowImageList(items);
        }

        private void DocumentStrip_DocumentClicked(object sender, EventArgs<Pair<DocumentWorkspace, DocumentClickAction>> e)
        {
            if (e.Data.Second == DocumentClickAction.Select)
            {
                PerformLayout();
            }
        }

        private void DocumentStrip_DocumentListChanged(object sender, EventArgs e)
        {
            PerformLayout();

            if (this.documentStrip.DocumentCount == 0)
            {
                this.viewConfigStrip.Enabled = false;
                this.toolChooserStrip.Enabled = false;
                this.toolConfigStrip.Enabled = false;
            }
            else
            {
                this.viewConfigStrip.Enabled = true;
                this.toolChooserStrip.Enabled = true;
                this.toolConfigStrip.Enabled = true;
            }
        }
    }
}
