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
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// This class contains information about the pointer's position,
    /// buttons, wheel rotation, and pressure, if applicable.
    /// </summary>
    public sealed class StylusEventArgs 
        : MouseEventArgs
    {
        private PointF position;
        public float Fx 
        {
            get 
            {
                return position.X;
            }
        }

        public float Fy 
        {
            get 
            {
                return position.Y;
            }
        }

        private float pressure;
        public float Pressure 
        {
            get
            {
                return pressure;
            }
        }

        /// <summary>
        /// Constructs a new StylusEventArgs object
        /// </summary>
        /// <param name="button">Which button was pressed</param>
        /// <param name="clicks">The number of times the button was pressed</param>
        /// <param name="x">The horizontal position of the pointer</param>
        /// <param name="y">The vertical position of the pointer</param>
        /// <param name="delta">The number of detents the wheel has rotated, signed</param>
        public StylusEventArgs(MouseEventArgs e)
            : base(e.Button, e.Clicks, e.X, e.Y, e.Delta)
        {
            this.position = new PointF(e.X, e.Y);
            this.pressure = 1.0f;
        }

        /// <summary>
        /// Constructs a new StylusEventArgs object
        /// </summary>
        /// <param name="button">Which button was pressed</param>
        /// <param name="clicks">The number of times the button was pressed</param>
        /// <param name="x">The horizontal position of the pointer</param>
        /// <param name="y">The vertical position of the pointer</param>
        /// <param name="delta">The number of detents the wheel has rotated, signed</param>
        public StylusEventArgs(MouseButtons button, int clicks, float fx, float fy, int delta)
            : this(button, clicks, fx, fy, delta, 1.0f)
        {
        }

        /// <summary>
        /// Constructs a new StylusEventArgs object
        /// </summary>
        /// <param name="button">Which button was pressed</param>
        /// <param name="clicks">The number of times the button was pressed</param>
        /// <param name="x">The horizontal position of the pointer</param>
        /// <param name="y">The vertical position of the pointer</param>
        /// <param name="delta">The number of detents the wheel has rotated, signed</param>
        /// <param name="pressure">The force applied with the pointer, as a fraction of the maximum</param>
        public StylusEventArgs(MouseButtons button, int clicks, float fx, float fy, int delta, float pressure)
            : base(button, clicks, (int)Math.Round(fx), (int)Math.Round(fy), delta)
        {
            this.position = new PointF(fx, fy);
            this.pressure = pressure;
        }
    }
}
