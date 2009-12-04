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
    internal sealed class HistoryRedoAction
        : DocumentWorkspaceAction
    {
        public override HistoryMemento PerformAction(DocumentWorkspace documentWorkspace)
        {
            if (documentWorkspace.History.RedoStack.Count > 0)
            {
                if (!(documentWorkspace.History.RedoStack[documentWorkspace.History.RedoStack.Count - 1] is NullHistoryMemento))
                {
                    using (new WaitCursorChanger(documentWorkspace.FindForm()))
                    {
                        documentWorkspace.History.StepForward();
                        documentWorkspace.Update();
                    }
                }

                Utility.GCFullCollect();
            }

            return null;
        }

        public HistoryRedoAction()
            : base(ActionFlags.KeepToolActive)
        {
            // We use ActionFlags.KeepToolActive because the process of undo/redo has its own
            // set of protocols for determine whether to keep the tool active, or to refresh it
        }
    }
}
