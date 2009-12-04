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
    [Serializable]
    public abstract class EffectConfigToken
        : ICloneable
    {
        /// <summary>
        /// This should simply call "new myType(this)" ... do not call base class'
        /// implementation of Clone, as this is handled by the constructors.
        /// </summary>
        public abstract object Clone();
        
        public EffectConfigToken()
        {
        }

        protected EffectConfigToken(EffectConfigToken copyMe)
        {
        }
    }
}

