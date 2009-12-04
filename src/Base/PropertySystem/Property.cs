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
using System.ComponentModel;

namespace PaintDotNet.PropertySystem
{
    public abstract class Property
        : ICloneable<Property>
    {
        public static ValueValidationFailureResult DefaultValueValidationFailureResult
        {
            get
            {
#if DEBUG
                return ValueValidationFailureResult.ThrowException;
#else
                return ValueValidationFailureResult.Clamp;
#endif
            }
        }

        public static Property Create(Type valueType, object name)
        {
            return Create(valueType, name, null);
        }

        public static Property Create(Type valueType, object name, object defaultValue)
        {
            // TODO: find some way to do this better, using attributes+reflection or something, yes?

            if (valueType == typeof(bool))
            {
                return new BooleanProperty(name, (bool)(defaultValue ?? (object)false));
            }
            else if (valueType == typeof(double))
            {
                return new DoubleProperty(name, (double)(defaultValue ?? (object)0.0));
            }
            else if (valueType == typeof(Pair<double, double>))
            {
                return new DoubleVectorProperty(name, (Pair<double, double>)(defaultValue ?? (object)Pair.Create(0.0, 0.0)));
            }
            else if (valueType == typeof(int))
            {
                return new Int32Property(name, (int)(defaultValue ?? (object)0));
            }
            else if (valueType == typeof(string))
            {
                return new StringProperty(name, (string)defaultValue);
            }
            else if (typeof(ImageResource).IsAssignableFrom(valueType))
            {
                return new ImageProperty(name, (ImageResource)defaultValue);
            }
            else if (valueType.IsEnum)
            {
                return StaticListChoiceProperty.CreateForEnum(
                    valueType,
                    name,
                    defaultValue ?? ((object[])Enum.GetValues(valueType))[0],
                    false);
            }

            throw new ArgumentException(string.Format("Not a valid type: {0}", valueType.FullName));
        }

        private string name;
        private Type valueType;
        private object ourValue;
        private object defaultValue;
        private bool readOnly;
        private ValueValidationFailureResult vvfResult;
        private EventHandler readOnlyChanged;
        private EventHandler valueChanged;
        private bool eventAddAllowed = true;

        internal IDisposable BeginEventAddMoratorium()
        {
            if (!this.eventAddAllowed)
            {
                throw new InvalidOperationException("An event add moratorium is already in effect");
            }

            IDisposable undoFn = new CallbackOnDispose(() => this.eventAddAllowed = true);

            this.eventAddAllowed = false;

            return undoFn;
        }

        public event EventHandler ReadOnlyChanged
        {
            add
            {
                if (this.eventAddAllowed)
                {
                    this.readOnlyChanged += value;
                }
            }

            remove
            {
                this.readOnlyChanged -= value;
            }
        }

        protected virtual void OnReadOnlyChanged()
        {
            if (this.readOnlyChanged != null)
            {
                this.readOnlyChanged(this, EventArgs.Empty);
            }
        }

        public event EventHandler ValueChanged
        {
            add
            {
                if (this.eventAddAllowed)
                {
                    this.valueChanged += value;
                }
            }

            remove
            {
                this.valueChanged -= value;
            }
        }

        protected virtual void OnValueChanged()
        {
            if (this.valueChanged != null)
            {
                this.valueChanged(this, EventArgs.Empty);
            }
        }

        public ValueValidationFailureResult ValueValidationFailureResult
        {
            get
            {
                return this.vvfResult;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        [Obsolete("Use the ValueType property instead")]
        public Type Type
        {
            get
            {
                return this.valueType;
            }
        }

        public Type ValueType
        {
            get
            {
                return this.valueType;
            }
        }

        public object Value
        {
            get
            {
                return this.ourValue;
            }

            set
            {
                SetValueCore(value);
            }
        }

        protected virtual object OnCoerceValue(object newValue)
        {
            return newValue;
        }

        private void SetValueCore(object value)
        {
            VerifyNotReadOnly();

            object value2 = OnCoerceValue(value);

            if (!value2.Equals(this.ourValue))
            {
                if (ValidateNewValue(value2))
                {
                    this.ourValue = value2;
                    OnValueChanged();
                }
                else
                {
                    switch (this.vvfResult)
                    {
                        case ValueValidationFailureResult.ThrowException:
                            throw new ArgumentOutOfRangeException(string.Format("{0} is not a valid value for {1}", value2.ToString(), this.name));

                        case ValueValidationFailureResult.Ignore:
                            System.Diagnostics.Debug.WriteLine(string.Format("{0} is not a valid value for {1}", value2.ToString(), this.name));
                            OnValueChanged();
                            break;

                        case ValueValidationFailureResult.Clamp:
                            object clampedValue = ClampNewValue(value2);
                            Value = clampedValue;
                            break;
                    }
                }
            }
        }

        private object ClampNewValue(object newValue)
        {
            return OnClampNewValue(newValue);
        }

        protected abstract object OnClampNewValue(object newValue);

        public object DefaultValue
        {
            get
            {
                return this.defaultValue;
            }
        }

        public bool ReadOnly
        {
            get
            {
                return this.readOnly;
            }

            set
            {
                if (this.readOnly != value)
                {
                    this.readOnly = value;
                    OnReadOnlyChanged();
                }
            }
        }

        internal Property(object name, Type valueType, object defaultValue, bool readOnly, ValueValidationFailureResult vvfResult)
        {
            if (defaultValue != null)
            {
                Type defaultValueType = defaultValue.GetType();

                if (!valueType.IsAssignableFrom(defaultValueType))
                {
                    throw new ArgumentOutOfRangeException(
                        "type",
                        string.Format(
                            "defaultValue is not of type specified in constructor. valueType.Name = {0}, defaultValue.GetType().Name = {1}",
                            valueType.Name,
                            defaultValue.GetType().Name));
                }
            }

            this.name = name.ToString();
            this.valueType = valueType;
            this.ourValue = defaultValue;
            this.defaultValue = defaultValue;
            this.readOnly = readOnly;

            switch (vvfResult)
            {
                case ValueValidationFailureResult.Clamp:
                case ValueValidationFailureResult.Ignore:
                case ValueValidationFailureResult.ThrowException:
                    this.vvfResult = vvfResult;
                    break;

                default:
                    throw new InvalidEnumArgumentException("vvfResult", (int)vvfResult, typeof(ValueValidationFailureResult));
            }
        }

        internal Property(Property cloneMe, Property sentinelNotUsed)
        {
            // sentinelNotUsed is just there so that this constructor can be unambiguous from Property<T>(object)
            // the call to no op is so that code verifiers aren't thrown off
            sentinelNotUsed.NoOp();

            this.name = cloneMe.name;
            this.valueType = cloneMe.valueType;
            this.ourValue = cloneMe.ourValue;
            this.defaultValue = cloneMe.defaultValue;
            this.readOnly = cloneMe.readOnly;
            this.vvfResult = cloneMe.vvfResult;
        }

        protected void VerifyNotReadOnly()
        {
            if (this.readOnly)
            {
                throw new ReadOnlyException("This property is read only");
            }

            return;
        }

        protected virtual bool ValidateNewValue(object newValue)
        {
            return true;
        }

        protected virtual string PropertyValueToString(object value)
        {
            return value.ToString();
        }

        // NOTE: Cloning must NOT clone event subscriptions.
        public abstract Property Clone();

        object ICloneable.Clone()
        {
            return Clone();
        }

        // lets our use of sentinelNotUsed in the constructor to be FXCOP safe
        protected void NoOp()
        {
        }
    }    
}
