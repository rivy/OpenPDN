/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;

namespace PaintDotNet.HistoryMementos
{
    internal class SwapLayerHistoryMemento
        : HistoryMemento
    {
        private int layerIndex1;
        private int layerIndex2;
        private IHistoryWorkspace historyWorkspace;

        protected override HistoryMemento OnUndo()
        {
            SwapLayerHistoryMemento slha = new SwapLayerHistoryMemento(this.Name, this.Image,
                this.historyWorkspace, this.layerIndex2, this.layerIndex1);

            Layer layer1 = (Layer)this.historyWorkspace.Document.Layers[this.layerIndex1];
            Layer layer2 = (Layer)this.historyWorkspace.Document.Layers[this.layerIndex2];

            int firstIndex = Math.Min(layerIndex1, layerIndex2);
            int secondIndex = Math.Max(layerIndex1, layerIndex2);

            if (secondIndex - firstIndex == 1)
            {
                this.historyWorkspace.Document.Layers.RemoveAt(layerIndex1);
                this.historyWorkspace.Document.Layers.Insert(layerIndex2, layer1);
            }
            else
            {
                // general version
                this.historyWorkspace.Document.Layers[layerIndex1] = layer2;
                this.historyWorkspace.Document.Layers[layerIndex2] = layer1;
            }

            ((Layer)this.historyWorkspace.Document.Layers[this.layerIndex1]).Invalidate();
            ((Layer)this.historyWorkspace.Document.Layers[this.layerIndex2]).Invalidate();

            return slha;
        }

        public SwapLayerHistoryMemento(string name, ImageResource image, IHistoryWorkspace historyWorkspace, int layerIndex1, int layerIndex2)
            : base(name, image)
        {
            this.historyWorkspace = historyWorkspace;
            this.layerIndex1 = layerIndex1;
            this.layerIndex2 = layerIndex2;

            if (this.layerIndex1 < 0 || this.layerIndex2 < 0 ||
                this.layerIndex1 >= this.historyWorkspace.Document.Layers.Count ||
                this.layerIndex2 >= this.historyWorkspace.Document.Layers.Count)
            {
                throw new ArgumentOutOfRangeException("layerIndex[1|2]", "out of range");
            }
        }
    }
}
