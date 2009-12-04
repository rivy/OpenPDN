/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;

namespace PaintDotNet.SystemLayer.GpcWrapper
{
    internal struct Vertex
    {
        public double X;
        public double Y;

        public Vertex(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }
    }
}
