/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;

namespace PaintDotNet.Effects
{
    public class LevelsEffectConfigToken 
        : EffectConfigToken
    {
        private UnaryPixelOps.Level levels = null;

        public UnaryPixelOps.Level Levels
        {
            get 
            {
                return levels;
            }

            set 
            {
                levels = value;
            }
        }

        public LevelsEffectConfigToken()
        {
            levels = new UnaryPixelOps.Level();
        }

        public override object Clone()
        {
            LevelsEffectConfigToken cpy = new LevelsEffectConfigToken();
            cpy.levels = (UnaryPixelOps.Level)this.levels.Clone();
            return cpy;
        }
    }
}
