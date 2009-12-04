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
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PaintDotNet
{
    public sealed class ControlShadow 
        : Control
    {
        private Image roundedEdgeUL = null;
        private Image roundedEdgeUR = null;
        private Image roundedEdgeLL = null;
        private Image roundedEdgeLR = null;

        private System.ComponentModel.Container components = null;

        private Control occludingControl;

        private static bool betaTagDone = false;
        private string betaTagString = null;
        private int betaTagOpacity = 255;
        private Timer betaTagTimer;
        private DateTime betaTagStart;

        [Browsable(false)]
        public Control OccludingControl
        {
            get 
            { 
                return this.occludingControl; 
            }

            set 
            {
                this.occludingControl = value;
                Invalidate();
            }
        }

        public ControlShadow()
        {
            this.SetStyle(ControlStyles.Opaque, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            this.Dock = DockStyle.Fill;
            this.DoubleBuffered = true;
            this.ResizeRedraw = true;

            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            BackColor = Color.FromArgb(0xc0, 0xc0, 0xc0);

            this.roundedEdgeUL = PdnResources.GetImageResource("Images.RoundedEdgeUL.png").Reference;
            this.roundedEdgeUR = PdnResources.GetImageResource("Images.RoundedEdgeUR.png").Reference;
            this.roundedEdgeLL = PdnResources.GetImageResource("Images.RoundedEdgeLL.png").Reference;
            this.roundedEdgeLR = PdnResources.GetImageResource("Images.RoundedEdgeLR.png").Reference;

            if (!PdnInfo.IsFinalBuild && !betaTagDone)
            {
                betaTagDone = true;

                string betaTagStringFormat = PdnResources.GetString("ControlShadow.BetaTag.Text.Format");
                string appName = PdnInfo.GetFullAppName();
                string expiredDateString = PdnInfo.ExpirationDate.ToShortDateString();
                this.betaTagString = string.Format(betaTagStringFormat, appName, expiredDateString);

                this.betaTagStart = DateTime.Now;
                this.betaTagTimer = new Timer();
                this.betaTagTimer.Interval = 100;
                this.betaTagTimer.Tick += new EventHandler(BetaTagTimer_Tick);
                this.betaTagTimer.Enabled = true;
            }
        }

        private void BetaTagTimer_Tick(object sender, EventArgs e)
        {
            int newOpacity;
            TimeSpan uptime = DateTime.Now - this.betaTagStart;

            if (uptime.TotalMilliseconds < 10000)
            {
                newOpacity = 255;
            }
            else
            {
                newOpacity = (int)(255 - (((uptime.TotalMilliseconds - 10000) * 128) / 1000));
                newOpacity = Math.Max(0, newOpacity);
            }

            if (this.betaTagOpacity != newOpacity)
            {
                this.betaTagOpacity = newOpacity;
                Invalidate();
            }

            if (this.betaTagOpacity == 0)
            {
                this.betaTagTimer.Enabled = false;
                this.betaTagTimer.Tick -= BetaTagTimer_Tick;
                this.betaTagTimer.Dispose();
                this.betaTagTimer = null;
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
        }
        #endregion

        protected override void OnPaint(PaintEventArgs pe)
        {
            DrawShadow(pe.Graphics, pe.ClipRectangle);
            base.OnPaint(pe);
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            base.OnPaintBackground(pevent);
        }

        private void DrawShadow(Graphics g, Rectangle clipRect)
        {
            if (this.occludingControl != null)
            {
                // Draw the outline rectangle
                Rectangle outlineRect = new Rectangle(new Point(0, 0), this.occludingControl.Size);

                outlineRect = occludingControl.RectangleToScreen(outlineRect);
                outlineRect = RectangleToClient(outlineRect);
                outlineRect.X -= 1;
                outlineRect.Y -= 1;
                outlineRect.Width += 2;
                outlineRect.Height += 2;

                g.DrawLines(
                    Pens.Black,
                    new Point[]
                    {
                        new Point(outlineRect.Left, outlineRect.Top),
                        new Point(outlineRect.Right, outlineRect.Top),
                        new Point(outlineRect.Right, outlineRect.Bottom),
                        new Point(outlineRect.Left, outlineRect.Bottom),
                        new Point(outlineRect.Left, outlineRect.Top)
                    }); 
                
                using (PdnRegion backRegion = new PdnRegion(clipRect))
                {
                    Rectangle occludingRect = new Rectangle(0, 0, this.occludingControl.Width, this.occludingControl.Height);
                    occludingRect = this.occludingControl.RectangleToScreen(occludingRect);
                    occludingRect = RectangleToClient(occludingRect);
                    backRegion.Exclude(occludingRect);
                    backRegion.Exclude(outlineRect);

                    using (Brush backBrush = new SolidBrush(this.BackColor))
                    {
                        g.FillRegion(backBrush, backRegion.GetRegionReadOnly());
                    }
                }
                
                Rectangle edgeRect = new Rectangle(0, 0, this.roundedEdgeUL.Width, this.roundedEdgeUR.Height);

                Rectangle ulEdgeRect = new Rectangle(outlineRect.Left - 3, outlineRect.Top - 3, this.roundedEdgeUL.Width, this.roundedEdgeUL.Height);
                Rectangle urEdgeRect = new Rectangle(outlineRect.Right - 3, outlineRect.Top - 3, this.roundedEdgeUR.Width, this.roundedEdgeUR.Height);
                Rectangle llEdgeRect = new Rectangle(outlineRect.Left - 3, outlineRect.Bottom - 3, this.roundedEdgeLL.Width, this.roundedEdgeLL.Height);
                Rectangle lrEdgeRect = new Rectangle(outlineRect.Right - 3, outlineRect.Bottom - 3, this.roundedEdgeLR.Width, this.roundedEdgeLR.Height);

                g.DrawImage(this.roundedEdgeUL, ulEdgeRect, edgeRect, GraphicsUnit.Pixel);
                g.DrawImage(this.roundedEdgeUR, urEdgeRect, edgeRect, GraphicsUnit.Pixel);
                g.DrawImage(this.roundedEdgeLL, llEdgeRect, edgeRect, GraphicsUnit.Pixel);
                g.DrawImage(this.roundedEdgeLR, lrEdgeRect, edgeRect, GraphicsUnit.Pixel);

                Color c1 = Color.FromArgb(95, Color.Black);
                Color c2 = Color.FromArgb(47, Color.Black);
                Color c3 = Color.FromArgb(15, Color.Black);

                Pen p1 = new Pen(c1);
                Pen p2 = new Pen(c2);
                Pen p3 = new Pen(c3);

                // Draw top soft edge
                g.DrawLine(p1, ulEdgeRect.Right, outlineRect.Top - 1, urEdgeRect.Left - 1, outlineRect.Top - 1);
                g.DrawLine(p2, ulEdgeRect.Right, outlineRect.Top - 2, urEdgeRect.Left - 1, outlineRect.Top - 2);
                g.DrawLine(p3, ulEdgeRect.Right, outlineRect.Top - 3, urEdgeRect.Left - 1, outlineRect.Top - 3);

                // Draw bottom soft edge
                g.DrawLine(p1, llEdgeRect.Right, outlineRect.Bottom + 0, lrEdgeRect.Left - 1, outlineRect.Bottom + 0);
                g.DrawLine(p2, llEdgeRect.Right, outlineRect.Bottom + 1, lrEdgeRect.Left - 1, outlineRect.Bottom + 1);
                g.DrawLine(p3, llEdgeRect.Right, outlineRect.Bottom + 2, lrEdgeRect.Left - 1, outlineRect.Bottom + 2);

                // Draw left soft edge
                g.DrawLine(p1, outlineRect.Left - 1, ulEdgeRect.Bottom, outlineRect.Left - 1, llEdgeRect.Top - 1);
                g.DrawLine(p2, outlineRect.Left - 2, ulEdgeRect.Bottom, outlineRect.Left - 2, llEdgeRect.Top - 1);
                g.DrawLine(p3, outlineRect.Left - 3, ulEdgeRect.Bottom, outlineRect.Left - 3, llEdgeRect.Top - 1);

                // Draw right soft edge
                g.DrawLine(p1, outlineRect.Right + 0, urEdgeRect.Bottom, outlineRect.Right + 0, lrEdgeRect.Top - 1);
                g.DrawLine(p2, outlineRect.Right + 1, urEdgeRect.Bottom, outlineRect.Right + 1, lrEdgeRect.Top - 1);
                g.DrawLine(p3, outlineRect.Right + 2, urEdgeRect.Bottom, outlineRect.Right + 2, lrEdgeRect.Top - 1);

                p1.Dispose();
                p1 = null;

                p2.Dispose();
                p2 = null;

                p3.Dispose();
                p3 = null;
            }

            if (this.betaTagString != null)
            {
                Color betaTagColor = Color.FromArgb(this.betaTagOpacity, SystemColors.WindowText);
                Brush betaTagBrush = new SolidBrush(betaTagColor);
                StringFormat sf = (StringFormat)StringFormat.GenericTypographic.Clone();

                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Near;

                g.DrawString(
                    this.betaTagString,
                    this.Font,
                    betaTagBrush,
                    ClientRectangle.Width / 2,
                    1,
                    sf);

                sf.Dispose();
                sf = null;

                betaTagBrush.Dispose();
                betaTagBrush = null;
            }
        }

        protected override void WndProc(ref Message m)
        {
            // Ignore focus
            if (m.Msg == 7 /* WM_SETFOCUS */)
            {
                return;
            }

            base.WndProc (ref m);
        }
    }
}
