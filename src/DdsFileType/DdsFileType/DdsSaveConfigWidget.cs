//------------------------------------------------------------------------------
/*
    @brief		DDS File Type Plugin for Paint.NET

    @note		Copyright (c) 2007 Dean Ashton         http://www.dmashton.co.uk

    Permission is hereby granted, free of charge, to any person obtaining
    a copy of this software and associated documentation files (the 
    "Software"), to	deal in the Software without restriction, including
    without limitation the rights to use, copy, modify, merge, publish,
    distribute, sublicense, and/or sell copies of the Software, and to 
    permit persons to whom the Software is furnished to do so, subject to 
    the following conditions:

    The above copyright notice and this permission notice shall be included
    in all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
    OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
    MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
    IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY 
    CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, 
    TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
    SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
**/
//------------------------------------------------------------------------------

using PaintDotNet;
using PaintDotNet.SystemLayer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace DdsFileTypePlugin
{
    class DdsSaveConfigWidget : PaintDotNet.SaveConfigWidget
    {
        private System.Windows.Forms.RadioButton rangeFit;
        private System.Windows.Forms.RadioButton clusterFit;
        private System.Windows.Forms.RadioButton iterativeFit;
        private System.Windows.Forms.RadioButton uniformMetric;
        private System.Windows.Forms.RadioButton perceptualMetric;
        private System.Windows.Forms.CheckBox weightColourByAlpha;
        private System.Windows.Forms.ComboBox fileFormatList;
        private System.Windows.Forms.CheckBox generateMipMaps;
        private PaintDotNet.HeaderLabel compressorTypeLabel;
        private PaintDotNet.HeaderLabel	errorMetricLabel;
        private PaintDotNet.HeaderLabel additionalOptionsLabel;
        private System.Windows.Forms.Panel compressorTypePanel;
        private System.Windows.Forms.Panel errorMetricPanel;
        private	System.Windows.Forms.Panel additionalOptionsPanel;
    
        public DdsSaveConfigWidget()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
        }

        protected override void InitFileType()
        {
            this.fileType = new DdsFileType();
        }

        protected override void InitTokenFromWidget()
        {
            ((DdsSaveConfigToken)this.token).m_fileFormat			= ( DdsFileFormat)this.fileFormatList.SelectedIndex;
            if ( this.clusterFit.Checked )
                ((DdsSaveConfigToken)this.token).m_compressorType = 0;
            else
            if ( this.rangeFit.Checked )
                ((DdsSaveConfigToken)this.token).m_compressorType = 1;
            else
                ((DdsSaveConfigToken)this.token).m_compressorType = 2;

            ((DdsSaveConfigToken)this.token).m_errorMetric			= this.perceptualMetric.Checked ? 0 : 1;
            ((DdsSaveConfigToken)this.token).m_weightColourByAlpha	= this.weightColourByAlpha.Checked;
            ((DdsSaveConfigToken)this.token).m_generateMipMaps		= this.generateMipMaps.Checked;
        }

        protected override void InitWidgetFromToken(SaveConfigToken token)
        {
            if (token is DdsSaveConfigToken)
            {
                DdsSaveConfigToken ddsToken = (DdsSaveConfigToken)token;
                this.fileFormatList.SelectedIndex	= ( int )ddsToken.m_fileFormat;

                this.clusterFit.Checked				= ( ddsToken.m_compressorType == 0 );
                this.rangeFit.Checked				= ( ddsToken.m_compressorType == 1 );
                this.iterativeFit.Checked			= ( ddsToken.m_compressorType == 2 );

                this.perceptualMetric.Checked		= ( ddsToken.m_errorMetric == 0 );
                this.uniformMetric.Checked			= !this.perceptualMetric.Checked;

                this.weightColourByAlpha.Checked	= ddsToken.m_weightColourByAlpha;

                this.generateMipMaps.Checked		= ddsToken.m_generateMipMaps;
            }
            else
            {
                this.fileFormatList.SelectedIndex	= 0;

                this.clusterFit.Checked				= true;
                this.rangeFit.Checked				= false;
                this.iterativeFit.Checked			= false;

                this.perceptualMetric.Checked		= true;
                this.uniformMetric.Checked			= false;

                this.weightColourByAlpha.Checked	= false;

                this.generateMipMaps.Checked		= false;
            }
        }

        private void InitializeComponent()
        {
            this.rangeFit = new System.Windows.Forms.RadioButton();
            this.clusterFit = new System.Windows.Forms.RadioButton();
            this.iterativeFit = new System.Windows.Forms.RadioButton();
            this.uniformMetric = new System.Windows.Forms.RadioButton();
            this.perceptualMetric = new System.Windows.Forms.RadioButton();
            this.generateMipMaps = new System.Windows.Forms.CheckBox();
            this.weightColourByAlpha = new System.Windows.Forms.CheckBox();
            this.fileFormatList = new System.Windows.Forms.ComboBox();
            this.compressorTypeLabel = new PaintDotNet.HeaderLabel();
            this.errorMetricLabel = new PaintDotNet.HeaderLabel();
            this.additionalOptionsLabel = new PaintDotNet.HeaderLabel();
            this.compressorTypePanel = new System.Windows.Forms.Panel();
            this.errorMetricPanel = new System.Windows.Forms.Panel();
            this.additionalOptionsPanel = new System.Windows.Forms.Panel();
            this.compressorTypePanel.SuspendLayout();
            this.errorMetricPanel.SuspendLayout();
            this.additionalOptionsPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // rangeFit
            // 
            this.rangeFit.AutoSize = false;
            this.rangeFit.Name = "rangeFit";
            this.rangeFit.TabIndex = 0;
            this.rangeFit.TabStop = true;
            this.rangeFit.Text = PdnResources.GetString("DdsFileType.SaveConfigWidget.RangeFit.Text"); // "Range fit (Fast/LQ)"
            this.rangeFit.UseVisualStyleBackColor = true;
            this.rangeFit.CheckedChanged += new System.EventHandler(this.rangeFit_CheckedChanged);
            this.rangeFit.FlatStyle = FlatStyle.System;
            // 
            // clusterFit
            // 
            this.clusterFit.AutoSize = false;
            this.clusterFit.Name = "clusterFit";
            this.clusterFit.TabIndex = 1;
            this.clusterFit.TabStop = true;
            this.clusterFit.Text = PdnResources.GetString("DdsFileType.SaveConfigWidget.ClusterFit.Text"); // "Cluster fit (Slow/HQ)"
            this.clusterFit.UseVisualStyleBackColor = true;
            this.clusterFit.CheckedChanged += new System.EventHandler(this.clusterFit_CheckedChanged);
            this.clusterFit.FlatStyle = FlatStyle.System;
            // 
            // iterativeFit
            // 
            this.iterativeFit.AutoSize = false;
            this.iterativeFit.Name = "iterativeFit";
            this.iterativeFit.TabIndex = 2;
            this.iterativeFit.TabStop = true;
            this.iterativeFit.Text = PdnResources.GetString("DdsFileType.SaveConfigWidget.IterativeFit.Text"); // "Iterative fit (Slowest/HQ)";
            this.iterativeFit.UseVisualStyleBackColor = true;
            this.iterativeFit.CheckedChanged += new System.EventHandler(this.iterativeFit_CheckedChanged);
            this.iterativeFit.FlatStyle = FlatStyle.System;
            // 
            // uniformMetric
            // 
            this.uniformMetric.AutoSize = false;
            this.uniformMetric.Name = "uniformMetric";
            this.uniformMetric.TabIndex = 0;
            this.uniformMetric.TabStop = true;
            this.uniformMetric.Text = PdnResources.GetString("DdsFileType.SaveConfigWidget.Uniform.Text"); // "Uniform";
            this.uniformMetric.UseVisualStyleBackColor = true;
            this.uniformMetric.CheckedChanged += new System.EventHandler(this.uniformMetric_CheckedChanged);
            this.uniformMetric.FlatStyle = FlatStyle.System;
            // 
            // perceptualMetric
            // 
            this.perceptualMetric.AutoSize = false;
            this.perceptualMetric.Name = "perceptualMetric";
            this.perceptualMetric.TabIndex = 1;
            this.perceptualMetric.TabStop = true;
            this.perceptualMetric.Text = PdnResources.GetString("DdsFileType.SaveConfigWidget.Perceptual.Text"); // "Perceptual";
            this.perceptualMetric.UseVisualStyleBackColor = true;
            this.perceptualMetric.CheckedChanged += new System.EventHandler(this.perceptualMetric_CheckedChanged);
            this.perceptualMetric.FlatStyle = FlatStyle.System;
            // 
            // generateMipMaps
            // 
            this.generateMipMaps.AutoSize = false;
            this.generateMipMaps.Name = "generateMipMaps";
            this.generateMipMaps.TabIndex = 1;
            this.generateMipMaps.Text = PdnResources.GetString("DdsFileType.SaveConfigWidget.GenerateMipMaps.Text"); // "Generate Mip Maps";
            this.generateMipMaps.UseVisualStyleBackColor = true;
            this.generateMipMaps.CheckedChanged += new System.EventHandler(this.generateMipLevels_CheckedChanged);
            this.generateMipMaps.FlatStyle = FlatStyle.System;  
            // 
            // weightColourByAlpha
            // 
            this.weightColourByAlpha.AutoSize = false;
            this.weightColourByAlpha.Name = "weightColourByAlpha";
            this.weightColourByAlpha.TabIndex = 0;
            this.weightColourByAlpha.Text = PdnResources.GetString("DdsFileType.SaveConfigWidget.WeightColourByAlpha"); // "Weight Colour By Alpha";
            this.weightColourByAlpha.UseVisualStyleBackColor = true;
            this.weightColourByAlpha.CheckedChanged += new System.EventHandler(this.weightColourByAlpha_CheckedChanged);
            this.weightColourByAlpha.FlatStyle = FlatStyle.System;
            // 
            // fileFormatList
            // 
            this.fileFormatList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.fileFormatList.FormattingEnabled = true;
            this.fileFormatList.Items.AddRange(new object[] {
                PdnResources.GetString("DdsFileType.SaveConfigWidget.FileFormatList.DXT1"),     // "DXT1 (Opaque/1-bit Alpha)",
                PdnResources.GetString("DdsFileType.SaveConfigWidget.FileFormatList.DXT3"),     // "DXT3 (Explicit Alpha)",
                PdnResources.GetString("DdsFileType.SaveConfigWidget.FileFormatList.DXT5"),     // "DXT5 (Interpolated Alpha)",
                PdnResources.GetString("DdsFileType.SaveConfigWidget.FileFormatList.A8R8G8B8"), // "A8R8G8B8",
                PdnResources.GetString("DdsFileType.SaveConfigWidget.FileFormatList.X8R8G8B8"), // "X8R8G8B8",
                PdnResources.GetString("DdsFileType.SaveConfigWidget.FileFormatList.A8B8G8R8"), // "A8B8G8R8",
                PdnResources.GetString("DdsFileType.SaveConfigWidget.FileFormatList.X8B8G8R8"), // "X8B8G8R8",
                PdnResources.GetString("DdsFileType.SaveConfigWidget.FileFormatList.A1R5G5B5"), // "A1R5G5B5",
                PdnResources.GetString("DdsFileType.SaveConfigWidget.FileFormatList.A4R4G4B4"), // "A4R4G4B4",
                PdnResources.GetString("DdsFileType.SaveConfigWidget.FileFormatList.R8G8B8"),   // "R8G8B8",
                PdnResources.GetString("DdsFileType.SaveConfigWidget.FileFormatList.R5G6B5")    // "R5G6B5"
            });
            this.fileFormatList.Name = "fileFormatList";
            this.fileFormatList.TabIndex = 0;
            this.fileFormatList.SelectedIndexChanged += new System.EventHandler(this.fileFormatList_SelectedIndexChanged);
            this.fileFormatList.FlatStyle = FlatStyle.System;
            // 
            // compressorTypeLabel
            // 
            this.compressorTypeLabel.Name = "compressorTypeLabel";
            this.compressorTypeLabel.RightMargin = 0;
            this.compressorTypeLabel.TabIndex = 1;
            this.compressorTypeLabel.TabStop = false;
            this.compressorTypeLabel.Text = PdnResources.GetString("DdsFileType.SaveConfigWidget.CompressorTypeLabel.Text"); // "Compressor Type";
            // 
            // errorMetricLabel
            // 
            this.errorMetricLabel.Name = "errorMetricLabel";
            this.errorMetricLabel.RightMargin = 0;
            this.errorMetricLabel.TabIndex = 3;
            this.errorMetricLabel.TabStop = false;
            this.errorMetricLabel.Text = PdnResources.GetString("DdsFileType.SaveConfigWidget.ErrorMetricLabel.Text"); // "Error Metric";
            // 
            // additionalOptionsLabel
            // 
            this.additionalOptionsLabel.Name = "additionalOptionsLabel";
            this.additionalOptionsLabel.RightMargin = 0;
            this.additionalOptionsLabel.TabIndex = 5;
            this.additionalOptionsLabel.TabStop = false;
            this.additionalOptionsLabel.Text = PdnResources.GetString("DdsFileType.SaveConfigWidget.AdditionalOptions.Text"); // "Additional Options";
            // 
            // compressorTypePanel
            // 
            this.compressorTypePanel.Controls.Add(this.rangeFit);
            this.compressorTypePanel.Controls.Add(this.clusterFit);
            this.compressorTypePanel.Controls.Add(this.iterativeFit);
            this.compressorTypePanel.Name = "compressorTypePanel";
            this.compressorTypePanel.TabIndex = 2;
            // 
            // errorMetricPanel
            // 
            this.errorMetricPanel.Controls.Add(this.uniformMetric);
            this.errorMetricPanel.Controls.Add(this.perceptualMetric);
            this.errorMetricPanel.Name = "errorMetricPanel";
            this.errorMetricPanel.TabIndex = 4;
            // 
            // additionalOptionsPanel
            // 
            this.additionalOptionsPanel.Controls.Add(this.generateMipMaps);
            this.additionalOptionsPanel.Controls.Add(this.weightColourByAlpha);
            this.additionalOptionsPanel.Name = "additionalOptionsPanel";
            this.additionalOptionsPanel.TabIndex = 6;
            // 
            // DdsSaveConfigWidget
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.fileFormatList);
            this.Controls.Add(this.compressorTypePanel);
            this.Controls.Add(this.compressorTypeLabel);
            this.Controls.Add(this.errorMetricPanel);
            this.Controls.Add(this.errorMetricLabel);
            this.Controls.Add(this.additionalOptionsPanel);
            this.Controls.Add(this.additionalOptionsLabel);
            this.Name = "DdsSaveConfigWidget";
            this.compressorTypePanel.ResumeLayout(false);
            this.compressorTypePanel.PerformLayout();
            this.errorMetricPanel.ResumeLayout(false);
            this.errorMetricPanel.PerformLayout();
            this.additionalOptionsPanel.ResumeLayout(false);
            this.additionalOptionsPanel.PerformLayout();
            this.ResumeLayout(false);
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            int vMargin = UI.ScaleHeight(4);
            int hInset = UI.ScaleWidth(16);

            AutoSizeStrategy autoSizeStrategy = AutoSizeStrategy.ExpandHeightToContentAndKeepWidth;
            EdgeSnapOptions edgeSnapOptions = 
                EdgeSnapOptions.SnapLeftEdgeToContainerLeftEdge | 
                EdgeSnapOptions.SnapRightEdgeToContainerRightEdge;

            this.fileFormatList.Location = new Point(0, 0);
            this.fileFormatList.Width = ClientSize.Width;
            this.fileFormatList.PerformLayout();

            this.compressorTypeLabel.Location = new Point(0, this.fileFormatList.Bottom + vMargin * 2);
            this.compressorTypeLabel.Size = this.compressorTypeLabel.GetPreferredSize(new Size(ClientSize.Width - this.compressorTypeLabel.Left, 1));
            this.compressorTypeLabel.PerformLayout();

            this.compressorTypePanel.SuspendLayout();
            this.compressorTypePanel.Location = new Point(hInset, this.compressorTypeLabel.Bottom + vMargin);
            this.compressorTypePanel.Width = ClientSize.Width - this.compressorTypePanel.Left;
            this.rangeFit.Location = new Point(0, 0);
            LayoutUtility.PerformAutoLayout(this.rangeFit, autoSizeStrategy, edgeSnapOptions);
            this.clusterFit.Location = new Point(0, this.rangeFit.Bottom + vMargin);
            LayoutUtility.PerformAutoLayout(this.clusterFit, autoSizeStrategy, edgeSnapOptions);
            this.iterativeFit.Location = new Point(0, this.clusterFit.Bottom + vMargin);
            LayoutUtility.PerformAutoLayout(this.iterativeFit, autoSizeStrategy, edgeSnapOptions);
            this.compressorTypePanel.Height = this.iterativeFit.Bottom;
            this.compressorTypePanel.ResumeLayout(true);

            this.errorMetricLabel.Location = new Point(0, this.compressorTypePanel.Bottom + vMargin * 2);
            this.errorMetricLabel.Size = this.errorMetricLabel.GetPreferredSize(new Size(ClientSize.Width - this.errorMetricLabel.Left, 1));
            this.errorMetricLabel.PerformLayout();

            this.errorMetricPanel.SuspendLayout();
            this.errorMetricPanel.Location = new Point(hInset, this.errorMetricLabel.Bottom + vMargin);
            this.errorMetricPanel.Width = ClientSize.Width - this.errorMetricPanel.Left;
            this.uniformMetric.Location = new Point(0, 0);
            LayoutUtility.PerformAutoLayout(this.uniformMetric, autoSizeStrategy, edgeSnapOptions);
            this.perceptualMetric.Location = new Point(0, this.uniformMetric.Bottom + vMargin);
            LayoutUtility.PerformAutoLayout(this.perceptualMetric, autoSizeStrategy, edgeSnapOptions);
            this.errorMetricPanel.Height = this.perceptualMetric.Bottom;
            this.errorMetricPanel.ResumeLayout(true);

            this.additionalOptionsLabel.Location = new Point(0, this.errorMetricPanel.Bottom + vMargin * 2);
            this.additionalOptionsLabel.Size = this.additionalOptionsLabel.GetPreferredSize(new Size(ClientSize.Width - this.additionalOptionsLabel.Left, 1));
            this.additionalOptionsLabel.PerformLayout();

            this.additionalOptionsPanel.SuspendLayout();
            this.additionalOptionsPanel.Location = new Point(hInset, this.additionalOptionsLabel.Bottom + vMargin);
            this.additionalOptionsPanel.Width = ClientSize.Width - this.additionalOptionsPanel.Left;
            this.weightColourByAlpha.Location = new Point(0, 0);
            LayoutUtility.PerformAutoLayout(this.weightColourByAlpha, autoSizeStrategy, edgeSnapOptions);
            this.generateMipMaps.Location = new Point(0, this.weightColourByAlpha.Bottom + vMargin);
            LayoutUtility.PerformAutoLayout(this.generateMipMaps, autoSizeStrategy, edgeSnapOptions);
            this.additionalOptionsPanel.Height = this.generateMipMaps.Bottom;
            this.additionalOptionsPanel.ResumeLayout(true);

            this.ClientSize = new Size(ClientSize.Width, this.additionalOptionsPanel.Bottom);
            base.OnLayout(e);
        }

        private void CommonCompressorTypeChangeHandling(object sender, EventArgs e)
        {
            this.clusterFit.Enabled = ( this.fileFormatList.SelectedIndex < 3 );
            this.rangeFit.Enabled = ( this.fileFormatList.SelectedIndex < 3 );
            this.iterativeFit.Enabled = ( this.fileFormatList.SelectedIndex < 3 );
            this.weightColourByAlpha.Enabled = ( this.clusterFit.Checked || this.iterativeFit.Checked ) && ( this.fileFormatList.SelectedIndex < 3 );
            this.uniformMetric.Enabled = ( this.fileFormatList.SelectedIndex < 3 );
            this.perceptualMetric.Enabled = ( this.fileFormatList.SelectedIndex < 3 );
            this.UpdateToken();
        }

        private void fileFormatList_SelectedIndexChanged(object sender, EventArgs e)
        {
            CommonCompressorTypeChangeHandling( sender, e );
        }

        private void clusterFit_CheckedChanged(object sender, EventArgs e)
        {
            CommonCompressorTypeChangeHandling( sender, e );
        }

        private void rangeFit_CheckedChanged(object sender, EventArgs e)
        {
            CommonCompressorTypeChangeHandling( sender, e );
        }

        private void iterativeFit_CheckedChanged(object sender, EventArgs e)
        {
            CommonCompressorTypeChangeHandling( sender, e );
        }

        private void perceptualMetric_CheckedChanged(object sender, EventArgs e)
        {
            this.UpdateToken();
        }

        private void uniformMetric_CheckedChanged(object sender, EventArgs e)
        {
            this.UpdateToken();
        }

        private void weightColourByAlpha_CheckedChanged(object sender, EventArgs e)
        {
            this.UpdateToken();
        }

        private void generateMipLevels_CheckedChanged(object sender, EventArgs e)
        {
            this.UpdateToken();
        }
    }
}
