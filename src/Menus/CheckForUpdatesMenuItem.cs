/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet;
using PaintDotNet.SystemLayer;
using PaintDotNet.Updates;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace PaintDotNet.Menus
{
    internal sealed class CheckForUpdatesMenuItem
        : PdnMenuItem
    {
        private StateMachineExecutor stateMachineExecutor;
        private UpdatesStateMachine updatesStateMachine;
        private UpdatesDialog updatesDialog;
        private bool calledFinish = false;
        private System.Windows.Forms.Timer retryDialogTimer = null;

        protected override void OnAppWorkspaceChanged()
        {
            if (this.updatesStateMachine == null &&
                !PdnInfo.IsExpired &&
                (Security.IsAdministrator || Security.CanElevateToAdministrator))
            {
                StartUpdates();
            }

            base.OnAppWorkspaceChanged();
        }

        private void StartUpdates()
        {
            if (!AppWorkspace.IsHandleCreated)
            {
                // can't use AppWorkspace as a sync context until it has a handle
                AppWorkspace.HandleCreated += new EventHandler(AppWorkspace_HandleCreated);
            }
            else
            {
                InitUpdates();
                this.stateMachineExecutor.Start();
            }
        }

        private void AppWorkspace_HandleCreated(object sender, EventArgs e)
        {
            AppWorkspace.HandleCreated -= new EventHandler(AppWorkspace_HandleCreated);
            StartUpdates();
        }

        private void InitUpdates()
        {
            this.updatesStateMachine = new UpdatesStateMachine();
            this.updatesStateMachine.UIContext = AppWorkspace;

            this.stateMachineExecutor = new StateMachineExecutor(this.updatesStateMachine);
            this.stateMachineExecutor.SyncContext = AppWorkspace;

            this.stateMachineExecutor.StateMachineFinished += OnStateMachineFinished;
            this.stateMachineExecutor.StateBegin += OnStateBegin;
            this.stateMachineExecutor.StateWaitingForInput += OnStateWaitingForInput;
        }

        private void DisposeUpdates()
        {
            if (this.stateMachineExecutor != null)
            {
                this.stateMachineExecutor.StateMachineFinished -= OnStateMachineFinished;
                this.stateMachineExecutor.StateBegin -= OnStateBegin;
                this.stateMachineExecutor.StateWaitingForInput -= OnStateWaitingForInput;
                this.stateMachineExecutor.Dispose();
                this.stateMachineExecutor = null;
            }

            this.updatesStateMachine = null;
        }

        private void OnStateBegin(object sender, EventArgs<State> e)
        {
            if (e.Data is Updates.UpdateAvailableState && this.updatesDialog == null)
            {
                bool showDialogNow = true;

                // If no other modal window is on top of us, then go ahead and present
                // the updates dialog. Otherwise, set a timer to check every few seconds
                // and only when there's no other dialog sitting on top of us will we
                // present the dialog.

                Form ourForm = AppWorkspace.FindForm();
                PdnBaseForm asPBF = ourForm as PdnBaseForm;

                if (asPBF != null)
                {
                    if (!asPBF.IsCurrentModalForm)
                    {
                        showDialogNow = false;
                    }
                }

                if (showDialogNow)
                {
                    ShowUpdatesDialog();
                }
                else
                {
                    if (this.retryDialogTimer != null)
                    {
                        this.retryDialogTimer.Enabled = false;
                        this.retryDialogTimer.Dispose();
                        this.retryDialogTimer = null;
                    }

                    this.retryDialogTimer = new System.Windows.Forms.Timer();
                    this.retryDialogTimer.Interval = 3000;

                    this.retryDialogTimer.Tick +=
                        delegate(object sender2, EventArgs e2)
                        {
                            bool done = false;

                            if (IsDisposed)
                            {
                                done = true;
                            }

                            Form ourForm2 = AppWorkspace.FindForm();
                            PdnBaseForm asPBF2 = ourForm2 as PdnBaseForm;

                            if (asPBF2 == null)
                            {
                                done = true;
                            }
                            else
                            {
                                if (this.updatesDialog != null)
                                {
                                    // Updates dialog is already visible.
                                    done = true;
                                }
                                else if (asPBF2.IsCurrentModalForm && asPBF2.Enabled)
                                {
                                    ShowUpdatesDialog();
                                    done = true;
                                }
                            }

                            if (done && this.retryDialogTimer != null)
                            {
                                this.retryDialogTimer.Enabled = false;
                                this.retryDialogTimer.Dispose();
                                this.retryDialogTimer = null;
                            }
                        };

                    this.retryDialogTimer.Enabled = true;
                }
            }
            else if (e.Data is Updates.ReadyToCheckState)
            {
                if (this.updatesDialog == null)
                {
                    DisposeUpdates();
                }
            }
        }

        private void OnStateWaitingForInput(object sender, EventArgs<State> e)
        {
            Updates.InstallingState installingState = e.Data as Updates.InstallingState;

            if (installingState != null)
            {
                installingState.Finish(AppWorkspace);
                this.calledFinish = true;
            }
        }

        private void OnStateMachineFinished(object sender, EventArgs e)
        {
            DisposeUpdates();
        }

        private void ShowUpdatesDialog()
        {
            if (this.retryDialogTimer != null)
            {
                this.retryDialogTimer.Enabled = false;
                this.retryDialogTimer.Dispose();
                this.retryDialogTimer = null;
            }

            this.updatesDialog = new UpdatesDialog();
            this.updatesDialog.UpdatesStateMachine = this.stateMachineExecutor;

            if (!this.stateMachineExecutor.IsStarted)
            {
                //this.stateMachineExecutor.LowPriorityExecution = true;
                this.stateMachineExecutor.Start();
            }

            updatesDialog.ShowDialog(AppWorkspace);
            DialogResult dr = updatesDialog.DialogResult;

            this.updatesDialog.Dispose();
            this.updatesDialog = null;

            if (this.stateMachineExecutor != null)
            {
                if (dr == DialogResult.Yes &&
                    this.stateMachineExecutor.CurrentState is Updates.ReadyToInstallState)
                {
                    this.stateMachineExecutor.ProcessInput(Updates.UpdatesAction.Continue);

                    while (!this.calledFinish)
                    {
                        Application.DoEvents();
                        Thread.Sleep(10);
                    }
                }
            }
        }

        public CheckForUpdatesMenuItem()
        {
            this.Name = "CheckForUpdates";
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeUpdates();
            }

            base.Dispose(disposing);
        }

        protected override void OnClick(EventArgs e)
        {
            if (!Security.IsAdministrator && !Security.CanElevateToAdministrator)
            {
                Utility.ShowNonAdminErrorBox(AppWorkspace);
            }
            else
            {
                if (this.updatesStateMachine == null)
                {
                    InitUpdates();
                }

                ShowUpdatesDialog();
            } 
            
            base.OnClick(e);
        }
    }
}
