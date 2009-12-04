/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.Actions;
using PaintDotNet.HistoryMementos;
using System;
using System.Drawing.Drawing2D;

namespace PaintDotNet.HistoryFunctions
{
    internal sealed class SelectAllFunction
        : HistoryFunction
    {
        public static string StaticName
        {
            get
            {
                return PdnResources.GetString("SelectAllAction.Name");
            }
        }

        public override HistoryMemento OnExecute(IHistoryWorkspace historyWorkspace)
        {
            SelectionHistoryMemento sha = new SelectionHistoryMemento(
                StaticName, 
                PdnResources.GetImageResource("Icons.MenuEditSelectAllIcon.png"),
                historyWorkspace);

            EnterCriticalRegion();
            historyWorkspace.Selection.PerformChanging();
            historyWorkspace.Selection.Reset();
            historyWorkspace.Selection.SetContinuation(historyWorkspace.Document.Bounds, CombineMode.Replace);
            historyWorkspace.Selection.CommitContinuation();
            historyWorkspace.Selection.PerformChanged();

            return sha;
        }

        public SelectAllFunction()
            : base(ActionFlags.None)
        {
        }
    }
}
