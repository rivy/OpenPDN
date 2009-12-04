//------------------------------------------------------------------------------
/*
	@brief		DDS File Type Plugin for Paint.NET

	@note		Copyright (c) 2007 Dean Ashton         http://www.dmashton.co.uk

	Permission is hereby granted, free of charge, to any person obtaining
	a copy of this software and associated documentation files (the 
	"Software"), to	deal in the Software without restriction, including
	without limitation the rights to use, copy, modify, merge, publish,
	distribute, sublicense, and/or sell copies of the Software, and to 
	permit persons to whom the Software is furnished to do so, subject to 
	the following conditions:

	The above copyright notice and this permission notice shall be included
	in all copies or substantial portions of the Software.

	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
	OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
	MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
	IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY 
	CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, 
	TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
	SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
**/
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;
using PaintDotNet;
using PaintDotNet.SystemLayer;

namespace DdsFileTypePlugin
{
	internal sealed class DdsSquish
	{
		public enum SquishFlags
		{
			kDxt1						= ( 1 << 0 ),		// Use DXT1 compression.
			kDxt3						= ( 1 << 1 ),		// Use DXT3 compression.
			kDxt5						= ( 1 << 2 ), 		// Use DXT5 compression.
		
			kColourClusterFit			= ( 1 << 3 ),		// Use a slow but high quality colour compressor (the default).
			kColourRangeFit				= ( 1 << 4 ),		// Use a fast but low quality colour compressor.

			kColourMetricPerceptual		= ( 1 << 5 ),		// Use a perceptual metric for colour error (the default).
			kColourMetricUniform		= ( 1 << 6 ),		// Use a uniform metric for colour error.
	
			kWeightColourByAlpha		= ( 1 << 7 ),		// Weight the colour by alpha during cluster fit (disabled by default).

			kColourIterativeClusterFit	= ( 1 << 8 ),		// Use a very slow but very high quality colour compressor.
		}

		private	static bool	Is64Bit()
		{
			return ( Marshal.SizeOf( IntPtr.Zero ) == 8 ); 
		}

        internal delegate void ProgressFn(int workDone, int workTotal);

        private sealed class SquishInterface_32
        {
            [DllImport("Squish_x86.dll")]
            internal static extern unsafe void SquishCompressImage(byte* rgba, int width, int height, byte* blocks, int flags,
                [MarshalAs(UnmanagedType.FunctionPtr)] ProgressFn progressFn);

            [DllImport("Squish_x86.dll")]
            internal static extern unsafe void SquishDecompressImage(byte* rgba, int width, int height, byte* blocks, int flags,
                [MarshalAs(UnmanagedType.FunctionPtr)] ProgressFn progressFn);

            [DllImport("Squish_x86.dll")]
            internal static extern void SquishInitialize();
        }

        private sealed class SquishInterface_32_SSE2
        {
            [DllImport("Squish_x86_SSE2.dll")]
            internal static extern unsafe void SquishCompressImage(byte* rgba, int width, int height, byte* blocks, int flags,
                [MarshalAs(UnmanagedType.FunctionPtr)] ProgressFn progressFn);

            [DllImport("Squish_x86_SSE2.dll")]
            internal static extern unsafe void SquishDecompressImage(byte* rgba, int width, int height, byte* blocks, int flags,
                [MarshalAs(UnmanagedType.FunctionPtr)] ProgressFn progressFn);

            [DllImport("Squish_x86_SSE2.dll")]
            internal static extern void SquishInitialize();
        }

        private sealed class SquishInterface_64
		{
			[DllImport("Squish_x64.dll")]
			internal static extern unsafe void SquishCompressImage( byte* rgba, int width, int height, byte* blocks, int flags,
                [MarshalAs(UnmanagedType.FunctionPtr)] ProgressFn progressFn);

            [DllImport("Squish_x64.dll")]
			internal static	extern unsafe void SquishDecompressImage( byte* rgba, int width, int height, byte* blocks, int flags,
                [MarshalAs(UnmanagedType.FunctionPtr)] ProgressFn progressFn);

            [DllImport("Squish_x64.dll")]
            internal static extern void SquishInitialize();
        }

		private static unsafe void	CallCompressImage( byte[] rgba, int width, int height, byte[] blocks, int flags, ProgressFn progressFn )
		{
			fixed ( byte* pRGBA = rgba )
			{
				fixed ( byte* pBlocks = blocks )
				{
					if ( Processor.Architecture == ProcessorArchitecture.X64 )
						SquishInterface_64.SquishCompressImage( pRGBA, width, height, pBlocks, flags, progressFn );
                    else if ( Processor.IsFeaturePresent(ProcessorFeature.SSE2) )
                        SquishInterface_32_SSE2.SquishCompressImage(pRGBA, width, height, pBlocks, flags, progressFn);
                    else
						SquishInterface_32.SquishCompressImage( pRGBA, width, height, pBlocks, flags, progressFn );
				}
			}

            GC.KeepAlive(progressFn);
		}
		
