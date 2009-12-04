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
using System.Drawing;

namespace PaintDotNet
{
    public sealed class Vector<T>
    {
        private int count = 0;
        private T[] array;
        
        public Vector()
            : this(10)
        {
        }

        public Vector(int capacity)
        {
            this.array = new T[capacity];
        }

        public Vector(IEnumerable<T> copyMe)
        {
            foreach (T t in copyMe)
            {
                Add(t);
            }
        }

        public void Add(T pt)
        {
            if (this.count >= this.array.Length)
            {
                Grow(this.count + 1);
            }

            this.array[this.count] = pt;
            ++this.count;
        }

        public void Insert(int index, T item)
        {
            if (this.count >= this.array.Length)
            {
                Grow(this.count + 1);
            }

            ++this.count;

            for (int i = this.count - 1; i >= index + 1; --i)
            {
                this.array[i] = this.array[i - 1];
            }

            this.array[index] = item;
        }

        public void Clear()
        {
            this.count = 0;
        }

        public T this[int index]
        {
            get
            {
                return Get(index);
            }

            set
            {
                Set(index, value);
            }
        }

        public T Get(int index)
        {
            if (index < 0 || index >= this.count)
            {
                throw new ArgumentOutOfRangeException("index", index, "0 <= index < count");
            }

            return this.array[index];
        }

        public unsafe T GetUnchecked(int index)
        {
            return this.array[index];
        }

        public void Set(int index, T pt)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index", index, "0 <= index");
            }

            if (index >= this.array.Length)
            {
                Grow(index + 1);
            }

            this.array[index] = pt;
        }

        public int Count
        {
            get
            {
                return this.count;
            }
        }

        private void Grow(int min)
        {
            int newSize = this.array.Length;

            if (newSize <= 0)
            {
                newSize = 1;
            }

            while (newSize < min)
            {
                newSize = 1 + ((newSize * 10) / 8);
            }

            T[] replacement = new T[newSize];

            for (int i = 0; i < this.count; i++)
            {
                replacement[i] = this.array[i];
            }

            this.array = replacement;
        }

        public T[] ToArray()
        {
            T[] ret = new T[this.count];

            for (int i = 0; i < this.count; i++)
            {
                ret[i] = this.array[i];
            }

            return ret;
        }

        public unsafe T[] UnsafeArray
        {
            get
            {
                return this.array;
            }
        }

        /// <summary>
        /// Gets direct access to the array held by the Vector.
        /// The caller must not modify the array.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="length">The actual number of items stored in the array. This number will be less than or equal to array.Length.</param>
        /// <remarks>This method is supplied strictly for performance-critical purposes.</remarks>
        public unsafe void GetArrayReadOnly(out T[] arrayResult, out int lengthResult)
        {
            arrayResult = this.array;
            lengthResult = this.count;
        }
    }
}
