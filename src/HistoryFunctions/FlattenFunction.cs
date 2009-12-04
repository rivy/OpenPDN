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

namespace PaintDotNet.HistoryFunctions
{
    internal sealed class FlattenFunction
        : HistoryFunction
    {
        public static string StaticName
        {
            get
            {
                return PdnResources.GetString("FlattenFunction.Name");
            }
        }

        public override HistoryMemento OnExecute(IHistoryWorkspace historyWorkspace)
        {
            object savedSelection = null;
            List<HistoryMemento> actions = new List<HistoryMemento>();

            if (!historyWorkspace.Selection.IsEmpty)
            {
                savedSelection = historyWorkspace.Selection.Save();
                DeselectFunction da = new DeselectFunction();
                HistoryMemento hm = da.Execute(historyWorkspace);
                actions.Add(hm);
            }

            ReplaceDocumentHistoryMemento rdha = new ReplaceDocumentHistoryMemento(null, null, historyWorkspace);
            actions.Add(rdha);

            CompoundHistoryMemento chm = new CompoundHistoryMemento(
                StaticName,
                PdnResources.GetImageResource("Icons.MenuImageFlattenIcon.png"),
                actions);

            // TODO: we can save memory here by serializing, then flattening on to an existing layer
            Document flat = historyWorkspace.Document.Flatten();

            EnterCriticalRegion();
            historyWorkspace.Document = flat;

            if (savedSelection != null)
            {
                SelectionHistoryMemento shm = new SelectionHistoryMemento(null, null, historyWorkspace);
                historyWorkspace.Selection.Restore(savedSelection);
                chm.PushNewAction(shm);
            }

            return chm;
        }

        public FlattenFunction()
            : base(ActionFlags.None)
        {
        }
    }
}
