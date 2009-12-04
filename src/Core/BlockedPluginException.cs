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
    public class BlockedPluginException
        : PdnException
    {
        public BlockedPluginException()
        {
        }

        public BlockedPluginException(string message)
            : base(message)
        {
        }

        public BlockedPluginException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected BlockedPluginException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
