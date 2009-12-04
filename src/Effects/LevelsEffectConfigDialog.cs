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
using System.Windows.Forms;

namespace PaintDotNet.Effects
{
    public sealed class LevelsEffectConfigDialog 
        : EffectConfigDialog
    {
        private bool[] mask = new bool[3];
        private uint ignore = 0;
        private System.Windows.Forms.CheckBox redMaskCheckBox;
        private System.Windows.Forms.CheckBox greenMaskCheckBox;
        private System.Windows.Forms.CheckBox blueMaskCheckBox;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button autoButton;
        private System.Windows.Forms.Button resetButton;
        private System.Windows.Forms.ToolTip tooltipProvider;
        private TableLayoutPanel tableLayoutPanel2;
        private ColorGradientControl gradientInput;
        private NumericUpDown outputHiUpDown;
        private Panel swatchOutHigh;
        private NumericUpDown outputGammaUpDown;
        private Panel swatchOutMid;
        private Panel swatchOutLow;
        private NumericUpDown outputLowUpDown;
        private ColorGradientControl gradientOutput;
        private HistogramControl histogramOutput;
        private HistogramControl histogramInput;
        private NumericUpDown inputHiUpDown;
        private Panel swatchInHigh;
        private NumericUpDown inputLoUpDown;
        private Panel swatchInLow;
        private HeaderLabel headerHistogramOutput;
        private HeaderLabel headerControlsOutput;
        private HeaderLabel headerControlsInput;
        private HeaderLabel headerHistogramInput;
        private TableLayoutPanel tableMain;
        private System.ComponentModel.IContainer components;
    
        public LevelsEffectConfigDialog()
        {
            InitializeComponent();

            this.Text = PdnResources.GetString("LevelsEffectConfigDialog.Text");
            this.headerControlsOutput.Text = PdnResources.GetString("LevelsEffectConfigDialog.OutputGroupBox.Text");
            this.tooltipProvider.SetToolTip(this.outputGammaUpDown, PdnResources.GetString("LevelsEffectConfigDialog.OutputGammaUpDown.ToolTipText"));
            this.tooltipProvider.SetToolTip(this.swatchOutHigh, PdnResources.GetString("LevelsEffectConfigDialog.SwatchOutHigh.ToolTipText"));
            this.tooltipProvider.SetToolTip(this.swatchOutLow, PdnResources.GetString("LevelsEffectConfigDialog.SwatchOutLow.ToolTipText"));
            this.headerHistogramOutput.Text = PdnResources.GetString("LevelsEffectConfigDialog.OutputHistogramGroupBox.Text");
            this.tooltipProvider.SetToolTip(this.histogramOutput, PdnResources.GetString("LevelsEffectConfigDialog.HistogramOutput.ToolTipText"));
            this.headerHistogramInput.Text = PdnResources.GetString("LevelsEffectConfigDialog.InputHistogramGroupBox.Text");
            this.tooltipProvider.SetToolTip(this.histogramInput, PdnResources.GetString("LevelsEffectConfigDialog.HistogramInput.ToolTipText"));
            this.headerControlsInput.Text = PdnResources.GetString("LevelsEffectConfigDialog.InputGroupBox.Text");
            this.tooltipProvider.SetToolTip(this.swatchInHigh, PdnResources.GetString("LevelsEffectConfigDialog.SwatchInHigh.ToolTipText"));
            this.tooltipProvider.SetToolTip(this.swatchInLow, PdnResources.GetString("LevelsEffectConfigDialog.SwatchInLow.ToolTipText"));
            this.redMaskCheckBox.Text = PdnResources.GetString("LevelsEffectConfigDialog.RedMaskCheckBox.Text");
            this.tooltipProvider.SetToolTip(this.redMaskCheckBox, PdnResources.GetString("LevelsEffectConfigDialog.RedMaskCheckBox.ToolTipText"));
            this.greenMaskCheckBox.Text = PdnResources.GetString("LevelsEffectConfigDialog.GreenMaskCheckBox.Text");
            this.tooltipProvider.SetToolTip(this.greenMaskCheckBox, PdnResources.GetString("LevelsEffectConfigDialog.GreenMaskCheckBox.ToolTipText"));
            this.blueMaskCheckBox.Text = PdnResources.GetString("LevelsEffectConfigDialog.BlueMaskCheckBox.Text");
            this.tooltipProvider.SetToolTip(this.blueMaskCheckBox, PdnResources.GetString("LevelsEffectConfigDialog.BlueMaskCheckBox.ToolTipText"));
            this.okButton.Text = PdnResources.GetString("Form.OkButton.Text");
            this.cancelButton.Text = PdnResources.GetString("Form.CancelButton.Text");
            this.autoButton.Text = PdnResources.GetString("LevelsEffectConfigDialog.AutoButton.Text");
            this.tooltipProvider.SetToolTip(this.autoButton, PdnResources.GetString("LevelsEffectConfigDialog.AutoButton.ToolTipText"));
            this.resetButton.Text = PdnResources.GetString("LevelsEffectConfigDialog.ResetButton.Text");
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            PaintDotNet.HistogramRgb histogramRgb1 = new PaintDotNet.HistogramRgb();
            PaintDotNet.HistogramRgb histogramRgb2 = new PaintDotNet.HistogramRgb();
            this.redMaskCheckBox = new System.Windows.Forms.CheckBox();
            this.greenMaskCheckBox = new System.Windows.Forms.CheckBox();
            this.blueMaskCheckBox = new System.Windows.Forms.CheckBox();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.autoButton = new System.Windows.Forms.Button();
            this.resetButton = new System.Windows.Forms.Button();
            this.tooltipProvider = new System.Windows.Forms.ToolTip(this.components);
            this.tableMain = new System.Windows.Forms.TableLayoutPanel();
            this.headerHistogramOutput = new PaintDotNet.HeaderLabel();
            this.headerControlsOutput = new PaintDotNet.HeaderLabel();
            this.headerControlsInput = new PaintDotNet.HeaderLabel();
            this.headerHistogramInput = new PaintDotNet.HeaderLabel();
            this.swatchInLow = new System.Windows.Forms.Panel();
            this.inputHiUpDown = new System.Windows.Forms.NumericUpDown();
            this.swatchInHigh = new System.Windows.Forms.Panel();
            this.inputLoUpDown = new System.Windows.Forms.NumericUpDown();
            this.swatchOutLow = new System.Windows.Forms.Panel();
            this.outputGammaUpDown = new System.Windows.Forms.NumericUpDown();
            this.swatchOutHigh = new System.Windows.Forms.Panel();
            this.outputHiUpDown = new System.Windows.Forms.NumericUpDown();
            this.gradientInput = new PaintDotNet.ColorGradientControl();
            this.swatchOutMid = new System.Windows.Forms.Panel();
            this.gradientOutput = new PaintDotNet.ColorGradientControl();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.histogramInput = new PaintDotNet.HistogramControl();
            this.histogramOutput = new PaintDotNet.HistogramControl();
            this.outputLowUpDown = new System.Windows.Forms.NumericUpDown();
            this.tableMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.inputHiUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.inputLoUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.outputGammaUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.outputHiUpDown)).BeginInit();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.outputLowUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // redMaskCheckBox
            // 
            this.redMaskCheckBox.Checked = true;
            this.redMaskCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.redMaskCheckBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.redMaskCheckBox.Location = new System.Drawing.Point(165, 3);
            this.redMaskCheckBox.Name = "redMaskCheckBox";
            this.redMaskCheckBox.Size = new System.Drawing.Size(34, 23);
            this.redMaskCheckBox.TabIndex = 8;
            this.redMaskCheckBox.Click += new System.EventHandler(this.redMaskCheckBox_CheckedChanged);
            this.redMaskCheckBox.CheckedChanged += new System.EventHandler(this.redMaskCheckBox_CheckedChanged);
            this.redMaskCheckBox.FlatStyle = FlatStyle.System;
            // 
            // greenMaskCheckBox
            // 
            this.greenMaskCheckBox.Checked = true;
            this.greenMaskCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.greenMaskCheckBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.greenMaskCheckBox.Location = new System.Drawing.Point(205, 3);
            this.greenMaskCheckBox.Name = "greenMaskCheckBox";
            this.greenMaskCheckBox.Size = new System.Drawing.Size(34, 23);
            this.greenMaskCheckBox.TabIndex = 9;
            this.greenMaskCheckBox.Click += new System.EventHandler(this.greenMaskCheckBox_CheckedChanged);
            this.greenMaskCheckBox.CheckedChanged += new System.EventHandler(this.greenMaskCheckBox_CheckedChanged);
            this.greenMaskCheckBox.FlatStyle = FlatStyle.System;
            // 
            // blueMaskCheckBox
            // 
            this.blueMaskCheckBox.Checked = true;
            this.blueMaskCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.blueMaskCheckBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.blueMaskCheckBox.Location = new System.Drawing.Point(245, 3);
            this.blueMaskCheckBox.Name = "blueMaskCheckBox";
            this.blueMaskCheckBox.Size = new System.Drawing.Size(34, 23);
            this.blueMaskCheckBox.TabIndex = 10;
            this.blueMaskCheckBox.Click += new System.EventHandler(this.blueMaskCheckBox_CheckedChanged);
            this.blueMaskCheckBox.CheckedChanged += new System.EventHandler(this.blueMaskCheckBox_CheckedChanged);
            this.blueMaskCheckBox.FlatStyle = FlatStyle.System;
            // 
            // okButton
            // 
            this.okButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.okButton.Location = new System.Drawing.Point(285, 3);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 11;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cancelButton.Location = new System.Drawing.Point(366, 3);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(77, 23);
            this.cancelButton.TabIndex = 12;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // autoButton
            // 
            this.autoButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.autoButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.autoButton.Location = new System.Drawing.Point(3, 3);
            this.autoButton.Name = "autoButton";
            this.autoButton.Size = new System.Drawing.Size(75, 23);
            this.autoButton.TabIndex = 6;
            this.autoButton.Click += new System.EventHandler(this.autoButton_Click);
            // 
            // resetButton
            // 
            this.resetButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.resetButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.resetButton.Location = new System.Drawing.Point(84, 3);
            this.resetButton.Name = "resetButton";
            this.resetButton.Size = new System.Drawing.Size(75, 23);
            this.resetButton.TabIndex = 7;
            this.resetButton.Click += new System.EventHandler(this.resetButton_Click);
            // 
            // tableMain
            // 
            this.tableMain.ColumnCount = 6;
            this.tableMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableMain.Controls.Add(this.headerHistogramOutput, 5, 0);
            this.tableMain.Controls.Add(this.headerControlsOutput, 3, 0);
            this.tableMain.Controls.Add(this.headerControlsInput, 1, 0);
            this.tableMain.Controls.Add(this.headerHistogramInput, 0, 0);
            this.tableMain.Controls.Add(this.swatchInLow, 1, 7);
            this.tableMain.Controls.Add(this.inputHiUpDown, 1, 1);
            this.tableMain.Controls.Add(this.swatchInHigh, 1, 2);
            this.tableMain.Controls.Add(this.inputLoUpDown, 1, 8);
            this.tableMain.Controls.Add(this.swatchOutLow, 4, 7);
            this.tableMain.Controls.Add(this.outputGammaUpDown, 4, 4);
            this.tableMain.Controls.Add(this.swatchOutHigh, 4, 2);
            this.tableMain.Controls.Add(this.outputHiUpDown, 4, 1);
            this.tableMain.Controls.Add(this.gradientInput, 2, 1);
            this.tableMain.Controls.Add(this.swatchOutMid, 4, 5);
            this.tableMain.Controls.Add(this.gradientOutput, 3, 1);
            this.tableMain.Controls.Add(this.tableLayoutPanel2, 0, 9);
            this.tableMain.Controls.Add(this.histogramInput, 0, 1);
            this.tableMain.Controls.Add(this.histogramOutput, 5, 1);
            this.tableMain.Controls.Add(this.outputLowUpDown, 4, 8);
            this.tableMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableMain.Location = new System.Drawing.Point(0, 0);
            this.tableMain.Name = "tableMain";
            this.tableMain.RowCount = 10;
            this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableMain.Size = new System.Drawing.Size(452, 211);
            this.tableMain.TabStop = false;
            // 
            // headerHistogramOutput
            // 
            this.headerHistogramOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.headerHistogramOutput.Location = new System.Drawing.Point(319, 3);
            this.headerHistogramOutput.Name = "headerHistogramOutput";
            this.headerHistogramOutput.RightMargin = 3;
            this.headerHistogramOutput.Size = new System.Drawing.Size(130, 14);
            this.headerHistogramOutput.TabStop = false;
            // 
            // headerControlsOutput
            // 
            this.tableMain.SetColumnSpan(this.headerControlsOutput, 2);
            this.headerControlsOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.headerControlsOutput.Location = new System.Drawing.Point(229, 3);
            this.headerControlsOutput.Name = "headerControlsOutput";
            this.headerControlsOutput.RightMargin = 3;
            this.headerControlsOutput.Size = new System.Drawing.Size(84, 14);
            this.headerControlsOutput.TabStop = false;
            // 
            // headerControlsInput
            // 
            this.tableMain.SetColumnSpan(this.headerControlsInput, 2);
            this.headerControlsInput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.headerControlsInput.Location = new System.Drawing.Point(139, 3);
            this.headerControlsInput.Name = "headerControlsInput";
            this.headerControlsInput.RightMargin = 3;
            this.headerControlsInput.Size = new System.Drawing.Size(84, 14);
            this.headerControlsInput.TabStop = false;
            // 
            // headerHistogramInput
            // 
            this.headerHistogramInput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.headerHistogramInput.Location = new System.Drawing.Point(3, 3);
            this.headerHistogramInput.Name = "headerHistogramInput";
            this.headerHistogramInput.RightMargin = 3;
            this.headerHistogramInput.Size = new System.Drawing.Size(130, 14);
            this.headerHistogramInput.TabStop = false;
            // 
            // swatchInLow
            // 
            this.swatchInLow.BackColor = System.Drawing.Color.Black;
            this.swatchInLow.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.swatchInLow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.swatchInLow.Location = new System.Drawing.Point(139, 127);
            this.swatchInLow.Name = "swatchInLow";
            this.swatchInLow.Size = new System.Drawing.Size(44, 20);
            this.swatchInLow.TabStop = false;
            this.swatchInLow.DoubleClick += swatch_DoubleClick;
            // 
            // inputHiUpDown
            // 
            this.inputHiUpDown.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inputHiUpDown.Location = new System.Drawing.Point(139, 23);
            this.inputHiUpDown.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.inputHiUpDown.Name = "inputHiUpDown";
            this.inputHiUpDown.Size = new System.Drawing.Size(44, 20);
            this.inputHiUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.inputHiUpDown.ValueChanged += new System.EventHandler(this.txtInputHi_ValueChanged);
            this.inputHiUpDown.Validating += new System.ComponentModel.CancelEventHandler(this.txtInputHi_Validating);
            this.inputHiUpDown.TabIndex = 1;
            // 
            // swatchInHigh
            // 
            this.swatchInHigh.BackColor = System.Drawing.Color.White;
            this.swatchInHigh.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.swatchInHigh.Dock = System.Windows.Forms.DockStyle.Fill;
            this.swatchInHigh.Location = new System.Drawing.Point(139, 49);
            this.swatchInHigh.Name = "swatchInHigh";
            this.swatchInHigh.Size = new System.Drawing.Size(44, 20);
            this.swatchInHigh.TabStop = false;
            this.swatchInHigh.DoubleClick += swatch_DoubleClick;
            // 
            // inputLoUpDown
            // 
            this.inputLoUpDown.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inputLoUpDown.Location = new System.Drawing.Point(139, 153);
            this.inputLoUpDown.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.inputLoUpDown.Name = "inputLoUpDown";
            this.inputLoUpDown.Size = new System.Drawing.Size(44, 20);
            this.inputLoUpDown.TabIndex = 4;
            this.inputLoUpDown.ValueChanged += new System.EventHandler(this.txtInputLo_ValueChanged);
            this.inputLoUpDown.Validating += new System.ComponentModel.CancelEventHandler(this.txtInputLo_Validating);
            // 
            // swatchOutLow
            // 
            this.swatchOutLow.BackColor = System.Drawing.Color.Black;
            this.swatchOutLow.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.swatchOutLow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.swatchOutLow.Location = new System.Drawing.Point(269, 127);
            this.swatchOutLow.Name = "swatchOutLow";
            this.swatchOutLow.Size = new System.Drawing.Size(44, 20);
            this.swatchOutLow.TabStop = false;
            this.swatchOutLow.DoubleClick += swatch_DoubleClick;
            // 
            // outputGammaUpDown
            // 
            this.outputGammaUpDown.DecimalPlaces = 2;
            this.outputGammaUpDown.Dock = System.Windows.Forms.DockStyle.Fill;
            this.outputGammaUpDown.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.outputGammaUpDown.Location = new System.Drawing.Point(269, 75);
            this.outputGammaUpDown.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            65536});
            this.outputGammaUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.outputGammaUpDown.Name = "outputGammaUpDown";
            this.outputGammaUpDown.Size = new System.Drawing.Size(44, 20);
            this.outputGammaUpDown.TabIndex = 3;
            this.outputGammaUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.outputGammaUpDown.ValueChanged += new EventHandler(outputGammaUpDown_ValueChanged);
            this.outputGammaUpDown.Validating += new System.ComponentModel.CancelEventHandler(this.outputGammaUpDown_Validating);
            // 
            // swatchOutHigh
            // 
            this.swatchOutHigh.BackColor = System.Drawing.Color.White;
            this.swatchOutHigh.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.swatchOutHigh.Dock = System.Windows.Forms.DockStyle.Fill;
            this.swatchOutHigh.Location = new System.Drawing.Point(269, 49);
            this.swatchOutHigh.Name = "swatchOutHigh";
            this.swatchOutHigh.Size = new System.Drawing.Size(44, 20);
            this.swatchOutHigh.TabStop = false;
            this.swatchOutHigh.DoubleClick += swatch_DoubleClick;
            // 
            // outputHiUpDown
            // 
            this.outputHiUpDown.Dock = System.Windows.Forms.DockStyle.Fill;
            this.outputHiUpDown.Location = new System.Drawing.Point(269, 23);
            this.outputHiUpDown.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.outputHiUpDown.Name = "outputHiUpDown";
            this.outputHiUpDown.Size = new System.Drawing.Size(44, 20);
            this.outputHiUpDown.TabIndex = 2;
            this.outputHiUpDown.Value = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.outputHiUpDown.Validating += new System.ComponentModel.CancelEventHandler(this.outputHiUpDown_Validating);
            this.outputHiUpDown.ValueChanged += new EventHandler(outputHiUpDown_ValueChanged);
            // 
            // gradientInput
            // 
            this.gradientInput.MinColor = System.Drawing.Color.Black;
            this.gradientInput.Count = 2;
            this.gradientInput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gradientInput.Location = new System.Drawing.Point(189, 23);
            this.gradientInput.Name = "gradientInput";
            this.tableMain.SetRowSpan(this.gradientInput, 8);
            this.gradientInput.Size = new System.Drawing.Size(34, 150);
            this.gradientInput.MaxColor = System.Drawing.Color.White;
            this.gradientInput.Value = 0;
            this.gradientInput.ValueChanged += new PaintDotNet.IndexEventHandler(this.gradientInput_ValueChanged);
            this.gradientInput.TabStop = false;
            // 
            // swatchOutMid
            // 
            this.swatchOutMid.BackColor = System.Drawing.Color.White;
            this.swatchOutMid.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.swatchOutMid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.swatchOutMid.Location = new System.Drawing.Point(269, 101);
            this.swatchOutMid.Name = "swatchOutMid";
            this.swatchOutMid.Size = new System.Drawing.Size(44, 20);
            this.swatchOutMid.TabStop = false;
            // 
            // gradientOutput
            // 
            this.gradientOutput.MinColor = System.Drawing.Color.Black;
            this.gradientOutput.Count = 3;
            this.gradientOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gradientOutput.Location = new System.Drawing.Point(229, 23);
            this.gradientOutput.Name = "gradientOutput";
            this.tableMain.SetRowSpan(this.gradientOutput, 8);
            this.gradientOutput.Size = new System.Drawing.Size(34, 150);
            this.gradientOutput.MaxColor = System.Drawing.Color.White;
            this.gradientOutput.Value = 0;
            this.gradientOutput.ValueChanged += new PaintDotNet.IndexEventHandler(this.gradientOutput_ValueChanged);
            this.gradientOutput.TabStop = false;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 9;
            this.tableMain.SetColumnSpan(this.tableLayoutPanel2, 6);
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 81F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 81F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 81F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 83F));
            this.tableLayoutPanel2.Controls.Add(this.blueMaskCheckBox, 5, 0);
            this.tableLayoutPanel2.Controls.Add(this.greenMaskCheckBox, 4, 0);
            this.tableLayoutPanel2.Controls.Add(this.redMaskCheckBox, 3, 0);
            this.tableLayoutPanel2.Controls.Add(this.autoButton, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.resetButton, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.okButton, 7, 0);
            this.tableLayoutPanel2.Controls.Add(this.cancelButton, 8, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 179);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(446, 29);
            // 
            // histogramInput
            // 
            this.histogramInput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.histogramInput.FlipHorizontal = true;
            this.histogramInput.FlipVertical = false;
            this.histogramInput.Histogram = histogramRgb1;
            this.histogramInput.Location = new System.Drawing.Point(3, 23);
            this.histogramInput.Name = "histogramInput";
            this.tableMain.SetRowSpan(this.histogramInput, 8);
            this.histogramInput.Size = new System.Drawing.Size(130, 150);
            this.histogramInput.TabStop = false;
            // 
            // histogramOutput
            // 
            this.histogramOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.histogramOutput.FlipHorizontal = false;
            this.histogramOutput.FlipVertical = false;
            this.histogramOutput.Histogram = histogramRgb2;
            this.histogramOutput.Location = new System.Drawing.Point(319, 23);
            this.histogramOutput.Name = "histogramOutput";
            this.tableMain.SetRowSpan(this.histogramOutput, 8);
            this.histogramOutput.Size = new System.Drawing.Size(130, 150);
            this.histogramOutput.TabStop = false;
            // 
            // outputLowUpDown
            // 
            this.outputLowUpDown.Dock = System.Windows.Forms.DockStyle.Fill;
            this.outputLowUpDown.Location = new System.Drawing.Point(269, 153);
            this.outputLowUpDown.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.outputLowUpDown.Name = "outputLowUpDown";
            this.outputLowUpDown.Size = new System.Drawing.Size(44, 20);
            this.outputLowUpDown.TabIndex = 5;
            this.outputLowUpDown.ValueChanged += new System.EventHandler(this.outputLowUpDown_ValueChanged);
            this.outputLowUpDown.Validating += new System.ComponentModel.CancelEventHandler(this.outputLowUpDown_Validating);
            // 
            // LevelsEffectConfigDialog
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(452, 211);
            this.Controls.Add(this.tableMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.MinimumSize = new System.Drawing.Size(460, 245);
            this.Name = "LevelsEffectConfigDialog";
            this.Load += new System.EventHandler(this.LevelsEffectConfigDialog_Load);
            this.Controls.SetChildIndex(this.tableMain, 0);
            this.tableMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.inputHiUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.inputLoUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.outputGammaUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.outputHiUpDown)).EndInit();
            this.tableLayoutPanel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.outputLowUpDown)).EndInit();
            this.ResumeLayout(false);

        }

        private void SetEnabledControls(bool enabled)
        {
            this.inputHiUpDown.Enabled = enabled;
            this.outputHiUpDown.Enabled = enabled;
            this.inputLoUpDown.Enabled = enabled;
            this.outputLowUpDown.Enabled = enabled;
            this.outputGammaUpDown.Enabled = enabled;
            this.gradientInput.Enabled = enabled;
            this.gradientOutput.Enabled = enabled;
        }

        private void MaskChanged() 
        {
            bool anyOn = mask[0] || mask[1] || mask[2];

            SetEnabledControls(anyOn);

            ColorBgra top = ColorBgra.Black;

            top.Bgra |= mask[0] ? (uint)0xFF : 0;
            top.Bgra |= mask[1] ? (uint)0xFF00 : 0;
            top.Bgra |= mask[2] ? (uint)0xFF0000 : 0;

            gradientInput.MaxColor = top.ToColor();
            gradientOutput.MaxColor = top.ToColor();

            for (int i = 0; i < 3; ++i)
            {
                histogramInput.SetSelected(i, mask[i]);
                histogramOutput.SetSelected(i, mask[i]);
            }

            ignore++;
            InitDialogFromToken();
            ignore--;
        }

        private int MaskAvg(ColorBgra before) 
        {
            int count = 0, total = 0;   

            for (int c = 0; c < 3; c++) 
            {
                if (mask[c])
                {
                    total += before[c];
                    count++;
                }
            }

            if (count > 0) 
            {
                return total / count;
            } 
            else
            {
                return 0;
            }
        }

        private ColorBgra UpdateByMask(ColorBgra before, byte val) 
        {
            ColorBgra after = before;
            int average = -1, oldaverage = -1;

            if (!(mask[0] || mask[1] || mask[2]))
            {
                return before;
            }

            do
            {
                float factor;

                oldaverage = average;
                average = MaskAvg(after);

                if (average == 0)
                {
                    break;
                }
                factor = (float)val / average;

                for (int c = 0; c < 3; c++) 
                {
                    if (mask[c]) 
                    {
                        after[c] = (byte)Utility.ClampToByte(after[c] * factor);
                    }
                }
            } while (average != val && oldaverage != average);

            while (average != val) 
            {
                average = MaskAvg(after);
                int diff = val - average;

                for (int c = 0; c < 3; c++) 
                {
                    if (mask[c]) 
                    {
                        after[c] = (byte)Utility.ClampToByte(after[c] + diff);
                    }
                }
            }

            after.A = 255;
            return after;           
        }

        private void LevelsEffectConfigDialog_Load(object sender, System.EventArgs e)
        {
            histogramInput.Histogram.UpdateHistogram(this.EffectSourceSurface, this.Selection);
            mask[0] = true;
            mask[1] = true;
            mask[2] = true;
            MaskChanged();
            UpdateOutputHistogram();
        }

        private void UpdateOutputHistogram() 
        {
            ((HistogramRgb)this.histogramOutput.Histogram).SetFromLeveledHistogram((HistogramRgb)this.histogramInput.Histogram, ((LevelsEffectConfigToken)this.theEffectToken).Levels);
            this.histogramOutput.Update();
        }

        protected override void InitialInitToken()
        {
            theEffectToken = new LevelsEffectConfigToken();
        }

        private void UpdateGammaByMask(UnaryPixelOps.Level levels, float val) 
        {
            float average = -1;

            if (!(mask[0] || mask[1] || mask[2]))
            {
                return;
            }

            do
            {
                average = MaskGamma(levels);
                float factor = val / average;

                for (int c = 0; c < 3; c++) 
                {
                    if (mask[c]) 
                    {
                        levels.SetGamma(c, factor * levels.GetGamma(c));
                    }
                }
            } while (Math.Abs(val - average) > 0.001);
        }

        protected override void InitTokenFromDialog()
        {
            UnaryPixelOps.Level levels = ((LevelsEffectConfigToken)theEffectToken).Levels;

            levels.ColorOutHigh = UpdateByMask(levels.ColorOutHigh, (byte)outputHiUpDown.Value);
            levels.ColorOutLow = UpdateByMask(levels.ColorOutLow, (byte)outputLowUpDown.Value);

            levels.ColorInHigh = UpdateByMask(levels.ColorInHigh, (byte)inputHiUpDown.Value);
            levels.ColorInLow = UpdateByMask(levels.ColorInLow, (byte)inputLoUpDown.Value);

            UpdateGammaByMask(levels, (float)outputGammaUpDown.Value);

            swatchInHigh.BackColor = levels.ColorInHigh.ToColor();
            swatchInHigh.Invalidate();

            swatchInLow.BackColor = levels.ColorInLow.ToColor();
            swatchInLow.Invalidate();

            swatchOutHigh.BackColor = levels.ColorOutHigh.ToColor();
            swatchOutHigh.Invalidate();

            swatchOutMid.BackColor = levels.Apply(((HistogramRgb)histogramInput.Histogram).GetMeanColor()).ToColor();
            swatchOutMid.Invalidate();

            swatchOutLow.BackColor = levels.ColorOutLow.ToColor();
            swatchOutLow.Invalidate();
        }

        private float MaskGamma(UnaryPixelOps.Level levels) 
        {
            int count = 0;
            float total = 0;

            for (int c = 0; c < 3; c++) 
            {
                if (mask[c])
                {
                    total += levels.GetGamma(c);
                    count++;
                }
            }

            if (count > 0) 
            {
                return total / count;
            } 
            else
            {
                return 1;
            }
    
        }

        protected override void InitDialogFromToken(EffectConfigToken effectToken)
        {
            UnaryPixelOps.Level levels = ((LevelsEffectConfigToken)effectToken).Levels;

            float gamma = MaskGamma(levels);
            int lo = MaskAvg(levels.ColorOutLow);
            int hi = MaskAvg(levels.ColorOutHigh);
            int md = (int)(lo + (hi - lo) * Math.Pow(0.5, gamma));

            outputHiUpDown.Value = hi;
            outputGammaUpDown.Value = (decimal)gamma;
            outputLowUpDown.Value = lo;
            inputHiUpDown.Value = MaskAvg(levels.ColorInHigh);
            inputLoUpDown.Value = MaskAvg(levels.ColorInLow);

            gradientOutput.SetValue(0, lo);
            gradientOutput.SetValue(1, md);
            gradientOutput.SetValue(2, hi);

            swatchInHigh.BackColor = levels.ColorInHigh.ToColor();
            swatchInLow.BackColor = levels.ColorInLow.ToColor();
            swatchOutMid.BackColor = levels.Apply(((HistogramRgb)histogramInput.Histogram).GetMeanColor()).ToColor();
            swatchOutMid.Invalidate();
            swatchOutHigh.BackColor = levels.ColorOutHigh.ToColor();
            swatchOutLow.BackColor = levels.ColorOutLow.ToColor();
        }

        private void UpdateLevels() 
        {   
            FinishTokenUpdate();
            UpdateOutputHistogram();
        }

        private void gradientOutput_ValueChanged(object sender, IndexEventArgs e)
        {
            if (ignore == 0) 
            {
                int lo = gradientOutput.GetValue(0), md, hi = gradientOutput.GetValue(2);
                md = (int)(lo + (hi - lo) * Math.Pow(0.5, (double)outputGammaUpDown.Value));
                ignore++;

                switch (e.Index) 
                {
                    case 0:
                        outputLowUpDown.Text = lo.ToString();
                        break;

                    case 1:
                        md = gradientOutput.GetValue(1);
                        outputGammaUpDown.Value = (decimal)Utility.Clamp(1 / Math.Log(0.5, (float)(md - lo) / (float)(hi - lo)), 0.1, 10.0);
                        break;

                    case 2:
                        outputHiUpDown.Text = hi.ToString();
                        break;
                }

                gradientOutput.SetValue(1, md);
                UpdateLevels();
                ignore--;
            }
        }

        private void outputHiUpDown_ValueChanged(object sender, System.EventArgs e)
        {
            if (ignore == 0) 
            {
                ignore++;
                gradientOutput.SetValue(2, (int)outputHiUpDown.Value);
                UpdateLevels();
                ignore--;
            }
        }

        private void outputGammaUpDown_ValueChanged(object sender, System.EventArgs e)
        {
            int lo = gradientOutput.GetValue(0);
            int hi = gradientOutput.GetValue(2);
            int md = (int)(lo + (hi - lo) * Math.Pow(0.5, (double)outputGammaUpDown.Value));

            gradientOutput.SetValue(1, md);

            if (ignore == 0) 
            {
                ignore++;
                UpdateLevels();
                ignore--;
            }
        }

        private void outputLowUpDown_ValueChanged(object sender, System.EventArgs e)
        {
            if (ignore == 0) 
            {
                ignore++;
                gradientOutput.SetValue(0, (int)outputLowUpDown.Value);
                UpdateLevels();
                ignore--;
            }
        }

        private void gradientInput_ValueChanged(object sender, IndexEventArgs e)
        {
            if (ignore == 0) 
            {
                int lo = gradientInput.GetValue(0), hi = gradientInput.GetValue(1);
                ignore++;

                switch (e.Index) 
                {
                    case 0:
                        inputLoUpDown.Text = lo.ToString();
                        break;

                    case 1:
                        inputHiUpDown.Text = hi.ToString();
                        break;
                }

                UpdateLevels();
                ignore--;
            }
        }

        private void txtInputHi_ValueChanged(object sender, System.EventArgs e)
        {
            gradientInput.SetValue(1, (int)inputHiUpDown.Value);

            if (ignore == 0) 
            {
                ignore++;
                UpdateLevels();
                ignore--;
            }
        }

        private void txtInputLo_ValueChanged(object sender, System.EventArgs e)
        {
            gradientInput.SetValue(0, (int)inputLoUpDown.Value);

            if (ignore == 0) 
            {
                ignore++;
                UpdateLevels();
                ignore--;
            }
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout (levent);
        }

        private void okButton_Click(object sender, System.EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void cancelButton_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }

        private void resetButton_Click(object sender, System.EventArgs e)
        {
            ((LevelsEffectConfigToken)this.EffectToken).Levels = new UnaryPixelOps.Level();
            ignore++;
            InitDialogFromToken();
            ignore--;
            UpdateLevels();     
        }

        private void autoButton_Click(object sender, System.EventArgs e)
        {
            ((LevelsEffectConfigToken)this.EffectToken).Levels = ((HistogramRgb)histogramInput.Histogram).MakeLevelsAuto();

            ignore++;
            InitDialogFromToken();
            ignore--;
            UpdateLevels();
        }

        private void swatch_DoubleClick(object sender, System.EventArgs e)
        {
            SystemLayer.Tracing.Ping((sender as Control).Name);

            UnaryPixelOps.Level levels = ((LevelsEffectConfigToken)theEffectToken).Levels;

            using (ColorDialog cd = new ColorDialog())
            {
                if ((sender is Panel)) 
                {
                    cd.Color = ((Panel)sender).BackColor;
                    cd.AnyColor = true;

                    if (cd.ShowDialog(this) == DialogResult.OK) 
                    {
                        ColorBgra col = ColorBgra.FromColor(cd.Color);

                        if (sender == swatchInLow) 
                        {
                            levels.ColorInLow = col;
                        }
                        else if (sender == swatchInHigh) 
                        {
                            levels.ColorInHigh = col;
                        }
                        else if (sender == swatchOutLow) 
                        {
                            levels.ColorOutLow = col;
                        }
                        else if (sender == swatchOutMid)
                        {
                            ColorBgra lo = levels.ColorInLow;
                            ColorBgra md = ((HistogramRgb)histogramInput.Histogram).GetMeanColor();
                            ColorBgra hi = levels.ColorInHigh;
                            ColorBgra out_lo = levels.ColorOutLow;
                            ColorBgra out_hi = levels.ColorOutHigh;

                            for (int i = 0; i < 3; i++) 
                            {
                                double logA = (col[i] - out_lo[i]) / (out_hi[i] - out_lo[i]);
                                double logBase = (md[i] - lo[i]) / (hi[i] - lo[i]);
                                double logVal = (logBase == 1.0) ? 0.0 : Math.Log(logA, logBase);

                                levels.SetGamma(i, (float)Utility.Clamp(logVal, 0.1, 10.0));
                            }
                        }
                        else if (sender == swatchOutHigh) 
                        {
                            levels.ColorOutHigh = col;
                        }
                        else if (sender == swatchInHigh) 
                        {
                            levels.ColorInHigh = col;
                        }

                        InitDialogFromToken();
                    }
                }
            }
        }

        private void blueMaskCheckBox_CheckedChanged(object sender, System.EventArgs e)
        {
            mask[0] = blueMaskCheckBox.Checked;
            MaskChanged();
        }

        private void greenMaskCheckBox_CheckedChanged(object sender, System.EventArgs e)
        {
            mask[1] = greenMaskCheckBox.Checked;
            MaskChanged();
        }

        private void redMaskCheckBox_CheckedChanged(object sender, System.EventArgs e)
        {
            mask[2] = redMaskCheckBox.Checked;
            MaskChanged();
        }

        private void txtInputHi_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            txtInputHi_ValueChanged(sender, EventArgs.Empty);
        }

        private void outputHiUpDown_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            outputHiUpDown_ValueChanged(sender, EventArgs.Empty);      
        }

        private void txtInputLo_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            txtInputLo_ValueChanged(sender, EventArgs.Empty);       
        }

        private void outputLowUpDown_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            outputLowUpDown_ValueChanged(sender, EventArgs.Empty);      
        }

        private void outputGammaUpDown_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            txtInputHi_ValueChanged(sender, EventArgs.Empty);       
        }
    }
}