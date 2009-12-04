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

namespace PaintDotNet
{
    /// <summary>
    /// Symbolic constants for our settings. Settings are stored in CurrentUser unless
    /// otherwise specified.
    /// </summary>
    internal sealed class SettingNames
    {
        /// <summary>
        /// The width of the main window (MainForm).
        /// </summary>
        /// <remarks>
        /// Written on app close, and read on app startup.
        /// </remarks>
        public const string Width = "Width";

        /// <summary>
        /// The height of the main window (MainForm).
        /// </summary>
        /// <remarks>
        /// Written on app close, and read on app startup.
        /// </remarks>
        public const string Height = "Height";

        /// <summary>
        /// The y-coordinate of the top edge of the main window (MainForm).
        /// </summary>
        /// <remarks>
        /// Written on app close, and read on app startup.
        /// </remarks>
        public const string Top = "Top";

        /// <summary>
        /// The x-coordinate of the left edge of the main window (MainForm).
        /// </summary>
        /// <remarks>
        /// Written on app close, and read on app startup.
        /// </remarks>
        public const string Left = "Left";

        /// <summary>
        /// The maximum number of items to store in the MRU list (File -> Open Recent).
        /// </summary>
        /// <remarks>
        /// Read or written whenever the File -> Open Recent menu is opened.
        /// </remarks>
        public const string MruMax = "MRUMax";

        /// <summary>
        /// The window state of the main window (MainForm). This can be either Maximized or Normal, 
        /// and corresponds to the FormWindowState enumeration.
        /// </summary>
        /// <remarks>
        /// Written on app close, and read on app startup.
        /// </remarks>
        public const string WindowState = "WindowState";

        /// <summary>
        /// The state of whether rulers are enabled in the DocumentWorkspace.
        /// </summary>
        /// <remarks>
        /// Written to whenever the value is changed, and read on app startup.
        /// </remarks>
        public const string Rulers = "Rulers";

        /// <summary>
        /// The unit of measurement the user has selected via the WorkspaceOptionsConfigWidget.
        /// </summary>
        /// <remarks>
        /// Written to whenever the value is changed, and read on app startup.
        /// </remarks>
        public const string Units = "Units";

        /// <summary>
        /// The type of font smoothing the user has chosen in the TextConfigStrip.
        /// </summary>
        /// <remarks>
        /// Written to whenever the value is changed, and read on app startup.
        /// </remarks>
        public const string FontSmoothing = "FontSmoothing";

        /// <summary>
        /// The last unit of measurement the user selected via the WorkspaceOptionsConfigWidget
        /// that was NOT pixels.
        /// </summary>
        /// <remarks>
        /// Written whenever the user changes the setting, and read on app startup.
        /// </remarks>
        public const string LastNonPixelUnits = "LastNonPixelUnits";

        public static MeasurementUnit GetLastNonPixelUnits()
        {
            string stringValue = Settings.CurrentUser.GetString(LastNonPixelUnits, MeasurementUnit.Inch.ToString());
            MeasurementUnit units;

            try
            {
                units = (MeasurementUnit)Enum.Parse(typeof(MeasurementUnit), stringValue, true);
            }

            catch
            {
                units = MeasurementUnit.Inch;
            }

            return units;
        }

        /// <summary>
        /// The state of whether the grid is enabled in the DocumentWorkspace.
        /// </summary>
        /// <remarks>
        /// Written to whenever the value is changed, and read on app startup.
        /// </remarks>
        public const string DrawGrid = "DrawGrid";

        /// <summary>
        /// The state of whether translucent windows are enabled (Window -> Translucent).
        /// </summary>
        /// <remarks>
        /// This setting is read whenever the Window menu is opened, and written to
        /// whenever the user changes the setting.
        /// </remarks>
        public const string TranslucentWindows = "TranslucentWindows";

        /// <summary>
        /// The state of whether the Tools floating form is visible.
        /// </summary>
        /// <remarks>
        /// Written on app close, and read on app startup.
        /// </remarks>
        public const string ToolsFormVisible = "ToolsForm.Visible";

        /// <summary>
        /// The state of whether the Colors floating form is visible.
        /// </summary>
        /// <remarks>
        /// Written on app close, and read on app startup.
        /// </remarks>
        public const string ColorsFormVisible = "ColorsForm.Visible";

        /// <summary>
        /// The state of whether the History floating form is visible.
        /// </summary>
        /// <remarks>
        /// Written on app close, and read on app startup.
        /// </remarks>
        public const string HistoryFormVisible = "HistoryForm.Visible";

        /// <summary>
        /// The state of whether the Layers floating form is visible.
        /// </summary>
        /// <remarks>
        /// Written on app close, and read on app startup.
        /// </remarks>
        public const string LayersFormVisible = "LayersForm.Visible";

        /// <summary>
        /// The last resampling algorithm that was selected in the Resize dialog.
        /// </summary>
        /// <remarks>
        /// Read from whenever a Resize dialog is shown to the user, and written
        /// to whenever the user closes the Resize dialog without cancelling it.
        /// </remarks>
        public const string LastResamplingMethod = "LastResamplingMethod";

