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
using System.Collections.Specialized;

namespace PaintDotNet.HistoryMementos
{
    internal class LayerPropertyHistoryMemento
        : HistoryMemento
    {
        private object properties;
        private IHistoryWorkspace historyWorkspace;
        private int layerIndex;

        protected override HistoryMemento OnUndo()
        {
            HistoryMemento ha = new LayerPropertyHistoryMemento(Name, Image, this.historyWorkspace, this.layerIndex);
            Layer layer = (Layer)this.historyWorkspace.Document.Layers[layerIndex];
            layer.LoadProperties(properties, true);
            layer.PerformPropertyChanged();
            return ha;
        }

        public LayerPropertyHistoryMemento(string name, ImageResource image, IHistoryWorkspace historyWorkspace, int layerIndex)
            : base(name, image)
        {
            this.historyWorkspace = historyWorkspace;
            this.layerIndex = layerIndex;
            this.properties = ((Layer)this.historyWorkspace.Document.Layers[layerIndex]).SaveProperties();
        }
    }
}
