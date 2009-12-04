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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace PaintDotNet
{
    public class PdnBaseDialog 
        : PdnBaseForm
    {
        protected System.Windows.Forms.Button baseOkButton;
        protected System.Windows.Forms.Button baseCancelButton;
        private System.ComponentModel.IContainer components = null;

        public PdnBaseDialog()
        {
            // This call is required by the Windows Form Designer.
            InitializeComponent();

            if (!this.DesignMode)
            {
                this.baseOkButton.Text = PdnResources.GetString("Form.OkButton.Text");
                this.baseCancelButton.Text = PdnResources.GetString("Form.CancelButton.Text");
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

        public DialogResult ShowDialog(Control owner)
        {
            return Utility.ShowDialog(this, owner);
        }

        #region Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.baseOkButton = new System.Windows.Forms.Button();
            this.baseCancelButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // baseOkButton
            // 
            this.baseOkButton.Location = new System.Drawing.Point(77, 128);
            this.baseOkButton.Name = "baseOkButton";
            this.baseOkButton.TabIndex = 1;
            this.baseOkButton.FlatStyle = FlatStyle.System;
            this.baseOkButton.Click += new System.EventHandler(this.baseOkButton_Click);
            // 
            // baseCancelButton
            // 
            this.baseCancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.baseCancelButton.Location = new System.Drawing.Point(165, 128);
            this.baseCancelButton.Name = "baseCancelButton";
            this.baseCancelButton.TabIndex = 2;
            this.baseCancelButton.FlatStyle = FlatStyle.System; 
            this.baseCancelButton.Click += new System.EventHandler(this.baseCancelButton_Click);
            // 
            // PdnBaseDialog
            // 
            this.AcceptButton = this.baseOkButton;
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.baseCancelButton;
            this.ClientSize = new System.Drawing.Size(248, 158);
            this.Controls.Add(this.baseCancelButton);
            this.Controls.Add(this.baseOkButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MinimizeBox = false;
            this.Name = "PdnBaseDialog";
            this.ShowInTaskbar = false;
            this.Text = "PdnBaseDialog";
            this.Controls.SetChildIndex(this.baseOkButton, 0);
            this.Controls.SetChildIndex(this.baseCancelButton, 0);
            this.ResumeLayout(false);

        }
        #endregion

        private void baseOkButton_Click(object sender, System.EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void baseCancelButton_Click(object sender, System.EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}

