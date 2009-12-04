/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;

namespace PaintDotNet.Effects
{
    public sealed class EffectsCollection
    {
        private Assembly[] assemblies;
        private List<Type> effects;

        // Assembly, TypeName, Exception[]
        private List<Triple<Assembly, Type, Exception>> loaderExceptions =
            new List<Triple<Assembly, Type, Exception>>();

        private void AddLoaderException(Triple<Assembly, Type, Exception> loaderException)
        {
            lock (this)
            {
                this.loaderExceptions.Add(loaderException);
            }
        }

        public Triple<Assembly, Type, Exception>[] GetLoaderExceptions()
        {
            lock (this)
            {
                return this.loaderExceptions.ToArray();
            }
        }

        public EffectsCollection(List<Assembly> assemblies)
        {
            this.assemblies = assemblies.ToArray();
            this.effects = null;
        }

        public EffectsCollection(List<Type> effects)
        {
            this.assemblies = null;
            this.effects = new List<Type>(effects);
        }

        public Type[] Effects
        {
            get
            {
                lock (this)
                {
                    if (this.effects == null)
                    {
                        List<Triple<Assembly, Type, Exception>> errors = new List<Triple<Assembly, Type, Exception>>();
                        this.effects = GetEffectsFromAssemblies(this.assemblies, errors);

                        for (int i = 0; i < errors.Count; ++i)
                        {
                            AddLoaderException(errors[i]);
                        }
                    }
                }

                return this.effects.ToArray();
            }
        }

        private static Version GetAssemblyVersionFromType(Type type)
        {
            try
            {
                Assembly assembly = type.Assembly;
                AssemblyName assemblyName = new AssemblyName(assembly.FullName);
                return assemblyName.Version;
            }

            catch (Exception)
            {
                return new Version(0, 0, 0, 0);
            }
        }

        private static bool CheckForGuidOnType(Type type, Guid guid)
        {
            try
            {
                object[] attributes = type.GetCustomAttributes(typeof(GuidAttribute), true);

                foreach (GuidAttribute guidAttr in attributes)
                {
                    if (new Guid(guidAttr.Value) == guid)
                    {
                        return true;
                    }
                }
            }

            catch (Exception)
            {
            }

            return false;
        }

        private static bool CheckForAnyGuidOnType(Type type, Guid[] guids)
        {
            foreach (Guid guid in guids)
            {
                if (CheckForGuidOnType(type, guid))
                {
                    return true;
                }
            }

            return false;
        }

        private static readonly Guid[] deprecatedEffectGuids =
            new Guid[]
                {
                    new Guid("9A1EB3D9-0A36-4d32-9BB2-707D6E5A9D2C"), // TwistEffect from old DistortionEffects.dll
                    new Guid("3154E367-6B4D-4960-B4D8-F6D06E1C9C24"), // TileEffect from old DistortionEffects.dll
                    new Guid("1445F876-356D-4a7c-B726-50457F6E7AEF"), // PolarInversionEffect or BulgeEffect from old DistortionEffects.dll (accidentally put same guid on both)
                    new Guid("270DCBF1-CE42-411e-9885-162E2BFA8265"), // GlowEffect from old GlowEffect.dll
                };

        private static string UnstableMessage
        {
            get
            {
                return PdnResources.GetString("BlockedPluginException.UnstablePlugin");
            }
        }

        private static string BuiltInMessage
        {
            get
            {
                return PdnResources.GetString("BlockedPluginException.PluginIsNowBuiltIn");
            }
        }

