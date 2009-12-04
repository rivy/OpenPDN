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
    internal sealed class PdnMainMenu
        : MenuStripEx
    {
        private FileMenu fileMenu;
        private EditMenu editMenu;
        private ViewMenu viewMenu;
        private ImageMenu imageMenu;
        private LayersMenu layersMenu;
        private AdjustmentsMenu adjustmentsMenu;
        private EffectsMenu effectsMenu;
        private WindowMenu windowMenu;
        private HelpMenu helpMenu;
        private AppWorkspace appWorkspace;

        public void CheckForUpdates()
        {
            this.helpMenu.CheckForUpdates();
        }

        public void RunEffect(Type effectType)
        {
            // TODO: this is kind of a hack
            this.adjustmentsMenu.RunEffect(effectType);
        }

        public AppWorkspace AppWorkspace
        {
            get
            {
                return this.appWorkspace;
            }

            set
            {
                this.appWorkspace = value;
                this.fileMenu.AppWorkspace = value;
                this.editMenu.AppWorkspace = value;
                this.viewMenu.AppWorkspace = value;
                this.imageMenu.AppWorkspace = value;
                this.layersMenu.AppWorkspace = value;
                this.adjustmentsMenu.AppWorkspace = value;
                this.effectsMenu.AppWorkspace = value;
                this.windowMenu.AppWorkspace = value;
                this.helpMenu.AppWorkspace = value;
            }
        }

        public void PopulateEffects()
        {
            this.adjustmentsMenu.PopulateEffects();
            this.effectsMenu.PopulateEffects();
        }

        public PdnMainMenu()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.fileMenu = new FileMenu();
            this.editMenu = new EditMenu();
            this.viewMenu = new ViewMenu();
            this.imageMenu = new ImageMenu();
            this.adjustmentsMenu = new AdjustmentsMenu();
            this.effectsMenu = new EffectsMenu();
            this.layersMenu = new LayersMenu();
            this.windowMenu = new WindowMenu();
            this.helpMenu = new HelpMenu();
            SuspendLayout();
            //
            // PdnMainMenu
            //
            this.Name = "PdnMainMenu";
            this.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.Items.AddRange(
                new ToolStripItem[] 
                {
                    this.fileMenu,
                    this.editMenu,
                    this.viewMenu,
                    this.imageMenu,
                    this.layersMenu,
                    this.adjustmentsMenu,
                    this.effectsMenu,
                    this.windowMenu,
                    this.helpMenu
                });
            ResumeLayout();
        }
    }
}
