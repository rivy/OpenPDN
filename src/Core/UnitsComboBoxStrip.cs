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
using System.Windows.Forms;

namespace PaintDotNet
{
    public sealed class UnitsComboBoxStrip
          : ToolStripComboBox,
            IUnitsComboBox
    {
        private UnitsComboBoxHandler comboBoxHandler;

        public UnitsComboBoxStrip()
        {
            this.comboBoxHandler = new UnitsComboBoxHandler(this.ComboBox);
        }

        public UnitsDisplayType UnitsDisplayType
        {
            get
            {
                return this.comboBoxHandler.UnitsDisplayType;
            }

            set
            {
                this.comboBoxHandler.UnitsDisplayType = value;
            }
        }

        public bool LowercaseStrings
        {
            get
            {
                return this.comboBoxHandler.LowercaseStrings;
            }

            set
            {
                this.comboBoxHandler.LowercaseStrings = value;
            }
        }

        public MeasurementUnit Units
        {
            get
            {
                return this.comboBoxHandler.Units;
            }

            set
            {
                this.comboBoxHandler.Units = value;
            }
        }

        public string UnitsText
        {
            get
            {
                return this.comboBoxHandler.UnitsText;
            }
        }

        public bool PixelsAvailable
        {
            get
            {
                return this.comboBoxHandler.PixelsAvailable;
            }

            set
            {
                this.comboBoxHandler.PixelsAvailable = value;
            }
        }

        public bool InchesAvailable
        {
            get
            {
                return this.comboBoxHandler.InchesAvailable;
            }
        }

        public bool CentimetersAvailable
        {
            get
            {
                return this.comboBoxHandler.CentimetersAvailable;
            }
        }

        public void RemoveUnit(MeasurementUnit removeMe)
        {
            this.comboBoxHandler.AddUnit(removeMe);
        }

        public void AddUnit(MeasurementUnit addMe)
        {
            this.comboBoxHandler.AddUnit(addMe);
        }

        public event EventHandler UnitsChanged
        {
            add
            {
                this.comboBoxHandler.UnitsChanged += value;
            }

            remove
            {
                this.comboBoxHandler.UnitsChanged -= value;
            }
        }
    }
}
