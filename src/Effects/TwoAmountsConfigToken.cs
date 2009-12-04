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
    public class TwoAmountsConfigToken
        : EffectConfigToken
    {
        private int amount1;
        public int Amount1
        {
            get
            {
                return amount1;
            }

            set
            {
                amount1 = value;
            }
        }

        private int amount2;
        public int Amount2
        {
            get
            {
                return amount2;
            }

            set
            {
                amount2 = value;
            }
        }
        
        public override object Clone()
        {
            return new TwoAmountsConfigToken(this);
        }

        public TwoAmountsConfigToken(int amount1, int amount2)
        {
            this.amount1 = amount1;
            this.amount2 = amount2;
        }

        public TwoAmountsConfigToken(TwoAmountsConfigToken copyMe)
            : base(copyMe)
        {
            this.amount1 = copyMe.amount1;
            this.amount2 = copyMe.amount2;
        }
    }
}
