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
using System.Drawing.Drawing2D;

namespace PaintDotNet.HistoryMementos
{
    internal class SelectionHistoryMemento
        : HistoryMemento
    {
        private object savedSelectionData;
        private IHistoryWorkspace historyWorkspace;

        public SelectionHistoryMemento(string name, ImageResource image, IHistoryWorkspace historyWorkspace)
            : base(name, image)
        {
            this.historyWorkspace = historyWorkspace;
            this.savedSelectionData = this.historyWorkspace.Selection.Save();
        }

        protected override HistoryMemento OnUndo()
        {
            SelectionHistoryMemento sha = new SelectionHistoryMemento(Name, Image, this.historyWorkspace);
            this.historyWorkspace.Selection.Restore(this.savedSelectionData);
            return sha;
        }
    }
}
