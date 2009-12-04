/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.HistoryFunctions;
using System;
using System.Collections.Generic;

namespace PaintDotNet.Actions
{
    internal sealed class PasteInToNewLayerAction
    {
        private DocumentWorkspace documentWorkspace;

        public bool PerformAction()
        {
            HistoryFunctionResult hfr = this.documentWorkspace.ExecuteFunction(new AddNewBlankLayerFunction());

            if (hfr == HistoryFunctionResult.Success)
            {
                PasteAction pa = new PasteAction(this.documentWorkspace);
                bool result = pa.PerformAction();

                if (!result)
                {
                    using (new WaitCursorChanger(this.documentWorkspace))
                    {
                        this.documentWorkspace.History.StepBackward();
                    }
                }
                else
                {
                    return true;
                }
            }

            return false;
        }

        public PasteInToNewLayerAction(DocumentWorkspace documentWorkspace)
        {
            this.documentWorkspace = documentWorkspace;
        }
    }
}
