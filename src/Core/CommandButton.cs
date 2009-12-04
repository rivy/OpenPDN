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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace PaintDotNet 
{
    public sealed class CommandButton
        : ButtonBase
    {
        private Font actionTextFont;
        private string actionText;
        private Font explanationTextFont;
        private string explanationText;
        private Image actionImage;
        private Image actionImageDisabled;

        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public override bool AutoSize
        {
            get
            {
                return base.AutoSize;
            }

            set
            {
                base.AutoSize = value;
                PerformLayout();
                Invalidate(true);
            }
        }

        public string ActionText
        {
            get
            {
                return this.actionText;
            }

            set
            {
                if (this.actionText != value)
                {
                    this.actionText = value;
                    this.Text = value; // ensure that mnemonics get processed correctly
                    PerformLayout();
                    Invalidate(true);
                }
            }
        }

        public string ExplanationText
        {
            get
            {
                return this.explanationText;
            }

            set
            {
                if (this.explanationText != value)
                {
                    this.explanationText = value;
                    PerformLayout();
                    Invalidate(true);
                }
            }
        }

        public Image ActionImage
        {
            get
            {
                return this.actionImage;
            }

            set
            {
                if (this.actionImage != null)
                {
                    this.actionImageDisabled.Dispose();
                    this.actionImageDisabled = null;
                    this.actionImage.Dispose();
                    this.actionImage = null;
                }

                if (value != null)
                {
                    this.actionImage = value;
                    this.actionImageDisabled = ToolStripRenderer.CreateDisabledImage(this.actionImage);
                }

                PerformLayout();
                Invalidate(true);
            }
        }

        public CommandButton()
        {
            InitializeComponent();
            this.actionTextFont = new Font(this.Font.FontFamily, this.Font.Size * 1.25f, this.Font.Style);
            this.explanationTextFont = this.Font;
        }

        protected override void OnPaintButton(Graphics g, PushButtonState state, bool drawFocusCues, bool drawKeyboardCues)
        {
            MeasureAndDraw(g, true, state, drawFocusCues, drawKeyboardCues);
        }

        private Size MeasureAndDraw(Graphics g, bool enableDrawing, PushButtonState state, bool drawFocusCues, bool drawKeyboardCues)
        {
            if (enableDrawing)
            {
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.CompositingMode = CompositingMode.SourceOver;
                g.InterpolationMode = InterpolationMode.Bilinear;
            }

            int marginX = UI.ScaleWidth(9);
            int marginYTop = UI.ScaleHeight(8);
            int marginYBottom = UI.ScaleHeight(9);
            int paddingX = UI.ScaleWidth(8);
            int paddingY = UI.ScaleHeight(3);
            int offsetX = 0;
            int offsetY = 0;

            bool drawAsDefault = (state == PushButtonState.Default);

            if (enableDrawing)
            {
                using (Brush backBrush = new SolidBrush(this.BackColor))
                {
                    CompositingMode oldCM = g.CompositingMode;
                    g.CompositingMode = CompositingMode.SourceCopy;
                    g.FillRectangle(backBrush, ClientRectangle);
                    g.CompositingMode = oldCM;
                }

                Rectangle ourRect = new Rectangle(0, 0, ClientSize.Width, ClientSize.Height);

                if (state == PushButtonState.Pressed)
                {
                    offsetX = 1;
                    offsetY = 1;
                }

                UI.DrawCommandButton(g, state, ourRect, BackColor, this);
            }

            Rectangle actionImageRect;

            Brush textBrush = new SolidBrush(SystemColors.WindowText);

            if (this.actionImage == null)
            {
                actionImageRect = new Rectangle(offsetX, offsetY + marginYTop, 0, 0);
            }
            else
            {
                actionImageRect = new Rectangle(offsetX + marginX, offsetY + marginYTop, 
                    UI.ScaleWidth(this.actionImage.Width), UI.ScaleHeight(this.actionImage.Height));

                Rectangle srcRect = new Rectangle(0, 0, this.actionImage.Width, this.actionImage.Height);

                if (enableDrawing)
                {
                    Image drawMe = Enabled ? this.actionImage : this.actionImageDisabled;

                    if (Enabled)
                    {
                        actionImageRect.Y += 3;
                        actionImageRect.X += 1;
                        g.DrawImage(this.actionImageDisabled, actionImageRect, srcRect, GraphicsUnit.Pixel);
                        actionImageRect.X -= 1;
                        actionImageRect.Y -= 3;
                    }

                    actionImageRect.Y += 2;
                    g.DrawImage(drawMe, actionImageRect, srcRect, GraphicsUnit.Pixel);
                    actionImageRect.Y -= 2;                    
                }
            }

            int actionTextX = actionImageRect.Right + paddingX;
            int actionTextY = actionImageRect.Top;
            int actionTextWidth = ClientSize.Width - actionTextX - marginX + offsetX;

            StringFormat stringFormat = (StringFormat)StringFormat.GenericTypographic.Clone();
            stringFormat.HotkeyPrefix = drawKeyboardCues ? HotkeyPrefix.Show : HotkeyPrefix.Hide;

            SizeF actionTextSize = g.MeasureString(this.actionText, this.actionTextFont, actionTextWidth, stringFormat);

            Rectangle actionTextRect = new Rectangle(actionTextX, actionTextY, 
                actionTextWidth, (int)Math.Ceiling(actionTextSize.Height));

            if (enableDrawing)
            {
                if (state == PushButtonState.Disabled)
                {
                    ControlPaint.DrawStringDisabled(g, this.actionText, this.actionTextFont, this.BackColor, actionTextRect, stringFormat);
                }
                else
                {
                    g.DrawString(this.actionText, this.actionTextFont, textBrush, actionTextRect, stringFormat);
                }
            }

            int descriptionTextX = actionTextX;
            int descriptionTextY = actionTextRect.Bottom + paddingY;
            int descriptionTextWidth = actionTextWidth;

            SizeF descriptionTextSize = g.MeasureString(this.explanationText, this.explanationTextFont, 
                descriptionTextWidth, stringFormat);

            Rectangle descriptionTextRect = new Rectangle(descriptionTextX, descriptionTextY, 
                descriptionTextWidth, (int)Math.Ceiling(descriptionTextSize.Height));

            if (enableDrawing)
            {
                if (state == PushButtonState.Disabled)
                {
                    ControlPaint.DrawStringDisabled(g, this.explanationText, this.explanationTextFont, this.BackColor, descriptionTextRect, stringFormat);
                }
                else
                {
                    g.DrawString(this.explanationText, this.explanationTextFont, textBrush, descriptionTextRect, stringFormat);
                }
            }

            if (enableDrawing)
            {
                if (drawFocusCues)
                {
                    ControlPaint.DrawFocusRectangle(g, new Rectangle(3, 3, ClientSize.Width - 5, ClientSize.Height - 5));
                }
            }

            if (textBrush != null)
            {
                textBrush.Dispose();
                textBrush = null;
            }

            stringFormat.Dispose();
            stringFormat = null;

            Size layoutSize = new Size(ClientSize.Width, descriptionTextRect.Bottom + marginYBottom);
            return layoutSize;
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            if (AutoSize)
            {
                Size layoutSize;

                using (Graphics g = CreateGraphics())
                {
                    layoutSize = MeasureAndDraw(g, false, PushButtonState.Normal, false, false);
                }

                this.ClientSize = layoutSize;
            }

            base.OnLayout(levent);
        }

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.AccessibleRole = AccessibleRole.PushButton;
            this.TabStop = true;
            this.DoubleBuffered = true;
            this.Name = "CommandButton";
            PerformLayout();
        }
    }
}
