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
using System.Globalization;
using System.Windows.Forms;

namespace PaintDotNet
{
    internal class ChooseToolDefaultsDialog
        : PdnBaseForm
    {
        private Button cancelButton;
        private Button saveButton;
        private Label introText;
        private Label defaultToolText;
        private Button resetButton;
        private Button loadFromToolBarButton;
        private ToolChooserStrip toolChooserStrip;
        private Type toolType = Tool.DefaultToolType;
        private AppEnvironment toolBarAppEnvironment;
        private Type toolBarToolType;
        private HeaderLabel bottomSeparator;
        private List<ToolConfigRow> toolConfigRows = new List<ToolConfigRow>();

        private sealed class ToolConfigRow
        {
            private ToolConfigStrip toolConfigStrip;
            private HeaderLabel headerLabel;
            private ToolBarConfigItems toolBarConfigItems;

            public ToolBarConfigItems ToolBarConfigItems
            {
                get
                {
                    return this.toolBarConfigItems;
                }
            }

            public HeaderLabel HeaderLabel
            {
                get
                {
                    return this.headerLabel;
                }
            }

            public ToolConfigStrip ToolConfigStrip
            {
                get
                {
                    return this.toolConfigStrip;
                }
            }

            private string GetHeaderResourceName()
            {
                string resName1 = this.toolBarConfigItems.ToString();
                string resName2 = resName1.Replace(", ", "");
                return "ChooseToolDefaultsDialog.ToolConfigRow." + resName2 + ".HeaderLabel.Text";
            }

            public ToolConfigRow(ToolBarConfigItems toolBarConfigItems)
            {
                this.toolBarConfigItems = toolBarConfigItems;

                this.headerLabel = new HeaderLabel();
                this.headerLabel.Name = "headerLabel:" + toolBarConfigItems.ToString();
                this.headerLabel.Text = PdnResources.GetString(GetHeaderResourceName());
                this.headerLabel.RightMargin = 0;

                this.toolConfigStrip = new ToolConfigStrip();
                this.toolConfigStrip.Name = "toolConfigStrip:" + toolBarConfigItems.ToString();
                this.toolConfigStrip.AutoSize = true;
                this.toolConfigStrip.Dock = DockStyle.None;
                this.toolConfigStrip.GripStyle = ToolStripGripStyle.Hidden;
                this.toolConfigStrip.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
                this.toolConfigStrip.ToolBarConfigItems = this.toolBarConfigItems;
            }
        }

        public void SetToolBarSettings(Type newToolType, AppEnvironment newToolBarAppEnvironment)
        {
            this.toolBarToolType = newToolType;
            this.toolBarAppEnvironment = newToolBarAppEnvironment.Clone();
        }

        public void LoadUIFromAppEnvironment(AppEnvironment newAppEnvironment)
        {
            SuspendLayout();

            foreach (ToolConfigRow row in this.toolConfigRows)
            {
                row.ToolConfigStrip.LoadFromAppEnvironment(newAppEnvironment);
            }

            ResumeLayout();
        }

        public void SetDefaultToolType(Type newDefaultToolType)
        {
            this.toolChooserStrip.SelectTool(newDefaultToolType);
        }

        public AppEnvironment CreateAppEnvironmentFromUI()
        {
            AppEnvironment newAppEnvironment = new AppEnvironment();

            foreach (ToolConfigRow row in this.toolConfigRows)
            {
                if ((row.ToolBarConfigItems & ToolBarConfigItems.AlphaBlending) != 0)
                {
                    newAppEnvironment.AlphaBlending = row.ToolConfigStrip.AlphaBlending;
                }

                if ((row.ToolBarConfigItems & ToolBarConfigItems.Antialiasing) != 0)
                {
                    newAppEnvironment.AntiAliasing = row.ToolConfigStrip.AntiAliasing;
                }

                if ((row.ToolBarConfigItems & ToolBarConfigItems.Brush) != 0)
                {
                    newAppEnvironment.BrushInfo = row.ToolConfigStrip.BrushInfo;
                }

                if ((row.ToolBarConfigItems & ToolBarConfigItems.ColorPickerBehavior) != 0)
                {
                    newAppEnvironment.ColorPickerClickBehavior = row.ToolConfigStrip.ColorPickerClickBehavior;
                }

                if ((row.ToolBarConfigItems & ToolBarConfigItems.FloodMode) != 0)
                {
                    newAppEnvironment.FloodMode = row.ToolConfigStrip.FloodMode;
                }

                if ((row.ToolBarConfigItems & ToolBarConfigItems.Gradient) != 0)
                {
                    newAppEnvironment.GradientInfo = row.ToolConfigStrip.GradientInfo;
                }

                if ((row.ToolBarConfigItems & ToolBarConfigItems.Pen) != 0 ||
                    (row.ToolBarConfigItems & ToolBarConfigItems.PenCaps) != 0)
                {
                    newAppEnvironment.PenInfo = row.ToolConfigStrip.PenInfo;
                }

                if ((row.ToolBarConfigItems & ToolBarConfigItems.Resampling) != 0)
                {
                    newAppEnvironment.ResamplingAlgorithm = row.ToolConfigStrip.ResamplingAlgorithm;
                }

                if ((row.ToolBarConfigItems & ToolBarConfigItems.SelectionCombineMode) != 0)
                {
                    newAppEnvironment.SelectionCombineMode = row.ToolConfigStrip.SelectionCombineMode;
                }

                if ((row.ToolBarConfigItems & ToolBarConfigItems.SelectionDrawMode) != 0)
                {
                    newAppEnvironment.SelectionDrawModeInfo = row.ToolConfigStrip.SelectionDrawModeInfo;
                }

                if ((row.ToolBarConfigItems & ToolBarConfigItems.ShapeType) != 0)
                {
                    newAppEnvironment.ShapeDrawType = row.ToolConfigStrip.ShapeDrawType;
                }

                if ((row.ToolBarConfigItems & ToolBarConfigItems.Text) != 0)
                {
                    newAppEnvironment.FontInfo = row.ToolConfigStrip.FontInfo;
                    newAppEnvironment.FontSmoothing = row.ToolConfigStrip.FontSmoothing;
                    newAppEnvironment.TextAlignment = row.ToolConfigStrip.FontAlignment;
                }

                if ((row.ToolBarConfigItems & ToolBarConfigItems.Tolerance) != 0)
                {
                    newAppEnvironment.Tolerance = row.ToolConfigStrip.Tolerance;
                }
            }

            return newAppEnvironment;
        }

        public Type ToolType
        {
            get
            {
                return this.toolType;
            }

            set
            {
                this.toolChooserStrip.SelectTool(value);
                this.toolType = value;
            }
        }

        public ChooseToolDefaultsDialog()
        {
            UI.InitScaling(this);
            SuspendLayout();

            InitializeComponent();

            this.toolConfigRows.Add(new ToolConfigRow(ToolBarConfigItems.ShapeType | ToolBarConfigItems.Brush | ToolBarConfigItems.Pen | ToolBarConfigItems.PenCaps));
            this.toolConfigRows.Add(new ToolConfigRow(ToolBarConfigItems.SelectionCombineMode | ToolBarConfigItems.SelectionDrawMode));
            this.toolConfigRows.Add(new ToolConfigRow(ToolBarConfigItems.Text));
            this.toolConfigRows.Add(new ToolConfigRow(ToolBarConfigItems.Gradient));
            this.toolConfigRows.Add(new ToolConfigRow(ToolBarConfigItems.Tolerance | ToolBarConfigItems.FloodMode));
            this.toolConfigRows.Add(new ToolConfigRow(ToolBarConfigItems.ColorPickerBehavior));
            this.toolConfigRows.Add(new ToolConfigRow(ToolBarConfigItems.Resampling));
            this.toolConfigRows.Add(new ToolConfigRow(ToolBarConfigItems.AlphaBlending | ToolBarConfigItems.Antialiasing));

            for (int i = 0; i < this.toolConfigRows.Count; ++i)
            {
                Controls.Add(this.toolConfigRows[i].HeaderLabel);
                Controls.Add(this.toolConfigRows[i].ToolConfigStrip);
            }

            ResumeLayout();
            PerformLayout();

            this.toolChooserStrip.SetTools(DocumentWorkspace.ToolInfos);

            PdnBaseForm.RegisterFormHotKey(
                Keys.Escape,
                delegate(Keys keys)
                {
                    this.cancelButton.PerformClick();
                    return true;
                });
        }

        protected override void OnLoad(EventArgs e)
        {
            this.saveButton.Select();
            base.OnLoad(e);
        }

        public override void LoadResources()
        {
            this.Text = PdnResources.GetString("ChooseToolDefaultsDialog.Text");
            this.Icon = Utility.ImageToIcon(PdnResources.GetImageResource("Icons.MenuLayersLayerPropertiesIcon.png").Reference);

            this.introText.Text = PdnResources.GetString("ChooseToolDefaultsDialog.IntroText.Text");
            this.defaultToolText.Text = PdnResources.GetString("ChooseToolDefaultsDialog.DefaultToolText.Text");

            this.loadFromToolBarButton.Text = PdnResources.GetString("ChooseToolDefaultsDialog.LoadFromToolBarButton.Text");
            this.cancelButton.Text = PdnResources.GetString("Form.CancelButton.Text");
            this.saveButton.Text = PdnResources.GetString("Form.SaveButton.Text");
            this.resetButton.Text = PdnResources.GetString("Form.ResetButton.Text");

            base.LoadResources();
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            int leftMargin = UI.ScaleWidth(8);
            int rightMargin = UI.ScaleWidth(8);
            int topMargin = UI.ScaleHeight(8);
            int bottomMargin = UI.ScaleHeight(8);
            int buttonHMargin = UI.ScaleWidth(7);
            int afterIntroTextVMargin = UI.ScaleHeight(16);
            int afterHeaderVMargin = UI.ScaleHeight(3);
            int hMargin = UI.ScaleWidth(7);
            int vMargin = UI.ScaleHeight(7);
            int insetWidth = ClientSize.Width - leftMargin - rightMargin;

            this.introText.Location = new Point(leftMargin, topMargin);
            this.introText.Width = insetWidth;
            this.introText.Height = this.introText.GetPreferredSize(this.introText.Size).Height;

            this.defaultToolText.Location = new Point(
                leftMargin,
                this.introText.Bottom + afterIntroTextVMargin);

            this.toolChooserStrip.Location = new Point(
                this.defaultToolText.Right + hMargin, 
                this.defaultToolText.Top + (this.defaultToolText.Height - this.toolChooserStrip.Height) / 2);

            int y = vMargin + Math.Max(this.defaultToolText.Bottom, this.toolChooserStrip.Bottom);
            int maxInsetWidth = insetWidth;

            for (int i = 0; i < this.toolConfigRows.Count; ++i)
            {
                this.toolConfigRows[i].HeaderLabel.Location = new Point(leftMargin, y);
                this.toolConfigRows[i].HeaderLabel.Width = insetWidth;
                y = this.toolConfigRows[i].HeaderLabel.Bottom + afterHeaderVMargin;

                this.toolConfigRows[i].ToolConfigStrip.Location = new Point(leftMargin + 3, y);
                Size preferredSize = this.toolConfigRows[i].ToolConfigStrip.GetPreferredSize(
                    new Size(this.toolConfigRows[i].ToolConfigStrip.Width, 1));

                this.toolConfigRows[i].ToolConfigStrip.Size = preferredSize;

                maxInsetWidth = Math.Max(maxInsetWidth, this.toolConfigRows[i].ToolConfigStrip.Width);

                y = this.toolConfigRows[i].ToolConfigStrip.Bottom + vMargin;
            }

            y += vMargin;

            this.bottomSeparator.Location = new Point(leftMargin, y);
            this.bottomSeparator.Width = insetWidth;
            this.bottomSeparator.Visible = false;

            y += this.bottomSeparator.Height;

            this.cancelButton.Location = new Point(ClientSize.Width - rightMargin - this.cancelButton.Width, y);

            this.saveButton.Location = new Point(
                this.cancelButton.Left - buttonHMargin - this.saveButton.Width,
                this.cancelButton.Top);

            this.resetButton.Location = new Point(leftMargin, this.saveButton.Top);

            this.loadFromToolBarButton.Location = new Point(this.resetButton.Right + buttonHMargin, this.resetButton.Top);

            y = this.resetButton.Bottom + bottomMargin;

            this.ClientSize = new Size(leftMargin + maxInsetWidth + rightMargin, y);

            if (IsHandleCreated && maxInsetWidth > insetWidth)
            {
                BeginInvoke(new Procedure(PerformLayout), null);
            }

            base.OnLayout(levent);
        }

        private void InitializeComponent()
        {
            this.cancelButton = new Button();
            this.saveButton = new Button();
            this.introText = new Label();
            this.defaultToolText = new Label();
            this.resetButton = new Button();
            this.loadFromToolBarButton = new Button();
            this.toolChooserStrip = new ToolChooserStrip();
            this.bottomSeparator = new HeaderLabel();
            this.SuspendLayout();
            // 
            // cancelButton
            // 
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.AutoSize = true;
            this.cancelButton.Click += new EventHandler(CancelButton_Click);
            this.cancelButton.TabIndex = 3;
            this.cancelButton.FlatStyle = FlatStyle.System;
            // 
            // saveButton
            // 
            this.saveButton.Name = "saveButton";
            this.saveButton.AutoSize = true;
            this.saveButton.Click += new EventHandler(SaveButton_Click);
            this.saveButton.TabIndex = 2;
            this.saveButton.FlatStyle = FlatStyle.System;
            // 
            // introText
            // 
            this.introText.Name = "introText";
            this.introText.TabStop = false;
            // 
            // defaultToolText
            // 
            this.defaultToolText.Name = "defaultToolText";
            this.defaultToolText.AutoSize = true;
            this.defaultToolText.TabStop = false;
            // 
            // resetButton
            // 
            this.resetButton.Name = "resetButton";
            this.resetButton.AutoSize = true;
            this.resetButton.Click += new EventHandler(ResetButton_Click);
            this.resetButton.TabIndex = 0;
            this.resetButton.FlatStyle = FlatStyle.System;
            //
            // loadFromToolBarButton
            //
            this.loadFromToolBarButton.Name = "loadFromToolBarButton";
            this.loadFromToolBarButton.AutoSize = true;
            this.loadFromToolBarButton.Click += new EventHandler(LoadFromToolBarButton_Click);
            this.loadFromToolBarButton.FlatStyle = FlatStyle.System;
            this.loadFromToolBarButton.TabIndex = 1;
            //
            // toolChooserStrip
            //
            this.toolChooserStrip.Name = "toolChooserStrip";
            this.toolChooserStrip.Dock = DockStyle.None;
            this.toolChooserStrip.GripStyle = ToolStripGripStyle.Hidden;
            this.toolChooserStrip.ShowChooseDefaults = false;
            this.toolChooserStrip.UseToolNameForLabel = true;
            this.toolChooserStrip.ToolClicked += new ToolClickedEventHandler(ToolChooserStrip_ToolClicked);
            //
            // bottomSeparator
            //
            this.bottomSeparator.Name = "bottomSeparator";
            this.bottomSeparator.RightMargin = 0;
            // 
            // ChooseToolDefaultsDialog
            // 
            this.AcceptButton = this.saveButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(448, 173);
            this.Controls.Add(this.resetButton);
            this.Controls.Add(this.loadFromToolBarButton);
            this.Controls.Add(this.introText);
            this.Controls.Add(this.defaultToolText);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.toolChooserStrip);
            this.Controls.Add(this.bottomSeparator);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.Location = new System.Drawing.Point(0, 0);
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.Name = "ChooseToolDefaultsDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void LoadFromToolBarButton_Click(object sender, EventArgs e)
        {
            ToolType = this.toolBarToolType;
            LoadUIFromAppEnvironment(this.toolBarAppEnvironment);
        }

        private void ToolChooserStrip_ToolClicked(object sender, ToolClickedEventArgs e)
        {
            ToolType = e.ToolType;
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            AppEnvironment defaults = new AppEnvironment();
            defaults.SetToDefaults();
            ToolType = Tool.DefaultToolType;
            LoadUIFromAppEnvironment(defaults);
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Yes;
            Close();
        }
    }
}
