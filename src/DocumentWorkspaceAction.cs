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
    /// <summary>
    /// Provides a way to do a tool-less action that operates on the DocumentWorkspace.
    /// DocumentActions must NOT touch directly the History -- they should return history 
    /// actions that can undo what they have already done. These history actions will
    /// then be placed in to the history by whomever invoked the DocumentAction.
    /// If the action does not affect the Document, it should return null from its
    /// PerformAction method.
    /// DocumentActions should ONLY mutate the DocumentWorkspace and any contained
    /// objects.
    /// </summary>
    internal abstract class DocumentWorkspaceAction
    {
        private ActionFlags actionFlags;
        public ActionFlags ActionFlags
        {
            get
            {
                return this.actionFlags;
            }
        }

        /// <summary>
        /// Implement this to provide an action. You must return a HistoryMemento so that you
        /// can be undone. However, you should return null if you didn't do anything that
        /// affected the document.
        /// </summary>
        /// <returns>A HistoryMemento object that will be placed onto the HistoryStack.</returns>
        public abstract HistoryMemento PerformAction(DocumentWorkspace documentWorkspace);

        /// <summary>
        /// Initializes an instance of a class dervied from DocumentAction.
        /// </summary>
        /// <param name="documentWorkspace">The DocumentWorkspace to interact with.</param>
        /// <param name="actionFlags">Flags that describe action behavior or requirements.</param>
        public DocumentWorkspaceAction(ActionFlags actionFlags)
        {
            this.actionFlags = actionFlags;
            SystemLayer.Tracing.LogFeature("DWAction(" + GetType().Name + ")");
        }
    }
}
