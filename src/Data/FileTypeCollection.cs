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
using System.Collections.Generic;
using System.Text;

namespace PaintDotNet
{
    /// <summary>
    /// Represents a collection of FileType instances.
    /// </summary>
    [Serializable]
    public class FileTypeCollection
    {
        private FileType[] fileTypes;
        public FileType[] FileTypes
        {
            get
            {
                return (FileType[])fileTypes.Clone();
            }
        }

        public int Length
        {
            get
            {
                return fileTypes.Length;
            }
        }

        public FileType this[int index]
        {
            get
            {
                return fileTypes[index];
            }
        }

        public string[] AllExtensions
        {
            get
            {
                List<string> exts = new List<string>();

                foreach (FileType fileType in this.fileTypes)
                {
                    foreach (string ext in fileType.Extensions)
                    {
                        exts.Add(ext);
                    }
                }

                return exts.ToArray();
            }
        }

        internal FileTypeCollection(FileType[] fileTypes)
        {
            this.fileTypes = fileTypes;
        }

        public FileTypeCollection(ICollection fileTypes)
        {
            this.fileTypes = new FileType[fileTypes.Count];
            int dstIndex = 0;

            foreach (FileType ft in fileTypes)
            {
                this.fileTypes[dstIndex] = ft;
                ++dstIndex;
            }
        }

        public static FileType[] FilterFileTypeList(FileType[] input, bool excludeCantSave, bool excludeCantLoad)
        {
            List<FileType> filtered = new List<FileType>();

            foreach (FileType fileType in input)
            {
                if (excludeCantSave && !fileType.SupportsSaving)
                {
                    continue;
                }

                if (excludeCantLoad && !fileType.SupportsLoading)
                {
                    continue;
                }

                filtered.Add(fileType);
            }

            return filtered.ToArray();
        }

        public string ToString(bool excludeCantSave, bool excludeCantLoad)
        {
            FileType[] filtered = FilterFileTypeList(this.fileTypes, excludeCantSave, excludeCantLoad);

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < filtered.Length; ++i)
            {
                FileType fileType = filtered[i];
                sb.Append(fileType.ToString());

                if (i != filtered.Length - 1)
                {
                    sb.Append("|");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Allows you to include an "All" type at the top that includes all the filetypes
        /// "All images (*.bmp, *.gif, ...)" for instance.
        /// </summary>
        /// <param name="includeAll">Whether or not to include the 'all' file type at the top</param>
        /// <param name="allName">The name of the 'all' type (example: "All images"). If this is null
        /// but includeAll is true, then this defaults to the string "All image types"</param>
        public string ToString(bool includeAll, string allName, bool excludeCantSave, bool excludeCantLoad)
        {
            if (allName == null)
            {
                allName = PdnResources.GetString("FileTypeCollection.AllImageTypes");
            }

            if (includeAll)
            {
                StringBuilder description = new StringBuilder(allName);
                StringBuilder formats = new StringBuilder();
                bool didFirst = false;
                FileType[] filtered = FilterFileTypeList(this.fileTypes, excludeCantSave, excludeCantLoad);

                for (int i = 0; i < filtered.Length; ++i)
                {
                    if (!didFirst)
                    {
                        didFirst = true;
                        description.Append(" (");
                    }

                    string[] extensions = (filtered[i]).Extensions;

                    for (int j = 0; j < extensions.Length; ++j)
                    {
                        description.Append("*");
                        description.Append(extensions[j]);
                        formats.Append("*");
                        formats.Append(extensions[j]);

                        // if this is NOT the last extension in the whole list ...
                        if (!(j == extensions.Length - 1 && i == filtered.Length - 1))
                        {
                            description.Append(", ");
                            formats.Append(";");
                        }
                    }

                }    

                if (didFirst)
                {
                    description.Append(")");
                }

                string ret = description.ToString() + "|" + formats.ToString();

                if (filtered.Length != 0)
                {
                    ret += "|" + ToString(excludeCantSave, excludeCantLoad);
                }

                return ret;
            }
            else
            {
                return ToString(excludeCantSave, excludeCantLoad);
            }
        }

        public int IndexOfFileType(FileType fileType)
        {
            if (fileType == null)
            {
                return -1;
            }

            for (int i = 0; i < fileTypes.Length; ++i)
            {
                if (fileTypes[i].Name == fileType.Name)
                {
                    return i;
                }
            }

            return -1;
        }

        public int IndexOfExtension(string findMeExt)
        {
            if (findMeExt == null)
            {
                return -1;
            }

            for (int i = 0; i < fileTypes.Length; ++i)
            {
                foreach (string ext in fileTypes[i].Extensions)
                {
                    if (ext.ToLower() == findMeExt.ToLower())
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        public int IndexOfName(string name)
        {
            for (int i = 0; i < fileTypes.Length; ++i)
            {
                if (fileTypes[i].Name == name)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
