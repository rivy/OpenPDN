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
    [Flags]
    public enum FileTypeFlags
        : long
    {
        None = 0,
        SupportsLayers = 1,
        SupportsCustomHeaders = 2,
        SupportsSaving = 4,
        SupportsLoading = 8,
        SavesWithProgress = 16
    }
}

