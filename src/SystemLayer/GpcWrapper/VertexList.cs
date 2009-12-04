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
using System.Collections.Generic;

namespace PaintDotNet.SystemLayer.GpcWrapper
{
    internal sealed class VertexList
    {
        public int NofVertices;
        public Vertex[] Vertex;

        public VertexList()
        {
        }

        public VertexList(Vertex[] v)
            : this(v, false)
        {
        }

        public VertexList(Vertex[] v, bool takeOwnership)
        {
            if (takeOwnership)
            {
                this.Vertex = v;
                this.NofVertices = v.Length;
            }
            else
            {
                this.Vertex = (Vertex[])v.Clone();
                this.NofVertices = v.Length;
            }
        }

        public VertexList(PointF[] p)
        {
            NofVertices = p.Length;
            Vertex = new Vertex[NofVertices];
            for (int i = 0; i < p.Length; i++)
                Vertex[i] = new Vertex((double)p[i].X, (double)p[i].Y);
        }

        public GraphicsPath ToGraphicsPath()
        {
            GraphicsPath graphicsPath = new GraphicsPath();
            graphicsPath.AddLines(ToPoints());
            return graphicsPath;
        }

        public PointF[] ToPoints()
        {
            PointF[] vertexArray = new PointF[NofVertices];
            for (int i = 0; i < NofVertices; i++)
            {
                vertexArray[i] = new PointF((float)Vertex[i].X, (float)Vertex[i].Y);
            }
            return vertexArray;
        }

        public GraphicsPath TristripToGraphicsPath()
        {
            GraphicsPath graphicsPath = new GraphicsPath();

            for (int i = 0; i < NofVertices - 2; i++)
            {
                graphicsPath.AddPolygon(new PointF[3]{ new PointF( (float)Vertex[i].X,   (float)Vertex[i].Y ),
				                                           new PointF( (float)Vertex[i+1].X, (float)Vertex[i+1].Y ),
				                                           new PointF( (float)Vertex[i+2].X, (float)Vertex[i+2].Y )  });
            }

            return graphicsPath;
        }

        public override string ToString()
        {
            string s = "Polygon with " + NofVertices + " vertices: ";

            for (int i = 0; i < NofVertices; i++)
            {
                s += Vertex[i].ToString();
                if (i != NofVertices - 1)
                    s += ",";
            }
            return s;
        }
    }
}
