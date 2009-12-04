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
using System.Drawing.Imaging;
using System.Threading;
using System.Windows.Forms;

namespace PaintDotNet
{
    internal class LayerElement 
        : UserControl
    {
        public static int ThumbSizePreScaling = 40;

        private Layer layer;
        private bool isSelected;
        private PropertyEventHandler layerPropertyChangedDelegate;
        private System.Windows.Forms.Label layerDescription;
        private System.Windows.Forms.PictureBox icon;
        private System.Windows.Forms.CheckBox layerVisible;
        private ThumbnailManager thumbnailManager;
        private int thumbnailSize = 16;
        private int suspendPreviewUpdates = 0;

        public ThumbnailManager ThumbnailManager
        {
            get
            {
                return this.thumbnailManager;
            }

            set
            {
                this.thumbnailManager = value;
            }
        }

        public int ThumbnailSize
        {
            get
            {
                return this.thumbnailSize;
            }

            set
            {
                if (this.thumbnailSize != value)
                {
                    this.thumbnailSize = value;
                    RefreshPreview();
                }
            }
        }

        public CheckBox LayerVisible
        {
            get
            {
                return this.layerVisible;
            }
        }

        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public bool IsSelected
        {
            get
            {
                return this.isSelected;
            }
            set
            {
                this.isSelected = value;

                if (this.isSelected)
                {
                    this.layerDescription.BackColor = SystemColors.Highlight;
                    this.layerDescription.ForeColor = SystemColors.HighlightText;
                    this.layerVisible.BackColor = this.layerDescription.BackColor;
                    this.icon.BackColor = SystemColors.Highlight;
                }
                else // !selected
                {               
                    this.layerDescription.ForeColor = SystemColors.WindowText;
                    this.layerDescription.BackColor = SystemColors.Window;
                    this.layerVisible.BackColor = this.layerDescription.BackColor;
                    this.icon.BackColor = SystemColors.Window;
                }

                Update();
            }
        }

        public Image Image
        {
            get
            {
                return this.icon.Image;
            }

            set
            {
                if (this.icon.Image != null)
                {
                    this.icon.Image.Dispose();
                    this.icon.Image = null;
                }

                this.icon.Image = value;
                Invalidate(true);
                Update();
            }
        }

        public Layer Layer 
        {
            get
            {
                return this.layer;
            }

            set
            {
                if (object.ReferenceEquals(this.layer, value))
                {
                    return;
                }

                if (this.layer != null)
                {
                    this.layer.PropertyChanged -= this.layerPropertyChangedDelegate;
                    this.layer.Invalidated -= new InvalidateEventHandler(Layer_Invalidated);
                }
                
                this.layer = value;

                if (this.layer != null)
                {
                    this.layer.PropertyChanged += this.layerPropertyChangedDelegate;
                    this.layer.Invalidated += new InvalidateEventHandler(Layer_Invalidated);
                    this.layerPropertyChangedDelegate(layer, new PropertyEventArgs("")); // sync up

                    // Add italics if it's the background layer
                    if (this.layer.IsBackground)
                    {
                        this.layerDescription.Font = new Font(this.layerDescription.Font.FontFamily, this.layerDescription.Font.Size, 
                            this.layerDescription.Font.Style | FontStyle.Italic);
                    }

                    RefreshPreview();
                }

                Update();
            }
        }
        
        public LayerElement()
        {
            // This call is required by the Windows.Forms Form Designer.
            this.SuspendLayout();
            InitializeComponent();
            InitializeComponent2();
            this.ResumeLayout(false);
            this.IsSelected = false;

            layerPropertyChangedDelegate = new PropertyEventHandler(LayerPropertyChangedHandler);

            this.TabStop = false;
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Layer = null;

                if (components != null)
                {
                    components.Dispose();
                    components = null;
                }
            }

            base.Dispose(disposing);
        }

        private void LayerPropertyChangedHandler(object sender, PropertyEventArgs e)
        {
            this.layerDescription.Text = layer.Name;
            this.layerVisible.Checked = layer.Visible;
        }

