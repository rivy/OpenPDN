/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.Actions;
using PaintDotNet.HistoryMementos;
using System;
using System.Drawing;

namespace PaintDotNet.HistoryFunctions
{
    internal sealed class FillSelectionFunction
        : HistoryFunction
    {
        public static string StaticName
        {
            get
            {
                return PdnResources.GetString("FillSelectionAction.Name");
            }
        }

        public static ImageResource StaticImage
        {
            get
            {
                return PdnResources.GetImageResource("Icons.MenuEditFillSelectionIcon.png");
            }
        }

        private ColorBgra fillColor;

        public override HistoryMemento OnExecute(IHistoryWorkspace historyWorkspace)
        {
            if (historyWorkspace.Selection.IsEmpty)
            {
                return null;
            }

            PdnRegion region = historyWorkspace.Selection.CreateRegion();
            BitmapLayer layer = ((BitmapLayer)historyWorkspace.ActiveLayer);
            PdnRegion simplifiedRegion = Utility.SimplifyAndInflateRegion(region);

            HistoryMemento hm = new BitmapHistoryMemento(
                StaticName,
                StaticImage,
                historyWorkspace,
                historyWorkspace.ActiveLayerIndex,
                simplifiedRegion);

            EnterCriticalRegion();

            layer.Surface.Clear(region, this.fillColor);
            layer.Invalidate(simplifiedRegion);

            simplifiedRegion.Dispose();
            region.Dispose();

            return hm;
        }

        public FillSelectionFunction(ColorBgra fillColor)
            : base(ActionFlags.None)
        {
            this.fillColor = fillColor;
        }
    }
}
