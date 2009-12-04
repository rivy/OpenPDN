/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace PaintDotNet
{
    // TODO: Eliminate poor code.
    /// <summary>
    /// Encapsulates a surface that can be copied to the clipboard.
    /// </summary>
    [Serializable]
    internal class SurfaceForClipboard
    {
        public MaskedSurface MaskedSurface;
        public Rectangle Bounds;

        public SurfaceForClipboard(MaskedSurface maskedSurface)
        {
            using (PdnRegion region = maskedSurface.CreateRegion())
            {
                this.Bounds = region.GetBoundsInt();
            }

            this.MaskedSurface = maskedSurface;
        }
    }
}