        // effectNamespace, effectName, maxVersion, reason
        private static readonly Quadruple<string, string, Version, string>[] blockedEffects =
            new Quadruple<string, string, Version, string>[]
                {
                    // pyrochild's Film effect, v1.0.0.0, does some seriously weird stuff that hard crashes us no matter what
                    Quadruple.Create("FilmEffect", "FilmEffect", new Version(1, 0, int.MaxValue, int.MaxValue), UnstableMessage),

                    // Ed Harvey's Threshold effect, v1.0, uses a timer in its config dialog that NullRef's on us
                    Quadruple.Create("EdHarvey.Edfects.Effects", "ThresholdEffect", new Version (1, 0, int.MaxValue, int.MaxValue), UnstableMessage),

                    // BoltBait's InkSketch, which is now built-in to Paint.NET
                    Quadruple.Create("InkSketch", "EffectPlugin", new Version(1, 0, int.MaxValue, int.MaxValue), BuiltInMessage),

                    // Portrait, which is now built-in to Paint.NET ("Soften Portrait")
                    Quadruple.Create("PortraitEffect", "EffectPlugin", new Version(1, 0, int.MaxValue, int.MaxValue), BuiltInMessage),

                    // Reduce Noice, which is now built-in to Paint.NET
                    Quadruple.Create("HistogramEffects", "ReduceNoiseEffect", new Version (1, 1, 0, 0), BuiltInMessage),

                    // Fragment, which is now built-in
                    Quadruple.Create("EdHarvey.Edfects.Effects", "FragmentEffect", new Version(3, 20, int.MaxValue, int.MaxValue), BuiltInMessage),

                    // Posterize, which is now built-in
                    Quadruple.Create("EdHarvey.Edfects.Effects", "PosterizeEffect", new Version(3, 31, int.MaxValue, int.MaxValue), BuiltInMessage),

                    // Some of pyrochild's effects were using some code in PaintDotNet.exe that is now marked internal. 
                    // For some reason it is not caught by the plugin crash dialog in non-Debug builds.
                    Quadruple.Create("pyrochild.effects.splatter", "Splatter", new Version(1, 0, int.MaxValue, int.MaxValue), UnstableMessage),
                    Quadruple.Create("zachwalker.CurvesPlus", "CurvesPlus", new Version(2, 4, int.MaxValue, int.MaxValue), UnstableMessage),

                    // Older versions of these were accessing code in paintdotnet.exe, or were using the EventHandler<T> that was moved
                    Quadruple.Create("pyrochild.effects.gradientmapping", "GradientMapping", new Version(2, 1, int.MaxValue, int.MaxValue), UnstableMessage),

                    // GREYCstoration, has known stability issues, and is also superceded by the built-in "Reduce Noise" anyway
                    Quadruple.Create("GREYCstoration", "BaseEffect", new Version(0, 9, int.MaxValue, int.MaxValue), UnstableMessage),
                    Quadruple.Create("GREYCstoration", "RestoreEffect", new Version(0, 9, int.MaxValue, int.MaxValue), UnstableMessage)
                };

        private static Dictionary<string, List<Triple<string, Version, string>>> indexedBlockedEffects;

