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
using System.IO;

namespace PaintDotNet
{
    public sealed class DeferredFormatter
    {
        private ArrayList objects = ArrayList.Synchronized(new ArrayList());
        private bool used = false;
        private object context;
        private long totalSize;
        private long totalReportedBytes;
        private bool useCompression;
        private object lockObject = new object();

        public object Context
        {
            get
            {
                return this.context;
            }
        }

        public bool UseCompression
        {
            get
            {
                return this.useCompression;
            }
        }

        public DeferredFormatter()
            : this(false, null)
        {
        }

        public DeferredFormatter(bool useCompression, object context)
        {
            this.useCompression = useCompression;
            this.context = context;
        }

        public void AddDeferredObject(IDeferredSerializable theObject, long objectByteSize)
        {
            if (used)
            {
                throw new InvalidOperationException("object already finished serialization");
            }

            this.totalSize += objectByteSize;
            objects.Add(theObject);
        }

        public event EventHandler ReportedBytesChanged;
        private void OnReportedBytesChanged()
        {
            if (ReportedBytesChanged != null)
            {
                ReportedBytesChanged(this, EventArgs.Empty);
            }
        }

        public long ReportedBytes
        {
            get
            {
                lock (lockObject)
                {
                    return totalReportedBytes;
                }
            }
        }

        /// <summary>
        /// Reports that bytes have been successfully been written.
        /// </summary>
        /// <param name="bytes"></param>
        public void ReportBytes(long bytes)
        {
            lock (lockObject)
            {
                totalReportedBytes += bytes;
            }

            OnReportedBytesChanged();
        }

        public void FinishSerialization(Stream output)
        {
            if (used)
            {
                throw new InvalidOperationException("object already finished deserialization or serialization");
            }

            used = true;

            foreach (IDeferredSerializable obj in this.objects)
            {
                obj.FinishSerialization(output, this);
            }

            this.objects = null;
        }

        public void FinishDeserialization(Stream input)
        {
            if (used)
            {
                throw new InvalidOperationException("object already finished deserialization or serialization");
            }

            used = true;

            foreach (IDeferredSerializable obj in this.objects)
            {
                obj.FinishDeserialization(input, this);
            }

            this.objects = null;
        }
    }
}
