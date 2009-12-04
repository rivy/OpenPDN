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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace PaintDotNet
{
    public sealed class SwatchControl
        : Control
    {
        private List<ColorBgra> colors = new List<ColorBgra>();
        private const int defaultUnscaledSwatchSize = 12;
        private int unscaledSwatchSize = defaultUnscaledSwatchSize;
        private bool mouseDown = false;
        private int mouseDownIndex = -1;
        private bool blinkHighlight = false;
        private const int blinkInterval = 500;
        private System.Windows.Forms.Timer blinkHighlightTimer;

        [Browsable(false)]
        public bool BlinkHighlight
        {
            get
            {
                return this.blinkHighlight;
            }

            set
            {
                this.blinkHighlight = value;
                this.blinkHighlightTimer.Enabled = value;
                Invalidate();
            }
        }

        public event EventHandler ColorsChanged;
        private void OnColorsChanged()
        {
            if (ColorsChanged != null)
            {
                ColorsChanged(this, EventArgs.Empty);
            }
        }

        [Browsable(false)]
        public ColorBgra[] Colors
        {
            get
            {
                return this.colors.ToArray();
            }

            set
            {
                this.colors = new List<ColorBgra>(value);
                this.mouseDown = false;
                Invalidate();
                OnColorsChanged();
            }
        }

        [DefaultValue(defaultUnscaledSwatchSize)]
        [Browsable(true)]
        public int UnscaledSwatchSize
        {
            get
            {
                return this.unscaledSwatchSize;
            }

            set
            {
                this.unscaledSwatchSize = value;
                this.mouseDown = false;
                Invalidate();
            }
        }

        public event EventHandler<EventArgs<Pair<int, MouseButtons>>> ColorClicked;
        private void OnColorClicked(int index, MouseButtons buttons)
        {
            if (ColorClicked != null)
            {
                ColorClicked(this, new EventArgs<Pair<int, MouseButtons>>(Pair.Create(index, buttons)));
            }
        }

        public SwatchControl()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.blinkHighlightTimer = new Timer();
            this.blinkHighlightTimer.Tick += new EventHandler(BlinkHighlightTimer_Tick);
            this.blinkHighlightTimer.Enabled = false;
            this.blinkHighlightTimer.Interval = blinkInterval;
            this.DoubleBuffered = true;
            this.ResizeRedraw = true;
        }

        private void BlinkHighlightTimer_Tick(object sender, EventArgs e)
        {
            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.blinkHighlightTimer != null)
                {
                    this.blinkHighlightTimer.Dispose();
                    this.blinkHighlightTimer = null;
                }
            }

            base.Dispose(disposing);
        }

        private int MouseXYToColorIndex(int x, int y)
        {
            if (x < 0 || y < 0 || x >= ClientSize.Width || y >= ClientSize.Height)
            {
                return -1;
            }

            int scaledSwatchSize = UI.ScaleWidth(this.unscaledSwatchSize);
            int swatchColumns = this.ClientSize.Width / scaledSwatchSize;
            int row = y / scaledSwatchSize;
            int col = x / scaledSwatchSize;
            int index = col + (row * swatchColumns);

            // Make sure they aren't on the last item of a row that actually got clipped off
            if (col == swatchColumns)
            {
                index = -1;
            }

            return index;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            this.mouseDown = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            this.mouseDown = true;
            this.mouseDownIndex = MouseXYToColorIndex(e.X, e.Y);
            Invalidate();
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            int colorIndex = MouseXYToColorIndex(e.X, e.Y);

            if (colorIndex == this.mouseDownIndex && 
                colorIndex >= 0 && 
                colorIndex < this.colors.Count)
            {
                OnColorClicked(colorIndex, e.Button);
            }

            this.mouseDown = false;
            Invalidate();
            base.OnMouseUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            Invalidate();
            base.OnMouseMove(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.CompositingMode = CompositingMode.SourceOver;
            int scaledSwatchSize = UI.ScaleWidth(this.unscaledSwatchSize);
            int swatchColumns = this.ClientSize.Width / scaledSwatchSize;

            Point mousePt = Control.MousePosition;
            mousePt = PointToClient(mousePt);
            int activeIndex = MouseXYToColorIndex(mousePt.X, mousePt.Y);

            for (int i = 0; i < this.colors.Count; ++i)
            {
                ColorBgra c = this.colors[i];

                int swatchX = i % swatchColumns;
                int swatchY = i / swatchColumns;

                Rectangle swatchRect = new Rectangle(
                    swatchX * scaledSwatchSize, 
                    swatchY * scaledSwatchSize, 
                    scaledSwatchSize, 
                    scaledSwatchSize);

                PushButtonState state;

                if (this.mouseDown)
                {
                    if (i == this.mouseDownIndex)
                    {
                        state = PushButtonState.Pressed;
                    }
                    else
                    {
                        state = PushButtonState.Normal;
                    }
                }
                else if (i == activeIndex)
                {
                    state = PushButtonState.Hot;
                }
                else
                {
                    state = PushButtonState.Normal;
                }

                bool drawOutline;

                switch (state)
                {
                    case PushButtonState.Hot:
                        drawOutline = true;
                        break;

                    case PushButtonState.Pressed:
                        drawOutline = false;
                        break;

                    case PushButtonState.Default:
                    case PushButtonState.Disabled:
                    case PushButtonState.Normal:
                        drawOutline = false;
                        break;

                    default:
                        throw new InvalidEnumArgumentException();
                }

                Utility.DrawColorRectangle(e.Graphics, swatchRect, c.ToColor(), drawOutline);
            }

            if (this.blinkHighlight)
            {
                int period = (Math.Abs(Environment.TickCount) / blinkInterval) % 2;
                Color color;
                
                switch (period)
                {
                    case 0:
                        color = SystemColors.Window;
                        break;

                    case 1:
                        color = SystemColors.Highlight;
                        break;

                    default:
                        throw new InvalidOperationException();
                }

                using (Pen pen = new Pen(color))
                {
                    e.Graphics.DrawRectangle(pen, new Rectangle(0, 0, Width - 1, Height - 1));
                }
            }

            base.OnPaint(e);
        }
    }
}
