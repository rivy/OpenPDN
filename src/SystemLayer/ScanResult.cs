/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;

namespace PaintDotNet.SystemLayer
{
    /// <summary>
    /// Defines the possible results when scanning.
    /// </summary>
    public enum ScanResult
    {
        /// <summary>
        /// The operation completed successfully.
        /// </summary>
        Success = 1,

        /// <summary>
        /// The user cancelled the operation.
        /// </summary>
        UserCancelled = 2,

        /// <summary>
        /// The device was busy or otherwise inaccessible.
        /// </summary>
        DeviceBusy = 3
    }
}
