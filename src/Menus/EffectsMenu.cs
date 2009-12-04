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

namespace PaintDotNet.Menus
{
    internal sealed class EffectsMenu
        : EffectMenuBase
    {
        public EffectsMenu()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Name = "Menu.Effects";
            this.Text = PdnResources.GetString("Menu.Effects.Text");
        }

        protected override bool EnableEffectShortcuts
        {
            get
            {
                return false;
            }
        }

        protected override bool EnableRepeatEffectMenuItem
        {
            get
            {
                return true;
            }
        }

        protected override bool FilterEffects(Effect effect)
        {
            return (effect.Category == EffectCategory.Effect);
        }
    }
}
