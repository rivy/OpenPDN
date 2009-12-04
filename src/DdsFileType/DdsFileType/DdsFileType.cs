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

using PaintDotNet;
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

namespace DdsFileTypePlugin
{
	// --------------------------------------------------------

	// We need this to register our DdsFileType object
    public class DDSFileTypes : IFileTypeFactory
    {
		public	static readonly FileType	Dds			= new DdsFileType();
	    private	static			FileType[]	fileTypes	= new FileType[] { Dds };

		internal FileTypeCollection GetFileTypeCollection()
		{
			return new FileTypeCollection( fileTypes );
		}

		public FileType[] GetFileTypeInstances()
		{
            DdsSquish.Initialize();
			return (FileType[])fileTypes.Clone();
		}
    }

	// This is the core of the application..
	[Guid("77511FB1-CA18-4424-8957-4C5F86EB7CD0")]
    public class DdsFileType : FileType
    {

		public DdsFileType() 
            : base(PdnResources.GetString("DdsFileType.Name"),
                   FileTypeFlags.SupportsLoading | FileTypeFlags.SupportsSaving | FileTypeFlags.SavesWithProgress, 
                   new string[] { ".dds" } )
		{
		}

		public override SaveConfigWidget CreateSaveConfigWidget()
		{
			return new DdsSaveConfigWidget();
		}

		protected override SaveConfigToken OnCreateDefaultSaveConfigToken()
		{
			return new DdsSaveConfigToken( 0, 0, 0, false, false );
		}

        protected override unsafe void OnSave( Document input, Stream output, SaveConfigToken token, Surface scratchSurface, ProgressEventHandler callback )
		{
			DdsSaveConfigToken ddsToken = ( DdsSaveConfigToken )token;

			// We need to be able to feast on the goo inside..
			scratchSurface.Clear( ColorBgra.Transparent );

			using ( RenderArgs ra = new RenderArgs( scratchSurface ) )
			{
				input.Render( ra, true );
			}

			// Create the DDS file, and save it..
			DdsFile ddsFile = new DdsFile();
			ddsFile.Save( output, scratchSurface, ddsToken, callback );
		}

		protected override Document OnLoad( Stream input )
		{
			DdsFile	ddsFile	= new DdsFile();
			ddsFile.Load( input );

			BitmapLayer layer			= Layer.CreateBackgroundLayer( ddsFile.GetWidth(), ddsFile.GetHeight() );
			Surface		surface			= layer.Surface;
			ColorBgra	writeColour		= new ColorBgra();

			byte[]		readPixelData	= ddsFile.GetPixelData();

			for ( int y = 0; y < ddsFile.GetHeight(); y++ )
			{
				for ( int x = 0; x < ddsFile.GetWidth(); x++ )
				{
					int			readPixelOffset = ( y * ddsFile.GetWidth() * 4 ) + ( x * 4 );
					
					writeColour.R = readPixelData[ readPixelOffset + 0 ];
					writeColour.G = readPixelData[ readPixelOffset + 1 ];
					writeColour.B = readPixelData[ readPixelOffset + 2 ];
					writeColour.A = readPixelData[ readPixelOffset + 3 ];

					surface[ x, y ] = writeColour;
				}
			}

			// Create a document, add the surface layer to it, and return to caller.
			Document	document	= new Document( surface.Width, surface.Height );
			document.Layers.Add( layer );
			return document;
		}
	}
}
