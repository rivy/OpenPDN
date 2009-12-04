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

namespace PaintDotNet
{
    /// <summary>
    /// Provides convenience methods for converting between enumeration values
    /// and their localized value names. This class only works with strings
    /// loaded via the PdnResources class.
    /// </summary>
    public sealed class EnumLocalizer
    {
        private Type enumType;
        private Hashtable valueToName;
        private Hashtable nameToValue;
        private static Hashtable typeToWrapper;

        public Type EnumType
        {
            get
            {
                return this.enumType;
            }
        }

        public object[] GetEnumValues()
        {
            ICollection valueNames = this.valueToName.Keys;
            object[] values = new object[valueNames.Count];

            int index = 0;
            foreach (string valueName in valueNames)
            {
                values[index] = Enum.Parse(this.enumType, valueName);
                ++index;
            }           

            return values;
        }

        public string[] GetLocalizedNames()
        {
            ICollection keysCollection = this.nameToValue.Keys;
            string[] keysArray = new string[keysCollection.Count];

            int index = 0;
            foreach (string key in keysCollection)
            {
                keysArray[index] = key;
                ++index;
            }

            return keysArray;
        }

        public static string EnumValueToLocalizedName(Type enumType, object enumValue)
        {
            EnumLocalizer wrapper = EnumLocalizer.Create(enumType);
            return wrapper.EnumValueToLocalizedName(enumValue);
        }
        
        public string EnumValueToLocalizedName(object enumValue)
        {
            object retValue = this.valueToName[enumValue.ToString()];

            if (retValue == null)
            {
                this.valueToName.Remove(enumValue);
                return null;
            }
            else
            {
                return (string)retValue;
            }
        }

        public object LocalizedNameToEnumValue(string locName)
        {
            object enumValueName = this.nameToValue[locName];

            if (enumValueName == null)
            {
                this.nameToValue.Remove(locName);
                return null;
            }
            else
            {
                object enumValue = Enum.Parse(this.enumType, (string)enumValueName);
                return enumValue;
            }
        }

        public string LocalizedNameToEnumValueName(string locName)
        {
            object enumValueName = this.nameToValue[locName];

            if (enumValueName == null)
            {
                this.nameToValue.Remove(locName);
                return null;
            }
            else
            {
                return (string)enumValueName;
            }
        }

        public static EnumLocalizer Create(Type enumType)
        {
            if (typeToWrapper == null)
            {
                typeToWrapper = new Hashtable();
            }

            object wrapper = typeToWrapper[enumType];

            if (wrapper == null)
            {
                wrapper = new EnumLocalizer(enumType);
                typeToWrapper[enumType] = wrapper;
            }

            return (EnumLocalizer)wrapper;
        }

        private EnumLocalizer(Type enumType)
        {
            this.enumType = enumType;
            
            this.valueToName = new Hashtable();
            this.nameToValue = new Hashtable();

            foreach (string enumValueName in Enum.GetNames(this.enumType))
            {
                string resourceName = this.enumType.Name + "." + enumValueName;
                string localizedName = PdnResources.GetString(resourceName);
                this.valueToName.Add(enumValueName, localizedName);
                this.nameToValue.Add(localizedName, enumValueName);
            }
        }
    }
}
