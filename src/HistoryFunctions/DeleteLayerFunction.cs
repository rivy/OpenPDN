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
    internal sealed class DeleteLayerFunction
        : HistoryFunction
    {
        public static string StaticName
        {
            get
            {
                return PdnResources.GetString("DeleteLayer.HistoryMementoName");
            }
        }

        public static ImageResource StaticImage
        {
            get
            {
                return PdnResources.GetImageResource("Icons.MenuLayersDeleteLayerIcon.png");
            }
        }

        private int layerIndex;

        public override HistoryMemento OnExecute(IHistoryWorkspace historyWorkspace)
        {
            if (this.layerIndex < 0 || this.layerIndex >= historyWorkspace.Document.Layers.Count)
            {
                throw new ArgumentOutOfRangeException("layerIndex = " + this.layerIndex + 
                    ", expected [0, " + historyWorkspace.Document.Layers.Count + ")");
            }

            HistoryMemento hm = new DeleteLayerHistoryMemento(StaticName, StaticImage, historyWorkspace, historyWorkspace.Document.Layers.GetAt(this.layerIndex));

            EnterCriticalRegion();
            historyWorkspace.Document.Layers.RemoveAt(this.layerIndex);

            return hm;
        }

        public DeleteLayerFunction(int layerIndex)
            : base(ActionFlags.None)
        {
            this.layerIndex = layerIndex;
        }
    }
}
