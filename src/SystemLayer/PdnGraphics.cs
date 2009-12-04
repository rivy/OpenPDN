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
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.InteropServices;

namespace PaintDotNet.SystemLayer
{
    /// <summary>
    /// These methods are used because we found some bugs in GDI+ / WinForms. Some
    /// were the cause of major flickering with the transparent toolforms.
    /// Other implementations of this class, or more generic implementations, may safely 
    /// thunk straight to equivelants in System.Drawing.Graphics.
    /// </summary>
    public static class PdnGraphics
    {
        public static GraphicsPath ClipPath(GraphicsPath subjectPath, CombineMode combineMode, GraphicsPath clipPath)
        {
            GpcWrapper.Polygon.Validate(combineMode);

            GpcWrapper.Polygon basePoly = new GpcWrapper.Polygon(subjectPath);

            GraphicsPath clipClone = (GraphicsPath)clipPath.Clone();
            clipClone.CloseAllFigures();
            GpcWrapper.Polygon clipPoly = new GpcWrapper.Polygon(clipClone);
            clipClone.Dispose();

            GpcWrapper.Polygon clippedPoly = GpcWrapper.Polygon.Clip(combineMode, basePoly, clipPoly);

            GraphicsPath returnPath = clippedPoly.ToGraphicsPath();
            returnPath.CloseAllFigures();
            return returnPath;
        }

        public static void SetPropertyItems(Image image, PropertyItem[] items)
        {
            PropertyItem[] pis = image.PropertyItems;

            foreach (PropertyItem pi in pis)
            {
                image.RemovePropertyItem(pi.Id);
            }

            foreach (PropertyItem pi in items)
            {
                image.SetPropertyItem(pi);
            }
        }

        /// <summary>
        /// Creates a new, zero-filled PropertyItem.
        /// </summary>
        /// <returns>A PropertyItem that is zero-filled.</returns>
        public static PropertyItem CreatePropertyItem()
        {
            PropertyItem2 pi2 = new PropertyItem2(0, 0, 0, new byte[0]);
            return pi2.ToPropertyItem();
        }

        /// <summary>
        /// Copies the given PropertyItem.
        /// </summary>
        /// <param name="pi">The PropertyItem to clone.</param>
        /// <returns>A copy of the given PropertyItem.</returns>
        public static PropertyItem ClonePropertyItem(PropertyItem pi)
        {
            byte[] valueClone;

            if (pi.Value == null)
            {
                valueClone = new byte[0];
            }
            else
            {
                valueClone = (byte[])pi.Value.Clone();
            }

            PropertyItem2 pi2 = new PropertyItem2(pi.Id, pi.Len, pi.Type, valueClone);
            return pi2.ToPropertyItem();
        }

        /// <summary>
        /// Serializes a PropertyItem into a string blob.
        /// </summary>
        /// <param name="pi">The PropertyItem to serialize.</param>
        /// <returns>A string that may be later deserialized using DeserializePropertyItem.</returns>
        /// <remarks>
        /// Note to implementors: The format for the serialized data is intentionally opaque for programmatic users
        /// of this class. However, since this data goes into .PDN files, it must be carefully maintained. See
        /// the PropertyItem2 class for details.
        /// </remarks>
        public static string SerializePropertyItem(PropertyItem pi)
        {
            PropertyItem2 pi2 = PropertyItem2.FromPropertyItem(pi);
            return pi2.ToBlob();
        }

        /// <summary>
        /// Deserializes a PropertyItem from a string previously returned from SerializePropertyItem.
        /// </summary>
        /// <param name="piBlob">The string data to deserialize.</param>
        /// <returns>A PropertyItem instance.</returns>
        /// <remarks>
        /// Note to implementors: The format for the serialized data is intentionally opaque for programmatic users
        /// of this class. However, since this data goes into .PDN files, it must be carefully maintained. See
        /// the PropertyItem2 class for details.
        /// </remarks>
        public static PropertyItem DeserializePropertyItem(string piBlob)
        {
            PropertyItem2 pi2 = PropertyItem2.FromBlob(piBlob);
            return pi2.ToPropertyItem();
        }

