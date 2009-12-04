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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace PaintDotNet
{
    // TODO: rename AppEnvironment -> ToolEnvironment or ToolConfigItems or something
    /// <summary>
    /// Manages document-independent workspace configuration details, and provides
    /// notification events for every item that can change.
    /// </summary>
    [Serializable]
    internal sealed class AppEnvironment
        : IDisposable,
          ICloneable,
          IDeserializationCallback
    {
        private TextAlignment textAlignment;
        private GradientInfo gradientInfo;
        private FontSmoothing fontSmoothing;
        private FontInfo fontInfo;
        private PenInfo penInfo;
        private BrushInfo brushInfo;
        private ColorBgra primaryColor;
        private ColorBgra secondaryColor;
        private bool alphaBlending;
        private ShapeDrawType shapeDrawType;
        private bool antiAliasing;
        private ColorPickerClickBehavior colorPickerClickBehavior;
        private ResamplingAlgorithm resamplingAlgorithm;
        private float tolerance;

        // Added in v3.20. If not found in the serialized data, must default to CombineMode.Replace.
        // Conveniently for us, this is equal to (CombineMode)0. Otherwise we would need to have a
        // boolean flag as well in order to detect if the data needed to be reset to default.
        [OptionalField]
        private CombineMode selectionCombineMode;

        [OptionalField]
        private FloodMode floodMode;

        [OptionalField]
        private SelectionDrawModeInfo selectionDrawModeInfo;

        public static AppEnvironment GetDefaultAppEnvironment()
        {
            AppEnvironment appEnvironment;

            try
            {
                string defaultAppEnvBase64 = Settings.CurrentUser.GetString(SettingNames.DefaultAppEnvironment, null);

                if (defaultAppEnvBase64 == null)
                {
                    appEnvironment = null;
                }
                else
                {
                    byte[] defaultAppEnvBytes = System.Convert.FromBase64String(defaultAppEnvBase64);
                    BinaryFormatter formatter = new BinaryFormatter();

                    using (MemoryStream stream = new MemoryStream(defaultAppEnvBytes, false))
                    {
                        object defaultAppEnvObject = formatter.Deserialize(stream);
                        appEnvironment = (AppEnvironment)defaultAppEnvObject;
                    }
                }
            }

            catch (Exception)
            {
                appEnvironment = null;
            }

            if (appEnvironment == null)
            {
                appEnvironment = new AppEnvironment();
                appEnvironment.SetToDefaults();
            }

            return appEnvironment;
        }

        public void SaveAsDefaultAppEnvironment()
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            formatter.Serialize(stream, this);
            byte[] bytes = stream.GetBuffer();
            string base64 = Convert.ToBase64String(bytes);
            Settings.CurrentUser.SetString(SettingNames.DefaultAppEnvironment, base64);
        }

        public void LoadFrom(AppEnvironment appEnvironment)
        {
            this.textAlignment = appEnvironment.textAlignment;
            this.gradientInfo = appEnvironment.gradientInfo.Clone();
            this.fontSmoothing = appEnvironment.fontSmoothing;
            this.fontInfo = appEnvironment.fontInfo.Clone();
            this.penInfo = appEnvironment.penInfo.Clone();
            this.brushInfo = appEnvironment.brushInfo.Clone();
            this.primaryColor = appEnvironment.primaryColor;
            this.secondaryColor = appEnvironment.secondaryColor;
            this.alphaBlending = appEnvironment.alphaBlending;
            this.shapeDrawType = appEnvironment.shapeDrawType;
            this.antiAliasing = appEnvironment.antiAliasing;
            this.colorPickerClickBehavior = appEnvironment.colorPickerClickBehavior;
            this.resamplingAlgorithm = appEnvironment.resamplingAlgorithm;
            this.tolerance = appEnvironment.tolerance;
            this.selectionCombineMode = appEnvironment.selectionCombineMode;
            this.floodMode = appEnvironment.floodMode;
            this.selectionDrawModeInfo = appEnvironment.selectionDrawModeInfo.Clone();
            PerformAllChanged();
        }

        #region Font stuff
        public TextAlignment TextAlignment
        {
            get
            {
                return this.textAlignment;
            }

            set
            {
                if (value != this.textAlignment)
                {
                    OnTextAlignmentChanging();
                    this.textAlignment = value;
                    OnTextAlignmentChanged();
                }
            }
        }

        [field: NonSerialized]
        public event EventHandler TextAlignmentChanging;

        private void OnTextAlignmentChanging()
        {
            if (TextAlignmentChanging != null)
            {
                TextAlignmentChanging(this, EventArgs.Empty);
            }
        }

        [field: NonSerialized]
        public event EventHandler TextAlignmentChanged;

        private void OnTextAlignmentChanged()
        {
            if (TextAlignmentChanged != null)
            {
                TextAlignmentChanged(this, EventArgs.Empty);
            }
        }

        public FontInfo FontInfo
        {
            get
            {
                return this.fontInfo;
            }

            set
            {
                if (this.fontInfo != value)
                {
                    OnFontInfoChanging();
                    this.fontInfo = value;
                    OnFontInfoChanged();
                }
            }
        }

        [field: NonSerialized]
        public event EventHandler FontInfoChanging;

        private void OnFontInfoChanging()
        {
            if (FontInfoChanging != null)
            {
                FontInfoChanging(this, EventArgs.Empty);
            }
        }

        [field: NonSerialized]
        public event EventHandler FontInfoChanged;

        private void OnFontInfoChanged()
        {
            if (FontInfoChanged != null)
            {
                FontInfoChanged(this, EventArgs.Empty);
            }
        }

        [field: NonSerialized]
        public event EventHandler FontSmoothingChanging;

        private void OnFontSmoothingChanging()
        {
            if (FontSmoothingChanging != null)
            {
                FontSmoothingChanging(this, EventArgs.Empty);
            }
        }

        [field: NonSerialized]
        public event EventHandler FontSmoothingChanged;

        private void OnFontSmoothingChanged()
        {
            if (FontSmoothingChanged != null)
            {
                FontSmoothingChanged(this, EventArgs.Empty);
            }
        }

        public FontSmoothing FontSmoothing
        {
            get
            {
                return this.fontSmoothing;
            }

            set
            {
                if (this.fontSmoothing != value)
                {
                    OnFontSmoothingChanging();
                    this.fontSmoothing = value;
                    OnFontSmoothingChanged();
                }
            }
        }
        #endregion

        #region GradientInfo
        [field: NonSerialized]
        public event EventHandler GradientInfoChanging;

        private void OnGradientInfoChanging()
        {
            if (GradientInfoChanging != null)
            {
                GradientInfoChanging(this, EventArgs.Empty);
            }
        }

        [field: NonSerialized]
        public event EventHandler GradientInfoChanged;

        private void OnGradientInfoChanged()
        {
            if (GradientInfoChanged != null)
            {
                GradientInfoChanged(this, EventArgs.Empty);
            }
        }

        public GradientInfo GradientInfo
        {
            get
            {
                return this.gradientInfo;
            }

            set
            {
                OnGradientInfoChanging();
                this.gradientInfo = value;
                OnGradientInfoChanged();
            }
        }
        #endregion

        #region PenInfo
        public Pen CreatePen(bool swapColors)
        {
            if (!swapColors)
            {
                return PenInfo.CreatePen(BrushInfo, PrimaryColor.ToColor(), SecondaryColor.ToColor());
            }
            else
            {
                return PenInfo.CreatePen(BrushInfo, SecondaryColor.ToColor(), PrimaryColor.ToColor());
            }
        }

        public PenInfo PenInfo
        {
            get
            {
                return this.penInfo.Clone();
            }

            set
            {
                if (this.penInfo != value)
                {
                    OnPenInfoChanging();
                    this.penInfo = value.Clone();
                    OnPenInfoChanged();
                }
            }
        }

        [field: NonSerialized]
        public event EventHandler PenInfoChanging;

        private void OnPenInfoChanging()
        {
            if (PenInfoChanging != null)
            {
                PenInfoChanging(this, EventArgs.Empty);
            }
        }

        [field: NonSerialized]
        public event EventHandler PenInfoChanged;

        private void OnPenInfoChanged()
        {
            if (PenInfoChanged != null)
            {
                PenInfoChanged(this, EventArgs.Empty);
            }
        }
        #endregion

        #region BrushInfo
        public Brush CreateBrush(bool swapColors)
        {
            if (!swapColors)
            {
                return BrushInfo.CreateBrush(PrimaryColor.ToColor(), SecondaryColor.ToColor());
            }
            else
            {
                return BrushInfo.CreateBrush(SecondaryColor.ToColor(), PrimaryColor.ToColor());
            }
        }

        public BrushInfo BrushInfo
        {
            get
            {
                return this.brushInfo.Clone();
            }

            set
            {
                OnBrushInfoChanging();
                this.brushInfo = value.Clone();
                OnBrushInfoChanged();
            }
        }

        [field: NonSerialized]
        public event EventHandler BrushInfoChanging;

        private void OnBrushInfoChanging()
        {
            if (BrushInfoChanging != null)
            {
                BrushInfoChanging(this, EventArgs.Empty);
            }
        }

        [field: NonSerialized]
        public event EventHandler BrushInfoChanged;

        private void OnBrushInfoChanged()
        {
            if (BrushInfoChanged != null)
            {
                BrushInfoChanged(this, EventArgs.Empty);
            }
        }
        #endregion

        #region PrimaryColor
        public ColorBgra PrimaryColor
        {
            get
            {
                return this.primaryColor;
            }

            set
            {
                if (this.primaryColor != value)
                {
                    OnPrimaryColorChanging();
                    this.primaryColor = value;
                    OnPrimaryColorChanged();
                }
            }
        }

        [field: NonSerialized]
        public event EventHandler PrimaryColorChanging;

        private void OnPrimaryColorChanging()
        {
            if (PrimaryColorChanging != null)
            {
                PrimaryColorChanging(this, EventArgs.Empty);
            }
        }

        [field: NonSerialized]
        public event EventHandler PrimaryColorChanged;

        private void OnPrimaryColorChanged()
        {
            if (PrimaryColorChanged != null)
            {
                PrimaryColorChanged(this, EventArgs.Empty);
            }
        }
        #endregion

        #region SecondaryColor
        public ColorBgra SecondaryColor
        {
            get
            {
                return this.secondaryColor;
            }

            set
            {
                if (this.secondaryColor != value)
                {
                    OnBackColorChanging();
                    this.secondaryColor = value;
                    OnSecondaryColorChanged();
                }
            }
        }

        [field: NonSerialized]
        public event EventHandler SecondaryColorChanging;

        private void OnBackColorChanging()
        {
            if (SecondaryColorChanging != null)
            {
                SecondaryColorChanging(this, EventArgs.Empty);
            }
        }

        [field: NonSerialized]
        public event EventHandler SecondaryColorChanged;

        private void OnSecondaryColorChanged()
        {
            if (SecondaryColorChanged != null)
            {
                SecondaryColorChanged(this, EventArgs.Empty);
            }
        }
        #endregion

        #region AlphaBlending
        public CompositingMode GetCompositingMode()
        {
            return this.alphaBlending ? CompositingMode.SourceOver : CompositingMode.SourceCopy;
        }
        
        public bool AlphaBlending
        {
            get
            {
                return this.alphaBlending;
            }

            set
            {
                if (value != this.alphaBlending)
                {
                    OnAlphaBlendingChanging();
                    this.alphaBlending = value;
                    OnAlphaBlendingChanged();
                }
            }
        }

        [field: NonSerialized]
        public event EventHandler AlphaBlendingChanging;

        private void OnAlphaBlendingChanging()
        {
            if (AlphaBlendingChanging != null)
            {
                AlphaBlendingChanging(this, EventArgs.Empty);
            }
        }

        [field: NonSerialized]
        public event EventHandler AlphaBlendingChanged;

        private void OnAlphaBlendingChanged()
        {
            if (AlphaBlendingChanged != null)
            {
                AlphaBlendingChanged(this, EventArgs.Empty);
            }
        }
        #endregion

        #region ShapeDrawType
        public ShapeDrawType ShapeDrawType
        {
            get
            {
                return this.shapeDrawType;
            }

            set
            {
                if (this.shapeDrawType != value)
                {
                    OnShapeDrawTypeChanging();
                    this.shapeDrawType = value;
                    OnShapeDrawTypeChanged();
                }
            }
        }

        [field: NonSerialized]
        public event EventHandler ShapeDrawTypeChanging;

        private void OnShapeDrawTypeChanging()
        {
            if (ShapeDrawTypeChanging != null)
            {
                ShapeDrawTypeChanging(this, EventArgs.Empty);
            }
        }

        [field: NonSerialized]
        public event EventHandler ShapeDrawTypeChanged;

        private void OnShapeDrawTypeChanged()
        {
            if (ShapeDrawTypeChanged != null)
            {
                ShapeDrawTypeChanged(this, EventArgs.Empty);
            }
        }
        #endregion

        #region AntiAliasing
        [field: NonSerialized]
        public event EventHandler AntiAliasingChanging;

        private void OnAntiAliasingChanging()
        {
            if (AntiAliasingChanging != null)
            {
                AntiAliasingChanging(this, EventArgs.Empty);
            }
        }

        [field: NonSerialized]
        public event EventHandler AntiAliasingChanged;

        private void OnAntiAliasingChanged()
        {
            if (AntiAliasingChanged != null)
            {
                AntiAliasingChanged(this, EventArgs.Empty);
            }
        }

        public bool AntiAliasing
        {
            get
            {
                return this.antiAliasing;
            }

            set
            {
                if (this.antiAliasing != value)
                {
                    OnAntiAliasingChanging();
                    this.antiAliasing = value;
                    OnAntiAliasingChanged();
                }
            }
        }
        #endregion

        #region Color Picker behavior
        [field: NonSerialized]
        public event EventHandler ColorPickerClickBehaviorChanging;

        private void OnColorPickerClickBehaviorChanging()
        {
            if (ColorPickerClickBehaviorChanging != null)
            {
                ColorPickerClickBehaviorChanging(this, EventArgs.Empty);
            }
        }

        [field: NonSerialized]
        public event EventHandler ColorPickerClickBehaviorChanged;

        private void OnColorPickerClickBehaviorChanged()
        {
            if (ColorPickerClickBehaviorChanged != null)
            {
                ColorPickerClickBehaviorChanged(this, EventArgs.Empty);
            }
        }

        public ColorPickerClickBehavior ColorPickerClickBehavior
        {
            get
            {
                return this.colorPickerClickBehavior;
            }

            set
            {
                if (this.colorPickerClickBehavior != value)
                {
                    OnColorPickerClickBehaviorChanging();
                    this.colorPickerClickBehavior = value;
                    OnColorPickerClickBehaviorChanged();
                }
            }
        }
        #endregion

        #region ResamplingAlgorithm
        [field: NonSerialized]
        public event EventHandler ResamplingAlgorithmChanging;

        private void OnResamplingAlgorithmChanging()
        {
            if (ResamplingAlgorithmChanging != null)
            {
                ResamplingAlgorithmChanging(this, EventArgs.Empty);
            }
        }

        [field: NonSerialized]
        public event EventHandler ResamplingAlgorithmChanged;

        private void OnResamplingAlgorithmChanged()
        {
            if (ResamplingAlgorithmChanged != null)
            {
                ResamplingAlgorithmChanged(this, EventArgs.Empty);
            }
        }

        public ResamplingAlgorithm ResamplingAlgorithm
        {
            get
            {
                return this.resamplingAlgorithm;
            }

            set
            {
                if (value != this.resamplingAlgorithm)
                {
                    OnResamplingAlgorithmChanging();
                    this.resamplingAlgorithm = value;
                    OnResamplingAlgorithmChanged();
                }
            }
        }
        #endregion

        #region Tolerance
        [field: NonSerialized]
        public event EventHandler ToleranceChanged;

        private void OnToleranceChanged()
        {
            if (ToleranceChanged != null)
            {
                ToleranceChanged(this, EventArgs.Empty);
            }
        }

        [field: NonSerialized]
        public event EventHandler ToleranceChanging;

        private void OnToleranceChanging()
        {
            if (ToleranceChanging != null)
            {
                ToleranceChanging(this, EventArgs.Empty);
            }
        }

        public float Tolerance
        {
            get
            {
                return tolerance;
            }

            set
            {
                if (tolerance != value)
                {
                    tolerance = value;
                    OnToleranceChanged();
                }
            }
        }
        #endregion

        #region SelectionCombineMode
        [field: NonSerialized]
        public event EventHandler SelectionCombineModeChanged;

        private void OnSelectionCombineModeChanged()
        {
            if (SelectionCombineModeChanged != null)
            {
                SelectionCombineModeChanged(this, EventArgs.Empty);
            }
        }

        public CombineMode SelectionCombineMode
        {
            get
            {
                return this.selectionCombineMode;
            }

            set
            {
                if (this.selectionCombineMode != value)
                {
                    this.selectionCombineMode = value;
                    OnSelectionCombineModeChanged();
                }
            }
        }
        #endregion

        #region FloodMode
        [field: NonSerialized]
        public event EventHandler FloodModeChanged;

        private void OnFloodModeChanged()
        {
            if (FloodModeChanged != null)
            {
                FloodModeChanged(this, EventArgs.Empty);
            }
        }

        public FloodMode FloodMode
        {
            get
            {
                return this.floodMode;
            }

            set
            {
                if (this.floodMode != value)
                {
                    this.floodMode = value;
                    OnFloodModeChanged();
                }
            }
        }
        #endregion

        #region SelectionDrawModeInfo
        [field: NonSerialized]
        public event EventHandler SelectionDrawModeInfoChanged;

        private void OnSelectionDrawModeInfoChanged()
        {
            if (SelectionDrawModeInfoChanged != null)
            {
                SelectionDrawModeInfoChanged(this, EventArgs.Empty);
            }
        }

        public SelectionDrawModeInfo SelectionDrawModeInfo
        {
            get
            {
                return this.selectionDrawModeInfo.Clone();
            }

            set
            {
                if (!this.selectionDrawModeInfo.Equals(value))
                {
                    this.selectionDrawModeInfo = value.Clone();
                    OnSelectionDrawModeInfoChanged();
                }
            }
        }
        #endregion

        public void PerformAllChanged()
        {
            OnFontInfoChanged();
            OnFontSmoothingChanged();
            OnPenInfoChanged();
            OnBrushInfoChanged();
            OnGradientInfoChanged();
            OnSecondaryColorChanged();
            OnPrimaryColorChanged();
            OnAlphaBlendingChanged();
            OnShapeDrawTypeChanged();
            OnAntiAliasingChanged();
            OnTextAlignmentChanged();
            OnToleranceChanged();
            OnColorPickerClickBehaviorChanged();
            OnResamplingAlgorithmChanging();
            OnSelectionCombineModeChanged();
            OnFloodModeChanged();
            OnSelectionDrawModeInfoChanged();
        }

        public void SetToDefaults()
        {
            this.antiAliasing = true;
            this.fontSmoothing = FontSmoothing.Smooth;
            this.primaryColor = ColorBgra.FromBgra(0, 0, 0, 255);
            this.secondaryColor = ColorBgra.FromBgra(255, 255, 255, 255);
            this.gradientInfo = new GradientInfo(GradientType.LinearClamped, false);
            this.penInfo = new PenInfo(PenInfo.DefaultDashStyle, 2.0f, PenInfo.DefaultLineCap, PenInfo.DefaultLineCap, PenInfo.DefaultCapScale);
            this.brushInfo = new BrushInfo(BrushType.Solid, HatchStyle.BackwardDiagonal);

            try
            {
                this.fontInfo = new FontInfo(new FontFamily("Arial"), 12, FontStyle.Regular);
            }

            catch (Exception)
            {
                this.fontInfo = new FontInfo(new FontFamily(GenericFontFamilies.SansSerif), 12, FontStyle.Regular);
            }

            this.textAlignment = TextAlignment.Left;
            this.shapeDrawType = ShapeDrawType.Outline;
            this.alphaBlending = true;
            this.tolerance = 0.5f;

            this.colorPickerClickBehavior = ColorPickerClickBehavior.NoToolSwitch;
            this.resamplingAlgorithm = ResamplingAlgorithm.Bilinear;
            this.selectionCombineMode = CombineMode.Replace;
            this.floodMode = FloodMode.Local;
            this.selectionDrawModeInfo = SelectionDrawModeInfo.CreateDefault();
        }

        public AppEnvironment()
        {
            SetToDefaults();
        }

        ~AppEnvironment()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        public AppEnvironment Clone()
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            formatter.Serialize(stream, this);
            stream.Seek(0, SeekOrigin.Begin);
            object cloned = formatter.Deserialize(stream);
            stream.Dispose();
            stream = null;
            return (AppEnvironment)cloned;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        void IDeserializationCallback.OnDeserialization(object sender)
        {
            if (this.selectionDrawModeInfo == null)
            {
                this.selectionDrawModeInfo = SelectionDrawModeInfo.CreateDefault();
            }
        }
    }
}