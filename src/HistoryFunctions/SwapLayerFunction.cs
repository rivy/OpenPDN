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
    internal sealed class SwapLayerFunction
        : HistoryFunction
    {
        private int layer1Index;
        private int layer2Index;

        public static string StaticName
        {
            get
            {
                return PdnResources.GetString("SwapLayerFunction.Name");
            }
        }

        public static ImageResource StaticImage
        {
            get
            {
                // TODO: find a real icon for this?
                //return PdnResources.GetImageResource("todo.png");
                return PdnResources.GetImageResource("Icons.MenuLayersMoveLayerUpIcon.png");
            }
        }

        public override HistoryMemento OnExecute(IHistoryWorkspace historyWorkspace)
        {
            if (layer1Index < 0 || layer1Index >= historyWorkspace.Document.Layers.Count ||
                layer2Index < 0 || layer2Index >= historyWorkspace.Document.Layers.Count)
            {
                throw new ArgumentOutOfRangeException("layer1Index = " + this.layer1Index + ", layer2Index = " + layer2Index + ", expected [0," + historyWorkspace.Document.Layers.Count + ")");
            }

            SwapLayerHistoryMemento slhm = new SwapLayerHistoryMemento(
                StaticName,
                StaticImage,
                historyWorkspace,
                layer1Index,
                layer2Index);

            Layer layer1 = historyWorkspace.Document.Layers.GetAt(layer1Index);
            Layer layer2 = historyWorkspace.Document.Layers.GetAt(layer2Index);

            EnterCriticalRegion();
            historyWorkspace.Document.Layers[layer1Index] = layer2;
            historyWorkspace.Document.Layers[layer2Index] = layer1;

            layer1.Invalidate();
            layer2.Invalidate();

            return slhm;
        }

        public SwapLayerFunction(int layer1Index, int layer2Index)
            : base(ActionFlags.None)
        {
            this.layer1Index = layer1Index;
            this.layer2Index = layer2Index;
        }
    }
}
