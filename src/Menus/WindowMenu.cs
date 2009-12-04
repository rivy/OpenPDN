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
using System.Windows.Forms;

namespace PaintDotNet.Menus
{
    internal sealed class WindowMenu
        : PdnMenuItem
    {
        private PdnMenuItem menuWindowResetWindowLocations;
        private ToolStripSeparator menuWindowSeperator1;
        private PdnMenuItem menuWindowTranslucent;
        private ToolStripSeparator menuWindowSeperator2;
        private PdnMenuItem menuWindowTools;
        private PdnMenuItem menuWindowHistory;
        private PdnMenuItem menuWindowLayers;
        private PdnMenuItem menuWindowColors;
        private ToolStripSeparator menuWindowSeparator3;
        private PdnMenuItem menuWindowOpenMdiList;
        private PdnMenuItem menuWindowNextTab;
        private PdnMenuItem menuWindowPreviousTab;
        
        public WindowMenu()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.menuWindowResetWindowLocations = new PdnMenuItem();
            this.menuWindowSeperator1 = new ToolStripSeparator();
            this.menuWindowTranslucent = new PdnMenuItem();
            this.menuWindowSeperator2 = new ToolStripSeparator();
            this.menuWindowTools = new PdnMenuItem();
            this.menuWindowHistory = new PdnMenuItem();
            this.menuWindowLayers = new PdnMenuItem();
            this.menuWindowColors = new PdnMenuItem();
            this.menuWindowSeparator3 = new ToolStripSeparator();
            this.menuWindowOpenMdiList = new PdnMenuItem();
            this.menuWindowNextTab = new PdnMenuItem();
            this.menuWindowPreviousTab = new PdnMenuItem();
            //
            // WindowMenu
            //
            this.DropDownItems.AddRange(
                new ToolStripItem[] 
                {
                    this.menuWindowResetWindowLocations,
                    this.menuWindowSeperator1,
                    this.menuWindowTranslucent,
                    this.menuWindowSeperator2,
                    this.menuWindowTools,
                    this.menuWindowHistory,
                    this.menuWindowLayers,
                    this.menuWindowColors,
                    this.menuWindowSeparator3,
                    this.menuWindowOpenMdiList,
                    this.menuWindowNextTab,
                    this.menuWindowPreviousTab
                });
            this.Name = "Menu.Window";
            this.Text = PdnResources.GetString("Menu.Window.Text");
            // 
            // menuWindowResetWindowLocations
            // 
            this.menuWindowResetWindowLocations.Name = "ResetWindowLocations";
            this.menuWindowResetWindowLocations.Click += new System.EventHandler(this.MenuWindowResetWindowLocations_Click);
            // 
            // menuWindowTranslucent
            // 
            this.menuWindowTranslucent.Name = "Translucent";
            this.menuWindowTranslucent.Click += new System.EventHandler(this.MenuWindowTranslucent_Click);
            // 
            // menuWindowTools
            // 
            this.menuWindowTools.Name = "Tools";
            this.menuWindowTools.ShortcutKeys = Keys.F5;
            this.menuWindowTools.Click += new System.EventHandler(this.MenuWindowTools_Click);
            // 
            // menuWindowHistory
            // 
            this.menuWindowHistory.Name = "History";
            this.menuWindowHistory.ShortcutKeys = Keys.F6;
            this.menuWindowHistory.Click += new System.EventHandler(this.MenuWindowHistory_Click);
            // 
            // menuWindowLayers
            // 
            this.menuWindowLayers.Name = "Layers";
            this.menuWindowLayers.ShortcutKeys = Keys.F7;
            this.menuWindowLayers.Click += new System.EventHandler(this.MenuWindowLayers_Click);
            // 
            // menuWindowColors
            // 
            this.menuWindowColors.Name = "Colors";
            this.menuWindowColors.ShortcutKeys = Keys.F8;
            this.menuWindowColors.Click += new System.EventHandler(this.MenuWindowColors_Click);
            //
            // menuWindowOpenMdiList
            //
            this.menuWindowOpenMdiList.Name = "OpenMdiList";
            this.menuWindowOpenMdiList.ShortcutKeys = Keys.Control | Keys.Q;
            this.menuWindowOpenMdiList.Click += new EventHandler(MenuWindowOpenMdiList_Click);
            //
            // menuWindowNextTab
            //
            this.menuWindowNextTab.Name = "NextTab";
            this.menuWindowNextTab.ShortcutKeys = Keys.Control | Keys.Tab;
            this.menuWindowNextTab.Click += new EventHandler(MenuWindowNextTab_Click);
            //
            // menuWindowPreviousTab
            //
            this.menuWindowPreviousTab.Name = "PreviousTab";
            this.menuWindowPreviousTab.ShortcutKeys = Keys.Control | Keys.Shift | Keys.Tab;
            this.menuWindowPreviousTab.Click += new EventHandler(MenuWindowPreviousTab_Click);
        }

