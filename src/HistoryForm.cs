/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace PaintDotNet
{
    internal class HistoryForm
        : FloatingToolForm
    {
        private PaintDotNet.HistoryControl historyControl;
        private System.Windows.Forms.ImageList imageList;
        private PaintDotNet.SystemLayer.ToolStripEx toolStrip;
        private ToolStripButton rewindButton;
        private ToolStripButton undoButton;
        private ToolStripButton redoButton;
        private ToolStripButton fastForwardButton;
        private System.ComponentModel.IContainer components;

        public HistoryControl HistoryControl
        {
            get
            {
                return historyControl;
            }
        }

        public HistoryForm()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            this.imageList.TransparentColor = Utility.TransparentKey;
            this.toolStrip.ImageList = this.imageList;

            int rewindIndex = imageList.Images.Add(PdnResources.GetImageResource("Icons.HistoryRewindIcon.png").Reference, imageList.TransparentColor);
            int undoIndex = imageList.Images.Add(PdnResources.GetImageResource("Icons.MenuEditUndoIcon.png").Reference, imageList.TransparentColor);
            int redoIndex = imageList.Images.Add(PdnResources.GetImageResource("Icons.MenuEditRedoIcon.png").Reference, imageList.TransparentColor);
            int fastForwardIndex = imageList.Images.Add(PdnResources.GetImageResource("Icons.HistoryFastForwardIcon.png").Reference, imageList.TransparentColor);

            rewindButton.ImageIndex = rewindIndex;
            undoButton.ImageIndex = undoIndex;
            redoButton.ImageIndex = redoIndex;
            fastForwardButton.ImageIndex = fastForwardIndex;

            this.Text = PdnResources.GetString("HistoryForm.Text");

            this.rewindButton.ToolTipText = PdnResources.GetString("HistoryForm.RewindButton.ToolTipText");
            this.undoButton.ToolTipText = PdnResources.GetString("HistoryForm.UndoButton.ToolTipText");
            this.redoButton.ToolTipText = PdnResources.GetString("HistoryForm.RedoButton.ToolTipText");
            this.fastForwardButton.ToolTipText = PdnResources.GetString("HistoryForm.FastForwardButton.ToolTipText");

            this.MinimumSize = this.Size;
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout (levent);

            // We have to test for null in case Layout is raised before our 
            // InitializeComponent is called (or is finished)
            if (historyControl != null)
            {
                historyControl.Size = new Size(ClientRectangle.Width, ClientRectangle.Height - (toolStrip.Height + 
                    (ClientRectangle.Height - toolStrip.Bottom)));
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
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.historyControl = new PaintDotNet.HistoryControl();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.toolStrip = new PaintDotNet.SystemLayer.ToolStripEx();
            this.rewindButton = new System.Windows.Forms.ToolStripButton();
            this.undoButton = new System.Windows.Forms.ToolStripButton();
            this.redoButton = new System.Windows.Forms.ToolStripButton();
            this.fastForwardButton = new System.Windows.Forms.ToolStripButton();
            this.toolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // historyControl
            // 
            this.historyControl.Dock = System.Windows.Forms.DockStyle.Top;
            this.historyControl.HistoryStack = null;
            this.historyControl.Location = new System.Drawing.Point(0, 0);
            this.historyControl.Name = "historyControl";
            this.historyControl.Size = new System.Drawing.Size(160, 152);
            this.historyControl.TabIndex = 0;
            this.historyControl.HistoryChanged += new System.EventHandler(this.HistoryControl_HistoryChanged);
            this.historyControl.RelinquishFocus += new EventHandler(HistoryControl_RelinquishFocus);
            this.historyControl.ManagedFocus = true;
            // 
            // imageList
            // 
            this.imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.imageList.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // toolStrip
            // 
            this.toolStrip.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.rewindButton,
            this.undoButton,
            this.redoButton,
            this.fastForwardButton});
            this.toolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.toolStrip.Location = new System.Drawing.Point(0, 139);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new System.Drawing.Size(160, 19);
            this.toolStrip.TabIndex = 2;
            this.toolStrip.Text = "toolStrip1";
            this.toolStrip.RelinquishFocus += new EventHandler(ToolStrip_RelinquishFocus);
            // 
            // rewindButton
            // 
            this.rewindButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.rewindButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.rewindButton.Name = "rewindButton";
            this.rewindButton.Size = new System.Drawing.Size(23, 4);
            this.rewindButton.Click += new System.EventHandler(this.OnToolStripButtonClick);
            // 
            // undoButton
            // 
            this.undoButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.undoButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.undoButton.Name = "undoButton";
            this.undoButton.Size = new System.Drawing.Size(23, 4);
            this.undoButton.Click += new System.EventHandler(this.OnToolStripButtonClick);
            // 
            // redoButton
            // 
            this.redoButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.redoButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.redoButton.Name = "redoButton";
            this.redoButton.Size = new System.Drawing.Size(23, 4);
            this.redoButton.Click += new System.EventHandler(this.OnToolStripButtonClick);
            // 
            // fastForwardButton
            // 
            this.fastForwardButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.fastForwardButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.fastForwardButton.Name = "fastForwardButton";
            this.fastForwardButton.Size = new System.Drawing.Size(23, 4);
            this.fastForwardButton.Click += new System.EventHandler(this.OnToolStripButtonClick);
            // 
            // HistoryForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(165, 158);
            this.Controls.Add(this.toolStrip);
            this.Controls.Add(this.historyControl);
            this.Name = "HistoryForm";
            this.Enter += new System.EventHandler(this.HistoryForm_Enter);
            this.Controls.SetChildIndex(this.historyControl, 0);
            this.Controls.SetChildIndex(this.toolStrip, 0);
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private void HistoryControl_RelinquishFocus(object sender, EventArgs e)
        {
            OnRelinquishFocus();
        }

        private void ToolStrip_RelinquishFocus(object sender, EventArgs e)
        {
            OnRelinquishFocus();
        }

        public event EventHandler UndoButtonClicked;
        protected virtual void OnUndoButtonClicked()
        {
            if (UndoButtonClicked != null)
            {
                UndoButtonClicked(this, EventArgs.Empty);
            }
        }

        public void PerformUndoClick()
        {
            OnUndoButtonClicked();
        }

        public event EventHandler RedoButtonClicked;
        protected virtual void OnRedoButtonClicked()
        {
            if (RedoButtonClicked != null)
            {
                RedoButtonClicked(this, EventArgs.Empty);
            }
        }

        public void PerformRedoClick()
        {
            OnRedoButtonClicked();
        }

        public event EventHandler RewindButtonClicked;
        protected virtual void OnRewindButtonClicked()
        {
            if (RewindButtonClicked != null)
            {
                RewindButtonClicked(this, EventArgs.Empty);
            }
        }

        public void PerformRewindClick()
        {
            OnRewindButtonClicked();
        }

        public event EventHandler FastForwardButtonClicked;
        protected virtual void OnFastForwardButtonClicked()
        {
            if (FastForwardButtonClicked != null)
            {
                FastForwardButtonClicked(this, EventArgs.Empty);
            }
        }

        public void PerformFastForwardClick()
        {
            OnFastForwardButtonClicked();
        }

        private void HistoryForm_Enter(object sender, System.EventArgs e)
        {
            PerformLayout();
        }

        private void UpdateHistoryButtons()
        {
            if (historyControl.HistoryStack == null)
            {
                rewindButton.Enabled = false;
                undoButton.Enabled = false;
                fastForwardButton.Enabled = false;
                redoButton.Enabled = false;
            }
            else
            {
                // Find reasons to disable the rewind and undo buttons
                if (historyControl.HistoryStack.UndoStack.Count <= 1)
                {
                    rewindButton.Enabled = false;
                    undoButton.Enabled = false;
                }
                else
                {
                    rewindButton.Enabled = true;
                    undoButton.Enabled = true;
                }

                // Find reasons to disable the redo and fast forward buttons
                if (historyControl.HistoryStack.RedoStack.Count == 0)
                {
                    fastForwardButton.Enabled = false;
                    redoButton.Enabled = false;
                }
                else
                {
                    fastForwardButton.Enabled = true;
                    redoButton.Enabled = true;
                }
            }
        }

        private void HistoryControl_HistoryChanged(object sender, System.EventArgs e)
        {
            OnRelinquishFocus();
            UpdateHistoryButtons();
        }

        private void OnToolStripButtonClick(object sender, EventArgs e)
        {
            if (sender == undoButton)
            {
                OnUndoButtonClicked();
            }
            else if (sender == redoButton)
            {
                OnRedoButtonClicked();
            }
            else if (sender == rewindButton)
            {
                OnRewindButtonClicked();
            }
            else if (sender == fastForwardButton)
            {
                OnFastForwardButtonClicked();
            }

            OnRelinquishFocus();
        }
    }
}
