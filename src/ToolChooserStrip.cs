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
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PaintDotNet
{
    internal class ToolChooserStrip
        : ToolStripEx,
          IToolChooser
    {
        private ToolStripSplitButton chooseToolButton;
        private int ignoreToolClicked = 0;
        private ToolInfo[] toolInfos = null;
        private Type activeTool = null;
        private bool showChooseDefaults = true;
        private bool useToolNameForLabel = false;
        private string chooseToolLabelText;

        public ToolChooserStrip()
        {
            this.chooseToolLabelText = PdnResources.GetString("ToolStripChooser.ChooseToolButton.Text");
            InitializeComponent();
        }

        public bool ShowChooseDefaults
        {
            get
            {
                return this.showChooseDefaults;
            }

            set
            {
                this.showChooseDefaults = value;
            }
        }

        public bool UseToolNameForLabel
        {
            get
            {
                return this.useToolNameForLabel;
            }

            set
            {
                this.useToolNameForLabel = value;
                SetToolButtonLabel();
            }
        }

        private void SetToolButtonLabel()
        {
            if (!this.useToolNameForLabel)
            {
                this.chooseToolButton.TextImageRelation = TextImageRelation.TextBeforeImage;
                this.chooseToolButton.Text = this.chooseToolLabelText;
            }
            else
            {
                this.chooseToolButton.TextImageRelation = TextImageRelation.ImageBeforeText;
                ToolInfo ti = null;

                if (this.toolInfos != null)
                {
                    ti = Array.Find(
                        this.toolInfos,
                        delegate(ToolInfo check)
                        {
                            return (check.ToolType == this.activeTool);
                        });
                }

                if (ti == null)
                {
                    this.chooseToolButton.Text = string.Empty;
                }
                else
                {
                    this.chooseToolButton.Text = ti.Name;
                }
            }
        }

        public event EventHandler ChooseDefaultsClicked;
        protected virtual void OnChooseDefaultsClicked()
        {
            if (ChooseDefaultsClicked != null)
            {
                ChooseDefaultsClicked(this, EventArgs.Empty);
            }
        }

        private void InitializeComponent()
        {
            this.chooseToolButton = new ToolStripSplitButton();
            this.SuspendLayout();
            //
            // chooseToolButton
            //
            this.chooseToolButton.Name = "chooseToolButton";
            this.chooseToolButton.Text = this.chooseToolLabelText;
            this.chooseToolButton.TextImageRelation = TextImageRelation.TextBeforeImage;
            this.chooseToolButton.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            this.chooseToolButton.DropDownOpening += new EventHandler(ChooseToolButton_DropDownOpening);
            this.chooseToolButton.DropDownClosed += new EventHandler(ChooseToolButton_DropDownClosed);
            this.chooseToolButton.DropDownItemClicked += new ToolStripItemClickedEventHandler(ChooseToolButton_DropDownItemClicked);
            this.chooseToolButton.Click +=
                delegate(object sender, EventArgs e)
                {
                    Tracing.LogFeature("ToolChooserStrip(chooseToolButton.Click)");
                    this.chooseToolButton.ShowDropDown();
                };
            //
            // ToolChooserStrip
            //
            this.Items.Add(new ToolStripSeparator());
            this.Items.Add(this.chooseToolButton);
            this.ResumeLayout(false);            
        }

        private void ChooseToolButton_DropDownClosed(object sender, EventArgs e)
        {
            this.chooseToolButton.DropDownItems.Clear();
        }
        
        private void ChooseToolButton_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            ToolInfo ti = e.ClickedItem.Tag as ToolInfo;

            if (ti != null)
            {
                Tracing.LogFeature("ToolChooserStrip(itemClicked(" + ti.ToolType.GetType().FullName + "))");
                OnToolClicked(ti.ToolType);
            }
        }

        private void ChooseTool_Click(object sender, EventArgs e)
        {
            OnChooseDefaultsClicked();
        }

        private void ChooseToolButton_DropDownOpening(object sender, EventArgs e)
        {
            this.chooseToolButton.DropDownItems.Clear();

            if (this.showChooseDefaults)
            {
                string chooseToolText = PdnResources.GetString("ToolChooserStrip.ChooseToolDefaults.Text");
                ImageResource chooseToolIcon = PdnResources.GetImageResource("Icons.MenuLayersLayerPropertiesIcon.png");

                ToolStripMenuItem tsmi = new ToolStripMenuItem(
                    chooseToolText,
                    chooseToolIcon.Reference,
                    ChooseTool_Click);

                this.chooseToolButton.DropDownItems.Add(tsmi);
                this.chooseToolButton.DropDownItems.Add(new ToolStripSeparator());
            }

            for (int i = 0; i < this.toolInfos.Length; ++i)
            {
                ToolStripMenuItem toolMI = new ToolStripMenuItem();
                toolMI.Image = this.toolInfos[i].Image.Reference;
                toolMI.Text = this.toolInfos[i].Name;
                toolMI.Tag = this.toolInfos[i];

                if (this.toolInfos[i].ToolType == this.activeTool)
                {
                    toolMI.Checked = true;
                }
                else
                {
                    toolMI.Checked = false;
                }

                this.chooseToolButton.DropDownItems.Add(toolMI);
            }
        }

        public void SelectTool(Type toolType)
        {
            SelectTool(toolType, true);
        }

        public void SelectTool(Type toolType, bool raiseEvent)
        {
            if (!raiseEvent)
            {
                ++this.ignoreToolClicked;
            }

            try
            {
                if (toolType != this.activeTool)
                {
                    foreach (ToolInfo ti in this.toolInfos)
                    {
                        if (ti.ToolType == toolType)
                        {
                            this.chooseToolButton.Image = ti.Image.Reference;
                            this.activeTool = toolType;
                            SetToolButtonLabel();
                            break;
                        }
                    }
                }
            }

            finally
            {
                if (!raiseEvent)
                {
                    --this.ignoreToolClicked;
                }
            }
        }

        public void SetTools(ToolInfo[] newToolInfos)
        {
            this.toolInfos = newToolInfos;
            SetToolButtonLabel();
        }

        public event ToolClickedEventHandler ToolClicked;
        protected virtual void OnToolClicked(Type toolType)
        {
            if (this.ignoreToolClicked <= 0)
            {
                SetToolButtonLabel();

                if (ToolClicked != null)
                {
                    ToolClicked(this, new ToolClickedEventArgs(toolType));
                }
            }
        }
    }
}
