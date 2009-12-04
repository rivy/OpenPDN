/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet;
using System;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;

namespace PaintDotNet.Effects
{
    public abstract class EffectConfigDialog<TEffect, TToken>
        : EffectConfigDialog
          where TToken : EffectConfigToken
          where TEffect : Effect<TToken>
    {
        protected abstract TToken CreateInitialToken();

        protected override sealed void InitialInitToken()
        {
            this.theEffectToken = CreateInitialToken();
        }

        protected abstract void InitDialogFromToken(TToken effectTokenCopy);

        protected override sealed void InitDialogFromToken(EffectConfigToken effectTokenCopy)
        {
            InitDialogFromToken((TToken)effectTokenCopy);
        }

        protected abstract void LoadIntoTokenFromDialog(TToken writeValuesHere);

        protected override sealed void InitTokenFromDialog()
        {
            LoadIntoTokenFromDialog((TToken)this.theEffectToken);
        }

        public new TEffect Effect
        {
            get
            {
                return (TEffect)base.Effect;
            }

            set
            {
                base.Effect = value;
            }
        }

        public new TToken EffectToken
        {
            get
            {
                return (TToken)base.EffectToken;
            }

            set
            {
                base.EffectToken = value;
            }
        }

        internal EffectConfigDialog(object context)
            : base(context)
        {
        }

        public EffectConfigDialog()
        {
        }
    }
}