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

namespace PaintDotNet.SystemLayer
{
    public static class CommonDialogs
    {
        public static IFileOpenDialog CreateFileOpenDialog()
        {
            if (OS.IsVistaOrLater)
            {
                return new VistaFileOpenDialog();
            }
            else
            {
                return new ClassicFileOpenDialog();
            }
        }

        public static IFileSaveDialog CreateFileSaveDialog()
        {
            if (OS.IsVistaOrLater)
            {
                return new VistaFileSaveDialog();
            }
            else
            {
                return new ClassicFileSaveDialog();
            }
        }
    }
}
