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
using System.Globalization;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
using System.Threading;

namespace PaintDotNet
{
    /// <summary>
    /// Manages an arbitrarily sized block of memory. You can also create child MemoryBlocks
    /// which reference a portion of the memory allocated by a parent MemoryBlock. If the parent
    /// is disposed, the children will not be valid.
    /// </summary>
    [Serializable]
    public unsafe sealed class MemoryBlock
        : IDisposable,
          ICloneable,
          IDeferredSerializable
    {
        // serialize 1MB at a time: this enables us to serialize very large blocks, and to conserve memory while doing so
        private const int serializationChunkSize = 1048576; 

        // blocks this size or larger are allocated with AllocateLarge (VirtualAlloc) instead of Allocate (HeapAlloc)
        private const long largeBlockThreshold = 65536;

        private long length;

        // if parentBlock == null, then we allocated the pointer and are responsible for deallocating it
        // if parentBlock != null, then the parentBlock allocated it, not us
        [NonSerialized]
        private void *voidStar;

        [NonSerialized]
        private bool valid; // if voidStar is null, and this is false, we know that it's null because allocation failed. otherwise we have a real error

        private MemoryBlock parentBlock = null;

        [NonSerialized]
        private IntPtr bitmapHandle = IntPtr.Zero; // if allocated using the "width, height" constructor, we keep track of a bitmap handle
        private int bitmapWidth;
        private int bitmapHeight;

        private bool disposed = false;

        public MemoryBlock Parent
        {
            get
            {
                return this.parentBlock;
            }
        }

        public long Length
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("MemoryBlock");
                }

