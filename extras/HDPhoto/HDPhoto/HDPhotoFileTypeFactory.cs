/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) Rick Brewster, Tom Jackson, and past contributors.            //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet;
using PaintDotNet.Data;
using System;

namespace PaintDotNet.Data
{
    public sealed class HDPhotoFileTypeFactory
        : IFileTypeFactory
    {
        public FileType[] GetFileTypeInstances()
        {
            Version minVersion = new Version(3, 20);
            Version maxVersion = new Version(3, 65536);

            if (PdnInfo.GetVersion() >= minVersion && PdnInfo.GetVersion() < maxVersion)
            {
                return new FileType[] { new HDPhotoFileType() };
            }
            else
            {
                return new FileType[0];
            }
        }
    }
}