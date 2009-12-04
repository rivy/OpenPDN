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
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace PaintDotNet
{
    internal class ColorDisplayWidget 
        : System.Windows.Forms.UserControl
    {
        private System.ComponentModel.IContainer components;

        private ColorRectangleControl primaryColorRectangle;
        private ColorRectangleControl secondaryColorRectangle;
        private IconBox blackAndWhiteIconBox;
        private ToolTip toolTip;
        private IconBox swapIconBox;
    
        protected override Size DefaultSize
        {
            get
            {
                return new Size(48, 48);
            }
        }

        public event EventHandler UserPrimaryColorChanged;
        protected virtual void OnUserPrimaryColorChanged()
        {
            if (UserPrimaryColorChanged != null)
            {
                UserPrimaryColorChanged(this, EventArgs.Empty);
            }
        }

        private ColorBgra userPrimaryColor;
        public ColorBgra UserPrimaryColor
        {
            get
            {
                return this.userPrimaryColor;
            }

            set
            {
                ColorBgra oldColor = this.userPrimaryColor;
                this.userPrimaryColor = value;
                this.primaryColorRectangle.RectangleColor = value.ToColor();
                Invalidate();
                Update();
            }
        }

        public event EventHandler UserSecondaryColorChanged;
        protected virtual void OnUserSecondaryColorChanged()
        {
            if (UserSecondaryColorChanged != null)
            {
                UserSecondaryColorChanged(this, EventArgs.Empty);
            }
        }

        private ColorBgra userSecondaryColor;
        public ColorBgra UserSecondaryColor
        {
            get
            {
                return userSecondaryColor;
            }

            set
            {
                ColorBgra oldColor = this.userSecondaryColor;
                this.userSecondaryColor = value;
                this.secondaryColorRectangle.RectangleColor = value.ToColor();
                Invalidate();
                Update();
            }
        }

        public ColorDisplayWidget()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            this.swapIconBox.Icon = new Bitmap(PdnResources.GetImageResource("Icons.SwapIcon.png").Reference);
            this.blackAndWhiteIconBox.Icon = new Bitmap(PdnResources.GetImageResource("Icons.BlackAndWhiteIcon.png").Reference);

            if (!DesignMode)
            {
                this.toolTip.SetToolTip(swapIconBox, PdnResources.GetString("ColorDisplayWidget.SwapIconBox.ToolTipText"));
                this.toolTip.SetToolTip(blackAndWhiteIconBox, PdnResources.GetString("ColorDisplayWidget.BlackAndWhiteIconBox.ToolTipText"));
                this.toolTip.SetToolTip(primaryColorRectangle, PdnResources.GetString("ColorDisplayWidget.ForeColorRectangle.ToolTipText"));
                this.toolTip.SetToolTip(secondaryColorRectangle, PdnResources.GetString("ColorDisplayWidget.BackColorRectangle.ToolTipText"));
            }
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            int ulX = (this.ClientRectangle.Width - UI.ScaleWidth(this.DefaultSize.Width)) / 2;
            int ulY = (this.ClientRectangle.Height - UI.ScaleHeight(this.DefaultSize.Height)) / 2;

            this.primaryColorRectangle.Location = new System.Drawing.Point(UI.ScaleWidth(ulX + 2), UI.ScaleHeight(ulY + 2));
            this.secondaryColorRectangle.Location = new System.Drawing.Point(UI.ScaleWidth(ulX + 18), UI.ScaleHeight(ulY + 18));
            this.swapIconBox.Location = new System.Drawing.Point(UI.ScaleWidth(ulX + 30), UI.ScaleHeight(ulY + 2));
            this.blackAndWhiteIconBox.Location = new System.Drawing.Point(UI.ScaleWidth(ulX + 2), UI.ScaleHeight(ulY + 31));

            base.OnLayout(levent);
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

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.primaryColorRectangle = new PaintDotNet.ColorRectangleControl();
            this.secondaryColorRectangle = new PaintDotNet.ColorRectangleControl();
            this.swapIconBox = new PaintDotNet.IconBox();
            this.blackAndWhiteIconBox = new PaintDotNet.IconBox();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // foreColorRectangle
            // 
            this.primaryColorRectangle.Name = "foreColorRectangle";
            this.primaryColorRectangle.RectangleColor = System.Drawing.Color.FromArgb(((System.Byte)(0)), ((System.Byte)(0)), ((System.Byte)(192)));
            this.primaryColorRectangle.Size = new System.Drawing.Size(28, 28);
            this.primaryColorRectangle.TabIndex = 0;
            this.primaryColorRectangle.Click += new System.EventHandler(this.PrimaryColorRectangle_Click);
            this.primaryColorRectangle.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Control_KeyUp);
            // 
            // backColorRectangle
            // 
            this.secondaryColorRectangle.Name = "backColorRectangle";
            this.secondaryColorRectangle.RectangleColor = System.Drawing.Color.Magenta;
            this.secondaryColorRectangle.Size = new System.Drawing.Size(28, 28);
            this.secondaryColorRectangle.TabIndex = 1;
            this.secondaryColorRectangle.Click += new System.EventHandler(this.SecondaryColorRectangle_Click);
            this.secondaryColorRectangle.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Control_KeyUp);
            // 
            // swapIconBox
            // 
            this.swapIconBox.Icon = null;
            this.swapIconBox.Name = "swapIconBox";
            this.swapIconBox.Size = new System.Drawing.Size(15, 15);
            this.swapIconBox.TabIndex = 2;
            this.swapIconBox.TabStop = false;
            this.swapIconBox.Click += new System.EventHandler(this.SwapIconBox_Click);
            this.swapIconBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Control_KeyUp);
            this.swapIconBox.DoubleClick += new System.EventHandler(this.SwapIconBox_Click);
            // 
            // blackAndWhiteIconBox
            // 
            this.blackAndWhiteIconBox.Icon = null;
            this.blackAndWhiteIconBox.Name = "blackAndWhiteIconBox";
            this.blackAndWhiteIconBox.Size = new System.Drawing.Size(15, 15);
            this.blackAndWhiteIconBox.TabIndex = 3;
            this.blackAndWhiteIconBox.TabStop = false;
            this.blackAndWhiteIconBox.Click += new System.EventHandler(this.BlackAndWhiteIconBox_Click);
            this.blackAndWhiteIconBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Control_KeyUp);
            this.blackAndWhiteIconBox.DoubleClick += new System.EventHandler(this.BlackAndWhiteIconBox_Click);
            // 
            // toolTip
            // 
            this.toolTip.ShowAlways = true;
            // 
            // ColorDisplayWidget
            // 
            this.Controls.Add(this.blackAndWhiteIconBox);
            this.Controls.Add(this.swapIconBox);
            this.Controls.Add(this.primaryColorRectangle);
            this.Controls.Add(this.secondaryColorRectangle);
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.Name = "ColorDisplayWidget";
            this.Size = new System.Drawing.Size(48, 48);
            this.ResumeLayout(false);

        }
        #endregion

        public event EventHandler SwapColorsClicked;
        protected virtual void OnSwapColorsClicked()
        {
            if (SwapColorsClicked != null)
            {
                SwapColorsClicked(this, EventArgs.Empty);
            }
        }

        private void SwapIconBox_Click(object sender, System.EventArgs e)
        {
            OnSwapColorsClicked();
        }

        public event EventHandler BlackAndWhiteButtonClicked;
        protected virtual void OnBlackAndWhiteButtonClicked()
        {
            if (BlackAndWhiteButtonClicked != null)
            {
                BlackAndWhiteButtonClicked(this, EventArgs.Empty);
            }
        }

        private void BlackAndWhiteIconBox_Click(object sender, System.EventArgs e)
        {
            OnBlackAndWhiteButtonClicked();
        }

        public event EventHandler UserPrimaryColorClick;
        protected virtual void OnUserPrimaryColorClick()
        {
            if (UserPrimaryColorClick != null)
            {
                UserPrimaryColorClick(this, EventArgs.Empty);
            }
        }

        private void PrimaryColorRectangle_Click(object sender, System.EventArgs e)
        {
            OnUserPrimaryColorClick();
        }

        public event EventHandler UserSecondaryColorClick;
        protected virtual void OnUserSecondaryColorClick()
        {
            if (UserSecondaryColorClick != null)
            {
                UserSecondaryColorClick(this, EventArgs.Empty);
            }
        }

        private void SecondaryColorRectangle_Click(object sender, System.EventArgs e)
        {
            OnUserSecondaryColorClick();
        }

        private void Control_KeyUp(object sender, KeyEventArgs e)
        {
            this.OnKeyUp(e);
        }
    }
}
