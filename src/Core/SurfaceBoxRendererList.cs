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
using System.Collections;   
using System.Windows.Forms;

namespace PaintDotNet
{
    public sealed class SurfaceBoxRendererList
    {
        private SurfaceBoxRenderer[] list;
        private SurfaceBoxRenderer[] topList;
        private Size sourceSize;
        private Size destinationSize;
        private ScaleFactor scaleFactor; // ratio is dst:src
        private object lockObject = new object();

        public object SyncRoot
        {
            get
            {
                return this.lockObject;
            }
        }

        public ScaleFactor ScaleFactor
        {
            get
            {
                return this.scaleFactor;
            }
        }

        public SurfaceBoxRenderer[][] Renderers
        {
            get
            {
                return new SurfaceBoxRenderer[][] { list, topList };
            }
        }

        private void ComputeScaleFactor()
        {
            scaleFactor = new ScaleFactor(this.DestinationSize.Width, this.SourceSize.Width);
        }

        public Point SourceToDestination(Point pt)
        {
            return this.scaleFactor.ScalePoint(pt);
        }

        public RectangleF SourceToDestination(Rectangle rect)
        {
            return this.scaleFactor.ScaleRectangle((RectangleF)rect);
        }

        public Point DestinationToSource(Point pt)
        {
            return this.scaleFactor.UnscalePoint(pt);
        }

        public void Add(SurfaceBoxRenderer addMe, bool alwaysOnTop)
        {
            SurfaceBoxRenderer[] startList = alwaysOnTop ? this.topList : this.list;
            SurfaceBoxRenderer[] listPlusOne = new SurfaceBoxRenderer[startList.Length + 1];

            for (int i = 0; i < startList.Length; ++i)
            {
                listPlusOne[i] = startList[i];
            }

            listPlusOne[listPlusOne.Length - 1] = addMe;

            if (alwaysOnTop)
            {
                this.topList = listPlusOne;
            }
            else
            {
                this.list = listPlusOne;
            }

            Invalidate();
        }

        public void Remove(SurfaceBoxRenderer removeMe)
        {
            if (this.list.Length == 0 && this.topList.Length == 0)
            {
                throw new InvalidOperationException("zero items left, can't remove anything");
            }
            else
            {            
                bool found = false;

                if (this.list.Length > 0)
                {
                    SurfaceBoxRenderer[] listSubOne = new SurfaceBoxRenderer[this.list.Length - 1];
                    bool foundHere = false;
                    int dstIndex = 0;

                    for (int i = 0; i < this.list.Length; ++i)
                    {
                        if (this.list[i] == removeMe)
                        {
                            if (foundHere)
                            {
                                throw new ArgumentException("removeMe appeared multiple times in the list");
                            }
                            else
                            {
                                foundHere = true;
                            }
                        }
                        else
                        {
                            if (dstIndex == this.list.Length - 1)
                            {
                                // was not found
                            }
                            else
                            {
                                listSubOne[dstIndex] = this.list[i];
                                ++dstIndex;
                            }
                        }
                    }

                    if (foundHere)
                    {
                        this.list = listSubOne;
                        found = true;
                    }
                }

                if (this.topList.Length > 0)
                {
                    SurfaceBoxRenderer[] topListSubOne = new SurfaceBoxRenderer[this.topList.Length - 1];
                    int topDstIndex = 0;
                    bool foundHere = false;

                    for (int i = 0; i < this.topList.Length; ++i)
                    {
                        if (this.topList[i] == removeMe)
                        {
                            if (found || foundHere)
                            {
                                throw new ArgumentException("removeMe appeared multiple times in the list");
                            }
                            else
                            {
                                foundHere = true;
                            }
                        }
                        else
                        {
                            if (topDstIndex == this.topList.Length - 1)
                            {
                                // was not found
                            }
                            else
                            {
                                topListSubOne[topDstIndex] = this.topList[i];
                                ++topDstIndex;
                            }
                        }
                    }

                    if (foundHere)
                    {
                        this.topList = topListSubOne;
                        found = true;
                    }
                }

                if (!found)
                {
                    throw new ArgumentException("removeMe was not found", "removeMe");
                }

                Invalidate();
            }
        }

        private void OnDestinationSizeChanged()
        {
            InvalidateLookups();

            if (this.destinationSize.Width != 0 && this.sourceSize.Width != 0)
            {
                ComputeScaleFactor();

                for (int i = 0; i < this.list.Length; ++i)
                {
                    this.list[i].OnDestinationSizeChanged();
                }

                for (int i = 0; i < this.topList.Length; ++i)
                {
                    this.topList[i].OnDestinationSizeChanged();
                }
            }
        }

        public Size DestinationSize
        {
            get
            {
                return this.destinationSize;
            }

            set
            {
                if (this.destinationSize != value)
                {
                    this.destinationSize = value;
                    OnDestinationSizeChanged();
                }
            }
        }

        private void OnSourceSizeChanged()
        {
            InvalidateLookups();

            if (this.destinationSize.Width != 0 && this.sourceSize.Width != 0)
            {
                ComputeScaleFactor();

                for (int i = 0; i < this.list.Length; ++i)
                {
                    this.list[i].OnSourceSizeChanged();
                }

                for (int i = 0; i < this.topList.Length; ++i)
                {
                    this.topList[i].OnSourceSizeChanged();
                }
            }
        }

