/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Drawing;

namespace PaintDotNet.Data.Quantize
{
    public sealed class PaletteTable
    {
        private Color[] palette;

        public Color this[int index]
        {
            get
            {
                return this.palette[index];
            }

            set
            {
                this.palette[index] = value;
            }
        }

        private int GetDistanceSquared(Color a, Color b)
        {
            int dsq = 0; // delta squared
            int v; 

            v = a.B - b.B;
            dsq += v * v;
            v = a.G - b.G;
            dsq += v * v;
            v = a.R - b.R;
            dsq += v * v;

            return dsq;
        }

        public int FindClosestPaletteIndex(Color pixel)
        {
            int dsqBest = int.MaxValue;
            int ret = 0;

            for (int i = 0; i < this.palette.Length; ++i)
            {
                int dsq = GetDistanceSquared(this.palette[i], pixel);

                if (dsq < dsqBest)
                {
                    dsqBest = dsq;
                    ret = i;
                }
            }

            return ret;
        }

        public PaletteTable(Color[] palette)
        {
            this.palette = (Color[])palette.Clone();
        }
    }
}
