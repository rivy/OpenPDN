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
using System.Drawing.Imaging;

namespace PaintDotNet
{
    public sealed class Exif
    {
        private Exif()
        {
        }

        public static PropertyItem CreatePropertyItem(ExifTagID id, ExifTagType type, byte[] data)
        {
            return CreatePropertyItem((short)id, type, data);
        }

        public static PropertyItem CreatePropertyItem(short id, ExifTagType type, byte[] data)
        {
            PropertyItem pi = PdnGraphics.CreatePropertyItem();

            pi.Id = id;
            pi.Type = (short)type;
            pi.Len = data.Length;
            pi.Value = (byte[])data.Clone();

            return pi;
        }

        public static string DecodeAsciiValue(PropertyItem pi)
        {
            if (pi.Type != (short)ExifTagType.Ascii)
            {
                throw new ArgumentException("pi.Type != ExifTagType.Ascii");
            }
            
            string data = System.Text.Encoding.ASCII.GetString(pi.Value);
            if (data[data.Length - 1] == '\0')
            {
                data = data.Substring(0, data.Length - 1);
            }

            return data;
        }

        public static PropertyItem CreateAscii(ExifTagID id, string value)
        {
            return CreateAscii((short)id, value);
        }

        public static PropertyItem CreateAscii(short id, string value)
        {
            return CreatePropertyItem(id, ExifTagType.Ascii, EncodeAsciiValue(value + "\0"));
        }

        public static byte[] EncodeAsciiValue(string value)
        {
            return System.Text.Encoding.ASCII.GetBytes(value);
        }

        public static byte DecodeByteValue(PropertyItem pi)
        {
            if (pi.Type != (short)ExifTagType.Byte)
            {
                throw new ArgumentException("pi.Type != ExifTagType.Byte");
            }

            if (pi.Value.Length != 1)
            {
                throw new ArgumentException("pi.Value.Length != 1");
            }

            if (pi.Len != 1)
            {
                throw new ArgumentException("pi.Length != 1");
            }

            return pi.Value[0];
        }

        public static PropertyItem CreateByte(ExifTagID id, byte value)
        {
            return CreateByte((short)id, value);
        }

        public static PropertyItem CreateByte(short id, byte value)
        {
            return CreatePropertyItem(id, ExifTagType.Byte, EncodeByteValue(value));
        }

        public static byte[] EncodeByteValue(byte value)
        {
            return new byte[] { value };
        }

        public static ushort DecodeShortValue(PropertyItem pi)
        {
            if (pi.Type != (short)ExifTagType.Short)
            {
                throw new ArgumentException("pi.Type != ExifTagType.Short");
            }

            if (pi.Value.Length != 2)
            {
                throw new ArgumentException("pi.Value.Length != 2");
            }

            if (pi.Len != 2)
            {
                throw new ArgumentException("pi.Length != 2");
            }

            return (ushort)(pi.Value[0] + (pi.Value[1] << 8));
        }

        public static PropertyItem CreateShort(ExifTagID id, ushort value)
        {
            return CreateShort((short)id, value);
        }

        public static PropertyItem CreateShort(short id, ushort value)
        {
            return CreatePropertyItem(id, ExifTagType.Short, EncodeShortValue(value));
        }

        public static byte[] EncodeShortValue(ushort value)
        {
            return new byte[] { 
                                  (byte)(value & 0xff), 
                                  (byte)((value >> 8) & 0xff) 
                              };
        }

        public static uint DecodeLongValue(PropertyItem pi)
        {
            if (pi.Type != (short)ExifTagType.Long)
            {
                throw new ArgumentException("pi.Type != ExifTagType.Long");
            }

            if (pi.Value.Length != 4)
            {
                throw new ArgumentException("pi.Value.Length != 4");
            }

            if (pi.Len != 4)
            {
                throw new ArgumentException("pi.Length != 4");
            }

            return (uint)pi.Value[0] + ((uint)pi.Value[1] << 8) + ((uint)pi.Value[2] << 16) + ((uint)pi.Value[3] << 24);
        }

        public static PropertyItem CreateLong(ExifTagID id, uint value)
        {
            return CreateLong((short)id, value);
        }

        public static PropertyItem CreateLong(short id, uint value)
        {
            return CreatePropertyItem(id, ExifTagType.Long, EncodeLongValue(value));
        }

        public static byte[] EncodeLongValue(uint value)
        {
            return new byte[] {
                                  (byte)(value & 0xff),
                                  (byte)((value >> 8) & 0xff),
                                  (byte)((value >> 16) & 0xff),
                                  (byte)((value >> 24) & 0xff)
                              };
        }

        public static void DecodeRationalValue(PropertyItem pi, out uint numerator, out uint denominator)
        {
            if (pi.Type != (short)ExifTagType.Rational)
            {
                throw new ArgumentException("pi.Type != ExifTagType.Rational");
            }

            if (pi.Value.Length != 8)
            {
                throw new ArgumentException("pi.Value.Length != 8");
            }

            if (pi.Len != 8)
            {
                throw new ArgumentException("pi.Length != 8");
            }

            numerator = (uint)pi.Value[0] + ((uint)pi.Value[1] << 8) + ((uint)pi.Value[2] << 16) + ((uint)pi.Value[3] << 24);
            denominator = (uint)pi.Value[4] + ((uint)pi.Value[5] << 8) + ((uint)pi.Value[6] << 16) + ((uint)pi.Value[7] << 24);
        }