        private void InitializeComponent2()
        {
            this.Size = new System.Drawing.Size(200, SystemLayer.UI.ScaleWidth(LayerElement.ThumbSizePreScaling));
            this.icon.Size = new System.Drawing.Size(6 + this.Height, this.Height);
            this.layerDescription.Location = new System.Drawing.Point(this.icon.Right, 0);
            this.layerVisible.Size = new System.Drawing.Size(16, this.Height);
        }

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.layerDescription = new System.Windows.Forms.Label();
            this.icon = new System.Windows.Forms.PictureBox();
            this.layerVisible = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // layerDescription
            // 
            this.layerDescription.BackColor = SystemColors.Window;
            this.layerDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layerDescription.Name = "layerDescription";
            this.layerDescription.Size = new System.Drawing.Size(150, 50);
            this.layerDescription.TabIndex = 9;
            this.layerDescription.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.layerDescription.Click += new System.EventHandler(this.Control_Click);
            this.layerDescription.DoubleClick += new System.EventHandler(this.Control_DoubleClick);
            // 
            // icon
            // 
            this.icon.BackColor = System.Drawing.SystemColors.Control;
            this.icon.Dock = System.Windows.Forms.DockStyle.Left;
            this.icon.Location = new System.Drawing.Point(0, 0);
            this.icon.Name = "icon";
            this.icon.TabStop = false;
            this.icon.Click += new System.EventHandler(this.Control_Click);
            this.icon.DoubleClick += new System.EventHandler(this.Control_DoubleClick);
            // 
            // layerVisible
            // 
            this.layerVisible.BackColor = SystemColors.Window;
            this.layerVisible.Checked = true;
            this.layerVisible.CheckState = System.Windows.Forms.CheckState.Checked;
            this.layerVisible.Dock = System.Windows.Forms.DockStyle.Right;
            this.layerVisible.FlatStyle = System.Windows.Forms.FlatStyle.Standard;
            this.layerVisible.Location = new System.Drawing.Point(184, 0);
            this.layerVisible.Name = "layerVisible";
            this.layerVisible.TabIndex = 7;
            this.layerVisible.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.LayerVisible_KeyPress);
            this.layerVisible.CheckStateChanged += new System.EventHandler(this.LayerVisible_CheckStateChanged);
            this.layerVisible.KeyUp += new System.Windows.Forms.KeyEventHandler(this.LayerVisible_KeyUp);
            // 
            // LayerElement
            // 
            this.Controls.Add(this.layerDescription);
            this.Controls.Add(this.icon);
            this.Controls.Add(this.layerVisible);
            this.Name = "LayerElement";
            this.ResumeLayout(false);

        }
        #endregion

        private void Control_Click(object sender, System.EventArgs e)
        {
            OnClick(e);
        }

        private void Control_DoubleClick(object sender, System.EventArgs e)
        {
            OnDoubleClick(e);
        }

        private void LayerVisible_CheckStateChanged(object sender, System.EventArgs e)
        {
            this.layer.Visible = this.layerVisible.Checked;
            Update();
        }

        private void LayerVisible_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            this.OnKeyPress(e);
        }

        private void LayerVisible_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            this.OnKeyUp(e);
        }

        private void Layer_Invalidated(object sender, InvalidateEventArgs e)
        {
            RefreshPreview();
        }

        public void SuspendPreviewUpdates()
        {
            ++suspendPreviewUpdates;
        }

        public void ResumePreviewUpdates()
        {
            --suspendPreviewUpdates;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            RefreshPreview();
            base.OnHandleCreated(e);
        }

        public void RefreshPreview()
        {
            if (this.suspendPreviewUpdates > 0)
            {
                return;
            }

            if (!this.IsHandleCreated)
            {
                return;
            }

            this.thumbnailManager.QueueThumbnailUpdate(this.layer, this.thumbnailSize, OnThumbnailRendered);
        }

        private void OnThumbnailRendered(object sender, EventArgs<Pair<IThumbnailProvider, Surface>> e)
        {
            if (!IsDisposed)
            {
                Bitmap thumbBitmap = e.Data.Second.CreateAliasedBitmap();
                Bitmap bitmap = new Bitmap(this.icon.Width, this.icon.Height, PixelFormat.Format32bppArgb);

                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CompositingMode = CompositingMode.SourceCopy;
                    g.Clear(Color.Transparent);

                    Rectangle thumbRect = new Rectangle(
                        (bitmap.Width - thumbBitmap.Width) / 2,
                        (bitmap.Height - thumbBitmap.Height) / 2,
                        thumbBitmap.Width,
                        thumbBitmap.Height);

                    g.DrawImage(
                        thumbBitmap,
                        thumbRect,
                        new Rectangle(new Point(0, 0), thumbBitmap.Size),
                        GraphicsUnit.Pixel);

                    Rectangle outlineRect = thumbRect;
                    --outlineRect.X;
                    --outlineRect.Y;
                    ++outlineRect.Width;
                    ++outlineRect.Height;
                    g.DrawRectangle(Pens.Black, outlineRect);

                    g.CompositingMode = CompositingMode.SourceOver;

                    Rectangle dropShadowRect = outlineRect;
                    dropShadowRect.Inflate(1, 1);
                    ++dropShadowRect.Width;
                    ++dropShadowRect.Height;
                    Utility.DrawDropShadow1px(g, dropShadowRect);
                }

                thumbBitmap.Dispose();
                this.Image = bitmap;
            }
        }
    }
}
