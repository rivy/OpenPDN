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
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace PaintDotNet.Updates
{
    internal class StartupState
        : UpdatesState
    {
        // Beta and alpha builds should check every day
        // Final builds should check every 5 days
        public static int UpdateCheckIntervalDays
        {
            get
            {
                if (PdnInfo.IsFinalBuild)
                {
                    return 5;
                }
                else
                {
                    return 1;
                }
            }
        }

        // If the build is final and less than 1 week old, then do NOT auto check for updates, no matter what.
        // This may help alleviate any release-day flooding
        // Pre-release builds have no such minimum time before checking.
        public static int MinBuildAgeForUpdateChecking
        {
            get
            {
                if (PdnInfo.IsFinalBuild)
                {
                    return 7;
                }
                else
                {
                    return 0;
                }
            }
        }

        // If the build is over 2 years old, then cease checking for updates.
        // Either we've stopped putting out new builds, or the user doesn't want to update,
        // or the user hardly ever uses the app anyway.
        // Check Now... will still continue to function.
        public const int MaxBuildAgeForUpdateChecking = 2 * 365;

        private static void DeleteUpdateMsi()
        {
            // If we just installed an update, then delete it! Save some hard drive space.
            string msiDeleteMeFull = Settings.CurrentUser.GetString(SettingNames.UpdateMsiFileName, null);
            string msiDeleteMe = Path.GetFileName(msiDeleteMeFull); // make sure someone can't put "..\..\..\..\windows\system32\cmd.exe" or something
            string msiDeleteMeExt = Path.GetExtension(msiDeleteMe);

            string setupTempDir = Environment.ExpandEnvironmentVariables(@"%TEMP%\PdnSetup");

            // Delete the update monitor exe if possible
            // Delete the update monitor exe if possible
            foreach (var fileName in new string[] { "UpdateMonitor.exe", "UpdateMonitor.exe.config" })
            {
                try
                {
                    FileSystem.TryDeleteFile(setupTempDir, fileName);
                }

                catch (Exception)
                {
                    // discard any error
                }
            }

            if (msiDeleteMe != null &&
                (string.Compare(".msi", msiDeleteMeExt, true, CultureInfo.InvariantCulture) == 0 ||
                 string.Compare(".exe", msiDeleteMeExt, true, CultureInfo.InvariantCulture) == 0))
            {
                string tempDir = Environment.ExpandEnvironmentVariables("%TEMP%");
                string msiPath = Path.Combine(tempDir, msiDeleteMe);
                int retryCount = 3;

                while (retryCount > 0)
                {
                    if (FileSystem.TryDeleteFile(msiPath))
                    {
                        break;
                    }

                    Thread.Sleep(500);
                    --retryCount;
                }

                Settings.CurrentUser.TryDelete(SettingNames.UpdateMsiFileName);
            }

            // Try to remove the dir from the temp folder
            if (Directory.Exists(setupTempDir))
            {
                FileSystem.TryDeleteDirectory(setupTempDir);
            }
        }

        /// <summary>
        /// Determines if it is time to check for updates.
        /// </summary>
        /// <returns>true if we should check for updates, false if it is not yet time to do so.</returns>
        /// <remarks>
        /// This method takes in to consideration whether update checking is enabled, and if it
        /// has been long enough since the last time we checked for updates.
        /// </remarks>
        public static bool ShouldCheckForUpdates()
        {
            bool shouldCheckForUpdates;
            bool autoCheckForUpdates = ("1" == Settings.SystemWide.GetString(SettingNames.AutoCheckForUpdates, "0"));

            TimeSpan minAge = new TimeSpan(MinBuildAgeForUpdateChecking, 0, 0, 0);
            TimeSpan maxAge = new TimeSpan(MaxBuildAgeForUpdateChecking, 0, 0, 0);

            TimeSpan buildAge = (DateTime.Now - PdnInfo.BuildTime);

            if (buildAge < minAge || buildAge > maxAge)
            {
                shouldCheckForUpdates = false;
            }
            else if (autoCheckForUpdates)
            {
                try
                {
                    string lastUpdateCheckTimeTicksString = Settings.CurrentUser.GetString(SettingNames.LastUpdateCheckTimeTicks, null);

                    if (lastUpdateCheckTimeTicksString == null)
                    {
                        shouldCheckForUpdates = true;
                    }
                    else
                    {
                        long lastUpdateCheckTimeTicks = long.Parse(lastUpdateCheckTimeTicksString);
                        DateTime lastUpdateCheckTime = new DateTime(lastUpdateCheckTimeTicks);

                        TimeSpan timeSinceLastCheck = DateTime.Now - lastUpdateCheckTime;

                        shouldCheckForUpdates = (timeSinceLastCheck > new TimeSpan(UpdateCheckIntervalDays, 0, 0, 0));
                    }
                }

                catch
                {
                    shouldCheckForUpdates = true;
                }
            }
            else
            {
                shouldCheckForUpdates = false;
            }

            return shouldCheckForUpdates;
        }

        public static void PingLastUpdateCheckTime()
        {
            Settings.CurrentUser.SetString(SettingNames.LastUpdateCheckTimeTicks, DateTime.Now.Ticks.ToString());
        }

        public override void OnEnteredState()
        {
            DeleteUpdateMsi();

            if ((Security.IsAdministrator || Security.CanElevateToAdministrator) &&
                PdnInfo.StartupTest == StartupTestType.None &&
                ShouldCheckForUpdates())
            {
                PingLastUpdateCheckTime();
                StateMachine.QueueInput(PrivateInput.GoToChecking);
            }
            else
            {
                StateMachine.QueueInput(UpdatesAction.Continue);
            }
        }

        public override void ProcessInput(object input, out State newState)
        {
            if (input.Equals(UpdatesAction.Continue))
            {
                newState = new ReadyToCheckState();
            }
            else if (input.Equals(PrivateInput.GoToChecking))
            {
                newState = new CheckingState();
            }
            else
            {
                throw new ArgumentException();
            }
        }

        public StartupState()
            : base(false, false, MarqueeStyle.Marquee)
        {
        }
    }
}
