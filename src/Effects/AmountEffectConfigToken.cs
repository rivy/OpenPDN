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
    /// Used for configuring effects that just need one variable,
    /// an integer that specifies a range that describes "how much"
    /// to apply the effect.
    /// </summary>
    public class AmountEffectConfigToken
        : EffectConfigToken
    {
        private int amount;
        public int Amount
        {
            get
            {
                return amount;
            }

            set
            {
                amount = value;
            }
        }

        public override object Clone()
        {
            return new AmountEffectConfigToken(this);
        }

        public AmountEffectConfigToken(int amount)
        {
            this.amount = amount;
        }

        protected AmountEffectConfigToken(AmountEffectConfigToken copyMe)
            : base(copyMe)
        {
            this.amount = copyMe.amount;
        }
    }
}
