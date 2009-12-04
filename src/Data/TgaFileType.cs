/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

/////////////////////////////////////////////////////////////////////////////////
// Some of this code is adapted from code in the CxImage library by Davide Pizzolato. 
// The following text is the original license.txt from that library:
// 
// COPYRIGHT NOTICE, DISCLAIMER, and LICENSE:
// 
// CxImage version 5.99c 17/Oct/2004
// 
// CxImage : Copyright (C) 2001 - 2004, Davide Pizzolato
// 
// Original CImage and CImageIterator implementation are:
// Copyright (C) 1995, Alejandro Aguilar Sierra (asierra(at)servidor(dot)unam(dot)mx)
// 
// Covered code is provided under this license on an "as is" basis, without warranty
// of any kind, either expressed or implied, including, without limitation, warranties
// that the covered code is free of defects, merchantable, fit for a particular purpose
// or non-infringing. The entire risk as to the quality and performance of the covered
// code is with you. Should any covered code prove defective in any respect, you (not
// the initial developer or any other contributor) assume the cost of any necessary
// servicing, repair or correction. This disclaimer of warranty constitutes an essential
// part of this license. No use of any covered code is authorized hereunder except under
// this disclaimer.
// 
// Permission is hereby granted to use, copy, modify, and distribute this
// source code, or portions hereof, for any purpose, including commercial applications,
// freely and without fee, subject to the following restrictions: 
// 
// 1. The origin of this software must not be misrepresented; you must not
// claim that you wrote the original software. If you use this software
// in a product, an acknowledgment in the product documentation would be
// appreciated but is not required.
// 
// 2. Altered source versions must be plainly marked as such, and must not be
// misrepresented as being the original software.
// 
// 3. This notice may not be removed or altered from any source distribution.
// 
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet;
using PaintDotNet.IndirectUI;
using PaintDotNet.PropertySystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace PaintDotNet.Data
{
    public sealed class TgaFileType
        : InternalFileType
    {
        public enum PropertyNames
        {
            BitDepth = 0,
            RleCompress = 1
        }

        public enum TgaBitDepthUIChoices
        {
            AutoDetect = 0,
            Bpp32 = 1,
            Bpp24 = 2
        }

        protected override bool IsReflexive(PropertyBasedSaveConfigToken token)
        {
            if ((TgaBitDepthUIChoices)token.GetProperty<StaticListChoiceProperty>(PropertyNames.BitDepth).Value == TgaBitDepthUIChoices.Bpp32)
            {
                return true;
            }
            else
            {
                return base.IsReflexive(token);
            }
        }

        private enum TgaType 
            : byte
        {
            Null = 0,
            Map = 1,
            Rgb = 2,
            Mono = 3,
            RleMap = 9,
            RleRgb = 10,
            RleMono = 11,
            CompMap = 32,
            CompMap4 = 33
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TgaHeader
        {
            public byte idLength;            // Image ID Field Length
            public byte cmapType;            // Color Map Type
            public TgaType imageType;        // Image Type

            public ushort cmapIndex;         // First Entry Index
            public ushort cmapLength;        // Color Map Length
            public byte cmapEntrySize;       // Color Map Entry Size

            public ushort xOrigin;           // X-origin of Image
            public ushort yOrigin;           // Y-origin of Image
            public ushort imageWidth;        // Image Width
            public ushort imageHeight;       // Image Height
            public byte pixelDepth;          // Pixel Depth
            public byte imageDesc;           // Image Descriptor

            public void Write(Stream output)
            {
                output.WriteByte(this.idLength);
                output.WriteByte(this.cmapType);
                output.WriteByte((byte)this.imageType);

                Utility.WriteUInt16(output, this.cmapIndex);
                Utility.WriteUInt16(output, this.cmapLength);
                output.WriteByte(this.cmapEntrySize);

                Utility.WriteUInt16(output, this.xOrigin);
                Utility.WriteUInt16(output, this.yOrigin);
                Utility.WriteUInt16(output, this.imageWidth);
                Utility.WriteUInt16(output, this.imageHeight);
                output.WriteByte(this.pixelDepth);
                output.WriteByte(this.imageDesc);
            }

            public TgaHeader(Stream input)
            {
                int byteRead = input.ReadByte();
                if (byteRead == -1)
                {
                    throw new EndOfStreamException();
                }
                else
                {
                    this.idLength = (byte)byteRead;
                }

                byteRead = input.ReadByte();
                if (byteRead == -1)
                {
                    throw new EndOfStreamException();
                }
                else
                {
                    this.cmapType = (byte)byteRead;
                }

                byteRead = input.ReadByte();
                if (byteRead == -1)
                {
                    throw new EndOfStreamException();
                }
                else
                {
                    this.imageType = (TgaType)byteRead;
                }

                int shortRead = Utility.ReadUInt16(input);
                if (shortRead == -1)
                {
                    throw new EndOfStreamException();
                }
                else
                {
                    this.cmapIndex = (ushort)shortRead;
                }

                shortRead = Utility.ReadUInt16(input);
                if (shortRead == -1)
                {
                    throw new EndOfStreamException();
                }
                else
                {
                    this.cmapLength = (ushort)shortRead;
                }

                byteRead = input.ReadByte();
                if (byteRead == -1)
                {
                    throw new EndOfStreamException();
                }
                else
                {
                    this.cmapEntrySize = (byte)byteRead;
                }

                shortRead = Utility.ReadUInt16(input);
                if (shortRead == -1)
                {
                    throw new EndOfStreamException();
                }
                else
                {
                    this.xOrigin = (ushort)shortRead;
                }

                shortRead = Utility.ReadUInt16(input);
                if (shortRead == -1)
                {
                    throw new EndOfStreamException();
                }
                else
                {
                    this.yOrigin = (ushort)shortRead;
                }

                shortRead = Utility.ReadUInt16(input);
                if (shortRead == -1)
                {
                    throw new EndOfStreamException();
                }
                else
                {
                    this.imageWidth = (ushort)shortRead;
                }

                shortRead = Utility.ReadUInt16(input);
                if (shortRead == -1)
                {
                    throw new EndOfStreamException();
                }
                else
                {
                    this.imageHeight = (ushort)shortRead;
                }

                byteRead = input.ReadByte();
                if (byteRead == -1)
                {
                    throw new EndOfStreamException();
                }
                else
                {
                    this.pixelDepth = (byte)byteRead;
                }

                byteRead = input.ReadByte();
                if (byteRead == -1)
                {
                    throw new EndOfStreamException();
                }
                else
                {
                    this.imageDesc = (byte)byteRead;
                }                
            }
        }

        internal override Set<SavableBitDepths> CreateAllowedBitDepthListFromToken(PropertyBasedSaveConfigToken token)
        {
            TgaBitDepthUIChoices bitDepth = (TgaBitDepthUIChoices)token.GetProperty<StaticListChoiceProperty>(PropertyNames.BitDepth).Value;

            Set<SavableBitDepths> bitDepths = new Set<SavableBitDepths>();

            switch (bitDepth)
            {
                case TgaBitDepthUIChoices.AutoDetect:
                    bitDepths.Add(SavableBitDepths.Rgb24);
                    bitDepths.Add(SavableBitDepths.Rgba32);
                    break;

                case TgaBitDepthUIChoices.Bpp24:
                    bitDepths.Add(SavableBitDepths.Rgb24);
                    break;

                case TgaBitDepthUIChoices.Bpp32:
                    bitDepths.Add(SavableBitDepths.Rgba32);
                    break;

                default:
                    throw new InvalidEnumArgumentException("bitDepth", (int)bitDepth, typeof(TgaBitDepthUIChoices));
            }

            return bitDepths;
        }

        internal override int GetDitherLevelFromToken(PropertyBasedSaveConfigToken token)
        {
            return 0;
        }

        internal override int GetThresholdFromToken(PropertyBasedSaveConfigToken token)
        {
            return 0;
        }

        public override PropertyCollection OnCreateSavePropertyCollection()
        {
            List<Property> props = new List<Property>();

            props.Add(StaticListChoiceProperty.CreateForEnum<TgaBitDepthUIChoices>(PropertyNames.BitDepth, TgaBitDepthUIChoices.AutoDetect, false));
            props.Add(new BooleanProperty(PropertyNames.RleCompress, true));

            return new PropertyCollection(props);
        }

        public override ControlInfo OnCreateSaveConfigUI(PropertyCollection props)
        {
            ControlInfo configUI = CreateDefaultSaveConfigUI(props);

            configUI.SetPropertyControlValue(
                PropertyNames.BitDepth,
                ControlInfoPropertyNames.DisplayName,
                PdnResources.GetString("TgaFileType.ConfigUI.BitDepth.DisplayName"));

            configUI.SetPropertyControlType(PropertyNames.BitDepth, PropertyControlType.RadioButton);

            PropertyControlInfo bitDepthPCI = configUI.FindControlForPropertyName(PropertyNames.BitDepth);
            bitDepthPCI.SetValueDisplayName(TgaBitDepthUIChoices.AutoDetect, PdnResources.GetString("TgaFileType.ConfigUI.BitDepth.AutoDetect.DisplayName"));
            bitDepthPCI.SetValueDisplayName(TgaBitDepthUIChoices.Bpp24, PdnResources.GetString("TgaFileType.ConfigUI.BitDepth.Bpp24.DisplayName"));
            bitDepthPCI.SetValueDisplayName(TgaBitDepthUIChoices.Bpp32, PdnResources.GetString("TgaFileType.ConfigUI.BitDepth.Bpp32.DisplayName"));

            configUI.SetPropertyControlValue(
                PropertyNames.RleCompress,
                ControlInfoPropertyNames.DisplayName,
                string.Empty);

            configUI.SetPropertyControlValue(
                PropertyNames.RleCompress,
                ControlInfoPropertyNames.Description,
                PdnResources.GetString("TgaFileType.ConfigUI.RleCompress.Description"));

            return configUI;
        }

        protected override Document OnLoad(System.IO.Stream input)
        {
            TgaHeader header = new TgaHeader(input);
            bool compressed;

            switch (header.imageType)
            {
                case TgaType.Map:
                case TgaType.Rgb:
                case TgaType.Mono:
                    compressed = false;
                    break;
                    
                case TgaType.RleMap:
                case TgaType.RleRgb:
                case TgaType.RleMono:
                    compressed = true;
                    break;

                default:
                    throw new FormatException("unknown TGA image type");
            }

            if (header.imageWidth == 0 || 
                header.imageHeight == 0 || 
                header.pixelDepth == 0 || 
                header.cmapLength > 256)
            {
                throw new FormatException("bad TGA header");
            }

            if (header.pixelDepth != 8 && 
                header.pixelDepth != 15 && 
                header.pixelDepth != 16 && 
                header.pixelDepth != 24 && 
                header.pixelDepth != 32)
            {
                throw new FormatException("bad TGA header: pixelDepth not one of {8, 15, 16, 24, 32}");
            }

            if (header.idLength > 0)
            {
                input.Position += header.idLength; // skip descriptor
            }

            BitmapLayer layer = Layer.CreateBackgroundLayer(header.imageWidth, header.imageHeight);

            try
            {
                Surface surface = layer.Surface;
                surface.Clear((ColorBgra)0xffffffff);

                ColorBgra[] palette = null;
                if (header.cmapType != 0)
                {
                    palette = LoadPalette(input, header.cmapLength);
                }

                if (header.imageType == TgaType.Mono ||
                    header.imageType == TgaType.RleMono)
                {
                    palette = CreateGrayPalette();
                }

                // Bits 0 - 3 of the image descriptor byte describe number of bits used for alpha channel
                // For loading, we won't worry about this. Not all TGA implementations are correct (such
                // as older Paint.NET TGA implementations!) and we don't want to lose all their alpha bits.
                //int alphaBits = header.imageDesc & 0xf;

                // Bits 4 & 5 of the image descriptor byte control the ordering of the pixels
                bool xReversed = ((header.imageDesc & 16) == 16);
                bool yReversed = ((header.imageDesc & 32) == 32);            

                byte rleLeftOver = 255; // for images with illegal packet boundary

                for (int y = 0; y < header.imageHeight; ++y)
                {
                    MemoryBlock dstRow;

                    if (yReversed)
                    {
                        dstRow = surface.GetRow(y);
                    }
                    else
                    {
                        dstRow = surface.GetRow(header.imageHeight - y - 1);
                    }

                    if (compressed)
                    {
                        rleLeftOver = ExpandCompressedLine(dstRow, 0, ref header, input, header.imageWidth, y, rleLeftOver, palette);
                    }
                    else
                    {
                        ExpandUncompressedLine(dstRow, 0, ref header, input, header.imageWidth, y, 0, palette);
                    }
                }

                if (xReversed)
                {
                    MirrorX(surface);
                }

                Document document = new Document(surface.Width, surface.Height);
                document.Layers.Add(layer);
                return document;
            }

            catch
            {
                if (layer != null)
                {
                    layer.Dispose();
                    layer = null;
                }

                throw;
            }
        }

        private void MirrorX(Surface surface)
        {
            for (int y = 0; y < surface.Height; ++y)
            {
                for (int x = 0; x < surface.Width / 2; ++x)
                {
                    ColorBgra rightSide = surface[surface.Width - x - 1, y];
                    surface[surface.Width - x - 1, y] = surface[x, y];
                    surface[x, y] = rightSide;
                }
            }
        }

        private ColorBgra[] CreateGrayPalette()
        {
            ColorBgra[] palette = new ColorBgra[256];

            for (int i = 0; i < palette.Length; ++i)
            {
                palette[i] = ColorBgra.FromBgra((byte)i, (byte)i, (byte)i, 255);
            }

            return palette;
        }

        private ColorBgra[] LoadPalette(Stream input, int count)
        {
            ColorBgra[] palette = new ColorBgra[count];

            for (int i = 0; i < palette.Length; ++i)
            {
                int blue = input.ReadByte();
                if (blue == -1)
                {
                    throw new EndOfStreamException();
                }

                int green = input.ReadByte();
                if (green == -1)
                {
                    throw new EndOfStreamException();
                }

                int red = input.ReadByte();
                if (red == -1)
                {
                    throw new EndOfStreamException();
                }

                palette[i] = ColorBgra.FromBgra((byte)blue, (byte)green, (byte)red, 255);
            }

            return palette;
        }

        private byte ExpandCompressedLine(MemoryBlock dst, int dstIndex, ref TgaHeader header, Stream input, int width, int y, byte rleLeftOver, ColorBgra[] palette)
        {
            byte rle;
            long filePos = 0;

            int x = 0;
            while (x < width)
            {
                if (rleLeftOver != 255)
                {
                    rle = rleLeftOver;
                    rleLeftOver = 255;
                }
                else
                {
                    int byte1 = input.ReadByte();

                    if (byte1 == -1)
                    {
                        throw new EndOfStreamException();
                    }
                    else
                    {
                        rle = (byte)byte1;
                    }
                }

                if ((rle & 128) != 0)
                {
                    // RLE Encoded packet
                    rle -= 127; // calculate real repeat count

                    if ((x + rle) > width)
                    {
                        rleLeftOver = (byte)(128 + (rle - (width - x) - 1));
                        filePos = input.Position;
                        rle = (byte)(width - x);
                    }
                
                    ColorBgra color = ReadColor(input, header.pixelDepth, palette);

                    for (int ix = 0; ix < rle; ++ix)
                    {
                        int index = dstIndex + (ix * ColorBgra.SizeOf);

                        dst[index] = color[0];
                        dst[1 + index] = color[1];
                        dst[2 + index] = color[2];
                        dst[3 + index] = color[3];
                    }

                    if (rleLeftOver != 255)
                    {
                        input.Position = filePos;
                    }
                }
                else
                {
                    // Raw packet
                    rle += 1; // calculate real repeat count

                    if ((x + rle) > width)
                    {
                        rleLeftOver = (byte)(rle - (width - x) - 1);
                        rle = (byte)(width - x);
                    }

                    ExpandUncompressedLine(dst, dstIndex, ref header, input, rle, y, x, palette);
                }

                dstIndex += rle * ColorBgra.SizeOf;
                x += rle;
            }

            return rleLeftOver;
        }

        private void ExpandUncompressedLine(MemoryBlock dst, int dstIndex, ref TgaHeader header, Stream input, int width, int y, int xoffset, ColorBgra[] palette)
        {
            for (int i = 0; i < width; ++i)
            {
                ColorBgra color = ReadColor(input, header.pixelDepth, palette);
                dst[dstIndex] = color[0];
                dst[1 + dstIndex] = color[1];
                dst[2 + dstIndex] = color[2];
                dst[3 + dstIndex] = color[3];
                dstIndex += 4;
            }
        }

        private ColorBgra ReadColor(Stream input, int pixelDepth, ColorBgra[] palette)
        {
            ColorBgra color;

            switch (pixelDepth)
            {
                case 32:
                {
                    long colorInt = Utility.ReadUInt32(input);

                    if (colorInt == -1)
                    {
                        throw new EndOfStreamException();
                    }

                    color = ColorBgra.FromUInt32((uint)colorInt);
                    break;
                }

                case 24:
                {
                    int colorInt = Utility.ReadUInt24(input);

                    if (colorInt == -1)
                    {
                        throw new EndOfStreamException();
                    }

                    color = ColorBgra.FromUInt32((uint)colorInt);
                    color.A = 255;
                    break;
                }

                case 15:
                case 16:
                {
                    int colorWord = Utility.ReadUInt16(input);

                    if (colorWord == -1)
                    {
                        throw new EndOfStreamException();
                    }

                    color = ColorBgra.FromBgra(
                        (byte)((colorWord >> 7) & 0xf8),
                        (byte)((colorWord >> 2) & 0xf8),
                        (byte)((colorWord & 0x1f) * 8),
                        255);

                    break;
                }

                case 8:
                {
                    int colorByte = input.ReadByte();

                    if (colorByte == -1)
                    {
                        throw new EndOfStreamException();
                    }

                    if (colorByte >= palette.Length)
                    {
                        throw new FormatException("color index was outside the bounds of the palette");
                    }

                    color = palette[colorByte];                            
                    break;
                }

                default:
                    throw new FormatException("colorDepth was not one of {8, 15, 16, 24, 32}");
            }

            return color;
        }

        internal override void FinalSave(
            Document input, 
            Stream output, 
            Surface scratchSurface, 
            int ditherLevel, 
            SavableBitDepths bitDepth, 
            PropertyBasedSaveConfigToken token, 
            ProgressEventHandler progressCallback)
        {
            bool rleCompress = token.GetProperty<BooleanProperty>(PropertyNames.RleCompress).Value;
            SaveTga(scratchSurface, output, bitDepth, rleCompress, progressCallback);
        }

        private void SaveTga(Surface input, Stream output, SavableBitDepths bitDepth, bool rleCompress, ProgressEventHandler progressCallback)
        {
            TgaHeader header = new TgaHeader();

            header.idLength = 0;
            header.cmapType = 0;
            header.imageType = rleCompress ? TgaType.RleRgb : TgaType.Rgb;
            header.cmapIndex = 0;
            header.cmapLength = 0;
            header.cmapEntrySize = 0; // if bpp=8, set this to 24
            header.xOrigin = 0;
            header.yOrigin = 0;
            header.imageWidth = (ushort)input.Width;
            header.imageHeight = (ushort)input.Height;

            header.imageDesc = 0;

            switch (bitDepth)
            {
                case SavableBitDepths.Rgba32:
                    header.pixelDepth = 32;
                    header.imageDesc |= 8;
                    break;

                case SavableBitDepths.Rgb24:
                    header.pixelDepth = 24;
                    break;

                default:
                    throw new InvalidEnumArgumentException("bitDepth", (int)bitDepth, typeof(SavableBitDepths));
            }

            header.Write(output);

            // write palette if doing 8-bit
            // ... todo?

            for (int y = input.Height - 1; y >= 0; --y)
            {
                // non-rle output
                if (rleCompress)
                {
                    SaveTgaRowRle(output, input, ref header, y);
                }
                else
                {
                    SaveTgaRowRaw(output, input, ref header, y);
                }

                if (progressCallback != null)
                {
                    progressCallback(this, new ProgressEventArgs(100.0 * ((double)(input.Height - y) / (double)input.Height)));
                }
            }
        }

        private class TgaPacketStateMachine
        {
            private bool rlePacket = false;
            private ColorBgra[] packetColors = new ColorBgra[128];
            private int packetLength;
            private Stream output;
            private int bitDepth;

            public void Flush()
            {
                byte header = (byte)((rlePacket ? 128 : 0) + (byte)(packetLength - 1));
                output.WriteByte(header);

                int length = (rlePacket ? 1 : packetLength);
                for (int i = 0; i < length; ++i)
                {
                    WriteColor(this.output, packetColors[i], this.bitDepth);
                }

                packetLength = 0;
            }

            public void Push(ColorBgra color)
            {
                if (packetLength == 0)
                {
                    // Starting a fresh packet.
                    rlePacket = false;
                    packetColors[0] = color;
                    packetLength = 1;
                }
                else if (packetLength == 1)
                {
                    // 2nd byte of this packet... decide RLE or non-RLE.
                    rlePacket = (color == packetColors[0]);
                    packetColors[1] = color;
                    packetLength = 2;
                }
                else if (packetLength == packetColors.Length)
                {
                    // Packet is full. Start a new one.
                    Flush();
                    Push(color);
                }
                else if (packetLength >= 2 && rlePacket && color != packetColors[packetLength - 1])
                {
                    // We were filling in an RLE packet, and we got a non-repeated color.
                    // Emit the current packet and start a new one.
                    Flush();
                    Push(color);
                }
                else if (packetLength >= 2 && rlePacket && color == packetColors[packetLength - 1])
                {
                    // We are filling in an RLE packet, and we got another repeated color.
                    // Add the new color to the current packet.
                    ++packetLength;
                    packetColors[packetLength - 1] = color;
                }
                else if (packetLength >= 2 && !rlePacket && color != packetColors[packetLength - 1])
                {
                    // We are filling in a raw packet, and we got another random color.
                    // Add the new color to the current packet.
                    ++packetLength;
                    packetColors[packetLength - 1] = color;
                }
                else if (packetLength >= 2 && !rlePacket && color == packetColors[packetLength - 1])
                {
                    // We were filling in a raw packet, but we got a repeated color.
                    // Emit the current packet without its last color, and start a
                    // new RLE packet that starts with a length of 2.
                    --packetLength;
                    Flush();
                    Push(color);
                    Push(color);
                }
            }

            public TgaPacketStateMachine(Stream output, int bitDepth)
            {
                this.output = output;
                this.bitDepth = bitDepth;
            }
        }

        private static void SaveTgaRowRle(Stream output, Surface input, ref TgaHeader header, int y)
        {
            TgaPacketStateMachine machine = new TgaPacketStateMachine(output, header.pixelDepth);

            for (int x = 0; x < input.Width; ++x)
            {
                machine.Push(input[x, y]);
            }

            machine.Flush();
        }

        private static void SaveTgaRowRaw(Stream output, Surface input, ref TgaHeader header, int y)
        {
            for (int x = 0; x < input.Width; ++x)
            {
                ColorBgra color = input[x, y];
                WriteColor(output, color, header.pixelDepth);
            }
        }

        private static void WriteColor(Stream output, ColorBgra color, int bitDepth)
        {
            switch (bitDepth)
            {
                case 24:
                {
                    int red = ((color.R * color.A) + (255 * (255 - color.A))) / 255;
                    int green = ((color.G * color.A) + (255 * (255 - color.A))) / 255;
                    int blue = ((color.B * color.A) + (255 * (255 - color.A))) / 255;
                    int colorInt = blue + (green << 8) + (red << 16);

                    Utility.WriteUInt24(output, colorInt);
                    break;
                }

                case 32:
                    Utility.WriteUInt32(output, color.Bgra);
                    break;
            }
        }

        public TgaFileType()
            : base("TGA",
                   FileTypeFlags.SavesWithProgress |
                       FileTypeFlags.SupportsLoading |
                       FileTypeFlags.SupportsSaving,
                   new string[] { ".tga" })
        {
        }
    }
}
