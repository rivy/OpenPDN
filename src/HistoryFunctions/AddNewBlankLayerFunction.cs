/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.HistoryMementos;
using System;

namespace PaintDotNet.HistoryFunctions
{
    internal sealed class AddNewBlankLayerFunction
        : HistoryFunction
    {
        public override HistoryMemento OnExecute(IHistoryWorkspace historyWorkspace)
        {
            BitmapLayer newLayer = null;
            newLayer = new BitmapLayer(historyWorkspace.Document.Width, historyWorkspace.Document.Height);
            string newLayerNameFormat = PdnResources.GetString("AddNewBlankLayer.LayerName.Format");
            newLayer.Name = string.Format(newLayerNameFormat, (1 + historyWorkspace.Document.Layers.Count).ToString());

            int newLayerIndex = historyWorkspace.ActiveLayerIndex + 1;

            NewLayerHistoryMemento ha = new NewLayerHistoryMemento(
                PdnResources.GetString("AddNewBlankLayer.HistoryMementoName"),
                PdnResources.GetImageResource("Icons.MenuLayersAddNewLayerIcon.png"),
                historyWorkspace,
                newLayerIndex);

            EnterCriticalRegion();

            historyWorkspace.Document.Layers.Insert(newLayerIndex, newLayer);

            return ha;
        }

        public AddNewBlankLayerFunction()
            : base(ActionFlags.None)
        {
        }
    }
}
