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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;

namespace PaintDotNet
{
    internal sealed class HistoryControl 
        : Control
    {
        private enum ItemType
        {
            Undo,
            Redo
        }

        private bool managedFocus = false;
        private VScrollBar vScrollBar;
        private HistoryStack historyStack = null;
        private int itemHeight;
        private int undoItemHighlight = -1;
        private int redoItemHighlight = -1;
        private int scrollOffset = 0;
        private Point lastMouseClientPt = new Point(-1, -1);
        private int ignoreScrollOffsetSet = 0;

        private void SuspendScrollOffsetSet()
        {
            ++this.ignoreScrollOffsetSet;
        }

        private void ResumeScrollOffsetSet()
        {
            --this.ignoreScrollOffsetSet;
        }

        public bool ManagedFocus
        {
            get
            {
                return this.managedFocus;
            }

            set
            {
                this.managedFocus = value;
            }
        }

        public event EventHandler RelinquishFocus;
        private void OnRelinquishFocus()
        {
            if (RelinquishFocus != null)
            {
                RelinquishFocus(this, EventArgs.Empty);
            }
        }

        private int ItemCount
        {
            get
            {
                if (this.historyStack == null)
                {
                    return 0;
                }
                else
                {
                    return this.historyStack.UndoStack.Count + this.historyStack.RedoStack.Count;
                }
            }
        }

        public event EventHandler ScrollOffsetChanged;
        private void OnScrollOffsetChanged()
        {
            this.vScrollBar.Value = Utility.Clamp(this.scrollOffset, this.vScrollBar.Minimum, this.vScrollBar.Maximum);

            if (ScrollOffsetChanged != null)
            {
                ScrollOffsetChanged(this, EventArgs.Empty);
            }
        }

        public int MinScrollOffset
        {
            get
            {
                return 0;
            }
        }

        public int MaxScrollOffset
        {
            get
            {
                return Math.Max(0, ViewHeight - ClientSize.Height);
            }
        }

        public int ScrollOffset
        {
            get
            {
                return this.scrollOffset;
            }

            set
            {
                if (this.ignoreScrollOffsetSet <= 0)
                {
                    int clampedOffset = Utility.Clamp(value, MinScrollOffset, MaxScrollOffset);

                    if (this.scrollOffset != clampedOffset)
                    {
                        this.scrollOffset = clampedOffset;
                        OnScrollOffsetChanged();
                        Invalidate(false);
                    }
                }
            }
        }

        public Rectangle ViewRectangle
        {
            get
            {
                return new Rectangle(0, 0, ViewWidth, ViewHeight);
            }
        }

        public Rectangle ClientRectangleToViewRectangle(Rectangle clientRect)
        {
            Point clientPt = ClientPointToViewPoint(clientRect.Location);
            return new Rectangle(clientPt, clientRect.Size);
        }

        public int ViewWidth
        {
            get
            {
                int width;

                if (this.vScrollBar.Visible)
                {
                    width = ClientSize.Width - this.vScrollBar.Width;
                }
                else
                {
                    width = ClientSize.Width;
                }

                return width;
            }
        }

        private int ViewHeight
        {
            get
            {
                return ItemCount * this.itemHeight;
            }
        }

        private void EnsureItemIsFullyVisible(ItemType itemType, int itemIndex)
        {
            Point itemPt = StackIndexToViewPoint(itemType, itemIndex);
            Rectangle itemRect = new Rectangle(itemPt, new Size(ViewWidth, this.itemHeight));

            int minOffset = itemRect.Bottom - ClientSize.Height;
            int maxOffset = itemRect.Top;

            ScrollOffset = Utility.Clamp(ScrollOffset, minOffset, maxOffset);
        }

        private Point ClientPointToViewPoint(Point pt)
        {
            return new Point(pt.X, pt.Y + ScrollOffset);
        }

        private void ViewPointToStackIndex(Point viewPt, out ItemType itemType, out int itemIndex)
        {
            Rectangle undoRect = UndoViewRectangle;

            if (viewPt.Y >= undoRect.Top && viewPt.Y < undoRect.Bottom)
            {
                itemType = ItemType.Undo;
                itemIndex = (viewPt.Y - undoRect.Top) / this.itemHeight;
            }
            else
            {
                Rectangle redoRect = RedoViewRectangle;

                itemType = ItemType.Redo;
                itemIndex = (viewPt.Y - redoRect.Top) / this.itemHeight;
            }
        }

        private Point StackIndexToViewPoint(ItemType itemType, int itemIndex)
        {
            int y;
            Rectangle typeRect;

            if (itemType == ItemType.Undo)
            {
                typeRect = UndoViewRectangle;
            }
            else // if (itemTYpe == ItemType.Redo)
            {
                typeRect = RedoViewRectangle;
            }

            y = (itemIndex * this.itemHeight) + typeRect.Top;
            return new Point(0, y);
        }

        public event EventHandler HistoryChanged;
        private void OnHistoryChanged()
        {
            this.vScrollBar.Maximum = ViewHeight;

            if (HistoryChanged != null)
            {
                HistoryChanged(this, EventArgs.Empty);
            }
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            int totalItems;

            if (this.historyStack == null)
            {
                totalItems = 0;
            }
            else
            {
                totalItems = this.historyStack.UndoStack.Count + this.historyStack.RedoStack.Count;
            }

            int totalHeight = totalItems * this.itemHeight;

            if (totalHeight > ClientSize.Height)
            {
                this.vScrollBar.Visible = true;
                this.vScrollBar.Location = new Point(ClientSize.Width - this.vScrollBar.Width, 0);
                this.vScrollBar.Height = ClientSize.Height;
                this.vScrollBar.Minimum = 0;
                this.vScrollBar.Maximum = totalHeight;
                this.vScrollBar.LargeChange = ClientSize.Height;
                this.vScrollBar.SmallChange = this.itemHeight;
            }
            else
            {
                this.vScrollBar.Visible = false;
            }

            if (this.historyStack != null)
            {
                ScrollOffset = Utility.Clamp(ScrollOffset, MinScrollOffset, MaxScrollOffset);
            }

            base.OnLayout(levent);
        }

        private Rectangle UndoViewRectangle
        {
            get
            {
                return new Rectangle(0, 0, ViewWidth, this.itemHeight * this.historyStack.UndoStack.Count);
            }
        }

        private Rectangle RedoViewRectangle
        {
            get
            {
                int undoRectBottom = this.itemHeight * this.historyStack.UndoStack.Count;
                return new Rectangle(0, undoRectBottom, ViewWidth, this.itemHeight * this.historyStack.RedoStack.Count);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (this.historyStack != null)
            {
                using (SolidBrush backBrush = new SolidBrush(BackColor))
                {
                    e.Graphics.FillRectangle(backBrush, e.ClipRectangle);
                }

                e.Graphics.TranslateTransform(0, -this.scrollOffset);

                int afterImageHMargin = UI.ScaleWidth(1);

                StringFormat stringFormat = (StringFormat)StringFormat.GenericTypographic.Clone();
                stringFormat.LineAlignment = StringAlignment.Center;
                stringFormat.Trimming = StringTrimming.EllipsisCharacter;

                Rectangle visibleViewRectangle = ClientRectangleToViewRectangle(ClientRectangle);

                // Fill in the background for the undo items
                Rectangle undoRect = UndoViewRectangle;
                e.Graphics.FillRectangle(SystemBrushes.Window, undoRect);

                // We only want to draw what's visible, so figure out the first and last
                // undo items that are actually visible and only draw them.
                Rectangle visibleUndoRect = Rectangle.Intersect(visibleViewRectangle, undoRect);

                int beginUndoIndex;
                int endUndoIndex;
                if (visibleUndoRect.Width > 0 && visibleUndoRect.Height > 0)
                {
                    ItemType itemType;
                    ViewPointToStackIndex(visibleUndoRect.Location, out itemType, out beginUndoIndex);
                    ViewPointToStackIndex(new Point(visibleUndoRect.Left, visibleUndoRect.Bottom - 1), out itemType, out endUndoIndex);
                }
                else
                {
                    beginUndoIndex = 0;
                    endUndoIndex = -1;
                }

                // Draw undo items
                for (int i = beginUndoIndex; i <= endUndoIndex; ++i)
                {
                    Image image;
                    ImageResource imageResource = this.historyStack.UndoStack[i].Image;

                    if (imageResource != null)
                    {
                        image = imageResource.Reference;
                    }
                    else
                    {
                        image = null;
                    }

                    int drawWidth;
                    if (image != null)
                    {
                        drawWidth = (image.Width * this.itemHeight) / image.Height;
                    }
                    else
                    {
                        drawWidth = this.itemHeight;
                    }

                    Brush textBrush;

                    if (i == this.undoItemHighlight)
                    {
                        Rectangle itemRect = new Rectangle(
                            0,
                            i * this.itemHeight,
                            ViewWidth,
                            this.itemHeight);

                        e.Graphics.FillRectangle(SystemBrushes.Highlight, itemRect);
                        textBrush = SystemBrushes.HighlightText;
                    }
                    else
                    {
                        textBrush = SystemBrushes.WindowText;
                    }

                    if (image != null)
                    {
                        e.Graphics.DrawImage(
                            image,
                            new Rectangle(0, i * this.itemHeight, drawWidth, this.itemHeight),
                            new Rectangle(0, 0, image.Width, image.Height),
                            GraphicsUnit.Pixel);
                    }

                    int textX = drawWidth + afterImageHMargin;

                    Rectangle textRect = new Rectangle(
                        textX,
                        i * this.itemHeight,
                        ViewWidth - textX,
                        this.itemHeight);

                    e.Graphics.DrawString(
                        this.historyStack.UndoStack[i].Name, 
                        Font,
                        textBrush, 
                        textRect, 
                        stringFormat);
                }

                // Fill in the background for the redo items
                Rectangle redoRect = RedoViewRectangle;
                e.Graphics.FillRectangle(Brushes.SlateGray, redoRect);

                Font redoFont = new Font(Font, Font.Style | FontStyle.Italic);

                // We only want to draw what's visible, so figure out the first and last
                // redo items that are actually visible and only draw them.
                Rectangle visibleRedoRect = Rectangle.Intersect(visibleViewRectangle, redoRect);

                int beginRedoIndex;
                int endRedoIndex;
                if (visibleRedoRect.Width > 0 && visibleRedoRect.Height > 0)
                {
                    ItemType itemType;
                    ViewPointToStackIndex(visibleRedoRect.Location, out itemType, out beginRedoIndex);
                    ViewPointToStackIndex(new Point(visibleRedoRect.Left, visibleRedoRect.Bottom - 1), out itemType, out endRedoIndex);
                }
                else
                {
                    beginRedoIndex = 0;
                    endRedoIndex = -1;
                } 

                // Draw redo items
                for (int i = beginRedoIndex; i <= endRedoIndex; ++i)
                {
                    Image image;
                    ImageResource imageResource = this.historyStack.RedoStack[i].Image;

                    if (imageResource != null)
                    {
                        image = imageResource.Reference;
                    }
                    else
                    {
                        image = null;
                    }

                    int drawWidth;

                    if (image != null)
                    {
                        drawWidth = (image.Width * this.itemHeight) / image.Height;
                    }
                    else
                    {
                        drawWidth = this.itemHeight;
                    }

                    int y = redoRect.Top + i * this.itemHeight;

                    Brush textBrush;
                    if (i == this.redoItemHighlight)
                    {
                        Rectangle itemRect = new Rectangle(
                            0,
                            y,
                            ViewWidth,
                            this.itemHeight);

                        e.Graphics.FillRectangle(SystemBrushes.Highlight, itemRect);
                        textBrush = SystemBrushes.HighlightText;
                    }
                    else
                    {
                        textBrush = SystemBrushes.InactiveCaptionText;
                    }

                    if (image != null)
                    {
                        e.Graphics.DrawImage(
                            image,
                            new Rectangle(0, y, drawWidth, this.itemHeight),
                            new Rectangle(0, 0, image.Width, image.Height),
                            GraphicsUnit.Pixel);
                    }

                    int textX = drawWidth + afterImageHMargin;

                    Rectangle textRect = new Rectangle(
                        textX,
                        y,
                        ViewWidth - textX,
                        this.itemHeight);

                    e.Graphics.DrawString(
                        this.historyStack.RedoStack[i].Name,
                        redoFont,
                        textBrush,
                        textRect,
                        stringFormat);
                }

                redoFont.Dispose();
                redoFont = null;

                stringFormat.Dispose();
                stringFormat = null;

                e.Graphics.TranslateTransform(0, this.scrollOffset);
            }

            base.OnPaint(e);
        }

        public HistoryStack HistoryStack
        {
            get
            {
                return this.historyStack;
            }

            set
            {
                if (this.historyStack != null)
                {
                    this.historyStack.Changed -= History_Changed;
                    this.historyStack.SteppedForward -= History_SteppedForward;
                    this.historyStack.SteppedBackward -= History_SteppedBackward;
                    this.historyStack.HistoryFlushed -= History_HistoryFlushed;
                    this.historyStack.NewHistoryMemento -= History_NewHistoryMemento;
                }

                this.historyStack = value;
                PerformLayout();

                if (this.historyStack != null)
                {
                    this.historyStack.Changed += History_Changed;
                    this.historyStack.SteppedForward += History_SteppedForward;
                    this.historyStack.SteppedBackward += History_SteppedBackward;
                    this.historyStack.HistoryFlushed += History_HistoryFlushed;
                    this.historyStack.NewHistoryMemento += History_NewHistoryMemento;
                    EnsureLastUndoItemIsFullyVisible();
                }

                Refresh();
                OnHistoryChanged();
            }
        }

        private void EnsureLastUndoItemIsFullyVisible()
        {
            int index = this.historyStack.UndoStack.Count - 1;
            EnsureItemIsFullyVisible(ItemType.Undo, index);
        }

        private void History_HistoryFlushed(object sender, EventArgs e)
        {
            if (IsDisposed)
            {
                return;
            }

            EnsureLastUndoItemIsFullyVisible();
            PerformMouseMove();
            PerformLayout();
            Refresh();
        }

        private void History_SteppedForward(object sender, EventArgs e)
        {
            if (IsDisposed)
            {
                return;
            }

            this.undoItemHighlight = -1;
            this.redoItemHighlight = -1;
            EnsureLastUndoItemIsFullyVisible();
            PerformMouseMove();
            PerformLayout();
            Refresh();
        }

        private void History_SteppedBackward(object sender, EventArgs e)
        {
            if (IsDisposed)
            {
                return;
            }

            this.undoItemHighlight = -1;
            this.redoItemHighlight = -1;
            EnsureLastUndoItemIsFullyVisible();
            PerformMouseMove();
            PerformLayout();
            Refresh();
        }

        private void History_NewHistoryMemento(object sender, EventArgs e)
        {
            if (IsDisposed)
            {
                return;
            }

            EnsureLastUndoItemIsFullyVisible();
            PerformMouseMove();
            PerformLayout();
            Invalidate();
        }

        private void History_Changed(object sender, EventArgs e)
        {
            if (IsDisposed)
            {
                return;
            }

            PerformMouseMove();
            PerformLayout();
            Refresh();
            OnHistoryChanged();
        }

        public HistoryControl()
        {
            UI.InitScaling(this);
            this.itemHeight = UI.ScaleHeight(16);

            SetStyle(ControlStyles.StandardDoubleClick, false);

            InitializeComponent();
        }

        private void KeyUpHandler(object sender, KeyEventArgs e)
        {
            this.OnKeyUp(e);
        }

        private void OnItemClicked(ItemType itemType, int itemIndex)
        {
            HistoryMemento hm;

            if (itemType == ItemType.Undo)
            {
                if (itemIndex >= 0 && itemIndex < this.historyStack.UndoStack.Count)
                {
                    hm = this.historyStack.UndoStack[itemIndex];
                }
                else
                {
                    hm = null;
                }
            }
            else
            {
                if (itemIndex >= 0 && itemIndex < this.historyStack.RedoStack.Count)
                {
                    hm = this.historyStack.RedoStack[itemIndex];
                }
                else
                {
                    hm = null;
                }
            }

            if (hm != null)
            {
                EnsureItemIsFullyVisible(itemType, itemIndex);
                OnItemClicked(itemType, hm);
            }
        }

        private void OnItemClicked(ItemType itemType, HistoryMemento hm)
        {
            int hmID = hm.ID;

            if (itemType == ItemType.Undo)
            {
                if (hmID == this.historyStack.UndoStack[historyStack.UndoStack.Count - 1].ID)
                {
                    if (historyStack.UndoStack.Count > 1)
                    {
                        this.historyStack.StepBackward();
                    }
                }
                else
                {
                    SuspendScrollOffsetSet();

                    this.historyStack.BeginStepGroup();

                    using (new WaitCursorChanger(this))
                    {
                        while (this.historyStack.UndoStack[this.historyStack.UndoStack.Count - 1].ID != hmID)
                        {
                            this.historyStack.StepBackward();
                        }
                    }

                    this.historyStack.EndStepGroup();

                    ResumeScrollOffsetSet();
                }
            }
            else // if (itemType == ItemType.Redo)
            {
                SuspendScrollOffsetSet();

                // Step forward to redo
                this.historyStack.BeginStepGroup();

                using (new WaitCursorChanger(this))
                {
                    while (this.historyStack.UndoStack[this.historyStack.UndoStack.Count - 1].ID != hmID)
                    {
                        this.historyStack.StepForward();
                    }
                }

                this.historyStack.EndStepGroup();

                ResumeScrollOffsetSet();
            }

            Focus();
        }

        protected override void OnResize(EventArgs e)
        {
            PerformLayout();
            base.OnResize(e);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            PerformLayout();
            base.OnSizeChanged(e);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            if (this.historyStack != null)
            {
                if (this.managedFocus && !MenuStripEx.IsAnyMenuActive && UI.IsOurAppActive)
                {
                    Focus();
                }
            }

            base.OnMouseEnter(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (this.historyStack != null)
            {
                Point clientPt = new Point(e.X, e.Y);
                Point viewPt = ClientPointToViewPoint(clientPt);

                ItemType itemType;
                int itemIndex;
                ViewPointToStackIndex(viewPt, out itemType, out itemIndex);

                switch (itemType)
                {
                    case ItemType.Undo:
                        if (itemIndex >= 0 && itemIndex < this.historyStack.UndoStack.Count)
                        {
                            this.undoItemHighlight = itemIndex;
                        }
                        else
                        {
                            this.undoItemHighlight = -1;
                        }

                        this.redoItemHighlight = -1;
                        break;

                    case ItemType.Redo:
                        this.undoItemHighlight = -1;

                        if (itemIndex >= 0 && itemIndex < this.historyStack.RedoStack.Count)
                        {
                            this.redoItemHighlight = itemIndex;
                        }
                        else
                        {
                            this.redoItemHighlight = -1;
                        } 
                        break;

                    default:
                        throw new InvalidEnumArgumentException();
                }

                Refresh();
                this.lastMouseClientPt = clientPt;
            }

            base.OnMouseMove(e);
        }

        protected override void OnClick(EventArgs e)
        {
            if (this.historyStack != null)
            {
                Point viewPt = ClientPointToViewPoint(this.lastMouseClientPt);

                ItemType itemType;
                int itemIndex;
                ViewPointToStackIndex(viewPt, out itemType, out itemIndex);

                OnItemClicked(itemType, itemIndex);
            }

            base.OnClick(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (this.historyStack != null)
            {
                int items = (e.Delta * SystemInformation.MouseWheelScrollLines) / SystemInformation.MouseWheelScrollDelta;
                int pixels = items * this.itemHeight;
                ScrollOffset -= pixels;

                PerformMouseMove();
            }

            base.OnMouseWheel(e);
        }

        private void PerformMouseMove()
        {
            Point clientPt = PointToClient(Control.MousePosition);

            if (ClientRectangle.Contains(clientPt))
            {
                MouseEventArgs me = new MouseEventArgs(MouseButtons.None, 0, clientPt.X, clientPt.Y, 0);
                OnMouseMove(me);
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if (this.historyStack != null)
            {
                this.undoItemHighlight = -1;
                this.redoItemHighlight = -1;
                Refresh();

                if (this.Focused && this.managedFocus)
                {
                    OnRelinquishFocus();
                }
            }

            base.OnMouseLeave(e);
        }

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.vScrollBar = new VScrollBar();
            SuspendLayout();
            //
            // vScrollBar
            //
            this.vScrollBar.Name = "vScrollBar";
            this.vScrollBar.ValueChanged += new EventHandler(VScrollBar_ValueChanged);
            //
            // HistoryControl
            //
            this.Name = "HistoryControl";
            this.TabStop = false;
            this.Controls.Add(this.vScrollBar);
            this.ResizeRedraw = true;
            this.DoubleBuffered = true;
            ResumeLayout();
            PerformLayout();
        }
        #endregion

        private void VScrollBar_ValueChanged(object sender, EventArgs e)
        {
            ScrollOffset = this.vScrollBar.Value;
        }
    }
}
