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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace PaintDotNet
{
    // TODO: rewrite this ... the code is out of control here as it has grown organically,
    //       and it's impossible to maintain. post-3.0
    internal class ColorsForm 
        : FloatingToolForm
    {
        // We want some buttons that don't have a gradient background or fancy border
        private sealed class OurToolStripRenderer
            : ToolStripProfessionalRenderer
        {
            protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
            {
                if (e.ToolStrip is ToolStripDropDown)
                {
                    base.OnRenderToolStripBackground(e);
                }
                else
                {
                    using (SolidBrush backBrush = new SolidBrush(e.BackColor))
                    {
                        e.Graphics.FillRectangle(backBrush, e.AffectedBounds);
                    }
                }
            }

            protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
            {
                // Do not render a border.
            }
        }

        private Label redLabel;
        private Label blueLabel;
        private Label greenLabel;
        private Label hueLabel;

        private NumericUpDown redUpDown;
        private NumericUpDown greenUpDown;
        private NumericUpDown blueUpDown;
        private NumericUpDown hueUpDown;
        private NumericUpDown valueUpDown;
        private NumericUpDown saturationUpDown;

        private System.ComponentModel.Container components = null;
        private Label saturationLabel;
        private Label valueLabel;
        private ComboBox whichUserColorBox;
        private ColorGradientControl valueGradientControl;
        private NumericUpDown alphaUpDown;
        private ColorGradientControl alphaGradientControl;
        private ColorWheel colorWheel;

        private int ignoreChangedEvents = 0;
        private ColorBgra lastPrimaryColor;
        private Button moreLessButton;
        private ColorBgra lastSecondaryColor;

        private int suspendSetWhichUserColor;
        private string lessText;
        private string moreText;
        private Size moreSize;
        private Size lessSize;
        private Control lessModeButtonSentinel;
        private Control moreModeButtonSentinel;
        private Control lessModeHeaderSentinel;
        private Control moreModeHeaderSentinel;
        private bool inMoreState = true;
        private System.Windows.Forms.Label hexLabel;
        private System.Windows.Forms.TextBox hexBox;
        private uint ignore = 0;
        private PaintDotNet.HeaderLabel rgbHeader;
        private PaintDotNet.HeaderLabel hsvHeader;
        private HeaderLabel swatchHeader;
        private SwatchControl swatchControl;
        private ColorDisplayWidget colorDisplayWidget;
        private ToolStripEx toolStrip;
        private ToolStripButton colorAddButton;
        private PaintDotNet.HeaderLabel alphaHeader;

        private Image colorAddOverlay;
        private ToolStripSplitButton colorPalettesButton;
        private Bitmap colorAddIcon;
        private ColorGradientControl hueGradientControl;
        private ColorGradientControl saturationGradientControl;
        private ColorGradientControl redGradientControl;
        private ColorGradientControl greenGradientControl;
        private ColorGradientControl blueGradientControl;

        private PaletteCollection paletteCollection = null;

        public PaletteCollection PaletteCollection
        {
            get
            {
                return this.paletteCollection;
            }

            set
            {
                this.paletteCollection = value;
            }
        }

        private bool IgnoreChangedEvents
        {
            get
            {
                return this.ignoreChangedEvents != 0;
            }
        }

        private class WhichUserColorWrapper
        {
            private WhichUserColor whichUserColor;

            public WhichUserColor WhichUserColor
            {
                get
                {
                    return this.whichUserColor;
                }
            }

            public override int GetHashCode()
            {
                return this.whichUserColor.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                WhichUserColorWrapper rhs = obj as WhichUserColorWrapper;

                if (rhs == null)
                {
                    return false;
                }

                if (rhs.whichUserColor == this.whichUserColor)
                {
                    return true;
                }

                return false;
            }

            public override string ToString()
            {
                return PdnResources.GetString("WhichUserColor." + this.whichUserColor.ToString());
            }

            public WhichUserColorWrapper(WhichUserColor whichUserColor)
            {
                this.whichUserColor = whichUserColor;
            }
        }

        public void SuspendSetWhichUserColor()
        {
            ++this.suspendSetWhichUserColor;
        }

        public void ResumeSetWhichUserColor()
        {
            --this.suspendSetWhichUserColor;
        }

        public WhichUserColor WhichUserColor
        {
            get
            {
                return ((WhichUserColorWrapper)whichUserColorBox.SelectedItem).WhichUserColor;
            }

            set
            {
                if (this.suspendSetWhichUserColor <= 0)
                {
                    whichUserColorBox.SelectedItem = new WhichUserColorWrapper(value);
                }
            }
        }

        public void ToggleWhichUserColor()
        {
            switch (WhichUserColor)
            {
                case WhichUserColor.Primary:
                    WhichUserColor = WhichUserColor.Secondary;
                    break;

                case WhichUserColor.Secondary:
                    WhichUserColor = WhichUserColor.Primary;
                    break;

                default:
                    throw new InvalidEnumArgumentException();
            }
        }

        public void SetColorControlsRedraw(bool enabled)
        {
            Control[] controls =
                new Control[]
                {
                    this.colorWheel,
                    this.whichUserColorBox,
                    this.hueGradientControl,
                    this.saturationGradientControl,
                    this.valueGradientControl,
                    this.redGradientControl,
                    this.greenGradientControl,
                    this.blueGradientControl,
                    this.alphaGradientControl,
                    this.hueUpDown,
                    this.saturationUpDown,
                    this.valueUpDown,
                    this.redUpDown,
                    this.greenUpDown,
                    this.blueUpDown,
                    this.alphaUpDown,
                    this.toolStrip
                };

            foreach (Control control in controls)
            {
                if (enabled)
                {
                    UI.ResumeControlPainting(control);
                    control.Invalidate(true);
                }
                else
                {
                    UI.SuspendControlPainting(control);
                }
            }
        }

        public event ColorEventHandler UserPrimaryColorChanged;
        protected virtual void OnUserPrimaryColorChanged(ColorBgra newColor)
        {
            if (UserPrimaryColorChanged != null && ignore == 0)
            {
                this.userPrimaryColor = newColor;
                UserPrimaryColorChanged(this, new ColorEventArgs(newColor));
                this.lastPrimaryColor = newColor;
                this.colorDisplayWidget.UserPrimaryColor = newColor;
            }

            RenderColorAddIcon(newColor);
        }

        private ColorBgra userPrimaryColor;
        public ColorBgra UserPrimaryColor
        {
            get
            {
                return userPrimaryColor;
            }

            set
            {
                if (IgnoreChangedEvents)
                {
                    return;
                }

                if (userPrimaryColor != value)
                {
                    userPrimaryColor = value;
                    OnUserPrimaryColorChanged(value);

                    if (WhichUserColor != WhichUserColor.Primary)
                    {
                        this.WhichUserColor = WhichUserColor.Primary;
                    }

                    ignore++;

                    // only do the update on the last one, so partial RGB info isn't parsed.
                    Utility.SetNumericUpDownValue(alphaUpDown, value.A);
                    Utility.SetNumericUpDownValue(redUpDown, value.R);
                    Utility.SetNumericUpDownValue(greenUpDown, value.G);
                    SetColorGradientValuesRgb(value.R, value.G, value.B);
                    SetColorGradientMinMaxColorsRgb(value.R, value.G, value.B);
                    SetColorGradientMinMaxColorsAlpha(value.A);

                    ignore--;
                    Utility.SetNumericUpDownValue(blueUpDown, value.B);
                    Update();

                    string hexText = GetHexNumericUpDownValue(value.R, value.G, value.B);
                    hexBox.Text = hexText;

                    SyncHsvFromRgb(value);
                    this.colorDisplayWidget.UserPrimaryColor = this.userPrimaryColor;
                }
            }
        }

        private string GetHexNumericUpDownValue(int red, int green, int blue)
        {
            int newHexNumber = (red << 16) | (green << 8) | blue;
            string newHexText = System.Convert.ToString(newHexNumber, 16);
            
            while (newHexText.Length < 6)
            {
                newHexText = "0" + newHexText;
            }

            return newHexText.ToUpper();
        }

        public event ColorEventHandler UserSecondaryColorChanged;
        protected virtual void OnUserSecondaryColorChanged(ColorBgra newColor)
        {
            if (UserSecondaryColorChanged != null && ignore == 0)
            {
                this.userSecondaryColor = newColor;
                UserSecondaryColorChanged(this, new ColorEventArgs(newColor));
                this.lastSecondaryColor = newColor;
                this.colorDisplayWidget.UserSecondaryColor = newColor;
            }

            RenderColorAddIcon(newColor);
        }

        private ColorBgra userSecondaryColor;
        public ColorBgra UserSecondaryColor
        {
            get
            {
                return userSecondaryColor;
            }

            set
            {
                if (IgnoreChangedEvents)
                {
                    return;
                }

                if (userSecondaryColor != value)
                {
                    userSecondaryColor = value;
                    OnUserSecondaryColorChanged(value);

                    if (WhichUserColor != WhichUserColor.Secondary)
                    {
                        this.WhichUserColor = WhichUserColor.Secondary;
                    }

                    ignore++;

                    //only do the update on the last one, so partial RGB info isn't parsed.
                    Utility.SetNumericUpDownValue(alphaUpDown, value.A);
                    Utility.SetNumericUpDownValue(redUpDown, value.R);
                    Utility.SetNumericUpDownValue(greenUpDown, value.G);

                    SetColorGradientValuesRgb(value.R, value.G, value.B);
                    SetColorGradientMinMaxColorsRgb(value.R, value.G, value.B);
                    SetColorGradientMinMaxColorsAlpha(value.A);

                    ignore--;
                    Utility.SetNumericUpDownValue(blueUpDown, value.B);
                    Update();

                    string hexText = GetHexNumericUpDownValue(value.R, value.G, value.B);
                    hexBox.Text = hexText;

                    SyncHsvFromRgb(value);
                    this.colorDisplayWidget.UserSecondaryColor = this.userSecondaryColor;
                }
            }
        }

        /// <summary>
        /// Convenience function for ColorsForm internals. Checks the value of the
        /// WhichUserColor property and raises either the UserPrimaryColorChanged or
        /// the UserSecondaryColorChanged events.
        /// </summary>
        /// <param name="newColor">The new color to notify clients about.</param>
        private void OnUserColorChanged(ColorBgra newColor)
        {
            switch (WhichUserColor)
            {
                case WhichUserColor.Primary:
                    OnUserPrimaryColorChanged(newColor);
                    break;

                case WhichUserColor.Secondary:
                    OnUserSecondaryColorChanged(newColor);
                    break;

                default:
                    throw new InvalidEnumArgumentException("WhichUserColor property");
            }
        }

        /// <summary>
        /// Whenever a color is changed via RGB methods, call this and the HSV
        /// counterparts will be sync'd up.
        /// </summary>
        /// <param name="newColor">The RGB color that should be converted to HSV.</param>
        private void SyncHsvFromRgb(ColorBgra newColor)
        {
            if (ignore == 0) 
            {
                ignore++;
                HsvColor hsvColor = HsvColor.FromColor(newColor.ToColor());

                Utility.SetNumericUpDownValue(hueUpDown, hsvColor.Hue);
                Utility.SetNumericUpDownValue(saturationUpDown, hsvColor.Saturation);
                Utility.SetNumericUpDownValue(valueUpDown, hsvColor.Value);

                SetColorGradientValuesHsv(hsvColor.Hue, hsvColor.Saturation, hsvColor.Value);
                SetColorGradientMinMaxColorsHsv(hsvColor.Hue, hsvColor.Saturation, hsvColor.Value);

                colorWheel.HsvColor = hsvColor;
                ignore--;
            }
        }

        private void SetColorGradientValuesRgb(int r, int g, int b)
        {
            PushIgnoreChangedEvents();

            if (redGradientControl.Value != r)
            {
                redGradientControl.Value = r;
            }

            if (greenGradientControl.Value != g)
            {
                greenGradientControl.Value = g;
            }

            if (blueGradientControl.Value != b)
            {
                blueGradientControl.Value = b;
            }

            PopIgnoreChangedEvents();
        }

        private void SetColorGradientValuesHsv(int h, int s, int v)
        {
            PushIgnoreChangedEvents();

            if (((hueGradientControl.Value * 360) / 255) != h)
            {
                hueGradientControl.Value = (255 * h) / 360;
            }

            if (((saturationGradientControl.Value * 100) / 255) != s)
            {
                saturationGradientControl.Value = (255 * s) / 100;
            }

            if (((valueGradientControl.Value * 100) / 255) != v)
            {
                valueGradientControl.Value = (255 * v) / 100;
            }

            PopIgnoreChangedEvents();
        }

        /// <summary>
        /// Whenever a color is changed via HSV methods, call this and the RGB
        /// counterparts will be sync'd up.
        /// </summary>
        /// <param name="newColor">The HSV color that should be converted to RGB.</param>
        private void SyncRgbFromHsv(HsvColor newColor)
        {
            if (ignore == 0) 
            {
                ignore++;
                RgbColor rgbColor = newColor.ToRgb();

                Utility.SetNumericUpDownValue(redUpDown, rgbColor.Red);
                Utility.SetNumericUpDownValue(greenUpDown, rgbColor.Green);
                Utility.SetNumericUpDownValue(blueUpDown, rgbColor.Blue);

                string hexText = GetHexNumericUpDownValue(rgbColor.Red, rgbColor.Green, rgbColor.Blue);
                hexBox.Text = hexText;

                SetColorGradientValuesRgb(rgbColor.Red, rgbColor.Green, rgbColor.Blue);
                SetColorGradientMinMaxColorsRgb(rgbColor.Red, rgbColor.Green, rgbColor.Blue);
                SetColorGradientMinMaxColorsAlpha((int)alphaUpDown.Value);

                ignore--;
            } 
        }

        private void RenderColorAddIcon(ColorBgra newColor)
        {
            if (this.colorAddIcon == null)
            {
                this.colorAddIcon = new Bitmap(16, 16, PixelFormat.Format32bppArgb);
            }

            using (Graphics g = Graphics.FromImage(this.colorAddIcon))
            {
                Rectangle rect = new Rectangle(0, 0, this.colorAddIcon.Width - 2, this.colorAddIcon.Height - 2);
                Utility.DrawColorRectangle(g, rect, newColor.ToColor(), true);
                g.DrawImage(this.colorAddOverlay, 0, 0);
            }

            this.colorAddButton.Image = this.colorAddIcon;
            this.colorAddButton.Invalidate();
        }

        public ColorsForm()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            whichUserColorBox.Items.Add(new WhichUserColorWrapper(WhichUserColor.Primary));
            whichUserColorBox.Items.Add(new WhichUserColorWrapper(WhichUserColor.Secondary));
            whichUserColorBox.SelectedIndex = 0;

            moreSize = this.ClientSize;
            lessSize = new Size(this.swatchHeader.Width + SystemLayer.UI.ScaleWidth(16), moreSize.Height);

            this.Text = PdnResources.GetString("ColorsForm.Text");
            this.redLabel.Text = PdnResources.GetString("ColorsForm.RedLabel.Text");
            this.blueLabel.Text = PdnResources.GetString("ColorsForm.BlueLabel.Text");
            this.greenLabel.Text = PdnResources.GetString("ColorsForm.GreenLabel.Text");
            this.saturationLabel.Text = PdnResources.GetString("ColorsForm.SaturationLabel.Text");
            this.valueLabel.Text = PdnResources.GetString("ColorsForm.ValueLabel.Text");
            this.hueLabel.Text = PdnResources.GetString("ColorsForm.HueLabel.Text");
            this.rgbHeader.Text = PdnResources.GetString("ColorsForm.RgbHeader.Text");
            this.hexLabel.Text = PdnResources.GetString("ColorsForm.HexLabel.Text");
            this.hsvHeader.Text = PdnResources.GetString("ColorsForm.HsvHeader.Text");
            this.alphaHeader.Text = PdnResources.GetString("ColorsForm.AlphaHeader.Text");

            this.lessText = "<< " + PdnResources.GetString("ColorsForm.MoreLessButton.Text.Less");
            this.moreText = PdnResources.GetString("ColorsForm.MoreLessButton.Text.More") + " >>";
            this.moreLessButton.Text = lessText;

            this.toolStrip.Renderer = new OurToolStripRenderer();

            this.colorAddOverlay = PdnResources.GetImageResource("Icons.ColorAddOverlay.png").Reference;
            this.colorPalettesButton.Image = PdnResources.GetImageResource("Icons.ColorPalettes.png").Reference;

            RenderColorAddIcon(this.UserPrimaryColor);

            this.colorAddButton.ToolTipText = PdnResources.GetString("ColorsForm.ColorAddButton.ToolTipText");
            this.colorPalettesButton.ToolTipText = PdnResources.GetString("ColorsForm.ColorPalettesButton.ToolTipText");

            // Load the current palette
            string currentPaletteString;

            try
            {
                currentPaletteString = Settings.CurrentUser.GetString(SettingNames.CurrentPalette, null);
            }

            catch (Exception)
            {
                currentPaletteString = null;
            }

            if (currentPaletteString == null)
            {
                string defaultPaletteString = PaletteCollection.GetPaletteSaveString(PaletteCollection.DefaultPalette);
                currentPaletteString = defaultPaletteString;
            }

            ColorBgra[] currentPalette = PaletteCollection.ParsePaletteString(currentPaletteString);

            this.swatchControl.Colors = currentPalette;
        }

        protected override void OnLoad(EventArgs e)
        {
            this.inMoreState = true;
            this.moreLessButton.PerformClick();
            base.OnLoad(e);
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

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.valueGradientControl = new PaintDotNet.ColorGradientControl();
            this.colorWheel = new PaintDotNet.ColorWheel();
            this.redUpDown = new System.Windows.Forms.NumericUpDown();
            this.greenUpDown = new System.Windows.Forms.NumericUpDown();
            this.blueUpDown = new System.Windows.Forms.NumericUpDown();
            this.redLabel = new System.Windows.Forms.Label();
            this.blueLabel = new System.Windows.Forms.Label();
            this.greenLabel = new System.Windows.Forms.Label();
            this.saturationLabel = new System.Windows.Forms.Label();
            this.valueLabel = new System.Windows.Forms.Label();
            this.hueLabel = new System.Windows.Forms.Label();
            this.valueUpDown = new System.Windows.Forms.NumericUpDown();
            this.saturationUpDown = new System.Windows.Forms.NumericUpDown();
            this.hueUpDown = new System.Windows.Forms.NumericUpDown();
            this.hexBox = new System.Windows.Forms.TextBox();
            this.hexLabel = new System.Windows.Forms.Label();
            this.whichUserColorBox = new System.Windows.Forms.ComboBox();
            this.alphaUpDown = new System.Windows.Forms.NumericUpDown();
            this.moreLessButton = new System.Windows.Forms.Button();
            this.lessModeButtonSentinel = new System.Windows.Forms.Control();
            this.moreModeButtonSentinel = new System.Windows.Forms.Control();
            this.lessModeHeaderSentinel = new System.Windows.Forms.Control();
            this.moreModeHeaderSentinel = new System.Windows.Forms.Control();
            this.rgbHeader = new PaintDotNet.HeaderLabel();
            this.hsvHeader = new PaintDotNet.HeaderLabel();
            this.alphaHeader = new PaintDotNet.HeaderLabel();
            this.swatchHeader = new PaintDotNet.HeaderLabel();
            this.swatchControl = new PaintDotNet.SwatchControl();
            this.colorDisplayWidget = new PaintDotNet.ColorDisplayWidget();
            this.toolStrip = new PaintDotNet.SystemLayer.ToolStripEx();
            this.colorAddButton = new System.Windows.Forms.ToolStripButton();
            this.colorPalettesButton = new System.Windows.Forms.ToolStripSplitButton();
            this.hueGradientControl = new PaintDotNet.ColorGradientControl();
            this.saturationGradientControl = new PaintDotNet.ColorGradientControl();
            this.alphaGradientControl = new PaintDotNet.ColorGradientControl();
            this.redGradientControl = new PaintDotNet.ColorGradientControl();
            this.greenGradientControl = new PaintDotNet.ColorGradientControl();
            this.blueGradientControl = new PaintDotNet.ColorGradientControl();
            ((System.ComponentModel.ISupportInitialize)(this.redUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.greenUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.blueUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.valueUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.saturationUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.hueUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.alphaUpDown)).BeginInit();
            this.toolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // valueGradientControl
            // 
            this.valueGradientControl.Count = 1;
            this.valueGradientControl.CustomGradient = null;
            this.valueGradientControl.DrawFarNub = true;
            this.valueGradientControl.DrawNearNub = false;
            this.valueGradientControl.Location = new System.Drawing.Point(243, 185);
            this.valueGradientControl.MaxColor = System.Drawing.Color.White;
            this.valueGradientControl.MinColor = System.Drawing.Color.Black;
            this.valueGradientControl.Name = "valueGradientControl";
            this.valueGradientControl.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.valueGradientControl.Size = new System.Drawing.Size(73, 19);
            this.valueGradientControl.TabIndex = 2;
            this.valueGradientControl.TabStop = false;
            this.valueGradientControl.Value = 0;
            this.valueGradientControl.ValueChanged += new PaintDotNet.IndexEventHandler(this.HsvGradientControl_ValueChanged);
            // 
            // colorWheel
            // 
            this.colorWheel.Location = new System.Drawing.Point(65, 35);
            this.colorWheel.Name = "colorWheel";
            this.colorWheel.Size = new System.Drawing.Size(129, 130);
            this.colorWheel.TabIndex = 3;
            this.colorWheel.TabStop = false;
            this.colorWheel.ColorChanged += new System.EventHandler(this.ColorWheel_ColorChanged);
            // 
            // redUpDown
            // 
            this.redUpDown.Location = new System.Drawing.Point(320, 24);
            this.redUpDown.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.redUpDown.Name = "redUpDown";
            this.redUpDown.Size = new System.Drawing.Size(56, 20);
            this.redUpDown.TabIndex = 2;
            this.redUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.redUpDown.Enter += new System.EventHandler(this.UpDown_Enter);
            this.redUpDown.ValueChanged += new System.EventHandler(this.UpDown_ValueChanged);
            this.redUpDown.Leave += new System.EventHandler(this.UpDown_Leave);
            this.redUpDown.KeyUp += new System.Windows.Forms.KeyEventHandler(this.UpDown_KeyUp);
            // 
            // greenUpDown
            // 
            this.greenUpDown.Location = new System.Drawing.Point(320, 48);
            this.greenUpDown.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.greenUpDown.Name = "greenUpDown";
            this.greenUpDown.Size = new System.Drawing.Size(56, 20);
            this.greenUpDown.TabIndex = 3;
            this.greenUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.greenUpDown.Enter += new System.EventHandler(this.UpDown_Enter);
            this.greenUpDown.ValueChanged += new System.EventHandler(this.UpDown_ValueChanged);
            this.greenUpDown.Leave += new System.EventHandler(this.UpDown_Leave);
            this.greenUpDown.KeyUp += new System.Windows.Forms.KeyEventHandler(this.UpDown_KeyUp);
            // 
            // blueUpDown
            // 
            this.blueUpDown.Location = new System.Drawing.Point(320, 72);
            this.blueUpDown.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.blueUpDown.Name = "blueUpDown";
            this.blueUpDown.Size = new System.Drawing.Size(56, 20);
            this.blueUpDown.TabIndex = 4;
            this.blueUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.blueUpDown.Enter += new System.EventHandler(this.UpDown_Enter);
            this.blueUpDown.ValueChanged += new System.EventHandler(this.UpDown_ValueChanged);
            this.blueUpDown.Leave += new System.EventHandler(this.UpDown_Leave);
            this.blueUpDown.KeyUp += new System.Windows.Forms.KeyEventHandler(this.UpDown_KeyUp);
            // 
            // redLabel
            // 
            this.redLabel.AutoSize = true;
            this.redLabel.Location = new System.Drawing.Point(222, 28);
            this.redLabel.Name = "redLabel";
            this.redLabel.Size = new System.Drawing.Size(15, 13);
            this.redLabel.TabIndex = 7;
            this.redLabel.Text = "R";
            this.redLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // blueLabel
            // 
            this.blueLabel.AutoSize = true;
            this.blueLabel.Location = new System.Drawing.Point(222, 76);
            this.blueLabel.Name = "blueLabel";
            this.blueLabel.Size = new System.Drawing.Size(14, 13);
            this.blueLabel.TabIndex = 8;
            this.blueLabel.Text = "B";
            this.blueLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // greenLabel
            // 
            this.greenLabel.AutoSize = true;
            this.greenLabel.Location = new System.Drawing.Point(222, 52);
            this.greenLabel.Name = "greenLabel";
            this.greenLabel.Size = new System.Drawing.Size(15, 13);
            this.greenLabel.TabIndex = 9;
            this.greenLabel.Text = "G";
            this.greenLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // saturationLabel
            // 
            this.saturationLabel.AutoSize = true;
            this.saturationLabel.Location = new System.Drawing.Point(222, 164);
            this.saturationLabel.Name = "saturationLabel";
            this.saturationLabel.Size = new System.Drawing.Size(17, 13);
            this.saturationLabel.TabIndex = 16;
            this.saturationLabel.Text = "S:";
            this.saturationLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // valueLabel
            // 
            this.valueLabel.AutoSize = true;
            this.valueLabel.Location = new System.Drawing.Point(222, 188);
            this.valueLabel.Name = "valueLabel";
            this.valueLabel.Size = new System.Drawing.Size(17, 13);
            this.valueLabel.TabIndex = 15;
            this.valueLabel.Text = "V:";
            this.valueLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // hueLabel
            // 
            this.hueLabel.AutoSize = true;
            this.hueLabel.Location = new System.Drawing.Point(222, 140);
            this.hueLabel.Name = "hueLabel";
            this.hueLabel.Size = new System.Drawing.Size(18, 13);
            this.hueLabel.TabIndex = 14;
            this.hueLabel.Text = "H:";
            this.hueLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // valueUpDown
            // 
            this.valueUpDown.Location = new System.Drawing.Point(320, 184);
            this.valueUpDown.Name = "valueUpDown";
            this.valueUpDown.Size = new System.Drawing.Size(56, 20);
            this.valueUpDown.TabIndex = 8;
            this.valueUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.valueUpDown.Enter += new System.EventHandler(this.UpDown_Enter);
            this.valueUpDown.ValueChanged += new System.EventHandler(this.UpDown_ValueChanged);
            this.valueUpDown.Leave += new System.EventHandler(this.UpDown_Leave);
            this.valueUpDown.KeyUp += new System.Windows.Forms.KeyEventHandler(this.UpDown_KeyUp);
            // 
            // saturationUpDown
            // 
            this.saturationUpDown.Location = new System.Drawing.Point(320, 160);
            this.saturationUpDown.Name = "saturationUpDown";
            this.saturationUpDown.Size = new System.Drawing.Size(56, 20);
            this.saturationUpDown.TabIndex = 7;
            this.saturationUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.saturationUpDown.Enter += new System.EventHandler(this.UpDown_Enter);
            this.saturationUpDown.ValueChanged += new System.EventHandler(this.UpDown_ValueChanged);
            this.saturationUpDown.Leave += new System.EventHandler(this.UpDown_Leave);
            this.saturationUpDown.KeyUp += new System.Windows.Forms.KeyEventHandler(this.UpDown_KeyUp);
            // 
            // hueUpDown
            // 
            this.hueUpDown.Location = new System.Drawing.Point(320, 136);
            this.hueUpDown.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
            this.hueUpDown.Name = "hueUpDown";
            this.hueUpDown.Size = new System.Drawing.Size(56, 20);
            this.hueUpDown.TabIndex = 6;
            this.hueUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.hueUpDown.Enter += new System.EventHandler(this.UpDown_Enter);
            this.hueUpDown.ValueChanged += new System.EventHandler(this.UpDown_ValueChanged);
            this.hueUpDown.Leave += new System.EventHandler(this.UpDown_Leave);
            this.hueUpDown.KeyUp += new System.Windows.Forms.KeyEventHandler(this.UpDown_KeyUp);
            // 
            // hexBox
            // 
            this.hexBox.Location = new System.Drawing.Point(320, 96);
            this.hexBox.Name = "hexBox";
            this.hexBox.Size = new System.Drawing.Size(56, 20);
            this.hexBox.TabIndex = 5;
            this.hexBox.Text = "000000";
            this.hexBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.hexBox.Enter += new System.EventHandler(this.HexUpDown_Enter);
            this.hexBox.Leave += new System.EventHandler(this.HexUpDown_Leave);
            this.hexBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.HexUpDown_KeyUp);
            this.hexBox.TextChanged += new System.EventHandler(this.UpDown_ValueChanged);
            // 
            // hexLabel
            // 
            this.hexLabel.AutoSize = true;
            this.hexLabel.Location = new System.Drawing.Point(222, 99);
            this.hexLabel.Name = "hexLabel";
            this.hexLabel.Size = new System.Drawing.Size(26, 13);
            this.hexLabel.TabIndex = 13;
            this.hexLabel.Text = "Hex";
            this.hexLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // whichUserColorBox
            // 
            this.whichUserColorBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.whichUserColorBox.Location = new System.Drawing.Point(8, 8);
            this.whichUserColorBox.Name = "whichUserColorBox";
            this.whichUserColorBox.Size = new System.Drawing.Size(112, 21);
            this.whichUserColorBox.TabIndex = 0;
            this.whichUserColorBox.SelectedIndexChanged += new System.EventHandler(this.WhichUserColorBox_SelectedIndexChanged);
            // 
            // alphaUpDown
            // 
            this.alphaUpDown.Location = new System.Drawing.Point(320, 228);
            this.alphaUpDown.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.alphaUpDown.Name = "alphaUpDown";
            this.alphaUpDown.Size = new System.Drawing.Size(56, 20);
            this.alphaUpDown.TabIndex = 10;
            this.alphaUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.alphaUpDown.Enter += new System.EventHandler(this.UpDown_Enter);
            this.alphaUpDown.ValueChanged += new System.EventHandler(this.UpDown_ValueChanged);
            this.alphaUpDown.Leave += new System.EventHandler(this.UpDown_Leave);
            this.alphaUpDown.KeyUp += new System.Windows.Forms.KeyEventHandler(this.UpDown_KeyUp);
            // 
            // moreLessButton
            // 
            this.moreLessButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.moreLessButton.Location = new System.Drawing.Point(126, 7);
            this.moreLessButton.Name = "moreLessButton";
            this.moreLessButton.Size = new System.Drawing.Size(75, 23);
            this.moreLessButton.TabIndex = 1;
            this.moreLessButton.Click += new System.EventHandler(this.MoreLessButton_Click);
            // 
            // lessModeButtonSentinel
            // 
            this.lessModeButtonSentinel.Location = new System.Drawing.Point(128, 7);
            this.lessModeButtonSentinel.Name = "lessModeButtonSentinel";
            this.lessModeButtonSentinel.Size = new System.Drawing.Size(0, 0);
            this.lessModeButtonSentinel.TabIndex = 22;
            this.lessModeButtonSentinel.Text = "we put the lessMore control here when in \"Less\" mode";
            this.lessModeButtonSentinel.Visible = false;
            // 
            // moreModeButtonSentinel
            // 
            this.moreModeButtonSentinel.Location = new System.Drawing.Point(165, 7);
            this.moreModeButtonSentinel.Name = "moreModeButtonSentinel";
            this.moreModeButtonSentinel.Size = new System.Drawing.Size(0, 0);
            this.moreModeButtonSentinel.TabIndex = 23;
            this.moreModeButtonSentinel.Visible = false;
            // 
            // lessModeHeaderSentinel
            // 
            this.lessModeHeaderSentinel.Location = new System.Drawing.Point(8, 40);
            this.lessModeHeaderSentinel.Name = "lessModeHeaderSentinel";
            this.lessModeHeaderSentinel.Size = new System.Drawing.Size(195, 185);
            this.lessModeHeaderSentinel.TabIndex = 24;
            this.lessModeHeaderSentinel.Visible = false;
            // 
            // moreModeHeaderSentinel
            // 
            this.moreModeHeaderSentinel.Location = new System.Drawing.Point(8, 40);
            this.moreModeHeaderSentinel.Name = "moreModeHeaderSentinel";
            this.moreModeHeaderSentinel.Size = new System.Drawing.Size(232, 216);
            this.moreModeHeaderSentinel.TabIndex = 25;
            this.moreModeHeaderSentinel.TabStop = false;
            this.moreModeHeaderSentinel.Visible = false;
            // 
            // rgbHeader
            // 
            this.rgbHeader.Location = new System.Drawing.Point(222, 8);
            this.rgbHeader.Name = "rgbHeader";
            this.rgbHeader.RightMargin = 0;
            this.rgbHeader.Size = new System.Drawing.Size(154, 14);
            this.rgbHeader.TabIndex = 27;
            this.rgbHeader.TabStop = false;
            // 
            // hsvHeader
            // 
            this.hsvHeader.Location = new System.Drawing.Point(222, 120);
            this.hsvHeader.Name = "hsvHeader";
            this.hsvHeader.RightMargin = 0;
            this.hsvHeader.Size = new System.Drawing.Size(154, 14);
            this.hsvHeader.TabIndex = 28;
            this.hsvHeader.TabStop = false;
            // 
            // alphaHeader
            // 
            this.alphaHeader.Location = new System.Drawing.Point(222, 212);
            this.alphaHeader.Name = "alphaHeader";
            this.alphaHeader.RightMargin = 0;
            this.alphaHeader.Size = new System.Drawing.Size(154, 14);
            this.alphaHeader.TabIndex = 29;
            this.alphaHeader.TabStop = false;
            // 
            // swatchHeader
            // 
            this.swatchHeader.Location = new System.Drawing.Point(8, 177);
            this.swatchHeader.Name = "swatchHeader";
            this.swatchHeader.RightMargin = 0;
            this.swatchHeader.Size = new System.Drawing.Size(193, 14);
            this.swatchHeader.TabIndex = 30;
            this.swatchHeader.TabStop = false;
            // 
            // swatchControl
            // 
            this.swatchControl.BlinkHighlight = false;
            this.swatchControl.Colors = new PaintDotNet.ColorBgra[0];
            this.swatchControl.Location = new System.Drawing.Point(8, 189);
            this.swatchControl.Name = "swatchControl";
            this.swatchControl.Size = new System.Drawing.Size(192, 74);
            this.swatchControl.TabIndex = 31;
            this.swatchControl.Text = "swatchControl1";
            this.swatchControl.ColorsChanged += this.SwatchControl_ColorsChanged;
            this.swatchControl.ColorClicked += this.SwatchControl_ColorClicked;
            // 
            // colorDisplayWidget
            // 
            this.colorDisplayWidget.Location = new System.Drawing.Point(4, 32);
            this.colorDisplayWidget.Name = "colorDisplayWidget";
            this.colorDisplayWidget.Size = new System.Drawing.Size(52, 52);
            this.colorDisplayWidget.TabIndex = 32;
            this.colorDisplayWidget.BlackAndWhiteButtonClicked += ColorDisplayWidget_BlackAndWhiteButtonClicked;
            this.colorDisplayWidget.SwapColorsClicked += ColorDisplayWidget_SwapColorsClicked;
            this.colorDisplayWidget.UserPrimaryColorClick += ColorDisplay_PrimaryColorClicked;
            this.colorDisplayWidget.UserSecondaryColorClick += ColorDisplay_SecondaryColorClicked;
            // 
            // toolStrip
            // 
            this.toolStrip.ClickThrough = true;
            this.toolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.colorAddButton,
                this.colorPalettesButton});
            this.toolStrip.Location = new System.Drawing.Point(5, 157);
            this.toolStrip.ManagedFocus = true;
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new System.Drawing.Size(65, 25);
            this.toolStrip.TabIndex = 33;
            this.toolStrip.Text = "toolStrip";
            // 
            // colorAddButton
            // 
            this.colorAddButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.colorAddButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.colorAddButton.Name = "colorAddButton";
            this.colorAddButton.Size = new System.Drawing.Size(23, 22);
            this.colorAddButton.Text = "colorAddButton";
            this.colorAddButton.Click += new System.EventHandler(this.ColorAddButton_Click);
            // 
            // colorPalettesButton
            // 
            this.colorPalettesButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.colorPalettesButton.Name = "colorPalettesButton";
            this.colorPalettesButton.Size = new System.Drawing.Size(16, 22);
            this.colorPalettesButton.Click += new System.EventHandler(this.ColorPalettesButton_Click);
            this.colorPalettesButton.DropDownOpening += new System.EventHandler(this.ColorPalettesButton_DropDownOpening);
            // 
            // hueGradientControl
            // 
            this.hueGradientControl.Count = 1;
            this.hueGradientControl.CustomGradient = null;
            this.hueGradientControl.DrawFarNub = true;
            this.hueGradientControl.DrawNearNub = false;
            this.hueGradientControl.Location = new System.Drawing.Point(243, 137);
            this.hueGradientControl.MaxColor = System.Drawing.Color.White;
            this.hueGradientControl.MinColor = System.Drawing.Color.Black;
            this.hueGradientControl.Name = "hueGradientControl";
            this.hueGradientControl.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.hueGradientControl.Size = new System.Drawing.Size(73, 19);
            this.hueGradientControl.TabIndex = 34;
            this.hueGradientControl.TabStop = false;
            this.hueGradientControl.Value = 0;
            this.hueGradientControl.ValueChanged += new PaintDotNet.IndexEventHandler(this.HsvGradientControl_ValueChanged);
            // 
            // saturationGradientControl
            // 
            this.saturationGradientControl.Count = 1;
            this.saturationGradientControl.CustomGradient = null;
            this.saturationGradientControl.DrawFarNub = true;
            this.saturationGradientControl.DrawNearNub = false;
            this.saturationGradientControl.Location = new System.Drawing.Point(243, 161);
            this.saturationGradientControl.MaxColor = System.Drawing.Color.White;
            this.saturationGradientControl.MinColor = System.Drawing.Color.Black;
            this.saturationGradientControl.Name = "saturationGradientControl";
            this.saturationGradientControl.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.saturationGradientControl.Size = new System.Drawing.Size(73, 19);
            this.saturationGradientControl.TabIndex = 35;
            this.saturationGradientControl.TabStop = false;
            this.saturationGradientControl.Value = 0;
            this.saturationGradientControl.ValueChanged += new PaintDotNet.IndexEventHandler(this.HsvGradientControl_ValueChanged);
            // 
            // alphaGradientControl
            // 
            this.alphaGradientControl.Count = 1;
            this.alphaGradientControl.CustomGradient = null;
            this.alphaGradientControl.DrawFarNub = true;
            this.alphaGradientControl.DrawNearNub = false;
            this.alphaGradientControl.Location = new System.Drawing.Point(243, 229);
            this.alphaGradientControl.MaxColor = System.Drawing.Color.White;
            this.alphaGradientControl.MinColor = System.Drawing.Color.Black;
            this.alphaGradientControl.Name = "alphaGradientControl";
            this.alphaGradientControl.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.alphaGradientControl.Size = new System.Drawing.Size(73, 19);
            this.alphaGradientControl.TabIndex = 36;
            this.alphaGradientControl.TabStop = false;
            this.alphaGradientControl.Value = 0;
            this.alphaGradientControl.ValueChanged += new PaintDotNet.IndexEventHandler(this.UpDown_ValueChanged);
            // 
            // redGradientControl
            // 
            this.redGradientControl.Count = 1;
            this.redGradientControl.CustomGradient = null;
            this.redGradientControl.DrawFarNub = true;
            this.redGradientControl.DrawNearNub = false;
            this.redGradientControl.Location = new System.Drawing.Point(243, 25);
            this.redGradientControl.MaxColor = System.Drawing.Color.White;
            this.redGradientControl.MinColor = System.Drawing.Color.Black;
            this.redGradientControl.Name = "redGradientControl";
            this.redGradientControl.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.redGradientControl.Size = new System.Drawing.Size(73, 19);
            this.redGradientControl.TabIndex = 37;
            this.redGradientControl.TabStop = false;
            this.redGradientControl.Value = 0;
            this.redGradientControl.ValueChanged += new PaintDotNet.IndexEventHandler(this.RgbGradientControl_ValueChanged);
            // 
            // greenGradientControl
            // 
            this.greenGradientControl.Count = 1;
            this.greenGradientControl.CustomGradient = null;
            this.greenGradientControl.DrawFarNub = true;
            this.greenGradientControl.DrawNearNub = false;
            this.greenGradientControl.Location = new System.Drawing.Point(243, 49);
            this.greenGradientControl.MaxColor = System.Drawing.Color.White;
            this.greenGradientControl.MinColor = System.Drawing.Color.Black;
            this.greenGradientControl.Name = "greenGradientControl";
            this.greenGradientControl.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.greenGradientControl.Size = new System.Drawing.Size(73, 19);
            this.greenGradientControl.TabIndex = 38;
            this.greenGradientControl.TabStop = false;
            this.greenGradientControl.Value = 0;
            this.greenGradientControl.ValueChanged += new PaintDotNet.IndexEventHandler(this.RgbGradientControl_ValueChanged);
            // 
            // blueGradientControl
            // 
            this.blueGradientControl.Count = 1;
            this.blueGradientControl.CustomGradient = null;
            this.blueGradientControl.DrawFarNub = true;
            this.blueGradientControl.DrawNearNub = false;
            this.blueGradientControl.Location = new System.Drawing.Point(243, 73);
            this.blueGradientControl.MaxColor = System.Drawing.Color.White;
            this.blueGradientControl.MinColor = System.Drawing.Color.Black;
            this.blueGradientControl.Name = "blueGradientControl";
            this.blueGradientControl.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.blueGradientControl.Size = new System.Drawing.Size(73, 19);
            this.blueGradientControl.TabIndex = 39;
            this.blueGradientControl.TabStop = false;
            this.blueGradientControl.Value = 0;
            this.blueGradientControl.ValueChanged += new PaintDotNet.IndexEventHandler(this.RgbGradientControl_ValueChanged);
            // 
            // ColorsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(386, 266);
            this.Controls.Add(this.valueLabel);
            this.Controls.Add(this.saturationLabel);
            this.Controls.Add(this.hueLabel);
            this.Controls.Add(this.greenLabel);
            this.Controls.Add(this.blueLabel);
            this.Controls.Add(this.redLabel);
            this.Controls.Add(this.hexLabel);
            this.Controls.Add(this.blueGradientControl);
            this.Controls.Add(this.greenGradientControl);
            this.Controls.Add(this.redGradientControl);
            this.Controls.Add(this.alphaGradientControl);
            this.Controls.Add(this.saturationGradientControl);
            this.Controls.Add(this.hueGradientControl);
            this.Controls.Add(this.toolStrip);
            this.Controls.Add(this.colorWheel);
            this.Controls.Add(this.colorDisplayWidget);
            this.Controls.Add(this.swatchControl);
            this.Controls.Add(this.swatchHeader);
            this.Controls.Add(this.alphaHeader);
            this.Controls.Add(this.hsvHeader);
            this.Controls.Add(this.rgbHeader);
            this.Controls.Add(this.valueGradientControl);
            this.Controls.Add(this.moreModeButtonSentinel);
            this.Controls.Add(this.lessModeButtonSentinel);
            this.Controls.Add(this.moreLessButton);
            this.Controls.Add(this.whichUserColorBox);
            this.Controls.Add(this.lessModeHeaderSentinel);
            this.Controls.Add(this.moreModeHeaderSentinel);
            this.Controls.Add(this.blueUpDown);
            this.Controls.Add(this.greenUpDown);
            this.Controls.Add(this.redUpDown);
            this.Controls.Add(this.hexBox);
            this.Controls.Add(this.hueUpDown);
            this.Controls.Add(this.saturationUpDown);
            this.Controls.Add(this.valueUpDown);
            this.Controls.Add(this.alphaUpDown);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "ColorsForm";
            this.Controls.SetChildIndex(this.alphaUpDown, 0);
            this.Controls.SetChildIndex(this.valueUpDown, 0);
            this.Controls.SetChildIndex(this.saturationUpDown, 0);
            this.Controls.SetChildIndex(this.hueUpDown, 0);
            this.Controls.SetChildIndex(this.hexBox, 0);
            this.Controls.SetChildIndex(this.redUpDown, 0);
            this.Controls.SetChildIndex(this.greenUpDown, 0);
            this.Controls.SetChildIndex(this.blueUpDown, 0);
            this.Controls.SetChildIndex(this.moreModeHeaderSentinel, 0);
            this.Controls.SetChildIndex(this.lessModeHeaderSentinel, 0);
            this.Controls.SetChildIndex(this.whichUserColorBox, 0);
            this.Controls.SetChildIndex(this.moreLessButton, 0);
            this.Controls.SetChildIndex(this.lessModeButtonSentinel, 0);
            this.Controls.SetChildIndex(this.moreModeButtonSentinel, 0);
            this.Controls.SetChildIndex(this.valueGradientControl, 0);
            this.Controls.SetChildIndex(this.rgbHeader, 0);
            this.Controls.SetChildIndex(this.hsvHeader, 0);
            this.Controls.SetChildIndex(this.alphaHeader, 0);
            this.Controls.SetChildIndex(this.swatchHeader, 0);
            this.Controls.SetChildIndex(this.swatchControl, 0);
            this.Controls.SetChildIndex(this.colorDisplayWidget, 0);
            this.Controls.SetChildIndex(this.colorWheel, 0);
            this.Controls.SetChildIndex(this.toolStrip, 0);
            this.Controls.SetChildIndex(this.hueGradientControl, 0);
            this.Controls.SetChildIndex(this.saturationGradientControl, 0);
            this.Controls.SetChildIndex(this.alphaGradientControl, 0);
            this.Controls.SetChildIndex(this.redGradientControl, 0);
            this.Controls.SetChildIndex(this.greenGradientControl, 0);
            this.Controls.SetChildIndex(this.blueGradientControl, 0);
            this.Controls.SetChildIndex(this.hexLabel, 0);
            this.Controls.SetChildIndex(this.redLabel, 0);
            this.Controls.SetChildIndex(this.blueLabel, 0);
            this.Controls.SetChildIndex(this.greenLabel, 0);
            this.Controls.SetChildIndex(this.hueLabel, 0);
            this.Controls.SetChildIndex(this.saturationLabel, 0);
            this.Controls.SetChildIndex(this.valueLabel, 0);
            ((System.ComponentModel.ISupportInitialize)(this.redUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.greenUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.blueUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.valueUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.saturationUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.hueUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.alphaUpDown)).EndInit();
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        public void SetUserColors(ColorBgra primary, ColorBgra secondary)
        {
            SetColorControlsRedraw(false);
            WhichUserColor which = WhichUserColor;
            UserPrimaryColor = primary;
            UserSecondaryColor = secondary;
            WhichUserColor = which;
            SetColorControlsRedraw(true);
        }

        public void SwapUserColors()
        {
            ColorBgra primary = this.UserPrimaryColor;
            ColorBgra secondary = this.UserSecondaryColor;
            SetUserColors(secondary, primary);
        }

        public void SetUserColorsToBlackAndWhite()
        {
            SetUserColors(ColorBgra.Black, ColorBgra.White);
        }

        private void ColorDisplayWidget_SwapColorsClicked(object sender, EventArgs e)
        {
            SwapUserColors();
            OnRelinquishFocus();
        }

        private void ColorDisplayWidget_BlackAndWhiteButtonClicked(object sender, EventArgs e)
        {
            SetUserColorsToBlackAndWhite();
            OnRelinquishFocus();
        }

        private void ColorDisplay_PrimaryColorClicked(object sender, System.EventArgs e)
        {
            WhichUserColor = WhichUserColor.Primary;
            OnRelinquishFocus();
        }

        private void ColorDisplay_SecondaryColorClicked(object sender, System.EventArgs e)
        {
            WhichUserColor = WhichUserColor.Secondary;
            OnRelinquishFocus();
        }

        private void WhichUserColorBox_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            ColorBgra color;

            switch (WhichUserColor)
            {
                case WhichUserColor.Primary:
                    color = userPrimaryColor;
                    break;

                case WhichUserColor.Secondary:
                    color = userSecondaryColor;
                    break;

                default:
                    throw new InvalidEnumArgumentException("WhichUserColor property");
            }

            PushIgnoreChangedEvents();
            Utility.SetNumericUpDownValue(redUpDown, color.R);
            Utility.SetNumericUpDownValue(greenUpDown, color.G);
            Utility.SetNumericUpDownValue(blueUpDown, color.B);

            string hexText = GetHexNumericUpDownValue(color.R, color.G, color.B);
            hexBox.Text = hexText;

            Utility.SetNumericUpDownValue(alphaUpDown, color.A);
            PopIgnoreChangedEvents();

            SetColorGradientMinMaxColorsRgb(color.R, color.G, color.B);
            SetColorGradientValuesRgb(color.R, color.G, color.B);
            SetColorGradientMinMaxColorsAlpha(color.A);

            SyncHsvFromRgb(color);

            OnRelinquishFocus();
        }

        private void ColorWheel_ColorChanged(object sender, EventArgs e)
        {
            if (IgnoreChangedEvents)
            {
                return;
            }

            PushIgnoreChangedEvents();

            HsvColor hsvColor = colorWheel.HsvColor;
            RgbColor rgbColor = hsvColor.ToRgb();
            ColorBgra color = ColorBgra.FromBgra((byte)rgbColor.Blue, (byte)rgbColor.Green, (byte)rgbColor.Red, (byte)alphaUpDown.Value);

            Utility.SetNumericUpDownValue(hueUpDown, hsvColor.Hue);
            Utility.SetNumericUpDownValue(saturationUpDown, hsvColor.Saturation);
            Utility.SetNumericUpDownValue(valueUpDown, hsvColor.Value);

            Utility.SetNumericUpDownValue(redUpDown, color.R);
            Utility.SetNumericUpDownValue(greenUpDown, color.G);
            Utility.SetNumericUpDownValue(blueUpDown, color.B);

            string hexText = GetHexNumericUpDownValue(color.R, color.G, color.B);
            hexBox.Text = hexText;
            
            Utility.SetNumericUpDownValue(alphaUpDown, color.A);

            SetColorGradientValuesHsv(hsvColor.Hue, hsvColor.Saturation, hsvColor.Value);
            SetColorGradientMinMaxColorsHsv(hsvColor.Hue, hsvColor.Saturation, hsvColor.Value);

            SetColorGradientValuesRgb(color.R, color.G, color.B);
            SetColorGradientMinMaxColorsRgb(color.R, color.G, color.B);
            SetColorGradientMinMaxColorsAlpha(color.A);

            switch (WhichUserColor)
            {
                case WhichUserColor.Primary:
                    //UserPrimaryColor = color;
                    this.userPrimaryColor = color;
                    OnUserPrimaryColorChanged(color);
                    OnRelinquishFocus();
                    break;

                case WhichUserColor.Secondary:
                    //UserSecondaryColor = color;
                    this.userSecondaryColor = color;
                    OnUserSecondaryColorChanged(color);
                    OnRelinquishFocus();
                    break;

                default:
                    throw new InvalidEnumArgumentException("WhichUserColor property");
            }

            PopIgnoreChangedEvents();

            Update();
        }

        private void SetColorGradientMinMaxColorsHsv(int h, int s, int v)
        {
            Color[] hueColors = new Color[361];

            for (int newH = 0; newH <= 360; ++newH)
            {
                HsvColor hsv = new HsvColor(newH, 100, 100);
                hueColors[newH] = hsv.ToColor();
            }

            this.hueGradientControl.CustomGradient = hueColors;

            Color[] satColors = new Color[101];

            for (int newS = 0; newS <= 100; ++newS)
            {
                HsvColor hsv = new HsvColor(h, newS, v);
                satColors[newS] = hsv.ToColor();
            }

            this.saturationGradientControl.CustomGradient = satColors;

            this.valueGradientControl.MaxColor = new HsvColor(h, s, 100).ToColor();
            this.valueGradientControl.MinColor = new HsvColor(h, s, 0).ToColor();
        }

        private void SetColorGradientMinMaxColorsRgb(int r, int g, int b)
        {
            this.redGradientControl.MaxColor = Color.FromArgb(255, g, b);
            this.redGradientControl.MinColor = Color.FromArgb(0, g, b);
            this.greenGradientControl.MaxColor = Color.FromArgb(r, 255, b);
            this.greenGradientControl.MinColor = Color.FromArgb(r, 0, b);
            this.blueGradientControl.MaxColor = Color.FromArgb(r, g, 255);
            this.blueGradientControl.MinColor = Color.FromArgb(r, g, 0);
        }

        private void SetColorGradientMinMaxColorsAlpha(int a)
        {
            Color[] colors = new Color[256];

            for (int newA = 0; newA <= 255; ++newA)
            {
                colors[newA] = Color.FromArgb(newA, this.redGradientControl.Value,
                    this.greenGradientControl.Value, this.blueGradientControl.Value);
            }

            this.alphaGradientControl.CustomGradient = colors;
        }

        private void RgbGradientControl_ValueChanged(object sender, IndexEventArgs ce)
        {
            if (IgnoreChangedEvents)
            {
                return;
            }

            int red;
            if (sender == redGradientControl)
            {
                red = redGradientControl.Value;
            }
            else
            {
                red = (int)redUpDown.Value;
            }

            int green;
            if (sender == greenGradientControl)
            {
                green = greenGradientControl.Value;
            }
            else
            {
                green = (int)greenUpDown.Value;
            }

            int blue;
            if (sender == blueGradientControl)
            {
                blue = blueGradientControl.Value;
            }
            else
            {
                blue = (int)blueUpDown.Value;
            }

            int alpha;
            if (sender == alphaGradientControl)
            {
                alpha = alphaGradientControl.Value;
            }
            else
            {
                alpha = (int)alphaUpDown.Value;
            }

            Color rgbColor = Color.FromArgb(alpha, red, green, blue);
            HsvColor hsvColor = HsvColor.FromColor(rgbColor);

            PushIgnoreChangedEvents();
            Utility.SetNumericUpDownValue(hueUpDown, hsvColor.Hue);
            Utility.SetNumericUpDownValue(saturationUpDown, hsvColor.Saturation);
            Utility.SetNumericUpDownValue(valueUpDown, hsvColor.Value);

            Utility.SetNumericUpDownValue(redUpDown, rgbColor.R);
            Utility.SetNumericUpDownValue(greenUpDown, rgbColor.G);
            Utility.SetNumericUpDownValue(blueUpDown, rgbColor.B);
            PopIgnoreChangedEvents();
            Utility.SetNumericUpDownValue(alphaUpDown, rgbColor.A);

            string hexText = GetHexNumericUpDownValue(rgbColor.R, rgbColor.G, rgbColor.B);
            hexBox.Text = hexText;

            ColorBgra color = ColorBgra.FromColor(rgbColor);

            switch (WhichUserColor)
            {
                case WhichUserColor.Primary:
                    UserPrimaryColor = color;
                    OnRelinquishFocus();
                    break;

                case WhichUserColor.Secondary:
                    UserSecondaryColor = color;
                    OnRelinquishFocus();
                    break;

                default:
                    throw new InvalidEnumArgumentException("WhichUserColor property");
            }

            Update();
        }

        private void HsvGradientControl_ValueChanged(object sender, IndexEventArgs e)
        {
            if (IgnoreChangedEvents)
            {
                return;
            }

            int hue;
            if (sender == hueGradientControl)
            {
                hue = (hueGradientControl.Value * 360) / 255;
            }
            else
            {
                hue = (int)hueUpDown.Value;
            }

            int saturation;
            if (sender == saturationGradientControl)
            {
                saturation = (saturationGradientControl.Value * 100) / 255;
            }
            else
            {
                saturation = (int)saturationUpDown.Value;
            }

            int value;
            if (sender == valueGradientControl)
            {
                value = (valueGradientControl.Value * 100) / 255;
            }
            else
            {
                value = (int)valueUpDown.Value;
            }

            HsvColor hsvColor = new HsvColor(hue, saturation, value);
            colorWheel.HsvColor = hsvColor;
            RgbColor rgbColor = hsvColor.ToRgb();
            ColorBgra color = ColorBgra.FromBgra((byte)rgbColor.Blue, (byte)rgbColor.Green, (byte)rgbColor.Red, (byte)alphaUpDown.Value);

            Utility.SetNumericUpDownValue(hueUpDown, hsvColor.Hue);
            Utility.SetNumericUpDownValue(saturationUpDown, hsvColor.Saturation);
            Utility.SetNumericUpDownValue(valueUpDown, hsvColor.Value);

            Utility.SetNumericUpDownValue(redUpDown, rgbColor.Red);
            Utility.SetNumericUpDownValue(greenUpDown, rgbColor.Green);
            Utility.SetNumericUpDownValue(blueUpDown, rgbColor.Blue);
            
            string hexText = GetHexNumericUpDownValue(rgbColor.Red, rgbColor.Green, rgbColor.Blue);
            hexBox.Text = hexText;
            
            switch (WhichUserColor)
            {
                case WhichUserColor.Primary:
                    UserPrimaryColor = color;
                    OnRelinquishFocus();
                    break;

                case WhichUserColor.Secondary:
                    UserSecondaryColor = color;
                    OnRelinquishFocus();
                    break;

                default:
                    throw new InvalidEnumArgumentException("WhichUserColor property");
            }

            Update();
        }

        private void UpDown_Enter(object sender, System.EventArgs e)
        {
            NumericUpDown nud = (NumericUpDown)sender;
            nud.Select(0, nud.Text.Length);
        }

        private void UpDown_Leave(object sender, System.EventArgs e)
        {
            UpDown_ValueChanged(sender, e);
        }

        private void HexUpDown_Enter(object sender, System.EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            tb.Select(0, tb.Text.Length);
        }

        private void HexUpDown_Leave(object sender, System.EventArgs e)
        {
            hexBox.Text = hexBox.Text.ToUpper();
            UpDown_ValueChanged(sender, e);
        }

        private void HexUpDown_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            TextBox tb = (TextBox)sender;

            if (CheckHexBox(tb.Text))
            {
                UpDown_ValueChanged(sender, e);
            }
        }

        private bool CheckHexBox(String checkHex)
        {
            int num;
        
            try
            {
                num = int.Parse(checkHex, System.Globalization.NumberStyles.HexNumber);
            }

            catch (FormatException)
            {
                return false;
            }

            catch (OverflowException)
            {
                return false;
            }
        
            if ((num <= 16777215) && (num >= 0))
            {
                return true;
            }   
            else
            {
                return false;
            }
        }

        private void UpDown_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            NumericUpDown nud = (NumericUpDown)sender;

            if (Utility.CheckNumericUpDown(nud))
            {
                UpDown_ValueChanged(sender, e);
            }
        }

        private void UpDown_ValueChanged(object sender, System.EventArgs e)
        {
            if (sender == alphaUpDown || sender == alphaGradientControl)
            {
                if (sender == alphaGradientControl)
                {
                    if (alphaUpDown.Value != (decimal)alphaGradientControl.Value)
                    {
                        alphaUpDown.Value = (decimal)alphaGradientControl.Value;
                    }
                }
                else
                {
                    if (alphaGradientControl.Value != (int)alphaUpDown.Value)
                    {
                        alphaGradientControl.Value = (int)alphaUpDown.Value;
                    }
                }

                PushIgnoreChangedEvents();

                switch (WhichUserColor)
                {
                    case WhichUserColor.Primary:
                        ColorBgra newPrimaryColor = ColorBgra.FromBgra(lastPrimaryColor.B, lastPrimaryColor.G, lastPrimaryColor.R, (byte)alphaGradientControl.Value);
                        this.userPrimaryColor = newPrimaryColor;
                        OnUserPrimaryColorChanged(newPrimaryColor);
                        break;

                    case WhichUserColor.Secondary:
                        ColorBgra newSecondaryColor = ColorBgra.FromBgra(lastSecondaryColor.B, lastSecondaryColor.G, lastSecondaryColor.R, (byte)alphaGradientControl.Value);
                        this.userSecondaryColor = newSecondaryColor;
                        OnUserSecondaryColorChanged(newSecondaryColor);
                        break;

                    default:
                        throw new InvalidEnumArgumentException("WhichUserColor property");
                }

                PopIgnoreChangedEvents();
                Update();
            }
            else if (IgnoreChangedEvents)
            {
                return;
            }
            else
            {
                PushIgnoreChangedEvents();

                if (sender == redUpDown || sender == greenUpDown || sender == blueUpDown)
                {
                    string hexText = GetHexNumericUpDownValue((int)redUpDown.Value, (int)greenUpDown.Value, (int)blueUpDown.Value);
                    hexBox.Text = hexText;

                    ColorBgra rgbColor = ColorBgra.FromBgra((byte)blueUpDown.Value, (byte)greenUpDown.Value, (byte)redUpDown.Value, (byte)alphaUpDown.Value);

                    SetColorGradientMinMaxColorsRgb(rgbColor.R, rgbColor.G, rgbColor.B);
                    SetColorGradientMinMaxColorsAlpha(rgbColor.A);
                    SetColorGradientValuesRgb(rgbColor.R, rgbColor.G, rgbColor.B);
                    SetColorGradientMinMaxColorsAlpha(rgbColor.A);

                    SyncHsvFromRgb(rgbColor);
                    OnUserColorChanged(rgbColor);
                }
                else if (sender == hexBox)
                {
                    int hexInt = 0;

                    if (hexBox.Text.Length > 0)
                    {
                        try
                        {
                            hexInt = int.Parse(hexBox.Text,System.Globalization.NumberStyles.HexNumber);
                        }

                        // Needs to be changed so it reads what the RGB values were last
                        catch (FormatException)
                        {
                            hexInt = 0;
                            hexBox.Text = "";
                        }

                        catch (OverflowException)
                        {
                            hexInt = 16777215;
                            hexBox.Text = "FFFFFF";
                        }
        
                        if (!((hexInt <= 16777215) && (hexInt >= 0)))
                        {
                            hexInt = 16777215;
                            hexBox.Text = "FFFFFF";
                        }   
                    }

                    int newRed = ((hexInt & 0xff0000) >> 16);
                    int newGreen = ((hexInt & 0x00ff00) >> 8);
                    int newBlue = (hexInt & 0x0000ff);
                
                    Utility.SetNumericUpDownValue(redUpDown, newRed);
                    Utility.SetNumericUpDownValue(greenUpDown, newGreen);
                    Utility.SetNumericUpDownValue(blueUpDown, newBlue);

                    SetColorGradientMinMaxColorsRgb(newRed, newGreen, newBlue);
                    SetColorGradientValuesRgb(newRed, newGreen, newBlue);
                    SetColorGradientMinMaxColorsAlpha((int)alphaUpDown.Value);

                    ColorBgra rgbColor = ColorBgra.FromBgra((byte)newBlue, (byte)newGreen, (byte)newRed, (byte)alphaUpDown.Value);
                    SyncHsvFromRgb(rgbColor);
                    OnUserColorChanged(rgbColor);
                }
                else if (sender == hueUpDown || sender == saturationUpDown || sender == valueUpDown)
                {
                    HsvColor oldHsvColor = colorWheel.HsvColor;
                    HsvColor newHsvColor = new HsvColor((int)hueUpDown.Value, (int)saturationUpDown.Value, (int)valueUpDown.Value);

                    if (oldHsvColor != newHsvColor)
                    {
                        colorWheel.HsvColor = newHsvColor;

                        SetColorGradientValuesHsv(newHsvColor.Hue, newHsvColor.Saturation, newHsvColor.Value);
                        SetColorGradientMinMaxColorsHsv(newHsvColor.Hue, newHsvColor.Saturation, newHsvColor.Value);

                        SyncRgbFromHsv(newHsvColor);
                        RgbColor rgbColor = newHsvColor.ToRgb();
                        OnUserColorChanged(ColorBgra.FromBgra((byte)rgbColor.Blue, (byte)rgbColor.Green, (byte)rgbColor.Red, (byte)alphaUpDown.Value));
                    }
                }

                PopIgnoreChangedEvents();
            }
        }

        private void PushIgnoreChangedEvents()
        {
            ++this.ignoreChangedEvents;
        }

        private void PopIgnoreChangedEvents()
        {
            --this.ignoreChangedEvents;
        }

        private void MoreLessButton_Click(object sender, System.EventArgs e)
        {
            OnRelinquishFocus();

            this.SuspendLayout();

            if (this.inMoreState)
            {
                this.inMoreState = false;
                Size newSize = lessSize;
                this.moreLessButton.Text = this.moreText;

                int heightDelta = (moreModeHeaderSentinel.Height - lessModeHeaderSentinel.Height);

                newSize.Height -= heightDelta;
                newSize.Height -= SystemLayer.UI.ScaleHeight(18);

                this.ClientSize = newSize;
            }
            else
            {
                this.inMoreState = true;
                this.moreLessButton.Text = this.lessText;

                this.ClientSize = moreSize;
            }

            this.swatchControl.Height = this.ClientSize.Height - SystemLayer.UI.ScaleHeight(4) - this.swatchControl.Top;

            this.ResumeLayout(false);
        }

        private void ColorAddButton_Click(object sender, EventArgs e)
        {
            if (this.colorAddButton.Checked)
            {
                this.colorAddButton.Checked = false;
                this.swatchControl.BlinkHighlight = false;
            }
            else
            {
                this.colorAddButton.Checked = true;
                this.swatchControl.BlinkHighlight = true;
            }
        }

        private ColorBgra GetColorFromUpDowns()
        {
            int r = (int)this.redUpDown.Value;
            int g = (int)this.greenUpDown.Value;
            int b = (int)this.blueUpDown.Value;
            int a = (int)this.alphaUpDown.Value;

            return ColorBgra.FromBgra((byte)b, (byte)g, (byte)r, (byte)a);
        }

        private void SwatchControl_ColorClicked(object sender, EventArgs<Pair<int, MouseButtons>> e)
        {
            List<ColorBgra> colors = new List<ColorBgra>(this.swatchControl.Colors);

            if (this.colorAddButton.Checked)
            {
                colors[e.Data.First] = GetColorFromUpDowns();
                this.swatchControl.Colors = colors.ToArray();
                this.colorAddButton.Checked = false;
                this.swatchControl.BlinkHighlight = false;
            }
            else
            {
                ColorBgra color = colors[e.Data.First];

                if (e.Data.Second == MouseButtons.Right)
                {
                    SetUserColors(UserPrimaryColor, color);
                }
                else
                {
                    switch (this.WhichUserColor)
                    {
                        case WhichUserColor.Primary:
                            this.UserPrimaryColor = color;
                            break;

                        case WhichUserColor.Secondary:
                            this.UserSecondaryColor = color;
                            break;

                        default:
                            throw new InvalidEnumArgumentException();
                    }
                }
            }

            OnRelinquishFocus();
        }

        private void ColorPalettesButton_Click(object sender, EventArgs e)
        {
            this.colorPalettesButton.ShowDropDown();
        }

        private void ColorPalettesButton_DropDownOpening(object sender, EventArgs e)
        {
            this.colorPalettesButton.DropDownItems.Clear();

            if (this.paletteCollection != null)
            {
                using (new WaitCursorChanger(this))
                {
                    this.paletteCollection.Load();
                }

                string[] paletteNames = this.paletteCollection.PaletteNames;

                foreach (string paletteName in paletteNames)
                {
                    this.colorPalettesButton.DropDownItems.Add(
                        paletteName,
                        PdnResources.GetImageResource("Icons.SwatchIcon.png").Reference, 
                        OnPaletteClickedHandler);
                }

                if (paletteNames.Length > 0)
                {
                    this.colorPalettesButton.DropDownItems.Add(new ToolStripSeparator());
                }
            }            

            this.colorPalettesButton.DropDownItems.Add(
                PdnResources.GetString("ColorsForm.ColorPalettesButton.SaveCurrentPaletteAs.Text"),
                PdnResources.GetImageResource("Icons.SavePaletteIcon.png").Reference,
                OnSavePaletteAsHandler);

            this.colorPalettesButton.DropDownItems.Add(
                PdnResources.GetString("ColorsForm.ColorPalettesButton.OpenPalettesFolder.Text"),
                PdnResources.GetImageResource("Icons.ColorPalettes.png").Reference,
                OnOpenPalettesFolderClickedHandler);

            this.colorPalettesButton.DropDownItems.Add(new ToolStripSeparator());

            this.colorPalettesButton.DropDownItems.Add(
                PdnResources.GetString("ColorsForm.ColorPalettesButton.ResetToDefaultPalette.Text"),
                null,
                OnResetPaletteHandler);
        }

        private void OnSavePaletteAsHandler(object sender, EventArgs e)
        {
            using (SavePaletteDialog spd = new SavePaletteDialog())
            {
                spd.PaletteNames = this.paletteCollection.PaletteNames;
                spd.ShowDialog(this);

                if (spd.DialogResult == DialogResult.OK)
                {
                    this.paletteCollection.AddOrUpdate(spd.PaletteName, this.swatchControl.Colors);

                    using (new WaitCursorChanger(this))
                    {
                        this.paletteCollection.Save();
                    }
                }
            }
        }

        private void OnResetPaletteHandler(object sender, EventArgs e)
        {
            this.swatchControl.Colors = PaletteCollection.DefaultPalette;
        }

        private void OnPaletteClickedHandler(object sender, EventArgs e)
        {
            ToolStripItem tsi = sender as ToolStripItem;

            if (tsi != null)
            {
                ColorBgra[] palette = this.paletteCollection.Get(tsi.Text);

                if (palette != null)
                {
                    this.swatchControl.Colors = palette;
                }
            }
        }

        private void SwatchControl_ColorsChanged(object sender, EventArgs e)
        {
            string paletteString = PaletteCollection.GetPaletteSaveString(this.swatchControl.Colors);
            Settings.CurrentUser.SetString(SettingNames.CurrentPalette, paletteString);
        }

        private void OnOpenPalettesFolderClickedHandler(object sender, EventArgs e)
        {
            PaletteCollection.EnsurePalettesPathExists();

            try
            {
                using (new WaitCursorChanger(this))
                {
                    Shell.BrowseFolder(this, PaletteCollection.PalettesPath);
                }
            }

            catch (Exception ex)
            {
                Tracing.Ping("Exception when launching PalettesPath (" + PaletteCollection.PalettesPath + "):" + ex.ToString());
            }
        }
    }
}
