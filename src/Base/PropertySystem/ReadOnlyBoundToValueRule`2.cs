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
    public sealed class ReadOnlyBoundToValueRule<TValue, TProperty>
        : PropertyCollectionRule
          where TProperty : Property<TValue>
    {
        private string targetPropertyName;
        private string sourcePropertyName;
        private bool inverse;
        private TValue[] valuesForReadOnly;

        // (inverse = false) -> If sourceProperty.Value equals any of the values in valuesForReadOnly, then targetProperty.ReadOnly will be set to true, else it will be set to false.
        // (inverse = true)  -> If sourceProperty.Value equals any of the values in valuesForReadOnly, then targetProperty.ReadOnly will be set to false, else it will be set to true.

        public ReadOnlyBoundToValueRule(Property targetProperty, TProperty sourceProperty, TValue valueForReadOnly, bool inverse)
            : this(targetProperty.Name, sourceProperty.Name, new TValue[1] { valueForReadOnly }, inverse)
        {
        }

        public ReadOnlyBoundToValueRule(Property targetProperty, TProperty sourceProperty, TValue[] valuesForReadOnly, bool inverse)
            : this(targetProperty.Name, sourceProperty.Name, valuesForReadOnly, inverse)
        {
        }

        public ReadOnlyBoundToValueRule(object targetPropertyName, object sourcePropertyName, TValue valueForReadOnly, bool inverse)
            : this(targetPropertyName, sourcePropertyName, new TValue[1] { valueForReadOnly }, inverse)
        {
        }

        public ReadOnlyBoundToValueRule(object targetPropertyName, object sourcePropertyName, TValue[] valuesForReadOnly, bool inverse)
        {
            this.targetPropertyName = targetPropertyName.ToString();
            this.sourcePropertyName = sourcePropertyName.ToString();
            this.valuesForReadOnly = (TValue[])valuesForReadOnly.Clone();
            this.inverse = inverse;
        }

        protected override void OnInitialized()
        {
            Property targetProperty = Owner[this.targetPropertyName];
            TProperty sourceProperty = (TProperty)Owner[this.sourcePropertyName];

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
            TProperty sourceProperty = (TProperty)Owner[this.sourcePropertyName];

            bool readOnly = false;

            foreach (TValue valueForReadOnly in this.valuesForReadOnly)
            {
                if (valueForReadOnly.Equals(sourceProperty.Value))
                {
                    readOnly = true;
                    break;
                }
            }

            targetProperty.ReadOnly = readOnly ^ this.inverse;
        }

        public override PropertyCollectionRule Clone()
        {
            return new ReadOnlyBoundToValueRule<TValue, TProperty>(
                this.targetPropertyName, 
                this.sourcePropertyName, 
                this.valuesForReadOnly, 
                this.inverse);
        }
    }
}
