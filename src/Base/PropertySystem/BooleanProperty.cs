/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) Rick Brewster, Tom Jackson, and past contributors.            //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

namespace PaintDotNet.PropertySystem
{
    public class BooleanProperty
        : ScalarProperty<bool>
    {
        public BooleanProperty(object name)
            : this(name, false)
        {
        }

        public BooleanProperty(object name, bool defaultValue)
            : this(name, defaultValue, false)
        {
        }

        public BooleanProperty(object name, bool defaultValue, bool readOnly)
            : this(name, defaultValue, readOnly, DefaultValueValidationFailureResult)
        {
        }

        public BooleanProperty(object name, bool defaultValue, bool readOnly, ValueValidationFailureResult vvfResult)
            : base(name, defaultValue, false, true, readOnly, vvfResult)
        {
        }

        private BooleanProperty(BooleanProperty copyMe, BooleanProperty sentinelNotUsed)
            : base(copyMe, sentinelNotUsed)
        {
        }

        public override Property Clone()
        {
            return new BooleanProperty(this, this);
        }
    }
}
