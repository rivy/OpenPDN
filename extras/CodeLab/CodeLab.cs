/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) Rick Brewster, Tom Jackson, and past contributors.            //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Setup/License.txt for full licensing and attribution details.       //
// 2                                                                           //
// 1                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.IO;
using Microsoft.CSharp;

namespace PaintDotNet.Effects
{
    public class CodeLab : Effect
    {
        public static Image StaticImage
        {
            get
            {
                Assembly ourAssembly = Assembly.GetCallingAssembly();
                Stream imageStream = ourAssembly.GetManifestResourceStream("PaintDotNet.Effects.Icons.CodeLab.png");
                Image image = Image.FromStream(imageStream);
                return image;
            }
        }

        public CodeLab()
            : base("Code Lab", StaticImage, true)
        {
        }

        public override EffectConfigDialog CreateConfigDialog()
        {
            CodeLabConfigDialog secd = new CodeLabConfigDialog();
            return secd;
        }

        public override void Render(EffectConfigToken parameters, RenderArgs dstArgs, RenderArgs srcArgs, Rectangle[] rois, int startIndex, int length)
        {
            CodeLabConfigToken sect = (CodeLabConfigToken)parameters;
            Effect userEffect = sect.UserScriptObject;

            if (userEffect != null)
            {
                userEffect.EnvironmentParameters = this.EnvironmentParameters;

                try
                {
                    userEffect.Render(null, dstArgs, srcArgs, rois, startIndex, length);
                }

                catch (Exception exc)
                {
                    sect.LastExceptions.Add(exc);
                    dstArgs.Surface.CopySurface(srcArgs.Surface);
                    sect.UserScriptObject = null;
                }
            }
        }
    }
}
