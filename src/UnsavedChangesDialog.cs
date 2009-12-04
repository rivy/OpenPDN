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
using System.Text;
using System.Windows.Forms;

namespace PaintDotNet
{
    internal class UnsavedChangesDialog 
        : PdnBaseForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        private DocumentStrip documentStrip;
        private HeaderLabel documentListHeader;
        private System.Windows.Forms.HScrollBar hScrollBar;
        private CommandButton saveButton;
        private CommandButton dontSaveButton;
        private CommandButton cancelButton;
        private System.Windows.Forms.Label infoLabel;

        private DocumentWorkspace[] documents;

        public DocumentWorkspace[] Documents
        {
            get
            {
                return (DocumentWorkspace[])this.documents.Clone();
            }

            set
            {
                this.documents = (DocumentWorkspace[])value.Clone();
                this.documentStrip.ClearItems();

                foreach (DocumentWorkspace dw in this.documents)
                {
                    this.documentStrip.AddDocumentWorkspace(dw);
                }

                this.hScrollBar.Maximum = this.documentStrip.ViewRectangle.Width;
                this.hScrollBar.LargeChange = this.documentStrip.ClientSize.Width;

                if (this.documentStrip.ClientRectangle.Width > this.documentStrip.ViewRectangle.Width)
                {
                    this.hScrollBar.Enabled = false;
                }
                else
                {
                    this.hScrollBar.Enabled = true;
                }

                ImageStrip.Item[] items = this.documentStrip.Items;
                foreach (ImageStrip.Item item in items)
                {
                    item.Checked = false;
                }
            }
        }

        public DocumentWorkspace SelectedDocument
        {
            get
            {
                return this.documentStrip.SelectedDocument;
            }

            set
            {
                this.documentStrip.SelectDocumentWorkspace(value);
            }
        }

