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
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;

namespace PaintDotNet.SystemLayer
{
    /// <summary>
    /// This is an implementation of SerializationBinder that tries to find a match
    /// for a type even if a direct match doesn't exist. This gets around versioning
    /// mismatches, and allows you to move data types between assemblies.
    /// </summary>
    /// <remarks>
    /// This class is in SystemLayer because there is code in this assembly that must
    /// make use of it. This class does not otherwise need to be here, and can be
    /// ignored by implementors.
    /// </remarks>
    public sealed class SerializationFallbackBinder
        : SerializationBinder
    {
        private List<Assembly> assemblies;

        public SerializationFallbackBinder()
        {
            this.assemblies = new List<Assembly>();
        }

        public void AddAssembly(Assembly assembly)
        {
            this.assemblies.Add(assembly);
        }

        private Type TryBindToType(Assembly assembly, string typeName)
        {
            Type type = assembly.GetType(typeName, false, true);
            return type;
        }

        public override Type BindToType(string assemblyName, string typeName)
        {
            Type type = null;

            foreach (Assembly tryAssembly in this.assemblies)
            {
                type = TryBindToType(tryAssembly, typeName);

                if (type != null)
                {
                    break;
                }
            }

            if (type == null)
            {
                string fullTypeName = typeName + ", " + assemblyName;

                try
                {
                    type = System.Type.GetType(fullTypeName, false, true);
                }

                catch (FileLoadException)
                {
                    type = null;
                }
            }

            return type;
        }
    }
}
