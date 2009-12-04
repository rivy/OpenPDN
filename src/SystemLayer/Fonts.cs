/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace PaintDotNet.SystemLayer
{
    /// <summary>
    /// Static methods related to font handling.
    /// </summary>
    public static class Fonts
    {
        /// <summary>
        /// Determines whether a font uses the 'symbol' character set.
        /// </summary>
        /// <remarks>
        /// Symbol fonts do not typically contain glyphs that represent letters of the alphabet.
        /// Instead they might contain pictures and symbols. As such, they are not useful for
        /// drawing text. Which means you can't use a symbol font to write out its own name for
        /// illustrative purposes (like for the font drop-down chooser).
        /// </remarks>
        public static bool IsSymbolFont(Font font)
        {
            NativeStructs.LOGFONT logFont = new NativeStructs.LOGFONT();
            font.ToLogFont(logFont);
            return logFont.lfCharSet == NativeConstants.SYMBOL_CHARSET;
        }

        private static IntPtr CreateFontObject(Font font, bool antiAliasing)
        {
            NativeStructs.LOGFONT logFont = new NativeStructs.LOGFONT();
            font.ToLogFont(logFont);

            int nHeight = logFont.lfHeight;
            int nWidth = logFont.lfWidth;
            int nEscapement = logFont.lfEscapement;
            int nOrientation = logFont.lfOrientation;
            int fnWeight = logFont.lfWeight;
            uint fdwItalic = logFont.lfItalic;
            uint fdwUnderline = logFont.lfUnderline;
            uint fdwStrikeOut = logFont.lfStrikeOut;
            uint fdwCharSet = logFont.lfCharSet;
            uint fdwOutputPrecision = logFont.lfOutPrecision;
            uint fdwClipPrecision = logFont.lfClipPrecision;
            uint fdwQuality;
            
            if (antiAliasing)
            {
                fdwQuality = NativeConstants.ANTIALIASED_QUALITY;
            }
            else
            {
                fdwQuality = NativeConstants.NONANTIALIASED_QUALITY;
            }

            uint fdwPitchAndFamily = logFont.lfPitchAndFamily;
            string lpszFace = logFont.lfFaceName;

            IntPtr hFont = SafeNativeMethods.CreateFontW(
                nHeight,
                nWidth,
                nEscapement,
                nOrientation,
                fnWeight,
                fdwItalic,
                fdwUnderline,
                fdwStrikeOut,
                fdwCharSet,
                fdwOutputPrecision,
                fdwClipPrecision,
                fdwQuality,
                fdwPitchAndFamily,
                lpszFace);

            if (hFont == IntPtr.Zero)
            {
                NativeMethods.ThrowOnWin32Error("CreateFontW returned NULL");
            }

            return hFont;
        }

        /// <summary>
        /// Measures text with the given graphics context, font, string, location, and anti-aliasing flag.
        /// </summary>
        /// <param name="g">The Graphics context to measure for.</param>
        /// <param name="font">The Font to measure with.</param>
        /// <param name="text">The string of text to measure.</param>
        /// <param name="antiAliasing">Whether the font should be rendered with anti-aliasing.</param>
        public static Size MeasureString(Graphics g, Font font, string text, bool antiAliasing, FontSmoothing fontSmoothing)
        {
            if (fontSmoothing == FontSmoothing.Smooth && antiAliasing)
            {
                PixelOffsetMode oldPOM = g.PixelOffsetMode;
                g.PixelOffsetMode = PixelOffsetMode.Half;

                TextRenderingHint oldTRH = g.TextRenderingHint;
                g.TextRenderingHint = TextRenderingHint.AntiAlias;

                StringFormat format = (StringFormat)StringFormat.GenericTypographic.Clone();
                format.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;

                SizeF sf = g.MeasureString(text, font, new PointF(0, 0), format);
                sf.Height = font.GetHeight();

                g.PixelOffsetMode = oldPOM;
                g.TextRenderingHint = oldTRH;
                return Size.Ceiling(sf);
            }
            else if (fontSmoothing == FontSmoothing.Sharp || !antiAliasing)
            {
                IntPtr hdc = IntPtr.Zero;
                IntPtr hFont = IntPtr.Zero;
                IntPtr hOldObject = IntPtr.Zero;

                try
                {
                    hdc = g.GetHdc();
                    hFont = CreateFontObject(font, antiAliasing);
                    hOldObject = SafeNativeMethods.SelectObject(hdc, hFont);

                    NativeStructs.RECT rect = new NativeStructs.RECT();
                    rect.left = 0;
                    rect.top = 0;
                    rect.right = rect.left;
                    rect.bottom = rect.top;

                    int result = SafeNativeMethods.DrawTextW(
                        hdc, 
                        text, 
                        text.Length, 
                        ref rect,
                        NativeConstants.DT_CALCRECT | 
                            NativeConstants.DT_LEFT | 
                            NativeConstants.DT_NOCLIP |
                            NativeConstants.DT_NOPREFIX | 
                            NativeConstants.DT_SINGLELINE | 
                            NativeConstants.DT_TOP);

                    if (result == 0)
                    {
                        NativeMethods.ThrowOnWin32Error("DrawTextW returned 0");
                    }

                    return new Size(rect.right - rect.left, rect.bottom - rect.top);
                }

                finally
                {
                    if (hOldObject != IntPtr.Zero)
                    {
                        SafeNativeMethods.SelectObject(hdc, hOldObject);
                        hOldObject = IntPtr.Zero;
                    }

                    if (hFont != IntPtr.Zero)
                    {
                        SafeNativeMethods.DeleteObject(hFont);
                        hFont = IntPtr.Zero;
                    }

                    if (hdc != IntPtr.Zero)
                    {
                        g.ReleaseHdc(hdc);
                        hdc = IntPtr.Zero;
                    }
                }
            }
            else
            {
                throw new InvalidEnumArgumentException("fontSmoothing = " + (int)fontSmoothing);
            }
        }

        /// <summary>
        /// Renders text with the given graphics context, font, string, location, and anti-aliasing flag.
        /// </summary>
        /// <param name="g">The Graphics context to render to.</param>
        /// <param name="font">The Font to render with.</param>
        /// <param name="text">The string of text to draw.</param>
        /// <param name="pt">The offset of where to start drawing (upper-left of rendering rectangle).</param>
        /// <param name="antiAliasing">Whether the font should be rendered with anti-aliasing.</param>
        public static void DrawText(Graphics g, Font font, string text, Point pt, bool antiAliasing, FontSmoothing fontSmoothing)
        {
            if (fontSmoothing == FontSmoothing.Smooth && antiAliasing)
            {
                PixelOffsetMode oldPOM = g.PixelOffsetMode;
                g.PixelOffsetMode = PixelOffsetMode.Half;

                TextRenderingHint oldTRH = g.TextRenderingHint;
                g.TextRenderingHint = TextRenderingHint.AntiAlias;

                StringFormat format = (StringFormat)StringFormat.GenericTypographic.Clone();
                format.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;

                g.DrawString(text, font, Brushes.Black, pt, format);

                g.PixelOffsetMode = oldPOM;
                g.TextRenderingHint = oldTRH;
            }
            else if (fontSmoothing == FontSmoothing.Sharp || !antiAliasing)
            {
                IntPtr hdc = IntPtr.Zero;
                IntPtr hFont = IntPtr.Zero;
                IntPtr hOldObject = IntPtr.Zero;

                try
                {
                    hdc = g.GetHdc();
                    hFont = CreateFontObject(font, antiAliasing);
                    hOldObject = SafeNativeMethods.SelectObject(hdc, hFont);

                    NativeStructs.RECT rect = new NativeStructs.RECT();
                    rect.left = pt.X;
                    rect.top = pt.Y;
                    rect.right = rect.left;
                    rect.bottom = rect.top;

                    int result = SafeNativeMethods.DrawTextW(
                        hdc, 
                        text, 
                        text.Length, 
                        ref rect,
                        NativeConstants.DT_LEFT | 
                            NativeConstants.DT_NOCLIP | 
                            NativeConstants.DT_NOPREFIX |
                            NativeConstants.DT_SINGLELINE | 
                            NativeConstants.DT_TOP);

                    if (result == 0)
                    {
                        NativeMethods.ThrowOnWin32Error("DrawTextW returned 0");
                    }
                }

                finally
                {
                    if (hOldObject != IntPtr.Zero)
                    {
                        SafeNativeMethods.SelectObject(hdc, hOldObject);
                        hOldObject = IntPtr.Zero;
                    }

                    if (hFont != IntPtr.Zero)
                    {
                        SafeNativeMethods.DeleteObject(hFont);
                        hFont = IntPtr.Zero;
                    }

                    if (hdc != IntPtr.Zero)
                    {
                        g.ReleaseHdc(hdc);
                        hdc = IntPtr.Zero;
                    }
                }
            }
            else
            {
                throw new InvalidEnumArgumentException("fontSmoothing = " + (int)fontSmoothing);
            }
        }
    }
}
