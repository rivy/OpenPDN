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
using System.Threading;
using System.Windows.Forms;

namespace PaintDotNet
{
    internal class ProgressDialog 
        : PdnBaseForm
    {
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label percentText;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Label descriptionLabel;
        private System.ComponentModel.IContainer components;
        private WaitCursorChanger waitCursorChanger;
        private bool cancelled;

        private int normalHeight;
        private int noButtonHeight;
        private bool cancellable = true;
        private bool done = false;

        public ProgressDialog()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            this.Value = 0.0;
            
            Point bottomPoint = this.PointToScreen(new Point(0, Bottom));
            Point topPoint = this.cancelButton.PointToScreen(new Point(0, 0));
            normalHeight = Height;
            noButtonHeight = Height - 32;

            this.cancelButton.Text = PdnResources.GetString("Form.CancelButton.Text");
        }

        public string Description
        {
            get
            {
                return descriptionLabel.Text;
            }

            set
            {
                descriptionLabel.Text = value;
            }
        }

        private bool marquee = false;
        public bool MarqueeMode
        {
            get
            {
                return this.marquee;
            }

            set
            {
                this.marquee = value;
                this.progressBar.Style = this.marquee ? ProgressBarStyle.Marquee : ProgressBarStyle.Blocks;
            }
        }

        public bool PercentTextVisible
        {
            get
            {
                return this.percentText.Visible;
            }

            set
            {
                this.percentText.Visible = value;
            }
        }

        public bool Cancellable
        {
            get
            {
                return cancelButton.Visible;
            }

            set
            {
                if (value)
                {
                    this.Height = normalHeight;
                    this.Cursor = System.Windows.Forms.Cursors.Default;
                }
                else
                {
                    this.Height = noButtonHeight;
                    this.Cursor = System.Windows.Forms.Cursors.WaitCursor;
                }

                this.cancelButton.Visible = value;
                cancellable = value;
            }
        }

        public bool Cancelled
        {
            get
            {
                return this.cancelled;
            }
        }

        public double Value
        {
            get
            {
                return (double)progressBar.Value;
            }

            set
            {
                int intValue = (int)value;
                string textFormat = PdnResources.GetString("ProgressDialog.PercentText.Text.Format");
                string text = string.Format(textFormat, intValue); 

                if (text != percentText.Text)
                {
                    percentText.Text = text;
                    progressBar.Value = Math.Max(progressBar.Minimum, Math.Min(progressBar.Maximum, intValue));
                    Update();
                }
            }
        }

        private void SetValueHigher(object higherValue)
        {
            double newValue = (double)higherValue;

            if (this.Value <= newValue)
            {
                this.Value = newValue;
            }
        }

        public void ExternalFinish()
        {
            this.done = true;
            DialogResult = DialogResult.OK;
            Close();
        }

        private int tileCount = 0;
        public void RenderedTileHandler(object sender, RenderedTileEventArgs e)
        {
            lock (this)
            {
                ++this.tileCount;
                double newValue = 100.0 * ((double)(tileCount + 1) / (double)e.TileCount);

                if (newValue > 100.0)
                {
                    newValue = 100.0;
                }

                if (this.IsHandleCreated)
                {
                    BeginInvoke(new WaitCallback(SetValueHigher), new object[] { newValue });
                }
            }
        }

        public void FinishedRenderingHandler(object sender, EventArgs e)
        {
            if (this.IsHandleCreated)
            {
                BeginInvoke(new Procedure(ExternalFinish), null);
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing (e);

            if (!cancellable && !done)
            {
                e.Cancel = true;
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
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.descriptionLabel = new System.Windows.Forms.Label();
            this.percentText = new System.Windows.Forms.Label();
            this.cancelButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(17, 32);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(184, 16);
            this.progressBar.Step = 1;
            this.progressBar.TabIndex = 0;
            // 
            // descriptionLabel
            // 
            this.descriptionLabel.AutoEllipsis = true;
            this.descriptionLabel.Location = new System.Drawing.Point(16, 8);
            this.descriptionLabel.Name = "descriptionLabel";
            this.descriptionLabel.Size = new System.Drawing.Size(184, 16);
            this.descriptionLabel.TabIndex = 1;
            // 
            // percentText
            // 
            this.percentText.Location = new System.Drawing.Point(59, 56);
            this.percentText.Name = "percentText";
            this.percentText.Size = new System.Drawing.Size(100, 16);
            this.percentText.TabIndex = 2;
            this.percentText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cancelButton.Location = new System.Drawing.Point(72, 80);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.TabIndex = 3;
            this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // ProgressDialog
            // 
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(218, 109);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.percentText);
            this.Controls.Add(this.descriptionLabel);
            this.Controls.Add(this.progressBar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ProgressDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Controls.SetChildIndex(this.progressBar, 0);
            this.Controls.SetChildIndex(this.descriptionLabel, 0);
            this.Controls.SetChildIndex(this.percentText, 0);
            this.Controls.SetChildIndex(this.cancelButton, 0);
            this.ResumeLayout(false);
        }
        #endregion

        public event EventHandler CancelClick;
        protected virtual void OnCancelClick()
        {
            if (CancelClick != null)
            {
                CancelClick(this, EventArgs.Empty);
            }
        }

        private void CancelButton_Click(object sender, System.EventArgs e)
        {
            this.cancelled = true;
            OnCancelClick();
            DialogResult = DialogResult.Cancel;
            Close();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.waitCursorChanger = new WaitCursorChanger(this.Owner);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            if (this.waitCursorChanger != null)
            {
                this.waitCursorChanger.Dispose();
                this.waitCursorChanger = null;
            }
        }
    }
}
