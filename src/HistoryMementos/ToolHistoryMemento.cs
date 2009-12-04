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
    /// A Tool may implement this class in order to provide history actions that do
    /// not deactivate the tool while being undone or redone.
    /// </summary>
    internal abstract class ToolHistoryMemento
        : HistoryMemento
    {
        private DocumentWorkspace documentWorkspace;
        private Type toolType;

        protected DocumentWorkspace DocumentWorkspace
        {
            get
            {
                return this.documentWorkspace;
            }
        }

        public Type ToolType
        {
            get
            {
                return this.toolType;
            }
        }

        protected abstract HistoryMemento OnToolUndo();

        protected sealed override HistoryMemento OnUndo()
        {
            if (this.documentWorkspace.GetToolType() != this.toolType)
            {
                this.documentWorkspace.SetToolFromType(this.toolType);
            }

            return OnToolUndo();
        }

        public ToolHistoryMemento(DocumentWorkspace documentWorkspace, string name, ImageResource image)
            : base(name, image)
        {
            this.documentWorkspace = documentWorkspace;
            this.toolType = documentWorkspace.GetToolType();
        }
    }
}
