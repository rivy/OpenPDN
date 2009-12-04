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

namespace PaintDotNet.Updates
{
    internal class UpdatesOptionsDialog 
        : PdnBaseForm
    {
        public const string CommandLineParameter = "/updateOptions";

        private Button saveButton;
        private CheckBox autoCheckBox;
        private CheckBox betaCheckBox;
        private Button cancelButton;
        private HeaderLabel headerLabel1;
        private Label allUsersNoticeLabel;

        public static void ShowUpdateOptionsDialog(IWin32Window owner)
        {
            ShowUpdateOptionsDialog(owner, false);
        }

        public static void ShowUpdateOptionsDialog(IWin32Window owner, bool allowNewInstance)
        {
            if (Security.IsAdministrator)
            {
                UpdatesOptionsDialog dialog = new UpdatesOptionsDialog();

                if (owner == null)
                {
                    dialog.ShowInTaskbar = true;
                }

                dialog.ShowDialog(owner);
            }
            else if (Security.CanElevateToAdministrator && allowNewInstance)
            {
                Startup.StartNewInstance(owner, true, new string[1] { CommandLineParameter });
            }
            else
            {
                Utility.ShowNonAdminErrorBox(owner);
            }
        }

        private UpdatesOptionsDialog()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            LoadSettings();
            LoadResources();
            base.OnLoad(e);
        }

        public override void LoadResources()
        {
            this.Text = PdnResources.GetString("UpdatesOptionsDialog.Text");

            Image iconImage = PdnResources.GetImageResource("Icons.SettingsIcon.png").Reference;
            this.Icon = Utility.ImageToIcon(iconImage, Utility.TransparentKey, false);

            this.saveButton.Text = PdnResources.GetString("UpdatesOptionsDialog.SaveButton.Text");
            this.autoCheckBox.Text = PdnResources.GetString("UpdatesOptionsDialog.AutoCheckBox.Text");
            this.betaCheckBox.Text = PdnResources.GetString("UpdatesOptionsDialog.BetaCheckBox.Text");
            this.allUsersNoticeLabel.Text = PdnResources.GetString("UpdatesOptionsDialog.AllUsersNoticeLabel.Text");
            this.cancelButton.Text = PdnResources.GetString("Form.CancelButton.Text");

            base.LoadResources();
        }

        private void LoadSettings()
        {
            string autoCheckString = Settings.SystemWide.GetString(SettingNames.AutoCheckForUpdates, "0");
            bool autoCheck = (autoCheckString == "1");
            this.autoCheckBox.Checked = autoCheck;

            string betaCheckString = Settings.SystemWide.GetString(SettingNames.AlsoCheckForBetas, "0");
            bool betaCheck = (betaCheckString == "1");
            this.betaCheckBox.Checked = betaCheck;
            this.betaCheckBox.Enabled = this.autoCheckBox.Checked;
        }

        private void SaveSettings()
        {
            string autoCheckString = autoCheckBox.Checked ? "1" : "0";
            Settings.SystemWide.SetString(SettingNames.AutoCheckForUpdates, autoCheckString);

            string betaCheckString = betaCheckBox.Checked ? "1" : "0";
            Settings.SystemWide.SetString(SettingNames.AlsoCheckForBetas, betaCheckString);
        }

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            this.saveButton = new System.Windows.Forms.Button();
            this.autoCheckBox = new System.Windows.Forms.CheckBox();
            this.betaCheckBox = new System.Windows.Forms.CheckBox();
            this.allUsersNoticeLabel = new System.Windows.Forms.Label();
            this.cancelButton = new System.Windows.Forms.Button();
            this.headerLabel1 = new PaintDotNet.HeaderLabel();
            this.SuspendLayout();
            // 
            // saveButton
            // 
            this.saveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.saveButton.Location = new System.Drawing.Point(236, 95);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(75, 23);
            this.saveButton.TabIndex = 0;
            this.saveButton.Text = ".save";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.FlatStyle = FlatStyle.System;
            this.saveButton.Click += new System.EventHandler(this.SaveButton_Click);
            // 
            // autoCheckBox
            // 
            this.autoCheckBox.AutoSize = true;
            this.autoCheckBox.Location = new System.Drawing.Point(8, 9);
            this.autoCheckBox.Name = "autoCheckBox";
            this.autoCheckBox.Size = new System.Drawing.Size(80, 17);
            this.autoCheckBox.TabIndex = 1;
            this.autoCheckBox.Text = "checkBox1";
            this.autoCheckBox.UseVisualStyleBackColor = true;
            this.autoCheckBox.FlatStyle = FlatStyle.System;
            this.autoCheckBox.CheckedChanged += new System.EventHandler(this.AutoCheckBox_CheckedChanged);
            // 
            // betaCheckBox
            // 
            this.betaCheckBox.AutoSize = true;
            this.betaCheckBox.Location = new System.Drawing.Point(26, 33);
            this.betaCheckBox.Name = "betaCheckBox";
            this.betaCheckBox.Size = new System.Drawing.Size(80, 17);
            this.betaCheckBox.TabIndex = 2;
            this.betaCheckBox.Text = "checkBox1";
            this.betaCheckBox.FlatStyle = FlatStyle.System;
            this.betaCheckBox.UseVisualStyleBackColor = true;
            // 
            // allUsersNoticeLabel
            // 
            this.allUsersNoticeLabel.AutoSize = true;
            this.allUsersNoticeLabel.Location = new System.Drawing.Point(7, 63);
            this.allUsersNoticeLabel.Name = "allUsersNoticeLabel";
            this.allUsersNoticeLabel.Size = new System.Drawing.Size(78, 13);
            this.allUsersNoticeLabel.TabIndex = 4;
            this.allUsersNoticeLabel.Text = ".allUsersNotice";
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(316, 95);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 5;
            this.cancelButton.Text = ".cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.FlatStyle = FlatStyle.System;
            this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // headerLabel1
            // 
            this.headerLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.headerLabel1.Location = new System.Drawing.Point(7, 80);
            this.headerLabel1.Name = "headerLabel1";
            this.headerLabel1.RightMargin = 0;
            this.headerLabel1.Size = new System.Drawing.Size(384, 14);
            this.headerLabel1.TabIndex = 6;
            this.headerLabel1.TabStop = false;
            // 
            // UpdatesOptionsDialog
            // 
            this.AcceptButton = this.saveButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(398, 125);
            this.Controls.Add(this.headerLabel1);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.betaCheckBox);
            this.Controls.Add(this.autoCheckBox);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.allUsersNoticeLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UpdatesOptionsDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "UpdatesOptionsDialog";
            this.Controls.SetChildIndex(this.allUsersNoticeLabel, 0);
            this.Controls.SetChildIndex(this.saveButton, 0);
            this.Controls.SetChildIndex(this.autoCheckBox, 0);
            this.Controls.SetChildIndex(this.betaCheckBox, 0);
            this.Controls.SetChildIndex(this.cancelButton, 0);
            this.Controls.SetChildIndex(this.headerLabel1, 0);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private void AutoCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            this.betaCheckBox.Enabled = this.autoCheckBox.Checked;
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            SaveSettings();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}