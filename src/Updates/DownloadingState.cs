/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet;
using System;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace PaintDotNet.Updates
{
    internal class DownloadingState
        : UpdatesState,
          INewVersionInfo
    {
        private PdnVersionInfo downloadMe;
        private SiphonStream abortMeStream;
        private Exception exception = null;
        private string zipTempName;

        public PdnVersionInfo NewVersionInfo
        {
            get
            {
                return this.downloadMe;
            }
        }

        public override bool CanAbort
        {
            get
            {
                return true;
            }
        }

        protected override void OnAbort()
        {
            SiphonStream abortMe = this.abortMeStream;

            if (abortMe != null)
            {
                abortMe.Abort(new Exception());
            }
        }

        public override void OnEnteredState()
        {
            this.zipTempName = Path.GetTempFileName() + ".zip";

            try
            {
                bool getFull;

                if (SystemLayer.OS.IsDotNetVersionInstalled(
                        downloadMe.NetFxMajorVersion,
                        downloadMe.NetFxMinorVersion,
                        downloadMe.NetFxServicePack,
                        true))
                {
                    getFull = false;
                }
                else
                {
                    getFull = true;
                }

                OnProgress(0.0);

                FileStream zipFileWrite = new FileStream(zipTempName, FileMode.Create, FileAccess.Write, FileShare.Read);

                try
                {
                    // we need to wrap the zipFileWrite in a SiphonStream so that we can
                    // Abort() it externally
                    SiphonStream monitorStream = new SiphonStream(zipFileWrite);
                    this.abortMeStream = monitorStream;

                    ProgressEventHandler progressCallback =
                        delegate(object sender, ProgressEventArgs e)
                        {
                            OnProgress(e.Percent);
                        };

                    string url;

                    url = downloadMe.ChooseDownloadUrl(getFull);
                    SystemLayer.Tracing.Ping("Chosen mirror url: " + url);

                    Utility.DownloadFile(new Uri(url), monitorStream, progressCallback);
                    monitorStream.Flush();

                    this.abortMeStream = null;
                    monitorStream = null;
                }

                finally
                {
                    if (zipFileWrite != null)
                    {
                        zipFileWrite.Close();
                        zipFileWrite = null;
                    }
                }

                StateMachine.QueueInput(PrivateInput.GoToExtracting);
            }

            catch (Exception ex)
            {
                this.exception = ex;

                if (this.AbortRequested)
                {
                    StateMachine.QueueInput(PrivateInput.GoToAborted);
                }
                else
                {
                    this.exception = ex;
                    StateMachine.QueueInput(PrivateInput.GoToError);
                }
            }
        }

        public override void ProcessInput(object input, out State newState)
        {
            if (input.Equals(PrivateInput.GoToExtracting))
            {
                newState = new ExtractingState(this.zipTempName, this.downloadMe);
            }
            else if (input.Equals(PrivateInput.GoToError))
            {
                string errorMessage;

                if (this.exception is WebException)
                {
                    errorMessage = Utility.WebExceptionToErrorMessage((WebException)this.exception);
                }
                else
                {
                    errorMessage = PdnResources.GetString("Updates.DownloadingState.GenericError");
                }

                newState = new ErrorState(this.exception, errorMessage);
            }
            else if (input.Equals(PrivateInput.GoToAborted))
            {
                newState = new AbortedState();
            }
            else
            {
                throw new ArgumentException();
            }
        }

        public DownloadingState(PdnVersionInfo downloadMe)
            : base(false, false, MarqueeStyle.Smooth)
        {
            this.downloadMe = downloadMe;
        }
    }
}
