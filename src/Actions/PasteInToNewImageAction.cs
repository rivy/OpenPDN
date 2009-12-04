/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.HistoryMementos;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace PaintDotNet.Actions
{
    internal sealed class PasteInToNewImageAction
        : AppWorkspaceAction
    {
        public override void PerformAction(AppWorkspace appWorkspace)
        {
            try
            {
                IDataObject pasted;
                Image image;

                using (new WaitCursorChanger(appWorkspace))
                {
                    Utility.GCFullCollect();
                    pasted = Clipboard.GetDataObject();
                    image = (Image)pasted.GetData(DataFormats.Bitmap);
                }

                if (image == null)
                {
                    Utility.ErrorBox(appWorkspace, PdnResources.GetString("PasteInToNewImageAction.Error.NoClipboardImage"));
                }
                else
                {
                    Size newSize = image.Size;
                    image.Dispose();
                    image = null;
                    pasted = null;

                    Document document = null;

                    using (new WaitCursorChanger(appWorkspace))
                    {
                        document = new Document(newSize);
                        DocumentWorkspace dw = appWorkspace.AddNewDocumentWorkspace();
                        dw.Document = document;

                        dw.History.PushNewMemento(new NullHistoryMemento(string.Empty, null));

                        PasteInToNewLayerAction pitnla = new PasteInToNewLayerAction(dw);
                        bool result = pitnla.PerformAction();

                        if (result)
                        {
                            dw.Selection.Reset();
                            dw.SetDocumentSaveOptions(null, null, null);
                            dw.History.ClearAll();

                            dw.History.PushNewMemento(
                                new NullHistoryMemento(
                                    PdnResources.GetString("NewImageAction.Name"),
                                    PdnResources.GetImageResource("Icons.MenuLayersAddNewLayerIcon.png")));

                            appWorkspace.ActiveDocumentWorkspace = dw;
                        }
                        else
                        {
                            appWorkspace.RemoveDocumentWorkspace(dw);
                            document.Dispose();
                        }
                    }
                }
            }

            catch (ExternalException)
            {
                Utility.ErrorBox(appWorkspace, PdnResources.GetString("AcquireImageAction.Error.Clipboard.TransferError"));
                return;
            }

            catch (OutOfMemoryException)
            {
                Utility.ErrorBox(appWorkspace, PdnResources.GetString("AcquireImageAction.Error.Clipboard.OutOfMemory"));
                return;
            }

            catch (ThreadStateException)
            {
                // The ApartmentState property of the application is not set to ApartmentState.STA
                // I don't think this one will ever happen, seeing as how Main is tagged with the
                // STA attribute.
                return;
            }
        }
    }
}
