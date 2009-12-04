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
using System.Threading;
using System.Windows.Forms;

namespace PaintDotNet.SystemLayer
{
    /// <summary>
    /// Encapsulates information and events about the current user session.
    /// This relates to Terminal Services in Windows.
    /// </summary>
    public static class UserSessions
    {
        private static OurControl messageControl;
        private static bool lastRemoteSessionValue;
        private static EventHandler sessionChanged;
        private static int sessionChangedCount;
        private static object lockObject = new object();

        private sealed class OurControl
            : Control
        {
            public event EventHandler WmWtSessionChange;

            private void OnWmWtSessionChange()
            {
                if (WmWtSessionChange != null)
                {
                    WmWtSessionChange(this, EventArgs.Empty);
                }
            }

            protected override void WndProc(ref Message m)
            {
                switch (m.Msg)
                {
                    case NativeConstants.WM_WTSSESSION_CHANGE:
                        OnWmWtSessionChange();
                        break;

                    default:
                        base.WndProc(ref m);
                        break;
                }
            }
        }

        private static void OnSessionChanged()
        {
            if (sessionChanged != null)
            {
                sessionChanged(null, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Occurs when the user changes between sessions. This event will only be
        /// raised when the value returned by IsRemote() changes.
        /// </summary>
        /// <remarks>
        /// For example, if the user is currently logged in at the console, and then
        /// switches to a remote session (they use Remote Desktop from another computer),
        /// then this event will be raised.
        /// Note to implementors: This may be implemented as a no-op.
        /// </remarks>
        public static event EventHandler SessionChanged
        {
            add
            {
                lock (lockObject)
                {
                    sessionChanged += value;
                    ++sessionChangedCount;

                    if (sessionChangedCount == 1)
                    {
                        messageControl = new OurControl();
                        messageControl.CreateControl(); // force the HWND to be created
                        messageControl.WmWtSessionChange += new EventHandler(SessionStrobeHandler);

                        SafeNativeMethods.WTSRegisterSessionNotification(messageControl.Handle, NativeConstants.NOTIFY_FOR_ALL_SESSIONS);
                        lastRemoteSessionValue = IsRemote;
                    }
                }
            }

            remove
            {
                lock (lockObject)
                {
                    sessionChanged -= value;
                    int decremented = Interlocked.Decrement(ref sessionChangedCount);

                    if (decremented == 0)
                    {
                        try
                        {
                            SafeNativeMethods.WTSUnRegisterSessionNotification(messageControl.Handle);
                        }

                        catch (EntryPointNotFoundException)
                        {
                        }

                        messageControl.Dispose();
                        messageControl = null;
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether the user is running within a remoted session (Terminal Server, Remote Desktop).
        /// </summary>
        /// <returns>
        /// <b>true</b> if we're running in a remote session, <b>false</b> otherwise.
        /// </returns>
        /// <remarks>
        /// You can use this to optimize the presentation of visual elements. Remote sessions
        /// are often bandwidth limited and less suitable for complex drawing.
        /// Note to implementors: This may be implemented as a no op; in this case, always return false.
        /// </remarks>
        public static bool IsRemote
        {
            get
            {
                return 0 != SafeNativeMethods.GetSystemMetrics(NativeConstants.SM_REMOTESESSION);
            }
        }

        private static void SessionStrobeHandler(object sender, EventArgs e)
        {
            if (IsRemote != lastRemoteSessionValue)
            {
                lastRemoteSessionValue = IsRemote;
                OnSessionChanged();
            }
        }
    }
}
