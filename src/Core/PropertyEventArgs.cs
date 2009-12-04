/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;

namespace PaintDotNet
{
    public class PropertyEventArgs
        : System.EventArgs
    {
        private string propertyName;
        public string PropertyName
        {
            get
            {
                return propertyName;
            }
        }

        public PropertyEventArgs(string propertyName)
        {
            this.propertyName = propertyName;
        }
    }
}
