/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Windows.Forms;

namespace PaintDotNet.Updates
{
    internal class ErrorState
        : UpdatesState
    {
        public override string InfoText
        {
            get
            {
                string infoTextFormat = PdnResources.GetString("UpdatesDialog.InfoText.Text.ErrorState.Format");
                string infoText = string.Format(infoTextFormat, this.errorMessage);
                return infoText;
            }
        }

        private Exception exception;
        public Exception Exception
        {
            get
            {
                return this.exception;
            }
        }

        private string errorMessage;
        public string ErrorMessage
        {
            get
            {
                return this.errorMessage;
            }
        }

        public override void OnEnteredState()
        {
            base.OnEnteredState();
        }

        public override void ProcessInput(object input, out State newState)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public ErrorState(Exception exception, string errorMessage)
            : base(true, false, MarqueeStyle.None)
        {
            this.exception = exception;
            this.errorMessage = errorMessage;
        }
    }
}
