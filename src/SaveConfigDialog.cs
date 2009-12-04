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
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace PaintDotNet
{
    internal class SaveConfigDialog 
        : PdnBaseDialog
    {
        private static readonly Size unscaledMinSize = new Size(600, 350);

        private static class SettingNames
        {
            // We store the bounds of the window relative to its owner.
            public const string Left = "SaveConfigDialog.Left";
            public const string Top = "SaveConfigDialog.Top";
            public const string Width = "SaveConfigDialog.Width";
            public const string Height = "SaveConfigDialog.Height";
            public const string WindowState = "SaveConfigDialog.WindowState";
        }

        private void LoadPositions()
        {
            Size minSize = UI.ScaleSize(unscaledMinSize);

            Form owner = Owner;

            Rectangle ownerWindowBounds;
            if (owner != null)
            {
                ownerWindowBounds = owner.Bounds;
            }
            else
            {
                ownerWindowBounds = Screen.PrimaryScreen.WorkingArea;
            }

            // Determine what our default relative bounds should be
            // These are client bounds that are relative to our owner's window bounds.
            // Or if we have no window, then this is for the primary monitor.
            Rectangle defaultRelativeClientBounds = new Rectangle(
                (ownerWindowBounds.Width - minSize.Width) / 2,
                (ownerWindowBounds.Height - minSize.Height) / 2,
                minSize.Width,
                minSize.Height);

            // Load the relative client bounds for the dialog. This is a client bounds that is
            // relative to the owner's window bounds.
            Rectangle relativeClientBounds;
            FormWindowState newFws;

            try
            {
                string newFwsString = Settings.CurrentUser.GetString(SettingNames.WindowState, FormWindowState.Normal.ToString());
                newFws = (FormWindowState)Enum.Parse(typeof(FormWindowState), newFwsString);

                int newLeft = Settings.CurrentUser.GetInt32(SettingNames.Left, defaultRelativeClientBounds.Left);
                int newTop = Settings.CurrentUser.GetInt32(SettingNames.Top, defaultRelativeClientBounds.Top);
                int newWidth = Math.Max(minSize.Width, Settings.CurrentUser.GetInt32(SettingNames.Width, defaultRelativeClientBounds.Width));
                int newHeight = Math.Max(minSize.Height, Settings.CurrentUser.GetInt32(SettingNames.Height, defaultRelativeClientBounds.Height));

                relativeClientBounds = new Rectangle(newLeft, newTop, newWidth, newHeight);
            }

            catch (Exception)
            {
                relativeClientBounds = defaultRelativeClientBounds;
                newFws = FormWindowState.Normal;
            }

            // Convert to client bounds from from client bounds that are relative to the owner's window bounds.
            // This will be our proposed client bounds.
            Rectangle proposedClientBounds = new Rectangle(
                relativeClientBounds.Left + ownerWindowBounds.Left,
                relativeClientBounds.Top + owner.Top,
                relativeClientBounds.Width,
                relativeClientBounds.Height);

            // Keep the default client bounds around as well
            Rectangle defaultClientBounds = new Rectangle(
                defaultRelativeClientBounds.Left + ownerWindowBounds.Left,
                defaultRelativeClientBounds.Top + ownerWindowBounds.Top,
                defaultRelativeClientBounds.Width,
                defaultRelativeClientBounds.Height);

            // Start applying the values.
            SuspendLayout();

            try
            {
                Rectangle newClientBounds = ValidateAndAdjustNewBounds(owner, proposedClientBounds, defaultClientBounds);
                Rectangle newWindowBounds = ClientBoundsToWindowBounds(newClientBounds);
                Bounds = newWindowBounds;
                WindowState = newFws;
            }

            finally
            {
                ResumeLayout(true);
            }
        }

        private Rectangle ValidateAndAdjustNewBounds(Form owner, Rectangle newClientBounds, Rectangle defaultClientBounds)
        {
            Rectangle returnBounds;

            // Ensure that the bounds they want are in bounds on any of the user's monitors
            // Although first convert from client bounds to window bounds
            Rectangle newWindowBounds = ClientBoundsToWindowBounds(newClientBounds);
            bool intersects = false;

            foreach (Screen screen in Screen.AllScreens)
            {
                intersects |= screen.Bounds.IntersectsWith(newWindowBounds);
            }

            // If the newClientBounds aren't visible anywhere, go with the defaultClientBounds
            Rectangle newClientBounds2;

            if (intersects)
            {
                newClientBounds2 = newClientBounds;
            }
            else
            {
                newClientBounds2 = defaultClientBounds;
            }

            // Now make sure that the bounds are forced to be on the same screen as the owner window
            Screen ourScreen;
            if (owner != null)
            {
                ourScreen = Screen.FromControl(owner);
            }
            else
            {
                ourScreen = Screen.PrimaryScreen;
            }

            Rectangle newWindowBounds2 = ClientBoundsToWindowBounds(newClientBounds2);
            Rectangle onScreenWindowBounds = EnsureRectIsOnScreen(ourScreen, newWindowBounds2);
            Rectangle finalNewClientBounds = WindowBoundsToClientBounds(onScreenWindowBounds);

            returnBounds = finalNewClientBounds;

            return returnBounds;
        }

        private void SavePositions()
        {
            if (WindowState != FormWindowState.Minimized)
            {
                if (WindowState != FormWindowState.Maximized)
                {
                    Form owner = Owner;
                    Point origin;

                    if (owner != null)
                    {
                        Rectangle ownerWindowBounds = owner.Bounds;
                        origin = ownerWindowBounds.Location;
                    }
                    else
                    {
                        origin = new Point(0, 0);
                    }

                    // Save our client rectangle relative to our parent window's bounds (including non-client)

                    Rectangle ourClientBounds = WindowBoundsToClientBounds(this.Bounds);

                    int relativeLeft = ourClientBounds.Left - origin.X;
                    int relativeTop = ourClientBounds.Top - origin.Y;

                    Settings.CurrentUser.SetInt32(SettingNames.Left, relativeLeft);
                    Settings.CurrentUser.SetInt32(SettingNames.Top, relativeTop);
                    Settings.CurrentUser.SetInt32(SettingNames.Width, ourClientBounds.Width);
                    Settings.CurrentUser.SetInt32(SettingNames.Height, ourClientBounds.Height);
                }

                Settings.CurrentUser.SetString(SettingNames.WindowState, WindowState.ToString());
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            if (IsShown)
            {
                SavePositions();
            }

            base.OnSizeChanged(e);
        }

        protected override void OnResize(EventArgs e)
        {
            if (IsShown)
            {
                SavePositions();
            }

            base.OnResize(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (IsShown)
            {
                SavePositions();
            }

            base.OnClosing(e);
        }

        private string fileSizeTextFormat;
        private System.Threading.Timer fileSizeTimer;
        private const int timerDelayTime = 100;

        private Cursor handIcon = new Cursor(PdnResources.GetResourceStream("Cursors.PanToolCursor.cur"));
        private Cursor handIconMouseDown = new Cursor(PdnResources.GetResourceStream("Cursors.PanToolCursorMouseDown.cur"));
        private Hashtable fileTypeToSaveToken = new Hashtable();
        private System.ComponentModel.IContainer components = null;
        private FileType fileType;
        private System.Windows.Forms.Button defaultsButton;
        private Document document;
        private bool disposeDocument = false;
        private HeaderLabel previewHeader;
        private PaintDotNet.DocumentView documentView;
        private PaintDotNet.SaveConfigWidget saveConfigWidget;
        private System.Windows.Forms.Panel saveConfigPanel;

        private PaintDotNet.HeaderLabel settingsHeader;

        private Surface scratchSurface;
        public Surface ScratchSurface
        {
            set
            {
                if (this.scratchSurface != null)
                {
                    throw new InvalidOperationException("May only set ScratchSurface once, and only before the dialog is shown");
                }

                this.scratchSurface = value;
            }
        }

        public event ProgressEventHandler Progress;
        protected virtual void OnProgress(int percent)
        {
            if (Progress != null)
            {
                Progress(this, new ProgressEventArgs((double)percent));
            }
        }

        /// <summary>
        /// Gets or sets the Document instance that is to be saved.
        /// If this is changed after the dialog is shown, the results are undefined.
        /// </summary>
        [Browsable(false)]
        public Document Document
        {
            get
            {
                return this.document;
            }

            set
            {   
                this.document = value;
            }
        }


        [Browsable(false)]
        public FileType FileType
        {
            get
            {
                return fileType;
            }

            set
            {
                if (this.fileType != null && this.fileType.Name == value.Name)
                {
                    return;
                }

                if (this.fileType != null)
                {
                    fileTypeToSaveToken[this.fileType] = this.SaveConfigToken;
                }

                this.fileType = value;
                SaveConfigToken token = (SaveConfigToken)fileTypeToSaveToken[this.fileType];

                if (token == null)
                {
                    token = this.fileType.GetLastSaveConfigToken();
                }

                // Make sure the token is of the expected type by checking it against the 'default' token from this file type
                SaveConfigToken defaultToken = this.fileType.CreateDefaultSaveConfigToken();
                if (token.GetType() != defaultToken.GetType())
                {
                    token = null;
                }

                if (token == null)
                {
                    token = this.fileType.CreateDefaultSaveConfigToken();
                }

                SaveConfigWidget newWidget = this.fileType.CreateSaveConfigWidget();
                newWidget.Token = token;
                newWidget.Location = this.saveConfigWidget.Location;
                this.TokenChangedHandler(this, EventArgs.Empty);
                this.saveConfigWidget.TokenChanged -= new EventHandler(TokenChangedHandler);
                SuspendLayout();
                this.saveConfigPanel.Controls.Remove(this.saveConfigWidget);
                this.saveConfigWidget = newWidget;
                this.saveConfigPanel.Controls.Add(this.saveConfigWidget);
                ResumeLayout(true);
                this.saveConfigWidget.TokenChanged += new EventHandler(TokenChangedHandler);

                if (this.saveConfigWidget is NoSaveConfigWidget)
                {
                    this.defaultsButton.Enabled = false;
                }
                else
                {
                    this.defaultsButton.Enabled = true;
                }
            }
        }

        [Browsable(false)]
        public SaveConfigToken SaveConfigToken
        {
            get
            {
                return this.saveConfigWidget.Token;
            }

            set
            {
                this.saveConfigWidget.Token = value;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            if (this.scratchSurface == null)
            {
                throw new InvalidOperationException("ScratchSurface was never set: it is null");
            }

            LoadPositions();

            base.OnLoad(e);
        }

        public SaveConfigDialog()
        {
            this.fileSizeTimer = new System.Threading.Timer(new System.Threading.TimerCallback(FileSizeTimerCallback), 
                null, 1000, System.Threading.Timeout.Infinite);

            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            this.Text = PdnResources.GetString("SaveConfigDialog.Text");
            this.fileSizeTextFormat = PdnResources.GetString("SaveConfigDialog.PreviewHeader.Text.Format");
            this.settingsHeader.Text = PdnResources.GetString("SaveConfigDialog.SettingsHeader.Text");
            this.defaultsButton.Text = PdnResources.GetString("SaveConfigDialog.DefaultsButton.Text");
            this.previewHeader.Text = PdnResources.GetString("SaveConfigDialog.PreviewHeader.Text");

            this.Icon = Utility.ImageToIcon(PdnResources.GetImageResource("Icons.MenuFileSaveIcon.png").Reference);

            this.documentView.Cursor = handIcon;

            //this.MinimumSize = this.Size;
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            // Bottom-right Buttons
            int buttonsBottomMargin = UI.ScaleHeight(8);
            int buttonsRightMargin = UI.ScaleWidth(8);
            int buttonsHMargin = UI.ScaleWidth(8);

            this.baseCancelButton.Location = new Point(
                ClientSize.Width - this.baseOkButton.Width - buttonsRightMargin, 
                ClientSize.Height - buttonsBottomMargin - this.baseCancelButton.Height);

            this.baseOkButton.Location = new Point(
                this.baseCancelButton.Left - buttonsHMargin - this.baseOkButton.Width, 
                ClientSize.Height - buttonsBottomMargin - this.baseOkButton.Height);

            int previewBottomMargin = UI.ScaleHeight(8);

            // Set up layout properties
            int topMargin = UI.ScaleHeight(6);
            int leftMargin = UI.ScaleWidth(8);
            int leftColumWidth = UI.ScaleWidth(200);
            int columHMargin = UI.ScaleWidth(8);
            int rightMargin = UI.ScaleWidth(8);
            int vMargin = UI.ScaleHeight(4);
            int rightColumnX = leftMargin + leftColumWidth + columHMargin;
            int rightColumnWidth = ClientSize.Width - rightColumnX - rightMargin;
            int defaultsButtonTopMargin = UI.ScaleHeight(12);
            int headerXAdjustment = -3;

            // Left column
            this.settingsHeader.Location = new Point(leftMargin + headerXAdjustment, topMargin);
            this.settingsHeader.Width = leftColumWidth - headerXAdjustment;
            this.settingsHeader.PerformLayout();

            this.saveConfigPanel.Location = new Point(leftMargin, this.settingsHeader.Bottom + vMargin);
            this.saveConfigPanel.Width = leftColumWidth;
            this.saveConfigPanel.PerformLayout();

            //this.saveConfigWidget.Location = new Point(0, 0);
            this.saveConfigWidget.Width = this.saveConfigPanel.Width - SystemInformation.VerticalScrollBarWidth;

            // Right column
            this.previewHeader.Location = new Point(rightColumnX + headerXAdjustment, topMargin);
            this.previewHeader.Width = rightColumnWidth - headerXAdjustment;
            this.previewHeader.PerformLayout();

            this.documentView.Location = new Point(rightColumnX, this.previewHeader.Bottom + vMargin);
            this.documentView.Size = new Size(
                rightColumnWidth, 
                this.baseCancelButton.Top - previewBottomMargin - this.documentView.Top);

            // Finish up setting the height on the left side
            this.saveConfigPanel.Height = this.documentView.Bottom - this.saveConfigPanel.Top -
                this.defaultsButton.Height - defaultsButtonTopMargin;

            this.saveConfigWidget.PerformLayout();

            int saveConfigHeight = Math.Min(this.saveConfigPanel.Height, this.saveConfigWidget.Height);

            this.defaultsButton.PerformLayout();

            this.defaultsButton.Location = new Point(
                leftMargin + (leftColumWidth - this.defaultsButton.Width) / 2,
                this.saveConfigPanel.Top + saveConfigHeight + defaultsButtonTopMargin);

            MinimumSize = UI.ScaleSize(unscaledMinSize);

            base.OnLayout(levent);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.disposeDocument && this.documentView.Document != null)
                {
                    Document disposeMe = this.documentView.Document;
                    this.documentView.Document = null;
                    disposeMe.Dispose();
                }

                CleanupTimer();

                if (this.handIcon != null)
                {
                    this.handIcon.Dispose();
                    this.handIcon = null;
                }

                if (this.handIconMouseDown != null)
                {
                    this.handIconMouseDown.Dispose();
                    this.handIconMouseDown = null;
                }
                                
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
            this.saveConfigPanel = new System.Windows.Forms.Panel();
            this.defaultsButton = new System.Windows.Forms.Button();
            this.saveConfigWidget = new PaintDotNet.SaveConfigWidget();
            this.previewHeader = new PaintDotNet.HeaderLabel();
            this.documentView = new PaintDotNet.DocumentView();
            this.settingsHeader = new PaintDotNet.HeaderLabel();
            this.SuspendLayout();
            // 
            // baseOkButton
            // 
            this.baseOkButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.baseOkButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.baseOkButton.Name = "baseOkButton";
            this.baseOkButton.TabIndex = 2;
            this.baseOkButton.Click += new System.EventHandler(this.BaseOkButton_Click);
            // 
            // baseCancelButton
            // 
            this.baseCancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.baseCancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.baseCancelButton.Name = "baseCancelButton";
            this.baseCancelButton.TabIndex = 3;
            this.baseCancelButton.Click += new System.EventHandler(this.BaseCancelButton_Click);
            // 
            // saveConfigPanel
            // 
            this.saveConfigPanel.AutoScroll = true;
            this.saveConfigPanel.Name = "saveConfigPanel";
            this.saveConfigPanel.TabIndex = 0;
            this.saveConfigPanel.TabStop = false;
            // 
            // defaultsButton
            // 
            this.defaultsButton.Name = "defaultsButton";
            this.defaultsButton.AutoSize = true;
            this.defaultsButton.FlatStyle = FlatStyle.System;
            this.defaultsButton.TabIndex = 1;
            this.defaultsButton.Click += new System.EventHandler(this.DefaultsButton_Click);
            // 
            // saveConfigWidget
            // 
            this.saveConfigWidget.Name = "saveConfigWidget";
            this.saveConfigWidget.TabIndex = 9;
            this.saveConfigWidget.Token = null;
            // 
            // previewHeader
            // 
            this.previewHeader.Name = "previewHeader";
            this.previewHeader.RightMargin = 0;
            this.previewHeader.TabIndex = 11;
            this.previewHeader.TabStop = false;
            this.previewHeader.Text = "Header";
            // 
            // documentView
            // 
            this.documentView.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.documentView.Document = null;
            this.documentView.Name = "documentView";
            this.documentView.PanelAutoScroll = true;
            this.documentView.RulersEnabled = false;
            this.documentView.TabIndex = 12;
            this.documentView.TabStop = false;
            this.documentView.DocumentMouseMove += new System.Windows.Forms.MouseEventHandler(this.DocumentView_DocumentMouseMove);
            this.documentView.DocumentMouseDown += new System.Windows.Forms.MouseEventHandler(this.DocumentView_DocumentMouseDown);
            this.documentView.DocumentMouseUp += new System.Windows.Forms.MouseEventHandler(this.DocumentView_DocumentMouseUp);
            this.documentView.Visible = false;
            // 
            // settingsHeader
            // 
            this.settingsHeader.Name = "settingsHeader";
            this.settingsHeader.TabIndex = 13;
            this.settingsHeader.TabStop = false;
            this.settingsHeader.Text = "Header";
            // 
            // SaveConfigDialog
            // 
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.defaultsButton);
            this.Controls.Add(this.settingsHeader);
            this.Controls.Add(this.previewHeader);
            this.Controls.Add(this.documentView);
            this.Controls.Add(this.saveConfigPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.MinimizeBox = false;
            this.MaximizeBox = true;
            this.Name = "SaveConfigDialog";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = FormStartPosition.Manual;
            this.Controls.SetChildIndex(this.saveConfigPanel, 0);
            this.Controls.SetChildIndex(this.documentView, 0);
            this.Controls.SetChildIndex(this.baseOkButton, 0);
            this.Controls.SetChildIndex(this.baseCancelButton, 0);
            this.Controls.SetChildIndex(this.previewHeader, 0);
            this.Controls.SetChildIndex(this.settingsHeader, 0);
            this.Controls.SetChildIndex(this.defaultsButton, 0);
            this.ResumeLayout(false);
        }
        #endregion

        private void DefaultsButton_Click(object sender, System.EventArgs e)
        {
            this.SaveConfigToken = this.FileType.CreateDefaultSaveConfigToken();
        }

        private void TokenChangedHandler(object sender, EventArgs e)
        {
            QueueFileSizeTextUpdate();
        }

        private void QueueFileSizeTextUpdate()
        {
            callbackDoneEvent.Reset();

            string computing = PdnResources.GetString("SaveConfigDialog.FileSizeText.Text.Computing");
            this.previewHeader.Text = string.Format(this.fileSizeTextFormat, computing);
            this.fileSizeTimer.Change(timerDelayTime, 0);
            OnProgress(0);
        }

        private volatile bool callbackBusy = false;
        private ManualResetEvent callbackDoneEvent = new ManualResetEvent(true);

        private void UpdateFileSizeAndPreview(string tempFileName)
        {
            if (this.IsDisposed)
            {
                return;
            }

            if (tempFileName == null)
            {
                string error = PdnResources.GetString("SaveConfigDialog.FileSizeText.Text.Error");
                this.previewHeader.Text = string.Format(this.fileSizeTextFormat, error);
            }
            else
            {
                FileInfo fi = new FileInfo(tempFileName);
                long fileSize = fi.Length;
                this.previewHeader.Text = string.Format(fileSizeTextFormat, Utility.SizeStringFromBytes(fileSize));
                this.documentView.Visible = true;

                // note: see comments for DocumentView.SuspendRefresh() for why we do these two backwards
                this.documentView.ResumeRefresh();

                Document disposeMe = null;
                try
                {
                    if (this.disposeDocument && this.documentView.Document != null)
                    {
                        disposeMe = this.documentView.Document;
                    }

                    if (this.fileType.IsReflexive(this.SaveConfigToken))
                    {
                        this.documentView.Document = this.Document;
                        this.documentView.Document.Invalidate();
                        this.disposeDocument = false;
                    }
                    else
                    {
                        FileStream stream = new FileStream(tempFileName, FileMode.Open, FileAccess.Read, FileShare.Read);

                        Document previewDoc;
                
                        try
                        {
                            Utility.GCFullCollect();
                            previewDoc = fileType.Load(stream);
                        }

                        catch
                        {
                            previewDoc = null;
                            TokenChangedHandler(this, EventArgs.Empty);
                        }

                        stream.Close();

                        if (previewDoc != null)
                        {
                            this.documentView.Document = previewDoc;
                            this.disposeDocument = true;
                        }

                        Utility.GCFullCollect();
                    }

                    try
                    {
                        fi.Delete();
                    }

                    catch
                    {
                    }
                }

                finally
                {
                    this.documentView.SuspendRefresh();

                    if (disposeMe != null)
                    {
                        disposeMe.Dispose();
                    }
                }
            }
        }

        private void SetFileSizeProgress(int percent)
        {
            string computingFormat = PdnResources.GetString("SaveConfigDialog.FileSizeText.Text.Computing.Format");
            string computing = string.Format(computingFormat, percent);
            this.previewHeader.Text = string.Format(this.fileSizeTextFormat, computing);
            int newPercent = Utility.Clamp(percent, 0, 100);
            OnProgress(newPercent);
        }

        private void FileSizeProgressEventHandler(object state, ProgressEventArgs e)
        {
            if (IsHandleCreated)
            {
                this.BeginInvoke(new Procedure<int>(SetFileSizeProgress), new object[] { (int)e.Percent });
            }
        }

        private void FileSizeTimerCallback(object state)
        {
            try
            {
                if (!this.IsHandleCreated)
                {
                    return;
                }

                if (callbackBusy)
                {
                    this.Invoke(new Procedure(QueueFileSizeTextUpdate));
                }
                else
                {
#if !DEBUG
                try
                {
#endif
                    FileSizeTimerCallbackImpl(state);
#if !DEBUG
                }

                // Catch rare instance where BeginInvoke gets called after the form's window handle is destroyed
                catch (InvalidOperationException)
                {

                }
#endif
                }
            }

            catch (Exception)
            {
                // Handle rare race condition where this method just fails because the form is gone
            }
        }

        private void FileSizeTimerCallbackImpl(object state)
        {
            if (this.fileSizeTimer == null)
            {
                return;
            }

            this.callbackBusy = true;

#if !DEBUG
            try
            {
#endif
                if (this.Document != null)
                {
                    string tempName = Path.GetTempFileName();
                    FileStream stream = new FileStream(tempName, FileMode.Create, FileAccess.Write, FileShare.Read);

                    this.FileType.Save(
                        this.Document, 
                        stream, 
                        this.SaveConfigToken, 
                        this.scratchSurface,
                        new ProgressEventHandler(FileSizeProgressEventHandler), 
                        true);

                    stream.Flush();
                    stream.Close();

                    this.BeginInvoke(new Procedure<string>(UpdateFileSizeAndPreview), new object[] { tempName });
                }
#if !DEBUG
            }

            catch
            {
                this.BeginInvoke(new Procedure<string>(UpdateFileSizeAndPreview), new object[] { null } );
            }

            finally
            {
#endif
                this.callbackDoneEvent.Set();
                this.callbackBusy = false;
#if !DEBUG
            }
#endif
        }

        private void CleanupTimer()
        {
            if (this.fileSizeTimer != null)
            {
                Do.TryBool(() => this.fileSizeTimer.Dispose()); // get crash reports here sometimes, go figure
                this.fileSizeTimer = null;
            }
        }

        private void BaseOkButton_Click(object sender, System.EventArgs e)
        {
            using (new WaitCursorChanger(this))
            {
                this.callbackDoneEvent.WaitOne();
            }

            CleanupTimer();
        }

        private void BaseCancelButton_Click(object sender, EventArgs e)
        {
            using (new WaitCursorChanger(this))
            {
                callbackDoneEvent.WaitOne();
            }

            CleanupTimer();
        }

        private bool documentMouseDown = false;
        private Point lastMouseXY;
        private void DocumentView_DocumentMouseDown(object sender, MouseEventArgs e)
        {
            if (e is StylusEventArgs)
            {
                return;
            }

            if (e.Button == MouseButtons.Left)
            {
                documentMouseDown = true;
                documentView.Cursor = handIconMouseDown;
                lastMouseXY = new Point(e.X, e.Y);
            }
        }

        private void DocumentView_DocumentMouseMove(object sender, MouseEventArgs e)
        {
            if (e is StylusEventArgs)
            {
                return;
            }

            if (documentMouseDown)
            {
                Point mouseXY = new Point(e.X, e.Y);
                Size delta = new Size(mouseXY.X - lastMouseXY.X, mouseXY.Y - lastMouseXY.Y);

                if (delta.Width != 0 || delta.Height != 0)
                {
                    PointF scrollPos = documentView.DocumentScrollPositionF;
                    PointF newScrollPos = new PointF(scrollPos.X - delta.Width, scrollPos.Y - delta.Height);
                    
                    documentView.DocumentScrollPositionF = newScrollPos;
                    documentView.Update();

                    lastMouseXY = mouseXY;
                    lastMouseXY.X -= delta.Width;
                    lastMouseXY.Y -= delta.Height;
                }
            }        
        }

        private void DocumentView_DocumentMouseUp(object sender, MouseEventArgs e)
        {
            if (e is StylusEventArgs)
            {
                return;
            }

            documentMouseDown = false;
            documentView.Cursor = handIcon;
        }
    }
}
