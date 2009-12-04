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
using System.Runtime.Serialization;

namespace PaintDotNet
{
    public interface IDeferredSerializable
        : ISerializable
    {
        void FinishSerialization(Stream output, DeferredFormatter context);
        void FinishDeserialization(Stream input, DeferredFormatter context);
    }
}
