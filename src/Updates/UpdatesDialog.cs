/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet;
using PaintDotNet.SystemLayer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PaintDotNet.Updates
{
    internal class UpdatesDialog 
        : PdnBaseForm
    {
        private Button closeButton;
        private Button optionsButton;
        private Button continueButton;
        private ProgressBar progressBar;
        private Label infoText;
        private LinkLabel moreInfoLink;
        private Uri moreInfoTarget;
        private Label versionNameLabel;
        private HeaderLabel headerLabel;
        private bool closeOnDoneState = false;
        private Label newVersionLabel;
        private Label progressLabel;

        private StateMachineExecutor updatesStateMachine;
        public StateMachineExecutor UpdatesStateMachine
        {
            get
            {
                return this.updatesStateMachine;
            }

            set
            {
                if (this.updatesStateMachine != null)
                {
                    this.updatesStateMachine.StateBegin -= UpdatesStateMachine_StateBegin;
                    this.updatesStateMachine.StateMachineBegin -= UpdatesStateMachine_StateMachineBegin;
                    this.updatesStateMachine.StateMachineFinished -= UpdatesStateMachine_StateMachineFinished;
                    this.updatesStateMachine.StateProgress -= UpdatesStateMachine_StateProgress;
                    this.updatesStateMachine.StateWaitingForInput -= UpdatesStateMachine_StateWaitingForInput;
                }

                this.updatesStateMachine = value;

                if (this.updatesStateMachine != null)
                {
                    this.updatesStateMachine.StateBegin += UpdatesStateMachine_StateBegin;
                    this.updatesStateMachine.StateMachineBegin += UpdatesStateMachine_StateMachineBegin;
                    this.updatesStateMachine.StateMachineFinished += UpdatesStateMachine_StateMachineFinished;
                    this.updatesStateMachine.StateProgress += UpdatesStateMachine_StateProgress;
                    this.updatesStateMachine.StateWaitingForInput += UpdatesStateMachine_StateWaitingForInput;
                }

                UpdateDynamicUI();
            }
        }

        private void UpdatesStateMachine_StateWaitingForInput(object sender, EventArgs<State> e)
        {
            this.continueButton.Enabled = true;
            UpdateDynamicUI();
        }

        private void UpdatesStateMachine_StateProgress(object sender, ProgressEventArgs e)
        {
            int newValue = Utility.Clamp((int)e.Percent, this.progressBar.Minimum, this.progressBar.Maximum);
            this.progressBar.Value = newValue;
            string progressLabelFormat = PdnResources.GetString("UpdatesDialog.ProgressLabel.Text.Format");
            string progressLabelText = string.Format(progressLabelFormat, newValue.ToString());
            this.progressLabel.Text = progressLabelText;

            UpdateDynamicUI();
        }

        private void UpdatesStateMachine_StateMachineFinished(object sender, EventArgs e)
        {
            UpdateDynamicUI();
        }

        private void UpdatesStateMachine_StateMachineBegin(object sender, EventArgs e)
        {
            UpdateDynamicUI();
        }

        private void UpdatesStateMachine_StateBegin(object sender, EventArgs<State> e)
        {
            this.progressBar.Value = 0;
            UpdateDynamicUI();

            if (e.Data is Updates.DoneState && this.closeOnDoneState)
            {
                this.DialogResult = DialogResult.OK;
                Close();
            }
            else if (e.Data is Updates.ReadyToCheckState)
            {
                this.updatesStateMachine.ProcessInput(UpdatesAction.Continue);
            }
            else if (e.Data is Updates.ReadyToInstallState)
            {
                ClientSize = new Size(ClientSize.Width, this.continueButton.Bottom + UI.ScaleHeight(7));

                this.closeButton.Enabled = false;
                this.closeButton.Visible = false;
                this.optionsButton.Enabled = false;
                this.optionsButton.Visible = false;
                this.headerLabel.Visible = false;

                this.continueButton.Location = new Point(
                    ClientSize.Width - this.continueButton.Width - UI.ScaleWidth(7), 
                    ClientSize.Height - this.continueButton.Height - UI.ScaleHeight(8));
            }
            else if (e.Data is Updates.AbortedState)
            {
                this.DialogResult = DialogResult.Abort;
                Close();
            }
        }

        private void UpdateDynamicUI()
        {
            this.Text = PdnResources.GetString("UpdatesDialog.Text");
            string closeButtonText = PdnResources.GetString("UpdatesDialog.CloseButton.Text");
            this.optionsButton.Text = PdnResources.GetString("UpdatesDialog.OptionsButton.Text");
            this.moreInfoLink.Text = PdnResources.GetString("UpdatesDialog.MoreInfoLink.Text");
            this.newVersionLabel.Text = PdnResources.GetString("UpdatesDialog.NewVersionLabel.Text");

            if (this.updatesStateMachine == null || this.updatesStateMachine.CurrentState == null)
            {
                this.infoText.Text = string.Empty;
                this.continueButton.Text = string.Empty;
                this.continueButton.Enabled = false;
                this.continueButton.Visible = false;
                this.moreInfoLink.Visible = false;
                this.moreInfoLink.Enabled = false;
                this.versionNameLabel.Visible = false;
                this.versionNameLabel.Enabled = false;
            }
            else
            {
                UpdatesState currentState = (UpdatesState)this.updatesStateMachine.CurrentState;

                this.infoText.Text = currentState.InfoText;
                this.continueButton.Text = currentState.ContinueButtonText;
                this.continueButton.Visible = currentState.ContinueButtonVisible;
                this.continueButton.Enabled = currentState.ContinueButtonVisible;
                this.progressBar.Style = (currentState.MarqueeStyle == MarqueeStyle.Marquee) ? ProgressBarStyle.Marquee : ProgressBarStyle.Continuous;
                this.progressBar.Visible = (currentState.MarqueeStyle != MarqueeStyle.None);
                this.progressLabel.Visible = this.progressBar.Visible;

                if (this.continueButton.Enabled || currentState is ErrorState || currentState is DoneState)
                {
                    closeButtonText = PdnResources.GetString("UpdatesDialog.CloseButton.Text");
                }
                else
                {
                    closeButtonText = PdnResources.GetString("Form.CancelButton.Text");
                }

                if (currentState is ErrorState)
                {
                    Size size = new Size(this.infoText.Width, 1);
                    Size preferredSize = this.infoText.GetPreferredSize(size);
                    this.infoText.Size = preferredSize;
                }

                INewVersionInfo asInvi = currentState as INewVersionInfo;

                if (asInvi != null)
                {
                    this.versionNameLabel.Text = asInvi.NewVersionInfo.FriendlyName;
                    this.versionNameLabel.Visible = true;
                    this.versionNameLabel.Enabled = true;
                    this.moreInfoTarget = new Uri(asInvi.NewVersionInfo.InfoUrl);
                    this.moreInfoLink.Visible = true;
                    this.moreInfoLink.Enabled = true;

                    this.newVersionLabel.Visible = true;
                    this.newVersionLabel.Font = new Font(this.newVersionLabel.Font, this.newVersionLabel.Font.Style | FontStyle.Bold);
                    this.versionNameLabel.Left = this.newVersionLabel.Right;
                    this.moreInfoLink.Left = this.versionNameLabel.Left;
                }
                else
                {
                    this.newVersionLabel.Visible = false;
                    this.versionNameLabel.Visible = false;
                    this.versionNameLabel.Enabled = false;
                    this.moreInfoLink.Visible = false;
                    this.moreInfoLink.Enabled = false;
                }
            }

            this.closeButton.Text = closeButtonText;

            Update();
        }

        public UpdatesDialog()
        {
            InitializeComponent();

            Image iconImage = PdnResources.GetImageResource("Icons.MenuHelpCheckForUpdatesIcon.png").Reference;
            this.Icon = Utility.ImageToIcon(iconImage, Utility.TransparentKey);

            if (Security.IsAdministrator)
            {
                this.optionsButton.Enabled = true;
            }
            else if (Security.CanElevateToAdministrator)
            {
                this.optionsButton.Enabled = true;
            }
            else
            {
                this.optionsButton.Enabled = false;
            }

            this.optionsButton.FlatStyle = FlatStyle.System;
            SystemLayer.UI.EnableShield(this.optionsButton, true);
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
            this.closeButton = new System.Windows.Forms.Button();
            this.optionsButton = new System.Windows.Forms.Button();
            this.continueButton = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.infoText = new System.Windows.Forms.Label();
            this.moreInfoLink = new System.Windows.Forms.LinkLabel();
            this.versionNameLabel = new System.Windows.Forms.Label();
            this.headerLabel = new PaintDotNet.HeaderLabel();
            this.newVersionLabel = new System.Windows.Forms.Label();
            this.progressLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // closeButton
            // 
            this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.closeButton.AutoSize = true;
            this.closeButton.Location = new System.Drawing.Point(262, 143);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(75, 23);
            this.closeButton.TabIndex = 0;
            this.closeButton.Text = "_close";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.FlatStyle = FlatStyle.System;
            this.closeButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // optionsButton
            // 
            this.optionsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.optionsButton.AutoSize = true;
            this.optionsButton.Click += new EventHandler(OptionsButton_Click);
            this.optionsButton.Location = new System.Drawing.Point(166, 143);
            this.optionsButton.Name = "optionsButton";
            this.optionsButton.Size = new System.Drawing.Size(91, 23);
            this.optionsButton.TabIndex = 1;
            this.optionsButton.Text = "_options...";
            this.optionsButton.FlatStyle = FlatStyle.System;
            this.optionsButton.UseVisualStyleBackColor = true;
            // 
            // continueButton
            // 
            this.continueButton.AutoSize = true;
            this.continueButton.Location = new System.Drawing.Point(7, 100);
            this.continueButton.Name = "continueButton";
            this.continueButton.Size = new System.Drawing.Size(75, 23);
            this.continueButton.TabIndex = 3;
            this.continueButton.Text = "_continue";
            this.continueButton.UseVisualStyleBackColor = true;
            this.continueButton.FlatStyle = FlatStyle.System;
            this.continueButton.Click += new System.EventHandler(this.ContinueButton_Click);
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(9, 103);
            this.progressBar.MarqueeAnimationSpeed = 40;
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(294, 18);
            this.progressBar.TabIndex = 4;
            // 
            // infoText
            // 
            this.infoText.Location = new System.Drawing.Point(7, 7);
            this.infoText.Name = "infoText";
            this.infoText.Size = new System.Drawing.Size(329, 45);
            this.infoText.TabIndex = 2;
            this.infoText.Text = ".blahblahblah";
            // 
            // moreInfoLink
            // 
            this.moreInfoLink.AutoSize = true;
            this.moreInfoLink.Location = new System.Drawing.Point(7, 75);
            this.moreInfoLink.Name = "moreInfoLink";
            this.moreInfoLink.Size = new System.Drawing.Size(66, 13);
            this.moreInfoLink.TabIndex = 5;
            this.moreInfoLink.TabStop = true;
            this.moreInfoLink.Text = "_more Info...";
            this.moreInfoLink.Click += new System.EventHandler(this.MoreInfoLink_Click);
            // 
            // versionNameLabel
            // 
            this.versionNameLabel.AutoSize = true;
            this.versionNameLabel.Location = new System.Drawing.Point(88, 57);
            this.versionNameLabel.Name = "versionNameLabel";
            this.versionNameLabel.Size = new System.Drawing.Size(84, 13);
            this.versionNameLabel.TabIndex = 6;
            this.versionNameLabel.Text = ".paint.net vX.YZ";
            // 
            // headerLabel
            // 
            this.headerLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.headerLabel.Location = new System.Drawing.Point(9, 126);
            this.headerLabel.Name = "headerLabel";
            this.headerLabel.RightMargin = 0;
            this.headerLabel.Size = new System.Drawing.Size(327, 15);
            this.headerLabel.TabIndex = 0;
            this.headerLabel.TabStop = false;
            // 
            // newVersionLabel
            // 
            this.newVersionLabel.AutoSize = true;
            this.newVersionLabel.Location = new System.Drawing.Point(7, 57);
            this.newVersionLabel.Name = "newVersionLabel";
            this.newVersionLabel.Size = new System.Drawing.Size(70, 13);
            this.newVersionLabel.TabIndex = 7;
            this.newVersionLabel.Text = ".new version:";
            // 
            // progressLabel
            // 
            this.progressLabel.AutoSize = true;
            this.progressLabel.Location = new System.Drawing.Point(310, 105);
            this.progressLabel.Name = "progressLabel";
            this.progressLabel.Size = new System.Drawing.Size(0, 13);
            this.progressLabel.TabIndex = 8;
            this.progressLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // UpdatesDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.closeButton;
            this.ClientSize = new System.Drawing.Size(343, 172);
            this.Controls.Add(this.progressLabel);
            this.Controls.Add(this.newVersionLabel);
            this.Controls.Add(this.headerLabel);
            this.Controls.Add(this.versionNameLabel);
            this.Controls.Add(this.moreInfoLink);
            this.Controls.Add(this.continueButton);
            this.Controls.Add(this.infoText);
            this.Controls.Add(this.optionsButton);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.progressBar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UpdatesDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private void OptionsButton_Click(object sender, EventArgs e)
        {
            UpdatesOptionsDialog.ShowUpdateOptionsDialog(this, true);
        }

        private void MoreInfoLink_Click(object sender, EventArgs e)
        {
            PdnInfo.OpenUrl(this, this.moreInfoTarget.ToString());
        }

        private void ContinueButton_Click(object sender, EventArgs e)
        {
            if (this.updatesStateMachine.CurrentState is Updates.ReadyToInstallState)
            {
                // whomever showed the dialog is responsible for "continuing" the state machine at this point
                // in order to properly handle closing the application. We will not actually pass it the
                // UpdatesAction.Continue input.
                this.DialogResult = DialogResult.Yes;
                Hide();
                Close();
            }
            else
            {
                this.updatesStateMachine.ProcessInput(Updates.UpdatesAction.Continue);
                this.continueButton.Enabled = false;
            }
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            if (this.updatesStateMachine != null)
            {
                this.updatesStateMachine.Abort();
                this.updatesStateMachine = null;
                this.closeButton.Enabled = false;
            }

            Close();
        }
    }
}
