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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace PaintDotNet
{
    public sealed class SnapManager
    {
        private Dictionary<SnapObstacle, SnapDescription> obstacles =
            new Dictionary<SnapObstacle, SnapDescription>();

        private const string isSnappedValueName = "IsSnapped";
        private const string leftValueName = "Left";
        private const string topValueName = "Top";
        private const string widthValueName = "Width";
        private const string heightValueName = "Height";
        private const string nullName = "";

        private const string snappedToValueName = "SnappedTo";
        private const string horizontalEdgeValueName = "HorizontalEdge";
        private const string verticalEdgeValueName = "VerticalEdge";
        private const string xOffsetValueName = "XOffset";
        private const string yOffsetValueName = "YOffset";

        private void SaveSnapObstacleData(ISimpleCollection<string, string> saveTo, SnapObstacle so)
        {
            string prefix = so.Name + ".";
            SnapDescription sd = this.obstacles[so];

            bool isSnappedValue = (sd != null);            
            saveTo.Set(prefix + isSnappedValueName, isSnappedValue.ToString(CultureInfo.InvariantCulture));

            if (isSnappedValue)
            {
                saveTo.Set(prefix + snappedToValueName, sd.SnappedTo.Name);
                saveTo.Set(prefix + horizontalEdgeValueName, sd.HorizontalEdge.ToString());
                saveTo.Set(prefix + verticalEdgeValueName, sd.VerticalEdge.ToString());
                saveTo.Set(prefix + xOffsetValueName, sd.XOffset.ToString(CultureInfo.InvariantCulture));
                saveTo.Set(prefix + yOffsetValueName, sd.YOffset.ToString(CultureInfo.InvariantCulture));
            }

            saveTo.Set(prefix + leftValueName, so.Bounds.Left.ToString(CultureInfo.InvariantCulture));
            saveTo.Set(prefix + topValueName, so.Bounds.Top.ToString(CultureInfo.InvariantCulture));
            saveTo.Set(prefix + widthValueName, so.Bounds.Width.ToString(CultureInfo.InvariantCulture));
            saveTo.Set(prefix + heightValueName, so.Bounds.Height.ToString(CultureInfo.InvariantCulture));
        }

        private void LoadSnapObstacleData(ISimpleCollection<string, string> loadFrom, SnapObstacle so)
        {
            string prefix = so.Name + ".";
            SnapDescription sd;

            string isSnappedString = loadFrom.Get(prefix + isSnappedValueName);
            bool isSnapped = bool.Parse(isSnappedString);

            if (isSnapped)
            {
                string snappedToString = loadFrom.Get(prefix + snappedToValueName);
                SnapObstacle snappedTo = FindObstacle(snappedToString);

                string horizontalEdgeString = loadFrom.Get(prefix + horizontalEdgeValueName);
                HorizontalSnapEdge horizontalEdge = (HorizontalSnapEdge)Enum.Parse(typeof(HorizontalSnapEdge), horizontalEdgeString, true);

                string verticalEdgeString = loadFrom.Get(prefix + verticalEdgeValueName);
                VerticalSnapEdge verticalEdge = (VerticalSnapEdge)Enum.Parse(typeof(VerticalSnapEdge), verticalEdgeString, true);

                string xOffsetString = loadFrom.Get(prefix + xOffsetValueName);
                int xOffset = int.Parse(xOffsetString, CultureInfo.InvariantCulture);

                string yOffsetString = loadFrom.Get(prefix + yOffsetValueName);
                int yOffset = int.Parse(yOffsetString, CultureInfo.InvariantCulture);

                sd = new SnapDescription(snappedTo, horizontalEdge, verticalEdge, xOffset, yOffset);
            }
            else
            {
                sd = null;
            }

            this.obstacles[so] = sd;

            string leftString = loadFrom.Get(prefix + leftValueName);
            int left = int.Parse(leftString, CultureInfo.InvariantCulture);

            string topString = loadFrom.Get(prefix + topValueName);
            int top = int.Parse(topString, CultureInfo.InvariantCulture);

            string widthString = loadFrom.Get(prefix + widthValueName);
            int width = int.Parse(widthString, CultureInfo.InvariantCulture);

            string heightString = loadFrom.Get(prefix + heightValueName);
            int height = int.Parse(heightString, CultureInfo.InvariantCulture);

            Rectangle newBounds = new Rectangle(left, top, width, height);
            so.RequestBoundsChange(newBounds);

            if (sd != null)
            {
                ParkObstacle(so, sd);
            }
        }

        // Requires that all SnapObstacles are already placed in this.obstacles
        public void Save(ISimpleCollection<string, string> saveTo)
        {
            foreach (SnapObstacle obstacle in this.obstacles.Keys)
            {
                // TODO: how do we 'erase' something that has this property set to false, for full generality?
                if (obstacle.EnableSave)
                {
                    SaveSnapObstacleData(saveTo, obstacle);
                }
            }
        }

        public void Load(ISimpleCollection<string, string> loadFrom)
        {
            SnapObstacle[] newObstacles = new SnapObstacle[this.obstacles.Count];
            this.obstacles.Keys.CopyTo(newObstacles, 0);

            foreach (SnapObstacle obstacle in newObstacles)
            {
                if (obstacle.EnableSave)
                {
                    LoadSnapObstacleData(loadFrom, obstacle);
                }
            }
        }

        public void ParkObstacle(ISnapObstacleHost obstacle, ISnapObstacleHost snappedTo, HorizontalSnapEdge hEdge, VerticalSnapEdge vEdge)
        {
            ParkObstacle(obstacle.SnapObstacle, snappedTo.SnapObstacle, hEdge, vEdge);
        }

        public void ParkObstacle(SnapObstacle obstacle, SnapObstacle snappedTo, HorizontalSnapEdge hEdge, VerticalSnapEdge vEdge)
        {
            SnapDescription sd = new SnapDescription(snappedTo, hEdge, vEdge, obstacle.SnapDistance, obstacle.SnapDistance);
            this.obstacles[obstacle] = sd;
            ParkObstacle(obstacle, sd);
        }

        public void ReparkObstacle(ISnapObstacleHost obstacle)
        {
            ReparkObstacle(obstacle.SnapObstacle);
        }

        public void ReparkObstacle(SnapObstacle obstacle)
        {
            if (this.obstacles.ContainsKey(obstacle))
            {
                SnapDescription sd = this.obstacles[obstacle];

                if (sd != null)
                {
                    ParkObstacle(obstacle, sd);
                }
            }
        }

        public void AddSnapObstacle(ISnapObstacleHost snapObstacleHost)
        {
            AddSnapObstacle(snapObstacleHost.SnapObstacle);
        }

        public void AddSnapObstacle(SnapObstacle snapObstacle)
        {
            if (!this.obstacles.ContainsKey(snapObstacle))
            {
                this.obstacles.Add(snapObstacle, null);

                if (snapObstacle.StickyEdges)
                {
                    snapObstacle.BoundsChanging += SnapObstacle_BoundsChanging;
                    snapObstacle.BoundsChanged += SnapObstacle_BoundsChanged;
                }
            }
        }

        private void SnapObstacle_BoundsChanging(object sender, EventArgs<Rectangle> e)
        {
        }

        private void SnapObstacle_BoundsChanged(object sender, EventArgs<Rectangle> e)
        {
            SnapObstacle senderSO = (SnapObstacle)sender;
            Rectangle fromRect = e.Data;
            Rectangle toRect = senderSO.Bounds;
            UpdateDependentObstacles(senderSO, fromRect, toRect);
        }

        private void UpdateDependentObstacles(SnapObstacle senderSO, Rectangle fromRect, Rectangle toRect)
        {
            int leftDelta = toRect.Left - fromRect.Left;
            int topDelta = toRect.Top - fromRect.Top;
            int rightDelta = toRect.Right - fromRect.Right;
            int bottomDelta = toRect.Bottom - fromRect.Bottom;

            foreach (SnapObstacle obstacle in this.obstacles.Keys)
            {
                if (!object.ReferenceEquals(senderSO, obstacle))                    
                {
                    SnapDescription sd = this.obstacles[obstacle];

                    if (sd != null && object.ReferenceEquals(sd.SnappedTo, senderSO))
                    {
                        int deltaX;

                        if (sd.VerticalEdge == VerticalSnapEdge.Right)
                        {
                            deltaX = rightDelta;
                        }
                        else
                        {
                            deltaX = leftDelta;
                        }

                        int deltaY;

                        if (sd.HorizontalEdge == HorizontalSnapEdge.Bottom)
                        {
                            deltaY = bottomDelta;
                        }
                        else
                        {
                            deltaY = topDelta;
                        }

                        Rectangle oldBounds = obstacle.Bounds;
                        Point newLocation1 = new Point(oldBounds.Left + deltaX, oldBounds.Top + deltaY);
                        Point newLocation2 = AdjustNewLocation(obstacle, newLocation1, sd);
                        Rectangle newBounds = new Rectangle(newLocation2, oldBounds.Size);

                        obstacle.RequestBoundsChange(newBounds);

                        // Recursively update anything snapped to this obstacle
                        UpdateDependentObstacles(obstacle, oldBounds, newBounds);
                    }
                }
            }
        }

        public void RemoveSnapObstacle(ISnapObstacleHost snapObstacleHost)
        {
            RemoveSnapObstacle(snapObstacleHost.SnapObstacle);
        }

        public void RemoveSnapObstacle(SnapObstacle snapObstacle)
        {
            if (this.obstacles.ContainsKey(snapObstacle))
            {
                this.obstacles.Remove(snapObstacle);

                if (snapObstacle.StickyEdges)
                {
                    snapObstacle.BoundsChanging -= SnapObstacle_BoundsChanging;
                    snapObstacle.BoundsChanged -= SnapObstacle_BoundsChanged;
                }
            }
        }

        public bool ContainsSnapObstacle(ISnapObstacleHost snapObstacleHost)
        {
            return ContainsSnapObstacle(snapObstacleHost.SnapObstacle);
        }

        public bool ContainsSnapObstacle(SnapObstacle snapObstacle)
        {
            return this.obstacles.ContainsKey(snapObstacle);
        }

        private static bool AreEdgesClose(int l1, int r1, int l2, int r2)
        {
            if (r1 < l2)
            {
                return false;
            }
            else if (r2 < l1)
            {
                return false;
            }
            else if (l1 <= l2 && l2 <= r1 && r1 <= r2)
            {
                return true;
            }
            else if (l2 <= l1 && l1 <= r2 && r2 <= r1)
            {
                return true;
            }
            else if (l1 <= l2 && r2 <= r1)
            {
                return true;
            }
            else if (l2 <= l1 && l1 <= r2)
            {
                return true;
            }

            throw new InvalidOperationException();
        }

        private SnapDescription DetermineNewSnapDescription(
            SnapObstacle avoider,
            Point newLocation,
            SnapObstacle avoidee,
            SnapDescription currentSnapDescription)
        {
            int ourSnapProximity;

            if (currentSnapDescription != null &&
                (currentSnapDescription.HorizontalEdge != HorizontalSnapEdge.Neither ||
                 currentSnapDescription.VerticalEdge != VerticalSnapEdge.Neither))
            {
                // the avoider is already snapped to the avoidee -- make it more difficult to un-snap
                ourSnapProximity = avoidee.SnapProximity * 2;
            }
            else
            {
                ourSnapProximity = avoidee.SnapProximity;
            }

            Rectangle avoiderRect = avoider.Bounds;
            avoiderRect.Location = newLocation;
            Rectangle avoideeRect = avoidee.Bounds;

            // Are the vertical edges close enough for snapping?
            bool vertProximity = AreEdgesClose(avoiderRect.Top, avoiderRect.Bottom, avoideeRect.Top, avoideeRect.Bottom);

            // Are the horizontal edges close enough for snapping?
            bool horizProximity = AreEdgesClose(avoiderRect.Left, avoiderRect.Right, avoideeRect.Left, avoideeRect.Right);

            // Compute distances from pertinent edges
            // (e.g. if SnapRegion.Interior, figure out distance from avoider's right edge to avoidee's right edge,
            //       if SnapRegion.Exterior, figure out distance from avoider's right edge to avoidee's left edge)
            int leftDistance;
            int rightDistance;
            int topDistance;
            int bottomDistance;

            switch (avoidee.SnapRegion)
            {
                case SnapRegion.Interior:
                    leftDistance = Math.Abs(avoiderRect.Left - avoideeRect.Left);
                    rightDistance = Math.Abs(avoiderRect.Right - avoideeRect.Right);
                    topDistance = Math.Abs(avoiderRect.Top - avoideeRect.Top);
                    bottomDistance = Math.Abs(avoiderRect.Bottom - avoideeRect.Bottom);
                    break;

                case SnapRegion.Exterior:
                    leftDistance = Math.Abs(avoiderRect.Left - avoideeRect.Right);
                    rightDistance = Math.Abs(avoiderRect.Right - avoideeRect.Left);
                    topDistance = Math.Abs(avoiderRect.Top - avoideeRect.Bottom);
                    bottomDistance = Math.Abs(avoiderRect.Bottom - avoideeRect.Top);
                    break;

                default:
                    throw new InvalidEnumArgumentException("avoidee.SnapRegion");
            }

            bool leftClose = (leftDistance < ourSnapProximity);
            bool rightClose = (rightDistance < ourSnapProximity);
            bool topClose = (topDistance < ourSnapProximity);
            bool bottomClose = (bottomDistance < ourSnapProximity);

            VerticalSnapEdge vEdge = VerticalSnapEdge.Neither;

            if (vertProximity)
            {
                if ((leftClose && avoidee.SnapRegion == SnapRegion.Exterior) ||
                    (rightClose && avoidee.SnapRegion == SnapRegion.Interior))
                {
                    vEdge = VerticalSnapEdge.Right;
                }
                else if ((rightClose && avoidee.SnapRegion == SnapRegion.Exterior) ||
                         (leftClose && avoidee.SnapRegion == SnapRegion.Interior))
                {
                    vEdge = VerticalSnapEdge.Left;
                }
            }

            HorizontalSnapEdge hEdge = HorizontalSnapEdge.Neither;

            if (horizProximity)
            {
                if ((topClose && avoidee.SnapRegion == SnapRegion.Exterior) ||
                    (bottomClose && avoidee.SnapRegion == SnapRegion.Interior))
                {
                    hEdge = HorizontalSnapEdge.Bottom;
                }
                else if ((bottomClose && avoidee.SnapRegion == SnapRegion.Exterior) ||
                         (topClose && avoidee.SnapRegion == SnapRegion.Interior))
                {
                    hEdge = HorizontalSnapEdge.Top;
                }
            }

            SnapDescription sd;

            if (hEdge != HorizontalSnapEdge.Neither || vEdge != VerticalSnapEdge.Neither)
            {
                int xOffset = avoider.SnapDistance;
                int yOffset = avoider.SnapDistance;

                if (hEdge == HorizontalSnapEdge.Neither)
                {
                    if (avoidee.SnapRegion == SnapRegion.Interior)
                    {
                        yOffset = avoiderRect.Top - avoideeRect.Top;
                        hEdge = HorizontalSnapEdge.Top;
                    }
                }

                if (vEdge == VerticalSnapEdge.Neither)
                {
                    if (avoidee.SnapRegion == SnapRegion.Interior)
                    {
                        xOffset = avoiderRect.Left - avoideeRect.Left;
                        vEdge = VerticalSnapEdge.Left;
                    }
                }

                sd = new SnapDescription(avoidee, hEdge, vEdge, xOffset, yOffset);
            }
            else
            {
                sd = null;
            }

            return sd;
        }

        private static void ParkObstacle(SnapObstacle avoider, SnapDescription snapDescription)
        {
            Point newLocation = avoider.Bounds.Location;
            Point adjustedLocation = AdjustNewLocation(avoider, newLocation, snapDescription);
            Rectangle newBounds = new Rectangle(adjustedLocation, avoider.Bounds.Size);
            avoider.RequestBoundsChange(newBounds);
        }

        private static Point AdjustNewLocation(SnapObstacle obstacle, Point newLocation, SnapDescription snapDescription)
        {
            if (snapDescription == null ||
                (snapDescription.HorizontalEdge == HorizontalSnapEdge.Neither &&
                 snapDescription.VerticalEdge == VerticalSnapEdge.Neither))
            {
                return obstacle.Bounds.Location;
            }

            Rectangle obstacleRect = new Rectangle(newLocation, obstacle.Bounds.Size);
            Rectangle snappedToRect = snapDescription.SnappedTo.Bounds;
            HorizontalSnapEdge hEdge = snapDescription.HorizontalEdge;
            VerticalSnapEdge vEdge = snapDescription.VerticalEdge;
            SnapRegion region = snapDescription.SnappedTo.SnapRegion;

            int deltaY = 0;

            if (hEdge == HorizontalSnapEdge.Top && region == SnapRegion.Exterior)
            {
                int newBottomEdge = snappedToRect.Top - snapDescription.YOffset;
                deltaY = obstacleRect.Bottom - newBottomEdge;
            }
            else if (hEdge == HorizontalSnapEdge.Bottom && region == SnapRegion.Exterior)
            {
                int newTopEdge = snappedToRect.Bottom + snapDescription.YOffset;
                deltaY = obstacleRect.Top - newTopEdge;
            }
            else if (hEdge == HorizontalSnapEdge.Top && region == SnapRegion.Interior)
            {
                int newTopEdge = Math.Min(snappedToRect.Bottom, snappedToRect.Top + snapDescription.YOffset);
                deltaY = obstacleRect.Top - newTopEdge;
            }
            else if (hEdge == HorizontalSnapEdge.Bottom && region == SnapRegion.Interior)
            {
                int newBottomEdge = Math.Max(snappedToRect.Top, snappedToRect.Bottom - snapDescription.YOffset);
                deltaY = obstacleRect.Bottom - newBottomEdge;
            }

            int deltaX = 0;

            if (vEdge == VerticalSnapEdge.Left && region == SnapRegion.Exterior)
            {
                int newRightEdge = snappedToRect.Left - snapDescription.XOffset;
                deltaX = obstacleRect.Right - newRightEdge;
            }
            else if (vEdge == VerticalSnapEdge.Right && region == SnapRegion.Exterior)
            {
                int newLeftEdge = snappedToRect.Right + snapDescription.XOffset;
                deltaX = obstacleRect.Left - newLeftEdge;
            }
            else if (vEdge == VerticalSnapEdge.Left && region == SnapRegion.Interior)
            {
                int newLeftEdge = Math.Min(snappedToRect.Right, snappedToRect.Left + snapDescription.XOffset);
                deltaX = obstacleRect.Left - newLeftEdge;
            }
            else if (vEdge == VerticalSnapEdge.Right && region == SnapRegion.Interior)
            {
                int newRightEdge = Math.Max(snappedToRect.Left, snappedToRect.Right - snapDescription.XOffset);
                deltaX = obstacleRect.Right - newRightEdge;
            }

            Point adjustedLocation = new Point(obstacleRect.Left - deltaX, obstacleRect.Top - deltaY);
            return adjustedLocation;
        }

        /// <summary>
        /// Given an obstacle and its attempted destination, determines the correct landing
        /// spot for an obstacle.
        /// </summary>
        /// <param name="movingObstacle">The obstacle that is moving.</param>
        /// <param name="newLocation">The upper-left coordinate of the obstacle's original intended destination.</param>
        /// <returns>
        /// A Point that determines where the obstacle should be placed instead. If there are no adjustments
        /// required to the obstacle's desintation, then the return value will be equal to newLocation.
        /// </returns>
        /// <remarks>
        /// movingObstacle's SnapDescription will also be updated. The caller of this method is required
        /// to update the SnapObstacle with the new, adjusted location.
        /// </remarks>
        public Point AdjustObstacleDestination(SnapObstacle movingObstacle, Point newLocation)
        {
            Point adjusted1 = AdjustObstacleDestination(movingObstacle, newLocation, false);
            Point adjusted2 = AdjustObstacleDestination(movingObstacle, adjusted1, true);
            return adjusted2;
        }

        public Point AdjustObstacleDestination(SnapObstacle movingObstacle, Point newLocation, bool considerStickies)
        {
            Point adjustedLocation = newLocation;
            SnapDescription sd = this.obstacles[movingObstacle];
            SnapDescription newSD = null;

            foreach (SnapObstacle avoidee in this.obstacles.Keys)
            {
                if (avoidee.StickyEdges != considerStickies)
                {
                    continue;
                }

                if (avoidee.Enabled && !object.ReferenceEquals(avoidee, movingObstacle))
                {
                    SnapDescription newSD2 = DetermineNewSnapDescription(movingObstacle, adjustedLocation, avoidee, newSD);

                    if (newSD2 != null)
                    {
                        Point adjustedLocation2 = AdjustNewLocation(movingObstacle, adjustedLocation, newSD2);
                        newSD = newSD2;
                        adjustedLocation = adjustedLocation2;
                        Rectangle newBounds = new Rectangle(adjustedLocation, movingObstacle.Bounds.Size);
                    }
                }
            }

            if (sd == null || !sd.SnappedTo.StickyEdges || newSD == null || newSD.SnappedTo.StickyEdges)
            {
                this.obstacles[movingObstacle] = newSD;
            }

            return adjustedLocation;
        }
        
        public SnapObstacle FindObstacle(string name)
        {
            foreach (SnapObstacle so in this.obstacles.Keys)
            {
                if (string.Compare(so.Name, name, true) == 0)
                {
                    return so;
                }
            }

            return null;
        }

        public static SnapManager FindMySnapManager(Control me)
        {
            if (!(me is ISnapObstacleHost))
            {
                throw new ArgumentException("must be called with a Control that implements ISnapObstacleHost");
            }

            ISnapManagerHost ismh;

            ismh = me as ISnapManagerHost;

            if (ismh == null)
            {
                ismh = me.FindForm() as ISnapManagerHost;
            }

            SnapManager sm;
            if (ismh != null)
            {
                sm = ismh.SnapManager;
            }
            else
            {
                sm = null;
            }

            return sm;
        }

        public SnapManager()
        {
        }
    }
}
