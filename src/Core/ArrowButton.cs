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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace PaintDotNet
{
    public sealed class ArrowButton
        : ButtonBase
    {
        private ArrowDirection arrowDirection = ArrowDirection.Right;
        private bool drawWithGradient = false;
        private bool reverseArrowColors = false;
        private float arrowOutlineWidth = 1.0f;
        private bool forcedPushed = false;
        private Surface backBufferSurface = null;
        private RenderArgs backBuffer = null;

        public bool ForcedPushedAppearance
        {
            get
            {
                return this.forcedPushed;
            }

            set
            {
                if (this.forcedPushed != value)
                {
                    this.forcedPushed = value;
                    Invalidate();
                }
            }
        }

        public bool ReverseArrowColors
        {
            get
            {
                return this.reverseArrowColors;
            }

            set
            {
                if (this.reverseArrowColors != value)
                {
                    this.reverseArrowColors = value;
                    Invalidate();
                }
            }
        }

        public float ArrowOutlineWidth
        {
            get
            {
                return this.arrowOutlineWidth;
            }

            set
            {
                if (this.arrowOutlineWidth != value)
                {
                    this.arrowOutlineWidth = value;
                    Invalidate();
                }
            }
        }

        public ArrowDirection ArrowDirection
        {
            get
            {
                return this.arrowDirection;
            }

            set
            {
                if (this.arrowDirection != value)
                {
                    this.arrowDirection = value;
                    Invalidate();
                }
            }
        }

        public bool DrawWithGradient
        {
            get
            {
                return this.drawWithGradient;
            }

            set
            {
                if (this.drawWithGradient != value)
                {
                    this.drawWithGradient = value;
                    Invalidate();
                }
            }
        }

        public ArrowButton()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.Transparent;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.backBuffer != null)
                {
                    this.backBuffer.Dispose();
                    this.backBuffer = null;
                }

                if (this.backBufferSurface != null)
                {
                    this.backBufferSurface.Dispose();
                    this.backBufferSurface = null;
                }
            }

            base.Dispose(disposing);
        }

        protected override void OnPaintButton(Graphics g, PushButtonState state, bool drawFocusCues, bool drawKeyboardCues)
        {
            PushButtonState newState;

            if (this.forcedPushed)
            {
                newState = PushButtonState.Pressed;
            }
            else
            {
                newState = state;
            }

            OnPaintButtonImpl(g, newState, drawFocusCues, drawKeyboardCues);
        }

        private void OnPaintButtonImpl(Graphics g, PushButtonState state, bool drawFocusCues, bool drawKeyboardCues)
        {
            Color backColor;
            Color outlineColor;
            Color arrowFillColor;
            Color arrowOutlineColor;

            switch (state)
            {
                case PushButtonState.Disabled:
                    backColor = Color.Transparent;
                    outlineColor = BackColor;
                    arrowFillColor = Color.Gray;
                    arrowOutlineColor = Color.Black;
                    break;

                case PushButtonState.Hot:
                    backColor = Color.FromArgb(64, SystemColors.HotTrack);
                    outlineColor = backColor;
                    arrowFillColor = Color.Blue;
                    arrowOutlineColor = Color.White;
                    break;

                case PushButtonState.Default:
                case PushButtonState.Normal:
                    backColor = Color.Transparent;
                    outlineColor = Color.Transparent;
                    arrowFillColor = Color.Black;
                    arrowOutlineColor = Color.White;
                    break;

                case PushButtonState.Pressed:
                    backColor = Color.FromArgb(192, SystemColors.Highlight);
                    outlineColor = Color.FromArgb(192, SystemColors.Highlight);
                    arrowFillColor = Color.Blue;
                    arrowOutlineColor = Color.White;
                    break;

                default:
                    throw new InvalidEnumArgumentException("buttonState");
            }

            // Draw parent background
            IPaintBackground asIpb = Parent as IPaintBackground;

            if (!this.drawWithGradient || asIpb == null)
            {
                if (asIpb != null)
                {
                    Rectangle screenRect = RectangleToScreen(ClientRectangle);
                    Rectangle parentRect = Parent.RectangleToClient(screenRect);

                    g.TranslateTransform(-Left, -Top, MatrixOrder.Append);
                    asIpb.PaintBackground(g, parentRect);
                    g.TranslateTransform(+Left, +Top, MatrixOrder.Append);
                }
                else
                {
                    using (SolidBrush backBrush = new SolidBrush(BackColor))
                    {
                        g.FillRectangle(backBrush, ClientRectangle);
                    }
                }
            }
            else
            {
                if (this.backBufferSurface != null &&
                    (this.backBufferSurface.Width != ClientSize.Width || this.backBufferSurface.Height != ClientSize.Height))
                {
                    this.backBuffer.Dispose();
                    this.backBuffer = null;

                    this.backBufferSurface.Dispose();
                    this.backBufferSurface = null;
                }

                if (this.backBufferSurface == null)
                {
                    this.backBufferSurface = new Surface(ClientSize.Width, ClientSize.Height);
                    this.backBuffer = new RenderArgs(this.backBufferSurface);
                }

                Rectangle screenRect = RectangleToScreen(ClientRectangle);
                Rectangle parentRect = Parent.RectangleToClient(screenRect);

                using (Graphics bg = Graphics.FromImage(this.backBuffer.Bitmap))
                {
                    bg.TranslateTransform(-Left, -Top, MatrixOrder.Append);
                    asIpb.PaintBackground(bg, parentRect);
                }

                BitmapData bitmapData = this.backBuffer.Bitmap.LockBits(
                    new Rectangle(0, 0, this.backBuffer.Bitmap.Width, this.backBuffer.Bitmap.Height), 
                    ImageLockMode.ReadWrite, 
                    PixelFormat.Format32bppArgb);

                int startAlpha;
                int finishAlpha;

                if (this.arrowDirection == ArrowDirection.Left || this.arrowDirection == ArrowDirection.Up)
                {
                    startAlpha = 255;
                    finishAlpha = 0;
                }
                else if (this.arrowDirection == ArrowDirection.Right || this.ArrowDirection == ArrowDirection.Down)
                {
                    startAlpha = 0;
                    finishAlpha = 255;
                }
                else
                {
                    throw new InvalidEnumArgumentException("this.arrowDirection");
                }

                unsafe
                {
                    if (this.arrowDirection == ArrowDirection.Left || this.arrowDirection == ArrowDirection.Right)
                    {
                        for (int x = 0; x < this.backBuffer.Bitmap.Width; ++x)
                        {
                            float lerp = (float)x / (float)(this.backBuffer.Bitmap.Width - 1);

                            if (this.arrowDirection == ArrowDirection.Left)
                            {
                                lerp = 1.0f - (float)Math.Cos(lerp * (Math.PI / 2.0));
                            }
                            else
                            {
                                lerp = (float)Math.Sin(lerp * (Math.PI / 2.0));
                            }

                            byte alpha = (byte)(startAlpha + ((int)(lerp * (finishAlpha - startAlpha))));
                            byte* pb = (byte*)bitmapData.Scan0.ToPointer() + (x * 4) + 3; // *4 because 4-bytes per pixel, +3 to get to alpha channel

                            for (int y = 0; y < this.backBuffer.Bitmap.Height; ++y)
                            {
                                *pb = alpha;
                                pb += bitmapData.Stride;
                            }
                        }
                    }
                    else if (this.arrowDirection == ArrowDirection.Up || this.arrowDirection == ArrowDirection.Down)
                    {
                        for (int y = 0; y < this.backBuffer.Bitmap.Height; ++y)
                        {
                            float lerp = (float)y / (float)(this.backBuffer.Bitmap.Height - 1);
                            lerp = 1.0f - (float)Math.Cos(lerp * (Math.PI / 2.0));

                            byte alpha = (byte)(startAlpha + ((int)(lerp * (finishAlpha - startAlpha))));
                            byte* pb = (byte*)bitmapData.Scan0.ToPointer() + (y * bitmapData.Stride) + 3; // *Stride for access to start of row, +3 to get to alpha channel

                            for (int x = 0; x < this.backBuffer.Bitmap.Width; ++x)
                            {
                                *pb = alpha;
                                pb += 4; // 4 for byte size of pixel
                            }
                        }
                    }
                }

                this.backBuffer.Bitmap.UnlockBits(bitmapData);
                bitmapData = null;

                g.DrawImage(this.backBuffer.Bitmap, new Point(0, 0));
            }

            using (SolidBrush fillBrush = new SolidBrush(backColor))
            {
                g.FillRectangle(fillBrush, ClientRectangle);
            }

            // Draw outline
            using (Pen outlinePen = new Pen(outlineColor))
            {
                g.DrawRectangle(outlinePen, new Rectangle(0, 0, ClientSize.Width - 1, ClientSize.Height - 1));
            }

            // Draw button
            g.SmoothingMode = SmoothingMode.AntiAlias;

            const int arrowInset = 3;
            int arrowSize = Math.Min(ClientSize.Width - arrowInset * 2, ClientSize.Height - arrowInset * 2) - 1;

            PointF a;
            PointF b;
            PointF c;

            switch (this.arrowDirection)
            {
                case ArrowDirection.Left:
                    a = new PointF(arrowInset, ClientSize.Height / 2);
                    b = new PointF(ClientSize.Width - arrowInset, (ClientSize.Height - arrowSize) / 2);
                    c = new PointF(ClientSize.Width - arrowInset, (ClientSize.Height + arrowSize) / 2);
                    break;

                case ArrowDirection.Right:
                    a = new PointF(ClientSize.Width - arrowInset, ClientSize.Height / 2);
                    b = new PointF(arrowInset, (ClientSize.Height - arrowSize) / 2);
                    c = new PointF(arrowInset, (ClientSize.Height + arrowSize) / 2);
                    break;

                case ArrowDirection.Up:
                    a = new PointF(ClientSize.Width / 2, (ClientSize.Height - arrowSize) / 2);
                    b = new PointF((ClientSize.Width - arrowSize) / 2, (ClientSize.Height + arrowSize) / 2);
                    c = new PointF((ClientSize.Width + arrowSize) / 2, (ClientSize.Height + arrowSize) / 2);
                    break;

                case ArrowDirection.Down:
                    a = new PointF(ClientSize.Width / 2, (ClientSize.Height + arrowSize) / 2);
                    b = new PointF((ClientSize.Width - arrowSize) / 2, (ClientSize.Height - arrowSize) / 2);
                    c = new PointF((ClientSize.Width + arrowSize) / 2, (ClientSize.Height - arrowSize) / 2);
                    break;

                default:
                    throw new InvalidEnumArgumentException("this.arrowDirection");
            }

            // SPIKE in order to get this rendering correctly right away
            if (this.arrowDirection == ArrowDirection.Down)
            {
                SmoothingMode oldSM = g.SmoothingMode;
                g.SmoothingMode = SmoothingMode.None;

                float top = b.Y - 2;
                float left = b.X;
                float right = c.X;
                int squareCount = (int)((right - left) / 3);

                Brush outlineBrush = new SolidBrush(arrowOutlineColor);
                Brush interiorBrush = new SolidBrush(arrowFillColor);

                g.FillRectangle(interiorBrush, left, top, right - left + 1, 3);

                ++left;
                while (left < right)
                {
                    RectangleF rect = new RectangleF(left, top + 1, 1, 1);
                    g.FillRectangle(outlineBrush, rect);
                    left += 2;
                }

                outlineBrush.Dispose();
                outlineBrush = null;

                interiorBrush.Dispose();
                interiorBrush = null;

                a.Y += 2;
                b.Y += 2;
                c.Y += 2;

                g.SmoothingMode = oldSM;
            }

            if (this.reverseArrowColors)
            {
                Utility.Swap(ref arrowFillColor, ref arrowOutlineColor);
            }

            using (Brush buttonBrush = new SolidBrush(arrowFillColor))
            {
                g.FillPolygon(buttonBrush, new PointF[] { a, b, c });
            }

            using (Pen buttonPen = new Pen(arrowOutlineColor, this.arrowOutlineWidth))
            {
                g.DrawPolygon(buttonPen, new PointF[] { a, b, c });
            }
        }
    }
}
