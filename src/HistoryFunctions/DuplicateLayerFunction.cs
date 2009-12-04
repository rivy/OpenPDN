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
    internal sealed class DuplicateLayerFunction
        : HistoryFunction
    {
        private int layerIndex;

        public static string StaticName
        {
            get
            {
                return PdnResources.GetString("DuplicateLayer.HistoryMementoName");
            }
        }

        public static ImageResource StaticImage
        {
            get
            {
                return PdnResources.GetImageResource("Icons.MenuLayersDuplicateLayerIcon.png");
            }
        }

        public override HistoryMemento OnExecute(IHistoryWorkspace historyWorkspace)
        {
            if (this.layerIndex < 0 || this.layerIndex >= historyWorkspace.Document.Layers.Count)
            {
                throw new ArgumentOutOfRangeException("layerIndex = " + layerIndex + ", expected [0, " + historyWorkspace.Document.Layers.Count + ")");
            }

            Layer newLayer = null;

            newLayer = (Layer)historyWorkspace.ActiveLayer.Clone();
            newLayer.IsBackground = false;
            int newIndex = 1 + this.layerIndex;

            HistoryMemento ha = new NewLayerHistoryMemento(
                StaticName,
                StaticImage,
                historyWorkspace,
                newIndex);

            EnterCriticalRegion();
            historyWorkspace.Document.Layers.Insert(newIndex, newLayer);
            newLayer.Invalidate();

            return ha;
        }

        public DuplicateLayerFunction(int layerIndex)
            : base(ActionFlags.None)
        {
            this.layerIndex = layerIndex;
        }
    }
}
