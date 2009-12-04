/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PaintDotNet.SystemLayer
{
    internal sealed class WiaInterfaceInProc
        : IWiaInterface
    {
        public bool IsComponentAvailable
        {
            get
            {
                return IsWia2Available();
            }
        }

        public bool CanPrint
        {
            get
            {
                return IsWia2Available();
            }
        }

        public bool CanScan
        {
            get
            {
                bool result;

                if (IsWia2Available())
                {
                    WIA.DeviceManagerClass dmc = new WIA.DeviceManagerClass();

                    if (dmc.DeviceInfos.Count > 0)
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }

                    Marshal.ReleaseComObject(dmc);
                    dmc = null;
                }
                else
                {
                    result = false;
                }

                return result;
            }
        }

        public void Print(Control owner, string fileName)
        {
            Tracing.Enter();

            WIA.VectorClass vector = new WIA.VectorClass();
            object tempName_o = (object)fileName;
            vector.Add(ref tempName_o, 0);
            object vector_o = (object)vector;
            WIA.CommonDialogClass cdc = new WIA.CommonDialogClass();

            // Ok, this looks weird, but here's the story.
            // When we show the WIA printing dialog, it is a modal dialog but the way
            // it handles itself is that the main window can still be interacted with.
            // I don't know why, and it doesn't matter to me except that it causes all
            // sorts of other problems (esp. related to the scratch surface.)
            // So we show a modal dialog that is effectively invisible so that the user
            // cannot interact with the main window while the print dialog is still open.

            Form modal = new Form();
            modal.ShowInTaskbar = false;
            modal.TransparencyKey = modal.BackColor;
            modal.FormBorderStyle = FormBorderStyle.None;

            modal.Shown +=
                delegate(object sender, EventArgs e)
                {
                    cdc.ShowPhotoPrintingWizard(ref vector_o);
                    modal.Close();
                };

            modal.ShowDialog(owner);
            modal = null;

            Marshal.ReleaseComObject(cdc);
            cdc = null;

            Tracing.Leave();
        }

        public ScanResult Scan(Control owner, string fileName)
        {
            ScanResult result;

            WIA.CommonDialogClass cdc = new WIA.CommonDialogClass();
            WIA.ImageFile imageFile = null;

            try
            {
                imageFile = cdc.ShowAcquireImage(WIA.WiaDeviceType.UnspecifiedDeviceType,
                                                 WIA.WiaImageIntent.UnspecifiedIntent,
                                                 WIA.WiaImageBias.MaximizeQuality,
                                                 "{00000000-0000-0000-0000-000000000000}",
                                                 true,
                                                 true,
                                                 false);

                Marshal.ReleaseComObject(cdc);
                cdc = null;
            }

            catch (System.Runtime.InteropServices.COMException)
            {
                result = ScanResult.DeviceBusy;
                imageFile = null;
            }

            if (imageFile != null)
            {
                imageFile.SaveFile(fileName);
                result = ScanResult.Success;
            }
            else
            {
                result = ScanResult.UserCancelled;
            }

            return result;
        }

        // Have to split this in to two functions because the WIA DLL is resolved
        // at the time a function is entered that depends on it (IsWia2AvailableImpl).
        // This way we can avoid a runtime error and maintain the semantics of
        // IsWia2Available().

        private static bool IsWia2Available()
        {
            try
            {
                return IsWia2AvailableImpl();
            }

            catch
            {
                return false;
            }
        }

        private static bool IsWia2AvailableImpl()
        {
            try
            {
                WIA.DeviceManagerClass dmc = new WIA.DeviceManagerClass();
                Marshal.ReleaseComObject(dmc);
                dmc = null;

                return true;
            }

            catch
            {
                return false;
            }
        }
    }
}
