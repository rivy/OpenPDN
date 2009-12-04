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
    /// This HistoryMemento can be used to save an entire Document for undo purposes
    /// Create this HistoryMemento, then use SetDocument(), then push this on to the
    /// History using PushNewAction.
    /// </summary>
    internal class ReplaceDocumentHistoryMemento
        : HistoryMemento
    {
        private IHistoryWorkspace historyWorkspace;

        [Serializable]
        private sealed class ReplaceDocumentHistoryMementoData
            : HistoryMementoData
        {
            private Document oldDocument;

            public Document OldDocument
            {
                get
                {
                    return oldDocument;
                }
            }

            public ReplaceDocumentHistoryMementoData(Document oldDocument)
            {
                this.oldDocument = oldDocument;
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    if (oldDocument != null)
                    {
                        oldDocument.Dispose();
                        oldDocument = null;
                    }
                }
            }
        }

        public ReplaceDocumentHistoryMemento(string name, ImageResource image, IHistoryWorkspace historyWorkspace)
            : base(name, image)
        {
            this.historyWorkspace = historyWorkspace;

            ReplaceDocumentHistoryMementoData data = new ReplaceDocumentHistoryMementoData(this.historyWorkspace.Document);
            this.Data = data;
        }

        protected override HistoryMemento OnUndo()
        {
            ReplaceDocumentHistoryMemento ha = new ReplaceDocumentHistoryMemento(Name, Image, this.historyWorkspace);
            this.historyWorkspace.Document = ((ReplaceDocumentHistoryMementoData)Data).OldDocument;
            return ha;
        }
    }
}
