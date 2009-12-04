/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace PaintDotNet.SystemLayer
{
    /// <summary>
    /// Provides methods and properties related to scanning and printing.
    /// </summary>
    public static class ScanningAndPrinting
    {
        private static IWiaInterface wiaInterface = null;

        private static IWiaInterface WiaInterface
        {
            get
            {
                if (wiaInterface == null)
                {
                    // Windows Server 2003 x64 and Windows XP x64 do not have a 64-bit version of wiaaut.dll available
                    if (Environment.OSVersion.Version.Major == 5 &&
                        Environment.OSVersion.Version.Minor == 2 &&
                        Processor.Architecture == ProcessorArchitecture.X64)
                    {
                        wiaInterface = new WiaInterfaceOutOfProc();
                    }
                    else
                    {
                        wiaInterface = new WiaInterfaceInProc();
                    }
                }

                return wiaInterface;
            }
        }

        public static bool IsComponentAvailable
        {
            get
            {
                return WiaInterface.IsComponentAvailable;
            }
        }

        public static bool CanPrint
        {
            get
            {
                return WiaInterface.CanPrint;
            }
        }

        public static bool CanScan
        {
            get
            {
                return WiaInterface.CanScan;
            }
        }

        public static void Print(Control owner, string fileName)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }

            if (!CanPrint)
            {
                throw new InvalidOperationException("Printing is not available");
            }

            string fileNameExt = Path.GetExtension(fileName);
            if (string.Compare(fileNameExt, ".bmp", true) != 0)
            {
                throw new ArgumentException("fileName must have a .bmp extension");
            } 
            
            WiaInterface.Print(owner, fileName);
        }

        public static ScanResult Scan(Control owner, string fileName)
        {
            if (!CanScan)
            {
                throw new InvalidOperationException("Scanning is not available");
            }

            ScanResult result = WiaInterface.Scan(owner, fileName);
            return result;
        }
    }
}
