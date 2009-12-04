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
    internal class CanvasSizeDialog 
        : ResizeDialog
    {
        private EnumLocalizer anchorEdgeNames = EnumLocalizer.Create(typeof(AnchorEdge));
        private AnchorChooserControl anchorChooserControl;
        private System.Windows.Forms.Label newSpaceLabel;
        private PaintDotNet.HeaderLabel anchorHeader;
        private System.Windows.Forms.ComboBox anchorEdgeCB;
        private System.ComponentModel.IContainer components = null;

        [DefaultValue(AnchorEdge.TopLeft)]
        public AnchorEdge AnchorEdge
        {
            get
            {
                return anchorChooserControl.AnchorEdge;
            }

            set
            {
                anchorChooserControl.AnchorEdge = value;
            }
        }

        public CanvasSizeDialog()
        {
            // This call is required by the Windows Form Designer.
            InitializeComponent();

            this.Icon = Utility.ImageToIcon(PdnResources.GetImageResource("Icons.MenuImageCanvasSizeIcon.png").Reference, Utility.TransparentKey);

            this.Text = PdnResources.GetString("CanvasSizeDialog.Text"); // "Canvas Size";
            this.anchorHeader.Text = PdnResources.GetString("CanvasSizeDialog.AnchorHeader.Text"); //"Anchor";
            this.newSpaceLabel.Text = PdnResources.GetString("CanvasSizeDialog.NewSpaceLabel.Text"); //"The new space will be filled with the currently selected background color.";

            foreach (string name in Enum.GetNames(typeof(AnchorEdge)))
            {
                AnchorEdge value = (AnchorEdge)Enum.Parse(typeof(AnchorEdge), name, true);
                string itemName = this.anchorEdgeNames.EnumValueToLocalizedName(value);
                this.anchorEdgeCB.Items.Add(itemName);

                if (value == this.AnchorEdge)
                {
                    this.anchorEdgeCB.SelectedItem = itemName;
                }
            }

            anchorChooserControl_AnchorEdgeChanged(anchorChooserControl, EventArgs.Empty);
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

        #region Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.anchorChooserControl = new PaintDotNet.AnchorChooserControl();
            this.newSpaceLabel = new System.Windows.Forms.Label();
            this.anchorHeader = new PaintDotNet.HeaderLabel();
            this.anchorEdgeCB = new System.Windows.Forms.ComboBox();
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
            this.constrainCheckBox.Location = new System.Drawing.Point(27, 74);
            this.constrainCheckBox.Name = "constrainCheckBox";
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(142, 366);
            this.okButton.Name = "okButton";
            this.okButton.TabIndex = 18;
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(220, 366);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.TabIndex = 19;
            // 
            // percentSignLabel
            // 
            this.percentSignLabel.Location = new System.Drawing.Point(200, 28);
            this.percentSignLabel.Name = "percentSignLabel";
            this.percentSignLabel.TabIndex = 23;
            // 
            // percentUpDown
            // 
            this.percentUpDown.Location = new System.Drawing.Point(120, 27);
            this.percentUpDown.Name = "percentUpDown";
            this.percentUpDown.TabIndex = 22;
            // 
            // absoluteRB
            // 
            this.absoluteRB.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.absoluteRB.Location = new System.Drawing.Point(8, 51);
            this.absoluteRB.Name = "absoluteRB";
            // 
            // percentRB
            // 
            this.percentRB.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.percentRB.Location = new System.Drawing.Point(8, 24);
            this.percentRB.Name = "percentRB";
            this.percentRB.TabIndex = 21;
            // 
            // asteriskTextLabel
            // 
            this.asteriskTextLabel.Enabled = false;
            this.asteriskTextLabel.Location = new System.Drawing.Point(400, 72);
            this.asteriskTextLabel.Name = "asteriskTextLabel";
            this.asteriskTextLabel.Visible = true;
            // 
            // asteriskLabel
            // 
            this.asteriskLabel.Enabled = false;
            this.asteriskLabel.Location = new System.Drawing.Point(648, 32);
            this.asteriskLabel.Name = "asteriskLabel";
            this.asteriskLabel.Visible = true;
            // 
            // resizedImageHeader
            // 
            this.resizedImageHeader.Name = "resizedImageHeader";
            this.resizedImageHeader.TabIndex = 20;
            // 
            // resolutionLabel
            // 
            this.resolutionLabel.Location = new System.Drawing.Point(32, 166);
            this.resolutionLabel.Name = "resolutionLabel";
            // 
            // unitsComboBox2
            // 
            this.unitsComboBox2.Location = new System.Drawing.Point(200, 165);
            this.unitsComboBox2.Name = "unitsComboBox2";
            // 
            // unitsComboBox1
            // 
            this.unitsComboBox1.Location = new System.Drawing.Point(200, 208);
            this.unitsComboBox1.Name = "unitsComboBox1";
            // 
            // resolutionUpDown
            // 
            this.resolutionUpDown.Location = new System.Drawing.Point(120, 165);
            this.resolutionUpDown.Name = "resolutionUpDown";
            // 
            // newWidthLabel1
            // 
            this.newWidthLabel1.Location = new System.Drawing.Point(32, 118);
            this.newWidthLabel1.Name = "newWidthLabel1";
            // 
            // newHeightLabel1
            // 
            this.newHeightLabel1.Location = new System.Drawing.Point(32, 142);
            this.newHeightLabel1.Name = "newHeightLabel1";
            // 
            // pixelsLabel1
            // 
            this.pixelsLabel1.Location = new System.Drawing.Point(200, 118);
            this.pixelsLabel1.Name = "pixelsLabel1";
            // 
            // newWidthLabel2
            // 
            this.newWidthLabel2.Location = new System.Drawing.Point(32, 209);
            this.newWidthLabel2.Name = "newWidthLabel2";
            // 
            // newHeightLabel2
            // 
            this.newHeightLabel2.Location = new System.Drawing.Point(32, 233);
            this.newHeightLabel2.Name = "newHeightLabel2";
            // 
            // pixelsLabel2
            // 
            this.pixelsLabel2.Location = new System.Drawing.Point(200, 142);
            this.pixelsLabel2.Name = "pixelsLabel2";
            // 
            // unitsLabel1
            // 
            this.unitsLabel1.Location = new System.Drawing.Point(200, 234);
            this.unitsLabel1.Name = "unitsLabel1";
            // 
            // pixelWidthUpDown
            // 
            this.pixelWidthUpDown.Location = new System.Drawing.Point(120, 117);
            this.pixelWidthUpDown.Name = "pixelWidthUpDown";
            // 
            // pixelHeightUpDown
            // 
            this.pixelHeightUpDown.Location = new System.Drawing.Point(120, 141);
            this.pixelHeightUpDown.Name = "pixelHeightUpDown";
            // 
            // printWidthUpDown
            // 
            this.printWidthUpDown.Location = new System.Drawing.Point(120, 208);
            this.printWidthUpDown.Name = "printWidthUpDown";
            // 
            // printHeightUpDown
            // 
            this.printHeightUpDown.Location = new System.Drawing.Point(120, 232);
            this.printHeightUpDown.Name = "printHeightUpDown";
            // 
            // pixelSizeHeader
            // 
            this.pixelSizeHeader.Location = new System.Drawing.Point(25, 98);
            this.pixelSizeHeader.Name = "pixelSizeHeader";
            // 
            // printSizeHeader
            // 
            this.printSizeHeader.Location = new System.Drawing.Point(25, 189);
            this.printSizeHeader.Name = "printSizeHeader";
            // 
            // resamplingLabel
            // 
            this.resamplingLabel.Enabled = false;
            this.resamplingLabel.Location = new System.Drawing.Point(384, 40);
            this.resamplingLabel.Name = "resamplingLabel";
            this.resamplingLabel.Visible = false;
            // 
            // resamplingAlgorithmComboBox
            // 
            this.resamplingAlgorithmComboBox.Enabled = false;
            this.resamplingAlgorithmComboBox.Location = new System.Drawing.Point(496, 32);
            this.resamplingAlgorithmComboBox.Name = "resamplingAlgorithmComboBox";
            this.resamplingAlgorithmComboBox.Visible = false;
            // 
            // anchorChooserControl
            // 
            this.anchorChooserControl.Location = new System.Drawing.Point(177, 275);
            this.anchorChooserControl.Name = "anchorChooserControl";
            this.anchorChooserControl.Size = new System.Drawing.Size(81, 81);
            this.anchorChooserControl.TabIndex = 17;
            this.anchorChooserControl.TabStop = false;
            this.anchorChooserControl.AnchorEdgeChanged += new System.EventHandler(this.anchorChooserControl_AnchorEdgeChanged);
            // 
            // newSpaceLabel
            // 
            this.newSpaceLabel.Location = new System.Drawing.Point(376, 296);
            this.newSpaceLabel.Name = "newSpaceLabel";
            this.newSpaceLabel.Size = new System.Drawing.Size(234, 32);
            this.newSpaceLabel.TabIndex = 20;
            // anchorHeader
            // 
            this.anchorHeader.Location = new System.Drawing.Point(8, 256);
            this.anchorHeader.Name = "anchorHeader";
            this.anchorHeader.Size = new System.Drawing.Size(288, 14);
            this.anchorHeader.TabIndex = 15;
            this.anchorHeader.TabStop = false;
            // 
            // 
            // anchorEdgeCB
            // 
            this.anchorEdgeCB.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.anchorEdgeCB.Location = new System.Drawing.Point(32, 275);
            this.anchorEdgeCB.Name = "anchorEdgeCB";
            this.anchorEdgeCB.Size = new System.Drawing.Size(120, 21);
            this.anchorEdgeCB.TabIndex = 16;
            this.anchorEdgeCB.SelectedIndexChanged += new System.EventHandler(this.anchorEdgeCB_SelectedIndexChanged);
            // 
            // CanvasSizeDialog
            // 
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(298, 395);
            this.Controls.Add(this.anchorEdgeCB);
            this.Controls.Add(this.anchorHeader);
            this.Controls.Add(this.anchorChooserControl);
            this.Controls.Add(this.newSpaceLabel);
            this.Location = new System.Drawing.Point(0, 0);
            this.Name = "CanvasSizeDialog";
            this.Controls.SetChildIndex(this.pixelsLabel1, 0);
            this.Controls.SetChildIndex(this.unitsLabel1, 0);
            this.Controls.SetChildIndex(this.newWidthLabel1, 0);
            this.Controls.SetChildIndex(this.resamplingLabel, 0);
            this.Controls.SetChildIndex(this.resolutionLabel, 0);
            this.Controls.SetChildIndex(this.asteriskTextLabel, 0);
            this.Controls.SetChildIndex(this.asteriskLabel, 0);
            this.Controls.SetChildIndex(this.pixelsLabel2, 0);
            this.Controls.SetChildIndex(this.percentSignLabel, 0);
            this.Controls.SetChildIndex(this.newSpaceLabel, 0);
            this.Controls.SetChildIndex(this.newHeightLabel1, 0);
            this.Controls.SetChildIndex(this.newWidthLabel2, 0);
            this.Controls.SetChildIndex(this.newHeightLabel2, 0);
            this.Controls.SetChildIndex(this.resizedImageHeader, 0);
            this.Controls.SetChildIndex(this.resolutionUpDown, 0);
            this.Controls.SetChildIndex(this.unitsComboBox2, 0);
            this.Controls.SetChildIndex(this.unitsComboBox1, 0);
            this.Controls.SetChildIndex(this.printWidthUpDown, 0);
            this.Controls.SetChildIndex(this.printHeightUpDown, 0);
            this.Controls.SetChildIndex(this.pixelSizeHeader, 0);
            this.Controls.SetChildIndex(this.printSizeHeader, 0);
            this.Controls.SetChildIndex(this.pixelHeightUpDown, 0);
            this.Controls.SetChildIndex(this.pixelWidthUpDown, 0);
            this.Controls.SetChildIndex(this.anchorChooserControl, 0);
            this.Controls.SetChildIndex(this.constrainCheckBox, 0);
            this.Controls.SetChildIndex(this.resamplingAlgorithmComboBox, 0);
            this.Controls.SetChildIndex(this.percentRB, 0);
            this.Controls.SetChildIndex(this.absoluteRB, 0);
            this.Controls.SetChildIndex(this.percentUpDown, 0);
            this.Controls.SetChildIndex(this.anchorHeader, 0);
            this.Controls.SetChildIndex(this.anchorEdgeCB, 0);
            this.Controls.SetChildIndex(this.okButton, 0);
            this.Controls.SetChildIndex(this.cancelButton, 0);
            ((System.ComponentModel.ISupportInitialize)(this.percentUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.resolutionUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pixelWidthUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pixelHeightUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.printWidthUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.printHeightUpDown)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion

        private void anchorChooserControl_AnchorEdgeChanged(object sender, System.EventArgs e)
        {
            string newItem = this.anchorEdgeNames.EnumValueToLocalizedName(anchorChooserControl.AnchorEdge);
            this.anchorEdgeCB.SelectedItem = newItem;
        }

        private void anchorEdgeCB_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            AnchorEdge newAnchorEdge = (AnchorEdge)this.anchorEdgeNames.LocalizedNameToEnumValue((string)this.anchorEdgeCB.SelectedItem);
            this.AnchorEdge = newAnchorEdge;
        }
    }
}