		private static unsafe void	CallDecompressImage( byte[] rgba, int width, int height, byte[] blocks, int flags, ProgressFn progressFn )
		{
			fixed ( byte* pRGBA = rgba )
			{
				fixed ( byte* pBlocks = blocks )
				{
                    if ( Processor.Architecture == ProcessorArchitecture.X64 )
                        SquishInterface_64.SquishDecompressImage(pRGBA, width, height, pBlocks, flags, progressFn);
					else if ( Processor.IsFeaturePresent(ProcessorFeature.SSE2) )
						SquishInterface_32_SSE2.SquishDecompressImage( pRGBA, width, height, pBlocks, flags, progressFn );
                    else
						SquishInterface_32.SquishDecompressImage( pRGBA, width, height, pBlocks, flags, progressFn );
				}
			}

            GC.KeepAlive(progressFn);
		}

        public static void Initialize()
        {
            if (Processor.Architecture == ProcessorArchitecture.X64)
            {
                SquishInterface_64.SquishInitialize();
            }
            else if (Processor.IsFeaturePresent(ProcessorFeature.SSE2))
            {
                SquishInterface_32_SSE2.SquishInitialize();
            }
            else
            {
                SquishInterface_32.SquishInitialize();
            }
        }

		// ---------------------------------------------------------------------------------------
		//	CompressImage
		// ---------------------------------------------------------------------------------------
		//
		//	Params
		//		inputSurface	:	Source byte array containing RGBA pixel data
		//		flags			:	Flags for squish compression control
		//
		//	Return	
		//		blockData		:	Array of bytes containing compressed blocks
		//
		// ---------------------------------------------------------------------------------------

		internal static byte[] CompressImage( Surface inputSurface, int squishFlags, ProgressFn progressFn )
		{
			// We need the input to be in a byte array for squish.. so create one.
			byte[]	pixelData	= new byte[ inputSurface.Width * inputSurface.Height * 4 ];

			for ( int y = 0; y < inputSurface.Height; y++ )
			{
				for ( int x = 0; x < inputSurface.Width; x++ )
				{
					ColorBgra	pixelColour = inputSurface.GetPoint( x, y );
					int			pixelOffset	= ( y * inputSurface.Width * 4 ) + ( x * 4 );
						
					pixelData[ pixelOffset + 0 ]	= pixelColour.R;
					pixelData[ pixelOffset + 1 ]	= pixelColour.G;
					pixelData[ pixelOffset + 2 ]	= pixelColour.B;
					pixelData[ pixelOffset + 3 ]	= pixelColour.A;
				}
			}

			// Compute size of compressed block area, and allocate 
			int blockCount = ( ( inputSurface.Width + 3 )/4 ) * ( ( inputSurface.Height + 3 )/4 );
			int blockSize = ( ( squishFlags & ( int )DdsSquish.SquishFlags.kDxt1 ) != 0 ) ? 8 : 16;

			// Allocate room for compressed blocks
			byte[]	blockData		= new byte[ blockCount * blockSize ];
	
			// Invoke squish::CompressImage() with the required parameters
			CallCompressImage( pixelData, inputSurface.Width, inputSurface.Height, blockData, squishFlags, progressFn );
				
			// Return our block data to caller..
			return	blockData;	
		}

		// ---------------------------------------------------------------------------------------
		//	DecompressImage
		// ---------------------------------------------------------------------------------------
		//
		//	Params
		//		inputSurface	:	Source byte array containing DXT block data
		//		width			:	Width of image in pixels
		//		height			:	Height of image in pixels
		//		flags			:	Flags for squish decompression control
		//
		//	Return	
		//		byte[]			:	Array of bytes containing decompressed blocks
		//
		// ---------------------------------------------------------------------------------------

		internal static byte[] DecompressImage( byte[] blocks, int width, int height, int flags )
		{
			// Allocate room for decompressed output
			byte[]	pixelOutput	= new byte[ width * height * 4 ];

			// Invoke squish::DecompressImage() with the required parameters
			CallDecompressImage( pixelOutput, width, height, blocks, flags, null );

			// Return our pixel data to caller..
			return pixelOutput;
		}
	}
}
