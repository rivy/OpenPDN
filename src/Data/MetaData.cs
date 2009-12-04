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
using System.Collections;
using System.Collections.Specialized;
using System.Drawing.Imaging;
using System.Text;

namespace PaintDotNet
{
    /// <summary>
    /// This class exposes two types of metadata: system, and user.
    /// It is provided mostly for batching operations: loading all the data, modifying the copy,
    /// and then saving back all the data.
    /// </summary>
    public class Metadata
    {
        /// <summary>
        /// This is the name of the section where EXIF tags are stored. 
        /// </summary>
        /// <remarks>
        /// All entries in here are expected to be PropertyItem objects which were serialized 
        /// using PdnGraphics.SerializePropertyItem. The name of each entry in this section is
        /// irrelevant, as some EXIF tags are allowed to occur more than once. Thus, if you
        /// want to search for EXIF tags of a certain ID you will have to deserialize each
        /// one and compare the Id property.
        /// It is the responsibility of the FileType implementation to load and save these.
        /// </remarks>
        public const string ExifSectionName = "$exif";

        /// <summary>
        /// This is the name of the section where user-defined metadata may go.
        /// </summary>
        public const string UserSectionName = "$user";

        /// <summary>
        /// This is the name of the section where the main document metadata goes that
        /// can be user-provided but is not necessarily user-defined.
        /// </summary>
        public const string MainSectionName = "$main";

        private NameValueCollection userMetaData;
        private const string sectionSeparator = ".";

        private int suppressChangeEvents = 0;

        public event EventHandler Changing;
        protected virtual void OnChanging()
        {
            if (suppressChangeEvents <= 0 && Changing != null)
            {
                Changing(this, EventArgs.Empty);
            }
        }

        public event EventHandler Changed;
        protected virtual void OnChanged()
        {
            if (suppressChangeEvents <= 0 && Changed != null)
            {
                Changed(this, EventArgs.Empty);
            }
        }

        private class ExifInfo
        {
            public string[] names;
            public PropertyItem[] items;

            public ExifInfo(string[] names, PropertyItem[] items)
            {
                this.names = names;
                this.items = items;
            }
        }

        private Hashtable exifIdToExifInfo = new Hashtable(); // maps short -> ExifInfo

        public string[] GetKeys(string section)
        {
            string sectionName = section + sectionSeparator;
            ArrayList keys = new ArrayList();

            foreach (string key in userMetaData.Keys)
            {
                if (key.StartsWith(sectionName))
                {
                    keys.Add(key.Substring(sectionName.Length));
                }
            }

            return (string[])keys.ToArray(typeof(string));
        }

        public string[] GetSections()
        {
            Set sections = new Set();

            foreach (string key in userMetaData.Keys)
            {
                int dotIndex = key.IndexOf(sectionSeparator);

                if (dotIndex != -1)
                {
                    string sectionName = key.Substring(0, dotIndex);

                    if (!sections.Contains(sectionName))
                    {
                        sections.Add(sectionName);
                    }
                }
            }

            return (string[])sections.ToArray(typeof(string));
        }

        /// <summary>
        /// Gets a value from the metadata collection.
        /// </summary>
        /// <param name="section">The logical section to retrieve from.</param>
        /// <param name="name">The name of the value to retrieve.</param>
        /// <returns>A string containing the value, or null if the value wasn't present.</returns>
        public string GetValue(string section, string name)
        {
            return userMetaData.Get(section + sectionSeparator + name);
        }

        public PropertyItem[] GetExifValues(ExifTagID id)
        {
            return GetExifValues((short)id);
        }

        public PropertyItem[] GetExifValues(short id)
        {
            ExifInfo info = (ExifInfo)this.exifIdToExifInfo[id];

            if (info == null)
            {
                return new PropertyItem[0];
            }
            else
            {
                return (PropertyItem[])info.items.Clone();
            }
        }

        public string GetUserValue(string name)
        {
            return GetValue(UserSectionName, name);
        }

        /// <summary>
        /// Removes a value from the metadata collection.
        /// </summary>
        /// <param name="section">The logical section to remove from.</param>
        /// <param name="name">The name of the value to retrieve.</param>
        public void RemoveValue(string section, string name)
        {
            OnChanging();
            userMetaData.Remove(section + sectionSeparator + name);
            OnChanged();
        }

        public void ReplaceExifValues(ExifTagID id, PropertyItem[] items)
        {
            ReplaceExifValues((short)id, items);
        }

        public void ReplaceExifValues(short id, PropertyItem[] items)
        {
            OnChanging();
            ++suppressChangeEvents;
            RemoveExifValues(id);
            AddExifValues(items);
            --suppressChangeEvents;
            OnChanged();
        }

        public void RemoveExifValues(ExifTagID id)
        {
            RemoveExifValues((short)id);
        }

