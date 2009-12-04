/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.Serialization;

namespace PaintDotNet
{
    [Serializable]
    public class SaveConfigToken
        : ICloneable,
          IDeserializationCallback

    {
        #region ICloneable Members
        /// <summary>
        /// This should simply call "new myType(this)" ... do not call base class'
        /// implementation of Clone, as this is handled by the constructors.
        /// </summary>
        public virtual object Clone()
        {
            return new SaveConfigToken(this);
        }
        #endregion
        
        public SaveConfigToken()
        {
        }

        protected SaveConfigToken(SaveConfigToken copyMe)
        {
        }

        public virtual void Validate()
        {
        }

        public void OnDeserialization(object sender)
        {
            Validate();
        }
    }
}