        public static PropertyItem CreateRational(ExifTagID id, uint numerator, uint denominator)
        {
            return CreateRational((short)id, numerator, denominator);
        }

        public static PropertyItem CreateRational(short id, uint numerator, uint denominator)
        {
            return CreatePropertyItem(id, ExifTagType.Rational, EncodeRationalValue(numerator, denominator));
        }

        public static byte[] EncodeRationalValue(uint numerator, uint denominator)
        {
            return new byte[] {
                                  (byte)(numerator & 0xff),
                                  (byte)((numerator >> 8) & 0xff),
                                  (byte)((numerator >> 16) & 0xff),
                                  (byte)((numerator >> 24) & 0xff),
                                  (byte)(denominator & 0xff),
                                  (byte)((denominator >> 8) & 0xff),
                                  (byte)((denominator >> 16) & 0xff),
                                  (byte)((denominator >> 24) & 0xff)
                              };
        }

        public static byte DecodeUndefinedValue(PropertyItem pi)
        {
            if (pi.Type != (short)ExifTagType.Undefined)
            {
                throw new ArgumentException("pi.Type != ExifTagType.Undefined");
            }

            if (pi.Value.Length != 1)
            {
                throw new ArgumentException("pi.Value.Length != 1");
            }

            if (pi.Len != 1)
            {
                throw new ArgumentException("pi.Length != 1");
            }

            return pi.Value[0];
        }

        public static PropertyItem CreateUndefined(ExifTagID id, byte value)
        {
            return CreateUndefined((short)id, value);
        }

        public static PropertyItem CreateUndefined(short id, byte value)
        {
            return CreatePropertyItem(id, ExifTagType.Undefined, EncodeUndefinedValue(value));
        }

        public static byte[] EncodeUndefinedValue(byte value)
        {
            return new byte[] { value };
        }

        public static int DecodeSLongValue(PropertyItem pi)
        {
            if (pi.Type != (short)ExifTagType.SLong)
            {
                throw new ArgumentException("pi.Type != ExifTagType.SLong");
            }

            if (pi.Value.Length != 4)
            {
                throw new ArgumentException("pi.Value.Length != 4");
            }

            if (pi.Len != 4)
            {
                throw new ArgumentException("pi.Length != 4");
            }

            return pi.Value[0] + (pi.Value[1] << 8) + (pi.Value[2] << 16) + (pi.Value[3] << 24);
        }

        public static PropertyItem CreateSLong(ExifTagID id, int value)
        {
            return CreateSLong((short)id, value);
        }

        public static PropertyItem CreateSLong(short id, int value)
        {
            return CreatePropertyItem(id, ExifTagType.SLong, EncodeSLongValue(value));
        }

        public static byte[] EncodeSLongValue(int value)
        {
            return new byte[] {
                                  (byte)(value & 0xff),
                                  (byte)((value >> 8) & 0xff),
                                  (byte)((value >> 16) & 0xff),
                                  (byte)((value >> 24) & 0xff)
                              };
        }

        public static void DecodeRationalValue(PropertyItem pi, out int numerator, out int denominator)
        {
            if (pi.Type != (short)ExifTagType.SRational)
            {
                throw new ArgumentException("pi.Type != ExifTagType.SRational");
            }

            if (pi.Value.Length != 8)
            {
                throw new ArgumentException("pi.Value.Length != 8");
            }

            if (pi.Len != 8)
            {
                throw new ArgumentException("pi.Length != 8");
            }

            numerator = pi.Value[0] + (pi.Value[1] << 8) + (pi.Value[2] << 16) + (pi.Value[3] << 24);
            denominator = pi.Value[4] + (pi.Value[5] << 8) + (pi.Value[6] << 16) + (pi.Value[7] << 24);
        }

        public static PropertyItem CreateSRational(ExifTagID id, int numerator, int denominator)
        {
            return CreateSRational((short)id, numerator, denominator);
        }

        public static PropertyItem CreateSRational(short id, int numerator, int denominator)
        {
            return CreatePropertyItem(id, ExifTagType.SRational, EncodeSRationalValue(numerator, denominator));
        }

        public static byte[] EncodeSRationalValue(int numerator, int denominator)
        {
            return new byte[] {
                                  (byte)(numerator & 0xff),
                                  (byte)((numerator >> 8) & 0xff),
                                  (byte)((numerator >> 16) & 0xff),
                                  (byte)((numerator >> 24) & 0xff),
                                  (byte)(denominator & 0xff),
                                  (byte)((denominator >> 8) & 0xff),
                                  (byte)((denominator >> 16) & 0xff),
                                  (byte)((denominator >> 24) & 0xff)
                              };
        }
    }
}
