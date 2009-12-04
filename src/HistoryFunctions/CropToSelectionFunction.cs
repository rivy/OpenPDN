/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.HistoryMementos;
using PaintDotNet.Actions;
using System;
using System.Drawing;

namespace PaintDotNet.HistoryFunctions
{
    /// <summary>
    /// Crops the image to the currently selected region.
    /// </summary>
    internal sealed class CropToSelectionFunction
        : HistoryFunction
    {
        public static string StaticName
        {
            get
            {
                return PdnResources.GetString("CropAction.Name");
            }
        }

        public override HistoryMemento OnExecute(IHistoryWorkspace historyWorkspace)
        {
            if (historyWorkspace.Selection.IsEmpty)
            {
                return null;
            }
            else
            {
                PdnRegion selectionRegion = historyWorkspace.Selection.CreateRegion();

                if (selectionRegion.GetArea() == 0)
                {
                    selectionRegion.Dispose();
                    return null;
                }

                SelectionHistoryMemento sha = new SelectionHistoryMemento(StaticName, null, historyWorkspace);
                ReplaceDocumentHistoryMemento rdha = new ReplaceDocumentHistoryMemento(StaticName, null, historyWorkspace);
                Rectangle boundingBox;
                Rectangle[] inverseRegionRects = null;

                boundingBox = Utility.GetRegionBounds(selectionRegion);

                using (PdnRegion inverseRegion = new PdnRegion(boundingBox))
                {
                    inverseRegion.Exclude(selectionRegion);

                    inverseRegionRects = Utility.TranslateRectangles(
                        inverseRegion.GetRegionScansReadOnlyInt(),
                        -boundingBox.X,
                        -boundingBox.Y);
                }

                selectionRegion.Dispose();
                selectionRegion = null;

                Document oldDocument = historyWorkspace.Document; // TODO: serialize this to disk so we don't *have* to store the full thing
                Document newDocument = new Document(boundingBox.Width, boundingBox.Height);

                // copy the document's meta data over
                newDocument.ReplaceMetaDataFrom(oldDocument);

                foreach (Layer layer in oldDocument.Layers)
                {
                    if (layer is BitmapLayer)
                    {
                        BitmapLayer oldLayer = (BitmapLayer)layer;
                        Surface croppedSurface = oldLayer.Surface.CreateWindow(boundingBox);
                        BitmapLayer newLayer = new BitmapLayer(croppedSurface);

                        ColorBgra clearWhite = ColorBgra.White.NewAlpha(0);

                        foreach (Rectangle rect in inverseRegionRects)
                        {
                            newLayer.Surface.Clear(rect, clearWhite);
                        }

                        newLayer.LoadProperties(oldLayer.SaveProperties());
                        newDocument.Layers.Add(newLayer);
                    }
                    else
                    {
                        throw new InvalidOperationException("Crop does not support Layers that are not BitmapLayers");
                    }
                }

                CompoundHistoryMemento cha = new CompoundHistoryMemento(
                    StaticName,
                    PdnResources.GetImageResource("Icons.MenuImageCropIcon.png"),
                    new HistoryMemento[] { sha, rdha });

                EnterCriticalRegion();
                historyWorkspace.Document = newDocument;

                return cha;
            }
        }

        public CropToSelectionFunction()
            : base(ActionFlags.None)
        {
        }
    }
}
