/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) Rick Brewster, Tom Jackson, and past contributors.            //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.Serialization;

namespace PaintDotNet
{
    [Serializable]
    public class ReadOnlyException
        : ApplicationException
    {
        public ReadOnlyException()
            : base()
        {
        }

        public ReadOnlyException(string message)
            : base(message)
        {
        }

        public ReadOnlyException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected ReadOnlyException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
