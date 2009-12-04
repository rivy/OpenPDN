/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;

namespace PaintDotNet.Actions
{
    internal sealed class HistoryRewindAction
        : DocumentWorkspaceAction
    {
        public override HistoryMemento PerformAction(DocumentWorkspace documentWorkspace)
        {
            DateTime lastUpdate = DateTime.Now;

            documentWorkspace.History.BeginStepGroup();

            using (new WaitCursorChanger(documentWorkspace))
            {
                documentWorkspace.SuspendToolCursorChanges();

                while (documentWorkspace.History.UndoStack.Count > 1)
                {
                    documentWorkspace.History.StepBackward();

                    if ((DateTime.Now - lastUpdate).TotalMilliseconds >= 500)
                    {
                        documentWorkspace.History.EndStepGroup();
                        documentWorkspace.Update();
                        lastUpdate = DateTime.Now;
                        documentWorkspace.History.BeginStepGroup();
                    }
                }

                documentWorkspace.ResumeToolCursorChanges();
            }

            documentWorkspace.History.EndStepGroup();

            Utility.GCFullCollect();
            documentWorkspace.Document.Invalidate();
            documentWorkspace.Update();

            return null;
        }

        public HistoryRewindAction()
            : base(ActionFlags.KeepToolActive)
        {
            // We use ActionFlags.KeepToolActive because the process of undo/redo has its own
            // set of protocols for determining whether to keep the tool active, or to refresh it
        }
    }
}
