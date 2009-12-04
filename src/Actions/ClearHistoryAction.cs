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
using System.Windows.Forms;

namespace PaintDotNet.Actions
{
    internal sealed class ClearHistoryAction
        : DocumentWorkspaceAction
    {
        public override HistoryMemento PerformAction(DocumentWorkspace documentWorkspace)
        {
            if (DialogResult.Yes == Utility.AskYesNo(documentWorkspace, 
                PdnResources.GetString("ClearHistory.Confirmation")))
            {
                documentWorkspace.History.ClearAll();

                documentWorkspace.History.PushNewMemento(new NullHistoryMemento(
                    PdnResources.GetString("ClearHistory.HistoryMementoName"),
                    PdnResources.GetImageResource("Icons.MenuLayersDeleteLayerIcon.png")));
            }

            return null;
        }

        public ClearHistoryAction()
            : base(ActionFlags.None)
        {
        }
    }
}
