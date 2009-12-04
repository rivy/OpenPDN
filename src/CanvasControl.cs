/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace PaintDotNet
{
    internal abstract class CanvasControl
        : SurfaceBoxGraphicsRenderer
    {
        private PointF location;
        private SizeF size;
        private Cursor cursor;

        public event EventHandler CursorChanged;
        protected virtual void OnCursorChanged()
        {
            if (CursorChanged != null)
            {
                CursorChanged(this, EventArgs.Empty);
            }
        }

        public Cursor Cursor
        {
            get
            {
                return this.cursor;
            }

            set
            {
                if (this.cursor != value)
                {
                    this.cursor = value;
                    OnCursorChanged();
                }
            }
        }

        public event EventHandler LocationChanging;
        protected virtual void OnLocationChanging()
        {
            if (LocationChanging != null)
            {
                LocationChanging(this, EventArgs.Empty);
            }
        }

        public event EventHandler LocationChanged;
        protected virtual void OnLocationChanged()
        {
            if (LocationChanged != null)
            {
                LocationChanged(this, EventArgs.Empty);
            }
        }

        public PointF Location
        {
            get
            {
                return this.location;
            }

            set
            {
                if (this.location != value)
                {
                    OnLocationChanging();
                    this.location = value;
                    OnLocationChanged();
                }
            }
        }

        public event EventHandler SizeChanging;
        protected virtual void OnSizeChanging()
        {
            if (SizeChanging != null)
            {
                SizeChanging(this, EventArgs.Empty);
            }
        }

        public event EventHandler SizeChanged;
        protected virtual void OnSizeChanged()
        {
            if (SizeChanged != null)
            {
                SizeChanged(this, EventArgs.Empty);
            }
        }

        public SizeF Size
        {
            get
            {
                return this.size;
            }

            set
            {
                if (this.size != value)
                {
                    OnSizeChanging();
                    this.size = value;
                    OnSizeChanged();
                }
            }
        }

        public float Width
        {
            get
            {
                return Size.Width;
            }

            set
            {
                Size = new SizeF(value, Size.Height);
            }
        }

        public float Height
        {
            get
            {
                return Size.Height;
            }

            set
            {
                Size = new SizeF(Size.Width, value);
            }
        }

        public RectangleF Bounds
        {
            get
            {
                return new RectangleF(this.location, this.size);
            }

            set
            {
                Location = value.Location;
                Size = value.Size;
            }
        }

        public PointF CanvasPointToControlPoint(PointF canvasPtF)
        {
            return new PointF(canvasPtF.X - this.location.X, canvasPtF.Y - this.location.Y);
        }

        public PointF ControlPointToCanvasPoint(PointF controlPtF)
        {
            return new PointF(controlPtF.X + this.location.X, controlPtF.Y + this.location.Y);
        }

        public RectangleF CanvasRectToControlRect(RectangleF canvasRectF)
        {
            return new RectangleF(CanvasPointToControlPoint(canvasRectF.Location), canvasRectF.Size);
        }

        public RectangleF ControlRectToCanvasRect(RectangleF controlRectF)
        {
            return new RectangleF(ControlPointToCanvasPoint(controlRectF.Location), controlRectF.Size);
        }

        public void PerformMouseEnter()
        {
            MouseEnter();
        }

        private void MouseEnter()
        {
            OnMouseEnter();
        }

        protected virtual void OnMouseEnter()
        {
        }

        public void PerformMouseDown(MouseEventArgs e)
        {
            MouseDown(e);
        }

        private void MouseDown(MouseEventArgs e)
        {
            MouseDown(e);
        }

        protected virtual void OnMouseDown(MouseEventArgs e)
        {
        }

        public void PerformMouseUp(MouseEventArgs e)
        {
            MouseUp(e);
        }

        private void MouseUp(MouseEventArgs e)
        {
            OnMouseUp(e);
        }

        protected virtual void OnMouseUp(MouseEventArgs e)
        {
        }

        public void PerformMouseLeave()
        {
            MouseLeave();
        }

        private void MouseLeave()
        {
            OnMouseLeave();
        }

        protected virtual void OnMouseLeave()
        {
        }

        public void PerformPulse()
        {
            Pulse();
        }

        private void Pulse()
        {
            OnPulse();
        }

        protected virtual void OnPulse()
        {
        }

        public override sealed void RenderToGraphics(Graphics g, Point offset)
        {
            OnRender(g, offset);   
        }

        protected virtual void OnRender(Graphics g, Point offset)
        {
        }

        protected CanvasControl(SurfaceBoxRendererList ownerList)
            : base(ownerList)
        {
        }
    }
}
