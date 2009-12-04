/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;

namespace PaintDotNet.HistoryFunctions
{
    internal sealed class FlipDocumentVerticalFunction
        : FlipDocumentFunction
    {
        public static string StaticName
        {
            get
            {
                return PdnResources.GetString("FlipDocumentVerticalAction.Name");
            }
        }

        public FlipDocumentVerticalFunction()
            : base(StaticName,
                   PdnResources.GetImageResource("Icons.MenuImageFlipVerticalIcon.png"), 
                   FlipType.Vertical)
        {
        }
    }
}
