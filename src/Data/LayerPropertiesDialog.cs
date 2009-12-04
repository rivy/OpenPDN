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
    public class LayerPropertiesDialog 
        : PdnBaseForm
    {
        protected System.Windows.Forms.CheckBox visibleCheckBox;
        protected System.Windows.Forms.Label nameLabel;
        protected System.Windows.Forms.TextBox nameBox;
        protected System.Windows.Forms.Button cancelButton;
        protected System.Windows.Forms.Button okButton;
        private System.ComponentModel.Container components = null;
        private object originalProperties = null;
        private PaintDotNet.HeaderLabel generalHeader;        

        private Layer layer;

        [Browsable(false)]
        public Layer Layer
        {
            get
            {
                return layer;
            }

            set
            {
                this.layer = value;
                this.originalProperties = this.layer.SaveProperties();
                InitDialogFromLayer();
            }
        }

        protected virtual void InitLayerFromDialog()
        {
            this.layer.Name = this.nameBox.Text;
            this.layer.Visible = this.visibleCheckBox.Checked;
            
            if (this.Owner != null)
            {
                this.Owner.Update();
            }
        }

        protected virtual void InitDialogFromLayer()
        {
            this.nameBox.Text = this.layer.Name;
            this.visibleCheckBox.Checked = this.layer.Visible;
        }

        public LayerPropertiesDialog()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            this.Icon = Utility.ImageToIcon(PdnResources.GetImage("Icons.MenuLayersLayerPropertiesIcon.png"), Color.FromArgb(192, 192, 192));

            this.Text = PdnResources.GetString("LayerPropertiesDialog.Text");
            this.visibleCheckBox.Text = PdnResources.GetString("LayerPropertiesDialog.VisibleCheckBox.Text");
            this.nameLabel.Text = PdnResources.GetString("LayerPropertiesDialog.NameLabel.Text");
            this.generalHeader.Text = PdnResources.GetString("LayerPropertiesDialog.GeneralHeader.Text");
            this.cancelButton.Text = PdnResources.GetString("Form.CancelButton.Text");
            this.okButton.Text = PdnResources.GetString("Form.OkButton.Text");
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

        protected override void OnLoad(EventArgs e)
        {
            this.nameBox.Select();
            this.nameBox.Select(0, this.nameBox.Text.Length);
            base.OnLoad(e);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.visibleCheckBox = new System.Windows.Forms.CheckBox();
            this.nameBox = new System.Windows.Forms.TextBox();
            this.nameLabel = new System.Windows.Forms.Label();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.generalHeader = new PaintDotNet.HeaderLabel();
            this.SuspendLayout();
            // 
            // visibleCheckBox
            // 
            this.visibleCheckBox.Location = new System.Drawing.Point(14, 43);
            this.visibleCheckBox.Name = "visibleCheckBox";
            this.visibleCheckBox.Size = new System.Drawing.Size(90, 16);
            this.visibleCheckBox.TabIndex = 3;
            this.visibleCheckBox.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.visibleCheckBox.FlatStyle = FlatStyle.System;
            this.visibleCheckBox.CheckedChanged += new System.EventHandler(this.VisibleCheckBox_CheckedChanged);
            // 
            // nameBox
            // 
            this.nameBox.Location = new System.Drawing.Point(64, 24);
            this.nameBox.Name = "nameBox";
            this.nameBox.Size = new System.Drawing.Size(200, 20);
            this.nameBox.TabIndex = 2;
            this.nameBox.Text = "";
            this.nameBox.Enter += new System.EventHandler(this.NameBox_Enter);
            // 
            // nameLabel
            // 
            this.nameLabel.Location = new System.Drawing.Point(6, 24);
            this.nameLabel.Name = "nameLabel";
            this.nameLabel.Size = new System.Drawing.Size(50, 16);
            this.nameLabel.TabIndex = 2;
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cancelButton.Location = new System.Drawing.Point(194, 69);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.Location = new System.Drawing.Point(114, 69);
            this.okButton.Name = "okButton";
            this.okButton.TabIndex = 0;
            this.okButton.FlatStyle = FlatStyle.System;
            this.okButton.Click += new System.EventHandler(this.OkButton_Click);
            // 
            // generalHeader
            // 
            this.generalHeader.Location = new System.Drawing.Point(6, 8);
            this.generalHeader.Name = "generalHeader";
            this.generalHeader.Size = new System.Drawing.Size(269, 14);
            this.generalHeader.TabIndex = 4;
            this.generalHeader.TabStop = false;
            // 
            // LayerPropertiesDialog
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(274, 96);
            this.ControlBox = true;
            this.Controls.Add(this.generalHeader);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.nameBox);
            this.Controls.Add(this.visibleCheckBox);
            this.Controls.Add(this.nameLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LayerPropertiesDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Controls.SetChildIndex(this.nameLabel, 0);
            this.Controls.SetChildIndex(this.visibleCheckBox, 0);
            this.Controls.SetChildIndex(this.nameBox, 0);
            this.Controls.SetChildIndex(this.cancelButton, 0);
            this.Controls.SetChildIndex(this.okButton, 0);
            this.Controls.SetChildIndex(this.generalHeader, 0);
            this.ResumeLayout(false);

        }
        #endregion

        private void NameBox_Enter(object sender, System.EventArgs e)
        {
            this.nameBox.Select(0, nameBox.Text.Length);
        }

        private void OkButton_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.OK;

            using (new WaitCursorChanger(this))
            {
                this.layer.PushSuppressPropertyChanged();
                InitLayerFromDialog();
                object currentProperties = this.layer.SaveProperties();
                this.layer.LoadProperties(this.originalProperties);
                this.layer.PopSuppressPropertyChanged();

                this.layer.LoadProperties(currentProperties);
                this.originalProperties = layer.SaveProperties();
                //layer.Invalidate(); // no need to call Invalidate() -- it will be called by OnClosed()
            }
            
            Close();
        }

        private void CancelButton_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            using (new WaitCursorChanger(this))
            {
                this.layer.PushSuppressPropertyChanged();
                this.layer.LoadProperties(this.originalProperties);
                this.layer.PopSuppressPropertyChanged();
                this.layer.Invalidate();
            } 
            
            base.OnClosed(e);
        }

        private void VisibleCheckBox_CheckedChanged(object sender, System.EventArgs e)
        {
            Layer.PushSuppressPropertyChanged();
            Layer.Visible = visibleCheckBox.Checked;
            Layer.PopSuppressPropertyChanged();
        }
    }
}
