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
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// This class implements some common things on top of the regular NumericUpDown class.
    /// </summary>
    public class PdnNumericUpDown
        : NumericUpDown
    {
        public PdnNumericUpDown()
        {
            TextAlign = HorizontalAlignment.Right;
        }

        protected override void OnEnter(EventArgs e)
        {
            Select(0, Text.Length);
            base.OnEnter(e);
        }

        protected override void OnLeave(EventArgs e)
        {
            if (Value < Minimum)
            {
                Value = Minimum;
            }
            else if (Value > Maximum)
            {
                Value = Maximum;
            }

            decimal parsedValue;

            if (decimal.TryParse(Text, out parsedValue))
            {
                Value = parsedValue;
            }

            base.OnLeave(e);
        }
    }
}
