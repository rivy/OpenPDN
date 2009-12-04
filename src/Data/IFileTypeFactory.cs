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
    /// This interface is used to generate FileType instances.
    /// The FileTypes class, when requested for a list of FileType instances,
    /// will use reflection to search for classes that implement this interface 
    /// and then call their GetFileTypeInstances() methods.
    /// </summary>
    public interface IFileTypeFactory
    {
        FileType[] GetFileTypeInstances();
    }
}
