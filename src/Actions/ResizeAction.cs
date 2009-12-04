/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet;
using PaintDotNet.HistoryMementos;
using PaintDotNet.SystemLayer;
using PaintDotNet.Threading;
using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Threading;
using System.Windows.Forms;

namespace PaintDotNet.Actions
{
    // TODO: split in to Action and Function
    internal sealed class ResizeAction 
        : DocumentWorkspaceAction
    {
        public static string StaticName
        {
            get
            {
                return PdnResources.GetString("ResizeAction.Name");
            }
        }

        public static ImageResource StaticImage
        {
            get
            {
                return PdnResources.GetImageResource("Icons.MenuImageResizeIcon.png");
            }
        }

        private sealed class FitSurfaceContext
        {
            private Surface dstSurface;
            private Surface srcSurface;
            private Rectangle[] dstRois;
            private ResamplingAlgorithm algorithm;

            public Surface DstSurface
            {
                get
                {
                    return dstSurface;
                }
            }

            public Surface SrcSurface
            {
                get
                {
                    return srcSurface;
                }
            }

            public Rectangle[] DstRois
            {
                get
                {
                    return dstRois;
                }
            }

            public ResamplingAlgorithm Algorithm
            {
                get
                {
                    return algorithm;
                }
            }

            public event Procedure RenderedRect;
            private void OnRenderedRect()
            {
                if (RenderedRect != null)
                {
                    RenderedRect();
                }
            }

            public void FitSurface(object context)
            {
                int index = (int)context;
                dstSurface.FitSurface(algorithm, srcSurface, dstRois[index]);
                OnRenderedRect();
            }

            public FitSurfaceContext(Surface dstSurface, Surface srcSurface, Rectangle[] dstRois, ResamplingAlgorithm algorithm)
            {
                this.dstSurface = dstSurface;
                this.srcSurface = srcSurface;
                this.dstRois = dstRois;
                this.algorithm = algorithm;
            }
        }

        private static BitmapLayer ResizeLayer(BitmapLayer layer, int width, int height, ResamplingAlgorithm algorithm,
            int tileCount, Procedure progressCallback, ref bool pleaseStopMonitor)
        {
            Surface surface = new Surface(width, height);
            surface.Clear(ColorBgra.FromBgra(255, 255, 255, 0));

            PaintDotNet.Threading.ThreadPool threadPool = new PaintDotNet.Threading.ThreadPool();
            int rectCount;
            
            if (tileCount == 0)
            {
                rectCount = Processor.LogicalCpuCount;
            }
            else
            {
                rectCount = tileCount;
            }

            Rectangle[] rects = new Rectangle[rectCount];
            Utility.SplitRectangle(surface.Bounds, rects);

            FitSurfaceContext fsc = new FitSurfaceContext(surface, layer.Surface, rects, algorithm);

            if (progressCallback != null)
            {
                fsc.RenderedRect += progressCallback;
            }

            WaitCallback callback = new WaitCallback(fsc.FitSurface);

            for (int i = 0; i < rects.Length; ++i)
            {
                if (pleaseStopMonitor)
                {
                    break;
                }
                else
                {
                    threadPool.QueueUserWorkItem(callback, BoxedConstants.GetInt32(i));
                }
            }

            threadPool.Drain();
            threadPool.DrainExceptions();

            if (pleaseStopMonitor)
            {
                surface.Dispose();
                surface = null;
            }

            BitmapLayer newLayer;

            if (surface == null)
            {
                newLayer = null;
            }
            else
            {
                newLayer = new BitmapLayer(surface, true);
                newLayer.LoadProperties(layer.SaveProperties());
            }

            if (progressCallback != null)
            {
                fsc.RenderedRect -= progressCallback;
            }

            return newLayer;
        }

        public static BitmapLayer ResizeLayer(BitmapLayer layer, int width, int height, ResamplingAlgorithm algorithm)
        {
            bool pleaseStop = false;
            return ResizeLayer(layer, width, height, algorithm, 0, null, ref pleaseStop);
        }

        private class ResizeProgressDialog
            : CallbackWithProgressDialog
        {
            private int maxTiles;
            private int tilesCompleted = 0;
            private int tilesPerLayer;
            private Document dst;
            private Document src;
            private Size newSize;
            private ResamplingAlgorithm algorithm;
            private bool returnVal;
            private bool pleaseStop = false;

            public ResizeProgressDialog(Control owner, Document dst, Document src, Size newSize, ResamplingAlgorithm algorithm)
                : base (owner, PdnInfo.GetBareProductName(), PdnResources.GetString("ResizeAction.ProgressDialog.Description"))
            {
                this.dst = dst;
                this.src = src;
                this.newSize = newSize;
                this.algorithm = algorithm;
                this.tilesPerLayer = 50 * Processor.LogicalCpuCount;
                this.maxTiles = tilesPerLayer * src.Layers.Count;
                this.Icon = Utility.ImageToIcon(StaticImage.Reference);
            }

            protected override void OnCancelClick()
            {
                this.pleaseStop = true;
                base.OnCancelClick();
            }

            private void RenderedRectHandler()
            {
                this.Owner.BeginInvoke(new Procedure(MarshaledProgressUpdate));
            }

            private void MarshaledProgressUpdate()
            {
                ++tilesCompleted;
                double progress = 100.0 * ((double)tilesCompleted / (double)maxTiles);
                this.Progress = (int)Math.Round(progress);
            }

            public bool DoResize()
            {
                DialogResult result = this.ShowDialog(true, false, new ThreadStart(ResizeDocument));

                if (!this.returnVal && !this.Cancelled)
                {
                    Utility.ErrorBox(this.Owner, PdnResources.GetString("ResizeAction.PerformAction.UnspecifiedError"));
                }

                return this.returnVal;
            }

            private void ResizeDocument()
            {
                this.pleaseStop = false;

                // This is only sort of a hack: we must try and allocate enough for 2 extra layer-sized buffers
                // Then we free them immediately. This is just so that if we don't have enough memory that we'll
                // fail sooner rather than later.
                Surface s1 = new Surface(this.newSize);
                Surface s2 = new Surface(this.newSize);

                try
                {
                    foreach (Layer layer in src.Layers)
                    {
                        if (this.pleaseStop)
                        {
                            this.returnVal = false;
                            return;
                        }

                        if (layer is BitmapLayer)
                        {
                            Layer newLayer = ResizeLayer((BitmapLayer)layer, this.newSize.Width, this.newSize.Height, this.algorithm,
                                this.tilesPerLayer, new Procedure(RenderedRectHandler), ref this.pleaseStop);

                            if (newLayer == null)
                            {
                                this.returnVal = false;
                                return;
                            }

                            dst.Layers.Add(newLayer);
                        }
                        else
                        {
                            throw new InvalidOperationException("Resize does not support Layers that are not BitmapLayers");
                        }
                    }
                }

                finally
                {
                    s1.Dispose();
                    s2.Dispose();
                }

                this.returnVal = true;
            }
        }

        public override HistoryMemento PerformAction(DocumentWorkspace documentWorkspace)
        {
            int newWidth;
            int newHeight;
            double newDpu;
            MeasurementUnit newDpuUnit;

            string resamplingAlgorithm = Settings.CurrentUser.GetString(SettingNames.LastResamplingMethod, 
                ResamplingAlgorithm.SuperSampling.ToString());

            ResamplingAlgorithm alg;
            
            try
            {
                alg = (ResamplingAlgorithm)Enum.Parse(typeof(ResamplingAlgorithm), resamplingAlgorithm, true);
            }

            catch
            {
                alg = ResamplingAlgorithm.SuperSampling;
            }

            bool maintainAspect = Settings.CurrentUser.GetBoolean(SettingNames.LastMaintainAspectRatio, true);

            using (ResizeDialog rd = new ResizeDialog())
            {
                rd.OriginalSize = documentWorkspace.Document.Size;
                rd.OriginalDpuUnit = documentWorkspace.Document.DpuUnit;
                rd.OriginalDpu = documentWorkspace.Document.DpuX;
                rd.ImageHeight = documentWorkspace.Document.Height;
                rd.ImageWidth = documentWorkspace.Document.Width;
                rd.ResamplingAlgorithm = alg;
                rd.LayerCount = documentWorkspace.Document.Layers.Count;
                rd.Units = rd.OriginalDpuUnit;
                rd.Resolution = documentWorkspace.Document.DpuX;
                rd.Units = SettingNames.GetLastNonPixelUnits();
                rd.ConstrainToAspect = maintainAspect;
            
                DialogResult result = rd.ShowDialog(documentWorkspace);

                if (result == DialogResult.Cancel)
                {
                    return null;
                }

                Settings.CurrentUser.SetString(SettingNames.LastResamplingMethod, rd.ResamplingAlgorithm.ToString());
                Settings.CurrentUser.SetBoolean(SettingNames.LastMaintainAspectRatio, rd.ConstrainToAspect);
                newDpuUnit = rd.Units;
                newWidth = rd.ImageWidth;
                newHeight = rd.ImageHeight;
                newDpu = rd.Resolution;
                alg = rd.ResamplingAlgorithm;

                if (newDpuUnit != MeasurementUnit.Pixel)
                {
                    Settings.CurrentUser.SetString(SettingNames.LastNonPixelUnits, newDpuUnit.ToString());

                    if (documentWorkspace.AppWorkspace.Units != MeasurementUnit.Pixel)
                    {
                        documentWorkspace.AppWorkspace.Units = newDpuUnit;
                    }
                }

                // if the new size equals the old size, there's really no point in doing anything
                if (documentWorkspace.Document.Size == new Size(rd.ImageWidth, rd.ImageHeight) &&
                    documentWorkspace.Document.DpuX == newDpu &&
                    documentWorkspace.Document.DpuUnit == newDpuUnit)
                {
                    return null;
                }
            }

            HistoryMemento ha;

            if (newWidth == documentWorkspace.Document.Width &&
                newHeight == documentWorkspace.Document.Height)
            {
                // Only adjusting Dpu or DpuUnit
                ha = new MetaDataHistoryMemento(StaticName, StaticImage, documentWorkspace);
                documentWorkspace.Document.DpuUnit = newDpuUnit;
                documentWorkspace.Document.DpuX = newDpu;
                documentWorkspace.Document.DpuY = newDpu;
            }
            else
            {
                try
                {
                    using (new WaitCursorChanger(documentWorkspace))
                    {
                        ha = new ReplaceDocumentHistoryMemento(StaticName, StaticImage, documentWorkspace);
                    }

                    Document newDocument = new Document(newWidth, newHeight);
                    newDocument.ReplaceMetaDataFrom(documentWorkspace.Document);
                    newDocument.DpuUnit = newDpuUnit;
                    newDocument.DpuX = newDpu;
                    newDocument.DpuY = newDpu;
                    ResizeProgressDialog rpd = new ResizeProgressDialog(documentWorkspace, newDocument, documentWorkspace.Document, new Size(newWidth, newHeight), alg);
                    Utility.GCFullCollect();
                    bool result = rpd.DoResize();

                    if (!result)
                    {
                        return null;
                    }

                    documentWorkspace.Document = newDocument;
                }

                catch (WorkerThreadException ex)
                {
                    if (ex.InnerException is OutOfMemoryException)
                    {
                        Utility.ErrorBox(documentWorkspace, PdnResources.GetString("ResizeAction.PerformAction.OutOfMemory"));
                        return null;
                    }
                    else
                    {
                        throw;
                    }
                }

                catch (OutOfMemoryException)
                {
                    Utility.ErrorBox(documentWorkspace, PdnResources.GetString("ResizeAction.PerformAction.OutOfMemory"));
                    return null;
                }
            }

            return ha;
        }

        public ResizeAction() 
            : base(ActionFlags.KeepToolActive)
        {
            // We use ActionFlags.KeepToolActive because opening this dialog does not necessitate
            // refreshing the tool. This is handled by DocumentWorkspace.set_Document as appropriate.
        }
    }
}
