/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;

namespace PaintDotNet
{
    public sealed class UnitsComboBoxHandler
        : IUnitsComboBox
    {
        private ComboBox comboBox;

        [Browsable(false)]
        public ComboBox ComboBox
        {
            get
            {
                return this.comboBox;
            }
        }

        public UnitsComboBoxHandler(ComboBox comboBox)
        {
            this.comboBox = comboBox;
            this.comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.comboBox.SelectedIndexChanged += ComboBox_SelectedIndexChanged;
            ReloadItems();
        }

        private bool lowercase = true;

        private Hashtable unitsToString;
        private Hashtable stringToUnits;

        // maps from MeasurementUnit->bool for whether that item should be in the list or not
        private Hashtable measurementItems;

        private UnitsDisplayType unitsDisplayType = UnitsDisplayType.Plural;

        [DefaultValue(UnitsDisplayType.Plural)]
        public UnitsDisplayType UnitsDisplayType
        {
            get
            {
                return this.unitsDisplayType;
            }

            set
            {
                if (this.unitsDisplayType != value)
                {
                    this.unitsDisplayType = value;
                    ReloadItems();
                }
            }
        }

        [DefaultValue(true)]
        public bool LowercaseStrings
        {
            get
            {
                return this.lowercase;
            }

            set
            {
                if (this.lowercase != value)
                {
                    this.lowercase = value;
                    ReloadItems();
                }
            }
        }

        [DefaultValue(MeasurementUnit.Pixel)]
        public MeasurementUnit Units
        {
            get
            {
                object selected = this.stringToUnits[ComboBox.SelectedItem];
                return (MeasurementUnit)selected;
            }

            set
            {
                object selectMe = this.unitsToString[value];
                ComboBox.SelectedItem = selectMe;
            }
        }

        [Browsable(false)]
        public string UnitsText
        {
            get
            {
                if (ComboBox.SelectedItem == null)
                {
                    return string.Empty;
                }
                else
                {
                    return (string)ComboBox.SelectedItem;
                }
            }
        }

        [DefaultValue(true)]
        public bool PixelsAvailable
        {
            get
            {
                return (bool)this.measurementItems[MeasurementUnit.Pixel];
            }

            set
            {
                if (value != this.PixelsAvailable)
                {
                    if (value)
                    {
                        AddUnit(MeasurementUnit.Pixel);
                    }
                    else
                    {
                        if (this.Units == MeasurementUnit.Pixel)
                        {
                            if (this.InchesAvailable)
                            {
                                this.Units = MeasurementUnit.Inch;
                            }
                            else if (this.CentimetersAvailable)
                            {
                                this.Units = MeasurementUnit.Centimeter;
                            }
                        }

                        RemoveUnit(MeasurementUnit.Pixel);
                    }
                }
            }
        }

        [DefaultValue(true)]
        public bool InchesAvailable
        {
            get
            {
                return (bool)this.measurementItems[MeasurementUnit.Inch];
            }
        }

        [DefaultValue(true)]
        public bool CentimetersAvailable
        {
            get
            {
                return (bool)this.measurementItems[MeasurementUnit.Centimeter];
            }
        }

        public void RemoveUnit(MeasurementUnit removeMe)
        {
            InitMeasurementItems();
            this.measurementItems[removeMe] = false;
            ReloadItems();
        }

        public void AddUnit(MeasurementUnit addMe)
        {
            InitMeasurementItems();
            this.measurementItems[addMe] = true;
            ReloadItems();
        }

        private void InitMeasurementItems()
        {
            if (this.measurementItems == null)
            {
                this.measurementItems = new Hashtable();
                this.measurementItems.Add(MeasurementUnit.Pixel, true);
                this.measurementItems.Add(MeasurementUnit.Centimeter, true);
                this.measurementItems.Add(MeasurementUnit.Inch, true);
            }
        }

        private void ReloadItems()
        {
            string suffix;

            switch (this.unitsDisplayType)
            {
                case UnitsDisplayType.Plural:
                    suffix = ".Plural";
                    break;

                case UnitsDisplayType.Singular:
                    suffix = string.Empty;
                    break;

                case UnitsDisplayType.Ratio:
                    suffix = ".Ratio";
                    break;

                default:
                    throw new InvalidEnumArgumentException("UnitsDisplayType");
            }

            InitMeasurementItems();

            MeasurementUnit oldUnits;

            if (this.unitsToString == null)
            {
                oldUnits = MeasurementUnit.Pixel;
            }
            else
            {
                oldUnits = this.Units;
            }

            ComboBox.Items.Clear();

            string pixelsString = PdnResources.GetString("MeasurementUnit.Pixel" + suffix);
            string inchesString = PdnResources.GetString("MeasurementUnit.Inch" + suffix);
            string centimetersString = PdnResources.GetString("MeasurementUnit.Centimeter" + suffix);

            if (lowercase)
            {
                // TODO: we shouldn't really be using ToLower() here, these should be separately localizable strings

                pixelsString = pixelsString.ToLower();
                inchesString = inchesString.ToLower();
                centimetersString = centimetersString.ToLower();
            }

            this.unitsToString = new Hashtable();
            this.unitsToString.Add(MeasurementUnit.Pixel, pixelsString);
            this.unitsToString.Add(MeasurementUnit.Inch, inchesString);
            this.unitsToString.Add(MeasurementUnit.Centimeter, centimetersString);

            this.stringToUnits = new Hashtable();

            if ((bool)this.measurementItems[MeasurementUnit.Pixel])
            {
                this.stringToUnits.Add(pixelsString, MeasurementUnit.Pixel);
                ComboBox.Items.Add(pixelsString);
            }

            if ((bool)this.measurementItems[MeasurementUnit.Inch])
            {
                this.stringToUnits.Add(inchesString, MeasurementUnit.Inch);
                ComboBox.Items.Add(inchesString);
            }

            if ((bool)this.measurementItems[MeasurementUnit.Centimeter])
            {
                this.stringToUnits.Add(centimetersString, MeasurementUnit.Centimeter);
                ComboBox.Items.Add(centimetersString);
            }

            if (!(bool)this.measurementItems[oldUnits])
            {
                if (ComboBox.Items.Count == 0)
                {
                    ComboBox.SelectedItem = null;
                }
                else
                {
                    ComboBox.SelectedIndex = 0;
                }
            }
            else
            {
                this.Units = oldUnits;
            }
        }

        public event EventHandler UnitsChanged;

        private void OnUnitsChanged()
        {
            if (UnitsChanged != null)
            {
                UnitsChanged(this, EventArgs.Empty);
            }
        }

        private void ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.OnUnitsChanged();
        }
    }
}