                return length;
            }
        }

        public IntPtr Pointer
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("MemoryBlock");
                }

                return new IntPtr(voidStar);
            }
        }

        public IntPtr BitmapHandle
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("MemoryBlock");
                }

                return this.bitmapHandle;
            }
        }

        public void *VoidStar
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("MemoryBlock");
                }

                return voidStar;
            }
        }

        public byte this[long index]
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("MemoryBlock");
                }

                if (index < 0 || index >= length)
                {
                    throw new ArgumentOutOfRangeException("index must be positive and less than Length");
                }

                unsafe
                {
                    return ((byte *)this.VoidStar)[index];
                }
            }

            set
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("MemoryBlock");
                }

                if (index < 0 || index >= length)
                {
                    throw new ArgumentOutOfRangeException("index must be positive and less than Length");
                }

                unsafe
                {
                    ((byte *)this.VoidStar)[index] = value;
                }
            }
        }

        public bool MaySetAllowWrites
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("MemoryBlock");
                }

                if (this.parentBlock != null)
                {
                    return this.parentBlock.MaySetAllowWrites;
                }
                else
                {
                    return (this.length >= largeBlockThreshold && this.bitmapHandle != IntPtr.Zero);
                }
            }
        }

        /// <summary>
        /// Sets a flag indicating whether the memory that this instance of MemoryBlock points to
        /// may be written to.
        /// </summary>
        /// <remarks>
        /// This flag is meant to be set to false for short periods of time. The value of this
        /// property is not persisted with serialization.
        /// </remarks>
        public bool AllowWrites
        {
            set
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("MemoryBlock");
                }

                if (!MaySetAllowWrites)
                {
                    throw new InvalidOperationException("May not set write protection on this memory block");
                }

                Memory.ProtectBlockLarge(new IntPtr(this.voidStar), (ulong)this.length, true, value);
            }
        }

        /// <summary>
        /// Copies bytes from one area of memory to another. Since this function works
        /// with MemoryBlock instances, it does bounds checking.
        /// </summary>
        /// <param name="dst">The MemoryBlock to copy bytes to.</param>
        /// <param name="dstOffset">The offset within dst to copy bytes to.</param>
        /// <param name="src">The MemoryBlock to copy bytes from.</param>
        /// <param name="srcOffset">The offset within src to copy bytes from.</param>
        /// <param name="length">The number of bytes to copy.</param>
        public static void CopyBlock(MemoryBlock dst, long dstOffset, MemoryBlock src, long srcOffset, long length)
        {
            if ((dstOffset + length > dst.length) || (srcOffset + length > src.length))
            {
                throw new ArgumentOutOfRangeException("", "copy ranges were out of bounds");
            }

            if (dstOffset < 0)
            {
                throw new ArgumentOutOfRangeException("dstOffset", dstOffset, "must be >= 0");
            }
             
            if (srcOffset < 0)
            {
                throw new ArgumentOutOfRangeException("srcOffset", srcOffset, "must be >= 0");
            }

            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length", length, "must be >= 0");
            }

            void *dstPtr = (void *)((byte *)dst.VoidStar + dstOffset);
            void *srcPtr = (void *)((byte *)src.VoidStar + srcOffset);
            Memory.Copy(dstPtr, srcPtr, (ulong)length);
        }

        /// <summary>
        /// Creates a new parent MemoryBlock and copies our contents into it
        /// </summary>
        object ICloneable.Clone()
        {
            if (disposed)
            {
                throw new ObjectDisposedException("MemoryBlock");
            }

            return (object)Clone();
        }

        /// <summary>
        /// Creates a new parent MemoryBlock and copies our contents into it
        /// </summary>
        public MemoryBlock Clone()
        {
            if (disposed)
            {
                throw new ObjectDisposedException("MemoryBlock");
            }

            MemoryBlock dupe = new MemoryBlock(this.length);
            CopyBlock(dupe, 0, this, 0, length);
            return dupe;
        }

        /// <summary>
        /// Creates a new MemoryBlock instance and allocates the requested number of bytes.
        /// </summary>
        /// <param name="bytes"></param>
        public MemoryBlock(long bytes)
        {
            if (bytes <= 0)
            {
                throw new ArgumentOutOfRangeException("bytes", bytes, "Bytes must be greater than zero");
            }

            this.length = bytes;
            this.parentBlock = null;
            this.voidStar = Allocate(bytes).ToPointer();
            this.valid = true;
        }

        public MemoryBlock(int width, int height)
        {
            if (width < 0 && height < 0)
            {
                throw new ArgumentOutOfRangeException("width/height", new Size(width, height), "width and height must be >= 0");
            }
            else if (width < 0)
            {
                throw new ArgumentOutOfRangeException("width", width, "width must be >= 0");
            } 
            else if (height < 0)
            {
                throw new ArgumentOutOfRangeException("height", width, "height must be >= 0");
            }

            this.length = width * height * ColorBgra.SizeOf;
            this.parentBlock = null;
            this.voidStar = Allocate(width, height, out this.bitmapHandle).ToPointer();
            this.valid = true;
            this.bitmapWidth = width;
            this.bitmapHeight = height;
        }

        /// <summary>
        /// Creates a new MemoryBlock instance that refers to part of another MemoryBlock.
        /// The other MemoryBlock is the parent, and this new instance is the child.
        /// </summary>
        public unsafe MemoryBlock(MemoryBlock parentBlock, long offset, long length)
        {
            if (offset + length > parentBlock.length)
            {
                throw new ArgumentOutOfRangeException();
            }   

            this.parentBlock = parentBlock;
            byte *bytePointer = (byte *)parentBlock.VoidStar;
            bytePointer += offset;
            this.voidStar = (void *)bytePointer;
            this.valid = true;
            this.length = length;
        }

        ~MemoryBlock()
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
            if (!disposed)
            {
                disposed = true;

                if (disposing)
                {
                }

                if (this.valid && parentBlock == null)
                {
                    if (this.bitmapHandle != IntPtr.Zero)
                    {
                        Memory.FreeBitmap(this.bitmapHandle, this.bitmapWidth, this.bitmapHeight);
                    }
                    else if (this.length >= largeBlockThreshold)
                    {
                        Memory.FreeLarge(new IntPtr(voidStar), (ulong)this.length);
                    }
                    else
                    {
                        Memory.Free(new IntPtr(voidStar));
                    }
                }

                parentBlock = null;
                voidStar = null;
                this.valid = false;
            }
        }

        private static IntPtr Allocate(int width, int height, out IntPtr handle)
        {
            return Allocate(width, height, out handle, true);
        }

        private static IntPtr Allocate(int width, int height, out IntPtr handle, bool allowRetry)
        {
            IntPtr block;

            try
            {
                block = Memory.AllocateBitmap(width, height, out handle);
            }

            catch (OutOfMemoryException)
            {
                if (allowRetry)
                {
                    Utility.GCFullCollect();
                    return Allocate(width, height, out handle, false);
                }
                else
                {
                    throw;
                }
            }

            return block;
        }

        private static IntPtr Allocate(long bytes)
        {
            return Allocate(bytes, true);
        }

        private static IntPtr Allocate(long bytes, bool allowRetry)
        {
            IntPtr block;

            try
            {
                if (bytes >= largeBlockThreshold)
                {
                    block = Memory.AllocateLarge((ulong)bytes);
                }
                else
                {
                    block = Memory.Allocate((ulong)bytes);
                }
            }

            catch (OutOfMemoryException)
            {
                if (allowRetry)
                {
                    Utility.GCFullCollect();
                    return Allocate(bytes, false);
                }
                else
                {
                    throw;
                }
            }

            return block;
        }

        public byte[] ToByteArray()
        {
            return ToByteArray(0, this.length);
        }

        public byte[] ToByteArray(long startOffset, long lengthDesired)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("MemoryBlock");
            }

            if (startOffset < 0)
            {
                throw new ArgumentOutOfRangeException("startOffset", "must be greater than or equal to zero");
            }

            if (lengthDesired < 0)
            {
                throw new ArgumentOutOfRangeException("length", "must be greater than or equal to zero");
            }

            if (startOffset + lengthDesired > this.length)
            {
                throw new ArgumentOutOfRangeException("startOffset, length", "startOffset + length must be less than Length");
            }

            byte[] dstArray = new byte[lengthDesired];
            byte *pbSrcArray = (byte *)this.VoidStar;

            fixed (byte *pbDstArray = dstArray)
            {
                Memory.Copy(pbDstArray, pbSrcArray + startOffset, (ulong)lengthDesired);
            }

            return dstArray;
        }

        private class OurSerializationException
            : SerializationException
        {
            public OurSerializationException()
            {
            }

            public OurSerializationException(string message)
                : base(message)
            {
            }

            public OurSerializationException(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {
            }

            public OurSerializationException(string message, Exception innerException)
                : base(message, innerException)
            {
            }
        }

        private MemoryBlock(SerializationInfo info, StreamingContext context)
        {
            disposed = false;

            // Try to read a 64-bit value, and for backwards compatibility fall back on a 32-bit value.
            try
            {
                 this.length = info.GetInt64("length64");
            }

            catch (SerializationException)
            {
                this.length = (long)info.GetInt32("length");
            }

            try
            {
                this.bitmapWidth = (int)info.GetInt32("bitmapWidth");
                this.bitmapHeight = (int)info.GetInt32("bitmapHeight");

                if (this.bitmapWidth != 0 || this.bitmapHeight != 0)
                {
                    long bytes = (long)this.bitmapWidth * (long)this.bitmapHeight * (long)ColorBgra.SizeOf;

                    if (bytes != this.length)
                    {
                        throw new ApplicationException("Invalid file format: width * height * 4 != length");
                    }
                }
            }

            catch (SerializationException)
            {
                this.bitmapWidth = 0;
                this.bitmapHeight = 0;
            }

            bool hasParent = info.GetBoolean("hasParent");

            if (hasParent)
            {
                this.parentBlock = (MemoryBlock)info.GetValue("parentBlock", typeof(MemoryBlock));

                // Try to read a 64-bit value, and for backwards compatibility fall back on a 32-bit value.
                long parentOffset;

                try
                {
                    parentOffset = info.GetInt64("parentOffset64");
                }

                catch (SerializationException)
                {
                    parentOffset = (long)info.GetInt32("parentOffset");
                }

                this.voidStar = (void *)((byte *)parentBlock.VoidStar + parentOffset);
                this.valid = true;
            }
            else
            {
                DeferredFormatter deferredFormatter = context.Context as DeferredFormatter;
                bool deferred = false;

                // Was this stream serialized with deferment?
                foreach (SerializationEntry entry in info)
                {
                    if (entry.Name == "deferred")
                    {
                        deferred = (bool)entry.Value;
                        break;
                    }
                }

                if (deferred && deferredFormatter != null)
                {
                    // The newest PDN files use deferred deserialization. This lets us read straight from the stream,
                    // minimizing memory use and adding the potential for multithreading
                    // Deserialization will complete in IDeferredDeserializer.FinishDeserialization()
                    deferredFormatter.AddDeferredObject(this, this.length);
                }
                else if (deferred && deferredFormatter == null)
                {
                    throw new InvalidOperationException("stream has deferred serialization streams, but a DeferredFormatter was not provided");
                }
                else
                {
                    this.voidStar = Allocate(this.length).ToPointer();
                    this.valid = true;

                    // Non-deferred format serializes one big byte[] chunk. This is also
                    // how PDN files were saved with v2.1 Beta 2 and before.
                    byte[] array = (byte[])info.GetValue("pointerData", typeof(byte[]));

                    fixed (byte *pbArray = array)
                    {
                        Memory.Copy(this.VoidStar, (void *)pbArray, (ulong)array.LongLength);
                    }
                }
            }
        }

        public void WriteFormat1Data(SerializationInfo info, StreamingContext context)
        {
            byte[] bytes = this.ToByteArray();
            info.AddValue("pointerData", bytes, typeof(byte[]));
        }

        public void WriteFormat2Data(SerializationInfo info, StreamingContext context)
        {
            DeferredFormatter deferred = context.Context as DeferredFormatter;

            if (deferred != null)
            {
                info.AddValue("deferred", true);
                deferred.AddDeferredObject(this, this.length);
            }
            else
            {
                WriteFormat1Data(info, context);
            }
        }

        private static void WriteUInt(Stream output, UInt32 theUInt)
        {
            output.WriteByte((byte)((theUInt >> 24) & 0xff));
            output.WriteByte((byte)((theUInt >> 16) & 0xff));
            output.WriteByte((byte)((theUInt >> 8) & 0xff));
            output.WriteByte((byte)(theUInt & 0xff));
        }

        private static uint ReadUInt(Stream output)
        {
            uint theUInt = 0;

            for (int i = 0; i < 4; ++i)
            {
                theUInt <<= 8;

                int theByte = output.ReadByte();

                if (theByte == -1)
                {
                    throw new EndOfStreamException();
                }

                theUInt += (UInt32)theByte;
            }

            return theUInt;
        }

        // Data starts with:
        // 1 byte: formatVersion
        //         0 for compressed w/ gzip chunks
        //         1 for non-compressed chunks
        //
        // IF formatVersion == 0:
        //   4 byte uint: chunkSize
        // 
        //   then compute: chunkCount = (length + chunkSize - 1) / chunkSize
        //   'length' is written as part of the usual .NET Serialization process in GetObjectData()
        //
        //   Each chunk has the following format:
        //   4 byte uint: chunkNumber
        //   4 byte uint: raw dataSize 'N' bytes (this will expand to more bytes after decompression)
        //   N bytes: data
        //
        //   The chunks may appear in any order; that is, chunk N is not necessarily followed by N+1,
        //   nor is it necessarily preceded by N-1.
        //
        // uints are written in big-endian order.

        private class DecompressChunkParms
        {
            private byte[] compressedBytes;
            private uint chunkSize;
            private long chunkOffset;
            private DeferredFormatter deferredFormatter;
            private ArrayList exceptions;

            public byte[] CompressedBytes
            {
                get
                {
                    return compressedBytes;
                }
            }

            public uint ChunkSize
            {
                get
                {
                    return chunkSize;
                }
            }

            public long ChunkOffset
            {
                get
                {
                    return chunkOffset;
                }
            }

            public DeferredFormatter DeferredFormatter
            {
                get
                {
                    return deferredFormatter;
                }
            }

            public ArrayList Exceptions
            {
                get
                {
                    return exceptions;
                }
            }

            public DecompressChunkParms(byte[] compressedBytes, uint chunkSize, long chunkOffset, DeferredFormatter deferredFormatter, ArrayList exceptions)
            {
                this.compressedBytes = compressedBytes;
                this.chunkSize = chunkSize;
                this.chunkOffset = chunkOffset;
                this.deferredFormatter = deferredFormatter;
                this.exceptions = exceptions;
            }
        }

        private void DecompressChunk(object context)
        {
            DecompressChunkParms parms = (DecompressChunkParms)context;

            try
            {
                DecompressChunk(parms.CompressedBytes, parms.ChunkSize, parms.ChunkOffset, parms.DeferredFormatter);
            }

            catch (Exception ex)
            {
                parms.Exceptions.Add(ex);
            }
        }

        private void DecompressChunk(byte[] compressedBytes, uint chunkSize, long chunkOffset, DeferredFormatter deferredFormatter)
        {
            // decompress data
            MemoryStream compressedStream = new MemoryStream(compressedBytes, false);
            GZipStream gZipStream = new GZipStream(compressedStream, CompressionMode.Decompress, true);

            byte[] decompressedBytes = new byte[chunkSize];
                
            int dstOffset = 0;
            while (dstOffset < decompressedBytes.Length)
            {
                int bytesRead = gZipStream.Read(decompressedBytes, dstOffset, (int)chunkSize - dstOffset);

                if (bytesRead == 0)
                {
                    throw new SerializationException("ran out of data to decompress");
                }

                dstOffset += bytesRead;
                deferredFormatter.ReportBytes((long)bytesRead);
            }

            // copy data
            fixed (byte *pbDecompressedBytes = decompressedBytes)
            {
                byte *pbDst = (byte *)this.VoidStar + chunkOffset;
                Memory.Copy(pbDst, pbDecompressedBytes, (ulong)chunkSize);
            }
        }

        void IDeferredSerializable.FinishDeserialization(Stream input, DeferredFormatter context)
        {
            // Allocate the memory
            if (this.bitmapWidth != 0 && this.bitmapHeight != 0)
            {
                this.voidStar = Allocate(this.bitmapWidth, this.bitmapHeight, out this.bitmapHandle).ToPointer();
                this.valid = true;
            }
            else
            {
                this.voidStar = Allocate(this.length).ToPointer();
                this.valid = true;
            }
            
            // formatVersion should equal 0
            int formatVersion = input.ReadByte();

            if (formatVersion == -1)
            {
                throw new EndOfStreamException();
            }

            if (formatVersion != 0 && formatVersion != 1)
            {
                throw new SerializationException("formatVersion was neither zero nor one");
            }

            // chunkSize
            uint chunkSize = ReadUInt(input);

            PaintDotNet.Threading.ThreadPool threadPool = new PaintDotNet.Threading.ThreadPool(Processor.LogicalCpuCount);
            ArrayList exceptions = new ArrayList(Processor.LogicalCpuCount);
            WaitCallback callback = new WaitCallback(DecompressChunk);

            // calculate chunkCount
            uint chunkCount = (uint)((this.length + (long)chunkSize - 1) / (long)chunkSize);
            bool[] chunksFound = new bool[chunkCount];

            for (uint i = 0; i < chunkCount; ++i)
            {
                // chunkNumber
                uint chunkNumber = ReadUInt(input);

                if (chunkNumber >= chunkCount)
                {
                    throw new SerializationException("chunkNumber read from stream is out of bounds");
                }

                if (chunksFound[chunkNumber])
                {
                    throw new SerializationException("already encountered chunk #" + chunkNumber.ToString());
                }

                chunksFound[chunkNumber] = true;
                
                // dataSize
                uint dataSize = ReadUInt(input);

                // calculate chunkOffset
                long chunkOffset = (long)chunkNumber * (long)chunkSize;

                // calculate decompressed chunkSize
                uint thisChunkSize = Math.Min(chunkSize, (uint)(this.length - chunkOffset));

                // bounds checking
                if (chunkOffset < 0 || chunkOffset >= this.length || chunkOffset + thisChunkSize > this.length)
                {
                    throw new SerializationException("data was specified to be out of bounds");
                }

                // read compressed data
                byte[] compressedBytes = new byte[dataSize];
                Utility.ReadFromStream(input, compressedBytes, 0, compressedBytes.Length);

                // decompress data
                if (formatVersion == 0)
                {
                    DecompressChunkParms parms = new DecompressChunkParms(compressedBytes, thisChunkSize, chunkOffset, context, exceptions);
                    threadPool.QueueUserWorkItem(callback, parms);
                }
                else
                {
                    fixed (byte *pbSrc = compressedBytes)
                    {
                        Memory.Copy((void *)((byte *)this.VoidStar + chunkOffset), (void *)pbSrc, thisChunkSize);
                    }
                }
            }

            threadPool.Drain();

            if (exceptions.Count > 0)
            {
                throw new SerializationException("Exception thrown by worker thread", (Exception)exceptions[0]);
            }
        }

        private class SerializeChunkParms
        {
            private Stream output;
            private uint chunkNumber;
            private long chunkOffset;
            private long chunkSize;
            private object previousLock;
            private DeferredFormatter deferredFormatter;
            private ArrayList exceptions;

            public Stream Output
            {
                get
                {
                    return output;
                }
            }

            public uint ChunkNumber
            {
                get
                {
                    return chunkNumber;
                }
            }

            public long ChunkOffset
            {
                get
                {
                    return chunkOffset;
                }
            }

            public long ChunkSize
            {
                get
                {
                    return chunkSize;
                }
            }

            public object PreviousLock
            {
                get
                {
                    return (previousLock == null) ? this : previousLock;
                }
            }

            public DeferredFormatter DeferredFormatter
            {
                get
                {
                    return deferredFormatter;
                }
            }

            public ArrayList Exceptions
            {
                get
                {
                    return exceptions;
                }
            }

            public SerializeChunkParms(Stream output, uint chunkNumber, long chunkOffset, long chunkSize, object previousLock,
                DeferredFormatter deferredFormatter, ArrayList exceptions)
            {
                this.output = output;
                this.chunkNumber = chunkNumber;
                this.chunkOffset = chunkOffset;
                this.chunkSize = chunkSize;
                this.previousLock = previousLock;
                this.deferredFormatter = deferredFormatter;
                this.exceptions = exceptions;
            }
        }

        private void SerializeChunk(object context)
        {
            SerializeChunkParms parms = (SerializeChunkParms)context;

            try
            {
                SerializeChunk(parms.Output, parms.ChunkNumber, parms.ChunkOffset, parms.ChunkSize, parms, parms.PreviousLock, parms.DeferredFormatter);
            }

            catch (Exception ex)
            {
                parms.Exceptions.Add(ex);
            }
        }                    

        private void SerializeChunk(Stream output, uint chunkNumber, long chunkOffset, long chunkSize, 
            object currentLock, object previousLock, DeferredFormatter deferredFormatter)
        {
            lock (currentLock)
            {
                bool useCompression = deferredFormatter.UseCompression;

                MemoryStream chunkOutput = new MemoryStream();

                // chunkNumber
                WriteUInt(chunkOutput, chunkNumber);

                // dataSize
                long rewindPos = chunkOutput.Position;
                WriteUInt(chunkOutput, 0); // we'll rewind and write this later
                long startPos = chunkOutput.Position;

                // Compress data
                byte[] array = new byte[chunkSize];

                fixed (byte *pbArray = array)
                {
                    Memory.Copy(pbArray, (byte *)this.VoidStar + chunkOffset, (ulong)chunkSize);
                }

                chunkOutput.Flush();

                if (useCompression)
                {
                    GZipStream gZipStream = new GZipStream(chunkOutput, CompressionMode.Compress, true);
                    gZipStream.Write(array, 0, array.Length);
                    gZipStream.Close();
                }
                else
                {
                    chunkOutput.Write(array, 0, array.Length);
                }

                long endPos = chunkOutput.Position;

                // dataSize
                chunkOutput.Position = rewindPos;
                uint dataSize = (uint)(endPos - startPos);
                WriteUInt(chunkOutput, dataSize);

                // bytes
                chunkOutput.Flush();

                lock (previousLock)
                {
                    output.Write(chunkOutput.GetBuffer(), 0, (int)chunkOutput.Length);
                    deferredFormatter.ReportBytes(chunkSize);
                }
            }
        }

        void IDeferredSerializable.FinishSerialization(Stream output, DeferredFormatter context)
        {
            bool useCompression = context.UseCompression;

            // formatVersion = 0 for GZIP, or 1 for uncompressed
            if (useCompression)
            {
                output.WriteByte(0);
            }
            else
            {
                output.WriteByte(1);
            }

            // chunkSize
            WriteUInt(output, serializationChunkSize);

            uint chunkCount = (uint)((this.length + (long)serializationChunkSize - 1) / (long)serializationChunkSize);

            PaintDotNet.Threading.ThreadPool threadPool = new PaintDotNet.Threading.ThreadPool(Processor.LogicalCpuCount);
            ArrayList exceptions = ArrayList.Synchronized(new ArrayList(Processor.LogicalCpuCount));
            WaitCallback callback = new WaitCallback(SerializeChunk);

            object previousLock = null;
            for (uint chunk = 0; chunk < chunkCount; ++chunk)
            {
                long chunkOffset = (long)chunk * (long)serializationChunkSize;
                uint chunkSize = Math.Min((uint)serializationChunkSize, (uint)(this.length - chunkOffset));
                SerializeChunkParms parms = new SerializeChunkParms(output, chunk, chunkOffset, chunkSize, previousLock, context, exceptions);
                threadPool.QueueUserWorkItem(callback, parms);
                previousLock = parms;
            }

            threadPool.Drain();
            output.Flush();

            if (exceptions.Count > 0)
            {
                throw new SerializationException("Exception thrown by worker thread", (Exception)exceptions[0]);
            }

            return;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("MemoryBlock");
            }

            info.AddValue("length64", this.length);

            if (this.bitmapWidth != 0 || this.bitmapHeight != 0 || this.bitmapHandle != IntPtr.Zero)
            {
                info.AddValue("bitmapWidth", bitmapWidth);
                info.AddValue("bitmapHeight", bitmapHeight);
            }

            info.AddValue("hasParent", this.parentBlock != null);

            if (parentBlock == null)
            {
                WriteFormat2Data(info, context);
            }
            else
            {
                info.AddValue("parentBlock", parentBlock, typeof(MemoryBlock));
                info.AddValue("parentOffset64", (long)((byte *)voidStar - (byte *)parentBlock.VoidStar));
            }
        }
    }
}
