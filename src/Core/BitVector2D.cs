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
using System.Collections.Specialized;
using System.Drawing;

namespace PaintDotNet
{
    public sealed class BitVector2D
        : IBitVector2D,
          ICloneable
    {
        private BitArray bitArray;
        private int width;
        private int height;

        public int Width
        {
            get
            {
                return width;
            }
        }

        public int Height
        {
            get
            {
                return height;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return (width == 0) || (height == 0);
            }
        }

        public bool this[int x, int y]
        {
            get
            {
                CheckBounds(x, y);
                return bitArray[x + (y * width)];
            }

            set
            {
                CheckBounds(x, y);
                bitArray[x + (y * width)] = value;
            }
        }

        public bool this[System.Drawing.Point pt]
        {
            get
            {
                CheckBounds(pt.X, pt.Y);
                return bitArray[pt.X + (pt.Y * width)];
            }

            set
            {
                CheckBounds(pt.X, pt.Y);
                bitArray[pt.X + (pt.Y * width)] = value;
            }
        }

        public BitVector2D(int width, int height)
        {
            this.width = width;
            this.height = height;
            this.bitArray = new BitArray(width * height, false);
        }

        public BitVector2D(BitVector2D copyMe)
        {
            this.width = copyMe.width;
            this.height = copyMe.height;
            this.bitArray = (BitArray)copyMe.bitArray.Clone();
        }

        private void CheckBounds(int x, int y)
        {
            if (x >= width || y >= height || x < 0 || y < 0)
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        public void Clear(bool newValue)
        {
            bitArray.SetAll(newValue);
        }

        public bool Get(int x, int y)
        {
            return this[x, y];
        }

        public bool GetUnchecked(int x, int y)
        {
            return bitArray[x + (y * width)];
        }

        public void Set(int x, int y, bool newValue)
        {
            this[x, y] = newValue;
        }

        public void Set(Point pt, bool newValue)
        {
            Set(pt.X, pt.Y, newValue);
        }

        public void Set(Rectangle rect, bool newValue)
        {
            for (int y = rect.Top; y < rect.Bottom; ++y)
            {
                for (int x = rect.Left; x < rect.Right; ++x)
                {
                    Set(x, y, newValue);
                }
            }
        }

        public void SetUnchecked(Rectangle rect, bool newValue)
        {
            for (int y = rect.Top; y < rect.Bottom; ++y)
            {
                for (int x = rect.Left; x < rect.Right; ++x)
                {
                    SetUnchecked(x, y, newValue);
                }
            }
        }

        public void Set(Scanline scan, bool newValue)
        {
            int x = scan.X;
            while (x < scan.X + scan.Length)
            {
                Set(x, scan.Y, newValue);
                ++x;
            }
        }

        public void SetUnchecked(Scanline scan, bool newValue)
        {
            int x = scan.X;
            while (x < scan.X + scan.Length)
            {
                SetUnchecked(x, scan.Y, newValue);
                ++x;
            }
        }

        public void Set(PdnRegion region, bool newValue)
        {
            foreach (Rectangle rect in region.GetRegionScansReadOnlyInt())
            {
                Set(rect, newValue);
            }
        }

        public void SetUnchecked(int x, int y, bool newValue)
        {
            bitArray[x + (y * width)] = newValue;
        }

        public void Invert(int x, int y)
        {
            Set(x, y, !Get(x, y));
        }

        public unsafe void InvertUnchecked(int x, int y)
        {
            SetUnchecked(x, y, !GetUnchecked(x, y));
        }

        public void Invert(Point pt)
        {
            Invert(pt.X, pt.Y);
        }

        public void Invert(Rectangle rect)
        {
            for (int y = rect.Top; y < rect.Bottom; ++y)
            {
                for (int x = rect.Left; x < rect.Right; ++x)
                {
                    Invert(x, y);
                }
            }
        }

        public void InvertUnchecked(Rectangle rect)
        {
            for (int y = rect.Top; y < rect.Bottom; ++y)
            {
                for (int x = rect.Left; x < rect.Right; ++x)
                {
                    InvertUnchecked(x, y);
                }
            }
        }

        public void Invert(Scanline scan)
        {
            int x = scan.X;

            while (x < scan.X + scan.Length)
            {
                Invert(x, scan.Y);
                ++x;
            }
        }

        public void InvertUnchecked(Scanline scan)
        {
            int x = scan.X;

            while (x < scan.X + scan.Length)
            {
                InvertUnchecked(x, scan.Y);
                ++x;
            }
        }

        public void Invert(PdnRegion region)
        {
            foreach (Rectangle rect in region.GetRegionScansReadOnlyInt())
            {
                Invert(rect);
            }
        }

        public object Clone()
        {
            return new BitVector2D(this);
        }
    }
}
