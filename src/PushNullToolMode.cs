/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

namespace PaintDotNet
{
    internal sealed class PushNullToolMode
        : IDisposable
    {
        private DocumentWorkspace documentWorkspace;

        public PushNullToolMode(DocumentWorkspace documentWorkspace)
        {
            this.documentWorkspace = documentWorkspace;
            this.documentWorkspace.PushNullTool();
        }

        ~PushNullToolMode()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.documentWorkspace != null)
                {
                    this.documentWorkspace.PopNullTool();
                    this.documentWorkspace = null;
                }
            }
        }
    }
}
