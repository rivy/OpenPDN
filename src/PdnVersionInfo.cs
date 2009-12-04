/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;

namespace PaintDotNet
{
    /// <summary>
    /// Contains information pertaining to a release of Paint.NET
    /// </summary>
    internal class PdnVersionInfo
    {
        private Version version;
        private string friendlyName;
        private int netFxMajorVersion;
        private int netFxMinorVersion;
        private int netFxServicePack;
        private string infoUrl;
        private string[] downloadUrls;
        private string[] fullDownloadUrls;
        private bool isFinal;

        public Version Version
        {
            get
            {
                return this.version;
            }
        }

        public string FriendlyName
        {
            get
            {
                return this.friendlyName;
            }
        }

        public int NetFxMajorVersion
        {
            get
            {
                return this.netFxMajorVersion;
            }
        }

        public int NetFxMinorVersion
        {
            get
            {
                return this.netFxMinorVersion;
            }
        }

        public int NetFxServicePack
        {
            get
            {
                return this.netFxServicePack;
            }
        }

        public string InfoUrl
        {
            get
            {
                return this.infoUrl;
            }
        }
        
        public string[] DownloadUrls
        {
            get
            {
                return (string[])this.downloadUrls.Clone();
            }
        }

        public string[] FullDownloadUrls
        {
            get
            {
                return (string[])this.fullDownloadUrls.Clone();
            }
        }

        public bool IsFinal
        {
            get
            {
                return this.isFinal;
            }
        }

        public string ChooseDownloadUrl(bool full)
        {
            DateTime now = DateTime.Now;
            string[] urls;

            if (full)
            {
                urls = FullDownloadUrls;
            }
            else
            {
                urls = DownloadUrls;
            }

            int index = Math.Abs(now.Second % urls.Length);
            return urls[index];
        }

        public PdnVersionInfo(
            Version version, 
            string friendlyName, 
            int netFxMajorVersion,
            int netFxMinorVersion,
            int netFxServicePack,
            string infoUrl, 
            string[] downloadUrls, 
            string[] fullDownloadUrls, 
            bool isFinal)
        {
            this.version = version;
            this.friendlyName = friendlyName;
            this.netFxMajorVersion = netFxMajorVersion;
            this.netFxMinorVersion = netFxMinorVersion;
            this.netFxServicePack = netFxServicePack;
            this.infoUrl = infoUrl;
            this.downloadUrls = (string[])downloadUrls.Clone();
            this.fullDownloadUrls = (string[])fullDownloadUrls.Clone();
            this.isFinal = isFinal;
        }
    }
}
