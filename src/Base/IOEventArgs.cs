/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

namespace PaintDotNet
{
    public sealed class IOEventArgs
        : EventArgs
    {
        /// <summary>
        /// Whether we are reporting a Read or Write operation.
        /// </summary>
        private IOOperationType ioOperationType;
        public IOOperationType IOOperationType
        {
            get
            {
                return ioOperationType;
            }
        }

        /// <summary>
        /// The offset within the file that the operation is to begin, or has finished, at.
        /// </summary>
        private long position;
        public long Position
        {
            get
            {
                return position;
            }
        }

        /// <summary>
        /// The number of bytes that were read or written.
        /// </summary>
        private int count;
        public int Count
        {
            get
            {
                return count;
            }
        }

        public IOEventArgs(IOOperationType ioOperationType, long position, int count)
        {
            this.ioOperationType = ioOperationType;
            this.position = position;
            this.count = count;
        }
    }
}
