/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Windows.Forms;

namespace PaintDotNet.Actions
{
    internal sealed class ClearMruListAction
        : AppWorkspaceAction
    {
        public override void PerformAction(AppWorkspace appWorkspace)
        {
            string question = PdnResources.GetString("ClearOpenRecentList.Dialog.Text");
            DialogResult result = Utility.AskYesNo(appWorkspace, question);

            if (result == DialogResult.Yes)
            {
                appWorkspace.MostRecentFiles.Clear();
                appWorkspace.MostRecentFiles.SaveMruList();
            }
        }

        public ClearMruListAction()
        {
        }
    }
}
