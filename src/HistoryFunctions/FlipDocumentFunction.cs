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
using System.Collections.Generic;
using System.Drawing;

namespace PaintDotNet.HistoryFunctions
{
    internal abstract class FlipDocumentFunction
        : HistoryFunction
    {
        private string historyName;
        private ImageResource undoImage;
        private FlipType flipType;

        public override HistoryMemento OnExecute(IHistoryWorkspace historyWorkspace)
        {
            List<HistoryMemento> actions = new List<HistoryMemento>();

            if (!historyWorkspace.Selection.IsEmpty)
            {
                DeselectFunction da = new DeselectFunction();
                EnterCriticalRegion();
                HistoryMemento hm = da.Execute(historyWorkspace);
                actions.Add(hm);
            }

            int count = historyWorkspace.Document.Layers.Count;

            for (int i = 0; i < count; ++i)
            {
                HistoryMemento memento = new FlipLayerHistoryMemento(this.historyName, undoImage, historyWorkspace, i, flipType);
                EnterCriticalRegion();
                HistoryMemento mementoToAdd = memento.PerformUndo();
                actions.Add(mementoToAdd);
            }

            return new CompoundHistoryMemento(this.historyName, undoImage, actions);
        }

        public FlipDocumentFunction(string historyName, ImageResource image, FlipType flipType)
            : base(ActionFlags.None)
        {
            this.historyName = historyName;
            this.undoImage = image;
            this.flipType = flipType;
        }
    }
}
