/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

// Based on: http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dnaspp/html/colorquant.asp

using PaintDotNet;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace PaintDotNet.Data.Quantize
{
    internal unsafe abstract class Quantizer
    {
        /// <summary>
        /// Flag used to indicate whether a single pass or two passes are needed for quantization.
        /// </summary>
        private bool singlePass;
        
        protected int ditherLevel;
        public int DitherLevel
        {
            get
            {
                return this.ditherLevel;
            }

            set
            {
                this.ditherLevel = value;
            }
        }

        /// <summary>
        /// Construct the quantizer
        /// </summary>
        /// <param name="singlePass">If true, the quantization only needs to loop through the source pixels once</param>
        /// <remarks>
        /// If you construct this class with a true value for singlePass, then the code will, when quantizing your image,
        /// only call the 'QuantizeImage' function. If two passes are required, the code will call 'InitialQuantizeImage'
        /// and then 'QuantizeImage'.
        /// </remarks>
        public Quantizer(bool singlePass)
        {
            this.singlePass = singlePass;
        }

        /// <summary>
        /// Quantize an image and return the resulting output bitmap
        /// </summary>
        /// <param name="source">The image to quantize</param>
        /// <returns>A quantized version of the image</returns>
        public Bitmap Quantize(Image source, ProgressEventHandler progressCallback)
        {
            // Get the size of the source image
            int height = source.Height;
            int width = source.Width;

            // And construct a rectangle from these dimensions
            Rectangle bounds = new Rectangle(0, 0, width, height);

            // First off take a 32bpp version of the image
            Bitmap img32bpp;
            
            if (source is Bitmap && source.PixelFormat == PixelFormat.Format32bppArgb)
            {
                img32bpp = (Bitmap)source;
            }
            else
            {
                img32bpp = new Bitmap(width, height, PixelFormat.Format32bppArgb);

                // Now lock the bitmap into memory
                using (Graphics g = Graphics.FromImage(img32bpp))
                {
                    g.PageUnit = GraphicsUnit.Pixel;

                    // Draw the source image onto the copy bitmap,
                    // which will effect a widening as appropriate.
                    g.DrawImage(source, 0, 0, bounds.Width, bounds.Height);
                }
            }

            // And construct an 8bpp version
            Bitmap output = new Bitmap(width, height, PixelFormat.Format8bppIndexed);

            // Define a pointer to the bitmap data
            BitmapData sourceData = null;

            try
            {
                // Get the source image bits and lock into memory
                sourceData = img32bpp.LockBits(bounds, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                // Call the FirstPass function if not a single pass algorithm.
                // For something like an octree quantizer, this will run through
                // all image pixels, build a data structure, and create a palette.
                if (!singlePass)
                {
                    FirstPass(sourceData, width, height, progressCallback);
                }

                // Then set the color palette on the output bitmap. I'm passing in the current palette 
                // as there's no way to construct a new, empty palette.
                output.Palette = this.GetPalette(output.Palette);

                // Then call the second pass which actually does the conversion
                SecondPass(sourceData, output, width, height, bounds, progressCallback);
            }

            finally
            {
                // Ensure that the bits are unlocked
                img32bpp.UnlockBits(sourceData);
            }

            if (img32bpp != source)
            {
                img32bpp.Dispose();
                img32bpp = null;
            }

            // Last but not least, return the output bitmap
            return output;
        }

        /// <summary>
        /// Execute the first pass through the pixels in the image
        /// </summary>
        /// <param name="sourceData">The source data</param>
        /// <param name="width">The width in pixels of the image</param>
        /// <param name="height">The height in pixels of the image</param>
        protected virtual void FirstPass(BitmapData sourceData, int width, int height, ProgressEventHandler progressCallback)
        {
            // Define the source data pointers. The source row is a byte to
            // keep addition of the stride value easier (as this is in bytes)
            byte* pSourceRow = (byte*)sourceData.Scan0.ToPointer();
            Int32* pSourcePixel;

            // Loop through each row
            for (int row = 0; row < height; row++)
            {
                // Set the source pixel to the first pixel in this row
                pSourcePixel = (Int32*)pSourceRow;

                // And loop through each column
                for (int col = 0; col < width; col++, pSourcePixel++)
                {
                    InitialQuantizePixel((ColorBgra *)pSourcePixel);
                }

                // Add the stride to the source row
                pSourceRow += sourceData.Stride;

                if (progressCallback != null)
                {
                    progressCallback(this, new ProgressEventArgs(100.0 * (((double)(row + 1) / (double)height) / 2.0)));
                }
            }
        }

        /// <summary>
        /// Execute a second pass through the bitmap
        /// </summary>
        /// <param name="sourceData">The source bitmap, locked into memory</param>
        /// <param name="output">The output bitmap</param>
        /// <param name="width">The width in pixels of the image</param>
        /// <param name="height">The height in pixels of the image</param>
        /// <param name="bounds">The bounding rectangle</param>
        protected virtual void SecondPass(BitmapData sourceData, Bitmap output, int width, int height, Rectangle bounds, ProgressEventHandler progressCallback)
        {
            BitmapData outputData = null;
            Color[] pallete = output.Palette.Entries;
            int weight = ditherLevel;

            try
            {
                // Lock the output bitmap into memory
                outputData = output.LockBits(bounds, ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);

                // Define the source data pointers. The source row is a byte to
                // keep addition of the stride value easier (as this is in bytes)
                byte* pSourceRow = (byte *)sourceData.Scan0.ToPointer();
                Int32* pSourcePixel = (Int32 *)pSourceRow;

                // Now define the destination data pointers
                byte* pDestinationRow = (byte *)outputData.Scan0.ToPointer();
                byte* pDestinationPixel = pDestinationRow;

                int[] errorThisRowR = new int[width + 1];
                int[] errorThisRowG = new int[width + 1];
                int[] errorThisRowB = new int[width + 1];

                for (int row = 0; row < height; row++)
                {
                    int[] errorNextRowR = new int[width + 1];
                    int[] errorNextRowG = new int[width + 1];
                    int[] errorNextRowB = new int[width + 1];

                    int ptrInc;

                    if ((row & 1) == 0)
                    {
                        pSourcePixel = (Int32*)pSourceRow;
                        pDestinationPixel = pDestinationRow;
                        ptrInc = +1;
                    }
                    else
                    {
                        pSourcePixel = (Int32*)pSourceRow + width - 1;
                        pDestinationPixel = pDestinationRow + width - 1;
                        ptrInc = -1;
                    }

                    // Loop through each pixel on this scan line
                    for (int col = 0; col < width; ++col)
                    {
                        // Quantize the pixel
                        ColorBgra srcPixel = *(ColorBgra *)pSourcePixel;
                        ColorBgra target = new ColorBgra();

                        target.B = Utility.ClampToByte(srcPixel.B - ((errorThisRowB[col] * weight) / 8));
                        target.G = Utility.ClampToByte(srcPixel.G - ((errorThisRowG[col] * weight) / 8));
                        target.R = Utility.ClampToByte(srcPixel.R - ((errorThisRowR[col] * weight) / 8));
                        target.A = srcPixel.A;

                        byte pixelValue = QuantizePixel(&target);
                        *pDestinationPixel = pixelValue;
                    
                        ColorBgra actual = ColorBgra.FromColor(pallete[pixelValue]);

                        int errorR = actual.R - target.R;
                        int errorG = actual.G - target.G;
                        int errorB = actual.B - target.B; 

                        // Floyd-Steinberg Error Diffusion:
                        // a) 7/16 error goes to x+1
                        // b) 5/16 error goes to y+1
                        // c) 3/16 error goes to x-1,y+1
                        // d) 1/16 error goes to x+1,y+1

                        const int a = 7;
                        const int b = 5;
                        const int c = 3;

                        int errorRa = (errorR * a) / 16;
                        int errorRb = (errorR * b) / 16;
                        int errorRc = (errorR * c) / 16;
                        int errorRd = errorR - errorRa - errorRb - errorRc;

                        int errorGa = (errorG * a) / 16;
                        int errorGb = (errorG * b) / 16;
                        int errorGc = (errorG * c) / 16;
                        int errorGd = errorG - errorGa - errorGb - errorGc;

                        int errorBa = (errorB * a) / 16;
                        int errorBb = (errorB * b) / 16;
                        int errorBc = (errorB * c) / 16;
                        int errorBd = errorB - errorBa - errorBb - errorBc;

                        errorThisRowR[col + 1] += errorRa;
                        errorThisRowG[col + 1] += errorGa;
                        errorThisRowB[col + 1] += errorBa;

                        errorNextRowR[width - col] += errorRb;
                        errorNextRowG[width - col] += errorGb;
                        errorNextRowB[width - col] += errorBb;

                        if (col != 0)
                        {
                            errorNextRowR[width - (col - 1)] += errorRc;
                            errorNextRowG[width - (col - 1)] += errorGc;
                            errorNextRowB[width - (col - 1)] += errorBc;
                        }

                        errorNextRowR[width - (col + 1)] += errorRd;
                        errorNextRowG[width - (col + 1)] += errorGd;
                        errorNextRowB[width - (col + 1)] += errorBd;

                        // unchecked is necessary because otherwise it throws a fit if ptrInc is negative.
                        unchecked
                        {
                            pSourcePixel += ptrInc;
                            pDestinationPixel += ptrInc;
                        }
                    }

                    // Add the stride to the source row
                    pSourceRow += sourceData.Stride;

                    // And to the destination row
                    pDestinationRow += outputData.Stride;

                    if (progressCallback != null)
                    {
                        progressCallback(this, new ProgressEventArgs(100.0 * (0.5 + ((double)(row + 1) / (double)height) / 2.0)));
                    }

                    errorThisRowB = errorNextRowB;
                    errorThisRowG = errorNextRowG;
                    errorThisRowR = errorNextRowR;
                }
            }
            
            finally
            {
                // Ensure that I unlock the output bits
                output.UnlockBits(outputData);
            }
        }

        /// <summary>
        /// Override this to process the pixel in the first pass of the algorithm
        /// </summary>
        /// <param name="pixel">The pixel to quantize</param>
        /// <remarks>
        /// This function need only be overridden if your quantize algorithm needs two passes,
        /// such as an Octree quantizer.
        /// </remarks>
        protected virtual void InitialQuantizePixel(ColorBgra *pixel)
        {
        }

        /// <summary>
        /// Override this to process the pixel in the second pass of the algorithm
        /// </summary>
        /// <param name="pixel">The pixel to quantize</param>
        /// <returns>The quantized value</returns>
        protected abstract byte QuantizePixel(ColorBgra *pixel);

        /// <summary>
        /// Retrieve the palette for the quantized image
        /// </summary>
        /// <param name="original">Any old palette, this is overrwritten</param>
        /// <returns>The new color palette</returns>
        protected abstract ColorPalette GetPalette(ColorPalette original);
    }
}
