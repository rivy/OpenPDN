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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace PaintDotNet
{
    public sealed class ImageListMenu
        : Control
    {
        private ComboBox comboBox;
        private StringFormat stringFormat;
        private Size itemSize = Size.Empty;
        private Size maxImageSize = Size.Empty;
        private Bitmap backBuffer = null;

        // layout parameters
        private int imageXInset;
        private int imageYInset;
        private int textLeftMargin;
        private int textRightMargin;
        private int textVMargin;

        public sealed class Item
        {
            private Image image;
            private string name;
            private bool selected;
            private object tag;

            public Image Image
            {
                get
                {
                    return this.image;
                }
            }

            public string Name
            {
                get
                {
                    return this.name;
                }
            }

            public bool Selected
            {
                get
                {
                    return this.selected;
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
                }
            }

            public override string ToString()
            {
                return this.name;
            }

            public Item(Image image, string name, bool selected)
            {
                this.image = image;
                this.name = name;
                this.selected = selected;
            }
        }

        public event EventHandler<EventArgs<Item>> ItemClicked;
        private void OnItemClicked(Item item)
        {
            if (ItemClicked != null)
            {
                ItemClicked(this, new EventArgs<Item>(item));
            }
        }

        public event EventHandler Closed;
        private void OnClosed()
        {
            if (Closed != null)
            {
                Closed(this, EventArgs.Empty);
            }
        }

        public bool IsImageListVisible
        {
            get
            {
                return this.comboBox.DroppedDown;
            }
        }

        private void DetermineMaxItemSize(Graphics g, Item[] items, out Size maxItemSizeResult, out Size maxImageSizeResult)
        {
            // Find max image height and width
            int maxImageWidth = 0;
            int maxImageHeight = 0;
            int maxTextWidth = 0;
            int maxTextHeight = 0;

            foreach (Item item in items)
            {
                maxImageWidth = Math.Max(maxImageWidth, item.Image.Width);
                maxImageHeight = Math.Max(maxImageHeight, item.Image.Height);

                SizeF textSizeF = g.MeasureString(item.Name, Font, new PointF(0, 0), this.stringFormat);
                Size textSize = Size.Ceiling(textSizeF);

                maxTextWidth = Math.Max(textSize.Width, maxTextWidth);
                maxTextHeight = Math.Max(textSize.Height, maxTextHeight);
            }

            int maxItemWidth = this.imageXInset + maxImageWidth + this.imageXInset + this.textLeftMargin + maxTextWidth + this.textRightMargin;

            int maxItemHeight = Math.Max(
                this.imageYInset + maxImageHeight + this.imageYInset,
                this.textVMargin + maxTextHeight + this.textVMargin);

            maxItemSizeResult = new Size(maxItemWidth, maxItemHeight);
            maxImageSizeResult = new Size(maxImageWidth, maxImageHeight);
        }

        public void ShowImageList(Item[] items)
        {
            HideImageList();

            this.comboBox.Items.AddRange(items);

            using (Graphics g = CreateGraphics())
            {
                DetermineMaxItemSize(g, items, out this.itemSize, out this.maxImageSize);
            }

            this.comboBox.ItemHeight = this.itemSize.Height;
            this.comboBox.DropDownWidth = this.itemSize.Width + SystemInformation.VerticalScrollBarWidth + UI.ScaleWidth(2);

            // Determine the max drop down height so that we don't cover up the button
            Screen ourScreen = Screen.FromControl(this);
            Point screenLocation = PointToScreen(new Point(this.comboBox.Left, this.comboBox.Bottom));
            int comboBoxToFloorHeight = ourScreen.WorkingArea.Height - screenLocation.Y;

            // make sure it is an integral multiple of itemSize.Height
            comboBoxToFloorHeight = this.itemSize.Height * (comboBoxToFloorHeight / this.itemSize.Height);
            // add 2 pixels for border
            comboBoxToFloorHeight += 2;

            // But make sure it can hold at least 3 items
            int minDropDownHeight = 2 + itemSize.Height * 3; // +2 for combobox's border

            int dropDownHeight = Math.Max(comboBoxToFloorHeight, minDropDownHeight);

            this.comboBox.DropDownHeight = dropDownHeight;

            int selectedIndex = Array.FindIndex(
                items,
                delegate(Item item)
                {
                    return item.Selected;
                });

            this.comboBox.SelectedIndex = selectedIndex;

            // Make sure the combobox does not spill past the right edge of the screen
            int left = PointToScreen(new Point(0, Height)).X;
            if (left + this.comboBox.DropDownWidth > ourScreen.WorkingArea.Right)
            {
                left = ourScreen.WorkingArea.Right - this.comboBox.DropDownWidth;
            }

            Point clientPt = PointToClient(new Point(left, screenLocation.Y));
            SuspendLayout();
            this.comboBox.Left = clientPt.X;
            ResumeLayout(false);

            // Set focus to it so it can get mouse wheel events, and then show it!
            this.comboBox.Focus();
            UI.ShowComboBox(this.comboBox, true);
        }

        public void HideImageList()
        {
            UI.ShowComboBox(this.comboBox, false); 
        }

        public ImageListMenu()
        {
            UI.InitScaling(this);
            InitializeComponent();

            this.imageXInset = UI.ScaleWidth(2);
            this.imageYInset = UI.ScaleHeight(4);
            this.textLeftMargin = UI.ScaleWidth(4);
            this.textRightMargin = UI.ScaleWidth(16);
            this.textVMargin = UI.ScaleHeight(2);

            this.stringFormat = (StringFormat)StringFormat.GenericTypographic.Clone();
        }

        private void InitializeComponent()
        {
            this.comboBox = new ComboBox();
            this.comboBox.Name = "comboBox";
            this.comboBox.MeasureItem += new MeasureItemEventHandler(ComboBox_MeasureItem);
            this.comboBox.DrawItem += new DrawItemEventHandler(ComboBox_DrawItem);
            this.comboBox.DropDown += new EventHandler(ComboBox_DropDown);
            this.comboBox.DropDownClosed += new EventHandler(ComboBox_DropDownClosed);
            this.comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.comboBox.SelectionChangeCommitted += new EventHandler(ComboBox_SelectionChangeCommitted);

            this.comboBox.DrawMode = DrawMode.OwnerDrawFixed;
            this.comboBox.Visible = true;
            this.TabStop = false;
            this.Controls.Add(this.comboBox);
            this.Name = "ImageListMenu";
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.stringFormat != null)
                {
                    this.stringFormat.Dispose();
                    this.stringFormat = null;
                }

                if (this.backBuffer != null)
                {
                    this.backBuffer.Dispose();
                    this.backBuffer = null;
                }
            }

            base.Dispose(disposing);
        }

        private void ComboBox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            int index = this.comboBox.SelectedIndex;

            if (index >= 0 && index < this.comboBox.Items.Count)
            {
                OnItemClicked((Item)this.comboBox.Items[index]);
            }
        }

        private void ComboBox_DropDown(object sender, EventArgs e)
        {
            MenuStripEx.PushMenuActivate();
        }

        private void ComboBox_DropDownClosed(object sender, EventArgs e)
        {
            MenuStripEx.PopMenuActivate();
            this.comboBox.Items.Clear();
            OnClosed();
        }

        private void ComboBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index == -1)
            {
                return;
            }

            if (this.backBuffer != null && 
                (this.backBuffer.Width != e.Bounds.Width || this.backBuffer.Height != e.Bounds.Height))
            {
                this.backBuffer.Dispose();
                this.backBuffer = null;
            }

            if (this.backBuffer == null)
            {
                this.backBuffer = new Bitmap(e.Bounds.Width, e.Bounds.Height, PixelFormat.Format24bppRgb);
            }

            Item item = (Item)this.comboBox.Items[e.Index];

            if (item.Image.PixelFormat == PixelFormat.Undefined)
            {
                return;
            }

            using (Graphics g = Graphics.FromImage(this.backBuffer))
            {
                Brush backBrush;
                Brush textBrush;
                bool selected = (e.State & DrawItemState.Selected) != 0;

                if (selected)
                {
                    backBrush = new SolidBrush(Color.FromArgb(128, SystemColors.Highlight));
                    textBrush = SystemBrushes.HighlightText;
                }
                else
                {
                    backBrush = (Brush)SystemBrushes.Window.Clone();
                    textBrush = SystemBrushes.WindowText;
                }

                Rectangle bounds = new Rectangle(0, 0, this.backBuffer.Width, this.backBuffer.Height);

                g.FillRectangle(SystemBrushes.Window, bounds);

                g.FillRectangle(backBrush, bounds);

                Rectangle imageRect = new Rectangle(
                        this.imageXInset + (this.maxImageSize.Width - item.Image.Width) / 2,
                        this.imageYInset + (this.maxImageSize.Height - item.Image.Height) / 2,
                        item.Image.Width,
                        item.Image.Height);

                g.DrawImage(
                    item.Image,
                    imageRect,
                    new Rectangle(0, 0, item.Image.Width, item.Image.Height),
                    GraphicsUnit.Pixel);

                Utility.DrawDropShadow1px(g, Rectangle.Inflate(imageRect, 1, 1));

                SizeF textSizeF = e.Graphics.MeasureString(item.Name, Font, new PointF(0, 0), this.stringFormat);
                Size textSize = Size.Ceiling(textSizeF);

                g.DrawString(
                    item.Name,
                    Font,
                    textBrush,
                    this.imageXInset + this.maxImageSize.Width + this.imageXInset + this.textLeftMargin,
                    (this.itemSize.Height - textSize.Height) / 2);

                backBrush.Dispose();
                backBrush = null;
            }

            CompositingMode oldCM = e.Graphics.CompositingMode;
            e.Graphics.CompositingMode = CompositingMode.SourceCopy;

            e.Graphics.DrawImage(
                this.backBuffer, 
                e.Bounds, 
                new Rectangle(0, 0, this.backBuffer.Width, this.backBuffer.Height), 
                GraphicsUnit.Pixel);

            e.Graphics.CompositingMode = oldCM;
        }

        private void ComboBox_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            e.ItemWidth = this.itemSize.Width;
            e.ItemHeight = this.itemSize.Height;
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            this.comboBox.Location = new Point(0, -this.comboBox.Height);
            base.OnLayout(levent);
        }
    }
}
