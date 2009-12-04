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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Security.Permissions;
using System.Threading;
using System.Windows.Forms;

namespace PaintDotNet
{
    public sealed class PersistedObject<T> 
        : IDisposable
    {
        private static ArrayList fileNames = ArrayList.Synchronized(new ArrayList());

        public static string[] FileNames
        {
            get
            {
                return (string[])fileNames.ToArray(typeof(string));
            }
        }

        // NOTE: We use a BSTR to hold the filename because we still need to be able to
        //       delete the file in our finalizer. However, the rules of finalizers say
        //       that you may not reference another object. Hence, we can not use a
        //       normal .NET System.String.
        private IntPtr bstrTempFileName = IntPtr.Zero;

        private string tempFileName;
        private WeakReference objectRef;
        private bool disposed = false;
        private ManualResetEvent theObjectSaved = new ManualResetEvent(false);

        private void WaitForObjectSaved()
        {
            ManualResetEvent theEvent = this.theObjectSaved;

            if (theEvent != null)
            {
                theEvent.WaitOne();
            }
        }

        private void WaitForObjectSaved(int timeoutMs)
        {
            ManualResetEvent theEvent = this.theObjectSaved;

            if (theEvent != null)
            {
                theEvent.WaitOne(timeoutMs, false);
            }
        }

        /// <summary>
        /// Gets the data stored in this instance of PersistedObject.
        /// </summary>
        /// <remarks>
        /// If the object has already been finalized and freed from memory, then this
        /// property will deserialize the object from disk before returning a new
        /// reference to it.
        /// </remarks>
        public T Object
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("PersistedObject");
                }

                T o;
                
                if (objectRef == null)
                {
                    o = default(T);
                }
                else
                {
                    o = (T)objectRef.Target;
                }

                if (o == null)
                {
                    string strTempFileName = Marshal.PtrToStringBSTR(this.bstrTempFileName);
                    FileStream stream = new FileStream(strTempFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                    BinaryFormatter formatter = new BinaryFormatter();
                    DeferredFormatter deferred = new DeferredFormatter();
                    StreamingContext context = new StreamingContext(formatter.Context.State, deferred);
                    formatter.Context = context;
                    T localObject = (T)formatter.Deserialize(stream);
                    deferred.FinishDeserialization(stream);
                    this.objectRef = new WeakReference(localObject);
                    stream.Close();

                    return localObject;
                }
                else
                {
                    return o;
                }
            }
        }

        /// <summary>
        /// Gets the data stored in this instance of PersistedObject.
        /// </summary>
        /// <remarks>
        /// If the object has already been finalized and freed from memory, then
        /// this property will return null.
        /// </remarks>
        public T WeakObject
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("PersistedObject");
                }

                if (objectRef == null)
                {
                    return default(T);
                }
                else
                {
                    return (T)objectRef.Target;
                }
            }
        }

        /// <summary>
        /// Ensures that the object held by this instance of PersistedObject is flushed to disk
        /// and freed from memory.
        /// </summary>
        public void Flush()
        {
            WaitForObjectSaved();

            // At this point we can now assume the object has been serialized to disk.
            object obj = this.WeakObject;
            IDisposable disposable = obj as IDisposable;

            if (disposable != null)
            {
                disposable.Dispose();
                disposable = null;
            }

            this.objectRef = null;
        }

        /// <summary>
        /// Creates a new instance of the PersistedObject class.
        /// </summary>
        /// <param name="theObject">
        /// The object to persist. It must be serializable.
        /// </param>
        /// <param name="background">
        /// Whether to serialize to disk in the background. If you specify true, then you must make
        /// sure not to mutate or dispose theObject.</param>
        /// <remarks>
        /// Deferred serialization via IDeferredSerializable and DeferredFormatter are supported,
        /// and the compression level will be set to none (zero) if background is false. The
        /// compression level will be one if background is true.
        /// </remarks>
        public PersistedObject(T theObject, bool background)
        {
            this.objectRef = new WeakReference(theObject);
            this.tempFileName = FileSystem.GetTempFileName();
            fileNames.Add(tempFileName);
            this.bstrTempFileName = Marshal.StringToBSTR(tempFileName);

            if (background)
            {
                Thread thread = new Thread(new ParameterizedThreadStart(PersistToDiskThread));
                thread.Start(theObject);
            }
            else
            {
                PersistToDisk(theObject);
            }
        }

        private void PersistToDiskThread(object theObject)
        {
            using (new ThreadBackground(ThreadBackgroundFlags.Cpu))
            {
                PersistToDisk(theObject);
            }
        }

        private void PersistToDisk(object theObject)
        {
            try
            {
                FileStream stream = new FileStream(this.tempFileName, FileMode.Create, FileAccess.Write, FileShare.Read);
                BinaryFormatter formatter = new BinaryFormatter();
                DeferredFormatter deferred = new DeferredFormatter(false, null);
                StreamingContext context = new StreamingContext(formatter.Context.State, deferred);

                formatter.Context = context;
                formatter.Serialize(stream, theObject);
                deferred.FinishSerialization(stream);
                stream.Flush();
                stream.Close();
            }

            finally
            {
                this.theObjectSaved.Set();
                this.theObjectSaved = null;
            }
        }

        ~PersistedObject()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                this.disposed = true;

                if (disposing)
                {
                    WaitForObjectSaved(1000);
                }

                string strTempFileName = Marshal.PtrToStringBSTR(this.bstrTempFileName);

                FileInfo fi = new FileInfo(strTempFileName);

                if (fi.Exists)
                {
                    bool result = FileSystem.TryDeleteFile(fi.FullName);

                    try
                    {
                        fileNames.Remove(strTempFileName);
                    }

                    catch
                    {
                    }
                }

                Marshal.FreeBSTR(this.bstrTempFileName);
                this.bstrTempFileName = IntPtr.Zero;

                if (disposing)
                {
                    ManualResetEvent theEvent = this.theObjectSaved;
                    this.theObjectSaved = null;

                    if (theEvent != null)
                    {
                        theEvent.Close();
                        theEvent = null;
                    }
                }
            }
        }

        static PersistedObject()
        {
            Application.ApplicationExit += new EventHandler(Application_ApplicationExit);
        }

        private static void Application_ApplicationExit(object sender, EventArgs e)
        {
            // Clean-up leftover persisted objects
            string[] fileNames = PersistedObject<T>.FileNames;

            if (fileNames.Length != 0)
            {
                foreach (string fileName in fileNames)
                {
                    FileInfo fi = new FileInfo(fileName);

                    if (fi.Exists)
                    {
                        bool result = FileSystem.TryDeleteFile(fi.FullName);
                    }
                }
            }
        }
    }
}
