/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.Actions;
using PaintDotNet.Effects;
using PaintDotNet.HistoryFunctions;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PaintDotNet.Menus
{
    internal sealed class LayersMenu
        : PdnMenuItem
    {
        private PdnMenuItem menuLayersAddNewLayer;
        private PdnMenuItem menuLayersDeleteLayer;
        private PdnMenuItem menuLayersDuplicateLayer;
        private PdnMenuItem menuLayersMergeLayerDown;
        private PdnMenuItem menuLayersImportFromFile;
        private ToolStripSeparator menuLayersSeparator1;
        private PdnMenuItem menuLayersFlipHorizontal;
        private PdnMenuItem menuLayersFlipVertical;
        private PdnMenuItem menuLayersRotateZoom;
        private ToolStripSeparator menuLayersSeparator2;
        private PdnMenuItem menuLayersLayerProperties;

        public LayersMenu()
        {
            InitializeComponent();

            // Fill in Rotate/Zoom menu item
            string rzName = RotateZoomEffect.StaticName;
            Keys rzShortcut = RotateZoomEffect.StaticShortcutKeys;
            ImageResource rzImage = RotateZoomEffect.StaticImage;
            string rzNameFormatString = PdnResources.GetString("Effects.Name.Format.Configurable");
            string rzMenuName = string.Format(rzNameFormatString, rzName);

            this.menuLayersRotateZoom.Text = rzMenuName;
            this.menuLayersRotateZoom.SetIcon(rzImage);
            this.menuLayersRotateZoom.ShortcutKeys = rzShortcut;
        }

        private void InitializeComponent()
        {
            this.menuLayersAddNewLayer = new PdnMenuItem();
            this.menuLayersDeleteLayer = new PdnMenuItem();
            this.menuLayersDuplicateLayer = new PdnMenuItem();
            this.menuLayersMergeLayerDown = new PdnMenuItem();
            this.menuLayersImportFromFile = new PdnMenuItem();
            this.menuLayersSeparator1 = new ToolStripSeparator();
            this.menuLayersFlipHorizontal = new PdnMenuItem();
            this.menuLayersFlipVertical = new PdnMenuItem();
            this.menuLayersRotateZoom = new PdnMenuItem();
            this.menuLayersSeparator2 = new ToolStripSeparator();
            this.menuLayersLayerProperties = new PdnMenuItem();
            //
            // LayersMenu
            //
            this.DropDownItems.AddRange(
                new ToolStripItem[]
                {
                    this.menuLayersAddNewLayer,
                    this.menuLayersDeleteLayer,
                    this.menuLayersDuplicateLayer,
                    this.menuLayersMergeLayerDown,
                    this.menuLayersImportFromFile,
                    this.menuLayersSeparator1,
                    this.menuLayersFlipHorizontal,
                    this.menuLayersFlipVertical,
                    this.menuLayersRotateZoom,
                    this.menuLayersSeparator2,
                    this.menuLayersLayerProperties
                });                
            this.Name = "Menu.Layers";
            this.Text = PdnResources.GetString("Menu.Layers.Text");
            // 
            // menuLayersAddNewLayer
            // 
            this.menuLayersAddNewLayer.Name = "AddNewLayer";
            this.menuLayersAddNewLayer.ShortcutKeys = Keys.Control | Keys.Shift | Keys.N;
            this.menuLayersAddNewLayer.Click += new System.EventHandler(this.MenuLayersAddNewLayer_Click);
            // 
            // menuLayersDeleteLayer
            // 
            this.menuLayersDeleteLayer.Name = "DeleteLayer";
            this.menuLayersDeleteLayer.ShortcutKeys = Keys.Control | Keys.Shift | Keys.Delete;
            this.menuLayersDeleteLayer.Click += new System.EventHandler(this.MenuLayersDeleteLayer_Click);
            // 
            // menuLayersDuplicateLayer
            // 
            this.menuLayersDuplicateLayer.Name = "DuplicateLayer";
            this.menuLayersDuplicateLayer.ShortcutKeys = Keys.Control | Keys.Shift | Keys.D;
            this.menuLayersDuplicateLayer.Click += new System.EventHandler(this.MenuLayersDuplicateLayer_Click);
            //
            // menuLayersMergeDown
            //
            this.menuLayersMergeLayerDown.Name = "MergeLayerDown";
            this.menuLayersMergeLayerDown.ShortcutKeys = Keys.Control | Keys.M;
            this.menuLayersMergeLayerDown.Click += new EventHandler(MenuLayersMergeDown_Click);
            // 
            // menuLayersImportFromFile
            // 
            this.menuLayersImportFromFile.Name = "ImportFromFile";
            this.menuLayersImportFromFile.Click += new System.EventHandler(this.MenuLayersImportFromFile_Click);
            // 
            // menuLayersFlipHorizontal
            // 
            this.menuLayersFlipHorizontal.Name = "FlipHorizontal";
            this.menuLayersFlipHorizontal.Click += new System.EventHandler(this.MenuLayersFlipHorizontal_Click);
            // 
            // menuLayersFlipVertical
            // 
            this.menuLayersFlipVertical.Name = "FlipVertical";
            this.menuLayersFlipVertical.Click += new System.EventHandler(this.MenuLayersFlipVertical_Click);
            //
            // menuLayersRotateZoom
            //
            this.menuLayersRotateZoom.Name = "RotateZoom";
            this.menuLayersRotateZoom.Click += new EventHandler(MenuLayersRotateZoom_Click);
            // 
            // menuLayersLayerProperties
            // 
            this.menuLayersLayerProperties.Name = "LayerProperties";
            this.menuLayersLayerProperties.ShortcutKeys = Keys.F4;
            this.menuLayersLayerProperties.Click += new System.EventHandler(this.MenuLayersLayerProperties_Click);             
        }

        protected override void OnDropDownOpening(EventArgs e)
        {
            bool enabled = (AppWorkspace.ActiveDocumentWorkspace != null);

            this.menuLayersAddNewLayer.Enabled = enabled;

            if (AppWorkspace.ActiveDocumentWorkspace != null &&
                AppWorkspace.ActiveDocumentWorkspace.Document != null &&
                AppWorkspace.ActiveDocumentWorkspace.Document.Layers.Count > 1)
            {
                this.menuLayersDeleteLayer.Enabled = true;
            }
            else
            {
                this.menuLayersDeleteLayer.Enabled = false;
            }

            this.menuLayersDuplicateLayer.Enabled = enabled;

            bool enableMergeDown = (AppWorkspace.ActiveDocumentWorkspace != null &&
                AppWorkspace.ActiveDocumentWorkspace.ActiveLayerIndex > 0);

            this.menuLayersMergeLayerDown.Enabled = enableMergeDown;

            this.menuLayersImportFromFile.Enabled = enabled;
            this.menuLayersFlipHorizontal.Enabled = enabled;
            this.menuLayersFlipVertical.Enabled = enabled;
            this.menuLayersRotateZoom.Enabled = enabled;
            this.menuLayersLayerProperties.Enabled = enabled;

            base.OnDropDownOpening(e);
        }

        private void MenuLayersAddNewLayer_Click(object sender, System.EventArgs e)
        {
            if (AppWorkspace.ActiveDocumentWorkspace != null)
            {
                AppWorkspace.ActiveDocumentWorkspace.ExecuteFunction(new AddNewBlankLayerFunction());
            }
        }

        private void MenuLayersDuplicateLayer_Click(object sender, System.EventArgs e)
        {
            AppWorkspace.Widgets.LayerForm.PerformDuplicateLayerClick();
        }

        private void MenuLayersMergeDown_Click(object sender, EventArgs e)
        {
            if (AppWorkspace.ActiveDocumentWorkspace != null &&
                AppWorkspace.ActiveDocumentWorkspace.ActiveLayerIndex > 0)
            {
                // TODO: keep this in sync with AppWorkspace. not appropriate to refactor into an Action for a 'dot' release
                int newLayerIndex = Utility.Clamp(
                    AppWorkspace.ActiveDocumentWorkspace.ActiveLayerIndex - 1,
                    0,
                    AppWorkspace.ActiveDocumentWorkspace.Document.Layers.Count - 1);

                AppWorkspace.ActiveDocumentWorkspace.ExecuteFunction(
                    new MergeLayerDownFunction(AppWorkspace.ActiveDocumentWorkspace.ActiveLayerIndex));

                AppWorkspace.ActiveDocumentWorkspace.ActiveLayerIndex = newLayerIndex;
            }
        }

        private void MenuLayersDeleteLayer_Click(object sender, System.EventArgs e)
        {
            AppWorkspace.Widgets.LayerForm.PerformDeleteLayerClick();
        }

        private void MenuLayersFlipHorizontal_Click(object sender, System.EventArgs e)
        {
            if (this.AppWorkspace.ActiveDocumentWorkspace != null)
            {
                this.AppWorkspace.ActiveDocumentWorkspace.ExecuteFunction(
                    new FlipLayerHorizontalFunction(this.AppWorkspace.ActiveDocumentWorkspace.ActiveLayerIndex));
            }
        }

        private void MenuLayersFlipVertical_Click(object sender, System.EventArgs e)
        {
            if (this.AppWorkspace.ActiveDocumentWorkspace != null)
            {
                this.AppWorkspace.ActiveDocumentWorkspace.ExecuteFunction(
                    new FlipLayerVerticalFunction(this.AppWorkspace.ActiveDocumentWorkspace.ActiveLayerIndex));
            }
        }

        private void MenuLayersLayerProperties_Click(object sender, System.EventArgs e)
        {
            if (AppWorkspace.ActiveDocumentWorkspace != null)
            {
                AppWorkspace.Widgets.LayerForm.PerformPropertiesClick();
            }
        }

        private void MenuLayersImportFromFile_Click(object sender, System.EventArgs e)
        {
            if (AppWorkspace.ActiveDocumentWorkspace != null)
            {
                AppWorkspace.ActiveDocumentWorkspace.PerformAction(new ImportFromFileAction());
            }
        }

        private void MenuLayersRotateZoom_Click(object sender, EventArgs e)
        {
            if (AppWorkspace.ActiveDocumentWorkspace != null)
            {
                AppWorkspace.RunEffect(typeof(RotateZoomEffect));
            }
        }
    }
}