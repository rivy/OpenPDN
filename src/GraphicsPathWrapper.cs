/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.Serialization;

namespace PaintDotNet
{
    /// <summary>
    /// Encapsulates the data necessary to create a PdnGraphicsPath object
    /// so that we can serialize one to the clipboard (or anywhere else
    /// for that matter)
    /// </summary>
    [Serializable]
    internal class GraphicsPathWrapper
    {
        private PointF[] points;
        private byte[] types;
        private FillMode fillMode;

        public PdnGraphicsPath CreateGraphicsPath()
        {
            return new PdnGraphicsPath(points, types, fillMode);
        }

        public GraphicsPathWrapper(PdnGraphicsPath path)
        {
            points = (PointF[])path.PathPoints.Clone();
            types = (byte[])path.PathTypes.Clone();
            fillMode = path.FillMode;
        }
    }
}
