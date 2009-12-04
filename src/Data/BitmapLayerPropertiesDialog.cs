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
    public sealed class BitmapLayerPropertiesDialog 
        : LayerPropertiesDialog
    {
        private System.Windows.Forms.Label opacityLabel;
        private System.Windows.Forms.Label blendModeLabel;
        private System.Windows.Forms.ComboBox blendOpComboBox;
        private System.Windows.Forms.NumericUpDown opacityUpDown;
        private System.Windows.Forms.TrackBar opacityTrackBar;
        private PaintDotNet.HeaderLabel blendingHeader;
        private System.ComponentModel.IContainer components = null;

        public BitmapLayerPropertiesDialog()
        {
            // This call is required by the Windows Form Designer.
            InitializeComponent();

            this.blendingHeader.Text = PdnResources.GetString("BitmapLayerPropertiesDialog.BlendingHeader.Text");
            this.blendModeLabel.Text = PdnResources.GetString("BitmapLayerPropertiesDialog.BlendModeLabel.Text");
            this.opacityLabel.Text = PdnResources.GetString("BitmapLayerPropertiesDialog.OpacityLabel.Text");

            // populate the blendOpComboBox with all the blend modes they're allowed to use
            foreach (Type type in UserBlendOps.GetBlendOps())
            {
                blendOpComboBox.Items.Add(UserBlendOps.CreateBlendOp(type));
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        private void SelectOp(UserBlendOp setOp)
        {
            foreach (object op in blendOpComboBox.Items)
            {
                if (op.ToString() == setOp.ToString())
                {
                    blendOpComboBox.SelectedItem = op;
                    break;
                }
            }
        }

        protected override void InitDialogFromLayer()
        {
            opacityUpDown.Value = Layer.Opacity;
            SelectOp(((BitmapLayer)Layer).BlendOp);
            base.InitDialogFromLayer();
        }

        protected override void InitLayerFromDialog()
        {
            ((BitmapLayer)Layer).Opacity = (byte)opacityUpDown.Value;

            if (blendOpComboBox.SelectedItem != null)
            {
                ((BitmapLayer)Layer).SetBlendOp((UserBlendOp)blendOpComboBox.SelectedItem);
            }

            base.InitLayerFromDialog();
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
            this.blendModeLabel = new System.Windows.Forms.Label();
            this.blendOpComboBox = new System.Windows.Forms.ComboBox();
            this.opacityUpDown = new System.Windows.Forms.NumericUpDown();
            this.opacityTrackBar = new System.Windows.Forms.TrackBar();
            this.opacityLabel = new System.Windows.Forms.Label();
            this.blendingHeader = new PaintDotNet.HeaderLabel();
            ((System.ComponentModel.ISupportInitialize)(this.opacityUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.opacityTrackBar)).BeginInit();
            this.SuspendLayout();
            // 
            // visibleCheckBox
            // 
            this.visibleCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.visibleCheckBox.Location = new System.Drawing.Point(8, 48);
            this.visibleCheckBox.Name = "visibleCheckBox";
            // 
            // nameLabel
            // 
            this.nameLabel.Location = new System.Drawing.Point(6, 27);
            this.nameLabel.Name = "nameLabel";
            // 
            // nameBox
            // 
            this.nameBox.Name = "nameBox";
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(193, 152);
            this.cancelButton.Name = "cancelButton";
            // 
            // okButton
            // 
            this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.okButton.Location = new System.Drawing.Point(112, 152);
            this.okButton.Name = "okButton";
            // 
            // blendModeLabel
            // 
            this.blendModeLabel.Location = new System.Drawing.Point(6, 92);
            this.blendModeLabel.Name = "blendModeLabel";
            this.blendModeLabel.AutoSize = true;
            this.blendModeLabel.Size = new System.Drawing.Size(50, 23);
            this.blendModeLabel.TabIndex = 4;
            // 
            // blendOpComboBox
            // 
            this.blendOpComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.blendOpComboBox.Location = new System.Drawing.Point(64, 88);
            this.blendOpComboBox.Name = "blendOpComboBox";
            this.blendOpComboBox.Size = new System.Drawing.Size(121, 21);
            this.blendOpComboBox.TabIndex = 4;
            this.blendOpComboBox.SelectedIndexChanged += new System.EventHandler(this.blendOpComboBox_SelectedIndexChanged);
            this.blendOpComboBox.MaxDropDownItems = 100;
            // 
            // opacityUpDown
            // 
            this.opacityUpDown.Location = new System.Drawing.Point(64, 116);
            this.opacityUpDown.Maximum = new System.Decimal(new int[] {
                                                                          255,
                                                                          0,
                                                                          0,
                                                                          0});
            this.opacityUpDown.Name = "opacityUpDown";
            this.opacityUpDown.Size = new System.Drawing.Size(56, 20);
            this.opacityUpDown.TabIndex = 5;
            this.opacityUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.opacityUpDown.Enter += new System.EventHandler(this.opacityUpDown_Enter);
            this.opacityUpDown.KeyUp += new System.Windows.Forms.KeyEventHandler(this.opacityUpDown_KeyUp);
            this.opacityUpDown.ValueChanged += new System.EventHandler(this.opacityUpDown_ValueChanged);
            this.opacityUpDown.Leave += new System.EventHandler(this.opacityUpDown_Leave);
            // 
            // opacityTrackBar
            // 
            this.opacityTrackBar.AutoSize = false;
            this.opacityTrackBar.LargeChange = 32;
            this.opacityTrackBar.Location = new System.Drawing.Point(129, 114);
            this.opacityTrackBar.Maximum = 255;
            this.opacityTrackBar.Name = "opacityTrackBar";
            this.opacityTrackBar.Size = new System.Drawing.Size(146, 24);
            this.opacityTrackBar.TabIndex = 6;
            this.opacityTrackBar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.opacityTrackBar.ValueChanged += new System.EventHandler(this.opacityTrackBar_ValueChanged);
            // 
            // opacityLabel
            // 
            this.opacityLabel.Location = new System.Drawing.Point(6, 118);
            this.opacityLabel.AutoSize = true;
            this.opacityLabel.Name = "opacityLabel";
            this.opacityLabel.Size = new System.Drawing.Size(48, 16);
            this.opacityLabel.TabIndex = 0;
            // 
            // blendingHeader
            // 
            this.blendingHeader.Location = new System.Drawing.Point(6, 72);
            this.blendingHeader.Name = "blendingHeader";
            this.blendingHeader.Size = new System.Drawing.Size(269, 14);
            this.blendingHeader.TabIndex = 8;
            this.blendingHeader.TabStop = false;
            // 
            // BitmapLayerPropertiesDialog
            // 
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(274, 181);
            this.Controls.Add(this.blendingHeader);
            this.Controls.Add(this.blendOpComboBox);
            this.Controls.Add(this.opacityUpDown);
            this.Controls.Add(this.opacityLabel);
            this.Controls.Add(this.blendModeLabel);
            this.Controls.Add(this.opacityTrackBar);
            this.Location = new System.Drawing.Point(0, 0);
            this.Name = "BitmapLayerPropertiesDialog";
            this.Controls.SetChildIndex(this.opacityTrackBar, 0);
            this.Controls.SetChildIndex(this.blendModeLabel, 0);
            this.Controls.SetChildIndex(this.opacityLabel, 0);
            this.Controls.SetChildIndex(this.opacityUpDown, 0);
            this.Controls.SetChildIndex(this.blendOpComboBox, 0);
            this.Controls.SetChildIndex(this.nameLabel, 0);
            this.Controls.SetChildIndex(this.visibleCheckBox, 0);
            this.Controls.SetChildIndex(this.nameBox, 0);
            this.Controls.SetChildIndex(this.blendingHeader, 0);
            this.Controls.SetChildIndex(this.cancelButton, 0);
            this.Controls.SetChildIndex(this.okButton, 0);
            ((System.ComponentModel.ISupportInitialize)(this.opacityUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.opacityTrackBar)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion

        private void ChangeLayerOpacity()
        {
            if (((BitmapLayer)Layer).Opacity != (byte)opacityUpDown.Value)
            {
                Layer.PushSuppressPropertyChanged();
                ((BitmapLayer)Layer).Opacity = (byte)opacityTrackBar.Value;
                Layer.PopSuppressPropertyChanged();
            }
        }

        private void opacityUpDown_ValueChanged(object sender, System.EventArgs e)
        {
            if (opacityTrackBar.Value != (int)opacityUpDown.Value)
            {
                using (new WaitCursorChanger(this))
                {
                    opacityTrackBar.Value = (int)opacityUpDown.Value;
                    ChangeLayerOpacity();
                }
            }
        }

        private void opacityUpDown_Enter(object sender, System.EventArgs e)
        {
            opacityUpDown.Select(0, opacityUpDown.Text.Length);
        }

        private void opacityUpDown_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
        }

        private void opacityTrackBar_ValueChanged(object sender, System.EventArgs e)
        {
            if (opacityUpDown.Value != (decimal)opacityTrackBar.Value)
            {
                using (new WaitCursorChanger(this))
                {
                    opacityUpDown.Value = (decimal)opacityTrackBar.Value;
                    ChangeLayerOpacity();
                }
            }
        }

        private void opacityUpDown_Leave(object sender, System.EventArgs e)
        {
            opacityUpDown_ValueChanged(sender, e);
        }

        private void blendOpComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            using (new WaitCursorChanger(this))
            {
                Layer.PushSuppressPropertyChanged();

                if (blendOpComboBox.SelectedItem != null)
                {
                    ((BitmapLayer)Layer).SetBlendOp((UserBlendOp)blendOpComboBox.SelectedItem);
                }

                Layer.PopSuppressPropertyChanged();
            }
        }
    }
}