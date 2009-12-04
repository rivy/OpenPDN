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
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace PaintDotNet.SystemLayer
{
    public static class Clipboard
    {
        public static Image GetEmfFromClipboard(IWin32Window currentWindow)
        {
            Image returnVal = null;

            if (NativeMethods.OpenClipboard(currentWindow.Handle))
            {
                try
                {
                    if (NativeMethods.IsClipboardFormatAvailable(NativeConstants.CF_ENHMETAFILE))
                    {
                        IntPtr hEmf = NativeMethods.GetClipboardData(NativeConstants.CF_ENHMETAFILE);

                        if (hEmf != IntPtr.Zero)
                        {
                            try
                            {
                                Metafile metafile = new Metafile(hEmf, true);
                                returnVal = metafile;
                            }

                            catch (Exception)
                            {

                            }
                        }
                    }
                }

                finally
                {
                    NativeMethods.CloseClipboard();
                }
            }

            GC.KeepAlive(currentWindow);
            return returnVal;
        }
    }
}
