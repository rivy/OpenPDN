/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace PaintDotNet
{
    public class PanelEx : 
        PaintDotNet.SystemLayer.ScrollPanel
    {
        private bool hideHScroll = false;

        public bool HideHScroll
        {
            get
            {
                return this.hideHScroll;
            }

            set
            {
                this.hideHScroll = value;
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            if (this.hideHScroll)
            {
                SystemLayer.UI.SuspendControlPainting(this);
            }

            base.OnSizeChanged(e);

            if (this.hideHScroll)
            {
                SystemLayer.UI.HideHorizontalScrollBar(this);
                SystemLayer.UI.ResumeControlPainting(this);
                Invalidate(true);
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            //base.OnMouseWheel(e);
        }
    }
}
