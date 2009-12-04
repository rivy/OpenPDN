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
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics;

namespace PaintDotNet
{
    internal class ResizeDialog 
        : PdnBaseForm
    {
        private sealed class ResizeConstrainer
        {
            private Size originalPixelSize;
            private double newWidth;
            private double newHeight;
            private MeasurementUnit units;
            private double resolution;
            private bool constrainToAspect;
            
            private double OriginalAspect
            {
                get
                {
                    return (double)originalPixelSize.Width / (double)originalPixelSize.Height;
                }
            }

            public Size OriginalPixelSize
            {
                get
                {
                    return this.originalPixelSize;
                }
            }

            public event EventHandler NewWidthChanged;
            private void OnNewWidthChanged()
            {
                if (NewWidthChanged != null)
                {
                    NewWidthChanged(this, EventArgs.Empty);
                }
            }

            public double NewPixelWidth
            {
                get
                {
                    if (this.Units == MeasurementUnit.Pixel)
                    {
                        return this.newWidth;
                    }
                    else
                    {
                        return this.newWidth * this.resolution;
                    }
                }

                set
                {
                    if (this.Units == MeasurementUnit.Pixel)
                    {
                        this.NewWidth = value;
                    }
                    else
                    {
                        this.NewWidth = value / this.resolution;
                    }
                }
            }

            public double NewWidth
            {
                get
                {
                    return this.newWidth;
                }

                set
                {
                    if (this.newWidth != value)
                    {
                        this.newWidth = value;
                        OnNewWidthChanged();

                        if (this.constrainToAspect)
                        {
                            double newNewHeight = value / OriginalAspect;

                            if (this.newHeight != newNewHeight)
                            {
                                this.newHeight = newNewHeight;
                                OnNewHeightChanged();
                            }
                        }
                    }
                }
            }

            public event EventHandler NewHeightChanged;
            private void OnNewHeightChanged()
            {
                if (NewHeightChanged != null)
                {
                    NewHeightChanged(this, EventArgs.Empty);
                }
            }

            public double NewPixelHeight
            {
                get
                {
                    if (this.Units == MeasurementUnit.Pixel)
                    {
                        return this.newHeight;
                    }
                    else
                    {
                        return this.newHeight * this.resolution;
                    }
                }

                set
                {
                    if (this.Units == MeasurementUnit.Pixel)
                    {
                        this.NewHeight = value;
                    }
                    else
                    {
                        this.NewHeight = value / this.resolution;
                    }
                }
            }

            public double NewHeight
            {
                get
                {
                    return this.newHeight;
                }

                set
                {
                    if (this.newHeight != value)
                    {
                        this.newHeight = value;
                        OnNewHeightChanged();

                        if (this.constrainToAspect)
                        {
                            double newNewWidth = value * OriginalAspect;

                            if (this.newWidth != newNewWidth)
                            {
                                this.newWidth = newNewWidth;
                                OnNewWidthChanged();
                            }
                        }
                    }
                }
            }

            public event EventHandler UnitsChanged;
            private void OnUnitsChanged()
            {
                if (UnitsChanged != null)
                {
                    UnitsChanged(this, EventArgs.Empty);
                }
            }

            public MeasurementUnit Units
            {
                get
                {
                    return this.units;
                }

                set
                {
                    if (this.units != value)
                    {
                        switch (value)
                        {
                            default:
                                throw new InvalidEnumArgumentException("value is not a valid member of the MeasurementUnit enumeration");

                            // Inches or Centimers -> Pixels
                            case MeasurementUnit.Pixel:
                                this.newWidth *= this.resolution;
                                this.newHeight *= this.resolution;
                                this.units = value;

                                OnUnitsChanged();
                                OnNewWidthChanged();
                                OnNewHeightChanged();
                                break;

                            case MeasurementUnit.Inch:
                            {
                                switch (this.units)
                                {
                                    default:
                                        throw new InvalidEnumArgumentException("this.units is not a valid member of the MeasurementUnit enumeration");

                                    // Centimeters -> Inches
                                    case MeasurementUnit.Centimeter:
                                        this.newWidth = Document.CentimetersToInches(this.newWidth);
                                        this.newHeight = Document.CentimetersToInches(this.newHeight);
                                        this.units = value;
                                        this.resolution = Document.InchesToCentimeters(this.resolution);
                                  
                                        OnUnitsChanged();
                                        OnResolutionChanged();
                                        OnNewWidthChanged();
                                        OnNewHeightChanged();
                                        break;
                                }
                                break;
                            }

                            case MeasurementUnit.Centimeter:
                            {
                                switch (this.units)
                                {
                                    default:
                                        throw new InvalidEnumArgumentException("this.units is not a valid member of the MeasurementUnit enumeration");

                                    // Inches -> Centimeters
                                    case MeasurementUnit.Inch:
                                        this.newWidth = Document.InchesToCentimeters(this.newWidth);
                                        this.newHeight = Document.InchesToCentimeters(this.newHeight);
                                        this.units = value;
                                        this.resolution = Document.CentimetersToInches(this.resolution);
                                   
                                        OnUnitsChanged();
                                        OnResolutionChanged();
                                        OnNewWidthChanged();
                                        OnNewHeightChanged();
                                        break;
                                }
                                break;
                            }
                        }
                    }
                }
            }

            public event EventHandler ResolutionChanged;
            private void OnResolutionChanged()
            {
                if (ResolutionChanged != null)
                {
                    ResolutionChanged(this, EventArgs.Empty);
                }
            }

            public const double MinResolution = 0.01;

            public double Resolution
            {
                get
                {
                    return this.resolution;
                }

                set
                {
                    if (value < MinResolution)
                    {
                        throw new ArgumentOutOfRangeException("value", value, "value must be >= 0.01");
                    }

                    if (this.resolution != value)
                    {
                        if (this.Units != MeasurementUnit.Pixel)
                        {
                            this.newWidth = ((double)this.newWidth * this.resolution) / value;
                            this.newHeight = ((double)this.newHeight * this.resolution) / value;
                        }

                        this.resolution = value;
                        OnResolutionChanged();

                        if (this.Units != MeasurementUnit.Pixel)
                        {
                            OnNewWidthChanged();
                            OnNewHeightChanged();
                        }
                    }
                }
            }

            public event EventHandler ConstrainToAspectChanged;
            private void OnConstrainToAspectChanged()
            {
                if (ConstrainToAspectChanged != null)
                {
                    ConstrainToAspectChanged(this, EventArgs.Empty);
                }
            }

            public bool ConstrainToAspect
            {
                get
                {
                    return this.constrainToAspect;
                }

                set
                {
                    if (this.constrainToAspect != value)
                    {
                        if (value)
                        {
                            double newNewHeight = this.newWidth / this.OriginalAspect;

                            if (this.newHeight != newNewHeight)
                            {
                                this.newHeight = newNewHeight;
                                OnNewHeightChanged();
                            }
                        }

                        this.constrainToAspect = value;
                        this.OnConstrainToAspectChanged();
                    }
                }
            }

            public void SetByPercent(double scale)
            {
                bool oldConstrain = this.constrainToAspect;
                this.constrainToAspect = false;
                this.NewPixelWidth = (double)this.OriginalPixelSize.Width * scale;
                this.NewPixelHeight = (double)this.OriginalPixelSize.Height * scale;
                this.constrainToAspect = true;
            }

            public ResizeConstrainer(Size originalPixelSize)
            {
                this.constrainToAspect = false;
                this.originalPixelSize = originalPixelSize;
                this.units = Document.DefaultDpuUnit;
                this.resolution = Document.GetDefaultDpu(this.units);
                this.newWidth = (double)this.originalPixelSize.Width / this.resolution;
                this.newHeight = (double)this.originalPixelSize.Height / this.resolution;
            }
        }            

        protected System.Windows.Forms.CheckBox constrainCheckBox;
        protected System.Windows.Forms.Button okButton;
        protected System.Windows.Forms.Button cancelButton;

        private EventHandler upDownValueChangedDelegate;

        private int layers;
        
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;
        protected System.Windows.Forms.Label percentSignLabel;
        protected System.Windows.Forms.NumericUpDown percentUpDown;
        protected System.Windows.Forms.RadioButton absoluteRB;
        protected System.Windows.Forms.RadioButton percentRB;
        protected System.Windows.Forms.Label asteriskTextLabel;
        protected System.Windows.Forms.Label asteriskLabel;
        protected PaintDotNet.HeaderLabel resizedImageHeader;
        protected System.Windows.Forms.Label resolutionLabel;
        protected PaintDotNet.UnitsComboBox unitsComboBox2;
        protected PaintDotNet.UnitsComboBox unitsComboBox1;
        protected System.Windows.Forms.NumericUpDown resolutionUpDown;

        private ResizeConstrainer constrainer;
        protected System.Windows.Forms.Label newWidthLabel1;
        protected System.Windows.Forms.Label newHeightLabel1;
        protected System.Windows.Forms.Label pixelsLabel1;
        protected System.Windows.Forms.Label newWidthLabel2;
        protected System.Windows.Forms.Label newHeightLabel2;
        protected System.Windows.Forms.Label pixelsLabel2;
        protected System.Windows.Forms.Label unitsLabel1;
        protected System.Windows.Forms.NumericUpDown pixelWidthUpDown;
        protected System.Windows.Forms.NumericUpDown pixelHeightUpDown;
        protected System.Windows.Forms.NumericUpDown printWidthUpDown;
        protected System.Windows.Forms.NumericUpDown printHeightUpDown;
        protected PaintDotNet.HeaderLabel pixelSizeHeader;
        protected PaintDotNet.HeaderLabel printSizeHeader;
        protected System.Windows.Forms.Label resamplingLabel;
        protected System.Windows.Forms.ComboBox resamplingAlgorithmComboBox;

        /// <summary>
        /// Gets or sets the image width, in units of pixels.
        /// </summary>
        public int ImageWidth
        {
            get
            {
                double doubleVal;

                if (!Utility.GetUpDownValueFromText(this.pixelWidthUpDown, out doubleVal))
                {
                    doubleVal = Math.Round(constrainer.NewPixelWidth);
                }

                int intVal = (int)Utility.Clamp(doubleVal, (double)int.MinValue, (double)int.MaxValue);

                return intVal;
            }

            set
            {
                this.constrainer.NewPixelWidth = value;
            }
        }

        /// <summary>
        /// Gets or sets the new image height, in units of pixels.
        /// </summary>
        public int ImageHeight
        {
            get
            {
                double doubleVal;

                if (!Utility.GetUpDownValueFromText(this.pixelHeightUpDown, out doubleVal))
                {
                    doubleVal = Math.Round(constrainer.NewPixelHeight);
                }

                int intVal = (int)Utility.Clamp(doubleVal, (double)int.MinValue, (double)int.MaxValue);

                return intVal;
            }

            set
            {
                this.constrainer.NewPixelHeight = value;
            }
        }

        public MeasurementUnit Units
        {
            get
            {
                return this.constrainer.Units;
            }

            set
            {
                this.constrainer.Units = value;
            }
        }

        public double Resolution
        {
            get
            {
                return this.constrainer.Resolution;
            }

            set
            {
                this.constrainer.Resolution = Math.Max(ResizeConstrainer.MinResolution, value);
            }
        }

        private double originalDpu = Document.GetDefaultDpu(Document.DefaultDpuUnit);
        private MeasurementUnit originalDpuUnit = Document.DefaultDpuUnit;

        /// <summary>
        /// Gets or sets the original image width, in units of pixels.
        /// </summary>
        /// <remarks>
        /// Setting this property will reset the Resolution and Units properties.
        /// </remarks>
        public Size OriginalSize
        {
            get
            {
                return this.constrainer.OriginalPixelSize;
            }

            set
            {
                this.constrainer = new ResizeConstrainer(value);
                SetupConstrainerEvents();
                UpdateSizeText();
            }
        }

        public double OriginalDpu
        {
            get
            {
                return this.originalDpu;
            }

            set
            {
                this.originalDpu = value;
                UpdateSizeText();
            }
        }

        public MeasurementUnit OriginalDpuUnit
        {
            get
            {
                return this.originalDpuUnit;
            }

            set
            {
                this.originalDpuUnit = value;
                UpdateSizeText();
            }
        }

        /// <summary>
        /// Gets the resampling algorithm chosen by the user, or sets the resampling algorithm to populate the UI with.
        /// </summary>
        public ResamplingAlgorithm ResamplingAlgorithm
        {
            get
            {
                return ((ResampleMethod)this.resamplingAlgorithmComboBox.SelectedItem).method;
            }
            
            set
            {
                this.resamplingAlgorithmComboBox.SelectedItem = new ResampleMethod(value);
                PopulateAsteriskLabels();
            }
        }

        public bool ConstrainToAspect
        {
            get
            {
                return this.constrainer.ConstrainToAspect;
            }

            set
            {
                this.constrainer.ConstrainToAspect = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of layers in the image.
        /// </summary>
        /// <remarks>
        /// This is used to compute the new byte size of the image that is shown to the user.
        /// </remarks>
        public int LayerCount
        {
            get
            {
                return layers;
            }

            set
            {
                layers = value;
                UpdateSizeText();
            }
        }

        private void UpdateSizeText()
        {
            long bytes = unchecked((long)layers * (long)ColorBgra.SizeOf * (long)constrainer.NewPixelWidth * (long)constrainer.NewPixelHeight);
            string bytesText = Utility.SizeStringFromBytes(bytes);
            string textFormat = PdnResources.GetString("ResizeDialog.ResizedImageHeader.Text.Format");
            this.resizedImageHeader.Text = string.Format(textFormat, bytesText);
        }

        private sealed class ResampleMethod
        {
            public ResamplingAlgorithm method;

            public override string ToString()
            {
                switch (method)
                {
                    case ResamplingAlgorithm.NearestNeighbor:
                        return PdnResources.GetString("ResizeDialog.ResampleMethod.NearestNeighbor");

                    case ResamplingAlgorithm.Bilinear:
                        return PdnResources.GetString("ResizeDialog.ResampleMethod.Bilinear");

                    case ResamplingAlgorithm.Bicubic:
                        return PdnResources.GetString("ResizeDialog.ResampleMethod.Bicubic");

                    case ResamplingAlgorithm.SuperSampling:
                        return PdnResources.GetString("ResizeDialog.ResampleMethod.SuperSampling");

                    default:
                        return method.ToString();
                }
            }

            public override bool Equals(object obj)
            {
                if (obj is ResampleMethod && ((ResampleMethod)obj).method == this.method)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public override int GetHashCode()
            {
                return this.method.GetHashCode();
            }

            public ResampleMethod(ResamplingAlgorithm method)
            {
                this.method = method;
            }
        }

        public ResizeDialog()
        {
            this.SuspendLayout(); // ResumeLayout() called in OnLoad(). This helps with layout w.r.t. visual inheritance (CanvasSizeDialog and NewFileDialog)
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            this.Text = PdnResources.GetString("ResizeDialog.Text");
            this.asteriskLabel.Text = PdnResources.GetString("ResizeDialog.AsteriskLabel.Text");
            this.percentSignLabel.Text = PdnResources.GetString("ResizeDialog.PercentSignLabel.Text");
            this.pixelSizeHeader.Text = PdnResources.GetString("ResizeDialog.PixelSizeHeader.Text");
            this.printSizeHeader.Text = PdnResources.GetString("ResizeDialog.PrintSizeHeader.Text");
            this.pixelsLabel1.Text = PdnResources.GetString("ResizeDialog.PixelsLabel1.Text");
            this.pixelsLabel2.Text = PdnResources.GetString("ResizeDialog.PixelsLabel2.Text");
            this.resolutionLabel.Text = PdnResources.GetString("ResizeDialog.ResolutionLabel.Text");
            this.percentRB.Text = PdnResources.GetString("ResizeDialog.PercentRB.Text");
            this.absoluteRB.Text = PdnResources.GetString("ResizeDialog.AbsoluteRB.Text");
            this.resamplingLabel.Text = PdnResources.GetString("ResizeDialog.ResamplingLabel.Text");
            this.cancelButton.Text = PdnResources.GetString("Form.CancelButton.Text");
            this.okButton.Text = PdnResources.GetString("Form.OkButton.Text");
            this.newWidthLabel1.Text = PdnResources.GetString("ResizeDialog.NewWidthLabel1.Text");
            this.newHeightLabel1.Text = PdnResources.GetString("ResizeDialog.NewHeightLabel1.Text");
            this.newWidthLabel2.Text = PdnResources.GetString("ResizeDialog.NewWidthLabel1.Text");
            this.newHeightLabel2.Text = PdnResources.GetString("ResizeDialog.NewHeightLabel1.Text");
            this.constrainCheckBox.Text = PdnResources.GetString("ResizeDialog.ConstrainCheckBox.Text");
            this.unitsLabel1.Text = unitsComboBox1.UnitsText;

            upDownValueChangedDelegate = new EventHandler(upDown_ValueChanged);

            this.constrainer = new ResizeConstrainer(new Size((int)this.pixelWidthUpDown.Value, (int)this.pixelHeightUpDown.Value));
            SetupConstrainerEvents();

            resamplingAlgorithmComboBox.Items.Clear();
            resamplingAlgorithmComboBox.Items.Add(new ResampleMethod(ResamplingAlgorithm.Bicubic));
            resamplingAlgorithmComboBox.Items.Add(new ResampleMethod(ResamplingAlgorithm.Bilinear));
            resamplingAlgorithmComboBox.Items.Add(new ResampleMethod(ResamplingAlgorithm.NearestNeighbor));
            resamplingAlgorithmComboBox.Items.Add(new ResampleMethod(ResamplingAlgorithm.SuperSampling));
            resamplingAlgorithmComboBox.SelectedItem = new ResampleMethod(ResamplingAlgorithm.SuperSampling);

            layers = 1;

            this.percentUpDown.Enabled = false;

            this.Icon = Utility.ImageToIcon(PdnResources.GetImageResource("Icons.MenuImageResizeIcon.png").Reference, Utility.TransparentKey);
            PopulateAsteriskLabels();
            OnRadioButtonCheckedChanged(this, EventArgs.Empty);    
        }

        private void SetupConstrainerEvents()
        {
            this.constrainer.ConstrainToAspectChanged += new EventHandler(OnConstrainerConstrainToAspectChanged);
            this.constrainer.NewHeightChanged += new EventHandler(constrainer_NewHeightChanged);
            this.constrainer.NewWidthChanged += new EventHandler(OnConstrainerNewWidthChanged);
            this.constrainer.ResolutionChanged += new EventHandler(OnConstrainerResolutionChanged);
            this.constrainer.UnitsChanged += new EventHandler(OnConstrainerUnitsChanged);

            constrainCheckBox.Checked = constrainer.ConstrainToAspect;
            SafeSetNudValue(this.pixelWidthUpDown, this.constrainer.NewPixelWidth);
            SafeSetNudValue(this.pixelHeightUpDown, this.constrainer.NewPixelHeight);
            SafeSetNudValue(this.printWidthUpDown, this.constrainer.NewWidth);
            SafeSetNudValue(this.printHeightUpDown, this.constrainer.NewHeight);
            SafeSetNudValue(this.resolutionUpDown, this.constrainer.Resolution);
            unitsComboBox1.Units = constrainer.Units;
        }

        protected override void OnLoad(EventArgs e)
        {
            this.ResumeLayout(true); // SuspendLayout() was called in constructor
            base.OnLoad(e);
            this.pixelWidthUpDown.Select();
            this.pixelWidthUpDown.Select(0, pixelWidthUpDown.Text.Length);
            this.PopulateAsteriskLabels();
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
            this.constrainCheckBox = new System.Windows.Forms.CheckBox();
            this.newWidthLabel1 = new System.Windows.Forms.Label();
            this.newHeightLabel1 = new System.Windows.Forms.Label();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.pixelWidthUpDown = new System.Windows.Forms.NumericUpDown();
            this.pixelHeightUpDown = new System.Windows.Forms.NumericUpDown();
            this.resizedImageHeader = new PaintDotNet.HeaderLabel();
            this.asteriskLabel = new System.Windows.Forms.Label();
            this.asteriskTextLabel = new System.Windows.Forms.Label();
            this.absoluteRB = new System.Windows.Forms.RadioButton();
            this.percentRB = new System.Windows.Forms.RadioButton();
            this.pixelsLabel1 = new System.Windows.Forms.Label();
            this.percentUpDown = new System.Windows.Forms.NumericUpDown();
            this.percentSignLabel = new System.Windows.Forms.Label();
            this.resolutionLabel = new System.Windows.Forms.Label();
            this.resolutionUpDown = new System.Windows.Forms.NumericUpDown();
            this.unitsComboBox2 = new PaintDotNet.UnitsComboBox();
            this.unitsComboBox1 = new PaintDotNet.UnitsComboBox();
            this.printWidthUpDown = new System.Windows.Forms.NumericUpDown();
            this.printHeightUpDown = new System.Windows.Forms.NumericUpDown();
            this.newWidthLabel2 = new System.Windows.Forms.Label();
            this.newHeightLabel2 = new System.Windows.Forms.Label();
            this.pixelsLabel2 = new System.Windows.Forms.Label();
            this.unitsLabel1 = new System.Windows.Forms.Label();
            this.pixelSizeHeader = new PaintDotNet.HeaderLabel();
            this.printSizeHeader = new PaintDotNet.HeaderLabel();
            this.resamplingLabel = new System.Windows.Forms.Label();
            this.resamplingAlgorithmComboBox = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.pixelWidthUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pixelHeightUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.percentUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.resolutionUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.printWidthUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.printHeightUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // constrainCheckBox
            // 
            this.constrainCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.constrainCheckBox.Location = new System.Drawing.Point(27, 101);
            this.constrainCheckBox.Name = "constrainCheckBox";
            this.constrainCheckBox.Size = new System.Drawing.Size(248, 16);
            this.constrainCheckBox.TabIndex = 25;
            this.constrainCheckBox.CheckedChanged += new System.EventHandler(this.constrainCheckBox_CheckedChanged);
            // 
            // newWidthLabel1
            // 
            this.newWidthLabel1.Location = new System.Drawing.Point(32, 145);
            this.newWidthLabel1.Name = "newWidthLabel1";
            this.newWidthLabel1.Size = new System.Drawing.Size(79, 16);
            this.newWidthLabel1.TabIndex = 0;
            this.newWidthLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // newHeightLabel1
            // 
            this.newHeightLabel1.Location = new System.Drawing.Point(32, 169);
            this.newHeightLabel1.Name = "newHeightLabel1";
            this.newHeightLabel1.Size = new System.Drawing.Size(79, 16);
            this.newHeightLabel1.TabIndex = 3;
            this.newHeightLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.okButton.Location = new System.Drawing.Point(142, 315);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(72, 23);
            this.okButton.TabIndex = 17;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cancelButton.Location = new System.Drawing.Point(220, 315);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(72, 23);
            this.cancelButton.TabIndex = 18;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // pixelWidthUpDown
            // 
            this.pixelWidthUpDown.Location = new System.Drawing.Point(120, 144);
            this.pixelWidthUpDown.Maximum = new System.Decimal(new int[] {
                                                                             2147483647,
                                                                             0,
                                                                             0,
                                                                             0});
            this.pixelWidthUpDown.Minimum = new System.Decimal(new int[] {
                                                                             2147483647,
                                                                             0,
                                                                             0,
                                                                             -2147483648});
            this.pixelWidthUpDown.Name = "pixelWidthUpDown";
            this.pixelWidthUpDown.Size = new System.Drawing.Size(72, 20);
            this.pixelWidthUpDown.TabIndex = 1;
            this.pixelWidthUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.pixelWidthUpDown.Value = new System.Decimal(new int[] {
                                                                           4,
                                                                           0,
                                                                           0,
                                                                           0});
            this.pixelWidthUpDown.Enter += new System.EventHandler(this.OnUpDownEnter);
            this.pixelWidthUpDown.KeyUp += new System.Windows.Forms.KeyEventHandler(this.OnUpDownKeyUp);
            this.pixelWidthUpDown.ValueChanged += new System.EventHandler(this.upDown_ValueChanged);
            this.pixelWidthUpDown.Leave += new System.EventHandler(this.OnUpDownLeave);
            // 
            // pixelHeightUpDown
            // 
            this.pixelHeightUpDown.Location = new System.Drawing.Point(120, 168);
            this.pixelHeightUpDown.Maximum = new System.Decimal(new int[] {
                                                                              2147483647,
                                                                              0,
                                                                              0,
                                                                              0});
            this.pixelHeightUpDown.Minimum = new System.Decimal(new int[] {
                                                                              2147483647,
                                                                              0,
                                                                              0,
                                                                              -2147483648});
            this.pixelHeightUpDown.Name = "pixelHeightUpDown";
            this.pixelHeightUpDown.Size = new System.Drawing.Size(72, 20);
            this.pixelHeightUpDown.TabIndex = 4;
            this.pixelHeightUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.pixelHeightUpDown.Value = new System.Decimal(new int[] {
                                                                            3,
                                                                            0,
                                                                            0,
                                                                            0});
            this.pixelHeightUpDown.Enter += new System.EventHandler(this.OnUpDownEnter);
            this.pixelHeightUpDown.KeyUp += new System.Windows.Forms.KeyEventHandler(this.OnUpDownKeyUp);
            this.pixelHeightUpDown.ValueChanged += new System.EventHandler(this.upDown_ValueChanged);
            this.pixelHeightUpDown.Leave += new System.EventHandler(this.OnUpDownLeave);
            // 
            // resizedImageHeader
            // 
            this.resizedImageHeader.Location = new System.Drawing.Point(6, 8);
            this.resizedImageHeader.Name = "resizedImageHeader";
            this.resizedImageHeader.Size = new System.Drawing.Size(290, 16);
            this.resizedImageHeader.TabIndex = 19;
            this.resizedImageHeader.TabStop = false;
            // 
            // asteriskLabel
            // 
            this.asteriskLabel.Location = new System.Drawing.Point(275, 28);
            this.asteriskLabel.Name = "asteriskLabel";
            this.asteriskLabel.Size = new System.Drawing.Size(13, 16);
            this.asteriskLabel.TabIndex = 15;
            this.asteriskLabel.Visible = false;
            // 
            // asteriskTextLabel
            // 
            this.asteriskTextLabel.Location = new System.Drawing.Point(8, 290);
            this.asteriskTextLabel.Name = "asteriskTextLabel";
            this.asteriskTextLabel.Size = new System.Drawing.Size(255, 16);
            this.asteriskTextLabel.TabIndex = 16;
            this.asteriskTextLabel.Visible = false;
            // 
            // absoluteRB
            // 
            this.absoluteRB.Checked = true;
            this.absoluteRB.Location = new System.Drawing.Point(8, 78);
            this.absoluteRB.Name = "absoluteRB";
            this.absoluteRB.Width = 264;
            this.absoluteRB.AutoSize = true;
            this.absoluteRB.TabIndex = 24;
            this.absoluteRB.TabStop = true;
            this.absoluteRB.FlatStyle = FlatStyle.System;
            this.absoluteRB.CheckedChanged += new System.EventHandler(this.OnRadioButtonCheckedChanged);
            // 
            // percentRB
            // 
            this.percentRB.Location = new System.Drawing.Point(8, 51);
            this.percentRB.Name = "percentRB";
            this.percentRB.TabIndex = 22;
            this.percentRB.AutoSize = true;
            this.percentRB.Width = 10;
            this.percentRB.FlatStyle = FlatStyle.System;
            this.percentRB.CheckedChanged += new System.EventHandler(this.OnRadioButtonCheckedChanged);
            // 
            // pixelsLabel1
            // 
            this.pixelsLabel1.Location = new System.Drawing.Point(200, 145);
            this.pixelsLabel1.Name = "pixelsLabel1";
            this.pixelsLabel1.Width = 93;
            this.pixelsLabel1.TabIndex = 2;
            this.pixelsLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // percentUpDown
            // 
            this.percentUpDown.Location = new System.Drawing.Point(120, 54);
            this.percentUpDown.Maximum = new System.Decimal(new int[] {
                                                                          2000,
                                                                          0,
                                                                          0,
                                                                          0});
            this.percentUpDown.Name = "percentUpDown";
            this.percentUpDown.Size = new System.Drawing.Size(72, 20);
            this.percentUpDown.TabIndex = 23;
            this.percentUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.percentUpDown.Value = new System.Decimal(new int[] {
                                                                        100,
                                                                        0,
                                                                        0,
                                                                        0});
            this.percentUpDown.Enter += new System.EventHandler(this.OnUpDownEnter);
            this.percentUpDown.KeyUp += new System.Windows.Forms.KeyEventHandler(this.OnUpDownKeyUp);
            this.percentUpDown.ValueChanged += new System.EventHandler(this.upDown_ValueChanged);
            // 
            // percentSignLabel
            // 
            this.percentSignLabel.Location = new System.Drawing.Point(200, 55);
            this.percentSignLabel.Name = "percentSignLabel";
            this.percentSignLabel.Size = new System.Drawing.Size(32, 16);
            this.percentSignLabel.TabIndex = 13;
            this.percentSignLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // resolutionLabel
            // 
            this.resolutionLabel.Location = new System.Drawing.Point(32, 193);
            this.resolutionLabel.Name = "resolutionLabel";
            this.resolutionLabel.Size = new System.Drawing.Size(79, 16);
            this.resolutionLabel.TabIndex = 6;
            this.resolutionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // resolutionUpDown
            // 
            this.resolutionUpDown.DecimalPlaces = 2;
            this.resolutionUpDown.Location = new System.Drawing.Point(120, 192);
            this.resolutionUpDown.Maximum = new System.Decimal(new int[] {
                                                                             65535,
                                                                             0,
                                                                             0,
                                                                             0});
            this.resolutionUpDown.Minimum = new System.Decimal(new int[] {
                                                                             1,
                                                                             0,
                                                                             0,
                                                                             327680});
            this.resolutionUpDown.Name = "resolutionUpDown";
            this.resolutionUpDown.Size = new System.Drawing.Size(72, 20);
            this.resolutionUpDown.TabIndex = 7;
            this.resolutionUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.resolutionUpDown.Value = new System.Decimal(new int[] {
                                                                           72,
                                                                           0,
                                                                           0,
                                                                           0});
            this.resolutionUpDown.Enter += new System.EventHandler(this.OnUpDownEnter);
            this.resolutionUpDown.KeyUp += new System.Windows.Forms.KeyEventHandler(this.OnUpDownKeyUp);
            this.resolutionUpDown.ValueChanged += new System.EventHandler(this.upDown_ValueChanged);
            this.resolutionUpDown.Leave += new System.EventHandler(this.OnUpDownLeave);
            // 
            // unitsComboBox2
            // 
            this.unitsComboBox2.Location = new System.Drawing.Point(200, 192);
            this.unitsComboBox2.Name = "unitsComboBox2";
            this.unitsComboBox2.PixelsAvailable = false;
            this.unitsComboBox2.Size = new System.Drawing.Size(88, 21);
            this.unitsComboBox2.TabIndex = 8;
            this.unitsComboBox2.Units = PaintDotNet.MeasurementUnit.Inch;
            this.unitsComboBox2.UnitsDisplayType = PaintDotNet.UnitsDisplayType.Ratio;
            this.unitsComboBox2.UnitsChanged += new System.EventHandler(this.OnUnitsComboBox2UnitsChanged);
            // 
            // unitsComboBox1
            // 
            this.unitsComboBox1.Location = new System.Drawing.Point(200, 235);
            this.unitsComboBox1.Name = "unitsComboBox1";
            this.unitsComboBox1.PixelsAvailable = false;
            this.unitsComboBox1.Size = new System.Drawing.Size(88, 21);
            this.unitsComboBox1.TabIndex = 12;
            this.unitsComboBox1.Units = PaintDotNet.MeasurementUnit.Inch;
            this.unitsComboBox1.UnitsChanged += new System.EventHandler(this.OnUnitsComboBox1UnitsChanged);
            // 
            // printWidthUpDown
            // 
            this.printWidthUpDown.DecimalPlaces = 2;
            this.printWidthUpDown.Location = new System.Drawing.Point(120, 235);
            this.printWidthUpDown.Maximum = new System.Decimal(new int[] {
                                                                             2147483647,
                                                                             0,
                                                                             0,
                                                                             0});
            this.printWidthUpDown.Minimum = new System.Decimal(new int[] {
                                                                             2147483647,
                                                                             0,
                                                                             0,
                                                                             -2147483648});
            this.printWidthUpDown.Name = "printWidthUpDown";
            this.printWidthUpDown.Size = new System.Drawing.Size(72, 20);
            this.printWidthUpDown.TabIndex = 11;
            this.printWidthUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.printWidthUpDown.Value = new System.Decimal(new int[] {
                                                                           2,
                                                                           0,
                                                                           0,
                                                                           0});
            this.printWidthUpDown.Enter += new System.EventHandler(this.OnUpDownEnter);
            this.printWidthUpDown.KeyUp += new System.Windows.Forms.KeyEventHandler(this.OnUpDownKeyUp);
            this.printWidthUpDown.ValueChanged += new System.EventHandler(this.upDown_ValueChanged);
            this.printWidthUpDown.Leave += new System.EventHandler(this.OnUpDownLeave);
            // 
            // printHeightUpDown
            // 
            this.printHeightUpDown.DecimalPlaces = 2;
            this.printHeightUpDown.Location = new System.Drawing.Point(120, 259);
            this.printHeightUpDown.Maximum = new System.Decimal(new int[] {
                                                                              2147483647,
                                                                              0,
                                                                              0,
                                                                              0});
            this.printHeightUpDown.Minimum = new System.Decimal(new int[] {
                                                                              2147483647,
                                                                              0,
                                                                              0,
                                                                              -2147483648});
            this.printHeightUpDown.Name = "printHeightUpDown";
            this.printHeightUpDown.Size = new System.Drawing.Size(72, 20);
            this.printHeightUpDown.TabIndex = 14;
            this.printHeightUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.printHeightUpDown.Value = new System.Decimal(new int[] {
                                                                            1,
                                                                            0,
                                                                            0,
                                                                            0});
            this.printHeightUpDown.Enter += new System.EventHandler(this.OnUpDownEnter);
            this.printHeightUpDown.KeyUp += new System.Windows.Forms.KeyEventHandler(this.OnUpDownKeyUp);
            this.printHeightUpDown.ValueChanged += new System.EventHandler(this.upDown_ValueChanged);
            this.printHeightUpDown.Leave += new System.EventHandler(this.OnUpDownLeave);
            // 
            // newWidthLabel2
            // 
            this.newWidthLabel2.Location = new System.Drawing.Point(32, 236);
            this.newWidthLabel2.Name = "newWidthLabel2";
            this.newWidthLabel2.Size = new System.Drawing.Size(79, 16);
            this.newWidthLabel2.TabIndex = 10;
            this.newWidthLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // newHeightLabel2
            // 
            this.newHeightLabel2.Location = new System.Drawing.Point(32, 260);
            this.newHeightLabel2.Name = "newHeightLabel2";
            this.newHeightLabel2.Size = new System.Drawing.Size(79, 16);
            this.newHeightLabel2.TabIndex = 13;
            this.newHeightLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pixelsLabel2
            // 
            this.pixelsLabel2.Location = new System.Drawing.Point(200, 169);
            this.pixelsLabel2.Name = "pixelsLabel2";
            this.pixelsLabel2.Size = new System.Drawing.Size(93, 16);
            this.pixelsLabel2.TabIndex = 5;
            this.pixelsLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // unitsLabel1
            // 
            this.unitsLabel1.Location = new System.Drawing.Point(200, 261);
            this.unitsLabel1.Name = "unitsLabel1";
            this.unitsLabel1.Size = new System.Drawing.Size(94, 16);
            this.unitsLabel1.TabIndex = 15;
            // 
            // pixelSizeHeader
            // 
            this.pixelSizeHeader.Location = new System.Drawing.Point(25, 125);
            this.pixelSizeHeader.Name = "pixelSizeHeader";
            this.pixelSizeHeader.Size = new System.Drawing.Size(271, 14);
            this.pixelSizeHeader.TabIndex = 26;
            this.pixelSizeHeader.TabStop = false;
            // 
            // printSizeHeader
            // 
            this.printSizeHeader.Location = new System.Drawing.Point(25, 216);
            this.printSizeHeader.Name = "printSizeHeader";
            this.printSizeHeader.Size = new System.Drawing.Size(271, 14);
            this.printSizeHeader.TabIndex = 9;
            this.printSizeHeader.TabStop = false;
            // 
            // resamplingLabel
            // 
            this.resamplingLabel.Location = new System.Drawing.Point(6, 30);
            this.resamplingLabel.Name = "resamplingLabel";
            this.resamplingLabel.AutoSize = true;
            this.resamplingLabel.Size = new System.Drawing.Size(88, 16);
            this.resamplingLabel.TabIndex = 20;
            this.resamplingLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // resamplingAlgorithmComboBox
            // 
            this.resamplingAlgorithmComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.resamplingAlgorithmComboBox.ItemHeight = 13;
            this.resamplingAlgorithmComboBox.Location = new System.Drawing.Point(120, 27);
            this.resamplingAlgorithmComboBox.Name = "resamplingAlgorithmComboBox";
            this.resamplingAlgorithmComboBox.Size = new System.Drawing.Size(152, 21);
            this.resamplingAlgorithmComboBox.Sorted = true;
            this.resamplingAlgorithmComboBox.TabIndex = 21;
            this.resamplingAlgorithmComboBox.FlatStyle = FlatStyle.System;
            this.resamplingAlgorithmComboBox.SelectedIndexChanged += new System.EventHandler(this.OnResamplingAlgorithmComboBoxSelectedIndexChanged);
            // 
            // ResizeDialog
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(298, 344);
            this.Controls.Add(this.printSizeHeader);
            this.Controls.Add(this.pixelSizeHeader);
            this.Controls.Add(this.unitsLabel1);
            this.Controls.Add(this.pixelsLabel2);
            this.Controls.Add(this.newHeightLabel2);
            this.Controls.Add(this.newWidthLabel2);
            this.Controls.Add(this.printHeightUpDown);
            this.Controls.Add(this.printWidthUpDown);
            this.Controls.Add(this.unitsComboBox1);
            this.Controls.Add(this.unitsComboBox2);
            this.Controls.Add(this.resolutionUpDown);
            this.Controls.Add(this.resolutionLabel);
            this.Controls.Add(this.resizedImageHeader);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.asteriskLabel);
            this.Controls.Add(this.asteriskTextLabel);
            this.Controls.Add(this.absoluteRB);
            this.Controls.Add(this.percentRB);
            this.Controls.Add(this.pixelWidthUpDown);
            this.Controls.Add(this.pixelHeightUpDown);
            this.Controls.Add(this.pixelsLabel1);
            this.Controls.Add(this.newHeightLabel1);
            this.Controls.Add(this.newWidthLabel1);
            this.Controls.Add(this.resamplingAlgorithmComboBox);
            this.Controls.Add(this.resamplingLabel);
            this.Controls.Add(this.constrainCheckBox);
            this.Controls.Add(this.percentUpDown);
            this.Controls.Add(this.percentSignLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ResizeDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Controls.SetChildIndex(this.percentSignLabel, 0);
            this.Controls.SetChildIndex(this.percentUpDown, 0);
            this.Controls.SetChildIndex(this.constrainCheckBox, 0);
            this.Controls.SetChildIndex(this.resamplingLabel, 0);
            this.Controls.SetChildIndex(this.resamplingAlgorithmComboBox, 0);
            this.Controls.SetChildIndex(this.newWidthLabel1, 0);
            this.Controls.SetChildIndex(this.newHeightLabel1, 0);
            this.Controls.SetChildIndex(this.pixelsLabel1, 0);
            this.Controls.SetChildIndex(this.pixelHeightUpDown, 0);
            this.Controls.SetChildIndex(this.pixelWidthUpDown, 0);
            this.Controls.SetChildIndex(this.percentRB, 0);
            this.Controls.SetChildIndex(this.absoluteRB, 0);
            this.Controls.SetChildIndex(this.asteriskTextLabel, 0);
            this.Controls.SetChildIndex(this.asteriskLabel, 0);
            this.Controls.SetChildIndex(this.okButton, 0);
            this.Controls.SetChildIndex(this.cancelButton, 0);
            this.Controls.SetChildIndex(this.resizedImageHeader, 0);
            this.Controls.SetChildIndex(this.resolutionLabel, 0);
            this.Controls.SetChildIndex(this.resolutionUpDown, 0);
            this.Controls.SetChildIndex(this.unitsComboBox2, 0);
            this.Controls.SetChildIndex(this.unitsComboBox1, 0);
            this.Controls.SetChildIndex(this.printWidthUpDown, 0);
            this.Controls.SetChildIndex(this.printHeightUpDown, 0);
            this.Controls.SetChildIndex(this.newWidthLabel2, 0);
            this.Controls.SetChildIndex(this.newHeightLabel2, 0);
            this.Controls.SetChildIndex(this.pixelsLabel2, 0);
            this.Controls.SetChildIndex(this.unitsLabel1, 0);
            this.Controls.SetChildIndex(this.pixelSizeHeader, 0);
            this.Controls.SetChildIndex(this.printSizeHeader, 0);
            ((System.ComponentModel.ISupportInitialize)(this.pixelWidthUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pixelHeightUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.percentUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.resolutionUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.printWidthUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.printHeightUpDown)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion

        // We have to delay closing the dialog after clicking OK because otherwise the
        // numbers do not 'sync up' properly when Maintain Aspect Ratio is checked.
        // For example, if you type 200 for width, that means the height goes from 0
        // to 1 to 15 to 150 ... however if you press ENTER immediately after the 2nd 0 in
        // 200, you get an image that is 20x15.
        // See bug #1195.
        private int okTimerInterval = 200;
        private System.Windows.Forms.Timer okTimer = null;

        private void okButton_Click(object sender, System.EventArgs e)
        {
            if (this.okTimer == null)
            {
                this.okTimer = new Timer();
                this.okTimer.Interval = okTimerInterval;
                this.okTimer.Tick += new EventHandler(okTimer_Tick);
                this.okTimer.Enabled = true;
            }
        }

        private void okTimer_Tick(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
            this.okTimer.Dispose();
        }

        private void cancelButton_Click(object sender, System.EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();       
        }

        private void constrainCheckBox_CheckedChanged(object sender, System.EventArgs e)
        {
            this.constrainer.ConstrainToAspect = constrainCheckBox.Checked;
        }

        private int ignoreUpDownValueChanged = 0;
        private int getValueFromText = 0;

        private void upDown_ValueChanged(object sender, System.EventArgs e)
        {
            if (ignoreUpDownValueChanged > 0)
            {
                return;
            }

            double val;

            if (sender == percentUpDown)
            {
                if (getValueFromText > 0)
                {
                    if (Utility.GetUpDownValueFromText(this.percentUpDown, out val))
                    {
                        if (val >= (double)this.percentUpDown.Minimum && 
                            val <= (double)this.percentUpDown.Maximum)
                        {
                            this.constrainer.SetByPercent(val / 100.0);
                        }
                    }
                }
                else
                {
                    this.constrainer.SetByPercent((double)this.percentUpDown.Value / 100.0);
                }
            }

            if (sender == pixelWidthUpDown)
            {
                if (getValueFromText > 0)
                {
                    if (Utility.GetUpDownValueFromText(this.pixelWidthUpDown, out val))
                    {
                        this.constrainer.NewPixelWidth = val;
                    }
                }
                else
                {
                    this.constrainer.NewPixelWidth = (double)this.pixelWidthUpDown.Value;
                }
            }

            if (sender == pixelHeightUpDown)
            {
                if (getValueFromText > 0)
                {
                    if (Utility.GetUpDownValueFromText(this.pixelHeightUpDown, out val))
                    {
                        this.constrainer.NewPixelHeight = val;
                    }
                }
                else
                {
                    this.constrainer.NewPixelHeight = (double)this.pixelHeightUpDown.Value;
                }
            }

            if (sender == printWidthUpDown)
            {
                if (getValueFromText > 0)
                {
                    if (Utility.GetUpDownValueFromText(this.printWidthUpDown, out val))
                    {
                        this.constrainer.NewWidth = val;
                    }
                }
                else
                {
                    this.constrainer.NewWidth = (double)this.printWidthUpDown.Value;
                }
            }

            if (sender == printHeightUpDown)
            {
                if (getValueFromText > 0)
                {
                    if (Utility.GetUpDownValueFromText(this.printHeightUpDown, out val))
                    {
                        this.constrainer.NewHeight = val;
                    }
                }
                else
                {
                    this.constrainer.NewHeight = (double)this.printHeightUpDown.Value;
                }
            }

            if (sender == resolutionUpDown)
            {
                if (getValueFromText > 0)
                {
                    if (Utility.GetUpDownValueFromText(this.resolutionUpDown, out val))
                    {
                        if (val >= ResizeConstrainer.MinResolution)
                        {
                            this.constrainer.Resolution = val;
                        }
                    }
                }
                else
                {
                    if ((double)this.resolutionUpDown.Value >= ResizeConstrainer.MinResolution)
                    {
                        this.constrainer.Resolution = (double)this.resolutionUpDown.Value;
                    }
                }
            }

            UpdateSizeText();
            PopulateAsteriskLabels();
            TryToEnableOkButton();
        }

        private void OnUpDownKeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Tab)
            {
                double val;

                if (Utility.GetUpDownValueFromText((NumericUpDown)sender, out val))
                {
                    UpdateSizeText();

                    ++getValueFromText;
                    upDown_ValueChanged(sender, e);
                    --getValueFromText;
                }

                TryToEnableOkButton();
            }
        }

        private void OnUpDownEnter(object sender, System.EventArgs e)
        {
            NumericUpDown nud = (NumericUpDown)sender;
            nud.Select(0, nud.Text.Length);
        }

        private void OnUpDownLeave(object sender, System.EventArgs e)
        {
            ((NumericUpDown)sender).Value = ((NumericUpDown)sender).Value;
            TryToEnableOkButton();
        }

        private void OnRadioButtonCheckedChanged(object sender, System.EventArgs e)
        {
            if (this.absoluteRB.Checked)
            {
                this.pixelWidthUpDown.Enabled = true;
                this.pixelHeightUpDown.Enabled = true;
                this.printWidthUpDown.Enabled = true;
                this.printHeightUpDown.Enabled = true;
                this.constrainCheckBox.Enabled = true;
                this.unitsComboBox1.Enabled = true;
                this.unitsComboBox2.Enabled = true;
                this.resolutionUpDown.Enabled = true;
                this.percentUpDown.Enabled = false;
            }
            else if (this.percentRB.Checked)
            {
                this.pixelWidthUpDown.Enabled = false;
                this.pixelHeightUpDown.Enabled = false;
                this.printWidthUpDown.Enabled = false;
                this.printHeightUpDown.Enabled = false;
                this.constrainCheckBox.Enabled = false;
                this.unitsComboBox1.Enabled = false;
                this.unitsComboBox2.Enabled = false;
                this.resolutionUpDown.Enabled = false;
                this.percentUpDown.Enabled = true;
                this.percentUpDown.Select();
            }
        }

        private void PopulateAsteriskLabels()
        {
            ResampleMethod rm = this.resamplingAlgorithmComboBox.SelectedItem as ResampleMethod;

            if (rm == null)
            {
                return;
            }

            switch (rm.method)
            {
                default:
                    this.asteriskLabel.Visible = false;
                    this.asteriskTextLabel.Visible = false;
                    break;

                case ResamplingAlgorithm.SuperSampling:
                    if (this.ImageWidth < this.OriginalSize.Width &&
                        this.ImageHeight < this.OriginalSize.Height)
                    {
                        this.asteriskTextLabel.Text = PdnResources.GetString("ResizeDialog.AsteriskTextLabel.SuperSampling");
                    }
                    else
                    {
                        this.asteriskTextLabel.Text = PdnResources.GetString("ResizeDialog.AsteriskTextLabel.Bicubic");
                    }

                    if (this.resamplingAlgorithmComboBox.Visible)
                    {
                        this.asteriskLabel.Visible = true;
                        this.asteriskTextLabel.Visible = true;
                    }

                    break;
            }
        }

        private void OnResamplingAlgorithmComboBoxSelectedIndexChanged(object sender, System.EventArgs e)
        {
            PopulateAsteriskLabels();
        }

        private void OnUnitsComboBox1UnitsChanged(object sender, System.EventArgs e)
        {
            this.constrainer.Units = unitsComboBox1.Units;
            this.unitsLabel1.Text = unitsComboBox1.UnitsText;
            UpdateSizeText();
            TryToEnableOkButton();
        }

        private void OnUnitsComboBox2UnitsChanged(object sender, System.EventArgs e)
        {
            this.unitsComboBox1.Units = this.unitsComboBox2.Units;
            UpdateSizeText();
            TryToEnableOkButton();
        }

        private void OnConstrainerConstrainToAspectChanged(object sender, EventArgs e)
        {
            this.constrainCheckBox.Checked = this.constrainer.ConstrainToAspect;
            UpdateSizeText();
            TryToEnableOkButton();
        }

        private void OnConstrainerNewWidthChanged(object sender, EventArgs e)
        {
            ++ignoreUpDownValueChanged;

            double val = 0.0;
            bool result;

            result = Utility.GetUpDownValueFromText(this.pixelWidthUpDown, out val);
            if (!result || val != this.constrainer.NewPixelWidth)
            {
                SafeSetNudValue(this.pixelWidthUpDown, this.constrainer.NewPixelWidth);
            }

            result = Utility.GetUpDownValueFromText(this.printWidthUpDown, out val);
            if (!result || val != this.constrainer.NewWidth)
            {
                SafeSetNudValue(this.printWidthUpDown, this.constrainer.NewWidth);
            }

            --ignoreUpDownValueChanged;
            UpdateSizeText();
            TryToEnableOkButton();
        }

        private void constrainer_NewHeightChanged(object sender, EventArgs e)
        {
            ++ignoreUpDownValueChanged;

            double val;

            if (Utility.GetUpDownValueFromText(this.pixelHeightUpDown, out val))
            {
                if (val != this.constrainer.NewPixelHeight)
                {
                    SafeSetNudValue(this.pixelHeightUpDown, this.constrainer.NewPixelHeight);
                }
            }

            if (Utility.GetUpDownValueFromText(this.printHeightUpDown, out val))
            {
                if (val != this.constrainer.NewHeight)
                {
                    SafeSetNudValue(this.printHeightUpDown, this.constrainer.NewHeight);
                }
            }

            --ignoreUpDownValueChanged;
            UpdateSizeText();
            TryToEnableOkButton();
        }

        private void OnConstrainerResolutionChanged(object sender, EventArgs e)
        {
            ++ignoreUpDownValueChanged;

            double val;

            if (Utility.GetUpDownValueFromText(this.resolutionUpDown, out val))
            {
                if (val != this.constrainer.Resolution)
                {
                    SafeSetNudValue(this.resolutionUpDown, this.constrainer.Resolution);
                }
            }

            --ignoreUpDownValueChanged;
            UpdateSizeText();
            TryToEnableOkButton();
        }

        private void OnConstrainerUnitsChanged(object sender, EventArgs e)
        {
            this.unitsComboBox1.Units = constrainer.Units;
            this.unitsComboBox2.Units = constrainer.Units;
            UpdateSizeText();
            TryToEnableOkButton();
        }

        private void TryToEnableOkButton()
        {
            double pixelWidth;
            double pixelHeight;
            double printWidth;
            double printHeight;
            double resolution;
            double percent;

            bool b1 = Utility.GetUpDownValueFromText(this.pixelWidthUpDown, out pixelWidth);
            bool b2 = Utility.GetUpDownValueFromText(this.pixelHeightUpDown, out pixelHeight);
            bool b3 = Utility.GetUpDownValueFromText(this.printWidthUpDown, out printWidth);
            bool b4 = Utility.GetUpDownValueFromText(this.printHeightUpDown, out printHeight);
            bool b5 = Utility.GetUpDownValueFromText(this.resolutionUpDown, out resolution);
            bool b6 = Utility.GetUpDownValueFromText(this.percentUpDown, out percent);

            bool b7 = (pixelWidth >= 1.0 && pixelWidth <= 65535.0);
            bool b8 = (pixelHeight >= 1.0 && pixelHeight <= 65535.0);
            bool b9 = (printWidth > 0.0);
            bool b10 = (printHeight > 0.0);
            bool b11 = (resolution >= ResizeConstrainer.MinResolution && resolution < 2000000.0);
            bool b12 = (percent >= (double)this.percentUpDown.Minimum && percent <= (double)this.percentUpDown.Maximum);
            
            bool enable = b1 && b2 && b3 && b4 && b5 && b6 && b7 && b8 && b9 && b10 && b11 && b12;
            okButton.Enabled = enable;
        }

        private void SafeSetNudValue(NumericUpDown nud, double value)
        {
            try
            {
                decimal newValue = (decimal)value;

                if (newValue >= nud.Minimum && newValue <= nud.Maximum)
                {
                    nud.Value = newValue;
                }
            }

            catch (OverflowException ex)
            {
                Tracing.Ping("Exception: " + ex.ToString());
            }
        }
    }
}
