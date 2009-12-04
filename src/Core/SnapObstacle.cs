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
    public abstract class SnapObstacle
    {
        public const int DefaultSnapProximity = 15;
        public const int DefaultSnapDistance = 3;

        private string name;
        protected Rectangle previousBounds; // for BoundsChanged event
        protected Rectangle bounds;
        private SnapRegion snapRegion;
        private bool stickyEdges;
        private int snapProximity;
        private int snapDistance;
        private bool enabled;
        private bool enableSave;

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        /// <summary>
        /// Gets the bounds of this snap obstacle, defined in coordinates relative to its container.
        /// </summary>
        public Rectangle Bounds
        {
            get
            {
                return this.bounds;
            }
        }

        protected virtual void OnBoundsChangeRequested(Rectangle newBounds, ref bool handled)
        {
        }

        public bool RequestBoundsChange(Rectangle newBounds)
        {
            bool handled = false;
            OnBoundsChangeRequested(newBounds, ref handled);
            return handled;
        }

        public SnapRegion SnapRegion
        {
            get
            {
                return this.snapRegion;
            }
        }

        /// <summary>
        /// Gets whether or not this obstacle has "sticky" edges.
        /// </summary>
        /// <remarks>
        /// If an obstacle has sticky edges, than any obstacle that is snapped on 
        /// to it will move with this obstacle.
        /// </remarks>
        public bool StickyEdges
        {
            get
            {
                return this.stickyEdges;
            }
        }

        /// <summary>
        /// Gets how close another obstacle must be to snap to this one, in pixels
        /// </summary>
        public int SnapProximity
        {
            get
            {
                return this.snapProximity;
            }
        }

        /// <summary>
        /// Gets how close another obstacle will be parked when it snaps to this one, in pixels.
        /// </summary>
        public int SnapDistance
        {
            get
            {
                return this.snapDistance;
            }
        }

        public bool Enabled
        {
            get
            {
                return this.enabled;
            }

            set
            {
                this.enabled = value;
            }
        }

        public bool EnableSave
        {
            get
            {
                return this.enableSave;
            }

            set
            {
                this.enableSave = value;
            }
        }

        /// <summary>
        /// Raised before the Bounds is changed.
        /// </summary>
        /// <remarks>
        /// The Data property of the event args is the value that Bounds is being set to.
        /// </remarks>
        public event EventHandler<EventArgs<Rectangle>> BoundsChanging;
        protected virtual void OnBoundsChanging()
        {
            if (BoundsChanging != null)
            {
                BoundsChanging(this, new EventArgs<Rectangle>(this.Bounds));
            }
        }

        /// <summary>
        /// Raised after the Bounds is changed.
        /// </summary>
        /// <remarks>
        /// The Data property of the event args is the value that Bounds was just changed from.
        /// </remarks>
        public event EventHandler<EventArgs<Rectangle>> BoundsChanged;
        protected virtual void OnBoundsChanged()
        {
            if (BoundsChanged != null)
            {
                BoundsChanged(this, new EventArgs<Rectangle>(this.previousBounds));
            }
        }

        internal SnapObstacle(string name, Rectangle bounds, SnapRegion snapRegion, bool stickyEdges)
            : this(name, bounds, snapRegion, stickyEdges, DefaultSnapProximity, DefaultSnapDistance)
        {
        }

        internal SnapObstacle(string name, Rectangle bounds, SnapRegion snapRegion, bool stickyEdges, int snapProximity, int snapDistance)
        {
            this.name = name;
            this.bounds = bounds;
            this.previousBounds = bounds;
            this.snapRegion = snapRegion;
            this.stickyEdges = stickyEdges;
            this.snapProximity = snapProximity;
            this.snapDistance = snapDistance;
            this.enabled = true;
            this.enableSave = true;
        }
    }
}
