/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: CLSCompliant(false)]
[assembly: ComVisible(false)]
[assembly: AssemblyTitle("Paint.NET Resources")]
[assembly: AssemblyDescription("Image and photo editing software written in C#.")]
[assembly: AssemblyCompany("dotPDN LLC")]
[assembly: AssemblyProduct("Paint.NET")]
[assembly: AssemblyCopyright("Copyright © 2008 dotPDN LLC, Rick Brewster, Tom Jackson, and past contributors. Portions Copyright © Microsoft Corporation. All Rights Reserved.")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: AssemblyVersion("3.36.*")]

// Change this to say "Final" for final builds. Otherwise the titlebar will contain
// a long version string. Final versions should just say the ApplicationProduct
// attribute (i.e., "Paint.NET" instead of "Paint.NET (Beta 2 build: 1.0.*.*)"
// Use this to hold the current milestone title, such as "Milestone 2" or "Beta 3"
[assembly: AssemblyConfiguration("Personal")]

[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyFile("")]
[assembly: AssemblyKeyName("")]
[assembly: StringFreezing()]
[assembly: Dependency("System.Windows.Forms", LoadHint.Always)]
[assembly: Dependency("System.Drawing", LoadHint.Always)]
[assembly: DefaultDependency(LoadHint.Always)]
[assembly: NeutralResourcesLanguage("en-US", UltimateResourceFallbackLocation.Satellite)]
