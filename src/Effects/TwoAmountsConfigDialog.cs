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
    public sealed class TwoAmountsConfigDialog
        : TwoAmountsConfigDialogBase
    {
        public TwoAmountsConfigDialog()
        {
        }
    }

    public abstract class TwoAmountsConfigDialogBase
        : EffectConfigDialog
    {
        private System.Windows.Forms.TrackBar amount1Slider;
        private System.Windows.Forms.NumericUpDown amount1UpDown;
        protected System.Windows.Forms.Button okButton;
        protected System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.NumericUpDown amount2UpDown;
        private System.Windows.Forms.TrackBar amount2Slider;
        private System.Windows.Forms.Button amount2Reset;
        private System.Windows.Forms.Button amount1Reset;
        private System.ComponentModel.IContainer components = null;

        private int amount1Default = 0;
        private PaintDotNet.HeaderLabel amount1Header;
        private PaintDotNet.HeaderLabel amount2Header;
        private int amount2Default = 0;

        public int Amount1Default
        {
            get
            {
                return amount1Default;
            }

            set
            {
                amount1Default = value;
                amount1Slider.Value = value;
                InitTokenFromDialog();
            }
        }

        public int Amount1Minimum
        {
            get
            {
                return amount1Slider.Minimum;
            }

            set
            {
                amount1Slider.Minimum = value;
                amount1UpDown.Minimum = (decimal)value;
                InitTokenFromDialog();
            }
        }

        public int Amount1Maximum
        {
            get
            {
                return amount1Slider.Maximum;
            }

            set
            {
                amount1Slider.Maximum = value;
                amount1UpDown.Maximum = (decimal)value;
                InitTokenFromDialog();
            }
        }

        public string Amount1Label
        {
            get
            {
                return amount1Header.Text;
            }

            set
            {
                amount1Header.Text = value;
            }
        }

        public int Amount2Default
        {
            get
            {
                return amount2Default;
            }

            set
            {
                amount2Default = value;
                amount2Slider.Value = value;
                InitTokenFromDialog();
            }
        }

        public int Amount2Minimum
        {
            get
            {
                return amount2Slider.Minimum;
            }

            set
            {
                amount2Slider.Minimum = value;
                amount2UpDown.Minimum = (decimal)value;
                InitTokenFromDialog();
            }
        }

        public int Amount2Maximum
        {
            get
            {
                return amount2Slider.Maximum;
            }

            set
            {
                amount2Slider.Maximum = value;
                amount2UpDown.Maximum = (decimal)value;
                InitTokenFromDialog();
            }
        }

        public string Amount2Label
        {
            get
            {
                return amount2Header.Text;
            }

            set
            {
                amount2Header.Text = value;
            }
        }

        internal TwoAmountsConfigDialogBase()
        {
            // This call is required by the Windows Form Designer.
            InitializeComponent();

            this.okButton.Text = PdnResources.GetString("Form.OkButton.Text");
            this.cancelButton.Text = PdnResources.GetString("Form.CancelButton.Text");
            this.amount1Reset.Text = PdnResources.GetString("TwoAmountsConfigDialog.Reset.Text");
            this.amount2Reset.Text = PdnResources.GetString("TwoAmountsConfigDialog.Reset.Text");
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

        protected override void InitialInitToken()
        {
            theEffectToken = new TwoAmountsConfigToken(Amount1Default, Amount2Default);
        }

        protected override void InitDialogFromToken(EffectConfigToken effectToken)
        {
            amount1Slider.Value = ((TwoAmountsConfigToken)effectToken).Amount1;
            amount2Slider.Value = ((TwoAmountsConfigToken)effectToken).Amount2;                        
        }

        protected override void InitTokenFromDialog()
        {
            ((TwoAmountsConfigToken)theEffectToken).Amount1 = amount1Slider.Value;
            ((TwoAmountsConfigToken)theEffectToken).Amount2 = amount2Slider.Value;
        }

        #region Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.amount1Slider = new System.Windows.Forms.TrackBar();
            this.amount1UpDown = new System.Windows.Forms.NumericUpDown();
            this.amount1Reset = new System.Windows.Forms.Button();
            this.amount2Reset = new System.Windows.Forms.Button();
            this.amount2UpDown = new System.Windows.Forms.NumericUpDown();
            this.amount2Slider = new System.Windows.Forms.TrackBar();
            this.amount1Header = new PaintDotNet.HeaderLabel();
            this.amount2Header = new PaintDotNet.HeaderLabel();
            ((System.ComponentModel.ISupportInitialize)(this.amount1Slider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.amount1UpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.amount2UpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.amount2Slider)).BeginInit();
            this.SuspendLayout();
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.okButton.Location = new System.Drawing.Point(101, 151);
            this.okButton.Size = new System.Drawing.Size(81, 23);
            this.okButton.Name = "okButton";
            this.okButton.TabIndex = 6;
            this.okButton.Click += new System.EventHandler(this.OnOkButtonClicked);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cancelButton.Location = new System.Drawing.Point(188, 151);
            this.cancelButton.Size = new System.Drawing.Size(81, 23);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.TabIndex = 7;
            this.cancelButton.Click += new System.EventHandler(this.OnCancelButtonClicked);
            // 
            // amount1Slider
            // 
            this.amount1Slider.LargeChange = 20;
            this.amount1Slider.Location = new System.Drawing.Point(1, 25);
            this.amount1Slider.Maximum = 100;
            this.amount1Slider.Minimum = -100;
            this.amount1Slider.Name = "amount1Slider";
            this.amount1Slider.Size = new System.Drawing.Size(175, 42);
            this.amount1Slider.TabIndex = 0;
            this.amount1Slider.TickFrequency = 10;
            this.amount1Slider.ValueChanged += new System.EventHandler(this.amount1Slider_ValueChanged);
            // 
            // amount1UpDown
            // 
            this.amount1UpDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.amount1UpDown.Location = new System.Drawing.Point(188, 25);
            this.amount1UpDown.Minimum = new System.Decimal(new int[] {
                                                                          100,
                                                                          0,
                                                                          0,
                                                                          -2147483648});
            this.amount1UpDown.Name = "amount1UpDown";
            this.amount1UpDown.Size = new System.Drawing.Size(81, 20);
            this.amount1UpDown.TabIndex = 1;
            this.amount1UpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.amount1UpDown.Enter += new System.EventHandler(this.amount1UpDown_Enter);
            this.amount1UpDown.ValueChanged += new System.EventHandler(this.amount1UpDown_ValueChanged);
            this.amount1UpDown.Leave += new System.EventHandler(this.amount1UpDown_Leave);
            // 
            // amount1Reset
            // 
            this.amount1Reset.Location = new System.Drawing.Point(188, 50);
            this.amount1Reset.Name = "amount1Reset";
            this.amount1Reset.Size = new System.Drawing.Size(81, 20);
            this.amount1Reset.TabIndex = 2;
            this.amount1Reset.Click += new System.EventHandler(this.amount1Reset_Click);
            // 
            // amount2Reset
            // 
            this.amount2Reset.Location = new System.Drawing.Point(188, 120);
            this.amount2Reset.Name = "amount2Reset";
            this.amount2Reset.Size = new System.Drawing.Size(81, 20);
            this.amount2Reset.TabIndex = 5;
            this.amount2Reset.FlatStyle = FlatStyle.System;
            this.amount2Reset.Click += new System.EventHandler(this.amount2Reset_Click);
            // 
            // amount2UpDown
            // 
            this.amount2UpDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.amount2UpDown.Location = new System.Drawing.Point(188, 95);
            this.amount2UpDown.Minimum = new System.Decimal(new int[] {
                                                                          100,
                                                                          0,
                                                                          0,
                                                                          -2147483648});
            this.amount2UpDown.Name = "amount2UpDown";
            this.amount2UpDown.Size = new System.Drawing.Size(81, 20);
            this.amount2UpDown.TabIndex = 4;
            this.amount2UpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.amount2UpDown.Enter += new System.EventHandler(this.amount2UpDown_Enter);
            this.amount2UpDown.ValueChanged += new System.EventHandler(this.amount2UpDown_ValueChanged);
            this.amount2UpDown.Leave += new System.EventHandler(this.amount2UpDown_Leave);
            // 
            // amount2Slider
            // 
            this.amount2Slider.LargeChange = 20;
            this.amount2Slider.Location = new System.Drawing.Point(1, 95);
            this.amount2Slider.Maximum = 100;
            this.amount2Slider.Minimum = -100;
            this.amount2Slider.Name = "amount2Slider";
            this.amount2Slider.Size = new System.Drawing.Size(175, 42);
            this.amount2Slider.TabIndex = 3;
            this.amount2Slider.TickFrequency = 10;
            this.amount2Slider.ValueChanged += new System.EventHandler(this.amount2Slider_ValueChanged);
            // 
            // amount1Header
            // 
            this.amount1Header.Location = new System.Drawing.Point(6, 8);
            this.amount1Header.Name = "amount1Header";
            this.amount1Header.Size = new System.Drawing.Size(271, 14);
            this.amount1Header.TabIndex = 9;
            this.amount1Header.TabStop = false;
            this.amount1Header.Text = "Header 1";
            // 
            // amount2Header
            // 
            this.amount2Header.Location = new System.Drawing.Point(6, 78);
            this.amount2Header.Name = "amount2Header";
            this.amount2Header.Size = new System.Drawing.Size(271, 14);
            this.amount2Header.TabIndex = 10;
            this.amount2Header.TabStop = false;
            this.amount2Header.Text = "Header 2";
            // 
            // TwoAmountsConfigDialog
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(275, 180);
            this.Controls.Add(this.amount2Header);
            this.Controls.Add(this.amount1Header);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.amount2Reset);
            this.Controls.Add(this.amount2UpDown);
            this.Controls.Add(this.amount2Slider);
            this.Controls.Add(this.amount1Reset);
            this.Controls.Add(this.amount1UpDown);
            this.Controls.Add(this.amount1Slider);
            this.Location = new System.Drawing.Point(0, 0);
            this.Name = "TwoAmountsConfigDialog";
            this.Controls.SetChildIndex(this.amount1Slider, 0);
            this.Controls.SetChildIndex(this.amount1UpDown, 0);
            this.Controls.SetChildIndex(this.amount1Reset, 0);
            this.Controls.SetChildIndex(this.amount2Slider, 0);
            this.Controls.SetChildIndex(this.amount2UpDown, 0);
            this.Controls.SetChildIndex(this.amount2Reset, 0);
            this.Controls.SetChildIndex(this.okButton, 0);
            this.Controls.SetChildIndex(this.cancelButton, 0);
            this.Controls.SetChildIndex(this.amount1Header, 0);
            this.Controls.SetChildIndex(this.amount2Header, 0);
            ((System.ComponentModel.ISupportInitialize)(this.amount1Slider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.amount1UpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.amount2UpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.amount2Slider)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion

        protected override void OnLoad(EventArgs e)
        {
            amount1UpDown.Select();
            amount1UpDown.Select(0, amount1UpDown.Text.Length);
            base.OnLoad(e);
        }

        protected virtual void OnOkButtonClicked(object sender, System.EventArgs e)
        {
            amount1UpDown_ValueChanged(sender, e);
            amount2UpDown_ValueChanged(sender, e);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        protected virtual void OnCancelButtonClicked(object sender, System.EventArgs e)
        {
            this.Close();
        }

        private void amount1Slider_ValueChanged(object sender, System.EventArgs e)
        {
            if (amount1UpDown.Value != (decimal)amount1Slider.Value)
            {
                amount1UpDown.Value = (decimal)amount1Slider.Value;
                FinishTokenUpdate();
            }
        }

        private void amount1UpDown_ValueChanged(object sender, System.EventArgs e)
        {
            if (amount1Slider.Value != (int)amount1UpDown.Value)
            {
                amount1Slider.Value = (int)amount1UpDown.Value;
                FinishTokenUpdate();
            }
        }

        private void amount1UpDown_Enter(object sender, System.EventArgs e)
        {
            amount1UpDown.Select(0, amount1UpDown.Text.Length);        
        }

        private void amount1UpDown_Leave(object sender, System.EventArgs e)
        {
            Utility.ClipNumericUpDown(amount1UpDown);

            if (Utility.CheckNumericUpDown(amount1UpDown))
            {
                amount1UpDown.Value = decimal.Parse(amount1UpDown.Text);
            }
        }

        private void amount2Slider_ValueChanged(object sender, System.EventArgs e)
        {
            if (amount2UpDown.Value != (decimal)amount2Slider.Value)
            {
                amount2UpDown.Value = (decimal)amount2Slider.Value;
                FinishTokenUpdate();
            }
        }

        private void amount2UpDown_ValueChanged(object sender, System.EventArgs e)
        {
            if (amount2Slider.Value != (int)amount2UpDown.Value)
            {
                amount2Slider.Value = (int)amount2UpDown.Value;
                FinishTokenUpdate();
            }
        }

        private void amount2UpDown_Enter(object sender, System.EventArgs e)
        {
            amount2UpDown.Select(0, amount2UpDown.Text.Length);        
        }

        private void amount2UpDown_Leave(object sender, System.EventArgs e)
        {
            Utility.ClipNumericUpDown(amount2UpDown);

            if (Utility.CheckNumericUpDown(amount2UpDown))
            {
                amount2UpDown.Value = decimal.Parse(amount2UpDown.Text);
            }
        }

        private void amount2Reset_Click(object sender, System.EventArgs e)
        {
            this.amount2Slider.Value = amount2Default;
        }

        private void amount1Reset_Click(object sender, System.EventArgs e)
        {
            this.amount1Slider.Value = amount1Default;
        }
    }
}

