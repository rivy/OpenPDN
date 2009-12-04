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
    internal sealed class EraseSelectionFunction
        : HistoryFunction
    {
        public static string StaticName
        {
            get
            {
                return PdnResources.GetString("EraseSelectionAction.Name");
            }
        }

        public static ImageResource StaticImage
        {
            get
            {
                return PdnResources.GetImageResource("Icons.MenuEditEraseSelectionIcon.png");
            }
        }

        public override HistoryMemento OnExecute(IHistoryWorkspace historyWorkspace)
        {
            if (historyWorkspace.Selection.IsEmpty)
            {
                return null;
            }

            SelectionHistoryMemento shm = new SelectionHistoryMemento(string.Empty, null, historyWorkspace);

            PdnRegion region = historyWorkspace.Selection.CreateRegion();

            BitmapLayer layer = ((BitmapLayer)historyWorkspace.ActiveLayer);
            PdnRegion simplifiedRegion = Utility.SimplifyAndInflateRegion(region);

            HistoryMemento hm = new BitmapHistoryMemento(
                null, 
                null,
                historyWorkspace,
                historyWorkspace.ActiveLayerIndex, 
                simplifiedRegion);
            
            HistoryMemento chm = new CompoundHistoryMemento(
                StaticName,
                StaticImage,
                new HistoryMemento[] { shm, hm });

            EnterCriticalRegion();

            layer.Surface.Clear(region, ColorBgra.FromBgra(255, 255, 255, 0));

            layer.Invalidate(simplifiedRegion);
            historyWorkspace.Document.Invalidate(simplifiedRegion);
            simplifiedRegion.Dispose();
            region.Dispose();
            historyWorkspace.Selection.Reset();

            return chm;
        }

        public EraseSelectionFunction()
            : base(ActionFlags.None)
        {
        }
    }
}
