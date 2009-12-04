/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;

namespace PaintDotNet.Effects
{
    [Obsolete]
    [Flags]
    public enum EffectTypeHint
        : int
    {
        /// <summary>
        /// Specifies that Paint.NET may make no special assumptions about the effect.
        /// This is the default.
        /// </summary>
        NoHints = 0,

        /// <summary>
        /// Specifies that the effect does its rendering in such a way that changes
        /// to a source pixel (x,y) only requires re-rendering of destination pixel
        /// (x,y) and none others.
        /// For example, Desaturate is Unary, whereas Blur is not.
        /// Auto-Levels is not unary because changings any pixel requires the levels
        /// computation to be recomputed which in turn affects all other pixels.
        /// </summary>
        Unary = 1,

        /// <summary>
        /// Specifies that an effect is fast to render. "Fast" is defined as being fast
        /// enough, in general, to be used for real-time rendering. This may be used
        /// in the future for an implementation of "effect layers" (layers that apply
        /// an effect as part of the rendering pipeline).
        /// For example, Desaturate and Invert Colors are fast whereas Blur is not.
        /// </summary>
        Fast = 2
    }
}
