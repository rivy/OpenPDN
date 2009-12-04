/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace PaintDotNet.SystemLayer.GpcWrapper
{
    internal static class NativeStructs
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct gpc_vertex                    /* Polygon vertex structure          */
        {
            public double x;            /* Vertex x component                */
            public double y;            /* vertex y component                */
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct gpc_vertex_list               /* Vertex list structure             */
        {
            public int num_vertices; /* Number of vertices in list        */
            public IntPtr vertex;       /* Vertex array pointer              */
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct gpc_polygon                   /* Polygon set structure             */
        {
            public int num_contours; /* Number of contours in polygon     */
            public IntPtr hole;         /* Hole / external contour flags     */
            public IntPtr contour;      /* Contour array pointer             */
        }
    }
}
