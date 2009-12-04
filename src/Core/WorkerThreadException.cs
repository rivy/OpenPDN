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
    /// This exception is thrown by a foreground thread when a background worker thread
    /// had an exception. This allows all exceptions to be handled by the foreground thread.
    /// </summary>
    public class WorkerThreadException
        : PdnException
    {
        private const string defaultMessage = "Worker thread threw an exception";

        public WorkerThreadException(Exception innerException)
            : this(defaultMessage, innerException)
        {
        }

        public WorkerThreadException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
