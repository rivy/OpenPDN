/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.Actions;
using PaintDotNet.SystemLayer;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace PaintDotNet.Updates
{
    internal class InstallingState
        : UpdatesState
    {
        private Exception exception;
        private string installerPath;
        private bool finishing = false;
        private bool haveFinished = false;

        public override void OnEnteredState()
        {
            try
            {
                OnEnteredStateImpl();
            }

            catch (Exception ex)
            {
                this.exception = ex;
                StateMachine.QueueInput(PrivateInput.GoToError);
            }
        }

        private void OnEnteredStateImpl()
        {
            string installerExt = Path.GetExtension(this.installerPath);
            bool extIsExe = (string.Compare(".exe", Path.GetExtension(installerExt), true) == 0);

            if (!extIsExe)
            {
                throw new InvalidOperationException("installerPath does not end in .exe: " + installerPath);
            }

            // Save the %TEMP% filename to the settings repository so that it will
            // be deleted the next time Paint.NET run
            string fileName = Path.GetFileName(installerPath);
        }

        public void Finish(AppWorkspace appWorkspace)
        {
            // Assumes we are running in the main UI thread 

            if (this.finishing)
            {
                return;
            }

            try
            {
                if (this.haveFinished)
                {
                    throw new ApplicationException("already called Finish()");
                }

                this.finishing = true;
                this.haveFinished = true;

                // Verify the update's signature
                bool verified = Security.VerifySignedFile(StateMachine.UIContext, this.installerPath, true, false);
                CloseAllWorkspacesAction cawa = new CloseAllWorkspacesAction();
                appWorkspace.PerformAction(cawa);

                if (verified && !cawa.Cancelled)
                {
                    // we're in the clear, launch the update!
                    Settings.CurrentUser.SetString(SettingNames.UpdateMsiFileName, this.installerPath);

                    if (0 == string.Compare(Path.GetExtension(this.installerPath), ".exe", true))
                    {
                        const string arguments = "/skipConfig";
                        Shell.Execute(appWorkspace, this.installerPath, arguments, ExecutePrivilege.RequireAdmin, ExecuteWaitType.RelaunchPdnOnExit);
                        Startup.CloseApplication();
                    }
                    else
                    {
                    }
                }
                else
                {
                    bool result = FileSystem.TryDeleteFile(this.installerPath);
                }
            }

            finally
            {
                this.finishing = false;
            }
        }

        public override void ProcessInput(object input, out State newState)
        {
            if (input.Equals(UpdatesAction.Continue))
            {
                newState = new DoneState();
            }
            else
            {
                throw new ArgumentException();
            }
        }

        public InstallingState(string installerPath)
            : base(false, false, MarqueeStyle.None)
        {
            this.installerPath = installerPath;
        }
    }
}
