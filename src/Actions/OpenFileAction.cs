/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Windows.Forms;

namespace PaintDotNet.Actions
{
    internal sealed class OpenFileAction
        : AppWorkspaceAction
    {
        public override void PerformAction(AppWorkspace appWorkspace)
        {
            string filePath;

            if (appWorkspace.ActiveDocumentWorkspace == null)
            {
                filePath = null;
            }
            else
            {
                // Default to the directory the active document came from
                string fileName;
                FileType fileType;
                SaveConfigToken saveConfigToken;
                appWorkspace.ActiveDocumentWorkspace.GetDocumentSaveOptions(out fileName, out fileType, out saveConfigToken);
                filePath = Path.GetDirectoryName(fileName);
            }

            string[] newFileNames;
            DialogResult result = DocumentWorkspace.ChooseFiles(appWorkspace, out newFileNames, true, filePath);

            if (result == DialogResult.OK)
            {
                appWorkspace.OpenFilesInNewWorkspace(newFileNames);
            }
        }

        public OpenFileAction()
        {
        }
    }
}
