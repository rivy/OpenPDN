/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.HistoryFunctions;
using PaintDotNet.HistoryMementos;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace PaintDotNet.Actions
{
    // TODO: split into Action and Function(s)
    internal sealed class ImportFromFileAction
        : DocumentWorkspaceAction
    {
        public static string StaticName
        {
            get
            {
                return PdnResources.GetString("ImportFromFileAction.Name");
            }
        }

        public static ImageResource StaticImage
        {
            get
            {
                return PdnResources.GetImageResource("Icons.MenuLayersImportFromFileIcon.png");
            }
        }

        private void Rollback(List<HistoryMemento> historyMementos)
        {
            for (int i = historyMementos.Count - 1; i >= 0; i--)
            {
                HistoryMemento ha = historyMementos[i];
                ha.PerformUndo();
            }
        }

        private HistoryMemento DoCanvasResize(DocumentWorkspace documentWorkspace, Size newLayerSize)
        {
            HistoryMemento retHA;

            int layerIndex = documentWorkspace.ActiveLayerIndex;

            Size newSize = new Size(Math.Max(newLayerSize.Width, documentWorkspace.Document.Width),
                Math.Max(newLayerSize.Height, documentWorkspace.Document.Height));

            Document newDoc;
            
            try
            {
                using (new WaitCursorChanger(documentWorkspace))
                {
                    Utility.GCFullCollect();

                    newDoc = CanvasSizeAction.ResizeDocument(documentWorkspace.Document, newSize, 
                        AnchorEdge.TopLeft, documentWorkspace.AppWorkspace.AppEnvironment.SecondaryColor);
                }
            }

            catch (OutOfMemoryException)
            {
                Utility.ErrorBox(documentWorkspace, PdnResources.GetString("ImportFromFileAction.AskForCanvasResize.OutOfMemory"));
                newDoc = null;
            }

            if (newDoc == null)
            {
                retHA = null;
            }
            else
            {
                retHA = new ReplaceDocumentHistoryMemento(string.Empty, null, documentWorkspace);

                using (new WaitCursorChanger(documentWorkspace))
                {
                    documentWorkspace.Document = newDoc;
                }

                documentWorkspace.ActiveLayer = (Layer)documentWorkspace.Document.Layers[layerIndex];
            }

            return retHA;
        }

        private HistoryMemento ImportOneLayer(DocumentWorkspace documentWorkspace, BitmapLayer layer)
        {
            HistoryMemento retHA;
            List<HistoryMemento> historyMementos = new List<HistoryMemento>();
            bool success = true;
            
            if (success)
            {
                if (!documentWorkspace.Selection.IsEmpty)
                {
                    HistoryMemento ha = new DeselectFunction().Execute(documentWorkspace);
                    historyMementos.Add(ha);
                }
            }

            if (success)
            {
                if (layer.Width > documentWorkspace.Document.Width ||
                    layer.Height > documentWorkspace.Document.Height)
                {
                    HistoryMemento ha = DoCanvasResize(documentWorkspace, layer.Size);
                
                    if (ha == null)
                    {
                        success = false;
                    }
                    else
                    {
                        historyMementos.Add(ha);
                    }
                }
            }

            if (success)
            {
                if (layer.Size != documentWorkspace.Document.Size)
                {
                    BitmapLayer newLayer;
                    
                    try
                    {
                        using (new WaitCursorChanger(documentWorkspace))
                        {
                            Utility.GCFullCollect();

                            newLayer = CanvasSizeAction.ResizeLayer((BitmapLayer)layer, documentWorkspace.Document.Size, 
                                AnchorEdge.TopLeft, ColorBgra.White.NewAlpha(0));
                        }
                    }

                    catch (OutOfMemoryException)
                    {
                        Utility.ErrorBox(documentWorkspace, PdnResources.GetString("ImportFromFileAction.ImportOneLayer.OutOfMemory"));
                        success = false;
                        newLayer = null;
                    }

                    if (newLayer != null)
                    {
                        layer.Dispose();
                        layer = newLayer;
                    }
                }
            }

            if (success)
            {
                NewLayerHistoryMemento nlha = new NewLayerHistoryMemento(string.Empty, null, documentWorkspace, documentWorkspace.Document.Layers.Count);
                documentWorkspace.Document.Layers.Add(layer);
                historyMementos.Add(nlha);
            }

            if (success)
            {
                HistoryMemento[] has = historyMementos.ToArray();
                retHA = new CompoundHistoryMemento(string.Empty, null, has);
            }
            else
            {
                Rollback(historyMementos);
                retHA = null;
            }

            return retHA;
        }

        /// <summary>
        /// Presents a user interface and performs the operations required for importing an entire document.
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        /// <remarks>
        /// This function will take ownership of the Document given to it, and will Dispose() of it.
        /// </remarks>
        private HistoryMemento ImportDocument(DocumentWorkspace documentWorkspace, Document document, out Rectangle lastLayerBounds)
        {
            List<HistoryMemento> historyMementos = new List<HistoryMemento>();
            bool[] selected;

            selected = new bool[document.Layers.Count];
            for (int i = 0; i < selected.Length; ++i)
            {
                selected[i] = true;
            }

            lastLayerBounds = Rectangle.Empty;

            if (selected != null)
            {
                List<Layer> layers = new List<Layer>();

                for (int i = 0; i < selected.Length; ++i)
                {
                    if (selected[i])
                    {
                        layers.Add((Layer)document.Layers[i]);
                    }
                }

                foreach (Layer layer in layers)
                {
                    document.Layers.Remove(layer);
                }

                document.Dispose();
                document = null;

                foreach (Layer layer in layers)
                {
                    lastLayerBounds = layer.Bounds;
                    HistoryMemento ha = ImportOneLayer(documentWorkspace, (BitmapLayer)layer);

                    if (ha != null)
                    {
                        historyMementos.Add(ha);
                    }
                    else
                    {
                        Rollback(historyMementos);
                        historyMementos.Clear();
                        break;
                    }
                }
            }

            if (document != null)
            {
                document.Dispose();
                document = null;
            }

            if (historyMementos.Count > 0)
            {
                HistoryMemento[] has = historyMementos.ToArray();
                return new CompoundHistoryMemento(string.Empty, null, has);
            }
            else
            {
                lastLayerBounds = Rectangle.Empty;
                return null;
            }
        }

        private HistoryMemento ImportOneFile(DocumentWorkspace documentWorkspace, string fileName, out Rectangle lastLayerBounds)
        {
            documentWorkspace.AppWorkspace.Widgets.StatusBarProgress.ResetProgressStatusBar();

            ProgressEventHandler progressCallback = (s, e) =>
                {
                    documentWorkspace.AppWorkspace.Widgets.StatusBarProgress.SetProgressStatusBar(e.Percent);
                };

            FileType fileType;
            Document document = DocumentWorkspace.LoadDocument(documentWorkspace, fileName, out fileType, progressCallback);

            documentWorkspace.AppWorkspace.Widgets.StatusBarProgress.EraseProgressStatusBar();

            if (document != null)
            {
                string name = Path.ChangeExtension(Path.GetFileName(fileName), null);
                string newLayerNameFormat = PdnResources.GetString("ImportFromFileAction.ImportOneFile.NewLayer.Format");

                foreach (Layer layer in document.Layers)
                {
                    layer.Name = string.Format(newLayerNameFormat, name, layer.Name);
                    layer.IsBackground = false;
                }

                HistoryMemento ha = ImportDocument(documentWorkspace, document, out lastLayerBounds);
                return ha;
            }
            else
            {
                lastLayerBounds = Rectangle.Empty;
                return null;
            }
        }

        public HistoryMemento ImportMultipleFiles(DocumentWorkspace documentWorkspace, string[] fileNames)
        {
            HistoryMemento retHA = null;
            List<HistoryMemento> historyMementos = new List<HistoryMemento>();
            Rectangle lastLayerBounds = Rectangle.Empty;

            foreach (string fileName in fileNames)
            {
                HistoryMemento ha = ImportOneFile(documentWorkspace, fileName, out lastLayerBounds);

                if (ha != null)
                {
                    historyMementos.Add(ha);
                }
                else
                {
                    Rollback(historyMementos);
                    historyMementos.Clear();
                    break;
                }
            }

            if (lastLayerBounds.Width > 0 && lastLayerBounds.Height > 0)
            {
                SelectionHistoryMemento sha = new SelectionHistoryMemento(null, null, documentWorkspace);
                historyMementos.Add(sha);
                documentWorkspace.Selection.PerformChanging();
                documentWorkspace.Selection.Reset();
                documentWorkspace.Selection.SetContinuation(lastLayerBounds, System.Drawing.Drawing2D.CombineMode.Replace);
                documentWorkspace.Selection.CommitContinuation();
                documentWorkspace.Selection.PerformChanged();
            }

            if (historyMementos.Count > 0)
            {
                HistoryMemento[] haArray = historyMementos.ToArray();
                retHA = new CompoundHistoryMemento(StaticName, StaticImage, haArray);
            }

            return retHA;
        }

        public override HistoryMemento PerformAction(DocumentWorkspace documentWorkspace)
        {
            string[] fileNames;
            string startingDir = Path.GetDirectoryName(documentWorkspace.FilePath);
            DialogResult result = DocumentWorkspace.ChooseFiles(documentWorkspace, out fileNames, true, startingDir);
            HistoryMemento retHA = null;

            if (result == DialogResult.OK)
            {
                Type oldToolType = documentWorkspace.GetToolType();
                documentWorkspace.SetTool(null);

                retHA = ImportMultipleFiles(documentWorkspace, fileNames);

                Type newToolType;
                if (retHA != null)
                {
                    CompoundHistoryMemento cha = new CompoundHistoryMemento(StaticName, StaticImage, new HistoryMemento[] { retHA });
                    retHA = cha;
                    newToolType = typeof(Tools.MoveTool);
                }
                else
                {
                    newToolType = oldToolType;
                }

                documentWorkspace.SetToolFromType(newToolType);
            }

            return retHA;
        }

        public ImportFromFileAction()
            : base(ActionFlags.KeepToolActive)
        {
            // We use ActionFlags.KeepToolActive because opening this dialog does not necessitate
            // refreshing the tool. This is handled by PerformAction() as appropriate.
            // The tool should only be changed if the action is performed, but not if the dialog
            // is cancelled out of.
        }
    }
}
