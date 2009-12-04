/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.Effects;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace PaintDotNet.Menus
{
    internal sealed class AdjustmentsMenu
        : EffectMenuBase
    {
        public AdjustmentsMenu()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Name = "Menu.Layers.Adjustments";
            this.Text = PdnResources.GetString("Menu.Layers.Adjustments.Text");
        }

        protected override bool EnableEffectShortcuts
        {
            get
            {
                return true;
            }
        }

        protected override bool EnableRepeatEffectMenuItem
        {
            get
            {
                return false;
            }
        }

        protected override Keys GetEffectShortcutKeys(Effect effect)
        {
            Keys keys;

            if (effect is DesaturateEffect)
            {
                keys = Keys.Control | Keys.Shift | Keys.G;
            }
            else if (effect is AutoLevelEffect)
            {
                keys = Keys.Control | Keys.Shift | Keys.L;
            }
            else if (effect is InvertColorsEffect)
            {
                keys = Keys.Control | Keys.Shift | Keys.I;
            }
            else if (effect is HueAndSaturationAdjustment)
            {
                keys = Keys.Control | Keys.Shift | Keys.U;
            }
            else if (effect is SepiaEffect)
            {
                keys = Keys.Control | Keys.Shift | Keys.E;
            }
            else if (effect is BrightnessAndContrastAdjustment)
            {
                keys = Keys.Control | Keys.Shift | Keys.C;
            }
            else if (effect is LevelsEffect)
            {
                keys = Keys.Control | Keys.L;
            }
            else if (effect is CurvesEffect)
            {
                keys = Keys.Control | Keys.Shift | Keys.M;
            }
            else if (effect is PosterizeAdjustment)
            {
                keys = Keys.Control | Keys.Shift | Keys.P;
            }
            else
            {
                keys = Keys.None;
            }

            return keys;
        }

        protected override bool FilterEffects(Effect effect)
        {
            return (effect.Category == EffectCategory.Adjustment);
        }
    }
}