        /// <summary>
        /// Draws a bitmap to a Graphics context.
        /// </summary>
        /// <param name="dst">The Graphics context to draw the bitmap on to.</param>
        /// <param name="dstRect">The clipping rectangle in destination coordinates.</param>
        /// <param name="dstMatrix">The transformation matrix to apply. This is only used to transform the upper-left corner of dstRect.</param>
        /// <param name="srcBitmapHandle">The handle to the bitmap obtained from Memory.AllocateBitmap().</param>
        /// <param name="srcWidth">The full width of the bitmap.</param>
        /// <param name="srcHeight">The full height of the bitmap.</param>
        /// <param name="srcOffsetX">The left edge of the source bitmap to draw from.</param>
        /// <param name="srcOffsetY">The top edge of the source bitmap to draw from.</param>
        public unsafe static void DrawBitmap(
            Graphics dst,
            Rectangle dstRect,
            Matrix dstMatrix,
            IntPtr srcBitmapHandle,
            int srcOffsetX,
            int srcOffsetY)
        {
            if (srcBitmapHandle == IntPtr.Zero)
            {
                throw new ArgumentNullException("srcBitmapHandle");
            }

            Point[] points = new Point[] { dstRect.Location };
            dstMatrix.TransformPoints(points);
            dstRect.Location = points[0];

            IntPtr hdc = IntPtr.Zero;
            IntPtr hbitmap = IntPtr.Zero;
            IntPtr chdc = IntPtr.Zero;
            IntPtr old = IntPtr.Zero;

            try
            {
                hdc = dst.GetHdc();
                chdc = SafeNativeMethods.CreateCompatibleDC(hdc);
                old = SafeNativeMethods.SelectObject(chdc, srcBitmapHandle);
                SafeNativeMethods.BitBlt(hdc, dstRect.Left, dstRect.Top, dstRect.Width, 
                    dstRect.Height, chdc, srcOffsetX, srcOffsetY, NativeConstants.SRCCOPY);
            }

            finally
            {
                if (old != IntPtr.Zero)
                {
                    SafeNativeMethods.SelectObject(chdc, old);
                    old = IntPtr.Zero;
                }

                if (chdc != IntPtr.Zero)
                {
                    SafeNativeMethods.DeleteDC(chdc);
                    chdc = IntPtr.Zero;
                }
            
                if (hdc != IntPtr.Zero)
                {
                    dst.ReleaseHdc(hdc);
                    hdc = IntPtr.Zero;
                }
            }

            GC.KeepAlive(dst);
        }
        
        internal unsafe static void GetRegionScans(IntPtr hRgn, out Rectangle[] scans, out int area)
        {
            uint bytes = 0;
            int countdown = screwUpMax;
            int error = 0;
                    
            // HACK: It seems that sometimes the GetRegionData will return ERROR_INVALID_HANDLE
            //       even though the handle (the HRGN) is fine. Maybe the function is not
            //       re-entrant? I'm not sure, but trying it again seems to fix it.
            while (countdown > 0)
            {
                bytes = SafeNativeMethods.GetRegionData(hRgn, 0, (NativeStructs.RGNDATA *)IntPtr.Zero);
                error = Marshal.GetLastWin32Error();

                if (bytes == 0)
                {
                    --countdown;
                    System.Threading.Thread.Sleep(5);
                }
                else
                {
                    break;
                }
            }

            // But if we retry several times and it still messes up then we will finally give up.
            if (bytes == 0)
            {
                throw new Win32Exception(error, "GetRegionData returned " + bytes.ToString() + ", GetLastError() = " + error.ToString());
            }

            byte *data;
                        
            // Up to 512 bytes, allocate on the stack. Otherwise allocate from the heap.
            if (bytes <= 512)
            {
                byte *data1 = stackalloc byte[(int)bytes];
                data = data1;
            }
            else
            {
                data = (byte *)Memory.Allocate(bytes).ToPointer();
            }                        

            try
            {
                NativeStructs.RGNDATA *pRgnData = (NativeStructs.RGNDATA *)data;
                uint result = SafeNativeMethods.GetRegionData(hRgn, bytes, pRgnData);

                if (result != bytes)
                {
                    throw new OutOfMemoryException("SafeNativeMethods.GetRegionData returned 0");
                }

                NativeStructs.RECT *pRects = NativeStructs.RGNDATA.GetRectsPointer(pRgnData);
                scans = new Rectangle[pRgnData->rdh.nCount];
                area = 0;
            
                for (int i = 0; i < scans.Length; ++i)
                {
                    scans[i] = Rectangle.FromLTRB(pRects[i].left, pRects[i].top, pRects[i].right, pRects[i].bottom);
                    area += scans[i].Width * scans[i].Height;
                }

                pRects = null;
                pRgnData = null;
            }
            
            finally
            {
                if (bytes > 512)
                {
                    Memory.Free(new IntPtr(data));
                }
            }
        }

