/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

namespace PaintDotNet
{
    public interface IUnitsComboBox
    {
        UnitsDisplayType UnitsDisplayType
        {
            get;
            set;
        }

        bool LowercaseStrings
        {
            get;
            set;
        }

        MeasurementUnit Units
        {
            get;
            set;
        }

        string UnitsText
        {
            get;
        }

        bool PixelsAvailable
        {
            get;
            set;
        }

        bool InchesAvailable
        {
            get;
        }

        bool CentimetersAvailable
        {
            get;
        }

        void RemoveUnit(MeasurementUnit removeMe);
        void AddUnit(MeasurementUnit addMe);

        event EventHandler UnitsChanged;
    }
}
