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
    /// Categories for effects that determine their placement within
    /// Paint.NET's menu hierarchy.
    /// </summary>
    public enum EffectCategory
    {
        /// <summary>
        /// The default category for an effect. This will place effects in to the "Effects" menu.
        /// </summary>
        Effect,

        /// <summary>
        /// Signifies that this effect should be an "Image Adjustment", placing the effect in
        /// the "Adjustments" submenu in the "Layers" menu.
        /// These types of effects are typically quick to execute. They are also preferably 
        /// "unary" (see EffectTypeHint) but are not required to be.
        /// </summary>
        Adjustment,

        /// <summary>
        /// Signifies that this effect should not be displayed in any menu.
        /// </summary>
        DoNotDisplay
    }
}
