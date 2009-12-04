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
using System.Reflection;
using System.Text;

namespace PaintDotNet
{
    public sealed class PluginSupportInfo
    {
        private PluginSupportInfo()
        {
        }

        private static IPluginSupportInfo GetPluginSupportInfo(ICustomAttributeProvider icap)
        {
            object[] attributes;

            try
            {
                attributes = icap.GetCustomAttributes(typeof(PluginSupportInfoAttribute), false);
            }

            catch (Exception)
            {
                attributes = new object[0]; 
            }

            if (attributes.Length == 1)
            {
                PluginSupportInfoAttribute psiAttr = (PluginSupportInfoAttribute)attributes[0];
                return (IPluginSupportInfo)psiAttr;
            }
            else
            {
                return null;
            }
        }

        public static IPluginSupportInfo GetPluginSupportInfo(Assembly assembly)
        {
            return GetPluginSupportInfo((ICustomAttributeProvider)assembly);
        }

        public static IPluginSupportInfo GetPluginSupportInfo(Type type)
        {
            return GetPluginSupportInfo((ICustomAttributeProvider)type) ?? GetPluginSupportInfo(type.Assembly);
        }

        public static IPluginSupportInfo GetPluginSupportInfo(object theObject)
        {
            if (theObject is IPluginSupportInfo)
            {
                return (IPluginSupportInfo)theObject;
            }
            else
            {
                return GetPluginSupportInfo(theObject.GetType());
            }
        }
    }
}
