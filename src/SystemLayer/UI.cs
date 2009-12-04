/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace PaintDotNet.SystemLayer
{
    /// <summary>
    /// Contains static methods related to the user interface.
    /// </summary>
    public static class UI
    {
        private static bool initScales = false;
        private static float xScale;
        private static float yScale;

        public static void FlashForm(Form form)
        {
            IntPtr hWnd = form.Handle;
            SafeNativeMethods.FlashWindow(hWnd, false);
            SafeNativeMethods.FlashWindow(hWnd, false);
            GC.KeepAlive(form);
        }

        /// <summary>
        /// In some circumstances, the window manager will draw the window larger than it reports
        /// its size to be. You can use this function to retrieve the size of this extra border
        /// padding.
        /// </summary>
        /// <param name="window"></param>
        /// <returns>
        /// An integer greater than or equal to zero that describes the size of the border padding
        /// which is not reported via the window's Size or Bounds property.
        /// </returns>
        /// <remarks>
        /// Note to implementors: This method may simply return 0. It is provided for use in Windows
        /// Vista when DWM+Aero is enabled in which case sizable FloatingToolForm windows do not
        /// visibly dock to the correct locations.
        /// </remarks>
        public static int GetExtendedFrameBounds(Form window)
        {
            int returnVal;

            if (OS.IsVistaOrLater)
            {
                unsafe
                {
                    int* rcVal = stackalloc int[4];

                    int hr = SafeNativeMethods.DwmGetWindowAttribute(
                        window.Handle,
                        NativeConstants.DWMWA_EXTENDED_FRAME_BOUNDS,
                        (void*)rcVal,
                        4 * (uint)sizeof(int));

                    if (hr >= 0)
                    {
                        returnVal = -rcVal[0];
                    }
                    else
                    {
                        returnVal = 0;
                    }
                }
            }
            else
            {
                returnVal = 0;
            }

            GC.KeepAlive(window);
            return Math.Max(0, returnVal);
        }

        private static void InitScaleFactors(Control c)
        {
            if (c == null)
            {
                xScale = 1.0f;
                yScale = 1.0f;
            }
            else
            {
                using (Graphics g = c.CreateGraphics())
                {
                    xScale = g.DpiX / 96.0f;
                    yScale = g.DpiY / 96.0f;
                }
            }

            initScales = true;
        }

        public static void InitScaling(Control c)
        {
            if (!initScales)
            {
                InitScaleFactors(c);
            }
        }

        public static float ScaleWidth(float width)
        {
            return (float)Math.Round(width * GetXScaleFactor());
        }

        public static int ScaleWidth(int width)
        {
            return (int)Math.Round((float)width * GetXScaleFactor());
        }

        public static int ScaleHeight(int height)
        {
            return (int)Math.Round((float)height * GetYScaleFactor());
        }

        public static float ScaleHeight(float height)
        {
            return (float)Math.Round(height * GetYScaleFactor());
        }

        public static Size ScaleSize(Size size)
        {
            return new Size(ScaleWidth(size.Width), ScaleHeight(size.Height));
        }

        public static Point ScalePoint(Point pt)
        {
            return new Point(ScaleWidth(pt.X), ScaleHeight(pt.Y));
        }

        public static float GetXScaleFactor()
        {
            if (!initScales)
            {
                throw new InvalidOperationException("Must call InitScaling() first");
            }

            return xScale;
        }

        public static float GetYScaleFactor()
        {
            if (!initScales)
            {
                throw new InvalidOperationException("Must call InitScaling() first");
            }

            return yScale;
        }

        public static void DrawCommandButton(
            Graphics g, 
            PushButtonState state, 
            Rectangle rect, 
            Color backColor,
            Control childControl)
        {
            VisualStyleElement element = null;
            int alpha = 255;
            
            if (OS.IsVistaOrLater)
            {
                const string className = "BUTTON";
                const int partID = NativeConstants.BP_COMMANDLINK;
                int stateID;

                switch (state)
                {
                    case PushButtonState.Default:
                        stateID = NativeConstants.CMDLS_DEFAULTED;
                        break;

                    case PushButtonState.Disabled:
                        stateID = NativeConstants.CMDLS_DISABLED;
                        break;
                        
                    case PushButtonState.Hot:
                        stateID = NativeConstants.CMDLS_HOT;
                        break;

                    case PushButtonState.Normal:
                        stateID = NativeConstants.CMDLS_NORMAL;
                        break;

                    case PushButtonState.Pressed:
                        stateID = NativeConstants.CMDLS_PRESSED;
                        break;

                    default:
                        throw new InvalidEnumArgumentException();
                }

                try
                {
                    element = VisualStyleElement.CreateElement(className, partID, stateID);

                    if (!VisualStyleRenderer.IsElementDefined(element))
                    {
                        element = null;
                    }
                }

                catch (InvalidOperationException)
                {
                    element = null;
                }
            }

            if (element == null)
            {
                switch (state)
                {
                    case PushButtonState.Default:
                        element = VisualStyleElement.Button.PushButton.Default;
                        alpha = 95;
                        break;

                    case PushButtonState.Disabled:
                        element = VisualStyleElement.Button.PushButton.Disabled;
                        break;

                    case PushButtonState.Hot:
                        element = VisualStyleElement.Button.PushButton.Hot;
                        break;

                    case PushButtonState.Normal:
                        alpha = 0;
                        element = VisualStyleElement.Button.PushButton.Normal;
                        break;
                    case PushButtonState.Pressed:
                        element = VisualStyleElement.Button.PushButton.Pressed;
                        break;

                    default:
                        throw new InvalidEnumArgumentException();
                }
            }

            if (element != null)
            {
                try
                {
                    VisualStyleRenderer renderer = new VisualStyleRenderer(element);
                    renderer.DrawParentBackground(g, rect, childControl);
                    renderer.DrawBackground(g, rect);
                }

                catch (Exception)
                {
                    element = null;
                }
            }

            if (element == null)
            {
                ButtonRenderer.DrawButton(g, rect, state);
            }

            if (alpha != 255)
            {
                using (Brush backBrush = new SolidBrush(Color.FromArgb(255 - alpha, backColor)))
                {
                    CompositingMode oldCM = g.CompositingMode;

                    try
                    {
                        g.CompositingMode = CompositingMode.SourceOver;
                        g.FillRectangle(backBrush, rect);
                    }

                    finally
                    {
                        g.CompositingMode = oldCM;
                    }
                }
            }
        }
       
        /// <summary>
        /// Sets the control's redraw state.
        /// </summary>
        /// <param name="control">The control whose state should be modified.</param>
        /// <param name="enabled">The new state for redrawing ability.</param>
        /// <remarks>
        /// Note to implementors: This method is used by SuspendControlPainting() and ResumeControlPainting().
        /// This may be implemented as a no-op.
        /// </remarks>
        private static void SetControlRedrawImpl(Control control, bool enabled)
        {
            SafeNativeMethods.SendMessageW(control.Handle, NativeConstants.WM_SETREDRAW, enabled ? new IntPtr(1) : IntPtr.Zero, IntPtr.Zero);
            GC.KeepAlive(control);
        }

        private static Dictionary<Control, int> controlRedrawStack = new Dictionary<Control, int>();

        /// <summary>
        /// Suspends the control's ability to draw itself.
        /// </summary>
        /// <param name="control">The control to suspend drawing for.</param>
        /// <remarks>
        /// When drawing is suspended, any painting performed in the control's WM_PAINT, OnPaint(),
        /// WM_ERASEBKND, or OnPaintBackground() handlers is completely ignored. Invalidation rectangles
        /// are not accumulated during this period, so when drawing is resumed (with 
        /// ResumeControlPainting()), it is usually a good idea to call Invalidate(true) on the control.
        /// This method must be matched at a later time by a corresponding call to ResumeControlPainting().
        /// If you call SuspendControlPainting() multiple times for the same control, then you must
        /// call ResumeControlPainting() once for each call.
        /// Note to implementors: Do not modify this method. Instead, modify SetControlRedrawImpl(),
        /// which may be implemented as a no-op.
        /// </remarks>
        public static void SuspendControlPainting(Control control)
        {
            int pushCount;

            if (controlRedrawStack.TryGetValue(control, out pushCount))
            {
                ++pushCount;
            }
            else
            {
                pushCount = 1;
            }

            if (pushCount == 1)
            {
                SetControlRedrawImpl(control, false);
            }

            controlRedrawStack[control] = pushCount;
        }

        /// <summary>
        /// Resumes the control's ability to draw itself.
        /// </summary>
        /// <param name="control">The control to suspend drawing for.</param>
        /// <remarks>
        /// This method must be matched by a preceding call to SuspendControlPainting(). If that method
        /// was called multiple times, then this method must be called a corresponding number of times
        /// in order to enable drawing.
        /// This method must be matched at a later time by a corresponding call to ResumeControlPainting().
        /// If you call SuspendControlPainting() multiple times for the same control, then you must
        /// call ResumeControlPainting() once for each call.
        /// Note to implementors: Do not modify this method. Instead, modify SetControlRedrawImpl(),
        /// which may be implemented as a no-op.
        /// </remarks>        
        public static void ResumeControlPainting(Control control)
        {
            int pushCount;

            if (controlRedrawStack.TryGetValue(control, out pushCount))
            {
                --pushCount;
            }
            else
            {
                throw new InvalidOperationException("There was no previous matching SuspendControlPainting() for this control");
            }

            if (pushCount == 0)
            {
                SetControlRedrawImpl(control, true);
                controlRedrawStack.Remove(control);
            }
            else
            {
                controlRedrawStack[control] = pushCount;
            }
        }

        /// <summary>
        /// Queries whether painting is enabled for the given control.
        /// </summary>
        /// <param name="control">The control to query suspension for.</param>
        /// <returns>
        /// false if the control's painting has been suspended via a call to SuspendControlPainting(),
        /// otherwise true.
        /// </returns>
        /// <remarks>
        /// You may use the return value of this method to optimize away painting. If this
        /// method returns false, then you may skip your entire OnPaint() method. This saves
        /// processor time by avoiding all of the non-painting drawing and resource initialization
        /// and destruction that is typically contained in OnPaint().
        /// This method assumes painting suspension is being exclusively managed with Suspend-
        /// and ResumeControlPainting().
        /// </remarks>
        public static bool IsControlPaintingEnabled(Control control)
        {
            int pushCount;

            if (!controlRedrawStack.TryGetValue(control, out pushCount))
            {
                pushCount = 0;
            }

            return (pushCount == 0);
        }

        private static IntPtr hRgn = SafeNativeMethods.CreateRectRgn(0, 0, 1, 1);

        /// <summary>
        /// This method retrieves the update region of a control.
        /// </summary>
        /// <param name="control">The control to retrieve the update region for.</param>
        /// <returns>
        /// An array of rectangles specifying the area that has been invalidated, or 
        /// null if this could not be determined.
        /// </returns>
        /// <remarks>
        /// This method is not thread safe.
        /// Note to implementors: This method may be implemented as a no-op. In this case, just return null.
        /// </remarks>
        public static Rectangle[] GetUpdateRegion(Control control)
        {
            SafeNativeMethods.GetUpdateRgn(control.Handle, hRgn, false);
            Rectangle[] scans;
            int area;
            PdnGraphics.GetRegionScans(hRgn, out scans, out area);
            GC.KeepAlive(control);
            return scans;
        }

        /// <summary>
        /// Sets a form's opacity.
        /// </summary>
        /// <param name="form"></param>
        /// <param name="opacity"></param>
        /// <remarks>
        /// Note to implementors: This may be implemented as just "form.Opacity = opacity".
        /// This method works around some visual clumsiness in .NET 2.0 related to
        /// transitioning between opacity == 1.0 and opacity != 1.0.</remarks>
        public static void SetFormOpacity(Form form, double opacity)
        {
            if (opacity < 0.0 || opacity > 1.0)
            {
                throw new ArgumentOutOfRangeException("opacity", "must be in the range [0, 1]");
            }

            uint exStyle = SafeNativeMethods.GetWindowLongW(form.Handle, NativeConstants.GWL_EXSTYLE);

            byte bOldAlpha = 255;

            if ((exStyle & NativeConstants.GWL_EXSTYLE) != 0)
            {
                uint dwOldKey;
                uint dwOldFlags;
                bool result = SafeNativeMethods.GetLayeredWindowAttributes(form.Handle, out dwOldKey, out bOldAlpha, out dwOldFlags);
            }

            byte bNewAlpha = (byte)(opacity * 255.0);
            uint newExStyle = exStyle;

            if (bNewAlpha != 255)
            {
                newExStyle |= NativeConstants.WS_EX_LAYERED;
            }

            if (newExStyle != exStyle || (newExStyle & NativeConstants.WS_EX_LAYERED) != 0)
            {
                if (newExStyle != exStyle)
                {
                    SafeNativeMethods.SetWindowLongW(form.Handle, NativeConstants.GWL_EXSTYLE, newExStyle);
                }

                if ((newExStyle & NativeConstants.WS_EX_LAYERED) != 0)
                {
                    SafeNativeMethods.SetLayeredWindowAttributes(form.Handle, 0, bNewAlpha, NativeConstants.LWA_ALPHA);
                }
            }

            GC.KeepAlive(form);
        }

        /// <summary>
        /// This WndProc implements click-through functionality. Some controls (MenuStrip, ToolStrip) will not
        /// recognize a click unless the form they are hosted in is active. So the first click will activate the
        /// form and then a second is required to actually make the click happen.
        /// </summary>
        /// <param name="m">The Message that was passed to your WndProc.</param>
        /// <returns>true if the message was processed, false if it was not</returns>
        /// <remarks>
        /// You should first call base.WndProc(), and then call this method. This method is only intended to
        /// change a return value, not to change actual processing before that.
        /// </remarks>
        internal static bool ClickThroughWndProc(ref Message m)
        {
            bool returnVal = false;

            if (m.Msg == NativeConstants.WM_MOUSEACTIVATE)
            {
                if (m.Result == (IntPtr)NativeConstants.MA_ACTIVATEANDEAT)
                {
                    m.Result = (IntPtr)NativeConstants.MA_ACTIVATE;
                    returnVal = true;
                }
            }

            return returnVal;
        }

        public static bool IsOurAppActive
        {
            get
            {
                foreach (Form form in Application.OpenForms)
                {
                    if (form == Form.ActiveForm)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        private static VisualStyleClass DetermineVisualStyleClass()
        {
            return Do.TryCatch(DetermineVisualStyleClassImpl, ex => VisualStyleClass.Other);
        }

        private static VisualStyleClass DetermineVisualStyleClassImpl()
        {
            VisualStyleClass vsClass;

            if (!VisualStyleInformation.IsSupportedByOS)
            {
                vsClass = VisualStyleClass.Classic;
            }
            else if (!VisualStyleInformation.IsEnabledByUser)
            {
                vsClass = VisualStyleClass.Classic;
            }
            else if (0 == string.Compare(VisualStyleInformation.Author, "MSX", StringComparison.InvariantCulture) &&
                     0 == string.Compare(VisualStyleInformation.DisplayName, "Aero style", StringComparison.InvariantCulture))
            {
                vsClass = VisualStyleClass.Aero;
            }
            else if (0 == string.Compare(VisualStyleInformation.Company, "Microsoft Corporation", StringComparison.InvariantCulture) &&
                     0 == string.Compare(VisualStyleInformation.Author, "Microsoft Design Team", StringComparison.InvariantCulture))
            {
                if (0 == string.Compare(VisualStyleInformation.DisplayName, "Windows XP style", StringComparison.InvariantCulture) ||  // Luna
                    0 == string.Compare(VisualStyleInformation.DisplayName, "Zune Style", StringComparison.InvariantCulture) ||        // Zune
                    0 == string.Compare(VisualStyleInformation.DisplayName, "Media Center style", StringComparison.InvariantCulture))  // Royale
                {
                    vsClass = VisualStyleClass.Luna;
                }
                else
                {
                    vsClass = VisualStyleClass.Other;
                }
            }
            else
            {
                vsClass = VisualStyleClass.Other;
            }

            return vsClass;
        }

        public static VisualStyleClass VisualStyleClass
        {
            get
            {
                return DetermineVisualStyleClass();
            }
        }

        public static void EnableShield(Button button, bool enableShield)
        {
            IntPtr hWnd = button.Handle;

            SafeNativeMethods.SendMessageW(
                hWnd,
                NativeConstants.BCM_SETSHIELD,
                IntPtr.Zero,
                enableShield ? new IntPtr(1) : IntPtr.Zero);

            GC.KeepAlive(button);
        }

        // TODO: get rid of this somehow! (this will happen when Layers window is rewritten, post-3.0)
        public static bool HideHorizontalScrollBar(Control c)
        {
            return SafeNativeMethods.ShowScrollBar(c.Handle, NativeConstants.SB_HORZ, false);
        }

        public static void RestoreWindow(IWin32Window window)
        {
            IntPtr hWnd = window.Handle;
            SafeNativeMethods.ShowWindow(hWnd, NativeConstants.SW_RESTORE);
            GC.KeepAlive(window);
        }

        public static void ShowComboBox(ComboBox comboBox, bool show)
        {
            IntPtr hWnd = comboBox.Handle;

            SafeNativeMethods.SendMessageW(
                hWnd,
                NativeConstants.CB_SHOWDROPDOWN,
                show ? new IntPtr(1) : IntPtr.Zero,
                IntPtr.Zero);

            GC.KeepAlive(comboBox);
        }

        /// <summary>
        /// Disables the system menu "Close" menu command, as well as the "X" close button on the window title bar.
        /// </summary>
        /// <remarks>
        /// Note to implementors: This method may *not* be implemented as a no-op. The purpose is to make it so that
        /// calling the Close() method is the only way to close a dialog, which is something that can only be done
        /// programmatically.
        /// </remarks>
        public static void DisableCloseBox(IWin32Window window)
        {
            IntPtr hWnd = window.Handle;
            IntPtr hMenu = SafeNativeMethods.GetSystemMenu(hWnd, false);

            if (hMenu == IntPtr.Zero)
            {
                NativeMethods.ThrowOnWin32Error("GetSystemMenu() returned NULL");
            }

            int result = SafeNativeMethods.EnableMenuItem(
                hMenu, 
                NativeConstants.SC_CLOSE, 
                NativeConstants.MF_BYCOMMAND | NativeConstants.MF_GRAYED);

            bool bResult = SafeNativeMethods.DrawMenuBar(hWnd);
            if (!bResult)
            {
                NativeMethods.ThrowOnWin32Error("DrawMenuBar returned FALSE");
            }

            GC.KeepAlive(window);
        }

        internal static void InvokeThroughModalTrampoline(IWin32Window owner, Procedure<IWin32Window> invokeMe)
        {
            using (Form modalityFix = new Form())
            {
                modalityFix.ShowInTaskbar = false;
                modalityFix.TransparencyKey = modalityFix.BackColor;
                UI.SetFormOpacity(modalityFix, 0);
                modalityFix.ControlBox = false;
                modalityFix.FormBorderStyle = FormBorderStyle.None;

                Control ownerAsControl = owner as Control;
                if (ownerAsControl != null)
                {
                    Form ownerForm = ownerAsControl.FindForm();

                    if (ownerForm != null)
                    {
                        Rectangle clientRect = ownerForm.RectangleToScreen(ownerForm.ClientRectangle);

                        modalityFix.Icon = ownerForm.Icon;
                        modalityFix.Location = clientRect.Location;
                        modalityFix.Size = clientRect.Size;
                        modalityFix.StartPosition = FormStartPosition.Manual;
                    }
                }

                modalityFix.Shown +=
                    delegate(object sender, EventArgs e)
                    {
                        invokeMe(modalityFix);
                        modalityFix.Close();
                    };

                modalityFix.ShowDialog(owner);
                GC.KeepAlive(modalityFix);
            }
        }
    }
}
