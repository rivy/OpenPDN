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
using System.Windows.Forms;

namespace PaintDotNet
{
    internal class ToolsControl 
        : UserControl, 
          IToolChooser
    {
        private ToolStripEx toolStripEx;
        private ImageList imageList;
        private const int tbWidth = 2; // two buttons per line in the toolbars
        private int ignoreToolClicked = 0;
        private Control onePxSpacingLeft;

        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public ToolsControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
        }

        public event ToolClickedEventHandler ToolClicked;
        protected virtual void OnToolClicked(Type toolType)
        {
            if (this.ignoreToolClicked <= 0)
            {
                if (ToolClicked != null)
                {
                    ToolClicked(this, new ToolClickedEventArgs(toolType));
                }
            }
        }

        public void SetTools(ToolInfo[] toolInfos)
        {
            if (this.toolStripEx != null)
            {
                this.toolStripEx.Items.Clear();
            }

            this.imageList = new ImageList();
            this.imageList.ColorDepth = ColorDepth.Depth32Bit;
            this.imageList.TransparentColor = Utility.TransparentKey;

            this.toolStripEx.ImageList = this.imageList;

            ToolStripItem[] buttons = new ToolStripItem[toolInfos.Length];
            string toolTipFormat = PdnResources.GetString("ToolsControl.ToolToolTip.Format");

            for (int i = 0; i < toolInfos.Length; ++i)
            {
                ToolInfo toolInfo = toolInfos[i];
                ToolStripButton button = new ToolStripButton();

                int imageIndex = imageList.Images.Add(
                    toolInfo.Image.Reference,
                    imageList.TransparentColor);

                button.ImageIndex = imageIndex;
                button.Tag = toolInfo.ToolType;
                button.ToolTipText = string.Format(toolTipFormat, toolInfo.Name, char.ToUpperInvariant(toolInfo.HotKey).ToString());
                buttons[i] = button;
            }

            this.toolStripEx.Items.AddRange(buttons);
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
                foreach (ToolStripButton button in this.toolStripEx.Items)
                {
                    if ((Type)button.Tag == toolType)
                    {
                        this.ToolStripEx_ItemClicked(this, new ToolStripItemClickedEventArgs(button));
                        return;
                    }
                }

                throw new ArgumentException("Tool type not found");
            }

            finally
            {
                if (!raiseEvent)
                {
                    --this.ignoreToolClicked;
                }
            }
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            int buttonWidth;

            if (this.toolStripEx.Items.Count > 0)
            {
                buttonWidth = this.toolStripEx.Items[0].Width;
            }
            else
            {
                buttonWidth = 0;
            }

            this.toolStripEx.Width =
                this.toolStripEx.Padding.Left +
                (buttonWidth * tbWidth) +
                (this.toolStripEx.Margin.Horizontal * tbWidth) +
                this.toolStripEx.Padding.Right;

            this.toolStripEx.Height = this.toolStripEx.GetPreferredSize(this.toolStripEx.Size).Height;

            this.Width = this.toolStripEx.Width + this.onePxSpacingLeft.Width;
            this.Height = this.toolStripEx.Height;

            base.OnLayout(e);
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                    components = null;
                }
            }

            base.Dispose(disposing);
        }

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.toolStripEx = new ToolStripEx();
            this.onePxSpacingLeft = new Control();
            this.SuspendLayout();
            //
            // toolStripEx
            //
            this.toolStripEx.Dock = System.Windows.Forms.DockStyle.Top;
            this.toolStripEx.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripEx.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.toolStripEx.ItemClicked += new ToolStripItemClickedEventHandler(ToolStripEx_ItemClicked);
            this.toolStripEx.Name = "toolStripEx";
            this.toolStripEx.AutoSize = true;
            this.toolStripEx.RelinquishFocus += new EventHandler(ToolStripEx_RelinquishFocus);
            //
            // onePxSpacingLeft
            //
            this.onePxSpacingLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.onePxSpacingLeft.Width = 1;
            this.onePxSpacingLeft.Name = "onePxSpacingLeft";
            // 
            // MainToolBar
            // 
            this.Controls.Add(this.toolStripEx);
            this.Controls.Add(this.onePxSpacingLeft);
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.Name = "MainToolBar";
            this.Size = new System.Drawing.Size(48, 328);
            this.ResumeLayout(false);
        }

        public event EventHandler RelinquishFocus;
        private void OnRelinquishFocus()
        {
            if (RelinquishFocus != null)
            {
                RelinquishFocus(this, EventArgs.Empty);
            }
        }

        private void ToolStripEx_RelinquishFocus(object sender, EventArgs e)
        {
            OnRelinquishFocus();
        }

        private void ToolStripEx_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            foreach (ToolStripButton button in this.toolStripEx.Items)
            {
                button.Checked = (button == e.ClickedItem);
            }

            OnToolClicked((Type)e.ClickedItem.Tag);
        }
    }
}

