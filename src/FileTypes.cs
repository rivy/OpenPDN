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
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// Provides static method and properties for obtaining all the FileType objects
    /// responsible for loading and saving Document instances. Loads FileType plugins
    /// too.
    /// </summary>
    internal sealed class FileTypes
    {
        private FileTypes()
        {
        }

        private static FileTypeCollection collection;
       
        public static FileTypeCollection GetFileTypes()
        {
            if (collection == null)
            {
                collection = LoadFileTypes();
            }

            return collection;
        }

        private static bool IsInterfaceImplemented(Type derivedType, Type interfaceType)
        {
            return -1 != Array.IndexOf<Type>(derivedType.GetInterfaces(), interfaceType); 
        }

        private static Type[] GetFileTypeFactoriesFromAssembly(Assembly assembly)
        {
            List<Type> fileTypeFactories = new List<Type>();

            foreach (Type type in assembly.GetTypes())
            {
                if (IsInterfaceImplemented(type, typeof(IFileTypeFactory)) && !type.IsAbstract)
                {
                    fileTypeFactories.Add(type);
                }
            }

            return fileTypeFactories.ToArray();
        }

        private static Type[] GetFileTypeFactoriesFromAssemblies(ICollection assemblies)
        {
            List<Type> allFactories = new List<Type>();

            foreach (Assembly assembly in assemblies)
            {
                Type[] factories;

                try
                {
                    factories = GetFileTypeFactoriesFromAssembly(assembly);
                }

                catch (Exception)
                {
                    continue;
                }

                foreach (Type type in factories)
                {
                    allFactories.Add(type);
                }
            }

            return allFactories.ToArray();
        }

        private static FileTypeCollection LoadFileTypes()
        {
            List<Assembly> assemblies = new List<Assembly>();

            // add the built-in IFileTypeFactory house
            assemblies.Add(typeof(FileType).Assembly);

            // enumerate the assemblies inside the FileTypes directory
            string homeDir = Path.GetDirectoryName(Application.ExecutablePath);
            string fileTypesDir = Path.Combine(homeDir, "FileTypes");
            bool dirExists;

            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(fileTypesDir);
                dirExists = dirInfo.Exists;
            }

            catch (Exception)
            {
                dirExists = false;
            }

            if (dirExists)
            {
                foreach (string fileName in Directory.GetFiles(fileTypesDir, "*.dll"))
                {
                    bool success;
                    Assembly pluginAssembly = null;

                    try
                    {
                        pluginAssembly = Assembly.LoadFrom(fileName);
                        success = true;
                    }

                    catch (Exception)
                    {
                        success = false;
                    }

                    if (success)
                    {
                        assemblies.Add(pluginAssembly);
                    }
                }
            }

            // Get all the IFileTypeFactory implementations
            Type[] fileTypeFactories = GetFileTypeFactoriesFromAssemblies(assemblies);
            List<FileType> allFileTypes = new List<FileType>(10);
            
            foreach (Type type in fileTypeFactories)
            {
                ConstructorInfo ci = type.GetConstructor(System.Type.EmptyTypes);
                IFileTypeFactory factory;

                try
                {
                    factory = (IFileTypeFactory)ci.Invoke(null);
                }

                catch (Exception)
                {
#if DEBUG
                    throw;
#else                    
                    continue;
#endif
                }

                FileType[] fileTypes;

                try
                {
                    fileTypes = factory.GetFileTypeInstances();
                }

                catch (Exception)
                {
#if DEBUG
                    throw;
#else
                    continue;
#endif
                }

                if (fileTypes != null)
                {
                    foreach (FileType fileType in fileTypes)
                    {
                        allFileTypes.Add(fileType);
                    }
                }
            }

            return new FileTypeCollection(allFileTypes);
        }
    }
}
