/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) Rick Brewster, Tom Jackson, and past contributors.            //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;

namespace PaintDotNet.IndirectUI
{
    public enum PropertyControlType
        : int
    {
        AngleChooser = 0,
        CheckBox = 1,
        PanAndSlider = 2,
        Slider = 3,
        IncrementButton = 4,
        DropDown = 5,
        TextBox = 6,
        RadioButton = 7,
        ColorWheel = 8
    }
}
