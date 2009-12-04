/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

namespace PaintDotNet
{
    /// <summary>
    /// This was written as a workaround for a bug in SharpZipLib that prevents it
    /// from working right with huge Write() commands. So we split the incoming
    /// requests into smaller requests, like 4KB each or so.
    /// 
    /// However, this didn't work around the bug. But now I use this class so that
    /// I can keep tabs on a serialization or deserialization operation and have a
    /// dialog box with a progress bar.
    /// </summary>
    public sealed class SiphonStream
        : Stream
    {
        private Exception throwMe;

        private Stream stream;
        private int siphonSize;

        private object tag = null;
        public object Tag
        {
            get
            {
                return this.tag;
            }

            set
            {
                this.tag = value;
            }
        }

        /// <summary>
        /// Causes the next call to Read() or Write() to throw an IOException instead. The
        /// exception passed to this method will be used as the InnerException.
        /// </summary>
        /// <param name="throwMe"></param>
        public void Abort(Exception newThrowMe)
        {
            if (newThrowMe == null)
            {
                throw new ArgumentException("throwMe may not be null", "throwMe");
            }

            this.throwMe = newThrowMe;
        }

        public event IOEventHandler IOFinished;
        private void OnIOFinished(IOEventArgs e)
        {
            if (IOFinished != null)
            {
                IOFinished(this, e);
            }
        }

        int readAccumulator = 0;
        int writeAccumulator = 0;

        private void ReadAccumulate(int count)
        {
            if (count == -1)
            {
                if (this.readAccumulator > 0)
                {
                    OnIOFinished(new IOEventArgs(IOOperationType.Read, this.Position, this.readAccumulator));
                    this.readAccumulator = 0;
                }
            }
            else
            {
                WriteAccumulate(-1);
                this.readAccumulator += count;

                while (this.readAccumulator > this.siphonSize)
                {
                    OnIOFinished(new IOEventArgs(IOOperationType.Read, this.Position - this.readAccumulator + this.siphonSize, this.siphonSize));
                    this.readAccumulator -= this.siphonSize;
                }
            }
        }

        private void WriteAccumulate(int count)
        {
            if (count == -1)
            {
                if (this.writeAccumulator > 0)
                {
                    OnIOFinished(new IOEventArgs(IOOperationType.Write, this.Position, writeAccumulator));
                    this.writeAccumulator = 0;
                }
            }
            else
            {
                ReadAccumulate(-1);
                this.writeAccumulator += count;

                while (this.writeAccumulator > this.siphonSize)
                {
                    OnIOFinished(new IOEventArgs(IOOperationType.Write, this.Position - this.writeAccumulator + this.siphonSize, this.siphonSize));
                    this.writeAccumulator -= this.siphonSize;
                }
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (throwMe != null)
            {
                throw new IOException("Aborted", this.throwMe);
            }

            int countLeft = count;
            int cursor = offset;
            int totalAmountRead = 0;

            while (cursor < offset + count)
            {
                int count2 = Math.Min(this.siphonSize, countLeft);
                int amountRead = stream.Read(buffer, cursor, count2);
                ReadAccumulate(amountRead);
                countLeft -= amountRead;
                cursor += amountRead;
                totalAmountRead += amountRead;

                if (amountRead == 0)
                {
                    break;
                }
            }

            return totalAmountRead;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (this.throwMe != null)
            {
                throw new IOException("Aborted", this.throwMe);
            }

            int countLeft = count;
            int cursor = offset;

            while (cursor < offset + count)
            {
                int count2 = Math.Min(this.siphonSize, countLeft);
                stream.Write(buffer, cursor, count2);
                WriteAccumulate(count2);
                countLeft -= count2;
                cursor += count2;
            }
        }

        public override bool CanRead
        {
            get
            {
                return this.stream.CanRead;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return this.stream.CanWrite;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return this.stream.CanSeek;
            }
        }

        public override void Flush()
        {
            this.stream.Flush();
        }

        public override long Length
        {
            get
            {
                return this.stream.Length;
            }
        }

        public override long Position
        {
            get
            {
                return this.stream.Position;
            }
            set
            {
                this.stream.Position = value;
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return this.stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            this.stream.SetLength(value);
        }

        public SiphonStream(Stream underlyingStream)
            : this(underlyingStream, 65536)
        {
        }

        public SiphonStream(Stream underlyingStream, int siphonSize)
        {
            this.stream = underlyingStream;
            this.siphonSize = siphonSize;
        }
    }
}
