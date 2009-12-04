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
    public sealed class PngFileType
        : InternalFileType
    {
        protected override bool IsReflexive(PropertyBasedSaveConfigToken token)
        {
            PngBitDepthUIChoices bitDepth = (PngBitDepthUIChoices)token.GetProperty<StaticListChoiceProperty>(PropertyNames.BitDepth).Value;

            // Only 32-bit is reflexive
            return (bitDepth == PngBitDepthUIChoices.Bpp32);
        }

        public PngFileType()
            : base("PNG", FileTypeFlags.SupportsLoading | FileTypeFlags.SupportsSaving, new string[] { ".png" })
        {
        }

        public enum PropertyNames
        {
            BitDepth = 0,
            DitherLevel = 1,
            Threshold = 2
        }

        public enum PngBitDepthUIChoices
        {
            AutoDetect = 0,
            Bpp32 = 1,
            Bpp24 = 2,
            Bpp8 = 3
        }

        public override ControlInfo OnCreateSaveConfigUI(PropertyCollection props)
        {
            ControlInfo configUI = CreateDefaultSaveConfigUI(props);

            configUI.SetPropertyControlValue(
                PropertyNames.BitDepth,
                ControlInfoPropertyNames.DisplayName,
                PdnResources.GetString("PngFileType.ConfigUI.BitDepth.DisplayName"));

            PropertyControlInfo bitDepthPCI = configUI.FindControlForPropertyName(PropertyNames.BitDepth);
            bitDepthPCI.SetValueDisplayName(PngBitDepthUIChoices.AutoDetect, PdnResources.GetString("PngFileType.ConfigUI.BitDepth.AutoDetect.DisplayName"));
            bitDepthPCI.SetValueDisplayName(PngBitDepthUIChoices.Bpp32, PdnResources.GetString("PngFileType.ConfigUI.BitDepth.Bpp32.DisplayName"));
            bitDepthPCI.SetValueDisplayName(PngBitDepthUIChoices.Bpp24, PdnResources.GetString("PngFileType.ConfigUI.BitDepth.Bpp24.DisplayName"));
            bitDepthPCI.SetValueDisplayName(PngBitDepthUIChoices.Bpp8, PdnResources.GetString("PngFileType.ConfigUI.BitDepth.Bpp8.DisplayName"));

            configUI.SetPropertyControlType(PropertyNames.BitDepth, PropertyControlType.RadioButton);

            configUI.SetPropertyControlValue(
                PropertyNames.DitherLevel,
                ControlInfoPropertyNames.DisplayName,
                PdnResources.GetString("PngFileType.ConfigUI.DitherLevel.DisplayName"));

            configUI.SetPropertyControlValue(
                PropertyNames.Threshold,
                ControlInfoPropertyNames.DisplayName,
                PdnResources.GetString("PngFileType.ConfigUI.Threshold.DisplayName"));

            configUI.SetPropertyControlValue(
                PropertyNames.Threshold,
                ControlInfoPropertyNames.Description,
                PdnResources.GetString("PngFileType.ConfigUI.Threshold.Description"));

            return configUI;
        }

        public override PropertyCollection OnCreateSavePropertyCollection()
        {
            List<Property> props = new List<Property>();

            props.Add(StaticListChoiceProperty.CreateForEnum<PngBitDepthUIChoices>(PropertyNames.BitDepth, PngBitDepthUIChoices.AutoDetect, false));
            props.Add(new Int32Property(PropertyNames.DitherLevel, 7, 0, 8));
            props.Add(new Int32Property(PropertyNames.Threshold, 128, 0, 255));

            List<PropertyCollectionRule> rules = new List<PropertyCollectionRule>();

            rules.Add(new ReadOnlyBoundToValueRule<object, StaticListChoiceProperty>(
                PropertyNames.Threshold, 
                PropertyNames.BitDepth, 
                PngBitDepthUIChoices.Bpp8, 
                true));

            rules.Add(new ReadOnlyBoundToValueRule<object, StaticListChoiceProperty>(
                PropertyNames.DitherLevel, 
                PropertyNames.BitDepth, 
                PngBitDepthUIChoices.Bpp8, 
                true));

            PropertyCollection pc = new PropertyCollection(props, rules);

            return pc;
        }

        protected override Document OnLoad(Stream input)
        {
            using (Image image = PdnResources.LoadImage(input))
            {
                Document document = Document.FromImage(image);
                return document;
            }
        }

        internal override Set<SavableBitDepths> CreateAllowedBitDepthListFromToken(PropertyBasedSaveConfigToken token)
        {
            PngBitDepthUIChoices bitDepthFromToken = (PngBitDepthUIChoices)token.GetProperty<StaticListChoiceProperty>(PropertyNames.BitDepth).Value;

            Set<SavableBitDepths> bitDepths = new Set<SavableBitDepths>();

            switch (bitDepthFromToken)
            {
                case PngBitDepthUIChoices.AutoDetect:
                    bitDepths.AddRange(SavableBitDepths.Rgb24, SavableBitDepths.Rgb8, SavableBitDepths.Rgba32, SavableBitDepths.Rgba8);
                    break;

                case PngBitDepthUIChoices.Bpp24:
                    bitDepths.AddRange(SavableBitDepths.Rgb24);
                    break;

                case PngBitDepthUIChoices.Bpp32:
                    bitDepths.AddRange(SavableBitDepths.Rgba32);
                    break;

                case PngBitDepthUIChoices.Bpp8:
                    bitDepths.AddRange(SavableBitDepths.Rgb8, SavableBitDepths.Rgba8);
                    break;

                default:
                    throw new InvalidEnumArgumentException("bitDepthFromToken", (int)bitDepthFromToken, typeof(PngBitDepthUIChoices));
            }

            return bitDepths;
        }

        internal override int GetThresholdFromToken(PropertyBasedSaveConfigToken token)
        {
            int threshold = token.GetProperty<Int32Property>(PropertyNames.Threshold).Value;
            return threshold;
        }

        internal override int GetDitherLevelFromToken(PropertyBasedSaveConfigToken token)
        {
            int ditherLevel = token.GetProperty<Int32Property>(PropertyNames.DitherLevel).Value;
            return ditherLevel;
        }
        
        internal override unsafe void FinalSave(
            Document input, 
            Stream output, 
            Surface scratchSurface, 
            int ditherLevel, 
            SavableBitDepths bitDepth, 
            PropertyBasedSaveConfigToken token,
            ProgressEventHandler progressCallback)
        {
            if (bitDepth == SavableBitDepths.Rgba32)
            {
                ImageCodecInfo icf = GdiPlusFileType.GetImageCodecInfo(ImageFormat.Png);
                EncoderParameters parms = new EncoderParameters(1);
                EncoderParameter parm = new EncoderParameter(System.Drawing.Imaging.Encoder.ColorDepth, 32);
                parms.Param[0] = parm;

                using (Bitmap bitmap = scratchSurface.CreateAliasedBitmap())
                {
                    GdiPlusFileType.LoadProperties(bitmap, input);
                    bitmap.Save(output, icf, parms);
                }
            }
            else if (bitDepth == SavableBitDepths.Rgb24)
            {
                // In order to save memory, we 'squish' the 32-bit bitmap down to 24-bit in-place
                // instead of allocating a new bitmap and copying it over.
                SquishSurfaceTo24Bpp(scratchSurface);

                ImageCodecInfo icf = GdiPlusFileType.GetImageCodecInfo(ImageFormat.Png);
                EncoderParameters parms = new EncoderParameters(1);
                EncoderParameter parm = new EncoderParameter(System.Drawing.Imaging.Encoder.ColorDepth, 24);
                parms.Param[0] = parm;

                using (Bitmap bitmap = CreateAliased24BppBitmap(scratchSurface))
                {
                    GdiPlusFileType.LoadProperties(bitmap, input);
                    bitmap.Save(output, icf, parms);
                }
            }
            else if (bitDepth == SavableBitDepths.Rgb8)
            {
                using (Bitmap quantized = Quantize(scratchSurface, ditherLevel, 256, false, progressCallback))
                {
                    ImageCodecInfo icf = GdiPlusFileType.GetImageCodecInfo(ImageFormat.Png);
                    EncoderParameters parms = new EncoderParameters(1);
                    EncoderParameter parm = new EncoderParameter(System.Drawing.Imaging.Encoder.ColorDepth, 8);
                    parms.Param[0] = parm;

                    GdiPlusFileType.LoadProperties(quantized, input);
                    quantized.Save(output, icf, parms);
                }
            }
            else if (bitDepth == SavableBitDepths.Rgba8)
            {
                using (Bitmap quantized = Quantize(scratchSurface, ditherLevel, 256, true, progressCallback))
                {
                    ImageCodecInfo icf = GdiPlusFileType.GetImageCodecInfo(ImageFormat.Png);
                    EncoderParameters parms = new EncoderParameters(1);
                    EncoderParameter parm = new EncoderParameter(System.Drawing.Imaging.Encoder.ColorDepth, 8);
                    parms.Param[0] = parm;

                    GdiPlusFileType.LoadProperties(quantized, input);
                    quantized.Save(output, icf, parms);
                }
            }
            else
            {
                throw new InvalidEnumArgumentException("bitDepth", (int)bitDepth, typeof(SavableBitDepths));
            }
        }
    }
}
