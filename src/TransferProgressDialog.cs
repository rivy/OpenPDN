/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PaintDotNet
{
    internal class TransferProgressDialog 
        : PdnBaseForm
    {
        private HeaderLabel separator1;
        private ProgressBar progressBar;
        private Button cancelButton;
        private Label itemText;
        private Label operationProgress;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        public string ItemText
        {
            get
            {
                return this.itemText.Text;
            }

            set
            {
                this.itemText.Text = value;
            }
        }

        public string OperationProgress
        {
            get
            {
                return this.operationProgress.Text;
            }

            set
            {
                this.operationProgress.Text = value;
            }
        }

        public event EventHandler CancelClicked;

        protected virtual void OnCancelClicked()
        {
            if (CancelClicked != null)
            {
                CancelClicked(this, EventArgs.Empty);
            }
        }

        public ProgressBar ProgressBar
        {
            get
            {
                return this.progressBar;
            }
        }

        public bool CancelEnabled
        {
            get
            {
                return this.cancelButton.Enabled;
            }

            set
            {
                this.cancelButton.Enabled = value;
            }
        }

        public TransferProgressDialog()
        {
            PdnBaseForm.RegisterFormHotKey(
                Keys.Escape,
                delegate(Keys keys)
                {
                    OnCancelClicked();
                    return true;
                });

            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            SystemLayer.UI.DisableCloseBox(this);
            base.OnLoad(e);
        }

        public override void LoadResources()
        {
            this.cancelButton.Text = PdnResources.GetString("Form.CancelButton.Text");
            base.LoadResources();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
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
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.cancelButton = new System.Windows.Forms.Button();
            this.itemText = new System.Windows.Forms.Label();
            this.separator1 = new PaintDotNet.HeaderLabel();
            this.operationProgress = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(10, 51);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(405, 19);
            this.progressBar.TabIndex = 0;
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(342, 91);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "cancelButton";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.FlatStyle = FlatStyle.System;
            this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // itemText
            // 
            this.itemText.AutoEllipsis = true;
            this.itemText.Location = new System.Drawing.Point(8, 8);
            this.itemText.Name = "itemText";
            this.itemText.Size = new System.Drawing.Size(404, 13);
            this.itemText.TabIndex = 2;
            this.itemText.Text = "itemText";
            // 
            // separator1
            // 
            this.separator1.Location = new System.Drawing.Point(9, 77);
            this.separator1.Name = "separator1";
            this.separator1.RightMargin = 0;
            this.separator1.Size = new System.Drawing.Size(406, 14);
            this.separator1.TabIndex = 4;
            this.separator1.TabStop = false;
            // 
            // operationProgress
            // 
            this.operationProgress.AutoEllipsis = true;
            this.operationProgress.Location = new System.Drawing.Point(8, 28);
            this.operationProgress.Name = "operationProgress";
            this.operationProgress.Size = new System.Drawing.Size(403, 13);
            this.operationProgress.TabIndex = 5;
            this.operationProgress.Text = "operationProgress";
            // 
            // TransferProgressDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(423, 121);
            this.Controls.Add(this.operationProgress);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.itemText);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.separator1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Location = new System.Drawing.Point(0, 0);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TransferProgressDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "TransferProgressDialog";
            this.Controls.SetChildIndex(this.separator1, 0);
            this.Controls.SetChildIndex(this.cancelButton, 0);
            this.Controls.SetChildIndex(this.itemText, 0);
            this.Controls.SetChildIndex(this.progressBar, 0);
            this.Controls.SetChildIndex(this.operationProgress, 0);
            this.ResumeLayout(false);

        }
        #endregion

        private void CancelButton_Click(object sender, EventArgs e)
        {
            OnCancelClicked();
        }
    }
}