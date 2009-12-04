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
using System;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace PaintDotNet.Updates
{
    internal class ExtractingState
        : UpdatesState,
          INewVersionInfo
    {
        private Exception exception;
        private string extractMe;
        private string installerPath;
        private PdnVersionInfo newVersionInfo;
        private SiphonStream abortMeStream = null;

        public PdnVersionInfo NewVersionInfo
        {
            get
            {
                return this.newVersionInfo;
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

            base.OnAbort();
        }

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

        public void OnEnteredStateImpl()
        {
            FileStream zipFileRead = new FileStream(this.extractMe, FileMode.Open, FileAccess.Read, FileShare.Read);
            FileStream exeFileWrite = null;

            try
            {
                ICSharpCode.SharpZipLib.Zip.ZipInputStream zipStream =
                    new ICSharpCode.SharpZipLib.Zip.ZipInputStream(zipFileRead);

                // Search for the first .msi file in the exe, and extract it
                ICSharpCode.SharpZipLib.Zip.ZipEntry zipEntry;
                bool foundExe = false;

                while (true)
                {
                    zipEntry = zipStream.GetNextEntry();

                    if (zipEntry == null)
                    {
                        break;
                    }

                    if (!zipEntry.IsDirectory &&
                        string.Compare(".exe", Path.GetExtension(zipEntry.Name), true, CultureInfo.InvariantCulture) == 0)
                    {
                        foundExe = true;
                        break;
                    }
                }

                if (!foundExe)
                {
                    this.exception = new FileNotFoundException();
                    StateMachine.QueueInput(PrivateInput.GoToError);
                }
                else
                {
                    int maxBytes = (int)zipEntry.Size;
                    int bytesSoFar = 0;

                    this.installerPath = Path.Combine(Path.GetDirectoryName(this.extractMe), zipEntry.Name);
                    exeFileWrite = new FileStream(this.installerPath, FileMode.Create, FileAccess.Write, FileShare.Read);
                    SiphonStream siphonStream2 = new SiphonStream(exeFileWrite, 4096);

                    this.abortMeStream = siphonStream2;

                    IOEventHandler ioFinishedDelegate =
                        delegate(object sender, IOEventArgs e)
                        {
                            bytesSoFar += e.Count;
                            double percent = 100.0 * ((double)bytesSoFar / (double)maxBytes);
                            OnProgress(percent);
                        };

                    OnProgress(0.0);

                    if (maxBytes > 0)
                    {
                        siphonStream2.IOFinished += ioFinishedDelegate;
                    }

                    Utility.CopyStream(zipStream, siphonStream2);

                    if (maxBytes > 0)
                    {
                        siphonStream2.IOFinished -= ioFinishedDelegate;
                    }

                    this.abortMeStream = null;
                    siphonStream2 = null;
                    exeFileWrite.Close();
                    exeFileWrite = null;
                    zipStream.Close();
                    zipStream = null;

                    StateMachine.QueueInput(PrivateInput.GoToReadyToInstall);
                }
            }

            catch (Exception ex)
            {
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

            finally
            {
                if (exeFileWrite != null)
                {
                    exeFileWrite.Close();
                    exeFileWrite = null;
                }

                if (zipFileRead != null)
                {
                    zipFileRead.Close();
                    zipFileRead = null;
                }

                if (this.exception != null || this.AbortRequested)
                {
                    if (this.installerPath != null)
                    {
                        bool result = FileSystem.TryDeleteFile(this.installerPath);
                    }
                }

                if (this.extractMe != null)
                {
                    bool result = FileSystem.TryDeleteFile(this.extractMe);
                }
            }
        }

        public override void ProcessInput(object input, out State newState)
        {
            if (input.Equals(PrivateInput.GoToReadyToInstall))
            {
                newState = new ReadyToInstallState(this.installerPath, this.newVersionInfo);
            }
            else if (input.Equals(PrivateInput.GoToError))
            {
                string errorMessage = PdnResources.GetString("Updates.ExtractingState.GenericError");
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

        public ExtractingState(string extractMe, PdnVersionInfo newVersionInfo)
            : base(false, false, MarqueeStyle.Smooth)
        {
            this.extractMe = extractMe;
            this.newVersionInfo = newVersionInfo;
        }
    }
}
