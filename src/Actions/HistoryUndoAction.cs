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

namespace PaintDotNet.Actions
{
    internal sealed class HistoryUndoAction
        : DocumentWorkspaceAction
    {
        public override HistoryMemento PerformAction(DocumentWorkspace documentWorkspace)
        {
            if (documentWorkspace.History.UndoStack.Count > 0)
            {
                if (!(documentWorkspace.History.UndoStack[documentWorkspace.History.UndoStack.Count - 1] is NullHistoryMemento))
                {
                    using (new WaitCursorChanger(documentWorkspace.FindForm()))
                    {
                        documentWorkspace.History.StepBackward();
                        documentWorkspace.Update();
                    }
                }

                Utility.GCFullCollect();
            }

            return null;
        }

        public HistoryUndoAction()
            : base(ActionFlags.KeepToolActive)
        {
            // We use ActionFlags.KeepToolActive because the process of undo/redo has its own
            // set of protocols for determine whether to keep the tool active, or to refresh it
        }
    }
}
