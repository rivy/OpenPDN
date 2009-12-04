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

namespace PaintDotNet
{
    public sealed class RenderedTileEventArgs
        : EventArgs
    {
        private PdnRegion renderedRegion;
        public PdnRegion RenderedRegion
        {
            get
            {
                return renderedRegion;
            }
        }

        private int tileNumber;
        public int TileNumber
        {
            get
            {
                return tileNumber;
            }
        }

        private int tileCount;
        public int TileCount
        {
            get
            {
                return tileCount;
            }
        }

        public RenderedTileEventArgs(PdnRegion renderedRegion, int tileCount, int tileNumber)
        {
            this.renderedRegion = renderedRegion;
            this.tileCount = tileCount;
            this.tileNumber = tileNumber;
        }
    }
}
