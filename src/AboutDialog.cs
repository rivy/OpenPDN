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
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace PaintDotNet
{
    internal class AboutDialog 
        : PdnBaseForm
    {
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Label creditsLabel;
        private System.Windows.Forms.RichTextBox richCreditsBox;
        private System.Windows.Forms.TextBox copyrightLabel;
        private Label versionLabel;
        private PdnBanner pdnBanner;

        public AboutDialog()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            this.richCreditsBox.BackColor = SystemColors.Window;

            string textFormat = PdnResources.GetString("AboutDialog.Text.Format");
            this.Text = string.Format(textFormat, PdnInfo.GetBareProductName());

            this.pdnBanner.BannerText = string.Empty;// PdnInfo.GetFriendlyVersionString();
            this.richCreditsBox.LoadFile(PdnResources.GetResourceStream("Files.AboutCredits.rtf"), RichTextBoxStreamType.RichText);
            this.copyrightLabel.Text = PdnInfo.GetCopyrightString();

            this.Icon = PdnInfo.AppIcon;

            this.okButton.Text = PdnResources.GetString("Form.OkButton.Text");
            this.okButton.Location = new Point((this.ClientSize.Width - this.okButton.Width) / 2, this.okButton.Top);

            this.creditsLabel.Text = PdnResources.GetString("AboutDialog.CreditsLabel.Text");

            Font bannerFont = this.pdnBanner.BannerFont;
            Font newBannerFont = Utility.CreateFont(bannerFont.Name, 8.0f, bannerFont.Style);
            this.pdnBanner.BannerFont = newBannerFont;
            newBannerFont.Dispose();
            bannerFont.Dispose();

            this.versionLabel.Text = PdnInfo.GetFullAppName();
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.okButton = new System.Windows.Forms.Button();
            this.creditsLabel = new System.Windows.Forms.Label();
            this.richCreditsBox = new System.Windows.Forms.RichTextBox();
            this.copyrightLabel = new System.Windows.Forms.TextBox();
            this.pdnBanner = new PaintDotNet.PdnBanner();
            this.versionLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // okButton
            // 
            this.okButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.okButton.Location = new System.Drawing.Point(139, 346);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 0;
            // 
            // creditsLabel
            // 
            this.creditsLabel.Location = new System.Drawing.Point(7, 132);
            this.creditsLabel.Name = "creditsLabel";
            this.creditsLabel.Size = new System.Drawing.Size(200, 16);
            this.creditsLabel.TabIndex = 5;
            // 
            // richCreditsBox
            // 
            this.richCreditsBox.CausesValidation = false;
            this.richCreditsBox.Location = new System.Drawing.Point(10, 153);
            this.richCreditsBox.Name = "richCreditsBox";
            this.richCreditsBox.ReadOnly = true;
            this.richCreditsBox.Size = new System.Drawing.Size(476, 187);
            this.richCreditsBox.TabIndex = 6;
            this.richCreditsBox.Text = "";
            this.richCreditsBox.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.RichCreditsBox_LinkClicked);
            // 
            // copyrightLabel
            // 
            this.copyrightLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.copyrightLabel.Location = new System.Drawing.Point(10, 95);
            this.copyrightLabel.Multiline = true;
            this.copyrightLabel.Name = "copyrightLabel";
            this.copyrightLabel.ReadOnly = true;
            this.copyrightLabel.Size = new System.Drawing.Size(481, 36);
            this.copyrightLabel.TabIndex = 4;
            // 
            // pdnBanner
            // 
            this.pdnBanner.BannerFont = new System.Drawing.Font("Tahoma", 10F);
            this.pdnBanner.BannerText = "headingText";
            this.pdnBanner.Location = new System.Drawing.Point(0, 0);
            this.pdnBanner.Name = "pdnBanner";
            this.pdnBanner.Size = new System.Drawing.Size(495, 71);
            this.pdnBanner.TabIndex = 7;
            // 
            // versionLabel
            // 
            this.versionLabel.AutoSize = true;
            this.versionLabel.Location = new System.Drawing.Point(7, 77);
            this.versionLabel.Name = "versionLabel";
            this.versionLabel.Size = new System.Drawing.Size(0, 13);
            this.versionLabel.TabIndex = 8;
            // 
            // AboutDialog
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.okButton;
            this.ClientSize = new System.Drawing.Size(495, 375);
            this.Controls.Add(this.versionLabel);
            this.Controls.Add(this.copyrightLabel);
            this.Controls.Add(this.richCreditsBox);
            this.Controls.Add(this.creditsLabel);
            this.Controls.Add(this.pdnBanner);
            this.Controls.Add(this.okButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Location = new System.Drawing.Point(0, 0);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutDialog";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Controls.SetChildIndex(this.okButton, 0);
            this.Controls.SetChildIndex(this.pdnBanner, 0);
            this.Controls.SetChildIndex(this.creditsLabel, 0);
            this.Controls.SetChildIndex(this.richCreditsBox, 0);
            this.Controls.SetChildIndex(this.copyrightLabel, 0);
            this.Controls.SetChildIndex(this.versionLabel, 0);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private void RichCreditsBox_LinkClicked(object sender, System.Windows.Forms.LinkClickedEventArgs e)
        {
            if (null != e.LinkText && e.LinkText.StartsWith("http://"))
            {
                PdnInfo.OpenUrl(this, e.LinkText);
            }
        }
    }
}
