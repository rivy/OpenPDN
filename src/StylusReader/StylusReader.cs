/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using Microsoft.Ink;
using Microsoft.StylusInput;
using Microsoft.StylusInput.PluginData;
using PaintDotNet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PaintDotNet
{
    public sealed class StylusReader 
    {
        private StylusReader()
        {
        }

        // If we don't keep the styluses, they get garbagecollected.
        private static Hashtable hookedControls = Hashtable.Synchronized(new Hashtable());

        public static void HookStylus(IStylusReaderHooks subject, Control control)
        {
            if (hookedControls.Contains(control))
            {
                throw new ApplicationException("control is already hooked");
            }

            RealTimeStylus stylus = new RealTimeStylus(control, true);
            PaintDotNet.StylusAsyncPlugin stylusReader = new PaintDotNet.StylusAsyncPlugin(subject, control);
            
            stylus.AsyncPluginCollection.Add(stylusReader);
            stylus.SetDesiredPacketDescription(new Guid[] { PacketProperty.X, 
                                                            PacketProperty.Y, 
                                                            PacketProperty.NormalPressure, 
                                                            PacketProperty.PacketStatus});
            stylus.Enabled = true;

            control.Disposed += new EventHandler(control_Disposed);

            WeakReference weakRef = new WeakReference(control);
            hookedControls.Add(weakRef, stylus);
        }

        public static void UnhookStylus(Control control)
        {
            lock (hookedControls.SyncRoot)
            {
                List<WeakReference> deleteUs = new List<WeakReference>();

                foreach (WeakReference weakRef in hookedControls.Keys)
                {
                    object target = weakRef.Target;

                    if (target == null)
                    {
                        deleteUs.Add(weakRef);
                    }
                    else
                    {
                        Control control2 = (Control)target;

                        if (object.ReferenceEquals(control, control2))
                        {
                            deleteUs.Add(weakRef);
                        }
                    }
                }

                foreach (WeakReference weakRef in deleteUs)
                {
                    RealTimeStylus stylus = (RealTimeStylus)hookedControls[weakRef];
                    stylus.Enabled = false;
                    stylus.AsyncPluginCollection.Clear();
                    hookedControls.Remove(weakRef);
                }
            }
        }

        private static void control_Disposed(object sender, EventArgs e)
        {
            Control asControl = (Control)sender;
            asControl.Disposed -= new EventHandler(control_Disposed);
            UnhookStylus(asControl);
        }
    }
}
