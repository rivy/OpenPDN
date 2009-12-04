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
using System.Runtime.InteropServices;

namespace PaintDotNet.SystemLayer.GpcWrapper
{
    internal static class NativeMethods
    {
        private static class X64
        {
            [DllImport("ShellExtension_x64.dll")]
            public static extern void gpc_polygon_clip(
                [In] NativeConstants.gpc_op set_operation,
                [In] ref NativeStructs.gpc_polygon subject_polygon,
                [In] ref NativeStructs.gpc_polygon clip_polygon,
                [In, Out] ref NativeStructs.gpc_polygon result_polygon);

            [DllImport("ShellExtension_x64.dll")]
            public static extern void gpc_free_polygon([In] ref NativeStructs.gpc_polygon polygon);
        }

        private static class X86
        {
            [DllImport("ShellExtension_x86.dll")]
            public static extern void gpc_polygon_clip(
                [In] NativeConstants.gpc_op set_operation,
                [In] ref NativeStructs.gpc_polygon subject_polygon,
                [In] ref NativeStructs.gpc_polygon clip_polygon,
                [In, Out] ref NativeStructs.gpc_polygon result_polygon);

            [DllImport("ShellExtension_x86.dll")]
            public static extern void gpc_free_polygon([In] ref NativeStructs.gpc_polygon polygon);
        }

        public static void gpc_polygon_clip(
            [In] NativeConstants.gpc_op set_operation,
            [In] ref NativeStructs.gpc_polygon subject_polygon,
            [In] ref NativeStructs.gpc_polygon clip_polygon,
            [In, Out] ref NativeStructs.gpc_polygon result_polygon)
        {
            if (Processor.Architecture == ProcessorArchitecture.X64)
            {
                X64.gpc_polygon_clip(set_operation, ref subject_polygon, ref clip_polygon, ref result_polygon);
            }
            else if (Processor.Architecture == ProcessorArchitecture.X86)
            {
                X86.gpc_polygon_clip(set_operation, ref subject_polygon, ref clip_polygon, ref result_polygon);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public static void gpc_free_polygon([In] ref NativeStructs.gpc_polygon polygon)
        {
            if (Processor.Architecture == ProcessorArchitecture.X64)
            {
                X64.gpc_free_polygon(ref polygon);
            }
            else if (Processor.Architecture == ProcessorArchitecture.X86)
            {
                X86.gpc_free_polygon(ref polygon);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }
}
