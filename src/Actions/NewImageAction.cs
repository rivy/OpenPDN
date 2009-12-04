/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.SystemLayer;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PaintDotNet.Actions
{
    internal sealed class NewImageAction
        : AppWorkspaceAction
    {
        public override void PerformAction(AppWorkspace appWorkspace)
        {
            using (NewFileDialog nfd = new NewFileDialog())
            {
                Size newDocSize = appWorkspace.GetNewDocumentSize();

                if (Utility.IsClipboardImageAvailable())
                {
                    try
                    {
                        Utility.GCFullCollect();
                        IDataObject clipData = System.Windows.Forms.Clipboard.GetDataObject();

                        using (Image clipImage = (Image)clipData.GetData(DataFormats.Bitmap))
                        {
                            int width2 = clipImage.Width;
                            int height2 = clipImage.Height;
                            newDocSize = new Size(width2, height2);
                        }
                    }

                    catch (Exception ex)
                    {
                        if (ex is OutOfMemoryException ||
                            ex is ExternalException ||
                            ex is NullReferenceException)
                        {
                            // ignore
                        }
                        else
                        {
                            throw;
                        }
                    }
                }

                nfd.OriginalSize = new Size(newDocSize.Width, newDocSize.Height);
                nfd.OriginalDpuUnit = SettingNames.GetLastNonPixelUnits();
                nfd.OriginalDpu = Document.GetDefaultDpu(nfd.OriginalDpuUnit);
                nfd.Units = nfd.OriginalDpuUnit;
                nfd.Resolution = nfd.OriginalDpu;
                nfd.ConstrainToAspect = Settings.CurrentUser.GetBoolean(SettingNames.LastMaintainAspectRatioNF, false);

                DialogResult dr = nfd.ShowDialog(appWorkspace);

                if (dr == DialogResult.OK)
                {
                    bool success = appWorkspace.CreateBlankDocumentInNewWorkspace(new Size(nfd.ImageWidth, nfd.ImageHeight), nfd.Units, nfd.Resolution, false);

                    if (success)
                    {
                        appWorkspace.ActiveDocumentWorkspace.ZoomBasis = ZoomBasis.FitToWindow;
                        Settings.CurrentUser.SetBoolean(SettingNames.LastMaintainAspectRatioNF, nfd.ConstrainToAspect);

                        if (nfd.Units != MeasurementUnit.Pixel)
                        {
                            Settings.CurrentUser.SetString(SettingNames.LastNonPixelUnits, nfd.Units.ToString());
                        }

                        if (appWorkspace.Units != MeasurementUnit.Pixel)
                        {
                            appWorkspace.Units = nfd.Units;
                        }
                    }
                }
            }
        }

        public NewImageAction()
        {
        }
    }
}
