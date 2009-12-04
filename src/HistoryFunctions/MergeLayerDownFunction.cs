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
using PaintDotNet.HistoryMementos;

namespace PaintDotNet.HistoryFunctions
{
    internal sealed class MergeLayerDownFunction
        : HistoryFunction
    {
        public static string StaticName
        {
            get
            {
                return PdnResources.GetString("MergeLayerDown.HistoryMementoName");
            }
        }

        public static ImageResource StaticImage
        {
            get
            {
                return PdnResources.GetImageResource("Icons.MenuLayersMergeLayerDownIcon.png");
            }   
        }

        private int layerIndex;

        public override HistoryMemento OnExecute(IHistoryWorkspace historyWorkspace)
        {
            if (this.layerIndex < 1 || this.layerIndex >= historyWorkspace.Document.Layers.Count)
            {
                throw new ArgumentException("layerIndex must be greater than or equal to 1, and a valid layer index. layerIndex=" + 
                    layerIndex + ", allowableRange=[0," + historyWorkspace.Document.Layers.Count + ")");
            }

            int bottomLayerIndex = this.layerIndex - 1;
            Rectangle bounds = historyWorkspace.Document.Bounds;
            PdnRegion region = new PdnRegion(bounds);

            BitmapHistoryMemento bhm = new BitmapHistoryMemento(
                null,
                null,
                historyWorkspace,
                bottomLayerIndex,
                region);

            BitmapLayer topLayer = (BitmapLayer)historyWorkspace.Document.Layers[this.layerIndex];
            BitmapLayer bottomLayer = (BitmapLayer)historyWorkspace.Document.Layers[bottomLayerIndex];
            RenderArgs bottomRA = new RenderArgs(bottomLayer.Surface);

            EnterCriticalRegion();

            topLayer.Render(bottomRA, region);
            bottomLayer.Invalidate();

            bottomRA.Dispose();
            bottomRA = null;

            region.Dispose();
            region = null;

            DeleteLayerFunction dlf = new DeleteLayerFunction(this.layerIndex);
            HistoryMemento dlhm = dlf.Execute(historyWorkspace);

            CompoundHistoryMemento chm = new CompoundHistoryMemento(StaticName, StaticImage, new HistoryMemento[] { bhm, dlhm });
            return chm;
        }

        public MergeLayerDownFunction(int layerIndex)
            : base(ActionFlags.None)
        {
            this.layerIndex = layerIndex;
        }
    }
}
