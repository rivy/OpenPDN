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
using System.Drawing;

namespace PaintDotNet
{
    /// <summary>
    /// Data structure to manage the Most Recently Used list of files.
    /// </summary>
    internal class MostRecentFiles
    {
        private Queue files; // contains MostRecentFile instances
        private int maxCount;
        private const int iconSize = 56;
        private bool loaded = false;

        public MostRecentFiles(int maxCount)
        {
            this.maxCount = maxCount;
            this.files = new Queue();
        }

        public bool Loaded
        {
            get
            {
                return this.loaded;
            }
        }

        public int Count
        {
            get
            {
                if (!this.loaded)
                {
                    LoadMruList();
                }

                return this.files.Count;
            }
        }

        public int MaxCount
        {
            get
            {
                return this.maxCount;
            }
        }

        public int IconSize
        {
            get
            {
                return UI.ScaleWidth(iconSize);
            }
        }

        public MostRecentFile[] GetFileList()
        {
            if (!Loaded)
            {
                LoadMruList();
            }

            object[] array = files.ToArray();
            MostRecentFile[] mrfArray = new MostRecentFile[array.Length];
            array.CopyTo(mrfArray, 0);
            return mrfArray;
        }

        public bool Contains(string fileName)
        {
            if (!Loaded)
            {
                LoadMruList();
            }

            string lcFileName = fileName.ToLower();

            foreach (MostRecentFile mrf in files)
            {
                string lcMrf = mrf.FileName.ToLower();

                if (0 == String.Compare(lcMrf, lcFileName))
                {
                    return true;
                }
            }

            return false;
        }

        public void Add(MostRecentFile mrf)
        {
            if (!Loaded)
            {
                LoadMruList();
            }

            if (!Contains(mrf.FileName))
            {
                files.Enqueue(mrf);

                while (files.Count > maxCount)
                {
                    files.Dequeue();
                }
            }
        }

        public void Remove(string fileName)
        {
            if (!Loaded)
            {
                LoadMruList();
            }

            if (!Contains(fileName))
            {
                return;
            }

            Queue newQueue = new Queue();

            foreach (MostRecentFile mrf in files)
            {
                if (0 != string.Compare(mrf.FileName, fileName, true))
                {
                    newQueue.Enqueue(mrf);
                }
            }

            this.files = newQueue;
        }

        public void Clear()
        {
            if (!Loaded)
            {
                LoadMruList();
            }

            foreach (MostRecentFile mrf in this.GetFileList())
            {
                Remove(mrf.FileName);
            }
        }

        public void LoadMruList()
        {
            try
            {
                this.loaded = true;
                Clear();

                for (int i = 0; i < MaxCount; ++i)
                {
                    try
                    {
                        string mruName = "MRU" + i.ToString();
                        string fileName = (string)Settings.CurrentUser.GetString(mruName);

                        if (fileName != null)
                        {
                            Image thumb = Settings.CurrentUser.GetImage(mruName + "Thumb");

                            if (fileName != null && thumb != null)
                            {
                                MostRecentFile mrf = new MostRecentFile(fileName, thumb);
                                Add(mrf);
                            }
                        }
                    }

                    catch
                    {
                        break;
                    }
                }
            }

            catch (Exception ex)
            {
                Tracing.Ping("Exception when loading MRU list: " + ex.ToString());
                Clear();
            }
        }

        public void SaveMruList()
        {
            if (Loaded)
            {
                Settings.CurrentUser.SetInt32(SettingNames.MruMax, MaxCount);
                MostRecentFile[] mrfArray = GetFileList();

                for (int i = 0; i < MaxCount; ++i)
                {
                    string mruName = "MRU" + i.ToString();
                    string mruThumbName = mruName + "Thumb";

                    if (i >= mrfArray.Length)
                    {
                        Settings.CurrentUser.Delete(mruName);
                        Settings.CurrentUser.Delete(mruThumbName);
                    }
                    else
                    {
                        MostRecentFile mrf = mrfArray[i];
                        Settings.CurrentUser.SetString(mruName, mrf.FileName);
                        Settings.CurrentUser.SetImage(mruThumbName, mrf.Thumb);
                    }
                }
            }
        }
    }
}
