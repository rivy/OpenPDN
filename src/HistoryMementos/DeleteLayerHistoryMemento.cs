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
    /// <summary>
    /// Provides the ability to undo deleting a layer.
    /// </summary>
    internal class DeleteLayerHistoryMemento
        : HistoryMemento
    {
        private int index;
        private IHistoryWorkspace historyWorkspace;

        [Serializable]
        private sealed class DeleteLayerHistoryMementoData
            : HistoryMementoData
        {
            private Layer layer;

            public Layer Layer
            {
                get
                {
                    return layer;
                }
            }

            public DeleteLayerHistoryMementoData(Layer layer)
            {
                this.layer = layer;
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    if (layer != null)
                    {
                        layer.Dispose();
                        layer = null;
                    }
                }
            }
        }

        protected override HistoryMemento OnUndo()
        {
            DeleteLayerHistoryMementoData data = (DeleteLayerHistoryMementoData)this.Data;
            HistoryMemento ha = new NewLayerHistoryMemento(Name, Image, this.historyWorkspace, this.index);
            this.historyWorkspace.Document.Layers.Insert(index, data.Layer);
            ((Layer)this.historyWorkspace.Document.Layers[index]).Invalidate();
            return ha;
        }

        public DeleteLayerHistoryMemento(string name, ImageResource image, IHistoryWorkspace historyWorkspace, Layer deleteMe)
            : base(name, image)
        {
            this.historyWorkspace = historyWorkspace;
            this.index = historyWorkspace.Document.Layers.IndexOf(deleteMe);
            this.Data = new DeleteLayerHistoryMementoData(deleteMe);
        }
    }
}
