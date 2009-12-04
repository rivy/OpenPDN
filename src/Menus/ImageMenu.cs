/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.Actions;
using PaintDotNet.HistoryFunctions;
using System;
using System.Windows.Forms;

namespace PaintDotNet.Menus
{
    internal sealed class ImageMenu
        : PdnMenuItem
    {
        private PdnMenuItem menuImageCrop;
        private PdnMenuItem menuImageResize;
        private PdnMenuItem menuImageCanvasSize;
        private ToolStripSeparator menuImageSeparator1;
        private PdnMenuItem menuImageFlipHorizontal;
        private PdnMenuItem menuImageFlipVertical;
        private ToolStripSeparator menuImageSeparator2;
        private PdnMenuItem menuImageRotate90CW;
        private PdnMenuItem menuImageRotate90CCW;
        private PdnMenuItem menuImageRotate180;
        private ToolStripSeparator menuImageSeparator3;
        private PdnMenuItem menuImageFlatten;

        public ImageMenu()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.menuImageCrop = new PdnMenuItem();
            this.menuImageResize = new PdnMenuItem();
            this.menuImageCanvasSize = new PdnMenuItem();
            this.menuImageSeparator1 = new ToolStripSeparator();
            this.menuImageFlipHorizontal = new PdnMenuItem();
            this.menuImageFlipVertical = new PdnMenuItem();
            this.menuImageSeparator2 = new ToolStripSeparator();
            this.menuImageRotate90CW = new PdnMenuItem();
            this.menuImageRotate90CCW = new PdnMenuItem();
            this.menuImageRotate180 = new PdnMenuItem();
            this.menuImageSeparator3 = new ToolStripSeparator();
            this.menuImageFlatten = new PdnMenuItem();
            //
            // ImageMenu
            //
            this.DropDownItems.AddRange(
                new System.Windows.Forms.ToolStripItem[] 
                {
                    this.menuImageCrop,
                    this.menuImageResize,
                    this.menuImageCanvasSize,
                    this.menuImageSeparator1,
                    this.menuImageFlipHorizontal,
                    this.menuImageFlipVertical,
                    this.menuImageSeparator2,
                    this.menuImageRotate90CW,
                    this.menuImageRotate90CCW,
                    this.menuImageRotate180,
                    this.menuImageSeparator3,
                    this.menuImageFlatten 
                });
            this.Name = "Menu.Image";
            this.Text = PdnResources.GetString("Menu.Image.Text"); 
            // 
            // menuImageCrop
            // 
            this.menuImageCrop.Name = "Crop";
            this.menuImageCrop.Click += new System.EventHandler(this.MenuImageCrop_Click);
            this.menuImageCrop.ShortcutKeys = Keys.Control | Keys.Shift | Keys.X;
            // 
            // menuImageResize
            // 
            this.menuImageResize.Name = "Resize";
            this.menuImageResize.ShortcutKeys = Keys.Control | Keys.R;
            this.menuImageResize.Click += new System.EventHandler(this.MenuImageResize_Click);
            // 
            // menuImageCanvasSize
            // 
            this.menuImageCanvasSize.Name = "CanvasSize";
            this.menuImageCanvasSize.ShortcutKeys = Keys.Control | Keys.Shift | Keys.R;
            this.menuImageCanvasSize.Click += new System.EventHandler(this.MenuImageCanvasSize_Click);
            // 
            // menuImageFlipHorizontal
            // 
            this.menuImageFlipHorizontal.Name = "FlipHorizontal";
            this.menuImageFlipHorizontal.Click += new System.EventHandler(this.MenuImageFlipHorizontal_Click);
            // 
            // menuImageFlipVertical
            // 
            this.menuImageFlipVertical.Name = "FlipVertical";
            this.menuImageFlipVertical.Click += new System.EventHandler(this.MenuImageFlipVertical_Click);
            // 
            // menuImageRotate90CW
            // 
            this.menuImageRotate90CW.Name = "Rotate90CW";
            this.menuImageRotate90CW.ShortcutKeys = Keys.Control | Keys.H;
            this.menuImageRotate90CW.Click += new System.EventHandler(this.MenuImageRotate90CW_Click);
            // 
            // menuImageRotate90CCW
            // 
            this.menuImageRotate90CCW.Name = "Rotate90CCW";
            this.menuImageRotate90CCW.ShortcutKeys = Keys.Control | Keys.G;
            this.menuImageRotate90CCW.Click += new System.EventHandler(this.MenuImageRotate90CCW_Click);
            // 
            // menuImageRotate180
            // 
            this.menuImageRotate180.Name = "Rotate180";
            this.menuImageRotate180.Click += new System.EventHandler(this.MenuImageRotate180_Click);
            this.menuImageRotate180.ShortcutKeys = Keys.Control | Keys.J;
            // 
            // menuImageFlatten
            // 
            this.menuImageFlatten.Name = "Flatten";
            this.menuImageFlatten.ShortcutKeys = Keys.Control | Keys.Shift | Keys.F;
            this.menuImageFlatten.Click += new System.EventHandler(this.MenuImageFlatten_Click);
        }

