/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.IndirectUI;
using PaintDotNet.PropertySystem;
using PaintDotNet.SystemLayer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.Serialization;
using System.Text;

namespace PaintDotNet
{
    public abstract class InternalFileType
         : PropertyBasedFileType
    {
        /// <summary>
        /// The actual bit-depths we can save with.
        /// </summary>
        internal enum SavableBitDepths
        {
            Rgba32, // 2^24 colors, plus a full 8-bit alpha channel
            Rgb24,  // 2^24 colors
            Rgb8,   // 256 colors
            Rgba8   // 255 colors + 1 transparent
        }

        protected unsafe void SquishSurfaceTo24Bpp(Surface surface)
        {
            byte* dst = (byte*)surface.GetRowAddress(0);
            int byteWidth = surface.Width * 3;
            int stride24bpp = ((byteWidth + 3) / 4) * 4; // round up to multiple of 4
            int delta = stride24bpp - byteWidth;

            for (int y = 0; y < surface.Height; ++y)
            {
                ColorBgra* src = surface.GetRowAddress(y);
                ColorBgra* srcEnd = src + surface.Width;

                while (src < srcEnd)
                {
                    dst[0] = src->B;
                    dst[1] = src->G;
                    dst[2] = src->R;
                    ++src;
                    dst += 3;
                }

                dst += delta;
            }

            return;
        }

        private string PrintSet<T>(Set<T> set)
        {
            StringBuilder sb = new StringBuilder();

            bool first = true;

            foreach (T item in set)
            {
                if (!first)
                {
                    sb.Append(", ");
                }

                first = false;

                sb.Append(item.ToString());
            }

            string sbTS = sb.ToString();

            return sbTS;
        }

        internal SavableBitDepths ChooseBitDepth(
            Set<SavableBitDepths> allowedBitDepths,
            Set<SavableBitDepths> losslessBitDepths,
            bool allOpaque,
            bool all0Or255Alpha,
            int uniqueColorCount)
        {
            if (allowedBitDepths.Count == 0)
            {
                throw new ArgumentException("Count must be 1 or more", "allowedBitDepths");
            }

            Tracing.Ping("allowedBitDepths = " + PrintSet(allowedBitDepths));
            Tracing.Ping("losslessBitDepths = " + PrintSet(losslessBitDepths));

            if (allowedBitDepths.Count == 1)
            {
                return allowedBitDepths.ToArray()[0];
            }

            // allowedBitDepths.Count >= 2

            Set<SavableBitDepths> bestBitDepths = Set<SavableBitDepths>.Intersect(allowedBitDepths, losslessBitDepths);

            if (bestBitDepths.Count == 1)
            {
                return bestBitDepths.ToArray()[0];
            }

            Set<SavableBitDepths> candidates;

            if (bestBitDepths.Count == 0)
            {
                candidates = allowedBitDepths;
            }
            else
            {
                candidates = bestBitDepths;
            }

            // candidates.Count >= 2

            // lossless choices

            if (candidates.Contains(SavableBitDepths.Rgba8) && all0Or255Alpha && uniqueColorCount <= 255)
            {
                return SavableBitDepths.Rgba8;
            }

            if (candidates.Contains(SavableBitDepths.Rgb8) && allOpaque && uniqueColorCount <= 256)
            {
                return SavableBitDepths.Rgb8;
            }

            if (candidates.Contains(SavableBitDepths.Rgb24) && allOpaque)
            {
                return SavableBitDepths.Rgb24;
            }

            if (candidates.Contains(SavableBitDepths.Rgba32))
            {
                return SavableBitDepths.Rgba32;
            }

            // forced choices -- we wanted Rgba32 but it was not allowed

            if (candidates.IsEqualTo(Set.Create(SavableBitDepths.Rgb8, SavableBitDepths.Rgb24)))
            {
                return SavableBitDepths.Rgb24;
            }

            if (candidates.IsEqualTo(Set.Create(SavableBitDepths.Rgb8, SavableBitDepths.Rgba8)))
            {
                return SavableBitDepths.Rgba8;
            }

            if (candidates.IsEqualTo(Set.Create(SavableBitDepths.Rgba8, SavableBitDepths.Rgb24)))
            {
                return SavableBitDepths.Rgb24;
            }

            throw new ArgumentException("Could not accomodate input values -- internal error?");
        }

        protected unsafe Bitmap CreateAliased24BppBitmap(Surface surface)
        {
            int stride = surface.Width * 3;
            int realStride = ((stride + 3) / 4) * 4; // round up to multiple of 4
            return new Bitmap(surface.Width, surface.Height, realStride, PixelFormat.Format24bppRgb, new IntPtr(surface.Scan0.VoidStar));
        }

        private unsafe void Analyze(Surface scratchSurface, out bool allOpaque, out bool all0or255Alpha, out int uniqueColorCount)
        {
            allOpaque = true;
            all0or255Alpha = true;
            Set<ColorBgra> uniqueColors = new Set<ColorBgra>();

            for (int y = 0; y < scratchSurface.Height; ++y)
            {
                ColorBgra* srcPtr = scratchSurface.GetRowAddress(y);
                ColorBgra* endPtr = srcPtr + scratchSurface.Width;

                while (srcPtr < endPtr)
                {
                    ColorBgra p = *srcPtr;

                    if (p.A != 255)
                    {
                        allOpaque = false;
                    }

                    if (p.A > 0 && p.A < 255)
                    {
                        all0or255Alpha = false;
                    }

                    if (p.A == 255 && !uniqueColors.Contains(p) && uniqueColors.Count < 300)
                    {
                        uniqueColors.Add(*srcPtr);
                    }

                    ++srcPtr;
                }
            }

            uniqueColorCount = uniqueColors.Count;
        }

        internal abstract Set<SavableBitDepths> CreateAllowedBitDepthListFromToken(PropertyBasedSaveConfigToken token);

        internal abstract int GetThresholdFromToken(PropertyBasedSaveConfigToken token);

        internal abstract int GetDitherLevelFromToken(PropertyBasedSaveConfigToken token);

        protected unsafe override sealed void OnSaveT(
            Document input,
            Stream output,
            PropertyBasedSaveConfigToken token,
            Surface scratchSurface,
            ProgressEventHandler progressCallback)
        {
            // flatten the document -- render w/ transparent background
            scratchSurface.Clear(ColorBgra.Transparent);

            using (RenderArgs ra = new RenderArgs(scratchSurface))
            {
                input.Render(ra, false);
            }

            // load properties from token
            int thresholdFromToken = GetThresholdFromToken(token);
            int ditherLevel = GetDitherLevelFromToken(token);

            Set<SavableBitDepths> allowedBitDepths = CreateAllowedBitDepthListFromToken(token);

            if (allowedBitDepths.Count == 0)
            {
                throw new ArgumentException("there must be at least 1 element returned from CreateAllowedBitDepthListFromToken()");
            }

            // allowedBitDepths.Count >= 1

            // set to 1 unless allowedBitDepths contains only Rgb8 and Rgba8
            int threshold;

            if (allowedBitDepths.IsSubsetOf(Set.Create(SavableBitDepths.Rgb8, SavableBitDepths.Rgba8)))
            {
                threshold = thresholdFromToken;
            }
            else
            {
                threshold = 1;
            }

            // Analyze image, try to detect what bit-depth or whatever to use, based on allowedBitDepths
            bool allOpaque;
            bool all0or255Alpha;
            int uniqueColorCount;

            Analyze(scratchSurface, out allOpaque, out all0or255Alpha, out uniqueColorCount);

            Set<SavableBitDepths> losslessBitDepths = new Set<SavableBitDepths>();
            losslessBitDepths.Add(SavableBitDepths.Rgba32);

            if (allOpaque)
            {
                losslessBitDepths.Add(SavableBitDepths.Rgb24);

                if (uniqueColorCount <= 256)
                {
                    losslessBitDepths.Add(SavableBitDepths.Rgb8);
                }
            }
            else if (all0or255Alpha && uniqueColorCount < 256)
            {
                losslessBitDepths.Add(SavableBitDepths.Rgba8);
            }

            SavableBitDepths bitDepth = ChooseBitDepth(allowedBitDepths, losslessBitDepths, allOpaque, all0or255Alpha, uniqueColorCount);

            if (bitDepth == SavableBitDepths.Rgba8 && threshold == 0 && allowedBitDepths.Contains(SavableBitDepths.Rgba8) && allowedBitDepths.Contains(SavableBitDepths.Rgb8))
            {
                // threshold of 0 should effectively force full 256 color palette, instead of 255+1 transparent
                bitDepth = SavableBitDepths.Rgb8;
            }

            // if bit depth is 24 or 8, then we have to do away with the alpha channel
            // for 8-bit, we must have pixels that have either 0 or 255 alpha
            if (bitDepth == SavableBitDepths.Rgb8 ||
                bitDepth == SavableBitDepths.Rgba8 ||
                bitDepth == SavableBitDepths.Rgb24)
            {
                UserBlendOps.NormalBlendOp blendOp = new UserBlendOps.NormalBlendOp();

                for (int y = 0; y < scratchSurface.Height; ++y)
                {
                    for (int x = 0; x < scratchSurface.Width; ++x)
                    {
                        ColorBgra p = scratchSurface[x, y];

                        if (p.A < threshold && bitDepth == SavableBitDepths.Rgba8)
                        {
                            p = ColorBgra.FromBgra(0, 0, 0, 0);
                        }
                        else
                        {
                            p = blendOp.Apply(ColorBgra.White, p);
                        }

                        scratchSurface[x, y] = p;
                    }
                }
            }

            Tracing.Ping("Chose " + bitDepth + ", ditherLevel=" + ditherLevel + ", threshold=" + threshold);

            // finally, do the save.
            FinalSave(input, output, scratchSurface, ditherLevel, bitDepth, token, progressCallback);
        }

        internal abstract void FinalSave(
            Document input,
            Stream output,
            Surface scratchSurface,
            int ditherLevel,
            SavableBitDepths bitDepth,
            PropertyBasedSaveConfigToken token,
            ProgressEventHandler progressCallback);

        internal InternalFileType(string name, FileTypeFlags flags, string[] extensions)
            : base(name, flags, extensions)
        {
        }
    }
}
