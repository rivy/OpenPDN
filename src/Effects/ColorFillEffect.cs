/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet;
using PaintDotNet.IndirectUI;
using PaintDotNet.PropertySystem;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PaintDotNet.Effects
{
    // This effect is for testing purposes only.
#if false
    public sealed class ColorFillEffect
        : InternalPropertyBasedEffect
    {
        public ColorFillEffect()
            : base("Color Fill", null, null, EffectFlags.Configurable)
        {
            if (PdnInfo.IsFinalBuild)
            {
                throw new InvalidOperationException("This effect should never make it in to a released build");
            }
        }

        public enum PropertyNames
        {
            AngleChooser,
            CheckBox,
            DoubleSlider,
            DoubleVectorPanAndSlider,
            DoubleVectorSlider,
            Int32ColorWheel,
            Int32IncrementButton,
            Int32Slider,
            StaticListDropDown,
            StaticListRadioButton,
            StringText,
        }

        private ColorBgra color;

        protected override PropertyCollection OnCreatePropertyCollection()
        {
            List<Property> props = new List<Property>();

            props.Add(new DoubleProperty(PropertyNames.AngleChooser, 0, -180, +180));
            props.Add(new BooleanProperty(PropertyNames.CheckBox, true));
            props.Add(new DoubleProperty(PropertyNames.DoubleSlider, 0, 0, 100));
            props.Add(new DoubleVectorProperty(PropertyNames.DoubleVectorPanAndSlider, Pair.Create(0.0, 0.0), Pair.Create(-1.0, -1.0), Pair.Create(+1.0, +1.0)));
            props.Add(new DoubleVectorProperty(PropertyNames.DoubleVectorSlider, Pair.Create(0.0, 0.0), Pair.Create(-1.0, -1.0), Pair.Create(+1.0, +1.0)));
            props.Add(new Int32Property(PropertyNames.Int32ColorWheel, 0, 0, 0xffffff));
            props.Add(new Int32Property(PropertyNames.Int32IncrementButton, 0, 0, 255));
            props.Add(new Int32Property(PropertyNames.Int32Slider, 0, 0, 100));
            props.Add(StaticListChoiceProperty.CreateForEnum<System.Drawing.GraphicsUnit>(PropertyNames.StaticListDropDown, GraphicsUnit.Millimeter, false));
            props.Add(StaticListChoiceProperty.CreateForEnum<System.Drawing.GraphicsUnit>(PropertyNames.StaticListRadioButton, GraphicsUnit.Document, false));
            props.Add(new StringProperty(PropertyNames.StringText, "hello", 100));

            return new PropertyCollection(props);
        }

        protected override ControlInfo OnCreateConfigUI(PropertyCollection props)
        {
            ControlInfo configUI = CreateDefaultConfigUI(props);

            configUI.SetPropertyControlType(PropertyNames.AngleChooser, PropertyControlType.AngleChooser);
            configUI.SetPropertyControlType(PropertyNames.CheckBox, PropertyControlType.CheckBox);
            configUI.SetPropertyControlType(PropertyNames.DoubleSlider, PropertyControlType.Slider);
            configUI.SetPropertyControlType(PropertyNames.DoubleVectorPanAndSlider, PropertyControlType.PanAndSlider);
            configUI.SetPropertyControlType(PropertyNames.DoubleVectorSlider, PropertyControlType.Slider);
            configUI.SetPropertyControlType(PropertyNames.Int32ColorWheel, PropertyControlType.ColorWheel);
            configUI.SetPropertyControlType(PropertyNames.Int32IncrementButton, PropertyControlType.IncrementButton);
            configUI.SetPropertyControlType(PropertyNames.Int32Slider, PropertyControlType.Slider);
            configUI.SetPropertyControlType(PropertyNames.StaticListDropDown, PropertyControlType.DropDown);
            configUI.SetPropertyControlType(PropertyNames.StaticListRadioButton, PropertyControlType.RadioButton);
            configUI.SetPropertyControlType(PropertyNames.StringText, PropertyControlType.TextBox);

            foreach (object propertyName in Enum.GetValues(typeof(PropertyNames)))
            {
                configUI.SetPropertyControlValue(propertyName, ControlInfoPropertyNames.DisplayName, string.Empty);
            }

            return configUI;
        }

        protected override void OnCustomizeConfigUIWindowProperties(PropertyCollection props)
        {
            base.OnCustomizeConfigUIWindowProperties(props);
            //props[ControlInfoPropertyNames.WindowIsSizable].Value = true;
        }

        protected override void OnSetRenderInfo(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            int colorValue = newToken.GetProperty<Int32Property>(PropertyNames.Int32ColorWheel).Value;
            this.color = ColorBgra.FromOpaqueInt32(colorValue);
            base.OnSetRenderInfo(newToken, dstArgs, srcArgs);
        }
        
        protected override void OnRender(Rectangle[] renderRects, int startIndex, int length)
        {
            foreach (Rectangle rect in renderRects)
            {
                for (int y = rect.Top; y < rect.Bottom; ++y)
                {
                    for (int x = rect.Left; x < rect.Right; ++x)
                    {
                        DstArgs.Surface[x, y] = this.color;
                    }
                }
            }
        }
    }
#endif
}