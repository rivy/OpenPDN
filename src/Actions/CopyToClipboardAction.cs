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
using System.Threading;
using System.Windows.Forms;

namespace PaintDotNet.Actions
{
    internal sealed class CopyToClipboardAction
    {
        private DocumentWorkspace documentWorkspace;

        public bool PerformAction()
        {
            bool success = true;

            if (this.documentWorkspace.Selection.IsEmpty ||
                !(this.documentWorkspace.ActiveLayer is BitmapLayer))
            {
                return false;
            }

            try
            {
                using (new WaitCursorChanger(this.documentWorkspace))
                {
                    Utility.GCFullCollect();
                    PdnRegion selectionRegion = this.documentWorkspace.Selection.CreateRegion();
                    PdnGraphicsPath selectionOutline = this.documentWorkspace.Selection.CreatePath();
                    BitmapLayer activeLayer = (BitmapLayer)this.documentWorkspace.ActiveLayer;
                    RenderArgs renderArgs = new RenderArgs(activeLayer.Surface);
                    MaskedSurface maskedSurface = new MaskedSurface(renderArgs.Surface, selectionOutline);
                    SurfaceForClipboard surfaceForClipboard = new SurfaceForClipboard(maskedSurface);
                    Rectangle selectionBounds = Utility.GetRegionBounds(selectionRegion);

                    if (selectionBounds.Width > 0 && selectionBounds.Height > 0)
                    {
                        Surface copySurface = new Surface(selectionBounds.Width, selectionBounds.Height);
                        Bitmap copyBitmap = copySurface.CreateAliasedBitmap();
                        Bitmap copyOpaqueBitmap = new Bitmap(copySurface.Width, copySurface.Height, PixelFormat.Format24bppRgb);

                        using (Graphics copyBitmapGraphics = Graphics.FromImage(copyBitmap))
                        {
                            copyBitmapGraphics.Clear(Color.White);
                        }

                        maskedSurface.Draw(copySurface, -selectionBounds.X, -selectionBounds.Y);

                        using (Graphics copyOpaqueBitmapGraphics = Graphics.FromImage(copyOpaqueBitmap))
                        {
                            copyOpaqueBitmapGraphics.Clear(Color.White);
                            copyOpaqueBitmapGraphics.DrawImage(copyBitmap, 0, 0);
                        }

                        DataObject dataObject = new DataObject();

                        dataObject.SetData(DataFormats.Bitmap, copyOpaqueBitmap);
                        dataObject.SetData(surfaceForClipboard);

                        int retryCount = 2;

                        while (retryCount >= 0)
                        {
                            try
                            {
                                using (new WaitCursorChanger(this.documentWorkspace))
                                {
                                    Clipboard.SetDataObject(dataObject, true);
                                }

                                break;
                            }

                            catch
                            {
                                if (retryCount == 0)
                                {
                                    success = false;
                                    Utility.ErrorBox(this.documentWorkspace,
                                        PdnResources.GetString("CopyAction.Error.TransferToClipboard"));
                                }
                                else
                                {
                                    Thread.Sleep(200);
                                }
                            }

                            finally
                            {
                                --retryCount;
                            }
                        }

                        copySurface.Dispose();
                        copyBitmap.Dispose();
                        copyOpaqueBitmap.Dispose();
                    }

                    selectionRegion.Dispose();
                    selectionOutline.Dispose();
                    renderArgs.Dispose();
                    maskedSurface.Dispose();
                }
            }

            catch (OutOfMemoryException)
            {
                success = false;
                Utility.ErrorBox(this.documentWorkspace, PdnResources.GetString("CopyAction.Error.OutOfMemory"));
            }

            catch (Exception)
            {
                success = false;
                Utility.ErrorBox(this.documentWorkspace, PdnResources.GetString("CopyAction.Error.Generic"));
            }

            Utility.GCFullCollect();
            return success;
        }

        public CopyToClipboardAction(DocumentWorkspace documentWorkspace)
        {
            SystemLayer.Tracing.LogFeature("CopyToClipboardAction");
            this.documentWorkspace = documentWorkspace;
        }
    }
}
