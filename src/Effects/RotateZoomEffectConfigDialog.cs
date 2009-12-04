/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet;
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;

namespace PaintDotNet.Effects
{
    public sealed class RotateZoomEffectConfigDialog 
        : EffectConfigDialog
    {
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.CheckBox keepBackgroundCheckBox;
        private System.Windows.Forms.CheckBox tileSourceCheckBox;
        private PaintDotNet.Effects.RollControl rollControl;
        private PaintDotNet.HeaderLabel headerRoll;
        private PaintDotNet.HeaderLabel headerPan;
        private System.Windows.Forms.Panel panelPan;
        private System.Windows.Forms.TrackBar trackBarZoom;
        private PaintDotNet.Effects.PanControl panControl;
        private PaintDotNet.HeaderLabel headerZoom;
        private System.Windows.Forms.Label zoomLabel;
        private System.Windows.Forms.Label panXLabel;
        private System.Windows.Forms.Label panYLabel;
        private System.Windows.Forms.Button panResetButton;
        private System.Windows.Forms.NumericUpDown panXUpDown;
        private System.Windows.Forms.NumericUpDown panYUpDown;
        private System.Windows.Forms.Label angleLabel;
        private System.Windows.Forms.NumericUpDown angleUpDown;
        private System.Windows.Forms.Button zoomResetButton;
        private System.Windows.Forms.Label twistAngleLabel;
        private System.Windows.Forms.Label twistRadiusLabel;
        private System.Windows.Forms.NumericUpDown twistAngleUpDown;
        private System.Windows.Forms.NumericUpDown twistRadiusUpDown;
        private System.Windows.Forms.Button rollResetButton;
        private System.Windows.Forms.Button resetAllButton;
        private PaintDotNet.HeaderLabel fineTuningHeader;
        private PaintDotNet.HeaderLabel headerLabel1;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public RotateZoomEffectConfigDialog()
        {
            InitializeComponent();

            this.Icon = Utility.ImageToIcon(RotateZoomEffect.StaticImage.Reference);
            this.Text = RotateZoomEffect.StaticName;
            this.okButton.Text = PdnResources.GetString("Form.OkButton.Text");
            this.cancelButton.Text = PdnResources.GetString("Form.CancelButton.Text");
            this.keepBackgroundCheckBox.Text = PdnResources.GetString("RotateZoomEffectConfigDialog.KeepBackgroundCheckBox.Text");
            this.tileSourceCheckBox.Text = PdnResources.GetString("RotateZoomEffectConfigDialog.TileSourceCheckBox.Text");
            this.headerPan.Text = PdnResources.GetString("RotateZoomEffectConfigDialog.HeaderPan.Text");
            this.panXLabel.Text = PdnResources.GetString("RotateZoomEffectConfigDialog.PanXLabel.Text");
            this.panYLabel.Text = PdnResources.GetString("RotateZoomEffectConfigDialog.PanYLabel.Text");
            this.twistAngleLabel.Text = PdnResources.GetString("RotateZoomEffectConfigDialog.TwistAngleLabel.Text");
            this.twistRadiusLabel.Text = PdnResources.GetString("RotateZoomEffectConfigDialog.TwistRadiusLabel.Text");
            this.panResetButton.Text = PdnResources.GetString("RotateZoomEffectConfigDialog.PanResetButton.Text");
            this.zoomResetButton.Text = PdnResources.GetString("RotateZoomEffectConfigDialog.ZoomResetButton.Text");
            this.rollResetButton.Text = PdnResources.GetString("RotateZoomEffectConfigDialog.RollResetButton.Text");
            this.resetAllButton.Text = PdnResources.GetString("RotateZoomEffectConfigDialog.ResetAllButton.Text");
            this.headerRoll.Text = PdnResources.GetString("RotateZoomEffectConfigDialog.HeaderRoll.Text");
            this.angleLabel.Text = PdnResources.GetString("RotateZoomEffectConfigDialog.AngleLabel.Text");
            this.headerZoom.Text = PdnResources.GetString("RotateZoomEffectConfigDialog.HeaderZoom.Text");
            this.fineTuningHeader.Text = PdnResources.GetString("RotateZoomEffectConfigDialog.FineTuningHeader.Text");
        }

        protected override void OnLoad(EventArgs e)
        {
            this.angleUpDown.Select();
            base.OnLoad(e);
        }

        protected override void InitialInitToken()
        {
            theEffectToken = new RotateZoomEffectConfigToken(true, 0, 0, 0, 1.0f, PointF.Empty, false, false);
        }

        protected override void InitDialogFromToken(EffectConfigToken effectToken)
        {
            RotateZoomEffectConfigToken token = (RotateZoomEffectConfigToken)effectToken;
            double r = Math.Sin(token.Tilt) * 90;
            double t = -token.PreRotateZ;

            panControl.Position = token.Offset;
            rollControl.Angle = (token.PostRotateZ - t) * 180 / Math.PI;
            rollControl.RollDirection = 180 * t / Math.PI;
            rollControl.RollAmount = r;
            keepBackgroundCheckBox.Checked = token.SourceAsBackground;
            tileSourceCheckBox.Checked = token.Tile;
            trackBarZoom.Value = (int)Math.Round(512 + 128 * Math.Log(token.Zoom, 2.0));

            TrackBarZoom_ValueChanged(this, EventArgs.Empty);
        }

        protected override void InitTokenFromDialog()
        {
            RotateZoomEffectConfigToken token = (RotateZoomEffectConfigToken)theEffectToken;
            double angle = rollControl.RollDirection * Math.PI / 180;
            double dist = rollControl.RollAmount;

            if (double.IsNaN(angle))
            {
                angle = 0;
                dist = 0;
            }

            token.Offset = panControl.Position;
            token.PreRotateZ = (float)(angle);
            token.PostRotateZ = (float)(-angle - rollControl.Angle * Math.PI / 180);
            token.Tilt = (float)Math.Asin(dist / 90);
            token.SourceAsBackground = keepBackgroundCheckBox.Checked;
            token.Tile = tileSourceCheckBox.Checked;
            token.Zoom = (float)Math.Pow(2.0, (trackBarZoom.Value - 512) / 128.0);
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

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.keepBackgroundCheckBox = new System.Windows.Forms.CheckBox();
            this.tileSourceCheckBox = new System.Windows.Forms.CheckBox();
            this.rollControl = new PaintDotNet.Effects.RollControl();
            this.headerRoll = new PaintDotNet.HeaderLabel();
            this.headerPan = new PaintDotNet.HeaderLabel();
            this.panelPan = new System.Windows.Forms.Panel();
            this.panControl = new PaintDotNet.Effects.PanControl();
            this.headerZoom = new PaintDotNet.HeaderLabel();
            this.trackBarZoom = new System.Windows.Forms.TrackBar();
            this.zoomLabel = new System.Windows.Forms.Label();
            this.panXLabel = new System.Windows.Forms.Label();
            this.panYLabel = new System.Windows.Forms.Label();
            this.panXUpDown = new System.Windows.Forms.NumericUpDown();
            this.panYUpDown = new System.Windows.Forms.NumericUpDown();
            this.panResetButton = new System.Windows.Forms.Button();
            this.angleLabel = new System.Windows.Forms.Label();
            this.angleUpDown = new System.Windows.Forms.NumericUpDown();
            this.zoomResetButton = new System.Windows.Forms.Button();
            this.twistAngleLabel = new System.Windows.Forms.Label();
            this.twistRadiusLabel = new System.Windows.Forms.Label();
            this.twistAngleUpDown = new System.Windows.Forms.NumericUpDown();
            this.twistRadiusUpDown = new System.Windows.Forms.NumericUpDown();
            this.rollResetButton = new System.Windows.Forms.Button();
            this.resetAllButton = new System.Windows.Forms.Button();
            this.fineTuningHeader = new PaintDotNet.HeaderLabel();
            this.headerLabel1 = new PaintDotNet.HeaderLabel();
            this.panelPan.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarZoom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panXUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panYUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.angleUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.twistAngleUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.twistRadiusUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.okButton.Location = new System.Drawing.Point(312, 312);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(72, 23);
            this.okButton.TabIndex = 26;
            this.okButton.Click += new System.EventHandler(this.OkButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cancelButton.Location = new System.Drawing.Point(392, 312);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(72, 23);
            this.cancelButton.TabIndex = 27;
            this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // keepBackgroundCheckBox
            // 
            this.keepBackgroundCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.keepBackgroundCheckBox.Location = new System.Drawing.Point(9, 316);
            this.keepBackgroundCheckBox.Name = "keepBackgroundCheckBox";
            this.keepBackgroundCheckBox.Width = 175;
            this.keepBackgroundCheckBox.TabIndex = 24;
            this.keepBackgroundCheckBox.FlatStyle = FlatStyle.System;
            this.keepBackgroundCheckBox.CheckedChanged += new System.EventHandler(this.KeepBackgroundCheckBox_CheckedChanged);
            // 
            // tileSourceCheckBox
            // 
            this.tileSourceCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.tileSourceCheckBox.Location = new System.Drawing.Point(9, 298);
            this.tileSourceCheckBox.Name = "tileSourceCheckBox";
            this.tileSourceCheckBox.Width = 175;
            this.tileSourceCheckBox.TabIndex = 23;
            this.tileSourceCheckBox.FlatStyle = FlatStyle.System;   
            this.tileSourceCheckBox.CheckedChanged += new System.EventHandler(this.TileSource_CheckedChanged);
            // 
            // rollControl
            // 
            this.rollControl.Angle = -70;
            this.rollControl.Location = new System.Drawing.Point(16, 32);
            this.rollControl.Name = "rollControl";
            this.rollControl.RollAmount = 0;
            this.rollControl.RollDirection = 0;
            this.rollControl.Size = new System.Drawing.Size(112, 120);
            this.rollControl.TabIndex = 3;
            this.rollControl.TabStop = false;
            this.rollControl.ValueChanged += new System.EventHandler(this.RollControl_ValueChanged);
            // 
            // headerRoll
            // 
            this.headerRoll.Location = new System.Drawing.Point(8, 8);
            this.headerRoll.Name = "headerRoll";
            this.headerRoll.RightMargin = 0;
            this.headerRoll.Size = new System.Drawing.Size(168, 14);
            this.headerRoll.TabIndex = 2;
            this.headerRoll.TabStop = false;
            // 
            // headerPan
            // 
            this.headerPan.Location = new System.Drawing.Point(199, 8);
            this.headerPan.Name = "headerPan";
            this.headerPan.RightMargin = 0;
            this.headerPan.Size = new System.Drawing.Size(129, 14);
            this.headerPan.TabIndex = 5;
            this.headerPan.TabStop = false;
            // 
            // panelPan
            // 
            this.panelPan.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panelPan.Controls.Add(this.panControl);
            this.panelPan.Location = new System.Drawing.Point(200, 29);
            this.panelPan.Name = "panelPan";
            this.panelPan.Size = new System.Drawing.Size(128, 120);
            this.panelPan.TabIndex = 6;
            // 
            // panControl
            // 
            this.panControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panControl.Location = new System.Drawing.Point(0, 0);
            this.panControl.Name = "panControl";
            this.panControl.Size = new System.Drawing.Size(124, 116);
            this.panControl.TabIndex = 0;
            this.panControl.TabStop = false;
            this.panControl.PositionChanged += new System.EventHandler(this.PanControl_PositionChanged);
            // 
            // headerZoom
            // 
            this.headerZoom.Location = new System.Drawing.Point(352, 8);
            this.headerZoom.Name = "headerZoom";
            this.headerZoom.RightMargin = 0;
            this.headerZoom.Size = new System.Drawing.Size(112, 14);
            this.headerZoom.TabIndex = 7;
            this.headerZoom.TabStop = false;
            // 
            // trackBarZoom
            // 
            this.trackBarZoom.Location = new System.Drawing.Point(352, 24);
            this.trackBarZoom.Maximum = 1024;
            this.trackBarZoom.Name = "trackBarZoom";
            this.trackBarZoom.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.trackBarZoom.Size = new System.Drawing.Size(42, 131);
            this.trackBarZoom.TabIndex = 8;
            this.trackBarZoom.TickFrequency = 64;
            this.trackBarZoom.Value = 512;
            this.trackBarZoom.ValueChanged += new System.EventHandler(this.TrackBarZoom_ValueChanged);
            // 
            // zoomLabel
            // 
            this.zoomLabel.Location = new System.Drawing.Point(400, 32);
            this.zoomLabel.Name = "zoomLabel";
            this.zoomLabel.AutoSize = true;
            this.zoomLabel.Width = 48;
            this.zoomLabel.TabIndex = 9;
            // 
            // panXLabel
            // 
            this.panXLabel.Location = new System.Drawing.Point(200, 208);
            this.panXLabel.Name = "panXLabel";
            this.panXLabel.AutoSize = true;
            this.panXLabel.Width = 56;
            this.panXLabel.TabIndex = 18;
            // 
            // panYLabel
            // 
            this.panYLabel.Location = new System.Drawing.Point(200, 232);
            this.panYLabel.Name = "panYLabel";
            this.panYLabel.AutoSize = true;
            this.panYLabel.Width = 56;
            this.panYLabel.TabIndex = 19;
            // 
            // panXUpDown
            // 
            this.panXUpDown.DecimalPlaces = 3;
            this.panXUpDown.Increment = new System.Decimal(new int[] {
                                                                         1,
                                                                         0,
                                                                         0,
                                                                         131072});
            this.panXUpDown.Location = new System.Drawing.Point(260, 204);
            this.panXUpDown.Maximum = new System.Decimal(new int[] {
                                                                       1000000000,
                                                                       0,
                                                                       0,
                                                                       0});
            this.panXUpDown.Minimum = new System.Decimal(new int[] {
                                                                       100000000,
                                                                       0,
                                                                       0,
                                                                       -2147483648});
            this.panXUpDown.Name = "panXUpDown";
            this.panXUpDown.Size = new System.Drawing.Size(68, 20);
            this.panXUpDown.TabIndex = 20;
            this.panXUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.panXUpDown.Enter += new System.EventHandler(this.NumericUpDown_Enter);
            this.panXUpDown.ValueChanged += new System.EventHandler(this.PanXUpDown_ValueChanged);
            this.panXUpDown.Leave += new System.EventHandler(this.NumericUpDown_Leave);
            // 
            // panYUpDown
            // 
            this.panYUpDown.DecimalPlaces = 3;
            this.panYUpDown.Increment = new System.Decimal(new int[] {
                                                                         1,
                                                                         0,
                                                                         0,
                                                                         131072});
            this.panYUpDown.Location = new System.Drawing.Point(260, 228);
            this.panYUpDown.Maximum = new System.Decimal(new int[] {
                                                                       1000000000,
                                                                       0,
                                                                       0,
                                                                       0});
            this.panYUpDown.Minimum = new System.Decimal(new int[] {
                                                                       1000000000,
                                                                       0,
                                                                       0,
                                                                       -2147483648});
            this.panYUpDown.Name = "panYUpDown";
            this.panYUpDown.Size = new System.Drawing.Size(68, 20);
            this.panYUpDown.TabIndex = 21;
            this.panYUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.panYUpDown.Enter += new System.EventHandler(this.NumericUpDown_Enter);
            this.panYUpDown.ValueChanged += new System.EventHandler(this.PanYUpDown_ValueChanged);
            this.panYUpDown.Leave += new System.EventHandler(this.NumericUpDown_Leave);
            // 
            // panResetButton
            // 
            this.panResetButton.Location = new System.Drawing.Point(248, 160);
            this.panResetButton.Name = "panResetButton";
            this.panResetButton.Size = new System.Drawing.Size(80, 23);
            this.panResetButton.TabIndex = 6;
            this.panResetButton.FlatStyle = FlatStyle.System;
            this.panResetButton.Click += new System.EventHandler(this.PanResetButton_Click);
            // 
            // angleLabel
            // 
            this.angleLabel.Location = new System.Drawing.Point(8, 208);
            this.angleLabel.Name = "angleLabel";
            this.angleLabel.AutoSize = true;
            this.angleLabel.Width = 88;
            this.angleLabel.TabIndex = 12;
            // 
            // angleUpDown
            // 
            this.angleUpDown.DecimalPlaces = 2;
            this.angleUpDown.Location = new System.Drawing.Point(108, 204);
            this.angleUpDown.Maximum = new System.Decimal(new int[] {
                                                                        360,
                                                                        0,
                                                                        0,
                                                                        0});
            this.angleUpDown.Minimum = new System.Decimal(new int[] {
                                                                        360,
                                                                        0,
                                                                        0,
                                                                        -2147483648});
            this.angleUpDown.Name = "angleUpDown";
            this.angleUpDown.Size = new System.Drawing.Size(68, 20);
            this.angleUpDown.TabIndex = 13;
            this.angleUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.angleUpDown.Enter += new System.EventHandler(this.NumericUpDown_Enter);
            this.angleUpDown.ValueChanged += new System.EventHandler(this.AngleUpDown_ValueChanged);
            this.angleUpDown.Leave += new System.EventHandler(this.NumericUpDown_Leave);
            // 
            // zoomResetButton
            // 
            this.zoomResetButton.Location = new System.Drawing.Point(384, 160);
            this.zoomResetButton.Name = "zoomResetButton";
            this.zoomResetButton.Size = new System.Drawing.Size(80, 23);
            this.zoomResetButton.TabIndex = 10;
            this.zoomResetButton.FlatStyle = FlatStyle.System;
            this.zoomResetButton.Click += new System.EventHandler(this.ZoomResetButton_Click);
            // 
            // twistAngleLabel
            // 
            this.twistAngleLabel.Location = new System.Drawing.Point(8, 232);
            this.twistAngleLabel.Name = "twistAngleLabel";
            this.twistAngleLabel.AutoSize = true;
            this.twistAngleLabel.Width = 88;
            this.twistAngleLabel.TabIndex = 14;
            // 
            // twistRadiusLabel
            // 
            this.twistRadiusLabel.Location = new System.Drawing.Point(8, 256);
            this.twistRadiusLabel.Name = "twistRadiusLabel";
            this.twistRadiusLabel.AutoSize = true;
            this.twistRadiusLabel.Width = 88;
            this.twistRadiusLabel.TabIndex = 16;
            // 
            // twistAngleUpDown
            // 
            this.twistAngleUpDown.DecimalPlaces = 2;
            this.twistAngleUpDown.Location = new System.Drawing.Point(108, 228);
            this.twistAngleUpDown.Maximum = new System.Decimal(new int[] {
                                                                             360,
                                                                             0,
                                                                             0,
                                                                             0});
            this.twistAngleUpDown.Minimum = new System.Decimal(new int[] {
                                                                             360,
                                                                             0,
                                                                             0,
                                                                             -2147483648});
            this.twistAngleUpDown.Name = "twistAngleUpDown";
            this.twistAngleUpDown.Size = new System.Drawing.Size(68, 20);
            this.twistAngleUpDown.TabIndex = 15;
            this.twistAngleUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.twistAngleUpDown.Enter += new System.EventHandler(this.NumericUpDown_Enter);
            this.twistAngleUpDown.ValueChanged += new System.EventHandler(this.TwistAngleUpDown_ValueChanged);
            this.twistAngleUpDown.Leave += new System.EventHandler(this.NumericUpDown_Leave);
            // 
            // twistRadiusUpDown
            // 
            this.twistRadiusUpDown.DecimalPlaces = 2;
            this.twistRadiusUpDown.Location = new System.Drawing.Point(108, 252);
            this.twistRadiusUpDown.Maximum = new System.Decimal(new int[] {
                                                                              8995,
                                                                              0,
                                                                              0,
                                                                              131072});
            this.twistRadiusUpDown.Name = "twistRadiusUpDown";
            this.twistRadiusUpDown.Size = new System.Drawing.Size(68, 20);
            this.twistRadiusUpDown.TabIndex = 17;
            this.twistRadiusUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.twistRadiusUpDown.Enter += new System.EventHandler(this.NumericUpDown_Enter);
            this.twistRadiusUpDown.ValueChanged += new System.EventHandler(this.TwistRadiusUpDown_ValueChanged);
            this.twistRadiusUpDown.Leave += new System.EventHandler(this.NumericUpDown_Leave);
            // 
            // rollResetButton
            // 
            this.rollResetButton.Location = new System.Drawing.Point(96, 160);
            this.rollResetButton.Name = "rollResetButton";
            this.rollResetButton.Size = new System.Drawing.Size(80, 23);
            this.rollResetButton.TabIndex = 4;
            this.rollResetButton.FlatStyle = FlatStyle.System;
            this.rollResetButton.Click += new System.EventHandler(this.RollResetButton_Click);
            // 
            // resetAllButton
            // 
            this.resetAllButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.resetAllButton.Location = new System.Drawing.Point(200, 312);
            this.resetAllButton.Name = "resetAllButton";
            this.resetAllButton.Size = new System.Drawing.Size(104, 23);
            this.resetAllButton.TabIndex = 25;
            this.resetAllButton.FlatStyle = FlatStyle.System;
            this.resetAllButton.Click += new System.EventHandler(this.ResetAllButton_Click);
            // 
            // fineTuningHeader
            // 
            this.fineTuningHeader.Location = new System.Drawing.Point(8, 184);
            this.fineTuningHeader.Name = "fineTuningHeader";
            this.fineTuningHeader.Size = new System.Drawing.Size(464, 14);
            this.fineTuningHeader.TabIndex = 11;
            this.fineTuningHeader.TabStop = false;
            // 
            // headerLabel1
            // 
            this.headerLabel1.Location = new System.Drawing.Point(8, 280);
            this.headerLabel1.Name = "headerLabel1";
            this.headerLabel1.Size = new System.Drawing.Size(464, 14);
            this.headerLabel1.TabIndex = 22;
            this.headerLabel1.TabStop = false;
            // 
            // RotateZoomEffectConfigDialog
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(474, 343);
            this.Controls.Add(this.headerLabel1);
            this.Controls.Add(this.fineTuningHeader);
            this.Controls.Add(this.resetAllButton);
            this.Controls.Add(this.rollResetButton);
            this.Controls.Add(this.twistRadiusUpDown);
            this.Controls.Add(this.twistAngleUpDown);
            this.Controls.Add(this.twistRadiusLabel);
            this.Controls.Add(this.twistAngleLabel);
            this.Controls.Add(this.zoomResetButton);
            this.Controls.Add(this.angleUpDown);
            this.Controls.Add(this.angleLabel);
            this.Controls.Add(this.panResetButton);
            this.Controls.Add(this.panYUpDown);
            this.Controls.Add(this.panXUpDown);
            this.Controls.Add(this.panYLabel);
            this.Controls.Add(this.panXLabel);
            this.Controls.Add(this.zoomLabel);
            this.Controls.Add(this.trackBarZoom);
            this.Controls.Add(this.panelPan);
            this.Controls.Add(this.headerPan);
            this.Controls.Add(this.headerRoll);
            this.Controls.Add(this.keepBackgroundCheckBox);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.tileSourceCheckBox);
            this.Controls.Add(this.rollControl);
            this.Controls.Add(this.headerZoom);
            this.Location = new System.Drawing.Point(0, 0);
            this.Name = "RotateZoomEffectConfigDialog";
            this.Controls.SetChildIndex(this.headerZoom, 0);
            this.Controls.SetChildIndex(this.rollControl, 0);
            this.Controls.SetChildIndex(this.tileSourceCheckBox, 0);
            this.Controls.SetChildIndex(this.okButton, 0);
            this.Controls.SetChildIndex(this.cancelButton, 0);
            this.Controls.SetChildIndex(this.keepBackgroundCheckBox, 0);
            this.Controls.SetChildIndex(this.headerRoll, 0);
            this.Controls.SetChildIndex(this.headerPan, 0);
            this.Controls.SetChildIndex(this.panelPan, 0);
            this.Controls.SetChildIndex(this.trackBarZoom, 0);
            this.Controls.SetChildIndex(this.zoomLabel, 0);
            this.Controls.SetChildIndex(this.panXLabel, 0);
            this.Controls.SetChildIndex(this.panYLabel, 0);
            this.Controls.SetChildIndex(this.panXUpDown, 0);
            this.Controls.SetChildIndex(this.panYUpDown, 0);
            this.Controls.SetChildIndex(this.panResetButton, 0);
            this.Controls.SetChildIndex(this.angleLabel, 0);
            this.Controls.SetChildIndex(this.angleUpDown, 0);
            this.Controls.SetChildIndex(this.zoomResetButton, 0);
            this.Controls.SetChildIndex(this.twistAngleLabel, 0);
            this.Controls.SetChildIndex(this.twistRadiusLabel, 0);
            this.Controls.SetChildIndex(this.twistAngleUpDown, 0);
            this.Controls.SetChildIndex(this.twistRadiusUpDown, 0);
            this.Controls.SetChildIndex(this.rollResetButton, 0);
            this.Controls.SetChildIndex(this.resetAllButton, 0);
            this.Controls.SetChildIndex(this.fineTuningHeader, 0);
            this.Controls.SetChildIndex(this.headerLabel1, 0);
            this.panelPan.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.trackBarZoom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panXUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panYUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.angleUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.twistAngleUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.twistRadiusUpDown)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion


        private void OkButton_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void CancelButton_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void KeepBackgroundCheckBox_CheckedChanged(object sender, System.EventArgs e)
        {
            FinishTokenUpdate();
        }

        private void RollControl_ValueChanged(object sender, System.EventArgs e)
        {
            if (this.angleUpDown.Value != (decimal)this.rollControl.Angle)
            {
                this.angleUpDown.Value = (decimal)this.rollControl.Angle;
            }

            if (this.twistAngleUpDown.Value != -(decimal)this.rollControl.RollDirection)
            {
                this.twistAngleUpDown.Value = -(decimal)this.rollControl.RollDirection;
            }

            if (this.twistRadiusUpDown.Value != (decimal)this.rollControl.RollAmount)
            {
                this.twistRadiusUpDown.Value = (decimal)this.rollControl.RollAmount;
            }

            UpdateUpDowns();
            FinishTokenUpdate();
        }

        private void PanControl_PositionChanged(object sender, System.EventArgs e)
        {
            if (panXUpDown.Value != (decimal)panControl.Position.X)
            {
                panXUpDown.Value = (decimal)panControl.Position.X;
            }

            if (panYUpDown.Value != (decimal)panControl.Position.Y)
            {
                panYUpDown.Value = (decimal)panControl.Position.Y;
            }

            UpdateUpDowns();
            FinishTokenUpdate();
        }

        private void TrackBarZoom_ValueChanged(object sender, System.EventArgs e)
        {
            FinishTokenUpdate();
            string zoomTextFormat = PdnResources.GetString("RotateZoomEffectConfigDialog.ZoomLabel.Text.Format");
            string zoomText = string.Format(zoomTextFormat, ((RotateZoomEffectConfigToken)theEffectToken).Zoom.ToString("F2"));
            this.zoomLabel.Text = zoomText;
            UpdateUpDowns();
        }

        private void TileSource_CheckedChanged(object sender, System.EventArgs e)
        {
            FinishTokenUpdate();
        }

        private void NumericUpDown_Enter(object sender, System.EventArgs e)
        {
            NumericUpDown nud = (NumericUpDown)sender;
            nud.Select(0, nud.Text.Length);
        }

        private void NumericUpDown_Leave(object sender, System.EventArgs e)
        {
            NumericUpDown nud = (NumericUpDown)sender;
            Utility.ClipNumericUpDown(nud);

            if (Utility.CheckNumericUpDown(nud))
            {
                nud.Value = decimal.Parse(nud.Text);
            }
        }

        private void PanXUpDown_ValueChanged(object sender, System.EventArgs e)
        {
            if (this.panControl.Position.X != (float)panXUpDown.Value)
            {
                this.panControl.Position = new PointF((float)panXUpDown.Value, this.panControl.Position.Y);
                FinishTokenUpdate();
            }
        }

        private void PanYUpDown_ValueChanged(object sender, System.EventArgs e)
        {
            if (this.panControl.Position.Y != (float)panYUpDown.Value)
            {
                this.panControl.Position = new PointF(this.panControl.Position.X, (float)panYUpDown.Value);
                FinishTokenUpdate();
            }
        }

        private void PanResetButton_Click(object sender, System.EventArgs e)
        {
            panXUpDown.Value = 0;
            panYUpDown.Value = 0;
        }

        private void AngleUpDown_ValueChanged(object sender, System.EventArgs e)
        {
            if (this.rollControl.Angle != (double)angleUpDown.Value)
            {
                this.rollControl.Angle = (double)angleUpDown.Value;
                UpdateUpDowns();
                FinishTokenUpdate();
            }
        }

        private void ZoomResetButton_Click(object sender, System.EventArgs e)
        {
            this.trackBarZoom.Value = 512; // 1.00
        }

        private void TwistAngleUpDown_ValueChanged(object sender, System.EventArgs e)
        {
            if (this.rollControl.RollDirection != -(float)this.twistAngleUpDown.Value)
            {
                this.rollControl.RollDirection = -(float)this.twistAngleUpDown.Value;
                FinishTokenUpdate();
            }
        }

        private void TwistRadiusUpDown_ValueChanged(object sender, System.EventArgs e)
        {
            if (this.rollControl.RollAmount != (float)this.twistRadiusUpDown.Value)
            {
                this.rollControl.RollAmount = (float)this.twistRadiusUpDown.Value;
                FinishTokenUpdate();
            }
        }

        private void RollResetButton_Click(object sender, System.EventArgs e)
        {
            this.rollControl.Angle = 0.0;
            this.rollControl.RollAmount = 0;
            this.rollControl.RollDirection = 0;
        }

        private void UpdateUpDowns()
        {
            this.twistAngleUpDown.Update();
            this.twistRadiusUpDown.Update();
            this.angleUpDown.Update();
            this.panXUpDown.Update();
            this.panYUpDown.Update();
        }

        private void ResetAllButton_Click(object sender, System.EventArgs e)
        {
            this.panResetButton.PerformClick();
            this.zoomResetButton.PerformClick();
            this.rollResetButton.PerformClick();
        }
    }
}
