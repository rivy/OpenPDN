/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Windows.Forms;

namespace PaintDotNet.Menus
{
#if false
    internal sealed class ToolsMenu
        : PdnMenuItem
    {
        private bool toolsListInit = false;
        private PdnMenuItem menuToolsAntialiasing;
        private PdnMenuItem menuToolsAlphaBlending;
        private ToolStripSeparator menuToolsSeperator;

        public ToolsMenu()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.menuToolsAntialiasing = new PdnMenuItem();
            this.menuToolsAlphaBlending = new PdnMenuItem();
            this.menuToolsSeperator = new ToolStripSeparator();
            // 
            // ToolsMenu
            //
            this.DropDownItems.AddRange(
                new ToolStripItem[]
                {
                    this.menuToolsAntialiasing,
                    this.menuToolsAlphaBlending,
                    this.menuToolsSeperator
                });
            this.Name = "Menu.Tools";
            this.Text = PdnResources.GetString("Menu.Tools.Text");
            // 
            // menuToolsAntiAliasing
            // 
            this.menuToolsAntialiasing.Name = "AntiAliasing";
            this.menuToolsAntialiasing.Click += new System.EventHandler(MenuToolsAntiAliasing_Click);
            //
            // menuToolsAlphaBlending
            //
            this.menuToolsAlphaBlending.Name = "AlphaBlending";
            this.menuToolsAlphaBlending.Click += new EventHandler(MenuToolsAlphaBlending_Click);
        }

        protected override void OnDropDownOpening(EventArgs e)
        {
            if (!this.toolsListInit)
            {
                this.DropDownItems.Clear();
                this.DropDownItems.Add(this.menuToolsAntialiasing);
                this.DropDownItems.Add(this.menuToolsAlphaBlending);
                this.DropDownItems.Add(this.menuToolsSeperator);

                foreach (ToolInfo toolInfo in DocumentWorkspace.ToolInfos)
                {
                    PdnMenuItem mi = new PdnMenuItem(toolInfo.Name, null, this.menuTools_ClickHandler);
                    mi.SetIcon(toolInfo.Image);
                    mi.Tag = toolInfo;
                    this.DropDownItems.Add(mi);
                }

                this.toolsListInit = true;
            }

            Type currentToolType;

            if (AppWorkspace.ActiveDocumentWorkspace != null)
            {
                currentToolType = AppWorkspace.ActiveDocumentWorkspace.GetToolType();
            }
            else
            {
                currentToolType = null;
            }

            foreach (ToolStripItem tsi in this.DropDownItems)
            {
                PdnMenuItem mi = tsi as PdnMenuItem;

                if (mi != null)
                {
                    ToolInfo toolInfo = mi.Tag as ToolInfo;

                    if (toolInfo != null)
                    {
                        if (toolInfo.ToolType == currentToolType)
                        {
                            mi.Checked = true;
                        }
                        else
                        {
                            mi.Checked = false;
                        }
                    }
                }
            }

            this.menuToolsAntialiasing.Checked = AppWorkspace.AppEnvironment.AntiAliasing;
            this.menuToolsAlphaBlending.Checked = AppWorkspace.AppEnvironment.AlphaBlending;

            base.OnDropDownOpening(e);
        }

        private void menuTools_ClickHandler(object sender, System.EventArgs e)
        {
            if (AppWorkspace.ActiveDocumentWorkspace != null)
            {
                PdnMenuItem mi = (PdnMenuItem)sender;
                ToolInfo toolInfo = (ToolInfo)mi.Tag;
                AppWorkspace.ActiveDocumentWorkspace.SetToolFromType(toolInfo.ToolType);
            }
        }

        private void MenuToolsAntiAliasing_Click(object sender, System.EventArgs e)
        {
            AppWorkspace.AppEnvironment.AntiAliasing = !AppWorkspace.AppEnvironment.AntiAliasing;
        }

        private void MenuToolsAlphaBlending_Click(object sender, EventArgs e)
        {
            AppWorkspace.AppEnvironment.AlphaBlending = !AppWorkspace.AppEnvironment.AlphaBlending;
        }
    }
#endif
}
