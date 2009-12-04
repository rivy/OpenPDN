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
    public interface IFileSaveDialog
        : IFileDialog
    {
        bool AddExtension
        {
            get;
            set;
        }

        bool OverwritePrompt
        {
            get;
            set;
        }

        string FileName
        {
            get;
            set;
        }
    }
}
