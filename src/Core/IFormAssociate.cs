/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// Used by classes to indicate they are associated with a certain Form, even if
    /// they are not contained within the Form. To this end, they are an Associate of
    /// the Form.
    /// </summary>
    public interface IFormAssociate
    {
        /// <summary>
        /// Gets the Form that this object is associated with, or null if there is
        /// no association.
        /// </summary>
        Form AssociatedForm
        {
            get;
        }
    }
}
