/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Windows.Forms;

namespace PaintDotNet.SystemLayer
{
    /// <summary>
    /// This class adds on to the functionality provided in System.Windows.Forms.MenuStrip.
    /// </summary>
    /// <remarks>
    /// The first aggravating thing I found out about the new toolstrips is that they do not "click through."
    /// If the form that is hosting a toolstrip is not active and you click on a button in the toolstrip, it 
    /// sets focus to the form but does NOT click the button. This makes sense in many situations, but 
    /// definitely not for Paint.NET.
    /// </remarks>
    public class MenuStripEx
        : MenuStrip
    {
        private bool clickThrough = true;
        private static int openCount = 0;

        public MenuStripEx()
        {
            this.ImageScalingSize = new System.Drawing.Size(UI.ScaleWidth(16), UI.ScaleHeight(16));
        }

        /// <summary>
        /// Gets or sets whether the ToolStripEx honors item clicks when its containing form does
        /// not have input focus.
        /// </summary>
        /// <remarks>
        /// Default value is true, which is the opposite of the behavior provided by the base
        /// ToolStrip class.
        /// </remarks>
        public bool ClickThrough
        {
            get
            {
                return this.clickThrough;
            }

            set
            {
                this.clickThrough = value;
            }
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (this.clickThrough)
            {
                UI.ClickThroughWndProc(ref m);
            }
        }

        /// <summary>
        /// Gets a value indicating whether any menu is currently open.
        /// </summary>
        /// <remarks>
        /// To be precise, this will return true if any menu has raised its MenuActivate event
        /// but has yet to raise its MenuDeactivate event.</remarks>
        public static bool IsAnyMenuActive
        {
            get
            {
                return openCount > 0;
            }
        }

        public static void PushMenuActivate()
        {
            ++openCount;
        }

        public static void PopMenuActivate()
        {
            --openCount;
        }

        protected override void OnMenuActivate(EventArgs e)
        {
            ++openCount;
            base.OnMenuActivate(e);
        }

        protected override void OnMenuDeactivate(EventArgs e)
        {
            --openCount;
            base.OnMenuDeactivate(e);
        }
    }
}
