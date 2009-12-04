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
    internal enum HistoryFunctionResult
    {
        /// <summary>
        /// The HistoryFunction completed successfully, and a new item has been placed in to the HistoryStack.
        /// </summary>
        Success,

        /// <summary>
        /// The HistoryFunction completed successfully, but did nothing.
        /// </summary>
        SuccessNoOp,

        /// <summary>
        /// The HistoryFunction was cancelled by the user. No changes have been made.
        /// </summary>
        Cancelled,

        /// <summary>
        /// There was not enough memory to execute the HistoryFunction. No changes have been made.
        /// </summary>
        OutOfMemory,

        /// <summary>
        /// There was an error executing the HistoryFunction. No changes have been made.
        /// </summary>
        NonFatalError
    }
}
