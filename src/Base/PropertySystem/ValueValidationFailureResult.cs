/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
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
    public enum ValueValidationFailureResult
    {
        /// <summary>
        /// If an invalid value is set through a property's Value property, then it will be ignored
        /// and the current value will be retained. A ValueChanged event will then be raised with 
        /// the property's retained value.
        /// </summary>
        Ignore,

        /// <summary>
        /// If an invalid value is set through a property's Value property, then it will either be
        /// clamped to within the valid range of the property, or it will be ignored. Clamping
        /// behavior is property value type specific; for example, an integer will be clamped to
        /// a certain range, whereas a string will be truncated past a certain length.
        /// If the invalid value cannot be clamped, then the property's Value will not change.
        /// A ValueChanged event will then be raised with the property's value, regardless of
        /// whether the value was changed or not.
        /// </summary>
        Clamp,

        /// <summary>
        /// If an invalid value is set through a property's Value property, then an exception will
        /// be raised.
        /// </summary>
        ThrowException
    }
}
