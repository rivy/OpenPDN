/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;

namespace PaintDotNet.Effects
{
    public sealed class ThreeAmountsConfigDialog
        : ThreeAmountsConfigDialogBase
    {
        public ThreeAmountsConfigDialog()
        {
        }
    }

    public abstract class ThreeAmountsConfigDialogBase
        : TwoAmountsConfigDialogBase
    {
        private System.Windows.Forms.Button amount3Reset;
        private System.Windows.Forms.NumericUpDown amount3UpDown;
        private System.Windows.Forms.TrackBar amount3Slider;
        private PaintDotNet.HeaderLabel amount3Header;
    
        private int amount3Default = 0;

        public int Amount3Default
        {
            get
            {
                return amount3Default;
            }

            set
            {
                amount3Default = value;
                amount3Slider.Value = value;
                InitTokenFromDialog();
            }
        }

        public int Amount3Minimum
        {
            get
            {
                return amount3Slider.Minimum;
            }

            set
            {
                amount3Slider.Minimum = value;
                amount3UpDown.Minimum = (decimal)value;
                InitTokenFromDialog();
            }
        }

        public int Amount3Maximum
        {
            get
            {
                return amount3Slider.Maximum;
            }

            set
            {
                amount3Slider.Maximum = value;
                amount3UpDown.Maximum = (decimal)value;
                InitTokenFromDialog();
            }
        }

        public string Amount3Label
        {
            get
            {
                return amount3Header.Text;
            }

            set
            {
                amount3Header.Text = value;
            }
        }

        protected override void InitialInitToken()
        {
            this.theEffectToken = new ThreeAmountsConfigToken(Amount1Default, Amount2Default, Amount3Default);
        }

        protected override void InitDialogFromToken(EffectConfigToken effectToken)
        {
            base.InitDialogFromToken (effectToken);
            amount3Slider.Value = ((ThreeAmountsConfigToken)effectToken).Amount3;
        }

        protected override void InitTokenFromDialog()
        {
            base.InitTokenFromDialog();
            ((ThreeAmountsConfigToken)theEffectToken).Amount3 = amount3Slider.Value;
        }

        private void InitializeComponent()
        {
            this.amount3Reset = new System.Windows.Forms.Button();
            this.amount3UpDown = new System.Windows.Forms.NumericUpDown();
            this.amount3Slider = new System.Windows.Forms.TrackBar();
            this.amount3Header = new PaintDotNet.HeaderLabel();
            ((System.ComponentModel.ISupportInitialize)(this.amount3UpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.amount3Slider)).BeginInit();
            this.SuspendLayout();
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(101, 219);
            this.okButton.Size = new System.Drawing.Size(81, 23);
            this.okButton.Name = "okButton";
            this.okButton.TabIndex = 9;
            this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(188, 219);
            this.cancelButton.Size = new System.Drawing.Size(81, 23);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.TabIndex = 10;
            this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            // 
            // amount3Reset
            // 
            this.amount3Reset.Location = new System.Drawing.Point(188, 188);
            this.amount3Reset.Name = "amount3Reset";
            this.amount3Reset.Size = new System.Drawing.Size(81, 20);
            this.amount3Reset.TabIndex = 8;
            this.amount3Reset.Click += new System.EventHandler(this.amount3Reset_Click);
            this.amount3Reset.FlatStyle = System.Windows.Forms.FlatStyle.System;
            // 
            // amount3UpDown
            // 
            this.amount3UpDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.amount3UpDown.Location = new System.Drawing.Point(188, 164);
            this.amount3UpDown.Minimum = new System.Decimal(new int[] {
                                                                          100,
                                                                          0,
                                                                          0,
                                                                          -2147483648});
            this.amount3UpDown.Name = "amount3UpDown";
            this.amount3UpDown.Size = new System.Drawing.Size(81, 20);
            this.amount3UpDown.TabIndex = 7;
            this.amount3UpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.amount3UpDown.Enter += new System.EventHandler(this.amount3UpDown_Enter);
            this.amount3UpDown.ValueChanged += new System.EventHandler(this.amount3UpDown_ValueChanged);
            this.amount3UpDown.Leave += new System.EventHandler(this.amount3UpDown_Leave);
            // 
            // amount3Slider
            // 
            this.amount3Slider.LargeChange = 20;
            this.amount3Slider.Location = new System.Drawing.Point(1, 164);
            this.amount3Slider.Maximum = 100;
            this.amount3Slider.Minimum = -100;
            this.amount3Slider.Name = "amount3Slider";
            this.amount3Slider.Size = new System.Drawing.Size(175, 42);
            this.amount3Slider.TabIndex = 6;
            this.amount3Slider.TickFrequency = 10;
            this.amount3Slider.ValueChanged += new System.EventHandler(this.amount3Slider_ValueChanged);
            // 
            // amount3Header
            // 
            this.amount3Header.Location = new System.Drawing.Point(6, 148);
            this.amount3Header.Name = "amount3Header";
            this.amount3Header.Size = new System.Drawing.Size(271, 14);
            this.amount3Header.TabIndex = 11;
            this.amount3Header.TabStop = false;
            this.amount3Header.Text = "Header 3";
            // 
            // ThreeAmountsConfigDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(275, 248);
            this.Controls.Add(this.amount3Header);
            this.Controls.Add(this.amount3Slider);
            this.Controls.Add(this.amount3Reset);
            this.Controls.Add(this.amount3UpDown);
            this.Location = new System.Drawing.Point(0, 0);
            this.Name = "ThreeAmountsConfigDialog";
            this.Controls.SetChildIndex(this.amount3UpDown, 0);
            this.Controls.SetChildIndex(this.amount3Reset, 0);
            this.Controls.SetChildIndex(this.amount3Slider, 0);
            this.Controls.SetChildIndex(this.okButton, 0);
            this.Controls.SetChildIndex(this.cancelButton, 0);
            this.Controls.SetChildIndex(this.amount3Header, 0);
            ((System.ComponentModel.ISupportInitialize)(this.amount3UpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.amount3Slider)).EndInit();
            this.ResumeLayout(false);

        }

        internal ThreeAmountsConfigDialogBase()
        {
            InitializeComponent();
            this.amount3Reset.Text = PdnResources.GetString("TwoAmountsConfigDialog.Reset.Text");
        }

        private void amount3Slider_ValueChanged(object sender, System.EventArgs e)
        {
            if (amount3UpDown.Value != (decimal)amount3Slider.Value)
            {
                amount3UpDown.Value = (decimal)amount3Slider.Value;
                FinishTokenUpdate();
            }
        }

        private void amount3UpDown_ValueChanged(object sender, System.EventArgs e)
        {
            if (amount3Slider.Value != (int)amount3UpDown.Value)
            {
                amount3Slider.Value = (int)amount3UpDown.Value;
                FinishTokenUpdate();
            }
        }

        private void amount3UpDown_Enter(object sender, System.EventArgs e)
        {
            amount3UpDown.Select(0, amount3UpDown.Text.Length);        
        }

        private void amount3UpDown_Leave(object sender, System.EventArgs e)
        {
            Utility.ClipNumericUpDown(amount3UpDown);

            if (Utility.CheckNumericUpDown(amount3UpDown))
            {
                amount3UpDown.Value = decimal.Parse(amount3UpDown.Text);
            }
        }

        private void amount3Reset_Click(object sender, System.EventArgs e)
        {
            this.amount3Slider.Value = amount3Default;
        }

        protected override void OnOkButtonClicked(object sender, EventArgs e)
        {
            amount3UpDown_Leave(sender, e);
            base.OnOkButtonClicked(sender, e);
        }
    }
}