        public UnsavedChangesDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">
        /// true if managed resources should be disposed; otherwise, false.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        public override void LoadResources()
        {
            this.Text = PdnResources.GetString("UnsavedChangesDialog.Text");
            this.Icon = Utility.ImageToIcon(PdnResources.GetImageResource("Icons.WarningIcon.png").Reference, false);

            this.infoLabel.Text = PdnResources.GetString("UnsavedChangesDialog.InfoLabel.Text");
            this.documentListHeader.Text = PdnResources.GetString("UnsavedChangesDialog.DocumentListHeader.Text");
            
            this.saveButton.ActionText = PdnResources.GetString("UnsavedChangesDialog.SaveButton.ActionText");
            this.saveButton.ExplanationText = PdnResources.GetString("UnsavedChangesDialog.SaveButton.ExplanationText");
            this.saveButton.ActionImage = PdnResources.GetImageResource("Icons.UnsavedChangesDialog.SaveButton.png").Reference;

            this.dontSaveButton.ActionText = PdnResources.GetString("UnsavedChangesDialog.DontSaveButton.ActionText");
            this.dontSaveButton.ExplanationText = PdnResources.GetString("UnsavedChangesDialog.DontSaveButton.ExplanationText");
            this.dontSaveButton.ActionImage = PdnResources.GetImageResource("Icons.MenuFileCloseIcon.png").Reference;

            this.cancelButton.ActionText = PdnResources.GetString("UnsavedChangesDialog.CancelButton.ActionText");
            this.cancelButton.ExplanationText = PdnResources.GetString("UnsavedChangesDialog.CancelButton.ExplanationText");
            this.cancelButton.ActionImage = PdnResources.GetImageResource("Icons.CancelIcon.png").Reference;

            base.LoadResources();        
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            int leftMargin = UI.ScaleWidth(8);
            int rightMargin = UI.ScaleWidth(8);
            int topMargin = UI.ScaleHeight(8);
            int bottomMargin = UI.ScaleHeight(8);
            int afterInfoLabelVMargin = UI.ScaleHeight(8);
            int afterDocumentListHeaderVMargin = UI.ScaleHeight(8);
            int afterDocumentListVMargin = UI.ScaleHeight(8);
            int commandButtonVMargin = UI.ScaleHeight(0);
            int insetWidth = ClientSize.Width - leftMargin - rightMargin;

            int y = topMargin;

            this.infoLabel.Location = new Point(leftMargin, y);
            this.infoLabel.Width = insetWidth;
            this.infoLabel.Height = this.infoLabel.GetPreferredSize(new Size(this.infoLabel.Width, 0)).Height;
            y += this.infoLabel.Height + afterInfoLabelVMargin;

            this.documentListHeader.Location = new Point(leftMargin, y);
            this.documentListHeader.Width = insetWidth;
            y += this.documentListHeader.Height + afterDocumentListHeaderVMargin;

            this.documentStrip.Location = new Point(leftMargin, y);
            this.documentStrip.Size = new Size(insetWidth, UI.ScaleHeight(72));
            this.hScrollBar.Location = new Point(leftMargin, this.documentStrip.Bottom);
            this.hScrollBar.Width = insetWidth;
            y += this.documentStrip.Height + hScrollBar.Height + afterDocumentListVMargin;

            this.saveButton.Location = new Point(leftMargin, y);
            this.saveButton.Width = insetWidth;
            this.saveButton.PerformLayout();
            y += this.saveButton.Height + commandButtonVMargin;

            this.dontSaveButton.Location = new Point(leftMargin, y);
            this.dontSaveButton.Width = insetWidth;
            this.dontSaveButton.PerformLayout();
            y += this.dontSaveButton.Height + commandButtonVMargin;

            this.cancelButton.Location = new Point(leftMargin, y);
            this.cancelButton.Width = insetWidth;
            this.cancelButton.PerformLayout();
            y += this.cancelButton.Height + bottomMargin;

            this.ClientSize = new Size(ClientSize.Width, y);
            base.OnLayout(levent);
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.documentStrip = new PaintDotNet.DocumentStrip();
            this.documentListHeader = new PaintDotNet.HeaderLabel();
            this.hScrollBar = new System.Windows.Forms.HScrollBar();
            this.saveButton = new PaintDotNet.CommandButton();
            this.dontSaveButton = new PaintDotNet.CommandButton();
            this.cancelButton = new PaintDotNet.CommandButton();
            this.infoLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // documentStrip
            // 
            this.documentStrip.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.documentStrip.DocumentClicked += DocumentList_DocumentClicked;
            this.documentStrip.DrawDirtyOverlay = false;
            this.documentStrip.EnsureSelectedIsVisible = false;
            this.documentStrip.ManagedFocus = true;
            this.documentStrip.Name = "documentList";
            this.documentStrip.ScrollOffset = 0;
            this.documentStrip.ScrollOffsetChanged += new EventHandler(DocumentList_ScrollOffsetChanged);
            this.documentStrip.ShowCloseButtons = false;
            this.documentStrip.ShowScrollButtons = false;
            this.documentStrip.TabIndex = 0;
            this.documentStrip.ThumbnailUpdateLatency = 10;
            // 
            // documentListHeader
            // 
            this.documentListHeader.Name = "documentListHeader";
            this.documentListHeader.RightMargin = 0;
            this.documentListHeader.TabIndex = 1;
            this.documentListHeader.TabStop = false;
            // 
            // hScrollBar
            // 
            this.hScrollBar.Name = "hScrollBar";
            this.hScrollBar.TabIndex = 2;
            this.hScrollBar.ValueChanged += new System.EventHandler(this.HScrollBar_ValueChanged);
            // 
            // saveButton
            // 
            this.saveButton.ActionImage = null;
            this.saveButton.AutoSize = true;
            this.saveButton.Name = "saveButton";
            this.saveButton.TabIndex = 4;
            this.saveButton.Click += new System.EventHandler(this.SaveButton_Click);
            // 
            // dontSaveButton
            // 
            this.dontSaveButton.ActionImage = null;
            this.dontSaveButton.AutoSize = true;
            this.dontSaveButton.Name = "dontSaveButton";
            this.dontSaveButton.TabIndex = 5;
            this.dontSaveButton.Click += new System.EventHandler(this.DontSaveButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.ActionImage = null;
            this.cancelButton.AutoSize = true;
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.TabIndex = 6;
            this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // infoLabel
            // 
            this.infoLabel.Name = "infoLabel";
            this.infoLabel.TabIndex = 7;
            // 
            // UnsavedChangesDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(450, 100);
            this.Controls.Add(this.infoLabel);
            this.Controls.Add(this.documentListHeader);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.hScrollBar);
            this.Controls.Add(this.dontSaveButton);
            this.Controls.Add(this.documentStrip);
            this.Controls.Add(this.saveButton);
            this.AcceptButton = this.saveButton;
            this.CancelButton = this.cancelButton;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.Location = new System.Drawing.Point(0, 0);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UnsavedChangesDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Controls.SetChildIndex(this.saveButton, 0);
            this.Controls.SetChildIndex(this.documentStrip, 0);
            this.Controls.SetChildIndex(this.dontSaveButton, 0);
            this.Controls.SetChildIndex(this.hScrollBar, 0);
            this.Controls.SetChildIndex(this.cancelButton, 0);
            this.Controls.SetChildIndex(this.documentListHeader, 0);
            this.Controls.SetChildIndex(this.infoLabel, 0);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        public event EventHandler<EventArgs<DocumentWorkspace>> DocumentClicked;
        protected virtual void OnDocumentClicked(DocumentWorkspace dw)
        {
            if (DocumentClicked != null)
            {
                DocumentClicked(this, new EventArgs<DocumentWorkspace>(dw));
            }
        }

        private void DocumentList_DocumentClicked(
            object sender, 
            EventArgs<Pair<DocumentWorkspace, DocumentClickAction>> e)
        {
            this.documentStrip.Update();
            OnDocumentClicked(e.Data.First);
        }

        private void HScrollBar_ValueChanged(object sender, EventArgs e)
        {
            this.documentStrip.ScrollOffset = this.hScrollBar.Value;
        }

        private void DocumentList_ScrollOffsetChanged(object sender, EventArgs e)
        {
            this.hScrollBar.Value = this.documentStrip.ScrollOffset;
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void DontSaveButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.No;
            Close();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Yes;
            Close();
        }
    }
}