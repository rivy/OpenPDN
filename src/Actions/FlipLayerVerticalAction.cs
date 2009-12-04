/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.HistoryFunctions;
using System;

namespace PaintDotNet.Actions
{
    internal class FlipLayerVerticalFunction
        : FlipLayerFunction
    {
        public static string StaticName
        {
            get
            {
                return PdnResources.GetString("FlipLayerVerticalAction.Name");
            }
        }

        public FlipLayerVerticalFunction(int layerIndex)
            : base(StaticName,
                   PdnResources.GetImageResource("Icons.MenuLayersFlipVerticalIcon.png"), 
                   FlipType.Vertical,
                   layerIndex)
        {
        }
    }
}
