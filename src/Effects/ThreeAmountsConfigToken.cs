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
    public class ThreeAmountsConfigToken
        : TwoAmountsConfigToken
    {
        private int amount3;

        public int Amount3
        {
            get
            {
                return amount3;
            }

            set
            {
                amount3 = value;
            }
        }

        public override object Clone()
        {
            return new ThreeAmountsConfigToken(this);
        }

        public ThreeAmountsConfigToken(int amount1, int amount2, int amount3)
            : base(amount1, amount2)
        {
            this.amount3 = amount3;
        }

        private ThreeAmountsConfigToken(ThreeAmountsConfigToken copyMe)
            : base(copyMe)
        {
            this.amount3 = copyMe.amount3;
        }
    }
}
