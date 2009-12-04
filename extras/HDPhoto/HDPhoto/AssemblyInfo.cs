/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) Rick Brewster, Tom Jackson, and past contributors.            //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("HDPhoto Plugin for Paint.NET")]
[assembly: AssemblyDescription("Image and photo editing software written in C#.")]
[assembly: AssemblyCompany("Paint.NET Team")]
[assembly: AssemblyProduct("Paint.NET")]
[assembly: AssemblyCopyright("Copyright © 2007 dotPDN LLC, Rick Brewster. All Rights Reserved.")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: AssemblyVersion("0.4.*")]
[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyFile("")]
[assembly: AssemblyKeyName("")]
[assembly: StringFreezing()]
[assembly: Dependency("PaintDotNet.Core", LoadHint.Always)]
[assembly: Dependency("PaintDotNet.Data", LoadHint.Always)]
[assembly: Dependency("PaintDotNet.SystemLayer", LoadHint.Always)]
[assembly: Dependency("System.Windows.Forms", LoadHint.Always)]
[assembly: Dependency("System.Drawing", LoadHint.Always)]
[assembly: Dependency("WindowsBase", LoadHint.Always)]
[assembly: Dependency("PresentationCore", LoadHint.Always)]
[assembly: ComVisibleAttribute(false)]
