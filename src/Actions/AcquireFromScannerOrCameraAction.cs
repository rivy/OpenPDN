/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.HistoryMementos;
using PaintDotNet.SystemLayer;
using System;
using System.Drawing;
using System.IO;

namespace PaintDotNet.Actions
{
    internal sealed class AcquireFromScannerOrCameraAction
        : AppWorkspaceAction
    {
        public override void PerformAction(AppWorkspace appWorkspace)
        {
            if (!ScanningAndPrinting.CanScan)
            {
                Utility.ShowWiaError(appWorkspace);
                return;
            }

            string tempName = Path.ChangeExtension(SystemLayer.FileSystem.GetTempFileName(), ".bmp");
            ScanResult result;

            try
            {
                result = ScanningAndPrinting.Scan(appWorkspace, tempName);
            }

            // If there was an exception, let's assume the user has already received an error dialog,
            // either from Windows or from the WIA UI, and let's /not/ present another error dialog.
            catch (Exception)
            {
                result = ScanResult.UserCancelled;
            }

            if (result == ScanResult.Success)
            {
                string errorText = null;

                try
                {
                    Image image;

                    try
                    {
                        image = PdnResources.LoadImage(tempName);
                    }

                    catch (FileNotFoundException)
                    {
                        errorText = PdnResources.GetString("LoadImage.Error.FileNotFoundException");
                        throw;
                    }

                    catch (OutOfMemoryException)
                    {
                        errorText = PdnResources.GetString("LoadImage.Error.OutOfMemoryException");
                        throw;
                    }

                    Document document;

                    try
                    {
                        document = Document.FromImage(image);
                    }

                    catch (OutOfMemoryException)
                    {
                        errorText = PdnResources.GetString("LoadImage.Error.OutOfMemoryException");
                        throw;
                    }

                    finally
                    {
                        image.Dispose();
                        image = null;
                    }

                    DocumentWorkspace dw = appWorkspace.AddNewDocumentWorkspace();

                    try
                    {
                        dw.Document = document;
                    }

                    catch (OutOfMemoryException)
                    {
                        errorText = PdnResources.GetString("LoadImage.Error.OutOfMemoryException");
                        throw;
                    }

                    document = null;
                    dw.SetDocumentSaveOptions(null, null, null);
                    dw.History.ClearAll();

                    HistoryMemento newHA = new NullHistoryMemento(
                        PdnResources.GetString("AcquireImageAction.Name"),
                        PdnResources.GetImageResource("Icons.MenuLayersAddNewLayerIcon.png"));

                    dw.History.PushNewMemento(newHA);

                    appWorkspace.ActiveDocumentWorkspace = dw;

                    // Try to delete the temp file but don't worry if we can't
                    try
                    {
                        File.Delete(tempName);
                    }

                    catch
                    {
                    }
                }

                catch (Exception)
                {
                    if (errorText != null)
                    {
                        Utility.ErrorBox(appWorkspace, errorText);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }
    }
}
