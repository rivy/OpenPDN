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
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace PaintDotNet.SystemLayer
{
    // TODO: remove? <-- depends on rewriting LayerControl and DocumentView so that it doesn't use a Panel, and that is post-3.0 stuff
    /// <summary>
    /// This is the same as System.Windows.Forms.Panel except for three things:
    /// 1. It exposes a Scroll event.
    /// 2. It allows you to disable SetFocus.
    /// 3. It has a much simplified interface for AutoScrollPosition, exposed via the ScrollPosition property.
    /// </summary>
    public class ScrollPanel
        : Panel
    {
        private bool ignoreSetFocus = false;

        /// <summary>
        /// Gets or sets whether the control ignores WM_SETFOCUS.
        /// </summary>
        public bool IgnoreSetFocus
        {
            get
            {
                return ignoreSetFocus;
            }

            set
            {
                ignoreSetFocus = value;
            }
        }

        /// <summary>
        /// Gets or sets the scrollbar position.
        /// </summary>
        [Browsable(false)]
        public Point ScrollPosition
        {
            get 
            { 
                return new Point(-AutoScrollPosition.X, -AutoScrollPosition.Y); 
            }

            set 
            { 
                AutoScrollPosition = value;
            }
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case NativeConstants.WM_SETFOCUS:
                    if (IgnoreSetFocus)
                    {
                        return;
                    }
                    else
                    {
                        goto default;
                    }

                default:
                    base.WndProc(ref m);
                    break;
            }
        }        
    }
}
