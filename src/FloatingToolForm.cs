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
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace PaintDotNet
{
    // TODO: move
    internal delegate bool CmdKeysEventHandler(object sender, ref Message msg, Keys keyData);

    internal class FloatingToolForm 
        : PdnBaseForm,
          ISnapObstacleHost
    {
        private System.ComponentModel.IContainer components = null;

        private ControlEventHandler controlAddedDelegate;
        private ControlEventHandler controlRemovedDelegate;
        private KeyEventHandler keyUpDelegate;
        private SnapObstacleController snapObstacle;

        public SnapObstacle SnapObstacle
        {
            get
            {
                if (this.snapObstacle == null)
                {
                    int distancePadding = UI.GetExtendedFrameBounds(this);
                    int distance = SnapObstacle.DefaultSnapDistance + distancePadding;

                    this.snapObstacle = new SnapObstacleController(this.Name, this.Bounds, SnapRegion.Exterior, false, SnapObstacle.DefaultSnapProximity, distance);
                    this.snapObstacle.BoundsChangeRequested += SnapObstacle_BoundsChangeRequested;
                }

                return this.snapObstacle;
            }
        }

        private void SnapObstacle_BoundsChangeRequested(object sender, HandledEventArgs<Rectangle> e)
        {
            this.Bounds = e.Data;
        }

        /// <summary>
        /// Occurs when it is appropriate for the parent to steal focus.
        /// </summary>
        public event EventHandler RelinquishFocus;
        protected virtual void OnRelinquishFocus()
        {
            // Only relinquish focus if we have it in the first place
            if (MenuStripEx.IsAnyMenuActive)
            {
                return;
            }

            if (RelinquishFocus != null)
            {
                RelinquishFocus(this, EventArgs.Empty);
            }
        }

        public FloatingToolForm()
        {
            this.KeyPreview = true;
            controlAddedDelegate = new ControlEventHandler(ControlAddedHandler);
            controlRemovedDelegate = new ControlEventHandler(ControlRemovedHandler);
            keyUpDelegate = new KeyEventHandler(KeyUpHandler);

            this.ControlAdded += controlAddedDelegate; // we don't override OnControlAdded so we can re-use the method (see code below for ControlAdded)
            this.ControlRemoved += controlRemovedDelegate;

            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            try
            {
                SystemLayer.UserSessions.SessionChanged += new EventHandler(UserSessions_SessionChanged);
                Microsoft.Win32.SystemEvents.DisplaySettingsChanged += new EventHandler(SystemEvents_DisplaySettingsChanged);
            }

            catch (Exception ex)
            {
                Tracing.Ping("Exception while signing up for some system events: " + ex.ToString());
            }
        }

        private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            if (Visible && IsShown)
            {
                EnsureFormIsOnScreen();
            }
        }

        private void UserSessions_SessionChanged(object sender, EventArgs e)
        {
            if (Visible && IsShown)
            {
                EnsureFormIsOnScreen();
            }
        }

        protected override void OnClick(EventArgs e)
        {
            OnRelinquishFocus();
            base.OnClick(e);
        }

        public event CmdKeysEventHandler ProcessCmdKeyEvent;

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            bool result = false;

            if (Utility.IsArrowKey(keyData))
            {
                KeyEventArgs kea = new KeyEventArgs(keyData);

                switch (msg.Msg)
                {
                    case 0x100: // WM_KEYDOWN:
                        this.OnKeyDown(kea);
                        return kea.Handled;

                /*
                case NativeMethods.WmConstants.WM_KEYUP:
                    this.OnKeyUp(kea);
                    return kea.Handled;
                */
                }
            }
            else
            {
                if (ProcessCmdKeyEvent != null)
                {
                    result = ProcessCmdKeyEvent(this, ref msg, keyData);
                }
            }

            if (!result)
            {
                result = base.ProcessCmdKey(ref msg, keyData);
            }

            return result;
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

                try
                {
                    SystemLayer.UserSessions.SessionChanged -= new EventHandler(UserSessions_SessionChanged);
                    Microsoft.Win32.SystemEvents.DisplaySettingsChanged -= new EventHandler(SystemEvents_DisplaySettingsChanged);
                }

                catch (Exception)
                {
                    // Ignore any errors
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
            // 
            // FloatingToolForm
            // 
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(292, 271);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FloatingToolForm";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.ForceActiveTitleBar = true;
        }
        #endregion

        private void ControlAddedHandler(object sender, ControlEventArgs e)
        {
            e.Control.ControlAdded += controlAddedDelegate;
            e.Control.ControlRemoved += controlRemovedDelegate;
            e.Control.KeyUp += keyUpDelegate;
        }

        private void ControlRemovedHandler(object sender, ControlEventArgs e)
        {
            e.Control.ControlAdded -= controlAddedDelegate;
            e.Control.ControlRemoved -= controlRemovedDelegate;
            e.Control.KeyUp -= keyUpDelegate;
        }

        private void KeyUpHandler(object sender, KeyEventArgs e)
        {
            if (!e.Handled)
            {
                this.OnKeyUp(e);
            }
        }

        private void UpdateSnapObstacleBounds()
        {
            if (this.snapObstacle != null)
            {
                this.snapObstacle.SetBounds(this.Bounds);
            }
        }

        private void UpdateParking()
        {
            if (this.FormBorderStyle == FormBorderStyle.Fixed3D ||
                this.FormBorderStyle == FormBorderStyle.FixedDialog ||
                this.FormBorderStyle == FormBorderStyle.FixedSingle ||
                this.FormBorderStyle == FormBorderStyle.FixedToolWindow)
            {
                ISnapManagerHost ismh = this.Owner as ISnapManagerHost;

                if (ismh != null)
                {
                    SnapManager mySM = ismh.SnapManager;
                    mySM.ReparkObstacle(this);
                }
            }
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            if (Visible)
            {
                EnsureFormIsOnScreen();
            }

            base.OnVisibleChanged(e);
        }

        protected override void OnResizeBegin(EventArgs e)
        {
            UpdateSnapObstacleBounds();
            UpdateParking();
            base.OnResizeBegin(e);
        }

        protected override void OnResize(EventArgs e)
        {
            UpdateSnapObstacleBounds();
            base.OnResize(e);
            UpdateParking();
        }

        protected override void OnResizeEnd(EventArgs e)
        {
            this.moving = false;
            UpdateSnapObstacleBounds();
            UpdateParking();
            base.OnResizeEnd(e);
            OnRelinquishFocus();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            UpdateSnapObstacleBounds();
            UpdateParking();
            base.OnSizeChanged(e);
        }

        private Size movingCursorDelta = Size.Empty; // dx,dy from  mousex,y to bounds.Location
        private bool moving = false;

        protected override void OnMoving(MovingEventArgs mea)
        {
            ISnapManagerHost snapHost = this.Owner as ISnapManagerHost;

            if (snapHost != null)
            {
                SnapManager sm = snapHost.SnapManager;

                // Make sure the window titlebar always follows a constant distance from the mouse cursor
                // Otherwise the window may "slip" as it snaps and unsnaps
                if (!this.moving)
                {
                    this.movingCursorDelta = new Size(
                        Cursor.Position.X - mea.Rectangle.X, 
                        Cursor.Position.Y - mea.Rectangle.Y);

                    this.moving = true;
                }

                mea.Rectangle = new Rectangle(
                    Cursor.Position.X - this.movingCursorDelta.Width,
                    Cursor.Position.Y - this.movingCursorDelta.Height,
                    mea.Rectangle.Width,
                    mea.Rectangle.Height);

                this.snapObstacle.SetBounds(mea.Rectangle);

                Point pt = mea.Rectangle.Location;
                Point newPt = sm.AdjustObstacleDestination(this.SnapObstacle, pt);
                Rectangle newRect = new Rectangle(newPt, mea.Rectangle.Size);

                this.snapObstacle.SetBounds(newRect);
               
                mea.Rectangle = newRect;
            }

            base.OnMoving(mea);
        }

        protected override void OnMove(EventArgs e)
        {
            UpdateSnapObstacleBounds();
            base.OnMove(e);
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            if (this.snapObstacle != null)
            {
                this.snapObstacle.Enabled = this.Enabled;
            }

            base.OnEnabledChanged(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            ISnapManagerHost smh = this.Owner as ISnapManagerHost;

            if (smh != null)
            {
                smh.SnapManager.AddSnapObstacle(this);
            }

            base.OnLoad(e);
        }
    }
}
