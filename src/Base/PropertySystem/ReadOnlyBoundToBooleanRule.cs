/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) Rick Brewster, Tom Jackson, and past contributors.            //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;

namespace PaintDotNet.PropertySystem
{
    /// <summary>
    /// Defines a rule that binds the ReadOnly flag of property A to the value of boolean property B.
    /// A and B must be different properties.
    /// </summary>
    public sealed class ReadOnlyBoundToBooleanRule
        : PropertyCollectionRule
    {
        private string targetPropertyName;
        private string sourceBooleanPropertyName;
        private bool inverse;

        public ReadOnlyBoundToBooleanRule(Property targetProperty, BooleanProperty sourceProperty, bool inverse)
            : this(targetProperty.Name, sourceProperty.Name, inverse)
        {
        }

        public ReadOnlyBoundToBooleanRule(object targetPropertyName, object sourceBooleanPropertyName, bool inverse)
        {
            this.targetPropertyName = targetPropertyName.ToString();
            this.sourceBooleanPropertyName = sourceBooleanPropertyName.ToString();
            this.inverse = inverse;
        }

        protected override void OnInitialized()
        {
            Property targetProperty = Owner[this.targetPropertyName];
            BooleanProperty sourceProperty = (BooleanProperty)Owner[this.sourceBooleanPropertyName];

            if (0 == string.Compare(targetProperty.Name, sourceProperty.Name, StringComparison.InvariantCulture))
            {
                throw new ArgumentException("source and target properties must be different");
            }

            Sync();

            sourceProperty.ValueChanged += new EventHandler(SourceProperty_ValueChanged);
        }

        private void SourceProperty_ValueChanged(object sender, EventArgs e)
        {
            Sync();
        }

        private void Sync()
        {
            Property targetProperty = Owner[this.targetPropertyName];
            BooleanProperty sourceProperty = (BooleanProperty)Owner[this.sourceBooleanPropertyName];

            bool readOnly = sourceProperty.Value;

            targetProperty.ReadOnly = readOnly ^ this.inverse;
        }

        public override PropertyCollectionRule Clone()
        {
            return new ReadOnlyBoundToBooleanRule(this.targetPropertyName, this.sourceBooleanPropertyName, this.inverse);
        }
    }
}