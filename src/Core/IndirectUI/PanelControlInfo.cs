/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) Rick Brewster, Tom Jackson, and past contributors.            //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace PaintDotNet.IndirectUI
{
    public sealed class PanelControlInfo
        : ControlInfo
    {
        public T AddChildControl<T>(T controlInfo)
            where T : ControlInfo
        {
            GetChildControlsCore().Add(controlInfo);
            return controlInfo;
        }

        public void RemoveChildControl(ControlInfo controlInfo)
        {
            GetChildControlsCore().Remove(controlInfo);
        }

        public PanelControlInfo()
        {
        }

        private PanelControlInfo(PanelControlInfo cloneMe)
            : base(cloneMe)
        {
        }

        internal override Control CreateWinFormsControl()
        {
            return new PanelControl(this);
        }

        public override ControlInfo Clone()
        {
            return new PanelControlInfo(this);
        }
    }
}