        private static Exception IsBannedEffect(Type effectType)
        {
            // Never block a built-in effect.
            if (effectType.Assembly == typeof(Effect).Assembly)
            {
                return null;
            }

            // Make an index of that big Quadruple[] up there. Otherwise we may never scale well because
            // we'll end up with the performance proportional to installed plugins * blocked plugins.
            if (indexedBlockedEffects == null)
            {
                lock (blockedEffects)
                {
                    indexedBlockedEffects = new Dictionary<string, List<Triple<string, Version, string>>>(StringComparer.InvariantCultureIgnoreCase);

                    foreach (var blockedEffect in blockedEffects)
                    {
                        string blockedEffectNamespace = blockedEffect.First;
                        var blockedEffectDetails = blockedEffect.GetTriple234();

                        List<Triple<string, Version, string>> blockedEffectDetailsSet;
                        if (!indexedBlockedEffects.TryGetValue(blockedEffectNamespace, out blockedEffectDetailsSet))
                        {
                            blockedEffectDetailsSet = new List<Triple<string, Version, string>>();
                            indexedBlockedEffects.Add(blockedEffectNamespace, blockedEffectDetailsSet);
                        }

                        blockedEffectDetailsSet.Add(blockedEffectDetails);
                    }
                }
            }

            // Gather the effect's details.
            Version assemblyVersion = GetAssemblyVersionFromType(effectType);
            string effectNamespace = effectType.Namespace;
            string effectName = effectType.Name;

#if DEBUG
            /*
            // Makes it easy to copy+paste into the big Quadruple[] up there
            SystemLayer.Tracing.Ping(string.Format(
                "IsBannedEffect -- Quadruple.Create(\"{0}\", \"{1}\", new Version({2}, {3}, {4}, {5}), ...)",
                effectNamespace,
                effectName,
                assemblyVersion.Major,
                assemblyVersion.Minor,
                assemblyVersion.Build,
                assemblyVersion.Revision));
             * */
#endif

            // Block list #1
            List<Triple<string, Version, string>> blockedEffectDetailsList2;
            if (indexedBlockedEffects.TryGetValue(effectNamespace, out blockedEffectDetailsList2))
            {
                foreach (var blockedEffectDetails2 in blockedEffectDetailsList2)
                {
                    if (0 == string.Compare(effectName, blockedEffectDetails2.First, StringComparison.InvariantCultureIgnoreCase) &&
                        assemblyVersion <= blockedEffectDetails2.Second)
                    {
                        return new BlockedPluginException(blockedEffectDetails2.Third);
                    }
                }
            }

            // Block list #2 -- Block based on Guid and namespace
            if (CheckForAnyGuidOnType(effectType, deprecatedEffectGuids) &&
                (0 == string.Compare(effectNamespace, "GlowEffect", StringComparison.InvariantCultureIgnoreCase) ||
                 0 == string.Compare(effectNamespace, "DistortionEffects", StringComparison.InvariantCultureIgnoreCase)))
            {
                return new BlockedPluginException();
            }

            return null;
        }

        private static List<Type> GetEffectsFromAssemblies(Assembly[] assemblies, IList<Triple<Assembly, Type, Exception>> errorsResult)
        {
            List<Type> effects = new List<Type>();

            foreach (Assembly assembly in assemblies)
            {
                GetEffectsFromAssembly(assembly, effects, errorsResult);
            }

            List<Type> removeUs = new List<Type>();

            foreach (Type effectType in effects)
            {
                Exception bannedEx = IsBannedEffect(effectType);

                if (bannedEx != null)
                {
                    removeUs.Add(effectType);
                    errorsResult.Add(Triple.Create(effectType.Assembly, effectType, bannedEx));
                }
            }

            foreach (Type removeThisType in removeUs)
            {
                effects.Remove(removeThisType);
            }

            return effects;
        }

        private static void GetEffectsFromAssembly(Assembly assembly, IList<Type> effectsResult, IList<Triple<Assembly, Type, Exception>> errorsResult)
        {
            try
            {
                Type[] types = GetTypesFromAssembly(assembly, errorsResult);

                foreach (Type type in types)
                {
                    if (type.IsSubclassOf(typeof(Effect)) && !type.IsAbstract && !Utility.IsObsolete(type, false))
                    {
                        effectsResult.Add(type);
                    }
                }
            }

            catch (ReflectionTypeLoadException)
            {
            }
        }

        private static Type[] GetTypesFromAssembly(Assembly assembly, IList<Triple<Assembly, Type, Exception>> errorsResult)
        {
            Type[] types;

            try
            {
                types = assembly.GetTypes();
            }

            catch (ReflectionTypeLoadException rex)
            {
                List<Type> typesList = new List<Type>();
                Type[] rexTypes = rex.Types;

                foreach (Type rexType in rexTypes)
                {
                    if (rexType != null)
                    {
                        typesList.Add(rexType);
                    }
                }

                foreach (Exception loadEx in rex.LoaderExceptions)
                {
                    // Set Type to null, and the error dialog will look at the exception, see it is a TypeLoadException,
                    // and use the TypeName property from there.
                    errorsResult.Add(Triple.Create<Assembly, Type, Exception>(assembly, null, loadEx));
                }

                types = typesList.ToArray();
            }

            return types;
        }
    }
}