        /// <summary>
        /// The last state of the "Maintain Aspect" check box in the Resize dialog.
        /// </summary>
        /// <remarks>
        /// Read from whenever a dialog is shown to the user and written to whenever the 
        /// user closes the dialog without cancelling it.
        /// </remarks>
        public const string LastMaintainAspectRatio = "LastMaintainAspectRatio";

        /// <summary>
        /// The last state of the "Maintain Aspect" check box in the Canvas Size dialog.
        /// </summary>
        /// <remarks>
        /// Read from whenever the dialog is shown to the user and written to whenever the 
        /// user closes the dialog without cancelling it.
        /// </remarks>
        public const string LastMaintainAspectRatioCS = "LastMaintainAspectRatioCS";

        /// <summary>
        /// The last state of the anchor edge in the Canvas Size dialog.
        /// </summary>
        /// <remarks>
        /// Read from whenever the dialog is shown to the user and written to whenever the
        /// user closes the dialog without cancelling it.
        /// </remarks>
        public const string LastCanvasSizeAnchorEdge = "LastCanvasSizeAnchorEdge";

        public static AnchorEdge GetLastCanvasSizeAnchorEdge()
        {
            string stringValue = Settings.CurrentUser.GetString(LastCanvasSizeAnchorEdge, AnchorEdge.TopLeft.ToString());
            AnchorEdge edge;

            try
            {
                edge = (AnchorEdge)Enum.Parse(typeof(AnchorEdge), stringValue, true);
            }

            catch
            {
                edge = AnchorEdge.TopLeft;
            }

            return edge;
        }

        /// <summary>
        /// The last state of the "Maintain Aspect" check box in the New File dialog.
        /// </summary>
        /// <remarks>
        /// Read from whenever the dialog is shown to the user and written to whenever the 
        /// user closes the dialog without cancelling it.
        /// </remarks>
        public const string LastMaintainAspectRatioNF = "LastMaintainAspectRatioNF";

        /// <summary>
        /// The last directory that was visible in a Open, Save, or Import dialog that 
        /// the user did not cancel.
        /// </summary>
        /// <remarks>
        /// Read from whenever an Open, Save, or Import dialog is shown to the user.
        /// Written to whenever one of those dialogs is closed without cancelling it.
        /// </remarks>
        public const string LastFileDialogDirectory = "LastFileDialogDirectory";

        /// <summary>
        /// The state of whether Paint.NET should automatically check for updates once per week.
        /// </summary>
        public const string AutoCheckForUpdates = "CHECKFORUPDATES";

        /// <summary>
        /// Whether or not Paint.NET should inform the user of pre-release versions (Betas) of Paint.NET.
        /// </summary>
        /// <remarks>
        /// This is a SystemWide setting and may not be changed by non-admins.
        /// </remarks>
        public const string AlsoCheckForBetas = "CHECKFORBETAS";

        /// <summary>
        /// The last time that we checked for updates. This is a 64-bit value that matches
        /// the Ticks property of the DateTime structure.
        /// </summary>
        /// <remarks>
        /// This is a CurrentUser setting.
        /// </remarks>
        public const string LastUpdateCheckTimeTicks = "LastUpdateCheckTimeTicks";

        /// <summary>
        /// After installation of the MSI, we set this registry key to keep track of where the
        /// file was stored. This string must be a file name (not path information), and always
        /// refers to a file in the %TEMP% directory. The filename must end with a .exe or .msi
        /// extension.
        /// </summary>
        /// <remarks>
        /// This is a CurrentUser setting.
        /// </remarks>
        public const string UpdateMsiFileName = "UpdateMsiFileName";

        /// <summary>
        /// Determines the resources file that is used to load strings from. Defaults to the
        /// system locale.
        /// </summary>
        public const string LanguageName = "LanguageName";

        /// <summary>
        /// Written to the registry to advertise what directory Paint.NET is installed to.
        /// </summary>
        /// <remarks>
        /// This is a SystemWide setting and may not be changed by non-admins.
        /// </remarks>
        public const string InstallDirectory = "TARGETDIR";

        /// <summary>
        /// The current palette is saved here, in the same format used for the palette files.
        /// </summary>
        /// <remarks>
        /// If this setting is missing or invalid, the default palette will be used.
        /// Read from at app startup, and written to whenever the current palette is changed.
        /// </remarks>
        public const string CurrentPalette = "CurrentPalette";

        /// <summary>
        /// The Type name of the default (startup) tool.
        /// </summary>
        /// <remarks>
        /// Read from at app startup, and written to whenever the default is saved
        /// from the ChooseToolDefaultsDialog form.
        /// </remarks>
        public const string DefaultToolTypeName = "DefaultToolTypeName";

        /// <summary>
        /// A serialized AppEnvironment that is loaded into the toolbar at startup.
        /// </summary>
        /// <remarks>
        /// Read from at app startup, and written to whenever the default is saved
        /// from the ChooseToolDefaultsDialog form.
        /// </remarks>
        public const string DefaultAppEnvironment = "DefaultAppEnvironment";

        private SettingNames()
        {
        }
    }
}
