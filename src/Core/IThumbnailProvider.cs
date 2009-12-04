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
    public interface IThumbnailProvider
    {
        /// <summary>
        /// Renders a thumbnail for the underlying object.
        /// </summary>
        /// <param name="maxEdgeLength">The maximum edge length of the thumbnail.</param>
        /// <returns>
        /// This method must only render the thumbnail without any borders. The Surface returned may have
        /// a maximum size of (maxEdgeLength x maxEdgeLength). This method may be called from any thread.
        /// The Surface returned is then owned by the calling method.
        /// </returns>
        /// <remarks>
        /// This method may throw exceptions; however, it must guarantee that the underlying object is
        /// still valid and coherent in this situation.
        /// </remarks>
        Surface RenderThumbnail(int maxEdgeLength);
    }
}
