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
    internal class NewFileDialog 
        : ResizeDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

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

        public NewFileDialog()
        {
            InitializeComponent();
            this.Icon = Utility.ImageToIcon(PdnResources.GetImageResource("Icons.MenuFileNewIcon.png").Reference, Utility.TransparentKey);
            this.Text = PdnResources.GetString("NewFileDialog.Text"); // "New";
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            ((System.ComponentModel.ISupportInitialize)(this.percentUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.resolutionUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pixelWidthUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pixelHeightUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.printWidthUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.printHeightUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // constrainCheckBox
            // 
            this.constrainCheckBox.Location = new System.Drawing.Point(8, 28);
            this.constrainCheckBox.Name = "constrainCheckBox";
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(123, 217);
            this.okButton.Name = "okButton";
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(201, 217);
            this.cancelButton.Name = "cancelButton";
            // 
            // percentSignLabel
            // 
            this.percentSignLabel.Enabled = false;
            this.percentSignLabel.Location = new System.Drawing.Point(520, 48);
            this.percentSignLabel.Name = "percentSignLabel";
            this.percentSignLabel.Visible = false;
            // 
            // percentUpDown
            // 
            this.percentUpDown.Location = new System.Drawing.Point(440, 48);
            this.percentUpDown.Name = "percentUpDown";
            this.percentUpDown.Visible = false;
            // 
            // absoluteRB
            // 
            this.absoluteRB.Enabled = false;
            this.absoluteRB.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.absoluteRB.Location = new System.Drawing.Point(328, 72);
            this.absoluteRB.Name = "absoluteRB";
            this.absoluteRB.Visible = false;
            // 
            // percentRB
            // 
            this.percentRB.Enabled = false;
            this.percentRB.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.percentRB.Location = new System.Drawing.Point(328, 40);
            this.percentRB.Name = "percentRB";
            this.percentRB.Visible = false;
            // 
            // asteriskTextLabel
            // 
            this.asteriskTextLabel.Enabled = false;
            this.asteriskTextLabel.Location = new System.Drawing.Point(328, 280);
            this.asteriskTextLabel.Name = "asteriskTextLabel";
            // 
            // asteriskLabel
            // 
            this.asteriskLabel.Enabled = false;
            this.asteriskLabel.Location = new System.Drawing.Point(592, 16);
            this.asteriskLabel.Name = "asteriskLabel";
            // 
            // resizedImageHeader
            // 
            this.resizedImageHeader.Name = "resizedImageHeader";
            this.resizedImageHeader.Size = new System.Drawing.Size(274, 16);
            // 
            // resolutionLabel
            // 
            this.resolutionLabel.Location = new System.Drawing.Point(16, 118);
            this.resolutionLabel.Name = "resolutionLabel";
            // 
            // unitsComboBox2
            // 
            this.unitsComboBox2.Location = new System.Drawing.Point(184, 117);
            this.unitsComboBox2.Name = "unitsComboBox2";
            // 
            // unitsComboBox1
            // 
            this.unitsComboBox1.Location = new System.Drawing.Point(184, 160);
            this.unitsComboBox1.Name = "unitsComboBox1";
            // 
            // resolutionUpDown
            // 
            this.resolutionUpDown.Location = new System.Drawing.Point(104, 117);
            this.resolutionUpDown.Name = "resolutionUpDown";
            // 
            // newWidthLabel1
            // 
            this.newWidthLabel1.Location = new System.Drawing.Point(16, 70);
            this.newWidthLabel1.Name = "newWidthLabel1";
            // 
            // newHeightLabel1
            // 
            this.newHeightLabel1.Location = new System.Drawing.Point(16, 94);
            this.newHeightLabel1.Name = "newHeightLabel1";
            // 
            // pixelsLabel1
            // 
            this.pixelsLabel1.Location = new System.Drawing.Point(184, 70);
            this.pixelsLabel1.Name = "pixelsLabel1";
            // 
            // newWidthLabel2
            // 
            this.newWidthLabel2.Location = new System.Drawing.Point(16, 161);
            this.newWidthLabel2.Name = "newWidthLabel2";
            // 
            // newHeightLabel2
            // 
            this.newHeightLabel2.Location = new System.Drawing.Point(16, 185);
            this.newHeightLabel2.Name = "newHeightLabel2";
            // 
            // pixelsLabel2
            // 
            this.pixelsLabel2.Location = new System.Drawing.Point(184, 94);
            this.pixelsLabel2.Name = "pixelsLabel2";
            // 
            // unitsLabel1
            // 
            this.unitsLabel1.Location = new System.Drawing.Point(184, 186);
            this.unitsLabel1.Name = "unitsLabel1";
            // 
            // pixelWidthUpDown
            // 
            this.pixelWidthUpDown.Location = new System.Drawing.Point(104, 69);
            this.pixelWidthUpDown.Name = "pixelWidthUpDown";
            // 
            // pixelHeightUpDown
            // 
            this.pixelHeightUpDown.Location = new System.Drawing.Point(104, 93);
            this.pixelHeightUpDown.Name = "pixelHeightUpDown";
            // 
            // printWidthUpDown
            // 
            this.printWidthUpDown.Location = new System.Drawing.Point(104, 160);
            this.printWidthUpDown.Name = "printWidthUpDown";
            // 
            // printHeightUpDown
            // 
            this.printHeightUpDown.Location = new System.Drawing.Point(104, 184);
            this.printHeightUpDown.Name = "printHeightUpDown";
            // 
            // pixelSizeHeader
            // 
            this.pixelSizeHeader.Location = new System.Drawing.Point(6, 50);
            this.pixelSizeHeader.Name = "pixelSizeHeader";
            // 
            // printSizeHeader
            // 
            this.printSizeHeader.Location = new System.Drawing.Point(6, 141);
            this.printSizeHeader.Name = "printSizeHeader";
            // 
            // resamplingLabel
            // 
            this.resamplingLabel.Enabled = false;
            this.resamplingLabel.Location = new System.Drawing.Point(320, 24);
            this.resamplingLabel.Name = "resamplingLabel";
            this.resamplingLabel.Visible = false;
            // 
            // resamplingAlgorithmComboBox
            // 
            this.resamplingAlgorithmComboBox.Enabled = false;
            this.resamplingAlgorithmComboBox.Location = new System.Drawing.Point(440, 16);
            this.resamplingAlgorithmComboBox.Name = "resamplingAlgorithmComboBox";
            this.resamplingAlgorithmComboBox.Visible = false;
            // 
            // NewFileDialog
            // 
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(279, 246);
            this.Location = new System.Drawing.Point(0, 0);
            this.Name = "NewFileDialog";
            this.Controls.SetChildIndex(this.printWidthUpDown, 0);
            this.Controls.SetChildIndex(this.printHeightUpDown, 0);
            ((System.ComponentModel.ISupportInitialize)(this.percentUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.resolutionUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pixelWidthUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pixelHeightUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.printWidthUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.printHeightUpDown)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion
    }
}
