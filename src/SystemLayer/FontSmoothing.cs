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
    public enum FontSmoothing
    {
        /// <summary>
        /// Specifies that text should be anti-aliased, but sharp. This should employ
        /// the same method that the OS uses for UI text, albeit with AA forced on.
        /// </summary>
        /// <remarks>
        /// On Windows, this uses GDI for rendering. ClearType is not used.
        /// </remarks>
        Sharp,

        /// <summary>
        /// Specifies that text should be anti-aliased in a smoother manner.
        /// </summary>
        /// <remarks>
        /// On Windows, this uses GDI+ for rendering. ClearType is not used.
        /// </remarks>
        Smooth
    }
}
