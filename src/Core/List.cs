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
    /// A very simple linked-list class, done functional style. Use null for
    /// the tail to indicate the end of a list.
    /// </summary>
    public sealed class List
    {
        private object head;
        public object Head
        {
            get
            {
                return head;
            }
        }

        private List tail;
        public List Tail
        {
            get
            {
                return tail;
            }
        }

        public List(object head, List tail)
        {
            this.head = head;
            this.tail = tail;
        }
    }
}
