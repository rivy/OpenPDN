/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;

namespace PaintDotNet.SystemLayer
{
    public interface IFileOpenDialog
        : IFileDialog
    {
        bool CheckFileExists
        {
            get;
            set;
        }

        bool Multiselect
        {
            get;
            set;
        }

        string[] FileNames
        {
            get;
        }
    }
}