        protected override void OnDropDownOpening(EventArgs e)
        {
            if (AppWorkspace.ActiveDocumentWorkspace == null)
            {
                this.menuImageCrop.Enabled = false;
                this.menuImageResize.Enabled = false;
                this.menuImageCanvasSize.Enabled = false;
                this.menuImageFlipHorizontal.Enabled = false;
                this.menuImageFlipVertical.Enabled = false;
                this.menuImageRotate90CW.Enabled = false;
                this.menuImageRotate90CCW.Enabled = false;
                this.menuImageRotate180.Enabled = false;
                this.menuImageFlatten.Enabled = false;
            }
            else
            {
                this.menuImageCrop.Enabled = !AppWorkspace.ActiveDocumentWorkspace.Selection.IsEmpty;
                this.menuImageResize.Enabled = true;
                this.menuImageCanvasSize.Enabled = true;
                this.menuImageFlipHorizontal.Enabled = true;
                this.menuImageFlipVertical.Enabled = true;
                this.menuImageRotate90CW.Enabled = true;
                this.menuImageRotate90CCW.Enabled = true;
                this.menuImageRotate180.Enabled = true;
                this.menuImageFlatten.Enabled = (AppWorkspace.ActiveDocumentWorkspace.Document.Layers.Count > 1);
            }

            base.OnDropDownOpening(e);
        }

        private void MenuImageCrop_Click(object sender, System.EventArgs e)
        {
            if (AppWorkspace.ActiveDocumentWorkspace != null)
            {
                if (!AppWorkspace.ActiveDocumentWorkspace.Selection.IsEmpty)
                {
                    AppWorkspace.ActiveDocumentWorkspace.ExecuteFunction(new CropToSelectionFunction());
                }
            }
        }

        private void MenuImageResize_Click(object sender, System.EventArgs e)
        {
            if (AppWorkspace.ActiveDocumentWorkspace != null)
            {
                AppWorkspace.ActiveDocumentWorkspace.PerformAction(new ResizeAction());
            }
        }

        private void MenuImageCanvasSize_Click(object sender, System.EventArgs e)
        {
            if (AppWorkspace.ActiveDocumentWorkspace != null)
            {
                AppWorkspace.ActiveDocumentWorkspace.PerformAction(new CanvasSizeAction());
            }
        }

        private void MenuImageFlipHorizontal_Click(object sender, System.EventArgs e)
        {
            if (AppWorkspace.ActiveDocumentWorkspace != null)
            {
                AppWorkspace.ActiveDocumentWorkspace.ExecuteFunction(new FlipDocumentHorizontalFunction());
            }
        }

        private void MenuImageFlipVertical_Click(object sender, System.EventArgs e)
        {
            if (AppWorkspace.ActiveDocumentWorkspace != null)
            {
                AppWorkspace.ActiveDocumentWorkspace.ExecuteFunction(new FlipDocumentVerticalFunction());
            }
        }

        private void MenuImageRotate90CW_Click(object sender, System.EventArgs e)
        {
            if (AppWorkspace.ActiveDocumentWorkspace != null)
            {
                HistoryFunction da = new RotateDocumentFunction(RotateType.Clockwise90);
                AppWorkspace.ActiveDocumentWorkspace.ExecuteFunction(da);
            }
        }

        private void MenuImageRotate90CCW_Click(object sender, System.EventArgs e)
        {
            if (AppWorkspace.ActiveDocumentWorkspace != null)
            {
                HistoryFunction da = new RotateDocumentFunction(RotateType.CounterClockwise90);
                AppWorkspace.ActiveDocumentWorkspace.ExecuteFunction(da);
            }
        }

        private void MenuImageRotate180_Click(object sender, System.EventArgs e)
        {
            if (AppWorkspace.ActiveDocumentWorkspace != null)
            {
                HistoryFunction da = new RotateDocumentFunction(RotateType.Rotate180);
                AppWorkspace.ActiveDocumentWorkspace.ExecuteFunction(da);
            }
        }

        private void MenuImageFlatten_Click(object sender, System.EventArgs e)
        {
            if (AppWorkspace.ActiveDocumentWorkspace != null)
            {
                if (AppWorkspace.ActiveDocumentWorkspace.Document.Layers.Count > 1)
                {
                    AppWorkspace.ActiveDocumentWorkspace.ExecuteFunction(new FlattenFunction());
                }
            }
        }
    }
}
