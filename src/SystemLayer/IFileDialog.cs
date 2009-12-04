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
using System.Reflection;
using System.Windows.Forms;

namespace PaintDotNet.SystemLayer
{
    public interface IFileDialog
        : IDisposable
    {
        bool CheckPathExists
        {
            get;
            set;
        }

        bool DereferenceLinks 
        { 
            get; 
            set; 
        }

        string Filter 
        { 
            get; 
            set; 
        }

        int FilterIndex 
        { 
            get; 
            set; 
        }

        string InitialDirectory
        {
            get;
            set;
        }

        string Title 
        { 
            set; 
        }

        /// <summary>
        /// Shows the common file dialog.
        /// </summary>
        /// <param name="owner">The owning window for this dialog.</param>
        /// <param name="uiCallbacks">
        /// A reference to an object that implements the IFileDialogUICallbacks interface. This
        /// may not be null.</param>
        DialogResult ShowDialog(IWin32Window owner, IFileDialogUICallbacks uiCallbacks);
    }
}
