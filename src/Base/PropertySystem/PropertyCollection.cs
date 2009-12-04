/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) Rick Brewster, Tom Jackson, and past contributors.            //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text;

namespace PaintDotNet.PropertySystem
{
    public sealed class PropertyCollection
        : INotifyPropertyChanged,
          IEnumerable<Property>,
          ICloneable<PropertyCollection>
    {
        private bool eventAddAllowed = true;
        private Dictionary<string, Property> properties = new Dictionary<string, Property>();
        private List<PropertyCollectionRule> rules = new List<PropertyCollectionRule>();

        public static PropertyCollection CreateEmpty()
        {
            return new PropertyCollection(new Property[0]);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public Property this[object propertyName]
        {
            get
            {
                string propertyNameString = propertyName.ToString();
                Property theProperty;
                this.properties.TryGetValue(propertyNameString, out theProperty);
                return theProperty;
            }
        }

        public int Count
        {
            get
            {
                return this.properties.Count;
            }
        }

        public IEnumerable<Property> Properties
        {
            get
            {
                return this.properties.Values;
            }
        }

        public IEnumerable<PropertyCollectionRule> Rules
        {
            get
            {
                return this.rules;
            }
        }

        public IEnumerable<string> PropertyNames
        {
            get
            {
                return this.properties.Keys;
            }
        }

        public PropertyCollection(IEnumerable<Property> properties)
        {
            Initialize(properties, new PropertyCollectionRule[0]);
        }

        public PropertyCollection(
            IEnumerable<Property> properties, 
            IEnumerable<PropertyCollectionRule> rules)
        {
            Initialize(properties, rules);
        }

        public IDisposable __Internal_BeginEventAddMoratorium()
        {
            if (!this.eventAddAllowed)
            {
                throw new InvalidOperationException("An event add moratorium is already in effect");
            }

            List<IDisposable> propUndoFns = new List<IDisposable>(this.properties.Count);

            foreach (Property property in this.properties.Values)
            {
                IDisposable propUndoFn = property.BeginEventAddMoratorium();
                propUndoFns.Add(propUndoFn);
            }

            IDisposable undoFn = new CallbackOnDispose(() =>
                {
                    this.eventAddAllowed = true;

                    foreach (IDisposable propUndoFn in propUndoFns)
                    {
                        propUndoFn.Dispose();
                    }
                });

            this.eventAddAllowed = false;

            return undoFn;
        }

        private void Initialize(
            IEnumerable<Property> properties, 
            IEnumerable<PropertyCollectionRule> rules)
        {
            foreach (Property property in properties)
            {
                Property propertyClone = property.Clone();
                this.properties.Add(propertyClone.Name, propertyClone);
            }

            foreach (PropertyCollectionRule rule in rules)
            {
                PropertyCollectionRule ruleClone = rule.Clone();
                this.rules.Add(ruleClone);
            }

            foreach (PropertyCollectionRule rule in this.rules)
            {
                rule.Initialize(this);
            }

            HookUpEvents();
        }

        private void Property_ValueChanged(object sender, EventArgs e)
        {
            OnPropertyChanged((sender as Property).Name);
        }

        private void Property_ReadOnlyChanged(object sender, EventArgs e)
        {
            OnPropertyChanged((sender as Property).Name);
        }

        private void HookUpEvents()
        {
            foreach (Property property in this.properties.Values)
            {
                property.ValueChanged -= Property_ValueChanged;
                property.ValueChanged += Property_ValueChanged;

                property.ReadOnlyChanged -= Property_ReadOnlyChanged;
                property.ReadOnlyChanged += Property_ReadOnlyChanged;
            }
        }

        public void CopyCompatibleValuesFrom(PropertyCollection srcProps)
        {
            CopyCompatibleValuesFrom(srcProps, false);
        }

        public void CopyCompatibleValuesFrom(PropertyCollection srcProps, bool ignoreReadOnlyFlags)
        {
            foreach (Property srcProp in srcProps)
            {
                Property dstProp = this[srcProp.Name];

                if (dstProp != null && dstProp.ValueType == srcProp.ValueType)
                {
                    if (dstProp.ReadOnly && ignoreReadOnlyFlags)
                    {
                        dstProp.ReadOnly = false;
                        dstProp.Value = srcProp.Value;
                        dstProp.ReadOnly = true;
                    }
                    else
                    {
                        dstProp.Value = srcProp.Value;
                    }
                }
            }
        }

        public static PropertyCollection CreateMerged(PropertyCollection pc1, PropertyCollection pc2)
        {
            foreach (Property p1 in pc1.Properties)
            {
                Property p2 = pc2[p1.Name];

                if (p2 != null)
                {
                    throw new ArgumentException("pc1 must not have any properties with the same name as in pc2");
                }
            }

            Property[] allProps = new Property[pc1.Count + pc2.Count];
            int index = 0;

            foreach (Property p1 in pc1)
            {
                allProps[index] = p1.Clone();
                ++index;
            }

            foreach (Property p2 in pc2)
            {
                allProps[index] = p2.Clone();
                ++index;
            }

            List<PropertyCollectionRule> allRules = new List<PropertyCollectionRule>();

            foreach (PropertyCollectionRule pcr1 in pc1.Rules)
            {
                allRules.Add(pcr1);
            }

            foreach (PropertyCollectionRule pcr2 in pc2.Rules)
            {
                allRules.Add(pcr2);
            }

            PropertyCollection mergedPC = new PropertyCollection(allProps, allRules);
            return mergedPC;
        }

        public PropertyCollection Clone()
        {
            List<Property> clonedProperties = new List<Property>();

            foreach (Property property in this.Properties)
            {
                Property clonedProperty = property.Clone();
                clonedProperties.Add(clonedProperty);
            }

            List<PropertyCollectionRule> clonedRules = new List<PropertyCollectionRule>();

            foreach (PropertyCollectionRule rule in this.rules)
            {
                PropertyCollectionRule clonedRule = rule.Clone();
                clonedRules.Add(clonedRule);
            }

            return new PropertyCollection(clonedProperties, clonedRules);
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        public IEnumerator<Property> GetEnumerator()
        {
            return Properties.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
