/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.SystemLayer;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace PaintDotNet
{
    /// <summary>
    /// Implements FileType for generic GDI+ codecs.
    /// </summary>
    /// <remarks>
    /// GDI+ file types do not support custom headers.
    /// </remarks>
    public class GdiPlusFileType
        : FileType
    {
        private ImageFormat imageFormat; 
        public ImageFormat ImageFormat
        {
            get
            {
                return this.imageFormat;
            }
        }

        protected override void OnSave(Document input, Stream output, SaveConfigToken token, Surface scratchSurface, ProgressEventHandler callback)
        {
            GdiPlusFileType.Save(input, output, scratchSurface, this.ImageFormat, callback);
        }

        public static void Save(Document input, Stream output, Surface scratchSurface, ImageFormat format, ProgressEventHandler callback)
        {
            // flatten the document
            scratchSurface.Clear(ColorBgra.FromBgra(0, 0, 0, 0));

            using (RenderArgs ra = new RenderArgs(scratchSurface))
            {
                input.Render(ra, true);
            }

            using (Bitmap bitmap = scratchSurface.CreateAliasedBitmap())
            {
                LoadProperties(bitmap, input);
                bitmap.Save(output, format);
            }
        }

        public static void LoadProperties(Image dstImage, Document srcDoc)
        {
            Bitmap asBitmap = dstImage as Bitmap;

            if (asBitmap != null)
            {
                // Sometimes GDI+ does not honor the resolution tags that we
                // put in manually via the EXIF properties.
                float dpiX;
                float dpiY;

                switch (srcDoc.DpuUnit)
                {
                    case MeasurementUnit.Centimeter:
                        dpiX = (float)Document.DotsPerCmToDotsPerInch(srcDoc.DpuX);
                        dpiY = (float)Document.DotsPerCmToDotsPerInch(srcDoc.DpuY);
                        break;

                    case MeasurementUnit.Inch:
                        dpiX = (float)srcDoc.DpuX;
                        dpiY = (float)srcDoc.DpuY;
                        break;

                    default:
                    case MeasurementUnit.Pixel:
                        dpiX = 1.0f;
                        dpiY = 1.0f;
                        break;
                }

                try
                {
                    asBitmap.SetResolution(dpiX, dpiY);
                }

                catch (Exception)
                {
                    // Ignore error
                }
            }

            Metadata metaData = srcDoc.Metadata;

            foreach (string key in metaData.GetKeys(Metadata.ExifSectionName))
            {
                string blob = metaData.GetValue(Metadata.ExifSectionName, key);
                PropertyItem pi = PdnGraphics.DeserializePropertyItem(blob);

                try
                {
                    dstImage.SetPropertyItem(pi);
                }

                catch (ArgumentException)
                {
                    // Ignore error: the image does not support property items
                }
            }
        }

        protected override Document OnLoad(Stream input)
        {
            using (Image image = PdnResources.LoadImage(input))
            {
                Document document = Document.FromImage(image);
                return document;
            }
        }
        
        public static ImageCodecInfo GetImageCodecInfo(ImageFormat format)
        {
            ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();

            foreach (ImageCodecInfo icf in encoders)
            {
                if (icf.FormatID == format.Guid)
                {
                    return icf;
                }
            }

            return null;
        }

        public GdiPlusFileType(string name, ImageFormat imageFormat, bool supportsLayers, string[] extensions)
            : this(name, imageFormat, supportsLayers, extensions, false)
        {
        }

        public GdiPlusFileType(string name, ImageFormat imageFormat, bool supportsLayers, string[] extensions, bool savesWithProgress)
            : base(name, 
                   (supportsLayers ? FileTypeFlags.SupportsLayers : 0) | 
                       FileTypeFlags.SupportsLoading | 
                       FileTypeFlags.SupportsSaving | 
                       (savesWithProgress ? FileTypeFlags.SavesWithProgress : 0),
                   extensions)
        {
            this.imageFormat = imageFormat;
        }
    }
}
