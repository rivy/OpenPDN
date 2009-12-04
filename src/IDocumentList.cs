/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;

namespace PaintDotNet
{
    internal interface IDocumentList
    {
        /// <summary>
        /// This event is raised when the user clicks on a Document in the list.
        /// </summary>
        event EventHandler<EventArgs<Pair<DocumentWorkspace, DocumentClickAction>>> DocumentClicked;

        event EventHandler DocumentListChanged;

        DocumentWorkspace[] DocumentList
        {
            get;
        }

        int DocumentCount
        {
            get;
        }

        void AddDocumentWorkspace(DocumentWorkspace addMe);
        void RemoveDocumentWorkspace(DocumentWorkspace removeMe);
        void SelectDocumentWorkspace(DocumentWorkspace selectMe);
    }
}
