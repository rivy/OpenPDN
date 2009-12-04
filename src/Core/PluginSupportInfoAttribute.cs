/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) Rick Brewster, Tom Jackson, and past contributors.            //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;

namespace PaintDotNet
{
    [AttributeUsage(
        AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Interface, 
        AllowMultiple = false, 
        Inherited = false)]
    public class PluginSupportInfoAttribute
        : Attribute,
          IPluginSupportInfo
    {
        private string displayName;
        private string author;
        private string copyright;
        private Version version = new Version();
        private Uri websiteUri;

        public string DisplayName
        {
            get
            {
                return this.displayName;
            }

            set
            {
                this.displayName = value;
            }
        }

        public string Author
        {
            get
            {
                return this.author;
            }
        }

        public string Copyright
        {
            get
            {
                return this.copyright;
            }
        }

        public Version Version
        {
            get
            {
                return this.version;
            }
        }

        public Uri WebsiteUri
        {
            get
            {
                return this.websiteUri;
            }
        }

        public PluginSupportInfoAttribute()
        {
        }

        public PluginSupportInfoAttribute(Type pluginSupportInfoProvider)
        {
            IPluginSupportInfo ipsi = (IPluginSupportInfo)Activator.CreateInstance(pluginSupportInfoProvider);
            this.displayName = ipsi.DisplayName;
            this.author = ipsi.Author;
            this.copyright = ipsi.Copyright;
            this.version = ipsi.Version;
            this.websiteUri = ipsi.WebsiteUri;
        }

        public PluginSupportInfoAttribute(string displayName, string author, string copyright, Version version, Uri websiteUri)
        {
            this.displayName = displayName;
            this.author = author;
            this.copyright = copyright;
            this.version = version;
            this.websiteUri = websiteUri;
        }
    }
}
