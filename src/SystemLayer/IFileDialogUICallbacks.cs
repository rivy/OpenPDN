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

namespace PaintDotNet.SystemLayer
{
    public interface IFileDialogUICallbacks
    {
        FileOverwriteAction ShowOverwritePrompt(IWin32Window owner, string pathName);
        bool ShowError(IWin32Window owner, string pathName, Exception ex); // return true if error dialog shown, or false to cause caller to re-throw
        IFileTransferProgressEvents CreateFileTransferProgressEvents();
    }
}