        private void MenuWindowOpenMdiList_Click(object sender, EventArgs e)
        {
            this.AppWorkspace.ToolBar.ShowDocumentList();
        }

        private void MenuWindowPreviousTab_Click(object sender, EventArgs e)
        {
            this.AppWorkspace.ToolBar.DocumentStrip.PreviousTab();
        }

        private void MenuWindowNextTab_Click(object sender, EventArgs e)
        {
            this.AppWorkspace.ToolBar.DocumentStrip.NextTab();
        }

        protected override void OnDropDownOpening(EventArgs e)
        {
            this.menuWindowTranslucent.Checked = PdnBaseForm.EnableOpacity;
            this.menuWindowTools.Checked = AppWorkspace.Widgets.ToolsForm.Visible;
            this.menuWindowHistory.Checked = AppWorkspace.Widgets.HistoryForm.Visible;
            this.menuWindowLayers.Checked = AppWorkspace.Widgets.LayerForm.Visible;
            this.menuWindowColors.Checked = AppWorkspace.Widgets.ColorsForm.Visible;

            if (UserSessions.IsRemote)
            {
                this.menuWindowTranslucent.Enabled = false;
                this.menuWindowTranslucent.Checked = false;
            }

            this.menuWindowOpenMdiList.Enabled = (AppWorkspace.DocumentWorkspaces.Length > 0);

            bool pluralDocuments = (AppWorkspace.DocumentWorkspaces.Length > 1);
            this.menuWindowNextTab.Enabled = pluralDocuments;
            this.menuWindowPreviousTab.Enabled = pluralDocuments;

            base.OnDropDownOpening(e);
        }

        private void ToggleFormVisibility(FloatingToolForm ftf)
        {
            ftf.Visible = !ftf.Visible;

            if (AppWorkspace.ActiveDocumentWorkspace != null)
            {
                AppWorkspace.ActiveDocumentWorkspace.Focus();
            }
        }

        private void MenuWindowTools_Click(object sender, System.EventArgs e)
        {
            ToggleFormVisibility(AppWorkspace.Widgets.ToolsForm);
        }

        private void MenuWindowHistory_Click(object sender, System.EventArgs e)
        {
            ToggleFormVisibility(AppWorkspace.Widgets.HistoryForm);
        }

        private void MenuWindowLayers_Click(object sender, System.EventArgs e)
        {
            ToggleFormVisibility(AppWorkspace.Widgets.LayerForm);
        }

        private void MenuWindowColors_Click(object sender, System.EventArgs e)
        {
            ToggleFormVisibility(AppWorkspace.Widgets.ColorsForm);
        }

        private void MenuWindowResetWindowLocations_Click(object sender, System.EventArgs e)
        {
            AppWorkspace.ResetFloatingForms();
            AppWorkspace.Widgets.ToolsForm.Visible = true;
            AppWorkspace.Widgets.HistoryForm.Visible = true;
            AppWorkspace.Widgets.LayerForm.Visible = true;
            AppWorkspace.Widgets.ColorsForm.Visible = true;
        }

        private void MenuWindowTranslucent_Click(object sender, System.EventArgs e)
        {
            PdnBaseForm.EnableOpacity = !PdnBaseForm.EnableOpacity;
        }
    }
}
