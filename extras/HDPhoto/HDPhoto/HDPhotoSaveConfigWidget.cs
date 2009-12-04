using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace PaintDotNet.Data
{
    public sealed class HDPhotoSaveConfigWidget
        : SaveConfigWidget
    {
        private System.Windows.Forms.TrackBar qualitySlider;
        private System.Windows.Forms.Label qualityLabel;
        private System.Windows.Forms.NumericUpDown qualityUpDown;
        private Panel panel1;
        private RadioButton bpp32RB;
        private RadioButton bpp24RB;
        private HeaderLabel bppHeader;
        private System.ComponentModel.IContainer components = null;

        public HDPhotoSaveConfigWidget()
        {
            // This call is required by the Windows Form Designer.
            InitializeComponent();

            this.qualityLabel.Text = Strings.HDPhotoSaveConfigWidget_QualitySlider_Text;
            this.bppHeader.Text = Strings.HDPhotoSaveConfigWidget_BppHeader_Text;
        }

        protected override void InitFileType()
        {
            this.fileType = new HDPhotoFileType();
        }

        protected override void InitTokenFromWidget()
        {
            ((HDPhotoSaveConfigToken)this.Token).Quality = this.qualitySlider.Value;
            ((HDPhotoSaveConfigToken)this.Token).BitDepth = (this.bpp24RB.Checked ? 24 : 32);
        }

        protected override void InitWidgetFromToken(SaveConfigToken token)
        {
            HDPhotoSaveConfigToken hdToken = (HDPhotoSaveConfigToken)token;
            this.qualitySlider.Value = hdToken.Quality;
            this.bpp24RB.Checked = (hdToken.BitDepth == 24);
            this.bpp32RB.Checked = (hdToken.BitDepth == 32);
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
            this.qualitySlider = new System.Windows.Forms.TrackBar();
            this.qualityLabel = new System.Windows.Forms.Label();
            this.qualityUpDown = new System.Windows.Forms.NumericUpDown();
            this.panel1 = new System.Windows.Forms.Panel();
            this.bpp32RB = new System.Windows.Forms.RadioButton();
            this.bpp24RB = new System.Windows.Forms.RadioButton();
            this.bppHeader = new PaintDotNet.HeaderLabel();
            ((System.ComponentModel.ISupportInitialize)(this.qualitySlider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.qualityUpDown)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // qualitySlider
            // 
            this.qualitySlider.Location = new System.Drawing.Point(0, 24);
            this.qualitySlider.Maximum = 100;
            this.qualitySlider.Minimum = 1;
            this.qualitySlider.Name = "qualitySlider";
            this.qualitySlider.Size = new System.Drawing.Size(180, 45);
            this.qualitySlider.TabIndex = 1;
            this.qualitySlider.TickFrequency = 10;
            this.qualitySlider.Value = 1;
            this.qualitySlider.ValueChanged += new System.EventHandler(this.QualitySlider_ValueChanged);
            // 
            // qualityLabel
            // 
            this.qualityLabel.AutoSize = true;
            this.qualityLabel.Location = new System.Drawing.Point(4, 3);
            this.qualityLabel.Name = "qualityLabel";
            this.qualityLabel.Size = new System.Drawing.Size(39, 13);
            this.qualityLabel.TabIndex = 1;
            this.qualityLabel.Text = "Quality";
            // 
            // qualityUpDown
            // 
            this.qualityUpDown.Location = new System.Drawing.Point(115, 0);
            this.qualityUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.qualityUpDown.Name = "qualityUpDown";
            this.qualityUpDown.Size = new System.Drawing.Size(56, 20);
            this.qualityUpDown.TabIndex = 0;
            this.qualityUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.qualityUpDown.Enter += new System.EventHandler(this.QualityUpDown_Enter);
            this.qualityUpDown.ValueChanged += new System.EventHandler(this.QualityUpDown_ValueChanged);
            this.qualityUpDown.Leave += new System.EventHandler(this.QualityUpDown_Leave);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.bpp32RB);
            this.panel1.Controls.Add(this.bpp24RB);
            this.panel1.Location = new System.Drawing.Point(0, 87);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(180, 59);
            this.panel1.TabIndex = 2;
            // 
            // bpp32RB
            // 
            this.bpp32RB.AutoSize = true;
            this.bpp32RB.Location = new System.Drawing.Point(10, 28);
            this.bpp32RB.Name = "bpp32RB";
            this.bpp32RB.Size = new System.Drawing.Size(51, 17);
            this.bpp32RB.TabIndex = 1;
            this.bpp32RB.TabStop = true;
            this.bpp32RB.Text = "32-bit";
            this.bpp32RB.UseVisualStyleBackColor = true;
            this.bpp32RB.CheckedChanged += new System.EventHandler(this.bpp32RB_CheckedChanged);
            // 
            // bpp24RB
            // 
            this.bpp24RB.AutoSize = true;
            this.bpp24RB.Location = new System.Drawing.Point(10, 4);
            this.bpp24RB.Name = "bpp24RB";
            this.bpp24RB.Size = new System.Drawing.Size(51, 17);
            this.bpp24RB.TabIndex = 0;
            this.bpp24RB.TabStop = true;
            this.bpp24RB.Text = "24-bit";
            this.bpp24RB.UseVisualStyleBackColor = true;
            this.bpp24RB.CheckedChanged += new System.EventHandler(this.bpp24RB_CheckedChanged);
            // 
            // bppHeader
            // 
            this.bppHeader.Location = new System.Drawing.Point(4, 67);
            this.bppHeader.Name = "bppHeader";
            this.bppHeader.RightMargin = 0;
            this.bppHeader.Size = new System.Drawing.Size(173, 14);
            this.bppHeader.TabIndex = 3;
            this.bppHeader.TabStop = false;
            this.bppHeader.Text = "Bit-depth";
            // 
            // HDPhotoSaveConfigWidget
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.bppHeader);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.qualityUpDown);
            this.Controls.Add(this.qualityLabel);
            this.Controls.Add(this.qualitySlider);
            this.Name = "HDPhotoSaveConfigWidget";
            this.Size = new System.Drawing.Size(180, 148);
            ((System.ComponentModel.ISupportInitialize)(this.qualitySlider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.qualityUpDown)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private void QualitySlider_ValueChanged(object sender, System.EventArgs e)
        {
            if (this.qualityUpDown.Value != (decimal)this.qualitySlider.Value)
            {
                this.qualityUpDown.Value = (decimal)this.qualitySlider.Value;
            }

            UpdateToken();
        }

        private void QualityUpDown_ValueChanged(object sender, System.EventArgs e)
        {
            if (this.qualitySlider.Value != (int)this.qualityUpDown.Value)
            {
                this.qualitySlider.Value = (int)this.qualityUpDown.Value;
            }
        }

        private void QualityUpDown_Leave(object sender, System.EventArgs e)
        {
            QualityUpDown_ValueChanged(sender, e);
        }

        private void QualityUpDown_Enter(object sender, System.EventArgs e)
        {
            qualityUpDown.Select(0, qualityUpDown.Text.Length);
        }

        private void bpp24RB_CheckedChanged(object sender, EventArgs e)
        {
            UpdateToken();
        }

        private void bpp32RB_CheckedChanged(object sender, EventArgs e)
        {
            UpdateToken();
        }
    }
}

