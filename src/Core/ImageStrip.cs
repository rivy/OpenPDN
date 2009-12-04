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
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace PaintDotNet
{
    /*
     * Coordinate spaces:
     * 
     * - Client -- This is the same as the Windows client space.
     * - Screen -- This is the same as the Windows screen space.
     * - View   -- This is a virtual coordinate in the viewable, scrollable area. Client->View is defined as Client.X+ScrollOffset. View->Client is View.X-ScrollOffset
     * - Item   -- This is relative to an item. Item->View is Item.X+(itemIndex * ItemViewSize.Width)
     * 
     * */

    public class ImageStrip
        : Control
    {
        public enum ItemPart
        {
            None,
            Image,
            CloseButton
        }

        public sealed class Item
        {
            private PushButtonState imageRenderState;
            private Image image;

            private bool selected;

            private PushButtonState checkRenderState;
            private CheckState checkState;

            private PushButtonState closeRenderState;

            private bool dirty;

            private bool lockedDirtyValue;
            private int dirtyValueLockCount = 0;

            private object tag;

            public event EventHandler Changed;
            private void OnChanged()
            {
                if (Changed != null)
                {
                    Changed(this, EventArgs.Empty);
                }
            }

            public Image Image
            {
                get
                {
                    return this.image;
                }

                set
                {
                    this.image = value;
                    OnChanged();
                }
            }

            public PushButtonState ImageRenderState
            {
                get
                {
                    return this.imageRenderState;
                }

                set
                {
                    if (this.imageRenderState != value)
                    {
                        this.imageRenderState = value;
                        OnChanged();
                    }
                }
            }

            public bool Selected
            {
                get
                {
                    return this.selected;
                }

                set
                {
                    if (this.selected != value)
                    {
                        this.selected = value;
                        OnChanged();
                    }
                }
            }

            public bool Dirty
            {
                get
                {
                    if (this.dirtyValueLockCount > 0)
                    {
                        return this.lockedDirtyValue;
                    }
                    else
                    {
                        return this.dirty;
                    }
                }

                set
                {
                    if (this.dirty != value)
                    {
                        this.dirty = value;

                        if (this.dirtyValueLockCount <= 0)
                        {
                            OnChanged();
                        }
                    }
                }
            }

            public void LockDirtyValue(bool forceValue)
            {
                ++this.dirtyValueLockCount;

                if (this.dirtyValueLockCount == 1)
                {
                    this.lockedDirtyValue = forceValue;
                }
            }

            public void UnlockDirtyValue()
            {
                --this.dirtyValueLockCount;

                if (this.dirtyValueLockCount == 0)
                {
                    OnChanged();
                }
                else if (this.dirtyValueLockCount < 0)
                {
                    throw new InvalidOperationException("Calls to UnlockDirtyValue() must be matched by a preceding call to LockDirtyValue()");
                }
            }

            public bool Checked
            {
                get
                {
                    return (CheckState == CheckState.Checked);
                }

                set
                {
                    if (value)
                    {
                        CheckState = CheckState.Checked;
                    }
                    else
                    {
                        CheckState = CheckState.Unchecked;
                    }
                }
            }

            public CheckState CheckState
            {
                get
                {
                    return this.checkState;
                }

                set
                {
                    if (this.checkState != value)
                    {
                        this.checkState = value;
                        OnChanged();
                    }
                }
            }

            public PushButtonState CheckRenderState
            {
                get
                {
                    return this.checkRenderState;
                }

                set
                {
                    if (this.checkRenderState != value)
                    {
                        this.checkRenderState = value;
                        OnChanged();
                    }
                }
            }

            public PushButtonState CloseRenderState
            {
                get
                {
                    return this.closeRenderState;
                }

                set
                {
                    if (this.closeRenderState != value)
                    {
                        this.closeRenderState = value;
                        OnChanged();
                    }
                }
            }

            public void SetPartRenderState(ItemPart itemPart, PushButtonState renderState)
            {
                switch (itemPart)
                {
                    case ItemPart.None:
                        break;

                    case ItemPart.CloseButton:
                        CloseRenderState = renderState;
                        break;

                    case ItemPart.Image:
                        ImageRenderState = renderState;
                        break;

                    default:
                        throw new InvalidEnumArgumentException();
                }
            }

            public object Tag
            {
                get
                {
                    return this.tag;
                }

                set
                {
                    this.tag = value;
                    OnChanged();
                }
            }

            public void Update()
            {
                OnChanged();
            }

            public Item()
            {
            }

            public Item(Image image)
            {
                this.image = image;
            }
        }

        private bool managedFocus = false;
        private bool showScrollButtons = false;
        private ArrowButton leftScrollButton;
        private ArrowButton rightScrollButton;

        private int scrollOffset = 0;
        private bool showCloseButtons = false;
        private const int closeButtonLength = 13;
        private int imagePadding = 2;
        private int closeButtonPadding = 2;

        private int mouseOverIndex = -1;
        private ItemPart mouseOverItemPart = ItemPart.None;
        private bool mouseOverApplyRendering = false;

        private int mouseDownIndex = -1;
        private MouseButtons mouseDownButton = MouseButtons.None;
        private ItemPart mouseDownItemPart = ItemPart.None;
        private bool mouseDownApplyRendering = false;

        private bool drawShadow = true;
        private bool drawDirtyOverlay = true;

        public bool DrawShadow
        {
            get
            {
                return this.drawShadow;
            }

            set
            {
                if (this.drawShadow != value)
                {
                    this.drawShadow = value;
                    Refresh();
                }
            }
        }

        public bool DrawDirtyOverlay
        {
            get
            {
                return this.drawDirtyOverlay;
            }

            set
            {
                if (this.drawDirtyOverlay != value)
                {
                    this.drawDirtyOverlay = value;
                    Refresh();
                }
            }
        }

        // This is done as an optimization: otherwise we're getting flooded with MouseMove events
        // and constantly refreshing our rendering. So CPU usage goes to heck.
        private Point lastMouseMovePt = new Point(-32000, -32000); 

        private List<Item> items = new List<Item>();

        protected ArrowButton LeftScrollButton
        {
            get
            {
                return this.leftScrollButton;
            }
        }

        protected ArrowButton RightScrollButton
        {
            get
            {
                return this.rightScrollButton;
            }
        }

        private void MouseStatesToItemStates()
        {
            UI.SuspendControlPainting(this);

            for (int i = 0; i < this.items.Count; ++i)
            {
                this.items[i].CheckRenderState = PushButtonState.Normal;
                this.items[i].CloseRenderState = PushButtonState.Normal;
                this.items[i].ImageRenderState = PushButtonState.Normal;
                this.items[i].Selected = false;
            }

            if (this.mouseDownApplyRendering)
            {
                if (this.mouseDownIndex < 0 || this.mouseDownIndex >= this.items.Count)
                {
                    this.mouseDownApplyRendering = false;
                }
                else
                {
                    this.items[this.mouseDownIndex].SetPartRenderState(this.mouseDownItemPart, PushButtonState.Pressed);
                    this.items[this.mouseDownIndex].Selected = true;
                }
            }
            else if (this.mouseOverApplyRendering)
            {
                if (this.mouseOverIndex < 0 || this.mouseOverIndex >= this.items.Count)
                {
                    this.mouseOverApplyRendering = false;
                }
                else
                {
                    this.items[this.mouseOverIndex].SetPartRenderState(this.mouseOverItemPart, PushButtonState.Hot);
                    this.items[this.mouseOverIndex].Selected = true;
                }
            }

            UI.ResumeControlPainting(this);
            Invalidate();
        }

        public ImageStrip()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.Selectable, false);

            DoubleBuffered = true;
            ResizeRedraw = true;

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.leftScrollButton = new ArrowButton();
            this.rightScrollButton = new ArrowButton();
            SuspendLayout();
            //
            // leftScrollButton
            //
            this.leftScrollButton.Name = "leftScrollButton";
            this.leftScrollButton.ArrowDirection = ArrowDirection.Left;
            this.leftScrollButton.ArrowOutlineWidth = 1.0f;
            this.leftScrollButton.Click += new EventHandler(LeftScrollButton_Click);
            this.leftScrollButton.DrawWithGradient = true;
            //
            // rightScrollButton
            //
            this.rightScrollButton.Name = "rightScrollButton";
            this.rightScrollButton.ArrowDirection = ArrowDirection.Right;
            this.rightScrollButton.ArrowOutlineWidth = 1.0f;
            this.rightScrollButton.Click += new EventHandler(RightScrollButton_Click);
            this.rightScrollButton.DrawWithGradient = true;
            //
            // ImageStrip
            //
            this.Name = "ImageStrip";
            this.TabStop = false;
            this.Controls.Add(this.leftScrollButton);
            this.Controls.Add(this.rightScrollButton);
            ResumeLayout();
            PerformLayout();
        }

        public event EventHandler<EventArgs<ArrowDirection>> ScrollArrowClicked;
        protected virtual void OnScrollArrowClicked(ArrowDirection arrowDirection)
        {
            if (ScrollArrowClicked != null)
            {
                ScrollArrowClicked(this, new EventArgs<ArrowDirection>(arrowDirection));
            }
        }

        private void LeftScrollButton_Click(object sender, EventArgs e)
        {
            Focus();
            OnScrollArrowClicked(ArrowDirection.Left);
        }

        private void RightScrollButton_Click(object sender, EventArgs e)
        {
            Focus();
            OnScrollArrowClicked(ArrowDirection.Right);
        }

        /// <summary>
        /// This event is raised when this control wishes to relinquish focus.
        /// </summary>
        public event EventHandler RelinquishFocus;

        private void OnRelinquishFocus()
        {
            if (RelinquishFocus != null)
            {
                RelinquishFocus(this, EventArgs.Empty);
            }
        }   
        
        /// <summary>
        /// Gets or sets whether the control manages focus.
        /// </summary>
        /// <remarks>
        /// If this is true, the toolstrip will capture focus when the mouse enters its client area. It will then
        /// relinquish focus (via the RelinquishFocus event) when the mouse leaves. It will not capture or
        /// attempt to relinquish focus if MenuStripEx.IsAnyMenuActive returns true.
        /// </remarks>
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

        public void AddItem(Item newItem)
        {
            if (this.items.Contains(newItem))
            {
                throw new ArgumentException("newItem was already added to this control");
            }

            newItem.Changed += Item_Changed;
            this.items.Add(newItem);

            PerformLayout();
            Invalidate();
        }

        public void RemoveItem(Item item)
        {
            if (!this.items.Contains(item))
            {
                throw new ArgumentException("item was never added to this control");
            }

            item.Changed -= Item_Changed;
            this.items.Remove(item);

            PerformLayout();
            Invalidate();
        }

        public void ClearItems()
        {
            SuspendLayout();
            UI.SuspendControlPainting(this);

            while (this.items.Count > 0)
            {
                RemoveItem(this.items[this.items.Count - 1]);
            }

            UI.ResumeControlPainting(this);
            ResumeLayout(true);

            Invalidate();
        }

        private void Item_Changed(object sender, EventArgs e)
        {
            Invalidate();
        }

        /// <summary>
        /// Raised when an item is clicked on.
        /// </summary>
        /// <remarks>
        /// e.Data.First is a reference to the Item. 
        /// e.Data.Second is the ItemPart.
        /// e.Data.Third is the MouseButtons that was used to click on the ItemPart.
        /// </remarks>
        public event EventHandler<EventArgs<Triple<Item, ItemPart, MouseButtons>>> ItemClicked;
        protected virtual void OnItemClicked(Item item, ItemPart itemPart, MouseButtons mouseButtons)
        {
            if (ItemClicked != null)
            {
                ItemClicked(this, new EventArgs<Triple<Item, ItemPart, MouseButtons>>(
                    Triple.Create(item, itemPart, mouseButtons)));
            }
        }

        public void PerformItemClick(int itemIndex, ItemPart itemPart, MouseButtons mouseButtons)
        {
            PerformItemClick(this.items[itemIndex], itemPart, mouseButtons);
        }

        public void PerformItemClick(Item item, ItemPart itemPart, MouseButtons mouseButtons)
        {
            OnItemClicked(item, itemPart, mouseButtons);
        }

        public Item[] Items
        {
            get
            {
                return this.items.ToArray();
            }
        }

        public int ItemCount
        {
            get
            {
                return this.items.Count;
            }
        }

        public bool ShowScrollButtons
        {
            get
            {
                return this.showScrollButtons;
            }

            set
            {
                if (this.showScrollButtons != value)
                {
                    this.showScrollButtons = value;
                    PerformLayout();
                    Invalidate(true);
                }
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
                Size itemSize = ItemSize;

                int itemsLength = itemSize.Width * this.items.Count;
                int viewLength = itemsLength - ClientSize.Width;
                int maxScrollOffset = Math.Max(0, viewLength);
                return maxScrollOffset;
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
                int clampedValue = Utility.Clamp(value, MinScrollOffset, MaxScrollOffset);

                if (this.scrollOffset != clampedValue)
                {
                    this.scrollOffset = clampedValue;
                    OnScrollOffsetChanged();
                    Invalidate(true);
                }
            }
        }

        public event EventHandler ScrollOffsetChanged;
        protected virtual void OnScrollOffsetChanged()
        {
            PerformLayout();

            if (ScrollOffsetChanged != null)
            {
                ScrollOffsetChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets the viewable area, in View coordinate space.
        /// </summary>
        public Rectangle ViewRectangle
        {
            get
            {
                Size itemSize = ItemSize;
                return new Rectangle(0, 0, itemSize.Width * ItemCount, itemSize.Height);
            }
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            int arrowWidth = UI.ScaleWidth(16);

            ScrollOffset = Utility.Clamp(this.scrollOffset, MinScrollOffset, MaxScrollOffset);

            // Determine arrow visibility / position
            this.leftScrollButton.Size = new Size(arrowWidth, ClientSize.Height);
            this.leftScrollButton.Location = new Point(0, 0);

            this.rightScrollButton.Size = new Size(arrowWidth, ClientSize.Height);
            this.rightScrollButton.Location = new Point(ClientSize.Width - this.rightScrollButton.Width, 0);

            bool showEitherButton = this.showScrollButtons && (this.ViewRectangle.Width > ClientRectangle.Width);
            bool showRightButton = (this.scrollOffset < MaxScrollOffset) && showEitherButton;
            bool showLeftButton = (this.scrollOffset > MinScrollOffset) && showEitherButton;

            this.rightScrollButton.Enabled = showRightButton;
            this.rightScrollButton.Visible = showRightButton;
            this.leftScrollButton.Enabled = showLeftButton;
            this.leftScrollButton.Visible = showLeftButton;

            base.OnLayout(levent);
        }

        public bool ShowCloseButtons
        {
            get
            {
                return this.showCloseButtons;
            }

            set
            {
                if (this.showCloseButtons != value)
                {
                    this.showCloseButtons = value;
                    PerformLayout();
                    Invalidate();
                }
            }
        }

        public int PreferredMinClientWidth
        {
            get
            {
                if (this.items.Count == 0)
                {
                    return 0;
                }

                int minWidth = ItemSize.Width;

                if (this.leftScrollButton.Visible || this.rightScrollButton.Visible)
                {
                    minWidth += this.leftScrollButton.Width;
                    minWidth += this.rightScrollButton.Width;
                }

                minWidth = Math.Min(minWidth, ViewRectangle.Width);

                return minWidth;
            }
        }

        public Size PreferredImageSize
        {
            get
            {
                Rectangle itemRect;
                Rectangle imageRect;

                MeasureItemPartRectangles(out itemRect, out imageRect);
                return new Size(imageRect.Width - imagePadding * 2, imageRect.Height - imagePadding * 2);
            }
        }

        public Size ItemSize
        {
            get
            {
                Rectangle itemRect;
                Rectangle imageRect;

                MeasureItemPartRectangles(out itemRect, out imageRect);
                return itemRect.Size;
            }
        }

        protected virtual void DrawItemBackground(Graphics g, Item item, Rectangle itemRect)
        {
        }

        protected virtual void DrawItemHighlight(
            Graphics g, 
            Item item, 
            Rectangle itemRect, 
            Rectangle highlightRect)
        {
            Color backFillColor;
            Color outlineColor;

            if (item.Checked)
            {
                backFillColor = Color.FromArgb(192, SystemColors.Highlight);
                outlineColor = backFillColor;
            }
            else if (item.Selected)
            {
                backFillColor = Color.FromArgb(64, SystemColors.HotTrack);
                outlineColor = Color.FromArgb(64, SystemColors.HotTrack);
            }
            else
            {
                backFillColor = Color.Transparent;
                outlineColor = Color.Transparent;
            }

            using (SolidBrush backFillBrush = new SolidBrush(backFillColor))
            {
                g.FillRectangle(backFillBrush, highlightRect);
            }

            using (Pen outlinePen = new Pen(outlineColor))
            {
                g.DrawRectangle(outlinePen, highlightRect.X, highlightRect.Y, highlightRect.Width - 1, highlightRect.Height - 1);
            }
        }

        protected virtual void DrawItemCloseButton(
            Graphics g, 
            Item item, 
            Rectangle itemRect, 
            Rectangle closeButtonRect)
        {
            if (item.Checked && item.Selected)
            {
                const string resourceNamePrefix = "Images.ImageStrip.CloseButton.";
                const string resourceNameSuffix = ".png";
                string resourceNameInfix = item.CloseRenderState.ToString();

                string resourceName = resourceNamePrefix + resourceNameInfix + resourceNameSuffix;

                ImageResource imageResource = PdnResources.GetImageResource(resourceName);
                Image image = imageResource.Reference;

                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                g.DrawImage(image, closeButtonRect, new Rectangle(0, 0, image.Width, image.Width), GraphicsUnit.Pixel);
            }
        }

        protected virtual void DrawItemDirtyOverlay(
            Graphics g,
            Item item,
            Rectangle itemRect,
            Rectangle dirtyOverlayRect)
        {
            Color outerPenColor = Color.White;
            Color innerPenColor = Color.Orange;

            const int xInset = 2;
            int scaledXInset = UI.ScaleWidth(xInset);

            const float outerPenWidth = 4.0f;
            const float innerPenWidth = 2.0f;

            float scaledOuterPenWidth = UI.ScaleWidth(outerPenWidth);
            float scaledInnerPenWidth = UI.ScaleWidth(innerPenWidth);

            SmoothingMode oldSM = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int left = dirtyOverlayRect.Left + scaledXInset;
            int top = dirtyOverlayRect.Top + scaledXInset;
            int right = dirtyOverlayRect.Right - scaledXInset;
            int bottom = dirtyOverlayRect.Bottom - scaledXInset;

            float r = Math.Min((right - left) / 2.0f, (bottom - top) / 2.0f);

            PointF centerPt = new PointF((left + right) / 2.0f, (top + bottom) / 2.0f);
            float twoPiOver5 = (float)(Math.PI * 0.4);

            PointF a = new PointF(centerPt.X + r * (float)Math.Sin(twoPiOver5), centerPt.Y - r * (float)Math.Cos(twoPiOver5));
            PointF b = new PointF(centerPt.X + r * (float)Math.Sin(2 * twoPiOver5), centerPt.Y - r * (float)Math.Cos(2 * twoPiOver5));
            PointF c = new PointF(centerPt.X + r * (float)Math.Sin(3 * twoPiOver5), centerPt.Y - r * (float)Math.Cos(3 * twoPiOver5));
            PointF d = new PointF(centerPt.X + r * (float)Math.Sin(4 * twoPiOver5), centerPt.Y - r * (float)Math.Cos(4 * twoPiOver5));
            PointF e = new PointF(centerPt.X + r * (float)Math.Sin(5 * twoPiOver5), centerPt.Y - r * (float)Math.Cos(5 * twoPiOver5));

            PointF[] lines =
                new PointF[]
                {
                    centerPt, a,
                    centerPt, b,
                    centerPt, c,
                    centerPt, d,
                    centerPt, e
                }; 

            using (Pen outerPen = new Pen(outerPenColor, scaledOuterPenWidth))
            {
                for (int i = 0; i < lines.Length; i += 2)
                {
                    g.DrawLine(outerPen, lines[i], lines[i + 1]);
                }
            }

            using (Pen innerPen = new Pen(innerPenColor, scaledInnerPenWidth))
            {
                for (int i = 0; i < lines.Length; i += 2)
                {
                    g.DrawLine(innerPen, lines[i], lines[i + 1]);
                }
            }

            g.SmoothingMode = oldSM;
        }

        protected virtual void DrawItemImageShadow(
            Graphics g,
            Item item,
            Rectangle itemRect,
            Rectangle imageRect,
            Rectangle imageInsetRect)
        {
            Rectangle shadowRect = Rectangle.Inflate(imageInsetRect, 1, 1);
            Utility.DrawDropShadow1px(g, shadowRect);
        }

        protected virtual void DrawItemImage(
            Graphics g, 
            Item item, 
            Rectangle itemRect, 
            Rectangle imageRect, 
            Rectangle imageInsetRect)
        {
            // Draw the image
            if (item.Image != null)
            {
                g.DrawImage(
                    item.Image,
                    imageInsetRect,
                    new Rectangle(0, 0, item.Image.Width, item.Image.Height),
                    GraphicsUnit.Pixel);
            }
        }

        private void DrawItem(Graphics g, Item item, Point offset)
        {
            Rectangle itemRect;
            Rectangle imageRect;
            Rectangle imageInsetRect;
            Rectangle closeButtonRect;
            Rectangle dirtyOverlayRect;

            MeasureItemPartRectangles(
                item, 
                out itemRect, 
                out imageRect, 
                out imageInsetRect,
                out closeButtonRect,
                out dirtyOverlayRect);

            itemRect.X += offset.X;
            itemRect.Y += offset.Y;

            imageRect.X += offset.X;
            imageRect.Y += offset.Y;

            imageInsetRect.X += offset.X;
            imageInsetRect.Y += offset.Y;

            closeButtonRect.X += offset.X;
            closeButtonRect.Y += offset.Y;

            dirtyOverlayRect.X += offset.X;
            dirtyOverlayRect.Y += offset.Y;

            DrawItemBackground(g, item, itemRect);

            Rectangle highlightRect = itemRect;
            DrawItemHighlight(g, item, itemRect, highlightRect);

            // Fill background and draw outline
            if (this.drawShadow)
            {
                DrawItemImageShadow(g, item, itemRect, imageRect, imageInsetRect);
            }

            DrawItemImage(g, item, itemRect, imageRect, imageInsetRect);

            if (this.showCloseButtons)
            {
                DrawItemCloseButton(g, item, itemRect, closeButtonRect);
            }

            if (this.drawDirtyOverlay && item.Dirty)
            {
                DrawItemDirtyOverlay(g, item, itemRect, dirtyOverlayRect);
            }
        }

        public Point ClientPointToViewPoint(Point clientPt)
        {
            int viewX = clientPt.X + this.scrollOffset;
            return new Point(viewX, clientPt.Y);
        }

        public Rectangle ClientRectangleToViewRectangle(Rectangle clientRect)
        {
            Point viewPt = ClientPointToViewPoint(clientRect.Location);
            return new Rectangle(viewPt, clientRect.Size);
        }

        public Point ViewPointToClientPoint(Point viewPt)
        {
            int clientX = viewPt.X - this.scrollOffset;
            return new Point(clientX, viewPt.Y);
        }

        public Rectangle ViewRectangleToClientRectangle(Rectangle viewRect)
        {
            Point clientPt = ViewPointToClientPoint(viewRect.Location);
            return new Rectangle(clientPt, viewRect.Size);
        }

        private Point ViewPointToItemPoint(int itemIndex, Point viewPt)
        {
            Rectangle itemRect = ItemIndexToItemViewRectangle(itemIndex);
            Point itemPt = new Point(viewPt.X - itemRect.X, viewPt.Y);
            return itemPt;
        }

        private Rectangle ItemIndexToItemViewRectangle(int itemIndex)
        {
            Size itemSize = ItemSize;
            return new Rectangle(itemSize.Width * itemIndex, itemSize.Height, itemSize.Width, itemSize.Height);
        }

        public int ViewPointToItemIndex(Point viewPt)
        {
            if (!ViewRectangle.Contains(viewPt))
            {
                return -1;
            }

            Size itemSize = ItemSize;
            int index = viewPt.X / itemSize.Width;

            return index;
        }

        private void MeasureItemPartRectangles(
            out Rectangle itemRect,
            out Rectangle imageRect)
        {
            itemRect = new Rectangle(
                0,
                0,
                ClientSize.Height,
                ClientSize.Height);

            imageRect = new Rectangle(
                itemRect.Left,
                itemRect.Top,
                itemRect.Width,
                itemRect.Width);
        }

        private void MeasureItemPartRectangles(
            Item item,
            out Rectangle itemRect,
            out Rectangle imageRect,
            out Rectangle imageInsetRect,
            out Rectangle closeButtonRect,
            out Rectangle dirtyOverlayRect)
        {
            MeasureItemPartRectangles(out itemRect, out imageRect);

            Rectangle imageInsetRectMax = new Rectangle(
                imageRect.Left + imagePadding,
                imageRect.Top + imagePadding,
                imageRect.Width - imagePadding * 2,
                imageRect.Height - imagePadding * 2);

            Size imageInsetSize;
            
            if (item.Image == null)
            {
                imageInsetSize = imageRect.Size;
            }
            else
            {
                imageInsetSize = Utility.ComputeThumbnailSize(item.Image.Size, imageInsetRectMax.Width);
            }

            int scaledCloseButtonLength = UI.ScaleWidth(closeButtonLength);
            int scaledCloseButtonPadding = UI.ScaleWidth(closeButtonPadding);

            imageInsetRect = new Rectangle(
                imageInsetRectMax.Left + (imageInsetRectMax.Width - imageInsetSize.Width) / 2,
                imageInsetRectMax.Bottom - imageInsetSize.Height,
                imageInsetSize.Width,
                imageInsetSize.Height);

            closeButtonRect = new Rectangle(
                imageInsetRectMax.Right - scaledCloseButtonLength - scaledCloseButtonPadding,
                imageInsetRectMax.Top + scaledCloseButtonPadding,
                scaledCloseButtonLength,
                scaledCloseButtonLength);

            dirtyOverlayRect = new Rectangle(
                imageInsetRectMax.Left + scaledCloseButtonPadding,
                imageInsetRectMax.Top + scaledCloseButtonPadding,
                scaledCloseButtonLength,
                scaledCloseButtonLength);
        }

        private ItemPart ItemPointToItemPart(Item item, Point pt)
        {
            Rectangle itemRect;
            Rectangle imageRect;
            Rectangle imageInsetRect;
            Rectangle closeButtonRect;
            Rectangle dirtyOverlayRect;

            MeasureItemPartRectangles(
                item,
                out itemRect,
                out imageRect,
                out imageInsetRect,
                out closeButtonRect,
                out dirtyOverlayRect);

            if (closeButtonRect.Contains(pt))
            {
                return ItemPart.CloseButton;
            }

            if (imageRect.Contains(pt))
            {
                return ItemPart.Image;
            }

            return ItemPart.None;
        }

        private Rectangle ItemIndexToClientRect(int itemIndex)
        {
            Size itemSize = ItemSize;

            Rectangle clientRect = new Rectangle(
                itemSize.Width * itemIndex,
                0,
                itemSize.Width,
                itemSize.Height);

            return clientRect;
        }

        private void CalculateVisibleScrollOffsets(
            int itemIndex,
            out int minOffset, 
            out int maxOffset,
            out int minFullyShownOffset,
            out int maxFullyShownOffset)
        {
            Rectangle itemClientRect = ItemIndexToClientRect(itemIndex);

            minOffset = itemClientRect.Left + 1 - ClientSize.Width;
            maxOffset = itemClientRect.Right - 1;
            minFullyShownOffset = itemClientRect.Right - ClientSize.Width;
            maxFullyShownOffset = itemClientRect.Left;

            if (this.leftScrollButton.Visible)
            {
                maxOffset -= this.leftScrollButton.Width;
                maxFullyShownOffset -= this.leftScrollButton.Width;
            }

            if (this.rightScrollButton.Visible)
            {
                minOffset += this.rightScrollButton.Width;
                minFullyShownOffset += this.rightScrollButton.Width;
            }
        }

        public Rectangle ScrolledViewRect
        {
            get
            {
                return new Rectangle(this.scrollOffset, 0, ClientSize.Width, ClientSize.Height);
            }
        }

        public bool IsItemVisible(int index)
        {
            Rectangle itemRect = ItemIndexToClientRect(index);
            Rectangle intersect = Rectangle.Intersect(itemRect, ScrolledViewRect);
            return (intersect.Width > 0 || intersect.Height > 0);
        }

        public bool IsItemFullyVisible(int index)
        {
            Rectangle itemRect = ItemIndexToClientRect(index);
            Rectangle svRect = ScrolledViewRect;

            if (this.leftScrollButton.Visible)
            {
                svRect.X += this.leftScrollButton.Width;
                svRect.Width -= this.leftScrollButton.Width;
            }

            if (this.rightScrollButton.Visible)
            {
                svRect.Width -= this.rightScrollButton.Width;
            }

            Rectangle intersect = Rectangle.Intersect(itemRect, svRect);
            return (intersect == itemRect);
        }

        public void EnsureItemFullyVisible(Item item)
        {
            int index = this.items.IndexOf(item);
            EnsureItemFullyVisible(index);
        }

        public void EnsureItemFullyVisible(int index)
        {
            if (IsItemFullyVisible(index))
            {
                return;
            }

            int minOffset;
            int maxOffset;
            int minFullyShownOffset;
            int maxFullyShownOffset;

            CalculateVisibleScrollOffsets(index, out minOffset, out maxOffset, 
                out minFullyShownOffset, out maxFullyShownOffset);

            // Pick the offset that moves the image the fewest number of pixels
            int oldOffset = this.scrollOffset;
            int dxMin = Math.Abs(oldOffset - minFullyShownOffset);
            int dxMax = Math.Abs(oldOffset - maxFullyShownOffset);

            if (dxMin <= dxMax)
            {
                this.ScrollOffset = minFullyShownOffset;
            }
            else
            {
                this.ScrollOffset = maxFullyShownOffset;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (UI.IsControlPaintingEnabled(this))
            {
                Size itemSize = ItemSize;
                Rectangle firstItemRect = new Rectangle(-this.scrollOffset, 0, itemSize.Width, itemSize.Height);

                for (int i = 0; i < this.items.Count; ++i)
                {
                    if (IsItemVisible(i))
                    {
                        Point itemOffset = new Point(firstItemRect.X + itemSize.Width * i, firstItemRect.Y);
                        DrawItem(e.Graphics, this.items[i], itemOffset);
                    }
                }
            }

            base.OnPaint(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (this.mouseDownButton == MouseButtons.None)
            {
                Point clientPt = new Point(e.X, e.Y);
                Point viewPt = ClientPointToViewPoint(clientPt);
                int itemIndex = ViewPointToItemIndex(viewPt);

                if (itemIndex >= 0 && itemIndex < this.items.Count)
                {
                    Item item = this.items[itemIndex];
                    Point itemPt = ViewPointToItemPoint(itemIndex, viewPt);
                    ItemPart itemPart = ItemPointToItemPart(item, itemPt);

                    if (itemPart == ItemPart.Image)
                    {
                        OnItemClicked(item, itemPart, e.Button);

                        this.mouseDownApplyRendering = false;
                        this.mouseOverIndex = itemIndex;
                        this.mouseOverItemPart = itemPart;
                        this.mouseOverApplyRendering = true;
                    }
                    else
                    {
                        this.mouseDownIndex = itemIndex;
                        this.mouseDownItemPart = itemPart;
                        this.mouseDownButton = e.Button;
                        this.mouseDownApplyRendering = true;
                        this.mouseOverApplyRendering = false;
                    }

                    MouseStatesToItemStates();
                    Refresh();
                }
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            GetFocus();

            Point clientPt = new Point(e.X, e.Y);

            if (clientPt != this.lastMouseMovePt)
            {
                Point viewPt = ClientPointToViewPoint(clientPt);
                int itemIndex = ViewPointToItemIndex(viewPt);

                if (this.mouseDownButton == MouseButtons.None)
                {
                    if (itemIndex >= 0 && itemIndex < this.items.Count)
                    {
                        Item item = this.items[itemIndex];
                        Point itemPt = ViewPointToItemPoint(itemIndex, viewPt);
                        ItemPart itemPart = ItemPointToItemPart(item, itemPt);

                        this.mouseOverIndex = itemIndex;
                        this.mouseOverItemPart = itemPart;
                        this.mouseOverApplyRendering = true;
                    }
                    else
                    {
                        this.mouseOverApplyRendering = false;
                    }
                }
                else
                {
                    this.mouseOverApplyRendering = false;

                    if (itemIndex != this.mouseDownIndex)
                    {
                        this.mouseDownApplyRendering = false;
                    }
                    else if (itemIndex < 0 || itemIndex >= this.items.Count)
                    {
                        this.mouseDownApplyRendering = false;
                    }
                    else
                    {
                        Item item = this.Items[itemIndex];
                        Point itemPt = ViewPointToItemPoint(itemIndex, viewPt);

                        ItemPart itemPart = ItemPointToItemPart(item, itemPt);

                        if (itemPart != this.mouseDownItemPart)
                        {
                            this.mouseDownApplyRendering = false;
                        }
                    }
                }

                MouseStatesToItemStates();
                Refresh();
            }

            this.lastMouseMovePt = clientPt;
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            bool raisedClickEvent = false;

            if (this.mouseDownButton == e.Button)
            {
                Point clientPt = new Point(e.X, e.Y);
                Point viewPt = ClientPointToViewPoint(clientPt);
                int itemIndex = ViewPointToItemIndex(viewPt);

                if (itemIndex >= 0 && itemIndex < this.items.Count)
                {
                    Item item = this.items[itemIndex];
                    Point itemPt = ViewPointToItemPoint(itemIndex, viewPt);
                    ItemPart itemPart = ItemPointToItemPart(item, itemPt);

                    if (itemIndex == this.mouseDownIndex && itemPart == this.mouseDownItemPart)
                    {
                        if (itemPart == ItemPart.CloseButton && !item.Checked)
                        {
                            // Can only close 'checked' images, just like how tab switching+closing works in IE7
                            itemPart = ItemPart.Image;
                        }

                        OnItemClicked(item, itemPart, this.mouseDownButton);
                        raisedClickEvent = true;
                    }

                    this.mouseOverApplyRendering = true;
                    this.mouseOverItemPart = itemPart;
                    this.mouseOverIndex = itemIndex;
                }

                this.mouseDownApplyRendering = false;
                this.mouseDownButton = MouseButtons.None;

                MouseStatesToItemStates();
                Refresh();
            }

            if (raisedClickEvent)
            {
                ForceMouseMove();
            }

            base.OnMouseUp(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            float count = (float)e.Delta / SystemInformation.MouseWheelScrollDelta;
            int pixels = (int)(count * ItemSize.Width);
            int newSO = ScrollOffset - pixels;
            ScrollOffset = newSO;

            ForceMouseMove();

            base.OnMouseWheel(e);
        }

        private void ForceMouseMove()
        {
            Point clientPt = PointToClient(Control.MousePosition);
            this.lastMouseMovePt = new Point(this.lastMouseMovePt.X + 1, this.lastMouseMovePt.Y + 1);
            MouseEventArgs me = new MouseEventArgs(MouseButtons.None, 0, clientPt.X, clientPt.Y, 0);
            OnMouseMove(me);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            GetFocus();
            base.OnMouseEnter(e);
        }

        private void GetFocus()
        {
            if (this.managedFocus && !MenuStripEx.IsAnyMenuActive && UI.IsOurAppActive)
            {
                this.Focus();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            this.mouseDownApplyRendering = false;
            this.mouseOverApplyRendering = false;

            MouseStatesToItemStates();
            Refresh();

            if (this.managedFocus && !MenuStripEx.IsAnyMenuActive && UI.IsOurAppActive)
            {
                OnRelinquishFocus();
            }

            base.OnMouseLeave(e);
        }       
    }
}
