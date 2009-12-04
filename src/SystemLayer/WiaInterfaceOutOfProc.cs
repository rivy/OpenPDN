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
    // Because wiaaut.dll is not available in 64-bit form for Windows Server 2003 or Windows XP,
    // we must out-of-proc this stuff.

    internal sealed class WiaInterfaceOutOfProc
          : IWiaInterface
    {
        private const string wiaProxy32ExeName = "WiaProxy32.exe";

        public int CallWiaProxy32(string args, bool spinEvents)
        {
            string ourPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            string proxyPath = Path.Combine(ourPath, wiaProxy32ExeName);
            ProcessStartInfo psi = new ProcessStartInfo(proxyPath, args);

            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;

            int exitCode = -1;

            try
            {
                Process process = Process.Start(psi);

                // Can't just use process.WaitForExit() because then the Paint.NET UI
                // will not repaint and it'll look weird because of that.
                while (!process.HasExited)
                {
                    if (spinEvents)
                    {
                        Application.DoEvents();
                    }

                    Thread.Sleep(10);
                }

                exitCode = process.ExitCode;
                process.Dispose();
            }

            catch (Exception)
            {
            }

            return exitCode;
        }

        /// <summary>
        /// Gets whether or not the scanning and printing features are available without
        /// taking into account whether a scanner or printer are actually connected.
        /// </summary>
        public bool IsComponentAvailable
        {
            get
            {
                return 1 == CallWiaProxy32("IsComponentAvailable 1", false);
            }
        }

        /// <summary>
        /// Gets whether printing is possible. This does not take into account whether a printer
        /// is actually connected or available, just that it is possible to print (it is possible
        /// that the printing UI has a facility for adding or loading a new printer).
        /// </summary>
        public bool CanPrint
        {
            get
            {
                return 1 == CallWiaProxy32("CanPrint 1", false);
            }
        }

        /// <summary>
        /// Gets whether scanning is possible. The user must have a scanner connect for this to return true.
        /// </summary>
        /// <remarks>
        /// This also covers image acquisition from, say, a camera.
        /// </remarks>
        public bool CanScan
        {
            get
            {
                return 1 == CallWiaProxy32("CanScan 1", false);
            }
        }

        /// <summary>
        /// Presents a user interface for printing the given image.
        /// </summary>
        /// <param name="owner">The parent/owner control for the UI that will be presented for printing.</param>
        /// <param name="fileName">The name of a file containing a bitmap (.BMP) to print.</param>
        public void Print(Control owner, string fileName)
        {
            // Disable the entire UI, otherwise it's possible to close PDN while the
            // print wizard is active! And then it crashes.
            Form ownedForm = owner.FindForm();
            bool[] ownedFormsEnabled = null;

            if (ownedForm != null)
            {
                ownedFormsEnabled = new bool[ownedForm.OwnedForms.Length];

                for (int i = 0; i < ownedForm.OwnedForms.Length; ++i)
                {
                    ownedFormsEnabled[i] = ownedForm.OwnedForms[i].Enabled;
                    ownedForm.OwnedForms[i].Enabled = false;
                }

                ownedForm.Enabled = false;
            }

            CallWiaProxy32("Print \"" + fileName + "\"", true);

            if (ownedForm != null)
            {
                for (int i = 0; i < ownedForm.OwnedForms.Length; ++i)
                {
                    ownedForm.OwnedForms[i].Enabled = ownedFormsEnabled[i];
                }

                ownedForm.Enabled = true;
                ownedForm.Activate();
            }
        }

        /// <summary>
        /// Presents a user interface for scanning.
        /// </summary>
        /// <param name="fileName">
        /// The filename of where to stored the scanned/acquired image. Only valid if the return value is ScanResult.Success.
        /// </param>
        /// <returns>The result of the scanning operation.</returns>
        public ScanResult Scan(Control owner, string fileName)
        {
            if (!CanScan)
            {
                throw new InvalidOperationException("Scanning is not available");
            }

            // Disable the entire UI, otherwise it's possible to close PDN while the
            // print wizard is active! And then it crashes.
            Form ownedForm = owner.FindForm();
            bool[] ownedFormsEnabled = null;

            if (ownedForm != null)
            {
                ownedFormsEnabled = new bool[ownedForm.OwnedForms.Length];

                for (int i = 0; i < ownedForm.OwnedForms.Length; ++i)
                {
                    ownedFormsEnabled[i] = ownedForm.OwnedForms[i].Enabled;
                    ownedForm.OwnedForms[i].Enabled = false;
                }

                owner.FindForm().Enabled = false;
            }

            // Do scanning
            int retVal = CallWiaProxy32("Scan \"" + fileName + "\"", true);

            // Un-disable everything
            if (ownedForm != null)
            {
                for (int i = 0; i < ownedForm.OwnedForms.Length; ++i)
                {
                    ownedForm.OwnedForms[i].Enabled = ownedFormsEnabled[i];
                }

                owner.FindForm().Enabled = true;
            }

            owner.FindForm().Activate();

            // Marshal the return code
            ScanResult result = (ScanResult)retVal;

            if (!Enum.IsDefined(typeof(ScanResult), result))
            {
                throw new ApplicationException("WiaProxy32 returned an error: " + retVal.ToString());
            }

            return result;
        }
    }
}
