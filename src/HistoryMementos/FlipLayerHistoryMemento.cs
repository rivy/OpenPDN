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

namespace PaintDotNet.HistoryMementos
{
    internal class FlipLayerHistoryMemento
        : HistoryMemento
    {
        private IHistoryWorkspace historyWorkspace;
        private int layerIndex;
        private FlipType flipType;

        private void Flip(Surface surface)
        {
            switch (this.flipType)
            {
                case FlipType.Horizontal:
                    for (int y = 0; y < surface.Height; ++y)
                    {
                        for (int x = 0; x < surface.Width / 2; ++x)
                        {
                            ColorBgra temp = surface[x, y];
                            surface[x, y] = surface[surface.Width - x - 1, y];
                            surface[surface.Width - x - 1, y] = temp;
                        }
                    }

                    break;

                case FlipType.Vertical:
                    for (int x = 0; x < surface.Width; ++x)
                    {
                        for (int y = 0; y < surface.Height / 2; ++y)
                        {
                            ColorBgra temp = surface[x, y];
                            surface[x, y] = surface[x, surface.Height - y - 1];
                            surface[x, surface.Height - y - 1] = temp;
                        }
                    }

                    break;

                default:
                    throw new InvalidOperationException("FlipType was invalid");
            }

            return;
        }

        protected override HistoryMemento OnUndo()
        {
            FlipLayerHistoryMemento fha = new FlipLayerHistoryMemento(this.Name, this.Image, 
                this.historyWorkspace, layerIndex, flipType);

            BitmapLayer layer = (BitmapLayer)this.historyWorkspace.Document.Layers[layerIndex];
            Flip(layer.Surface);
            layer.Invalidate();
            return fha;
        }

        public FlipLayerHistoryMemento(string name, ImageResource image, IHistoryWorkspace historyWorkspace, int layerIndex, FlipType flipType)
            : base(name, image)
        {
            this.historyWorkspace = historyWorkspace;
            this.layerIndex = layerIndex;
            this.flipType = flipType;
        }
    }
}
