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
    /// This history action doesn't really do anything. It is useful for putting in a
    /// "New Image" placeholder, since the first item in the undo stack can't really
    /// be "undone".
    /// NullHistoryMemento instances are also not undoable.
    /// </summary>
    internal class NullHistoryMemento
        : HistoryMemento
    {
        protected override HistoryMemento OnUndo()
        {
            throw new InvalidOperationException("NullHistoryMementos are not undoable");
        }

        public NullHistoryMemento(string name, ImageResource image)
            : base(name, image)
        {
        }
    }
}
