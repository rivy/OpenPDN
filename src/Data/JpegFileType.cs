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
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace PaintDotNet
{
    public sealed class JpegFileType
        : PropertyBasedFileType
    {
        public JpegFileType()
            : base("JPEG", FileTypeFlags.SupportsLoading | FileTypeFlags.SupportsSaving, new string[] { ".jpg", ".jpeg", ".jpe", ".jfif" })
        {
        }

        public enum PropertyNames
        {
            Quality = 0
        }

        public override PropertyCollection OnCreateSavePropertyCollection()
        {
            List<Property> props = new List<Property>();

            props.Add(new Int32Property(PropertyNames.Quality, 95, 0, 100));

            return new PropertyCollection(props);
        }

        public override ControlInfo OnCreateSaveConfigUI(PropertyCollection props)
        {
            ControlInfo configUI = CreateDefaultSaveConfigUI(props);

            configUI.SetPropertyControlValue(
                PropertyNames.Quality,
                ControlInfoPropertyNames.DisplayName,
                PdnResources.GetString("JpegFileType.ConfigUI.Quality.DisplayName") ?? "??");

            return configUI;
        }

        protected override void OnSaveT(Document input, Stream output, PropertyBasedSaveConfigToken token, Surface scratchSurface, ProgressEventHandler progressCallback)
        {
            int quality = token.GetProperty<Int32Property>(PropertyNames.Quality).Value;

            ImageCodecInfo icf = GdiPlusFileType.GetImageCodecInfo(ImageFormat.Jpeg);
            EncoderParameters parms = new EncoderParameters(1);
            EncoderParameter parm = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
            parms.Param[0] = parm;

            scratchSurface.Clear(ColorBgra.White);

            using (RenderArgs ra = new RenderArgs(scratchSurface))
            {
                input.Render(ra, false);
            }

            using (Bitmap bitmap = scratchSurface.CreateAliasedBitmap())
            {
                GdiPlusFileType.LoadProperties(bitmap, input);
                bitmap.Save(output, icf, parms);
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
    }
}