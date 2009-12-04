/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace PaintDotNet.Effects
{
    public sealed class CurvesEffectConfigDialog 
        : EffectConfigDialog
    {
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private CurveControl curveControl;
        private Dictionary<ColorTransferMode, CurveControl> curveControls;
        private System.ComponentModel.IContainer components = null;
        private PaintDotNet.HeaderLabel transferHeader;
        private System.Windows.Forms.Button resetButton;
        private System.Windows.Forms.ComboBox modeComboBox;
        private System.EventHandler curveControlValueChangedDelegate;
        private EventHandler<EventArgs<Point>> curveControlCoordinatesChangedDelegate;
        private TableLayoutPanel tableLayoutMain;
        private TableLayoutPanel tableLayoutPanelMask;
        private EnumLocalizer colorTransferNames;
        private CheckBox[] maskCheckBoxes;
        private EventHandler maskCheckChanged;
        private Label labelCoordinates;
        private Label labelHelpText;
        private bool finishTokenOnDropDownChanged = true;

        public CurvesEffectConfigDialog()
        {
            InitializeComponent();

            curveControlValueChangedDelegate = this.curveControl_ValueChanged;
            curveControlCoordinatesChangedDelegate = this.curveControl_CoordinatesChanged;
            colorTransferNames = EnumLocalizer.Create(typeof(ColorTransferMode));

            this.Text = PdnResources.GetString("CurvesEffectConfigDialog.Text");
            this.cancelButton.Text = PdnResources.GetString("Form.CancelButton.Text");
            this.okButton.Text = PdnResources.GetString("Form.OkButton.Text");
            this.resetButton.Text = PdnResources.GetString("CurvesEffectConfigDialog.ResetButton.Text");
            this.transferHeader.Text = PdnResources.GetString("CurvesEffectConfigDialog.TransferHeader.Text");
            this.labelHelpText.Text = PdnResources.GetString("CurvesEffectConfigDialog.HelpText.Text");
            this.modeComboBox.Items.Clear();
            this.modeComboBox.Items.AddRange(colorTransferNames.GetLocalizedNames());

            this.maskCheckChanged = new EventHandler(MaskCheckChanged);

            this.curveControls = new Dictionary<ColorTransferMode, CurveControl>();

            this.curveControls.Add(ColorTransferMode.Luminosity, new CurveControlLuminosity());
            this.curveControls.Add(ColorTransferMode.Rgb, new CurveControlRgb());
        }

        protected override void InitialInitToken()
        {
            CurvesEffectConfigToken token = new CurvesEffectConfigToken();
            theEffectToken = token;
        }

        protected override void InitTokenFromDialog()
        {
            ((CurvesEffectConfigToken)EffectToken).ColorTransferMode = curveControl.ColorTransferMode;
            ((CurvesEffectConfigToken)EffectToken).ControlPoints = (SortedList<int, int>[])curveControl.ControlPoints.Clone();
        }

        protected override void InitDialogFromToken(EffectConfigToken effectToken)
        {
            CurvesEffectConfigToken token = (CurvesEffectConfigToken)effectToken;

            bool oldValue = finishTokenOnDropDownChanged;
            finishTokenOnDropDownChanged = false;

            switch (token.ColorTransferMode)
            {
                case ColorTransferMode.Luminosity:
                    modeComboBox.SelectedItem = colorTransferNames.EnumValueToLocalizedName(ColorTransferMode.Luminosity);
                    break;

                case ColorTransferMode.Rgb:
                    modeComboBox.SelectedItem = colorTransferNames.EnumValueToLocalizedName(ColorTransferMode.Rgb);
                    break;
            }

            finishTokenOnDropDownChanged = oldValue;

            curveControl.ControlPoints = (SortedList<int, int>[])token.ControlPoints.Clone();
            curveControl.Invalidate();
            curveControl.Update();
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
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.transferHeader = new PaintDotNet.HeaderLabel();
            this.resetButton = new System.Windows.Forms.Button();
            this.modeComboBox = new System.Windows.Forms.ComboBox();
            this.labelCoordinates = new Label();
            this.labelHelpText = new Label();
            this.tableLayoutMain = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanelMask = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cancelButton.Location = new System.Drawing.Point(210, 292);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(81, 23);
            this.cancelButton.TabIndex = 5;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // okButton
            // 
            this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.okButton.Location = new System.Drawing.Point(130, 292);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(81, 23);
            this.okButton.TabIndex = 4;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // transferHeader
            // 
            this.tableLayoutMain.SetColumnSpan(this.transferHeader, 4);
            this.transferHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.transferHeader.Location = new System.Drawing.Point(9, 9);
            this.transferHeader.Name = "transferHeader";
            this.transferHeader.RightMargin = 0;
            this.transferHeader.Margin = new Padding(1, 3, 1, 1);
            this.transferHeader.Size = new System.Drawing.Size(115, 17);
            this.transferHeader.TabIndex = 20;
            this.transferHeader.TabStop = false;
            // 
            // resetButton
            // 
            this.resetButton.Location = new System.Drawing.Point(9, 292);
            this.resetButton.Name = "resetButton";
            this.resetButton.Size = new System.Drawing.Size(81, 23);
            this.resetButton.TabIndex = 3;
            this.resetButton.FlatStyle = FlatStyle.System;
            this.resetButton.Click += new System.EventHandler(this.resetButton_Click);
            // 
            // modeComboBox
            // 
            this.tableLayoutMain.SetColumnSpan(this.modeComboBox, 3);
            this.modeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.modeComboBox.Location = new System.Drawing.Point(130, 9);
            this.modeComboBox.Name = "modeComboBox";
            this.modeComboBox.Size = new System.Drawing.Size(90, 21);
            this.modeComboBox.TabIndex = 23;
            this.modeComboBox.SelectedIndexChanged += new System.EventHandler(this.modeComboBox_SelectedIndexChanged);
            //
            this.labelCoordinates.Dock = DockStyle.Fill;
            this.labelCoordinates.TextAlign = ContentAlignment.MiddleCenter;
            //
            this.tableLayoutMain.SetColumnSpan(this.labelHelpText, 4);
            this.labelHelpText.Dock = DockStyle.Fill;
            // 
            // tableLayoutMain
            // 
            this.tableLayoutMain.ColumnCount = 4;
            this.tableLayoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 88F));
            this.tableLayoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 74F));
            this.tableLayoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 74F));
            this.tableLayoutMain.Controls.Add(this.resetButton, 0, 5);
            this.tableLayoutMain.Controls.Add(this.okButton, 2, 5);
            this.tableLayoutMain.Controls.Add(this.cancelButton, 3, 5);
            this.tableLayoutMain.Controls.Add(this.transferHeader, 0, 0);
            this.tableLayoutMain.Controls.Add(this.tableLayoutPanelMask, 0, 3);
            this.tableLayoutMain.Controls.Add(this.modeComboBox, 0, 1);
            this.tableLayoutMain.Controls.Add(this.labelCoordinates, 3, 1);
            this.tableLayoutMain.Controls.Add(this.labelHelpText, 0, 4);
            this.tableLayoutMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutMain.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutMain.Name = "tableLayoutMain";
            this.tableLayoutMain.Padding = new System.Windows.Forms.Padding(6);
            this.tableLayoutMain.Margin = new Padding(2);
            this.tableLayoutMain.RowCount = 4;
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutMain.Size = new System.Drawing.Size(4,4);
            this.tableLayoutMain.TabIndex = 24;
            // 
            // tableLayoutPanelMask
            // 
            this.tableLayoutPanelMask.ColumnCount = 3;
            this.tableLayoutMain.SetColumnSpan(this.tableLayoutPanelMask, 4);
            this.tableLayoutPanelMask.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanelMask.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanelMask.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanelMask.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelMask.Location = new System.Drawing.Point(8, 262);
            this.tableLayoutPanelMask.Margin = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanelMask.Name = "tableLayoutPanelMask";
            this.tableLayoutPanelMask.RowCount = 1;
            this.tableLayoutPanelMask.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelMask.Size = new System.Drawing.Size(277, 25);
            this.tableLayoutPanelMask.TabIndex = 24;
            // 
            // CurvesEffectConfigDialog
            // 
            this.AcceptButton = this.okButton;
            this.CancelButton = this.cancelButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.ClientSize = new System.Drawing.Size(276, 382);
            this.MinimumSize = new Size(260, 276);
            this.Controls.Add(this.tableLayoutMain);
            this.Name = "CurvesEffectConfigDialog";
            this.Controls.SetChildIndex(this.tableLayoutMain, 0);
            this.tableLayoutMain.ResumeLayout(false);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.ResumeLayout(false);
        }
        #endregion

        protected override void OnLoad(EventArgs e)
        {
            this.okButton.Select();
            base.OnLoad(e);
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

        private void curveControl_ValueChanged(object sender, EventArgs e)
        {
            this.FinishTokenUpdate();
        }
        
        private void curveControl_CoordinatesChanged(object sender, EventArgs<Point> e)
        {
            Point pt = e.Data;
            string newText;

            if (pt.X >= 0)
            {
                string format = PdnResources.GetString("CurvesEffectConfigDialog.Coordinates.Format");
                newText = string.Format(format, pt.X, pt.Y);
            }
            else
            {
                newText = string.Empty;
            }

            if (newText != labelCoordinates.Text)
            {
                labelCoordinates.Text = newText;
                labelCoordinates.Update();
            }
        }

        private void resetButton_Click(object sender, System.EventArgs e)
        {
            curveControl.ResetControlPoints();
            this.FinishTokenUpdate();
        }

        private void MaskCheckChanged(object sender, System.EventArgs e)
        {
            for (int i = 0; i < maskCheckBoxes.Length; ++i)
            {
                if (maskCheckBoxes[i] == sender)
                {
                    curveControl.SetSelected(i, maskCheckBoxes[i].Checked);
                }
            }

            UpdateCheckboxEnables();
        }

        private void modeComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            CurveControl newCurveControl;
            ColorTransferMode colorTransferMode;

            if (modeComboBox.SelectedIndex >= 0)
            {
                colorTransferMode = (ColorTransferMode)colorTransferNames.LocalizedNameToEnumValue(modeComboBox.SelectedItem.ToString());
            }
            else
            {
                colorTransferMode = ColorTransferMode.Rgb;
            }

            newCurveControl = curveControls[colorTransferMode];

            if (curveControl != newCurveControl)
            {
                tableLayoutMain.Controls.Remove(curveControl);

                curveControl = newCurveControl;

                curveControl.Bounds = new Rectangle(0, 0, 258, 258);
                curveControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
                //curveControl.ResetControlPoints();
                tableLayoutMain.SetColumnSpan(this.curveControl, 4);
                curveControl.Dock = System.Windows.Forms.DockStyle.Fill;
                curveControl.ValueChanged += curveControlValueChangedDelegate;
                curveControl.CoordinatesChanged += curveControlCoordinatesChangedDelegate;
                tableLayoutMain.Controls.Add(curveControl, 0, 2);

                if (finishTokenOnDropDownChanged)
                {
                    FinishTokenUpdate();
                }

                int channels = newCurveControl.Channels;

                maskCheckBoxes = new CheckBox[channels];

                this.tableLayoutPanelMask.Controls.Clear();
                this.tableLayoutPanelMask.ColumnCount = channels;

                for (int i = 0; i < channels; ++i)
                {
                    CheckBox checkbox = new CheckBox();

                    checkbox.Dock = DockStyle.Fill;
                    checkbox.Checked = curveControl.GetSelected(i);
                    checkbox.CheckedChanged += maskCheckChanged;
                    checkbox.Text = curveControl.GetChannelName(i);
                    checkbox.FlatStyle = FlatStyle.System;

                    this.tableLayoutPanelMask.Controls.Add(checkbox, i, 0);
                    this.tableLayoutPanelMask.ColumnStyles[i].SizeType = SizeType.Percent;
                    this.tableLayoutPanelMask.ColumnStyles[i].Width = 100;
                    maskCheckBoxes[i] = checkbox;
                }

                UpdateCheckboxEnables();
            }
        }

        private void UpdateCheckboxEnables()
        {
            int countChecked = 0;

            for (int i = 0; i < maskCheckBoxes.Length; ++i)
            {
                if (maskCheckBoxes[i].Checked)
                {
                    ++countChecked;
                }
            }

            if (maskCheckBoxes.Length == 1)
            {
                maskCheckBoxes[0].Enabled = false;
            }
        }
    }
}
