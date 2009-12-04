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
    /// Defines a rule for two properties, A and B, such that A will always be less than or equal to B,
    /// and B will always be greater than or equal to A. If A is set to a new value that is greater than B,
    /// then B will be set equal to A. If B is set to a new value that is less than A, then A will be set
    /// equal to B.
    /// </summary>
    public sealed class SoftMutuallyBoundMinMaxRule<TValue, TProperty>
        : PropertyCollectionRule
          where TProperty : ScalarProperty<TValue>
          where TValue : struct, IComparable<TValue>
    {
        private string minPropertyName;
        private string maxPropertyName;

        public SoftMutuallyBoundMinMaxRule(Property minProperty, Property maxProperty)
            : this(minProperty.Name, maxProperty.Name)
        {
        }

        public SoftMutuallyBoundMinMaxRule(object minPropertyName, object maxPropertyName)
        {
            this.minPropertyName = minPropertyName.ToString();
            this.maxPropertyName = maxPropertyName.ToString();
        }

        protected override void OnInitialized()
        {
            TProperty minProperty = (TProperty)Owner[this.minPropertyName];
            TProperty maxProperty = (TProperty)Owner[this.maxPropertyName];

            if (ScalarProperty<TValue>.IsGreaterThan(minProperty.MinValue, maxProperty.MinValue))
            {
                throw new ArgumentOutOfRangeException("MinProperty.MinValue must be less than or equal to MaxProperty.MinValue");
            }

            if (ScalarProperty<TValue>.IsGreaterThan(minProperty.MaxValue, maxProperty.MaxValue))
            {
                throw new ArgumentOutOfRangeException("MinProperty.MaxValue must be less than or equal to MaxProperty.MaxValue");
            }

            // Analyze the PropertyCollection we are bound to in order to ensure that we do not
            // have any "infinite loops". It is safe to simply ensure that no other SoftMutuallyBoundMinMaxRule
            // has minPropertyName as a maxPropertyName.
            foreach (PropertyCollectionRule rule in this.Owner.Rules)
            {
                SoftMutuallyBoundMinMaxRule<TValue, TProperty> asOurRule = rule as SoftMutuallyBoundMinMaxRule<TValue, TProperty>;

                if (asOurRule != null)
                {
                    if (asOurRule.maxPropertyName.ToString() == this.minPropertyName.ToString())
                    {
                        throw new ArgumentException("The graph of SoftMutuallyBoundMinMaxRule's in the PropertyCollection has a cycle in it");
                    }
                }
            }

            minProperty.ValueChanged += new EventHandler(MinProperty_ValueChanged);
            maxProperty.ValueChanged += new EventHandler(MaxProperty_ValueChanged);
        }

        private void MaxProperty_ValueChanged(object sender, EventArgs e)
        {
            TProperty minProperty = (TProperty)Owner[this.minPropertyName];
            TProperty maxProperty = (TProperty)Owner[this.maxPropertyName];

            if (maxProperty.IsLessThan(minProperty))
            {
                minProperty.Value = maxProperty.Value;
            }
        }

        private void MinProperty_ValueChanged(object sender, EventArgs e)
        {
            TProperty minProperty = (TProperty)Owner[this.minPropertyName];
            TProperty maxProperty = (TProperty)Owner[this.maxPropertyName];

            if (minProperty.IsGreaterThan(maxProperty))
            {
                maxProperty.Value = minProperty.Value;
            }
        }

        public override PropertyCollectionRule Clone()
        {
            return new SoftMutuallyBoundMinMaxRule<TValue, TProperty>(this.minPropertyName, this.maxPropertyName);
        }
    }
}
