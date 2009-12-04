/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Specialized;
using System.Drawing;
using System.Globalization;
using System.Text;

namespace PaintDotNet
{
    public sealed class SnapDescription
    {
        private SnapObstacle snappedTo;
        private HorizontalSnapEdge horizontalEdge;
        private VerticalSnapEdge verticalEdge;
        private int xOffset;
        private int yOffset;

        public SnapObstacle SnappedTo
        {
            get
            {
                return this.snappedTo;
            }
        }

        public HorizontalSnapEdge HorizontalEdge
        {
            get
            {
                return this.horizontalEdge;
            }

            set
            {
                this.horizontalEdge = value;
            }
        }

        public VerticalSnapEdge VerticalEdge
        {
            get
            {
                return this.verticalEdge;
            }

            set
            {
                this.verticalEdge = value;
            }
        }

        public int XOffset
        {
            get
            {
                return this.xOffset;
            }

            set
            {
                this.xOffset = value;
            }
        }

        public int YOffset
        {
            get
            {
                return this.yOffset;
            }

            set
            {
                this.yOffset = value;
            }
        }

        public SnapDescription(
            SnapObstacle snappedTo,
            HorizontalSnapEdge horizontalEdge,
            VerticalSnapEdge verticalEdge,
            int xOffset,
            int yOffset)
        {
            if (snappedTo == null)
            {
                throw new ArgumentNullException("snappedTo");
            }

            this.snappedTo = snappedTo;
            this.horizontalEdge = horizontalEdge;
            this.verticalEdge = verticalEdge;
            this.xOffset = xOffset;
            this.yOffset = yOffset;
        }
    }
}
