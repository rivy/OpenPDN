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
using System.Text;

namespace PaintDotNet.IndirectUI
{
    public enum ControlInfoPropertyNames
    {
        DisplayName = 0,
        Description = 1,
        ControlType = 2,
        ButtonText = 3,
        UseExponentialScale = 4,
        DecimalPlaces = 5,
        SliderSmallChange = 6,
        SliderSmallChangeX = 7,
        SliderSmallChangeY = 8,
        SliderLargeChange = 9,
        SliderLargeChangeX = 10,
        SliderLargeChangeY = 11,
        UpDownIncrement = 12,
        UpDownIncrementX = 13,
        UpDownIncrementY = 14,
        StaticImageUnderlay = 15,
        Multiline = 16,
        ShowResetButton = 17,
        SliderShowTickMarks = 18,
        SliderShowTickMarksX = 19,
        SliderShowTickMarksY = 20,

        WindowTitle = 21,
        WindowWidthScale = 22,
        WindowIsSizable = 23
    }
}
