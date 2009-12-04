/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

//#define ENABLE_INK_IN_DEBUG_BUILDS

//#if WIN64
//#define NOINK
//#endif

using System;
using System.Collections;
using System.Reflection;
using System.Windows.Forms;
using PaintDotNet;

namespace PaintDotNet.SystemLayer
{
    public static class Ink
    {
        private static bool isInkAvailable = false;
        private static bool isInkAvailableInit = false;

        /// <summary>
        /// Adapts an IInkHook instance to work with the IStylusReaderHooks interface.
        /// </summary>
        private sealed class HookAdapter
            : IStylusReaderHooks
        {
            private IInkHooks subject;

            public HookAdapter(IInkHooks subject)
            {
                this.subject = subject;
            }

            public System.Drawing.Graphics CreateGraphics()
            {
                return subject.CreateGraphics();
            }

            public void PerformDocumentMouseMove(System.Windows.Forms.MouseButtons button, int clicks, float x, float y, int delta, float pressure)
            {
                subject.PerformDocumentMouseMove(button, clicks, x, y, delta, pressure);
            }

            public System.Drawing.PointF ScreenToDocument(System.Drawing.PointF pointF)
            {
                return subject.ScreenToDocument(pointF);
            }

            public void PerformDocumentMouseUp(System.Windows.Forms.MouseButtons button, int clicks, float x, float y, int delta, float pressure)
            {
                subject.PerformDocumentMouseUp(button, clicks, x, y, delta, pressure);
            }

            public void PerformDocumentMouseDown(System.Windows.Forms.MouseButtons button, int clicks, float x, float y, int delta, float pressure)
            {
                subject.PerformDocumentMouseDown(button, clicks, x, y, delta, pressure);
            }
        }

        /// <summary>
        /// Gets a value indicating whether Ink is available.
        /// </summary>
        /// <returns>true if Ink is available, or false if it is not</returns>
        /// <remarks>
        /// If ink is not available, then the other static methods or properties of this class will not
        /// be usable and will throw NotSupportedException.
        /// </remarks>
        public static bool IsAvailable()
        {
            if (!isInkAvailableInit)
            {
                // For debug builds we try to load the assembly. This enables us to work with ink
                // if we have the Tablet PC SDK installed. 
                // For retail builds we only enable ink on true blue Tablet PC's. Calling GetSystemMetrics
                // is much faster than attempting to load an assembly.
#if NOINK
                isInkAvailable = false;
#elif DEBUG
    #if ENABLE_INK_IN_DEBUG_BUILDS
                try
                {
                    Assembly inkAssembly = Assembly.Load("Microsoft.Ink, Version=1.7.2600.2180, Culture=\"\", PublicKeyToken=31bf3856ad364e35");
                    isInkAvailable = true;
                }

                catch (Exception)
                {
                    isInkAvailable = false;
                }
    #else
                isInkAvailable = false;
    #endif
#else
                if (SafeNativeMethods.GetSystemMetrics(NativeConstants.SM_TABLETPC) != 0)
                {
                    // Only enable ink if the system states it is a Tablet PC.
                    // In other words, don't incur the performance penalty and a few other
                    // weird things for regular PC's that just happen to have the SDK
                    // installed, or that have something like Vista Ultimate.
                    try
                    {
                        Assembly inkAssembly = Assembly.Load("Microsoft.Ink, Version=1.7.2600.2180, Culture=\"\", PublicKeyToken=31bf3856ad364e35");
                        isInkAvailable = true;
                    }

                    catch (Exception)
                    {
                        isInkAvailable = false;
                    }
                }
                else
                {
                    isInkAvailable = false;
                }
#endif

                isInkAvailableInit = true;
            }

            return isInkAvailable;
        }

        private static void HookInkImpl(IInkHooks subject, Control control)
        {
            HookAdapter adapter = new HookAdapter(subject);
            control.CreateControl();
            StylusReader.HookStylus(adapter, control);
        }

        /// <summary>
        /// Hooks Ink support in to a control.
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="control"></param>
        /// <exception cref="NotSupportedException">IsAvailable() returned false</exception>
        /// <remarks>Ink support will be automatically unhooked when the control's Disposed event is raised.</remarks>
        public static void HookInk(IInkHooks subject, Control control)
        {
            if (!IsAvailable())
            {
                throw new NotSupportedException("Ink is not available");
            }
            
            HookInkImpl(subject, control);
        }

        private static void UnhookInkImpl(Control control)
        {
            StylusReader.UnhookStylus(control);
        }

        /// <summary>
        /// Unhooks Ink support from a control.
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="control"></param>
        /// <exception cref="NotSupportedException">IsAvailable() returned false</exception>
        public static void UnhookInk(Control control)
        {
            if (!IsAvailable())
            {
                throw new NotSupportedException("Ink is not available");
            }

            UnhookInkImpl(control);
        }
    }
}
