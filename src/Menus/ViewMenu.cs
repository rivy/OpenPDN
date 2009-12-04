/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.Actions;
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace PaintDotNet.Menus
{
    internal sealed class ViewMenu
        : PdnMenuItem
    {
        private PdnMenuItem menuViewZoomIn;
        private PdnMenuItem menuViewZoomOut;
        private PdnMenuItem menuViewZoomToWindow;
        private PdnMenuItem menuViewZoomToSelection;
        private PdnMenuItem menuViewActualSize;
        private ToolStripSeparator menuViewSeparator1;
        private PdnMenuItem menuViewGrid;
        private PdnMenuItem menuViewRulers;
        private ToolStripSeparator menuViewSeparator2;
        private PdnMenuItem menuViewPixels;
        private PdnMenuItem menuViewInches;
        private PdnMenuItem menuViewCentimeters;

        private bool OnOemPlusShortcut(Keys keys)
        {
            this.menuViewZoomIn.PerformClick();
            return true;
        }

        private bool OnOemMinusShortcut(Keys keys)
        {
            this.menuViewZoomOut.PerformClick();
            return true;
        }

        private bool OnCtrlAltZero(Keys keys)
        {
            this.menuViewActualSize.PerformClick();
            return true;
        }

        public ViewMenu()
        {
            InitializeComponent();
            PdnBaseForm.RegisterFormHotKey(Keys.Control | Keys.OemMinus, OnOemMinusShortcut);
            PdnBaseForm.RegisterFormHotKey(Keys.Control | Keys.Oemplus, OnOemPlusShortcut);
            PdnBaseForm.RegisterFormHotKey(Keys.Control | Keys.Alt | Keys.D0, OnCtrlAltZero);
        }

        private void InitializeComponent()
        {
            this.menuViewZoomIn = new PdnMenuItem();
            this.menuViewZoomOut = new PdnMenuItem();
            this.menuViewZoomToWindow = new PdnMenuItem();
            this.menuViewZoomToSelection = new PdnMenuItem();
            this.menuViewActualSize = new PdnMenuItem();
            this.menuViewSeparator1 = new ToolStripSeparator();
            this.menuViewGrid = new PdnMenuItem();
            this.menuViewRulers = new PdnMenuItem();
            this.menuViewSeparator2 = new ToolStripSeparator();
            this.menuViewPixels = new PdnMenuItem();
            this.menuViewInches = new PdnMenuItem();
            this.menuViewCentimeters = new PdnMenuItem();
            // 
            // menuView
            // 
            this.DropDownItems.AddRange(
                new System.Windows.Forms.ToolStripItem[] 
                {
                    this.menuViewZoomIn,
                    this.menuViewZoomOut,
                    this.menuViewZoomToWindow,
                    this.menuViewZoomToSelection,
                    this.menuViewActualSize,
                    this.menuViewSeparator1,
                    this.menuViewGrid,
                    this.menuViewRulers,
                    this.menuViewSeparator2,
                    this.menuViewPixels,
                    this.menuViewInches,
                    this.menuViewCentimeters,
                });
            this.Name = "Menu.View";
            this.Text = PdnResources.GetString("Menu.View.Text"); 
            // 
            // menuViewZoomIn
            // 
            this.menuViewZoomIn.Name = "ZoomIn";
            this.menuViewZoomIn.ShortcutKeys = Keys.Control | Keys.Add;
            this.menuViewZoomIn.ShortcutKeyDisplayString = PdnResources.GetString("Menu.View.ZoomIn.ShortcutKeyDisplayString");
            this.menuViewZoomIn.Click += new System.EventHandler(this.MenuViewZoomIn_Click);
            // 
            // menuViewZoomOut
            // 
            this.menuViewZoomOut.Name = "ZoomOut";
            this.menuViewZoomOut.ShortcutKeys = Keys.Control | Keys.Subtract;
            this.menuViewZoomOut.ShortcutKeyDisplayString = PdnResources.GetString("Menu.View.ZoomOut.ShortcutKeyDisplayString");
            this.menuViewZoomOut.Click += new System.EventHandler(this.MenuViewZoomOut_Click);
            // 
            // menuViewZoomToWindow
            // 
            this.menuViewZoomToWindow.Name = "ZoomToWindow";
            this.menuViewZoomToWindow.ShortcutKeys = Keys.Control | Keys.B;
            this.menuViewZoomToWindow.Click += new System.EventHandler(this.MenuViewZoomToWindow_Click);
            // 
            // menuViewZoomToSelection
            // 
            this.menuViewZoomToSelection.Name = "ZoomToSelection";
            this.menuViewZoomToSelection.ShortcutKeys = Keys.Control | Keys.Shift | Keys.B;
            this.menuViewZoomToSelection.Click += new System.EventHandler(this.MenuViewZoomToSelection_Click);
            // 
            // menuViewActualSize
            // 
            this.menuViewActualSize.Name = "ActualSize";
            this.menuViewActualSize.ShortcutKeys = Keys.Control | Keys.Shift | Keys.A;
            this.menuViewActualSize.Click += new System.EventHandler(this.MenuViewActualSize_Click);
            // 
            // menuViewGrid
            // 
            this.menuViewGrid.Name = "Grid";
            this.menuViewGrid.Click += new System.EventHandler(this.MenuViewGrid_Click);
            // 
            // menuViewRulers
            // 
            this.menuViewRulers.Name = "Rulers";
            this.menuViewRulers.Click += new System.EventHandler(this.MenuViewRulers_Click);
            //
            // menuViewPixels
            //
            this.menuViewPixels.Name = "Pixels";
            this.menuViewPixels.Click += new EventHandler(MenuViewPixels_Click);
            this.menuViewPixels.Text = PdnResources.GetString("MeasurementUnit.Pixel.Plural");
            //
            // menuViewInches
            //
            this.menuViewInches.Name = "Inches";
            this.menuViewInches.Text = PdnResources.GetString("MeasurementUnit.Inch.Plural");
            this.menuViewInches.Click += new EventHandler(MenuViewInches_Click);
            //
            // menuViewCentimeters
            //
            this.menuViewCentimeters.Name = "Centimeters";
            this.menuViewCentimeters.Click += new EventHandler(MenuViewCentimeters_Click);
            this.menuViewCentimeters.Text = PdnResources.GetString("MeasurementUnit.Centimeter.Plural");
        }

        protected override void OnDropDownOpening(EventArgs e)
        {
            this.menuViewPixels.Checked = false;
            this.menuViewInches.Checked = false;
            this.menuViewCentimeters.Checked = false;

            switch (AppWorkspace.Units)
            {
                case MeasurementUnit.Pixel:
                    this.menuViewPixels.Checked = true;
                    break;

                case MeasurementUnit.Inch:
                    this.menuViewInches.Checked = true;
                    break;

                case MeasurementUnit.Centimeter:
                    this.menuViewCentimeters.Checked = true;
                    break;

                default:
                    throw new InvalidEnumArgumentException();
            }

            if (AppWorkspace.ActiveDocumentWorkspace != null)
            {
                this.menuViewZoomIn.Enabled = true;
                this.menuViewZoomOut.Enabled = true;
                this.menuViewZoomToWindow.Enabled = true;
                this.menuViewZoomToSelection.Enabled = !AppWorkspace.ActiveDocumentWorkspace.Selection.IsEmpty;
                this.menuViewActualSize.Enabled = true;
                this.menuViewGrid.Enabled = true;
                this.menuViewRulers.Enabled = true;
                this.menuViewPixels.Enabled = true;
                this.menuViewInches.Enabled = true;
                this.menuViewCentimeters.Enabled = true;

                this.menuViewZoomToWindow.Checked = (AppWorkspace.ActiveDocumentWorkspace.ZoomBasis == ZoomBasis.FitToWindow);
                this.menuViewGrid.Checked = AppWorkspace.ActiveDocumentWorkspace.DrawGrid;
                this.menuViewRulers.Checked = AppWorkspace.ActiveDocumentWorkspace.RulersEnabled;
            }
            else
            {
                this.menuViewZoomIn.Enabled = false;
                this.menuViewZoomOut.Enabled = false;
                this.menuViewZoomToWindow.Enabled = false;
                this.menuViewZoomToSelection.Enabled = false;
                this.menuViewActualSize.Enabled = false;
                this.menuViewGrid.Enabled = false;
                this.menuViewRulers.Enabled = false;
                this.menuViewPixels.Enabled = true;
                this.menuViewInches.Enabled = true;
                this.menuViewCentimeters.Enabled = true;
            }

            base.OnDropDownOpening(e);
        }

        private void MenuViewZoomIn_Click(object sender, System.EventArgs e)
        {
            if (AppWorkspace.ActiveDocumentWorkspace != null)
            {
                AppWorkspace.ActiveDocumentWorkspace.PerformAction(new ZoomInAction());
            }
        }

        private void MenuViewZoomOut_Click(object sender, System.EventArgs e)
        {
            if (AppWorkspace.ActiveDocumentWorkspace != null)
            {
                AppWorkspace.ActiveDocumentWorkspace.PerformAction(new ZoomOutAction());
            }
        }

        private void MenuViewZoomToWindow_Click(object sender, EventArgs e)
        {
            if (AppWorkspace.ActiveDocumentWorkspace != null)
            {
                AppWorkspace.ActiveDocumentWorkspace.PerformAction(new ZoomToWindowAction());
            }
        }

        private void MenuViewZoomToSelection_Click(object sender, EventArgs e)
        {
            if (AppWorkspace.ActiveDocumentWorkspace != null)
            {
                AppWorkspace.ActiveDocumentWorkspace.PerformAction(new ZoomToSelectionAction());
            }
        }

        private void MenuViewPixels_Click(object sender, EventArgs e)
        {
            AppWorkspace.Units = MeasurementUnit.Pixel;
        }

        private void MenuViewInches_Click(object sender, EventArgs e)
        {
            AppWorkspace.Units = MeasurementUnit.Inch;
        }

        private void MenuViewCentimeters_Click(object sender, EventArgs e)
        {
            AppWorkspace.Units = MeasurementUnit.Centimeter;
        }

        private void MenuViewActualSize_Click(object sender, System.EventArgs e)
        {
            if (AppWorkspace.ActiveDocumentWorkspace != null)
            {
                AppWorkspace.ActiveDocumentWorkspace.ZoomBasis = ZoomBasis.ScaleFactor;
                AppWorkspace.ActiveDocumentWorkspace.ScaleFactor = ScaleFactor.OneToOne;
            }
        }

        private void MenuViewRulers_Click(object sender, System.EventArgs e)
        {
            if (AppWorkspace.ActiveDocumentWorkspace != null)
            {
                AppWorkspace.ActiveDocumentWorkspace.RulersEnabled = !AppWorkspace.ActiveDocumentWorkspace.RulersEnabled;
            }
        }

        private void MenuViewGrid_Click(object sender, System.EventArgs e)
        {
            if (AppWorkspace.ActiveDocumentWorkspace != null)
            {
                AppWorkspace.ActiveDocumentWorkspace.DrawGrid = !AppWorkspace.ActiveDocumentWorkspace.DrawGrid;
            }
        }
    }
}
