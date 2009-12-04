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
    /// Gets around a limitation in System.Windows.Forms.PaintEventArgs in that it disposes
    /// the Graphics instance that is associated with it when it is disposed.
    /// </summary>
    public delegate void PaintEventHandler2(object sender, PaintEventArgs2 e);
}
