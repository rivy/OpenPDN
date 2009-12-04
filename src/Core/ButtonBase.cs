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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace PaintDotNet
{
    /// <summary>
    /// This class provides the logic for handling input and managing rendering
    /// states for a typical button type control. Just provide the rendering code!
    /// </summary>
    public abstract class ButtonBase
        : Control,
          IButtonControl
    {
        private bool isDefault = false;
        private bool drawPressed = false;
        private bool drawHover = false;
        private DialogResult dialogResult = DialogResult.None;

        public event EventHandler DialogResultChanged;
        protected virtual void OnDialogResultChanged()
        {
            if (DialogResultChanged != null)
            {
                DialogResultChanged(this, EventArgs.Empty);
            }
        }

        public DialogResult DialogResult
        {
            get
            {
                return this.dialogResult;
            }

            set
            {
                if (this.dialogResult != value)
                {
                    this.dialogResult = value;
                    OnDialogResultChanged();
                }
            }
        }

        public event EventHandler IsDefaultChanged;
        protected virtual void OnIsDefaultChanged()
        {
            if (IsDefaultChanged != null)
            {
                IsDefaultChanged(this, EventArgs.Empty);
            }
        }

        public bool IsDefault
        {
            get
            {
                return this.isDefault;
            }
        }

        protected internal ButtonBase()
        {
            UI.InitScaling(this);

            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.Selectable, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.StandardDoubleClick, false);

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.AccessibleRole = AccessibleRole.PushButton;
            this.Name = "ButtonBase";
            this.DoubleBuffered = true;
            this.TabStop = true;
        }

        public void NotifyDefault(bool value)
        {
            if (this.isDefault != value)
            {
                this.isDefault = value;
                OnIsDefaultChanged();
                Invalidate(true);
            }
        }

        public void PerformClick()
        {
            OnClick(EventArgs.Empty);
        }

        private bool ContainsMouseCursor
        {
            get
            {
                Point mousePt = Control.MousePosition;
                Rectangle screenRect = this.RectangleToScreen(ClientRectangle);

                return screenRect.Contains(mousePt);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            PushButtonState state;

            if (!Enabled)
            {
                state = PushButtonState.Disabled;
            }
            else if (this.drawPressed && ContainsMouseCursor)
            {
                state = PushButtonState.Pressed;
            }
            else if (this.drawHover)
            {
                state = PushButtonState.Hot;
            }
            else if (IsDefault)
            {
                state = PushButtonState.Default;
            }
            else
            {
                state = PushButtonState.Normal;
            }

            bool drawFocusCues = ShowFocusCues && Focused;
            bool drawKeyboardCues = ShowKeyboardCues;

            OnPaintButton(e.Graphics, state, drawFocusCues, drawKeyboardCues);
            base.OnPaint(e);
        }

        protected abstract void OnPaintButton(
            Graphics g,
            PushButtonState buttonState,
            bool drawFocusCues,
            bool drawKeyboardCues);

        protected override void OnEnabledChanged(EventArgs e)
        {
            Invalidate(true);
            base.OnEnabledChanged(e);
        }
        
        protected override void OnMouseEnter(EventArgs e)
        {
            this.drawHover = true;
            Invalidate(true);
            Update();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            this.drawPressed = true;
            Invalidate(true);
            base.OnMouseDown(mevent);
        }

        protected override void OnMouseMove(MouseEventArgs mevent)
        {
            Invalidate(true);
            base.OnMouseMove(mevent);
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            this.drawPressed = false;
            Invalidate(true);
            base.OnMouseUp(mevent);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            this.drawHover = false;
            Invalidate(true);
            Update();
            base.OnMouseLeave(e);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            Invalidate(true);
            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(EventArgs e)
        {
            this.drawPressed = false;
            Invalidate(true);
            base.OnLostFocus(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                this.drawPressed = true;
                Refresh();
            }

            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                this.drawPressed = false;
                Refresh();
                PerformClick();
            }

            base.OnKeyUp(e);
        }

        protected override bool ProcessMnemonic(char charCode)
        {
            if (CanSelect && IsMnemonic(charCode, this.Text))
            {
                OnClick(EventArgs.Empty);
            }

            return base.ProcessMnemonic(charCode);
        }
    }
}
