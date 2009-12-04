/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) Rick Brewster, Tom Jackson, and past contributors.            //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.SystemLayer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PaintDotNet.IndirectUI
{
    internal sealed class PanelControl
        : UserControl,
          IFirstSelection
    {
        private List<Control> controls = new List<Control>();

        protected override void OnLayout(LayoutEventArgs levent)
        {
            int vSpacing = UI.ScaleHeight(6);
            int insetWidth = Width;
            int y = 0;

            foreach (Control control in this.controls)
            {
                control.Location = new Point(0, y);
                control.Width = insetWidth;
                control.PerformLayout();

                y = control.Bottom + vSpacing;
            }

            ClientSize = new Size(ClientSize.Width, (y == 0) ? 0 : (y - vSpacing));

            base.OnLayout(levent);
        }

        public PanelControl(PanelControlInfo panelInfo)
        {
            SuspendLayout();

            DoubleBuffered = true;

            int tabIndex = 0;

            foreach (ControlInfo controlInfo in panelInfo.ChildControls)
            {
                Control childControl = controlInfo.CreateWinFormsControl();
                childControl.TabIndex = tabIndex;
                ++tabIndex;
                this.controls.Add(childControl);
            }

            Controls.AddRange(this.controls.ToArray());

            ResumeLayout(false);
        }

        bool IFirstSelection.FirstSelect()
        {
            foreach (Control control in this.controls)
            {
                IFirstSelection asIFS = control as IFirstSelection;

                if (asIFS != null)
                {
                    if (asIFS.FirstSelect())
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}