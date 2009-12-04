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
using PaintDotNet;

namespace DdsFileTypePlugin
{
	[Serializable]
	public class DdsSaveConfigToken : SaveConfigToken
	{
		public override object Clone()
		{
			return new DdsSaveConfigToken( this );
		}

		public DdsSaveConfigToken( DdsFileFormat fileFormat, int compressorType, int errorMetric, bool weightColourByAlpha, bool generateMipMaps )
		{
			m_fileFormat			= fileFormat;
			m_compressorType		= compressorType;
			m_errorMetric			= errorMetric;
			m_weightColourByAlpha	= weightColourByAlpha;
			m_generateMipMaps		= generateMipMaps;
		}

		// Converts token information into a form ready for passing on to Squish.
		public	int	GetSquishFlags()
		{
			int	squishFlags = 0;

			// Translate file format
			if ( m_fileFormat == DdsFileFormat.DDS_FORMAT_DXT1 )
				squishFlags |= ( int )DdsSquish.SquishFlags.kDxt1;
			else
			if ( m_fileFormat == DdsFileFormat.DDS_FORMAT_DXT3 )
				squishFlags |= ( int )DdsSquish.SquishFlags.kDxt3;
			else
			if ( m_fileFormat == DdsFileFormat.DDS_FORMAT_DXT5 )
				squishFlags |= ( int )DdsSquish.SquishFlags.kDxt5;

			// If this isn't a DXT file, then no flags
			if ( squishFlags == 0 )
				return squishFlags;

			// Translate compressor type
			if ( m_compressorType == 0 )
				squishFlags |= ( int )DdsSquish.SquishFlags.kColourClusterFit;
			else
			if ( m_compressorType == 1 )
				squishFlags |= ( int )DdsSquish.SquishFlags.kColourRangeFit;
			else
				squishFlags |= ( int )DdsSquish.SquishFlags.kColourIterativeClusterFit;	

			// Translate error metric
			if ( m_errorMetric == 0 )
				squishFlags	|= ( int )DdsSquish.SquishFlags.kColourMetricPerceptual;
			else
				squishFlags	|= ( int )DdsSquish.SquishFlags.kColourMetricUniform;

			// Now the colour weighting state (only valid for cluster fit)
			if ( ( m_compressorType == 0 )&& ( m_weightColourByAlpha ) )
				squishFlags |= ( int )DdsSquish.SquishFlags.kWeightColourByAlpha;

			return squishFlags;
		}

		protected DdsSaveConfigToken( DdsSaveConfigToken copyMe )
		{
			m_fileFormat			=	copyMe.m_fileFormat;
			m_compressorType		=	copyMe.m_compressorType;
			m_errorMetric			=	copyMe.m_errorMetric;
			m_weightColourByAlpha	=	copyMe.m_weightColourByAlpha;
			m_generateMipMaps		=	copyMe.m_generateMipMaps;
		}

		public override void Validate()
		{
			if ( ( m_compressorType != 0 ) && ( m_compressorType != 1 ) && ( m_compressorType != 2 ) )
			{
                throw new ArgumentOutOfRangeException( "Unrecognised compressor type (" + m_compressorType + ")" );
			}

			if ( ( m_errorMetric != 0 ) && ( m_errorMetric != 1 ) )
			{
                throw new ArgumentOutOfRangeException( "Unrecognised error metric type (" + m_errorMetric + ")" );
			}
		}

		public	DdsFileFormat	m_fileFormat;
		public	int				m_compressorType;
		public	int				m_errorMetric;
		public	bool			m_weightColourByAlpha;
		public	bool			m_generateMipMaps;
	}
}
