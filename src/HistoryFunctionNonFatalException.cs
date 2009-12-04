/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;

namespace PaintDotNet
{
    /// <summary>
    /// This class is treated differently than an Exception in that the Message property
    /// is used as a localized error string that will be displayed to the user. Also,
    /// exceptions of this type are treated as non-fatal, and it is assumed that any
    /// function being executed has not caused any changes.
    /// If null is given for localizedErrorText, a generic message may be provided to
    /// the user instead.
    /// </summary>
    internal class HistoryFunctionNonFatalException
        : Exception
    {
        private string localizedErrorText;
        public string LocalizedErrorText
        {
            get
            {
                return this.localizedErrorText;
            }
        }

        private const string message = "Non-fatal exception encountered";

        public HistoryFunctionNonFatalException()
        {
            this.localizedErrorText = null;
        }

        public HistoryFunctionNonFatalException(string localizedErrorText)
            : base(message)
        {
            this.localizedErrorText = localizedErrorText;
        }

        public HistoryFunctionNonFatalException(string localizedErrorText, Exception innerException)
            : base(message, innerException)
        {
            this.localizedErrorText = localizedErrorText;
        }
    }
}
