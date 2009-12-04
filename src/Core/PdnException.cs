/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.Serialization;

namespace PaintDotNet
{
    /// <summary>
    /// This is the base exception for all Paint.NET exceptions.
    /// </summary>
    public class PdnException
        : ApplicationException
    {
        public PdnException()
        {
        }

        public PdnException(string message)
            : base(message)
        {
        }

        public PdnException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected PdnException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
