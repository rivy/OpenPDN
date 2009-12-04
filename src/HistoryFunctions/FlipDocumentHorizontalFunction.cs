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
    internal sealed class FlipDocumentHorizontalFunction
        : FlipDocumentFunction
    {
        public static string StaticName
        {
            get
            {
                return PdnResources.GetString("FlipDocumentHorizontalAction.Name");
            }
        }

        public FlipDocumentHorizontalFunction()
            : base(StaticName,
                   PdnResources.GetImageResource("Icons.MenuImageFlipHorizontalIcon.png"), 
                   FlipType.Horizontal)
        {
        }
    }
}
