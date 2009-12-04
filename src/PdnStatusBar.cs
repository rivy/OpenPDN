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
using System.Drawing;
using System.Windows.Forms;

namespace PaintDotNet
{
    internal sealed class PdnStatusBar
        : StatusStrip, 
          IStatusBarProgress
    {
        private System.Windows.Forms.ToolStripStatusLabel contextStatusLabel;
        private System.Windows.Forms.ToolStripSeparator progressStatusSeparator;
        private System.Windows.Forms.ToolStripProgressBar progressStatusBar;
        private System.Windows.Forms.ToolStripStatusLabel imageInfoStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel cursorInfoStatusLabel;

        private string progressTextFormat = PdnResources.GetString("StatusBar.Progress.Percentage.Format");
        private ImageResource contextStatusImage;

        public string ImageInfoStatusText
        {
            get
            {
                return this.imageInfoStatusLabel.Text;
            }

            set
            {
                this.imageInfoStatusLabel.Text = value;
                Update();
            }
        }

        public string ContextStatusText
        {
            get
            {
                return this.contextStatusLabel.Text;
            }

            set
            {
                this.contextStatusLabel.Text = value;
                Update();
            }
        }

        public ImageResource ContextStatusImage
        {
            get
            {
                return this.contextStatusImage;
            }

            set
            {
                this.contextStatusImage = value;

                if (this.contextStatusImage == null)
                {
                    this.contextStatusLabel.Image = null;
                }
                else
                {
                    this.contextStatusLabel.Image = this.contextStatusImage.Reference;
                }

                Update();
            }
        }

        public string CursorInfoText
        {
            get
            {
                return this.cursorInfoStatusLabel.Text;
            }

            set
            {
                this.cursorInfoStatusLabel.Text = value;
                Update();
            }
        }
        
        public void ResetProgressStatusBarAsync()
        {
            this.BeginInvoke(new Procedure(ResetProgressStatusBar));
        }

        public void EraseProgressStatusBar()
        {
            try
            {
                this.progressStatusSeparator.Visible = false;
                this.progressStatusBar.Visible = false;
                this.progressStatusBar.Value = 0;
            }

            catch (NullReferenceException)
            {
                // See bug #2212 -- appears to be a bug in the framework
            }
        }

        public void EraseProgressStatusBarAsync()
        {
            this.BeginInvoke(new Procedure(EraseProgressStatusBar));
        }

        public void ResetProgressStatusBar()
        {
            try
            {
                this.progressStatusBar.Value = 0;
                this.progressStatusSeparator.Visible = true;
                this.progressStatusBar.Visible = true;
            }

            catch (NullReferenceException nrex)
            {
                Tracing.Ping(nrex.ToString());
            }
        }

        public double GetProgressStatusBarValue()
        {
            lock (this.progressStatusBar)
            {
                return this.progressStatusBar.Value;
            }
        }

        public void SetProgressStatusBar(double percent)
        {
            lock (this.progressStatusBar)
            {
                this.progressStatusBar.Value = (int)percent;
                bool visible = (percent != 100);
                this.progressStatusBar.Visible = visible;
                this.progressStatusSeparator.Visible = visible;
            }
        }

        public PdnStatusBar()
        {
            InitializeComponent();

            this.cursorInfoStatusLabel.Image = PdnResources.GetImageResource("Icons.CursorXYIcon.png").Reference;
            this.cursorInfoStatusLabel.Text = string.Empty;

            // imageInfo (width,height info)
            this.imageInfoStatusLabel.Image = PdnResources.GetImageResource("Icons.ImageSizeIcon.png").Reference;

            // progress
            this.progressStatusBar.Visible = false;
            this.progressStatusSeparator.Visible = false;
            this.progressStatusBar.Height -= 4;
            this.progressStatusBar.ProgressBar.Style = ProgressBarStyle.Continuous;
        }

        private void InitializeComponent()
        {
            this.contextStatusLabel = new ToolStripStatusLabel();
            this.progressStatusSeparator = new ToolStripSeparator();
            this.progressStatusBar = new ToolStripProgressBar();
            this.imageInfoStatusLabel = new ToolStripStatusLabel();
            this.cursorInfoStatusLabel = new ToolStripStatusLabel();
            SuspendLayout();
            //
            // contextStatusLabel
            //
            this.contextStatusLabel.Name = "contextStatusLabel";
            this.contextStatusLabel.Width = UI.ScaleWidth(436);
            this.contextStatusLabel.Spring = true;
            this.contextStatusLabel.TextAlign = ContentAlignment.MiddleLeft;
            this.contextStatusLabel.ImageAlign = ContentAlignment.MiddleLeft;
            //
            // progressStatusBar
            //
            this.progressStatusBar.Name = "progressStatusBar";
            this.progressStatusBar.Width = 130;
            this.progressStatusBar.AutoSize = false;
            //
            // imageInfoStatusLabel
            //
            this.imageInfoStatusLabel.Name = "imageInfoStatusLabel";
            this.imageInfoStatusLabel.Width = UI.ScaleWidth(130);
            this.imageInfoStatusLabel.TextAlign = ContentAlignment.MiddleLeft;
            this.imageInfoStatusLabel.ImageAlign = ContentAlignment.MiddleLeft;
            this.imageInfoStatusLabel.AutoSize = false;
            //
            // cursorInfoStatusLabel
            //
            this.cursorInfoStatusLabel.Name = "cursorInfoStatusLabel";
            this.cursorInfoStatusLabel.Width = UI.ScaleWidth(130);
            this.cursorInfoStatusLabel.TextAlign = ContentAlignment.MiddleLeft;
            this.cursorInfoStatusLabel.ImageAlign = ContentAlignment.MiddleLeft;
            this.cursorInfoStatusLabel.AutoSize = false;
            //
            // PdnStatusBar
            //
            this.Name = "PdnStatusBar";
            this.Items.Add(this.contextStatusLabel);
            this.Items.Add(this.progressStatusSeparator);
            this.Items.Add(this.progressStatusBar);
            this.Items.Add(new ToolStripSeparator());
            this.Items.Add(this.imageInfoStatusLabel);
            this.Items.Add(new ToolStripSeparator());
            this.Items.Add(this.cursorInfoStatusLabel);
            ResumeLayout(false);
        }
    }
}
