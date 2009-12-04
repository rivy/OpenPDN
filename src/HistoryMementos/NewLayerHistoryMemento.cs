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
    internal class NewLayerHistoryMemento
        : HistoryMemento
    {
        private int layerIndex;
        private IHistoryWorkspace historyWorkspace;

        protected override HistoryMemento OnUndo()
        {
            DeleteLayerHistoryMemento ha = new DeleteLayerHistoryMemento(Name, Image, this.historyWorkspace,
                (Layer)this.historyWorkspace.Document.Layers[layerIndex]);

            ha.ID = this.ID;
            this.historyWorkspace.Document.Layers.RemoveAt(layerIndex);
            this.historyWorkspace.Document.Invalidate();
            return ha;
        }

        public NewLayerHistoryMemento(string name, ImageResource image, IHistoryWorkspace historyWorkspace, int layerIndex)
            : base(name, image)
        {
            this.historyWorkspace = historyWorkspace;
            this.layerIndex = layerIndex;
        }
    }
}
