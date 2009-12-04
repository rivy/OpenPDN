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
    /// <summary>
    /// Declares an EventArgs type for an event that needs a single integer, interpreted
    /// as an index, as event information.
    /// </summary>
    public sealed class IndexEventArgs 
        : EventArgs
    {
        int index;

        public int Index
        {
            get
            {
                return index;
            }
        }

        public IndexEventArgs(int i)
        {
            this.index = i;
        }
    }
}