        private const int screwUpMax = 100;

        /// <summary>
        /// Retrieves an array of rectangles that approximates a region, and computes the
        /// pixel area of it. This method is necessary to work around some bugs in .NET
        /// and to increase performance for the way in which we typically use this data.
        /// </summary>
        /// <param name="region">The Region to retrieve data from.</param>
        /// <param name="scans">An array of Rectangle to put the scans into.</param>
        /// <param name="area">An integer to write the computed area of the region into.</param>
        /// <remarks>
        /// Note to implementors: Simple implementations may simple call region.GetRegionScans()
        /// and process the data for the 'out' variables.</remarks>
        public static void GetRegionScans(Region region, out Rectangle[] scans, out int area)
        {
            using (NullGraphics nullGraphics = new NullGraphics())
            {
                IntPtr hRgn = IntPtr.Zero;
                
                try
                {
                    hRgn = region.GetHrgn(nullGraphics.Graphics);
                    GetRegionScans(hRgn, out scans, out area);
                }

                finally
                {
                    if (hRgn != IntPtr.Zero)
                    {
                        SafeNativeMethods.DeleteObject(hRgn);
                        hRgn = IntPtr.Zero;
                    }
                }
            }

            GC.KeepAlive(region);
        }

        /// <summary>
        /// Draws a polygon. The last point is not joined to the beginning point. If there is an error while
        /// trying to draw, it is discarded and ignored.
        /// </summary>
        /// <param name="g">The Graphics context to draw to.</param>
        /// <param name="points">The points to draw. Lines are drawn between every point N to point N+1.</param>
        /// <param name="color">The color to draw with.</param>
        /// <remarks>
        /// Note to implementors: This method is used to avoid drawing with GDI+, which avoids flickering
        /// with our transparent toolforms. Implementations may thunk straight to g.DrawLines().
        /// </remarks>
        public static void DrawPolyLine(Graphics g, Color color, Point[] points)
        {
            try
            {
                DrawPolyLineImpl(g, color, points);
            }

            catch (Exception ex)
            {
                Tracing.Ping("Exception while executing PdnGraphics.DrawPolyLine: " + ex.ToString());
            }
        }

