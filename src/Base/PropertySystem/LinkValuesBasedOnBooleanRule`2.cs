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

namespace PaintDotNet.PropertySystem
{
    /// <summary>
    /// Defines a rule that binds the ReadOnly flag of property A to the value of boolean property B.
    /// A and B must be different properties.
    /// </summary>
    public sealed class LinkValuesBasedOnBooleanRule<TValue, TProperty>
        : PropertyCollectionRule
          where TProperty : ScalarProperty<TValue>
          where TValue : struct, IComparable<TValue>
    {
        private string[] targetPropertyNames;
        private string sourcePropertyName;
        private bool inverse;
        private string lastChangedPropertyName;

        // When inverse=false, the target properties will be linked when sourceProperty == true

        public LinkValuesBasedOnBooleanRule(IEnumerable<TProperty> targetProperties, BooleanProperty sourceProperty, bool inverse)
            : this(new List<TProperty>(targetProperties).ConvertAll(p => p.Name).ToArray(), sourceProperty.Name, inverse)
        {            
        }

        public LinkValuesBasedOnBooleanRule(object[] targetPropertyNames, object sourcePropertyName, bool inverse)
        {
            if (targetPropertyNames.Length < 2)
            {
                throw new ArgumentException("Must have at least 2 items in targetPropertyNames");
            }

            this.targetPropertyNames = new string[targetPropertyNames.Length];

            for (int i = 0; i < this.targetPropertyNames.Length; ++i)
            {
                this.targetPropertyNames[i] = targetPropertyNames[i].ToString();
            }

            this.sourcePropertyName = sourcePropertyName.ToString();
            this.inverse = inverse;
            this.lastChangedPropertyName = this.targetPropertyNames[0];
        }

        protected override void OnInitialized()
        {
            // Verify the sourcePropertyName is not in targetPropertyNames
            if (-1 != Array.IndexOf(this.targetPropertyNames, this.sourcePropertyName))
            {
                throw new ArgumentException("sourceProperty may not be in the list of targetProperties");
            }

            // Verify that the intersection of this rule's targetProperty and that of all the other Link*'d rules is empty
            Set<string> ourTargetPropertyNamesSet = null;

            foreach (PropertyCollectionRule rule in Owner.Rules)
            {
                var asLinkRule = rule as LinkValuesBasedOnBooleanRule<TValue, TProperty>;

                if (asLinkRule != null && !object.ReferenceEquals(this, asLinkRule))
                {
                    if (ourTargetPropertyNamesSet == null)
                    {
                        ourTargetPropertyNamesSet = new Set<string>(this.targetPropertyNames);
                    }

                    Set<string> theirTargetPropertyNamesSet = new Set<string>(asLinkRule.targetPropertyNames);

                    Set<string> intersection = Set<string>.Intersect(ourTargetPropertyNamesSet, theirTargetPropertyNamesSet);

                    if (intersection.Count != 0)
                    {
                        throw new ArgumentException("Cannot assign a property to be linked with more than one LinkValuesBasedOnBooleanRule instance");
                    }
                }
            }

            // Verify every property is of type TProperty
            // That all the ranges are the same
            // Sign up for events
            TProperty firstProperty = (TProperty)this.Owner[this.targetPropertyNames[0]];

            foreach (string targetPropertyName in this.targetPropertyNames)
            {
                TProperty targetProperty = (TProperty)this.Owner[targetPropertyName];

                if (!(targetProperty is TProperty))
                {
                    throw new ArgumentException("All of the target properties must be of type TProperty (" + typeof(TProperty).FullName + ")");
                }

                if (!ScalarProperty<TValue>.IsEqualTo(targetProperty.MinValue, firstProperty.MinValue) ||
                    !ScalarProperty<TValue>.IsEqualTo(targetProperty.MaxValue, firstProperty.MaxValue))
                {
                    throw new ArgumentException("All of the target properties must have the same min/max range");
                }

                targetProperty.ValueChanged += new EventHandler(TargetProperty_ValueChanged);
            }

            BooleanProperty sourceProperty = (BooleanProperty)this.Owner[sourcePropertyName];

            sourceProperty.ValueChanged += new EventHandler(SourceProperty_ValueChanged);

            Sync();
        }

        private void TargetProperty_ValueChanged(object sender, EventArgs e)
        {
            this.lastChangedPropertyName = ((TProperty)sender).Name;
            Sync();
        }

        private void SourceProperty_ValueChanged(object sender, EventArgs e)
        {
            Sync();
        }

        private void Sync()
        {
            BooleanProperty sourceProperty = (BooleanProperty)Owner[this.sourcePropertyName];

            if (sourceProperty.Value ^ this.inverse)
            {
                TProperty lastChangedProperty = (TProperty)Owner[this.lastChangedPropertyName];
                TValue newValue = lastChangedProperty.Value;

                foreach (string targetPropertyName in this.targetPropertyNames)
                {
                    TProperty targetProperty = (TProperty)Owner[targetPropertyName];

                    if (targetProperty.ReadOnly)
                    {
                        targetProperty.ReadOnly = false;
                        targetProperty.Value = newValue;
                        targetProperty.ReadOnly = true;
                    }
                    else
                    {
                        targetProperty.Value = newValue;
                    }
                }
            }
        }

        public override PropertyCollectionRule Clone()
        {
            return new LinkValuesBasedOnBooleanRule<TValue, TProperty>(this.targetPropertyNames, this.sourcePropertyName, this.inverse);
        }
    }
}