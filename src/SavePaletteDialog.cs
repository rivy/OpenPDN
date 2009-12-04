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
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PaintDotNet
{
    internal class SavePaletteDialog 
        : PdnBaseForm
    {
        private Label typeANameLabel;
        private TextBox textBox;
        private ListBox listBox;
        private Button saveButton;
        private Label palettesLabel;
        private Button cancelButton;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        public SavePaletteDialog()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            ValidatePaletteName();
            base.OnLoad(e);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.Icon != null)
                {
                    Icon icon = this.Icon;
                    this.Icon = null;
                    icon.Dispose();
                    icon = null;
                }

                if (this.components != null)
                {
                    this.components.Dispose();
                    this.components = null;
                }
            }

            base.Dispose(disposing);
        }

        public override void LoadResources()
        {
            this.Text = PdnResources.GetString("SavePaletteDialog.Text");
            this.Icon = Utility.ImageToIcon(PdnResources.GetImageResource("Icons.MenuFileSaveAsIcon.png").Reference);
            this.cancelButton.Text = PdnResources.GetString("Form.CancelButton.Text");
            this.saveButton.Text = PdnResources.GetString("Form.SaveButton.Text");
            this.typeANameLabel.Text = PdnResources.GetString("SavePaletteDialog.TypeANameLabel.Text");
            this.palettesLabel.Text = PdnResources.GetString("SavePaletteDialog.PalettesLabel.Text");
            base.LoadResources();
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.typeANameLabel = new System.Windows.Forms.Label();
            this.textBox = new System.Windows.Forms.TextBox();
            this.listBox = new System.Windows.Forms.ListBox();
            this.saveButton = new System.Windows.Forms.Button();
            this.palettesLabel = new System.Windows.Forms.Label();
            this.cancelButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // typeANameLabel
            // 
            this.typeANameLabel.AutoSize = true;
            this.typeANameLabel.Location = new System.Drawing.Point(5, 8);
            this.typeANameLabel.Margin = new System.Windows.Forms.Padding(0);
            this.typeANameLabel.Name = "typeANameLabel";
            this.typeANameLabel.Size = new System.Drawing.Size(50, 13);
            this.typeANameLabel.TabIndex = 0;
            this.typeANameLabel.Text = "infoLabel";
            // 
            // textBox
            // 
            this.textBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.textBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.textBox.Location = new System.Drawing.Point(8, 25);
            this.textBox.Name = "textBox";
            this.textBox.Size = new System.Drawing.Size(288, 20);
            this.textBox.TabIndex = 2;
            this.textBox.Validating += new System.ComponentModel.CancelEventHandler(this.TextBox_Validating);
            this.textBox.TextChanged += new System.EventHandler(this.TextBox_TextChanged);
            // 
            // palettesLabel
            // 
            this.palettesLabel.AutoSize = true;
            this.palettesLabel.Location = new System.Drawing.Point(5, 50);
            this.palettesLabel.Margin = new System.Windows.Forms.Padding(0);
            this.palettesLabel.Name = "palettesLabel";
            this.palettesLabel.Size = new System.Drawing.Size(35, 13);
            this.palettesLabel.TabIndex = 5;
            this.palettesLabel.Text = "label1";
            // 
            // listBox
            // 
            this.listBox.FormattingEnabled = true;
            this.listBox.Location = new System.Drawing.Point(8, 67);
            this.listBox.Name = "listBox";
            this.listBox.Size = new System.Drawing.Size(289, 108);
            this.listBox.Sorted = true;
            this.listBox.TabIndex = 3;
            this.listBox.SelectedIndexChanged += new System.EventHandler(this.ListBox_SelectedIndexChanged);
            // 
            // saveButton
            // 
            this.saveButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.saveButton.Location = new System.Drawing.Point(8, 185);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(75, 23);
            this.saveButton.TabIndex = 4;
            this.saveButton.Text = "button1";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.FlatStyle = FlatStyle.System;
            this.saveButton.Click += new System.EventHandler(this.SaveButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(89, 185);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 6;
            this.cancelButton.Text = "button1";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.FlatStyle = FlatStyle.System;
            this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // SavePaletteDialog
            // 
            this.AcceptButton = this.saveButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(310, 217);
            this.Controls.Add(this.palettesLabel);
            this.Controls.Add(this.listBox);
            this.Controls.Add(this.textBox);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.typeANameLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SavePaletteDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "SavePaletteDialog";
            this.Controls.SetChildIndex(this.typeANameLabel, 0);
            this.Controls.SetChildIndex(this.cancelButton, 0);
            this.Controls.SetChildIndex(this.saveButton, 0);
            this.Controls.SetChildIndex(this.textBox, 0);
            this.Controls.SetChildIndex(this.listBox, 0);
            this.Controls.SetChildIndex(this.palettesLabel, 0);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion    

        public string PaletteName
        {
            get
            {
                return this.textBox.Text;
            }

            set
            {
                this.textBox.Text = value;
            }
        }

        public string[] PaletteNames
        {
            set
            {
                this.listBox.Items.Clear();

                AutoCompleteStringCollection acsc = new AutoCompleteStringCollection();

                foreach (string paletteName in value)
                {
                    acsc.Add(paletteName);
                    this.listBox.Items.Add(paletteName);
                }

                this.textBox.AutoCompleteCustomSource = acsc;
                this.textBox.AutoCompleteSource = AutoCompleteSource.CustomSource;
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            Close();
        }

        private void ListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listBox.SelectedItem != null)
            {
                this.textBox.Text = this.listBox.SelectedItem.ToString();
                this.textBox.Focus();
                this.listBox.SelectedItem = null;
            }
        }

        private void ValidatePaletteName()
        {
            if (!PaletteCollection.ValidatePaletteName(this.textBox.Text))
            {
                this.saveButton.Enabled = false;

                if (!string.IsNullOrEmpty(this.textBox.Text))
                {
                    this.textBox.BackColor = Color.Red;
                }
            }
            else
            {
                this.saveButton.Enabled = true;
                this.textBox.BackColor = SystemColors.Window;
            }
        }

        private void TextBox_Validating(object sender, CancelEventArgs e)
        {
            ValidatePaletteName();
        }

        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            ValidatePaletteName();
        }
    }
}