        private static void DrawPolyLineImpl(Graphics g, Color color, Point[] points)
        {
            if (points.Length < 1)
            {
                return;
            }

            uint nativeColor = (uint)(color.R  + (color.G << 8) + (color.B << 16));

            IntPtr hdc = IntPtr.Zero;
            IntPtr pen = IntPtr.Zero;
            IntPtr oldObject = IntPtr.Zero;

            try
            {
                hdc = g.GetHdc();
                pen = SafeNativeMethods.CreatePen(NativeConstants.PS_SOLID, 1, nativeColor);

                if (pen == IntPtr.Zero)
                {
                    NativeMethods.ThrowOnWin32Error("CreatePen returned NULL");
                }

                oldObject = SafeNativeMethods.SelectObject(hdc, pen);

                NativeStructs.POINT pt;
                bool bResult = SafeNativeMethods.MoveToEx(hdc, points[0].X, points[0].Y, out pt);
                
                if (!bResult)
                {
                    NativeMethods.ThrowOnWin32Error("MoveToEx returned false");
                }

                for (int i = 1; i < points.Length; ++i)
                {
                    bResult = SafeNativeMethods.LineTo(hdc, points[i].X, points[i].Y);

                    if (!bResult)
                    {
                        NativeMethods.ThrowOnWin32Error("LineTo returned false");
                    }
                }
            }

            finally
            {
                if (oldObject != IntPtr.Zero)
                {
                    SafeNativeMethods.SelectObject(hdc, oldObject);
                    oldObject = IntPtr.Zero;
                }

                if (pen != IntPtr.Zero)
                {
                    SafeNativeMethods.DeleteObject(pen);
                    pen = IntPtr.Zero;
                }

                if (hdc != IntPtr.Zero)
                {
                    g.ReleaseHdc(hdc);
                    hdc = IntPtr.Zero;
                }
            }

            GC.KeepAlive(g);
        }

        /// <summary>
        /// Draws several filled rectangles using the same color. If there is an error while trying to draw,
        /// it is discarded and ignored.
        /// </summary>
        /// <param name="g">The Graphics context to draw to.</param>
        /// <param name="rects">A list of rectangles to draw.</param>
        /// <param name="color">The color to fill the rectangles with.</param>
        /// <remarks>
        /// Note to implementors: This method is used to avoid drawing with GDI+, which avoids flickering
        /// with our transparent toolforms. Implementations may thunk straight to g.FillRectangle().
        /// </remarks>
        public static void FillRectangles(Graphics g, Color color, Rectangle[] rects)
        {
            try
            {
                FillRectanglesImpl(g, color, rects);
            }

            catch (Exception ex)
            {
                Tracing.Ping("Exception while executing PdnGraphics.FillRectangles: " + ex.ToString());
            }
        }

        private static void FillRectanglesImpl(Graphics g, Color color, Rectangle[] rects)
        {
            uint nativeColor = (uint)(color.R  + (color.G << 8) + (color.B << 16));

            IntPtr hdc = IntPtr.Zero;
            IntPtr brush = IntPtr.Zero;
            IntPtr oldObject = IntPtr.Zero;

            try
            {
                hdc = g.GetHdc();
                brush = SafeNativeMethods.CreateSolidBrush(nativeColor);

                if (brush == IntPtr.Zero)
                {
                    NativeMethods.ThrowOnWin32Error("CreateSolidBrush returned NULL");
                }

                oldObject = SafeNativeMethods.SelectObject(hdc, brush);

                foreach (Rectangle rect in rects)
                {
                    NativeStructs.RECT nativeRect;

                    nativeRect.left = rect.Left;
                    nativeRect.top = rect.Top;
                    nativeRect.right = rect.Right;
                    nativeRect.bottom = rect.Bottom;

                    int result = SafeNativeMethods.FillRect(hdc, ref nativeRect, brush);

                    if (result == 0)
                    {
                        NativeMethods.ThrowOnWin32Error("FillRect returned zero");
                    }
                }
            }

            finally
            {
                if (oldObject != IntPtr.Zero)
                {
                    SafeNativeMethods.SelectObject(hdc, oldObject);
                    oldObject = IntPtr.Zero;
                }

                if (brush != IntPtr.Zero)
                {
                    SafeNativeMethods.DeleteObject(brush);
                    brush = IntPtr.Zero;
                }

                if (hdc != IntPtr.Zero)
                {
                    g.ReleaseHdc(hdc);
                    hdc = IntPtr.Zero;
                }
            }

            GC.KeepAlive(g);
        }
    }
}

