/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;

namespace PaintDotNet
{
    public enum ExifTagID
        : short
    {
        // Tags relating to image data structure
        ImageWidth = 256,
        ImageLength = 257,
        BitsPerSample = 258,
        Compression = 259,
        PhotometricInterpretation = 262,
        Orientation = 274,
        SamplesPerPixel = 277,
        PlanarConfiguration = 284,
        YCbCrSubSampling = 530,
        YCbCrPositioning = 531,
        XResolution = 282,
        YResolution = 283,
        ResolutionUnit = 296,

        // Tags relating to recording offset
        StripOffsets = 273,
        RowsPerStrip = 278,
        StripByteCounts = 279,
        JPEGInterchangeFormat = 513,
        JPEGInterchangeFormatLength = 514,

        // Tags relating to image data characteristics
        TransferFunction = 301,
        WhitePoint = 318,
        PrimaryChromaticities = 319,
        YCbCrCoefficients = 529,
        ReferenceBlackWhite = 532,

        // Other tags
        DateTime = 306,
        ImageDescription = 270,
        Make = 271,
        Model = 272,
        Software = 305,
        Artist = 315,
        Copyright = unchecked((short)33432)
    }
}
