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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace PaintDotNet.Effects
{
    public sealed class AmountEffectConfigDialog
        : AmountEffectConfigDialogBase
    {
        public AmountEffectConfigDialog()
        {
        }
    }

    public abstract class AmountEffectConfigDialogBase
        : EffectConfigDialog
    {
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TrackBar amountTrackBar;
        private System.Windows.Forms.NumericUpDown amountUpDown;
        private System.ComponentModel.IContainer components = null;
        private PaintDotNet.HeaderLabel headerLabel;
        public int sliderInitialValue = 2;

        internal AmountEffectConfigDialogBase()
        {
            // This call is required by the Windows Form Designer.
            InitializeComponent();

            this.cancelButton.Text = PdnResources.GetString("Form.CancelButton.Text");
            this.okButton.Text = PdnResources.GetString("Form.OkButton.Text");
        }

        public int SliderInitialValue
        {
            get
            {
                return sliderInitialValue;
            }

            set
            {
                this.sliderInitialValue = value;
                amountTrackBar.Value = value;
                amountUpDown.Value = value;
                FinishTokenUpdate();
            }
        }

        public int SliderMinimum
        {
            get
            {
                return amountTrackBar.Minimum;
            }

            set
            {
                amountTrackBar.Minimum = value;
                amountUpDown.Minimum = (decimal)value;
            }
        }

        public int SliderMaximum
        {
            get
            {
                return amountTrackBar.Maximum;
            }

            set
            {
                amountTrackBar.Maximum = value;
                amountUpDown.Maximum = (decimal)value;
            }
        }

        public string SliderLabel
        {
            get
            {
                return headerLabel.Text;
            }

            set
            {
                headerLabel.Text = value;
            }
        }

        public string SliderUnitsName
        {
            get
            {
                return label1.Text;
            }

            set
            {
                label1.Text = value;
            }
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
            this.amountTrackBar = new System.Windows.Forms.TrackBar();
            this.amountUpDown = new System.Windows.Forms.NumericUpDown();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.headerLabel = new PaintDotNet.HeaderLabel();
            ((System.ComponentModel.ISupportInitialize)(this.amountTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.amountUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // amountTrackBar
            // 
            this.amountTrackBar.AutoSize = false;
            this.amountTrackBar.Location = new System.Drawing.Point(3, 59);
            this.amountTrackBar.Maximum = 100;
            this.amountTrackBar.Minimum = 1;
            this.amountTrackBar.Name = "amountTrackBar";
            this.amountTrackBar.Size = new System.Drawing.Size(174, 24);
            this.amountTrackBar.TabIndex = 1;
            this.amountTrackBar.TickFrequency = 10;
            this.amountTrackBar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.amountTrackBar.Value = 1;
            this.amountTrackBar.ValueChanged += new System.EventHandler(this.amountTrackBar_ValueChanged);
            // 
            // amountUpDown
            // 
            this.amountUpDown.Location = new System.Drawing.Point(16, 32);
            this.amountUpDown.Minimum = new System.Decimal(new int[] {
                                                                         1,
                                                                         0,
                                                                         0,
                                                                         0});
            this.amountUpDown.Name = "amountUpDown";
            this.amountUpDown.Size = new System.Drawing.Size(64, 20);
            this.amountUpDown.TabIndex = 0;
            this.amountUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.amountUpDown.Value = new System.Decimal(new int[] {
                                                                       1,
                                                                       0,
                                                                       0,
                                                                       0});
            this.amountUpDown.Enter += new System.EventHandler(this.amountUpDown_Enter);
            this.amountUpDown.ValueChanged += new System.EventHandler(this.amountUpDown_ValueChanged);
            this.amountUpDown.Leave += new System.EventHandler(this.amountUpDown_Leave);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cancelButton.Location = new System.Drawing.Point(96, 93);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.TabIndex = 3;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.okButton.Location = new System.Drawing.Point(15, 93);
            this.okButton.Name = "okButton";
            this.okButton.TabIndex = 2;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(82, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 24);
            this.label1.TabIndex = 2;
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // headerLabel
            // 
            this.headerLabel.Location = new System.Drawing.Point(6, 8);
            this.headerLabel.Name = "headerLabel";
            this.headerLabel.Size = new System.Drawing.Size(170, 14);
            this.headerLabel.TabIndex = 7;
            this.headerLabel.TabStop = false;
            this.headerLabel.Text = "Header";
            // 
            // AmountEffectConfigDialog
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(177, 122);
            this.Controls.Add(this.headerLabel);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.amountTrackBar);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.amountUpDown);
            this.Location = new System.Drawing.Point(0, 0);
            this.Name = "AmountEffectConfigDialog";
            this.Controls.SetChildIndex(this.amountUpDown, 0);
            this.Controls.SetChildIndex(this.label1, 0);
            this.Controls.SetChildIndex(this.amountTrackBar, 0);
            this.Controls.SetChildIndex(this.okButton, 0);
            this.Controls.SetChildIndex(this.cancelButton, 0);
            this.Controls.SetChildIndex(this.headerLabel, 0);
            ((System.ComponentModel.ISupportInitialize)(this.amountTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.amountUpDown)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion

        protected override void InitialInitToken()
        {
            theEffectToken = new AmountEffectConfigToken(this.sliderInitialValue);
        }

        protected override void InitDialogFromToken(EffectConfigToken effectToken)
        {
            this.amountTrackBar.Value = ((AmountEffectConfigToken)effectToken).Amount;
        }

        protected override void InitTokenFromDialog()
        {
            ((AmountEffectConfigToken)theEffectToken).Amount = amountTrackBar.Value;
        }

        private void amountTrackBar_ValueChanged(object sender, System.EventArgs e)
        {
            if (amountTrackBar.Value != (int)amountUpDown.Value)
            {
                amountUpDown.Value = amountTrackBar.Value;
                FinishTokenUpdate();
            }
        }

        private void amountUpDown_ValueChanged(object sender, System.EventArgs e)
        {
            if (amountTrackBar.Value != (int)amountUpDown.Value)
            {
                amountTrackBar.Value = (int)amountUpDown.Value;
                FinishTokenUpdate();
            }
        }

        private void okButton_Click(object sender, System.EventArgs e)
        {
            // if the user types, then presses Enter or clicks OK, this will make sure we take what they typed and not the value of the trackbar            
            amountUpDown_Leave(sender, e);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void cancelButton_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }

        private void amountUpDown_Enter(object sender, System.EventArgs e)
        {
            amountUpDown.Select(0,amountUpDown.Text.Length);
        }

        private void amountUpDown_Leave(object sender, System.EventArgs e)
        {
            Utility.ClipNumericUpDown(amountUpDown);

            if (Utility.CheckNumericUpDown(amountUpDown))
            {
                amountUpDown.Value = decimal.Parse(amountUpDown.Text);
            }
        }
    }
}

