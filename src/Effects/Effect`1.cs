/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;
using System.Windows.Forms;

namespace PaintDotNet.Effects
{
    public abstract class Effect<TToken>
        : Effect
          where TToken : EffectConfigToken
    {
        private TToken token;
        private RenderArgs dstArgs;
        private RenderArgs srcArgs;

        protected TToken Token
        {
            get
            {
                return this.token;
            }
        }

        protected RenderArgs DstArgs
        {
            get
            {
                return this.dstArgs;
            }
        }

        protected RenderArgs SrcArgs
        {
            get
            {
                return this.srcArgs;
            }
        }

        protected abstract void OnRender(Rectangle[] renderRects, int startIndex, int length);

        public void Render(Rectangle[] renderRects, int startIndex, int length)
        {
            if (!this.SetRenderInfoCalled && !this.RenderInfoAvailable)
            {
                throw new InvalidOperationException("SetRenderInfo() was not called, nor was render info available implicitely");
            }

            OnRender(renderRects, startIndex, length);
        }

        protected virtual void OnSetRenderInfo(TToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
        }

        protected override sealed void OnSetRenderInfo(EffectConfigToken parameters, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            this.token = (TToken)parameters;
            this.dstArgs = dstArgs;
            this.srcArgs = srcArgs;

            this.OnSetRenderInfo((TToken)parameters, dstArgs, srcArgs);

            base.OnSetRenderInfo(parameters, dstArgs, srcArgs);
        }

        private bool renderInfoAvailable = false;
        internal protected bool RenderInfoAvailable
        {
            get
            {
                return this.renderInfoAvailable;
            }
        }

        public override sealed void Render(EffectConfigToken parameters, RenderArgs dstArgs, RenderArgs srcArgs, Rectangle[] rois, int startIndex, int length)
        {
            if (!this.SetRenderInfoCalled)
            {
                lock (this)
                {
                    this.token = (TToken)parameters;
                    this.dstArgs = dstArgs;
                    this.srcArgs = srcArgs;

                    this.renderInfoAvailable = true;

                    OnSetRenderInfo(this.token, this.dstArgs, this.srcArgs);
                }
            }

            Render(rois, startIndex, length);
        }

        public Effect(string name, Image image)
            : base(name, image)
        {
        }

        public Effect(string name, Image image, string subMenuName)
            : base(name, image, subMenuName)
        {
        }

        public Effect(string name, Image image, string subMenuName, EffectFlags flags)
            : base(name, image, subMenuName, flags)
        {
        }
    }
}