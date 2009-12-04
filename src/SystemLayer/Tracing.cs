/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.SystemLayer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace PaintDotNet.SystemLayer
{
    /// <summary>
    /// Methods for manual profiling and tracing.
    /// </summary>
    /// <remarks>
    /// This class does not rely on any system-specific functionality, but is placed
    /// in the SystemLayer assembly (as opposed to Core) so that classes here may 
    /// also used it.
    /// </remarks>
    public static class Tracing
    {
        private static void WriteLine(string msg)
        {
            Console.WriteLine(msg);
        }

        private static Set<string> features = new Set<string>();

        public static void LogFeature(string featureName)
        {
            if (string.IsNullOrEmpty(featureName))
            {
                return;
            }

            Ping("Logging feature: " + featureName);

            lock (features)
            {
                if (!features.Contains(featureName))
                {
                    features.Add(featureName);
                }
            }
        }

        public static IEnumerable<string> GetLoggedFeatures()
        {
            lock (features)
            {
                return features.ToArray();
            }
        }

#if DEBUG
        private static Timing timing = new Timing();
        private static Stack tracePoints = new Stack();

        private class TracePoint
        {
            private string message;
            private ulong timestamp;

            public string Message
            {
                get
                {
                    return message;
                }
            }

            public ulong Timestamp
            {
                get
                {
                    return timestamp;
                }
            }

            public TracePoint(string message, ulong timestamp)
            {
                this.message = message;
                this.timestamp = timestamp;
            }
        }
#endif

        [Conditional("DEBUG")]
        public static void Enter()
        {
#if DEBUG
            StackTrace trace = new StackTrace();
            StackFrame parentFrame = trace.GetFrame(1);
            MethodBase parentMethod = parentFrame.GetMethod();
            ulong now = timing.GetTickCount();
            string msg = new string(' ', 4 * tracePoints.Count) + parentMethod.DeclaringType.Name + "." + parentMethod.Name;
            WriteLine((now - timing.BirthTick).ToString() + ": " + msg);
            TracePoint tracePoint = new TracePoint(msg, now);
            tracePoints.Push(tracePoint);
#endif
        }

        [Conditional("DEBUG")]
        public static void Enter(string message)
        {
#if DEBUG
            StackTrace trace = new StackTrace();
            StackFrame parentFrame = trace.GetFrame(1);
            MethodBase parentMethod = parentFrame.GetMethod();
            ulong now = timing.GetTickCount();
            string msg = new string(' ', 4 * tracePoints.Count) + parentMethod.DeclaringType.Name + "." + 
                parentMethod.Name + ": " + message;
            WriteLine((now - timing.BirthTick).ToString() + ": " + msg);
            TracePoint tracePoint = new TracePoint(msg, now);
            tracePoints.Push(tracePoint);
#endif
        }

        [Conditional("DEBUG")]
        public static void Leave()
        {
#if DEBUG
            TracePoint tracePoint = (TracePoint)tracePoints.Pop();
            ulong now = timing.GetTickCount();
            WriteLine((now - timing.BirthTick).ToString() + ": " + tracePoint.Message + " (" + 
                (now - tracePoint.Timestamp).ToString() + "ms)");
#endif
        }

        [Conditional("DEBUG")]
        public static void Ping(string message, int callerCount)
        {
#if DEBUG
            StackTrace trace = new StackTrace();
            string callerString = "";
            for (int i = 0; i < Math.Min(trace.FrameCount - 1, callerCount); ++i)
            {
                StackFrame frame = trace.GetFrame(1 + i);
                MethodBase method = frame.GetMethod();
                callerString += method.DeclaringType.Name + "." + method.Name;
                if (i != callerCount - 1)
                {
                    callerString += " <- ";
                }
            }
            ulong now = timing.GetTickCount();
            WriteLine((now - timing.BirthTick).ToString() + ": " + new string(' ', 4 * tracePoints.Count) +
                callerString + (message != null ? (": " + message) : ""));
#endif
        }

        [Conditional("DEBUG")]
        public static void Ping(string message)
        {
#if DEBUG
            StackTrace trace = new StackTrace();
            StackFrame parentFrame = trace.GetFrame(1);
            MethodBase parentMethod = parentFrame.GetMethod();
            ulong now = timing.GetTickCount();
            WriteLine((now - timing.BirthTick).ToString() + ": " + new string(' ', 4 * tracePoints.Count) + 
                parentMethod.DeclaringType.Name + "." + parentMethod.Name + (message != null ? (": " + message) : ""));
#endif
        }

        [Conditional("DEBUG")]
        public static void Ping()
        {
#if DEBUG
            StackTrace trace = new StackTrace();
            StackFrame parentFrame = trace.GetFrame(1);
            MethodBase parentMethod = parentFrame.GetMethod();
            ulong now = timing.GetTickCount();
            WriteLine((now - timing.BirthTick).ToString() + ": " + new string(' ', 4 * tracePoints.Count) + 
                parentMethod.DeclaringType.Name + "." + parentMethod.Name);
#endif
        }

    }
}
