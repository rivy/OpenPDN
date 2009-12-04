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

namespace PaintDotNet
{
    public sealed class SnapObstacleController
        : SnapObstacle
    {
        /// <summary>
        /// Used for the obstacle to report changes in the obstacles size and/or location.
        /// </summary>
        public void SetBounds(Rectangle bounds)
        {
            if (this.bounds != bounds)
            {
                OnBoundsChanging();
                this.previousBounds = this.bounds;
                this.bounds = bounds;
                OnBoundsChanged();
            }
        }

        /// <summary>
        /// Raised when the SnapManager is requesting that the obstacle move and/or resize itself.
        /// Usually this happens in response to another snap container with "sticky edges" changing
        /// its boundary.
        /// </summary>
        public event HandledEventHandler<Rectangle> BoundsChangeRequested;

        protected override void OnBoundsChangeRequested(Rectangle newBounds, ref bool handled)
        {
            if (BoundsChangeRequested != null)
            {
                HandledEventArgs<Rectangle> e = new HandledEventArgs<Rectangle>(handled, newBounds);
                BoundsChangeRequested(this, e);
                handled = e.Handled;
            }

            base.OnBoundsChangeRequested(newBounds, ref handled);
        }

        public SnapObstacleController(string name, Rectangle bounds, SnapRegion snapRegion, bool stickyEdges)
            : base(name, bounds, snapRegion, stickyEdges)
        {
        }

        public SnapObstacleController(string name, Rectangle bounds, SnapRegion snapRegion, bool stickyEdges, int snapProximity, int snapDistance)
            : base(name, bounds, snapRegion, stickyEdges, snapProximity, snapDistance)
        {
        }
    }
}
