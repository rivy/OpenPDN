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
    /// Saves the state of the Document's metadata.
    /// </summary>
    internal class MetaDataHistoryMemento
        : HistoryMemento
    {
        private IHistoryWorkspace historyWorkspace;

        [Serializable]
        private class MetaDataHistoryMementoData
            : HistoryMementoData
        {
            private Document document;

            public Document Document
            {
                get
                {
                    return this.document;
                }
            }

            public MetaDataHistoryMementoData(Document document)
            {
                this.document = document;
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    if (this.document != null)
                    {
                        this.document.Dispose();
                        this.document = null;
                    }
                }

                base.Dispose(disposing);
            }
        }

        public MetaDataHistoryMemento(string name, ImageResource image, IHistoryWorkspace historyWorkspace)
            : base(name, image)
        {
            this.historyWorkspace = historyWorkspace;
            Document document = new Document(1, 1); // we need some place to store the metadata...
            document.ReplaceMetaDataFrom(historyWorkspace.Document);
            MetaDataHistoryMementoData data = new MetaDataHistoryMementoData(document);
            this.Data = data;
        }

        protected override HistoryMemento OnUndo()
        {
            MetaDataHistoryMemento redo = new MetaDataHistoryMemento(this.Name, this.Image, this.historyWorkspace);
            MetaDataHistoryMementoData data = (MetaDataHistoryMementoData)this.Data;
            this.historyWorkspace.Document.ReplaceMetaDataFrom(data.Document);
            return redo;
        }
    }
}
