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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace PaintDotNet.Updates
{
    internal class CheckingState
        : UpdatesState
    {
        // versions.txt schema:
        //     ; This is a comment
        //     DownloadPageUrl=downloadPageUrl                  // This should link to the main download page
        //     StableVersions=version1,version2,...,versionN    // A comma-separated list of all available stable versions available for download
        //     BetaVersions=version1,version2,...,versionN      // A comma-separated list of all available beta/pre-release versions available for download
        //
        //     version1_Name=name1                              // Friendly name for a given version
        //     version1_NetFxVersion=netFxVersion1              // What version of .NET does this version require?
        //                                                         For .NET 2.0, this should be specificed as 2.0.x, where x used to be the build number (50727) but is now ignored
        //                                                         For .NET 3.5, this should be specificed as 3.5.x, where x is the required service pack level
        //     version1_InfoUrl=infoUrl1                        // A URL that contains information about the given version
        //     version1_ZipUrlList="zipUrl1","zipUrl2",...,"zipUrlN"
        //                                                      // A comma-delimited list of URL's for mirrors to download the updater. One will be chosen at random.
        //     version1_FullZipUrlList="zipFullUrl1","zipFullUrl2",...,"zipFullUrlN"
        //                                                      // A comma-delimited list of URL's for mirrors to download the 'full' installer ('full' means it bundles the appropriate .NET installer
        //     ...
        //     versionN_Name=name1                              // Friendly name for a given version
        //     versionN_NetFxVersion=netFxVersionN              // What version of .NET does this version require?
        //     versionN_InfoUrl=infoUrlN                        // A URL that contains information about the given version
        //     versionN_ZipUrlList=zipUrlN                      // A comma-delimited list of URL's for mirrors to download the updater. One will be chosen at random.
        //     versionN_FullZipUrl=zipFullUrlN                  // A comma-delimited list of URL's for mirrors to download the 'full' installer ('full' means it bundles the appropriate .NET installer
        //     
        // Example:
        //     ; Paint.NET versions download manifest
        //     DownloadPageUrl=http://www.getpaint.net/download.htm
        //     StableVersions=2.1.1958.27164
        //     BetaVersions=2.5.2013.31044
        //     
        //     2.1.1958.27164_Name=Paint.NET v2.1b     
        //     2.1.1958.27164_InfoUrl=http://www.getpaint.net/roadmap.htm#v2_1
        //     2.1.1958.27164_NetFxVersion=1.1.4322
        //     2.1.1958.27164_ZipUrlList=http://www.getpaint.net/zip/PaintDotNet_2_1b.zip
        //     2.1.1958.27164_FullZipUrlList=http://www.getpaint.net/zip/PaintDotNet_2_1b_Full.zip
        //     
        //     2.5.2013.31044_Name=Paint.NET v2.5        
        //     2.5.2013.31044_InfoUrl=http://www.getpaint.net/roadmap.htm#v2_5
        //     2.5.2013.31044_NetFxVersion=1.1.4322
        //     2.5.2013.31044_ZipUrlList=http://www.getpaint.net/zip/PaintDotNet_2_5.zip
        //     2.5.2013.31044_FullZipUrlList=http://www.getpaint.net/zip/PaintDotNet_2_5_Full.zip
        //     
        //     2.6.2113.23752_Name=Paint.NET v2.6 Beta 1
        //     2.6.2113.23752_InfoUrl=http://www.getpaint.net/roadmap.htm#v2_6
        //     2.6.2113.23752_NetFxVersion=2.0.50727
        //     2.6.2113.23752_ZipUrlList="http://www.getpaint.net/zip/PaintDotNet_2_6_Beta1.zip","http://www.someotherhost.com/files/PaintDotNet_2_6_Beta1.zip"
        //     2.6.2113.23752_FullZipUrlList="http://www.getpaint.net/zip/PaintDotNet_2_6_Beta1_Full.zip","http://www.someotherhost.com/files/PaintDotNet_2_6_Beta1_Full.zip"
        //     
        // Notes:
        //     A line may have a comment on it. Just start the line with an asterisk, '*'
        //     Versions must be formatted in a manner parseable by the System.Version class.
        //     BetaVersions may be an empty list: "BetaVersions="
        //     versionN_InfoUrl may not be blank
        //     versionN_ZipUrl may not be blank
        //     versionN_ZipUrlSize must be greater than 0.
        //     If any error is detected while parsing, the entire schema will be declared as invalid and ignored.
        //     Everything is case-sensitive.

        private const string downloadPageUrlName = "DownloadPageUrl";
        private const string stableVersionsName = "StableVersions";
        private const string betaVersionsName = "BetaVersions";
        private const string nameNameFormat = "{0}_Name";
        private const string netFxVersionNameFormat = "{0}_NetFxVersion";
        private const string infoUrlNameFormat = "{0}_InfoUrl";
        private const string zipUrlListNameFormat = "{0}_ZipUrlList";
        private const string fullZipUrlListNameFormat = "{0}_FullZipUrlList";
        private const char commentChar = ';';

        // {0} is schema version
        // {1} is Windows revision (501 for XP, 502 for Server 2k3, 600 for Vista, 601 for Win7)
        // {2} is platform (x86, x64)
        // {3} is the locale (en, etc)
        private const string versionManifestRelativeUrlFormat = "/updates/versions.{0}.{1}.{2}.{3}.txt";
        private const string versionManifestTestRelativeUrl = "/updates/versions.txt.test.txt";
        private const int schemaVersion = 5;

        private PdnVersionManifest manifest;
        private int latestVersionIndex;
        private Exception exception;

        private ManualResetEvent checkingEvent = new ManualResetEvent(false);
        private ManualResetEvent abortEvent = new ManualResetEvent(false);

        private static string GetNeutralLocaleName(CultureInfo ci)
        {
            if (ci.IsNeutralCulture)
            {
                return ci.Name;
            }

            if (ci.Parent == null)
            {
                return ci.Name;
            }

            if (ci.Parent == ci)
            {
                return ci.Name;
            }

            return GetNeutralLocaleName(ci.Parent);
        }

        private static string VersionManifestUrl
        {
            get
            {
                Uri websiteUri = new Uri(InvariantStrings.WebsiteUrl);
                string versionManifestUrl;

                if (PdnInfo.IsTestMode)
                {
                    Uri versionManifestTestUri = new Uri(websiteUri, versionManifestTestRelativeUrl);
                    versionManifestUrl = versionManifestTestUri.ToString();
                }
                else
                {
                    string schemaVersionStr = schemaVersion.ToString(CultureInfo.InvariantCulture);
                    Version osVersion = Environment.OSVersion.Version;
                    ProcessorArchitecture platform = SystemLayer.Processor.Architecture;
                    OSType osType = SystemLayer.OS.Type;

                    // If this is XP x64, we want to fudge the NT version to be 5.1 instead of 5.2
                    // This helps us discern between XP x64 and Server 2003 x64 stats.
                    if (osVersion.Major == 5 && osVersion.Minor == 2 && platform == ProcessorArchitecture.X64 && osType == OSType.Workstation)
                    {
                        osVersion = new Version(5, 1, osVersion.Build, osVersion.Revision);
                    }

                    int osVersionInt = (osVersion.Major * 100) + osVersion.Minor;
                    string osVersionStr = osVersionInt.ToString(CultureInfo.InvariantCulture);
                    string platformStr = platform.ToString().ToLower();
                    string localeStr = GetNeutralLocaleName(PdnResources.Culture);
                    Uri versionManifestUrlFormatUri = new Uri(websiteUri, versionManifestRelativeUrlFormat);
                    string versionManifestUrlFormat = versionManifestUrlFormatUri.ToString();

                    versionManifestUrl = string.Format(versionManifestUrlFormat, schemaVersionStr, osVersionStr, platformStr, localeStr);
                }

                return versionManifestUrl;
            }
        }

        private static string[] BreakIntoLines(string text)
        {
            StringReader sr = new StringReader(text);
            List<string> strings = new List<string>();
            string line;

            while ((line = sr.ReadLine()) != null)
            {
                if (line.Length > 0 && line[0] != commentChar)
                {
                    strings.Add(line);
                }
            }

            return strings.ToArray();
        }

        private static void LineToNameValue(string line, out string name, out string value)
        {
            int equalIndex = line.IndexOf('=');

            if (equalIndex == -1)
            {
                throw new FormatException("Line had no equal sign (=) present");
            }

            name = line.Substring(0, equalIndex);

            int valueLength = line.Length - equalIndex - 1;

            if (valueLength == 0)
            {
                value = string.Empty;
            }
            else
            {
                value = line.Substring(equalIndex + 1, line.Length - equalIndex - 1);
            }
        }

        private static NameValueCollection LinesToNameValues(string[] lines)
        {
            NameValueCollection nvc = new NameValueCollection();

            foreach (string line in lines)
            {
                string name;
                string value;

                LineToNameValue(line, out name, out value);
                nvc.Add(name, value);
            }

            return nvc;
        }

        private static Version[] VersionStringToArray(string versions)
        {
            string[] versionStrings = versions.Split(',');

            // For the 'null' case...
            if (versionStrings.Length == 0 ||
                (versionStrings.Length == 1 && versionStrings[0].Length == 0))
            {
                return new Version[0];
            }

            Version[] versionList = new Version[versionStrings.Length];

            for (int i = 0; i < versionStrings.Length; ++i)
            {
                versionList[i] = new Version(versionStrings[i]);
            }

            return versionList;
        }

        private static string[] BuildVersionValueMapping(NameValueCollection nameValues, Version[] versions, string secondaryKeyFormat)
        {
            string[] newValues = new string[versions.Length];

            for (int i = 0; i < versions.Length; ++i)
            {
                string versionString = versions[i].ToString();
                string secondaryKey = string.Format(secondaryKeyFormat, versionString);
                string secondaryValue = nameValues[secondaryKey];
                newValues[i] = secondaryValue;
            }

            return newValues;
        }

        private static void SplitUrlList(string urlList, List<string> urlsOutput)
        {
            if (string.IsNullOrEmpty(urlList))
            {
                return;
            }

            string trimUrlList = urlList.Trim();
            string url;
            int commaIndex;
            
            if (trimUrlList[0] == '"')
            {
                int endQuoteIndex = trimUrlList.IndexOf('"', 1);
                commaIndex = trimUrlList.IndexOf(',', endQuoteIndex);
                url = trimUrlList.Substring(1, endQuoteIndex - 1);
            }
            else
            {
                commaIndex = trimUrlList.IndexOf(',');

                if (commaIndex == -1)
                {
                    url = trimUrlList;
                }
                else
                {
                    url = trimUrlList.Substring(0, commaIndex);
                }
            }

            string urlTail;
            if (commaIndex == -1)
            {
                urlTail = null;
            }
            else
            {
                urlTail = trimUrlList.Substring(commaIndex + 1);
            }

            urlsOutput.Add(url);
            SplitUrlList(urlTail, urlsOutput);
        }

        /// <summary>
        /// Downloads the latest updates manifest from the Paint.NET web server.
        /// </summary>
        /// <returns>The latest updates manifest, or null if there was an error in which case the exception argument will be non-null.</returns>
        private static PdnVersionManifest GetUpdatesManifest(out Exception exception)
        {
            try
            {
                string versionsUrl = VersionManifestUrl;

                Uri versionsUri = new Uri(versionsUrl);
                byte[] manifestBuffer = Utility.DownloadSmallFile(versionsUri);
                string manifestText = System.Text.Encoding.UTF8.GetString(manifestBuffer);
                string[] manifestLines = BreakIntoLines(manifestText);
                NameValueCollection nameValues = LinesToNameValues(manifestLines);

                string downloadPageUrl = nameValues[downloadPageUrlName];

                string stableVersionsStrings = nameValues[stableVersionsName];
                Version[] stableVersions = VersionStringToArray(stableVersionsStrings);
                string[] stableNames = BuildVersionValueMapping(nameValues, stableVersions, nameNameFormat);
                string[] stableNetFxVersions = BuildVersionValueMapping(nameValues, stableVersions, netFxVersionNameFormat);
                string[] stableInfoUrls = BuildVersionValueMapping(nameValues, stableVersions, infoUrlNameFormat);
                string[] stableZipUrls = BuildVersionValueMapping(nameValues, stableVersions, zipUrlListNameFormat);
                string[] stableFullZipUrls = BuildVersionValueMapping(nameValues, stableVersions, fullZipUrlListNameFormat);

                string betaVersionsStrings = nameValues[betaVersionsName];
                Version[] betaVersions = VersionStringToArray(betaVersionsStrings);
                string[] betaNames = BuildVersionValueMapping(nameValues, betaVersions, nameNameFormat);
                string[] betaNetFxVersions = BuildVersionValueMapping(nameValues, betaVersions, netFxVersionNameFormat);
                string[] betaInfoUrls = BuildVersionValueMapping(nameValues, betaVersions, infoUrlNameFormat);
                string[] betaZipUrls = BuildVersionValueMapping(nameValues, betaVersions, zipUrlListNameFormat);
                string[] betaFullZipUrls = BuildVersionValueMapping(nameValues, betaVersions, fullZipUrlListNameFormat);

                PdnVersionInfo[] versionInfos = new PdnVersionInfo[betaVersions.Length + stableVersions.Length];

                int cursor = 0;
                for (int i = 0; i < stableVersions.Length; ++i)
                {
                    List<string> zipUrlList = new List<string>();
                    SplitUrlList(stableZipUrls[i], zipUrlList);

                    List<string> fullZipUrlList = new List<string>();
                    SplitUrlList(stableFullZipUrls[i], fullZipUrlList);

                    Version netFxVersion = new Version(stableNetFxVersions[i]);

                    if (netFxVersion.Major == 2 && netFxVersion.Minor == 0)
                    {
                        netFxVersion = new Version(2, 0, 0); // discard the build # that is specified, since we use that for Service Pack level now
                    }

                    PdnVersionInfo info = new PdnVersionInfo(
                        stableVersions[i], 
                        stableNames[i], 
                        netFxVersion.Major,
                        netFxVersion.Minor,
                        netFxVersion.Build, // service pack
                        stableInfoUrls[i], 
                        zipUrlList.ToArray(), 
                        fullZipUrlList.ToArray(), 
                        true);

                    versionInfos[cursor] = info;
                    ++cursor;
                }

                for (int i = 0; i < betaVersions.Length; ++i)
                {
                    List<string> zipUrlList = new List<string>();
                    SplitUrlList(betaZipUrls[i], zipUrlList);

                    List<string> fullZipUrlList = new List<string>();
                    SplitUrlList(betaFullZipUrls[i], fullZipUrlList);

                    Version netFxVersion = new Version(betaNetFxVersions[i]);

                    if (netFxVersion.Major == 2 && netFxVersion.Minor == 0)
                    {
                        netFxVersion = new Version(2, 0, 0); // discard the build # that is specified, since we use that for Service Pack level now
                    }

                    PdnVersionInfo info = new PdnVersionInfo(
                        betaVersions[i], 
                        betaNames[i],
                        netFxVersion.Major,
                        netFxVersion.Minor,
                        netFxVersion.Build, // service pack
                        betaInfoUrls[i], 
                        zipUrlList.ToArray(), 
                        fullZipUrlList.ToArray(), 
                        false);

                    versionInfos[cursor] = info;
                    ++cursor;
                }

                PdnVersionManifest manifest = new PdnVersionManifest(downloadPageUrl, versionInfos);
                exception = null;
                return manifest;
            }

            catch (Exception ex)
            {
                exception = ex;
                return null;
            }
        }

        private static void CheckForUpdates(
            out PdnVersionManifest manifestResult,
            out int latestVersionIndexResult,
            out Exception exception)
        {
            exception = null;
            PdnVersionManifest manifest = null;
            manifestResult = null;
            latestVersionIndexResult = -1;

            int retries = 2;

            while (retries > 0)
            {
                try
                {
                    manifest = GetUpdatesManifest(out exception);
                    retries = 0;
                }

                catch (Exception ex)
                {
                    exception = ex;
                    --retries;

                    if (retries == 0)
                    {
                        manifest = null;
                    }
                }
            }

            if (manifest != null)
            {
                int stableIndex = manifest.GetLatestStableVersionIndex();
                int betaIndex = manifest.GetLatestBetaVersionIndex();

                // Check for betas as well?
                bool checkForBetas = ("1" == Settings.SystemWide.GetString(SettingNames.AlsoCheckForBetas, "0"));

                // Figure out which version we want to compare against the current version
                int latestIndex = stableIndex;

                if (checkForBetas)
                {
                    // If they like betas, and if the beta is newer than the latest stable release,
                    // then offer it to them.
                    if (betaIndex != -1 &&
                        (stableIndex == -1 || manifest.VersionInfos[betaIndex].Version >= manifest.VersionInfos[stableIndex].Version))
                    {
                        latestIndex = betaIndex;
                    }
                }

                // Now compare that version against the current version
                if (latestIndex != -1)
                {
                    if (PdnInfo.IsTestMode ||
                        manifest.VersionInfos[latestIndex].Version > PdnInfo.GetVersion())
                    {
                        manifestResult = manifest;
                        latestVersionIndexResult = latestIndex;
                    }
                }
            }
        }

        public override bool CanAbort
        {
            get
            {
                return true;
            }
        }

        protected override void OnAbort()
        {
            this.abortEvent.Set();
            base.OnAbort();
        }

        private void DoCheckThreadProc(object ignored)
        {
            try
            {
                System.Threading.Thread.Sleep(1500);
                CheckForUpdates(out this.manifest, out this.latestVersionIndex, out this.exception);
            }

            finally
            {
                this.checkingEvent.Set();
            }
        }

        public override void OnEnteredState()
        {
            this.checkingEvent.Reset();
            this.abortEvent.Reset();

            ThreadPool.QueueUserWorkItem(new WaitCallback(DoCheckThreadProc));

            WaitHandleArray events = new WaitHandleArray(2);
            events[0] = this.checkingEvent;
            events[1] = this.abortEvent;
            int waitResult = events.WaitAny();

            if (waitResult == 0 && manifest != null && latestVersionIndex != -1)
            {
                StateMachine.QueueInput(PrivateInput.GoToUpdateAvailable);
            }
            else if (waitResult == 1)
            {
                StateMachine.QueueInput(PrivateInput.GoToAborted);
            }
            else if (this.exception != null)
            {
                StateMachine.QueueInput(PrivateInput.GoToError);
            }
            else
            {
                StateMachine.QueueInput(PrivateInput.GoToDone);
            }
        }

        public override void ProcessInput(object input, out State newState)
        {
            if (input.Equals(PrivateInput.GoToUpdateAvailable))
            {
                newState = new UpdateAvailableState(this.manifest.VersionInfos[this.latestVersionIndex]);
            }
            else if (input.Equals(PrivateInput.GoToError))
            {
                string errorMessage;

                if (this.exception is WebException)
                {
                    errorMessage = Utility.WebExceptionToErrorMessage((WebException)this.exception);
                }
                else
                {
                    errorMessage = PdnResources.GetString("Updates.CheckingState.GenericError");
                }

                newState = new ErrorState(this.exception, errorMessage);
            }
            else if (input.Equals(PrivateInput.GoToDone))
            {
                newState = new DoneState();
            }
            else if (input.Equals(PrivateInput.GoToAborted))
            {
                newState = new AbortedState();
            }
            else
            {
                throw new ArgumentException();
            }
        }

        public CheckingState()
            : base(false, false, MarqueeStyle.Marquee)
        {
        }
    }
}
