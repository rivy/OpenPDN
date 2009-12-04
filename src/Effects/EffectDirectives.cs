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
    /// <summary>
    /// Flags that specify important information that an effect rendering host
    /// must be aware of and take into consideration when executing a particular
    /// effect.
    /// </summary>
    [Obsolete]
    [Flags]
    public enum EffectDirectives
    {
        /// <summary>
        /// No special directive.
        /// </summary>
        None = 0,

        /// <summary>
        /// Specifies that the effect must only execute in one thread at a time.
        /// Normally multiple threads are used in order to increase performance
        /// (esp. on dual processor / dual core systems).
        /// </summary>
        /// <remarks>
        /// This does not prevent multiple threads from being used to execute the effect,
        /// but guarantees that only one rendering thread will be active at any given
        /// time.
        /// </remarks>
        SingleThreaded = 1,
    }
}