        public void RemoveExifValues(short id)
        {
            object idObj = (object)id;
            ExifInfo info = (ExifInfo)this.exifIdToExifInfo[idObj];

            OnChanging();
            ++suppressChangeEvents;

            if (info != null)
            {
                foreach (string name in info.names)
                {
                    RemoveValue(ExifSectionName, name);
                }

                this.exifIdToExifInfo.Remove(idObj);
            }

            --suppressChangeEvents;
            OnChanged();
        }

        public void RemoveUserValue(string name)
        {
            RemoveValue(UserSectionName, name);
        }

        private void SetValueConcrete(string section, string name, string value)
        {
            OnChanging();
            userMetaData.Set(section + sectionSeparator + name, value);
            OnChanged();
        }

        /// <summary>
        /// Sets a value in the metadata collection.
        /// </summary>
        /// <param name="section">The logical section to add or update date in.</param>
        /// <param name="name">The name of the value to set.</param>
        /// <param name="value">The value to set.</param>
        public void SetValue(string section, string name, string value)
        {
            if (section == ExifSectionName)
            {
                throw new ArgumentException("you must use AddExifValues() to add items to the " + ExifSectionName + " section");
            }

            SetValueConcrete(section, name, value);
        }

        public void SetUserValue(string name, string value)
        {
            SetValue(Metadata.UserSectionName, name, value);
        }

        public void AddExifValues(PropertyItem[] items)
        {
            if (items.Length == 0)
            {
                return;
            }

            short id = unchecked((short)items[0].Id);

            for (int i = 1; i < items.Length; ++i)
            {
                if (unchecked((short)items[i].Id) != id)
                {
                    throw new ArgumentException("all PropertyItem instances in items must have the same id");
                }
            }

            string[] names = new string[items.Length];

            OnChanging();
            ++suppressChangeEvents;

            for (int i = 0; i < items.Length; ++i)
            {
                names[i] = GetUniqueExifName();
                string blob = PdnGraphics.SerializePropertyItem(items[i]);
                SetValueConcrete(ExifSectionName, names[i], blob);
            }

            object idObj = (object)id; // avoid boxing twice
            ExifInfo info = (ExifInfo)this.exifIdToExifInfo[idObj];

            if (info == null)
            {
                PropertyItem[] newItems = new PropertyItem[items.Length];

                for (int i = 0; i < newItems.Length; ++i)
                {
                    newItems[i] = PdnGraphics.ClonePropertyItem(items[i]);
                }

                info = new ExifInfo(names, newItems);
            }
            else
            {
                string[] names2 = new string[info.names.Length + names.Length];
                PropertyItem[] items2 = new PropertyItem[info.items.Length + items.Length];

                info.names.CopyTo(names2, 0);
                names.CopyTo(names2, info.names.Length);

                info.items.CopyTo(items2, 0);

                for (int i = 0; i < items.Length; ++i)
                {
                    items2[i + info.items.Length] = PdnGraphics.ClonePropertyItem(items[i]);
                }

                info = new ExifInfo(names2, items2);
            }

            this.exifIdToExifInfo[idObj] = info;

            --suppressChangeEvents;
            OnChanged();
        }

        private int nextUniqueId = 0;
        private string GetUniqueExifName()
        {
            int num = nextUniqueId;
            const string namePrefix = "tag";

            while (true)
            {
                string name = namePrefix + num.ToString();

                if (GetValue(ExifSectionName, name) == null)
                {
                    nextUniqueId = num + 1;
                    return name;
                }
                else
                {
                    ++num;
                }
            }
        }

        public void ReplaceWithDataFrom(Metadata source)
        {
            OnChanging();
            ++suppressChangeEvents;

            if (source != this && source.userMetaData != this.userMetaData)
            {
                Clear();

                foreach (string key in source.userMetaData.Keys)
                {
                    string value = source.userMetaData.Get(key);
                    this.userMetaData.Set(key, value);
                }

                ReconstructExifInfoCache();
            }

            --suppressChangeEvents;
            OnChanged();
        }

        public void Clear()
        {
            OnChanging();
            ++suppressChangeEvents;
            this.userMetaData.Clear();
            this.exifIdToExifInfo.Clear();
            --suppressChangeEvents;
            OnChanged();
        }

        private void ReconstructExifInfoCache()
        {
            OnChanging();
            ++suppressChangeEvents;

            exifIdToExifInfo.Clear();

            string[] exifKeys = GetKeys(ExifSectionName);
            string[] piBlobs = new string[exifKeys.Length];

            for (int i = 0; i < exifKeys.Length; ++i)
            {
                piBlobs[i] = GetValue(ExifSectionName, exifKeys[i]);
                this.RemoveValue(ExifSectionName, exifKeys[i]);
            }

            foreach (string piBlob in piBlobs)
            {
                PropertyItem pi = PdnGraphics.DeserializePropertyItem(piBlob);
                AddExifValues(new PropertyItem[] { pi });
            }

            --suppressChangeEvents;
            OnChanged();
        }

        internal Metadata(NameValueCollection userMetaData)
        {
            this.userMetaData = userMetaData;
            ReconstructExifInfoCache();
        }
    }
}