        public Size SourceSize
        {
            get
            {
                return this.sourceSize;
            }

            set
            {
                if (this.sourceSize != value)
                {
                    this.sourceSize = value;
                    OnSourceSizeChanged();
                }
            }
        }

        public int[] Dst2SrcLookupX
        {
            get
            {
                lock (this.SyncRoot)
                {
                    CreateD2SLookupX();
                }

                return this.d2SLookupX;
            }
        }

        private int[] d2SLookupX; // maps from destination->source coordinates
        private void CreateD2SLookupX()
        {
            if (this.d2SLookupX == null || this.d2SLookupX.Length != this.DestinationSize.Width + 1)
            {
                this.d2SLookupX = new int[this.DestinationSize.Width + 1];

                for (int x = 0; x < d2SLookupX.Length; ++x)
                {
                    Point pt = new Point(x, 0);
                    Point surfacePt = this.DestinationToSource(pt);

                    // Sometimes the scale factor is slightly different on one axis than
                    // on another, simply due to accuracy. So we have to clamp this value to
                    // be within bounds.
                    d2SLookupX[x] = Utility.Clamp(surfacePt.X, 0, this.SourceSize.Width - 1);
                }
            }
        }

        public int[] Dst2SrcLookupY
        {
            get
            {
                lock (this.SyncRoot)
                {
                    CreateD2SLookupY();
                }

                return this.d2SLookupY;
            }
        }

        private int[] d2SLookupY; // maps from destination->source coordinates
        private void CreateD2SLookupY()
        {
            if (this.d2SLookupY == null || this.d2SLookupY.Length != this.DestinationSize.Height + 1)
            {
                this.d2SLookupY = new int[this.DestinationSize.Height + 1];

                for (int y = 0; y < d2SLookupY.Length; ++y)
                {
                    Point pt = new Point(0, y);
                    Point surfacePt = this.DestinationToSource(pt);

                    // Sometimes the scale factor is slightly different on one axis than
                    // on another, simply due to accuracy. So we have to clamp this value to
                    // be within bounds.
                    d2SLookupY[y] = Utility.Clamp(surfacePt.Y, 0, this.SourceSize.Height - 1);
                }
            }
        }

        public int[] Src2DstLookupX
        {
            get
            {
                lock (this.SyncRoot)
                {
                    CreateS2DLookupX();
                }

                return this.s2DLookupX;
            }
        }

        private int[] s2DLookupX; // maps from source->destination coordinates
        private void CreateS2DLookupX()
        {
            if (this.s2DLookupX == null || this.s2DLookupX.Length != this.SourceSize.Width + 1)
            {
                this.s2DLookupX = new int[this.SourceSize.Width + 1];

                for (int x = 0; x < s2DLookupX.Length; ++x)
                {
                    Point pt = new Point(x, 0);
                    Point clientPt = this.SourceToDestination(pt);

                    // Sometimes the scale factor is slightly different on one axis than
                    // on another, simply due to accuracy. So we have to clamp this value to
                    // be within bounds.
                    s2DLookupX[x] = Utility.Clamp(clientPt.X, 0, this.DestinationSize.Width - 1);
                }
            }
        }

        public int[] Src2DstLookupY
        {
            get
            {
                lock (this.SyncRoot)
                {
                    CreateS2DLookupY();
                }

                return this.s2DLookupY;
            }
        }

        private int[] s2DLookupY; // maps from source->destination coordinates
        private void CreateS2DLookupY()
        {
            if (this.s2DLookupY == null || this.s2DLookupY.Length != this.SourceSize.Height + 1)
            {
                this.s2DLookupY = new int[this.SourceSize.Height + 1];

                for (int y = 0; y < s2DLookupY.Length; ++y)
                {
                    Point pt = new Point(0, y);
                    Point clientPt = this.SourceToDestination(pt);

                    // Sometimes the scale factor is slightly different on one axis than
                    // on another, simply due to accuracy. So we have to clamp this value to
                    // be within bounds.
                    s2DLookupY[y] = Utility.Clamp(clientPt.Y, 0, this.DestinationSize.Height - 1);
                }
            }
        }

        public void InvalidateLookups()
        {
            this.s2DLookupX = null;
            this.s2DLookupY = null;
            this.d2SLookupX = null;
            this.d2SLookupY = null;
        }

        public void Render(Surface dst, Point offset)
        {
            foreach (SurfaceBoxRenderer sbr in this.list)
            {
                if (sbr.Visible)
                {
                    sbr.Render(dst, offset);
                }
            }

            foreach (SurfaceBoxRenderer sbr in this.topList)
            {
                if (sbr.Visible)
                {
                    sbr.Render(dst, offset);
                }
            }
        }

        public event InvalidateEventHandler Invalidated;

        public void Invalidate(Rectangle rect)
        {
            if (Invalidated != null)
            {
                Invalidated(this, new InvalidateEventArgs(rect));
            }
        }

        public void Invalidate()
        {
            Invalidate(SurfaceBoxRenderer.MaxBounds);
        }

        public SurfaceBoxRendererList(Size sourceSize, Size destinationSize)
        {
            this.list = new SurfaceBoxRenderer[0];
            this.topList = new SurfaceBoxRenderer[0];
            this.sourceSize = sourceSize;
            this.destinationSize = destinationSize;
        }
    }
}
