/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace PaintDotNet.SystemLayer
{
    /// <summary>
    /// Provides static methods and properties related to the CPU.
    /// </summary>
    public static class Processor
    {
        private static int logicalCpuCount;
        private static string cpuName;

        static Processor()
        {
            logicalCpuCount = ConcreteLogicalCpuCount;
        }

        private static ProcessorArchitecture Convert(ushort wProcessorArchitecture)
        {
            ProcessorArchitecture platform;

            switch (wProcessorArchitecture)
            {
                case NativeConstants.PROCESSOR_ARCHITECTURE_AMD64:
                    platform = ProcessorArchitecture.X64;
                    break;

                case NativeConstants.PROCESSOR_ARCHITECTURE_INTEL:
                    platform = ProcessorArchitecture.X86;
                    break;

                default:
                case NativeConstants.PROCESSOR_ARCHITECTURE_UNKNOWN:
                    platform = ProcessorArchitecture.Unknown;
                    break;
            }

            return platform;
        }

        /// <summary>
        /// Returns the processor architecture that the current process is using.
        /// </summary>
        /// <remarks>
        /// Note that if the current process is 32-bit, but the OS is 64-bit, this
        /// property will still return X86 and not X64.
        /// </remarks>
        public static ProcessorArchitecture Architecture
        {
            get
            {
                NativeStructs.SYSTEM_INFO sysInfo = new NativeStructs.SYSTEM_INFO();
                NativeMethods.GetSystemInfo(ref sysInfo);
                ProcessorArchitecture architecture = Convert(sysInfo.wProcessorArchitecture);
                return architecture;
            }
        }

        /// <summary>
        /// Returns the processor architecture of the installed operating system.
        /// </summary>
        /// <remarks>
        /// Note that this may differ from the Architecture property if, for instance,
        /// this is a 32-bit process on a 64-bit OS.
        /// </remarks>
        public static ProcessorArchitecture NativeArchitecture
        {
            get
            {
                NativeStructs.SYSTEM_INFO sysInfo = new NativeStructs.SYSTEM_INFO();
                NativeMethods.GetNativeSystemInfo(ref sysInfo);
                ProcessorArchitecture architecture = Convert(sysInfo.wProcessorArchitecture);
                return architecture;
            }
        }

        private static string GetCpuName()
        {
            Guid processorClassGuid = new Guid("{50127DC3-0F36-415E-A6CC-4CB3BE910B65}");
            IntPtr hDiSet = IntPtr.Zero;
            string cpuName = null;

            try
            {
                hDiSet = NativeMethods.SetupDiGetClassDevsW(ref processorClassGuid, null, IntPtr.Zero, NativeConstants.DIGCF_PRESENT);

                if (hDiSet == NativeConstants.INVALID_HANDLE_VALUE)
                {
                    NativeMethods.ThrowOnWin32Error("SetupDiGetClassDevsW returned INVALID_HANDLE_VALUE");
                }

                bool bResult = false;
                uint memberIndex = 0;

                while (true)
                {
                    NativeStructs.SP_DEVINFO_DATA spDevinfoData = new NativeStructs.SP_DEVINFO_DATA();
                    spDevinfoData.cbSize = (uint)Marshal.SizeOf(typeof(NativeStructs.SP_DEVINFO_DATA));

                    bResult = NativeMethods.SetupDiEnumDeviceInfo(hDiSet, memberIndex, ref spDevinfoData);

                    if (!bResult)
                    {
                        int error = Marshal.GetLastWin32Error();

                        if (error == NativeConstants.ERROR_NO_MORE_ITEMS)
                        {
                            break;
                        }
                        else
                        {
                            throw new Win32Exception("SetupDiEnumDeviceInfo returned false, GetLastError() = " + error.ToString());
                        }
                    }

                    uint lengthReq = 0;
                    bResult = NativeMethods.SetupDiGetDeviceInstanceIdW(hDiSet, ref spDevinfoData, IntPtr.Zero, 0, out lengthReq);

                    if (bResult)
                    {
                        NativeMethods.ThrowOnWin32Error("SetupDiGetDeviceInstanceIdW(1) returned true");
                    }

                    if (lengthReq == 0)
                    {
                        NativeMethods.ThrowOnWin32Error("SetupDiGetDeviceInstanceIdW(1) returned false, but also 0 for lengthReq");
                    }

                    IntPtr str = IntPtr.Zero;
                    string regPath = null;

                    try
                    {
                        // Note: We cannot use Memory.Allocate() here because this property is
                        // usually retrieved during app shutdown, during which the heap may not
                        // be available.
                        str = Marshal.AllocHGlobal(checked((int)(sizeof(char) * (1 + lengthReq))));
                        bResult = NativeMethods.SetupDiGetDeviceInstanceIdW(hDiSet, ref spDevinfoData, str, lengthReq, out lengthReq);

                        if (!bResult)
                        {
                            NativeMethods.ThrowOnWin32Error("SetupDiGetDeviceInstanceIdW(2) returned false");
                        }

                        regPath = Marshal.PtrToStringUni(str);
                    }

                    finally
                    {
                        if (str != IntPtr.Zero)
                        {
                            Marshal.FreeHGlobal(str);
                            str = IntPtr.Zero;
                        }
                    }

                    string keyName = @"SYSTEM\CurrentControlSet\Enum\" + regPath;
                    using (RegistryKey procKey = Registry.LocalMachine.OpenSubKey(keyName, false))
                    {
                        const string friendlyName = "FriendlyName";

                        if (procKey != null)
                        {
                            object valueObj = procKey.GetValue(friendlyName);
                            string value = valueObj as string;

                            if (value != null)
                            {
                                cpuName = value;
                            }
                        }
                    }

                    if (cpuName != null)
                    {
                        break;
                    }

                    ++memberIndex;
                }
            }

            finally
            {
                if (hDiSet != IntPtr.Zero)
                {
                    NativeMethods.SetupDiDestroyDeviceInfoList(hDiSet);
                    hDiSet = IntPtr.Zero;
                }
            }

            return cpuName;
        }

        /// <summary>
        /// Returns the name of the CPU that is installed. If more than 1 CPU is installed,
        /// then the name of the first one is retrieved.
        /// </summary>
        /// <remarks>
        /// This is the name that shows up in Windows Device Manager in the "Processors" node.
        /// Note to implementors: This is only ever used for diagnostics (e.g., crash log).
        /// </remarks>
        public static string CpuName
        {
            get
            {
                if (cpuName == null)
                {
                    cpuName = GetCpuName();
                }

                return cpuName;
            }
        }

        /// <summary>
        /// Gets the number of logical or "virtual" processors installed in the computer.
        /// </summary>
        /// <remarks>
        /// This value may not return the actual number of processors installed in the system.
        /// It may be set to another number for testing and benchmarking purposes. It is
        /// recommended that you use this property instead of ConcreteLogicalCpuCount for the
        /// purposes of optimizing thread usage.
        /// The maximum value for this property is 32 when running as a 32-bit process, or
        /// 64 for a 64-bit process. Note that this implies the maximum is 32 for a 32-bit process
        /// even when running on a 64-bit system.
        /// </remarks>
        public static int LogicalCpuCount
        {
            get
            {
                return logicalCpuCount;
            }

            set
            {
                if (value < 1 || value > (IntPtr.Size * 8))
                {
                    throw new ArgumentOutOfRangeException("value", value, "must be in the range [0, " + (IntPtr.Size * 8).ToString() + "]");
                }

                logicalCpuCount = value;
            }
        }

        /// <summary>
        /// Gets the number of logical or "virtual" processors installed in the computer.
        /// </summary>
        /// <remarks>
        /// This property will always return the actual number of logical processors installed
        /// in the system. Note that processors such as Intel Xeons and Pentium 4's with
        /// HyperThreading will result in values that are twice the number of physical processor
        /// packages that have been installed (i.e. 2 Xeons w/ HT => ConcreteLogicalCpuCount = 4).
        /// </remarks>
        public static int ConcreteLogicalCpuCount
        {
            get
            {
                return Environment.ProcessorCount;
            }
        }

        /// <summary>
        /// Gets the approximate speed of the processor, in megahurtz.
        /// </summary>
        /// <remarks>
        /// No accuracy is guaranteed, and precision is dependent on the operating system.
        /// If there is an error determining the CPU speed, then 0 will be returned.
        /// </remarks>
        public static int ApproximateSpeedMhz
        {
            get
            {
                const string keyName = @"HARDWARE\DESCRIPTION\System\CentralProcessor\0";
                const string valueName = @"~MHz";
                int mhz = 0;

                try
                {
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyName, false))
                    {
                        if (key != null)
                        {
                            object value = key.GetValue(valueName);
                            mhz = (int)value;
                        }
                    }
                }

                catch (Exception)
                {
                    mhz = 0;
                }

                return mhz;
            }
        }

        private static ProcessorFeature features = (ProcessorFeature)0;

        public static ProcessorFeature Features
        {
            get
            {
                if (features == (ProcessorFeature)0)
                {
                    ProcessorFeature newFeatures = (ProcessorFeature)0;

                    // DEP
                    if (SafeNativeMethods.IsProcessorFeaturePresent(NativeConstants.PF_NX_ENABLED))
                    {
                        newFeatures |= ProcessorFeature.DEP;
                    }

                    // SSE
                    if (SafeNativeMethods.IsProcessorFeaturePresent(NativeConstants.PF_XMMI_INSTRUCTIONS_AVAILABLE))
                    {
                        newFeatures |= ProcessorFeature.SSE;
                    }

                    // SSE2
                    if (SafeNativeMethods.IsProcessorFeaturePresent(NativeConstants.PF_XMMI64_INSTRUCTIONS_AVAILABLE))
                    {
                        newFeatures |= ProcessorFeature.SSE2;
                    }

                    // SSE3
                    if (SafeNativeMethods.IsProcessorFeaturePresent(NativeConstants.PF_SSE3_INSTRUCTIONS_AVAILABLE))
                    {
                        newFeatures |= ProcessorFeature.SSE3;
                    }

                    features = newFeatures;
                }

                return features;
            }
        }

        public static bool IsFeaturePresent(ProcessorFeature feature)
        {
            return ((Features & feature) == feature);
        }
    }
}
