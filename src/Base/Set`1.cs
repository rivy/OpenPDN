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
using System.ComponentModel;

namespace PaintDotNet
{
    /// <summary>
    /// Represents an enumerable collection of items. Each item can only be present
    /// in the collection once. An item's identity is determined by a combination
    /// of the return values from its GetHashCode and Equals methods.
    /// This class is analagous to C++'s std::set template class.
    /// </summary>
    [Serializable]
    public class Set<T>
        : ICloneable,
          ICollection<T>
    {
        private Dictionary<T, object> dictionary;

        public static Set<T> Intersect(Set<T> set1, Set<T> set2)
        {
            Set<T> intersection = new Set<T>();

            foreach (T item in set1)
            {
                if (set2.Contains(item))
                {
                    intersection.Add(item);
                }
            }

            return intersection;
        }

        public static Set<T> Union(Set<T> set1, Set<T> set2)
        {
            Set<T> union = new Set<T>(set1);

            foreach (T item in set2)
            {
                if (!union.Contains(item))
                {
                    union.Add(item);
                }
            }

            return union;
        }

        public static Set<T> Without(Set<T> withUs, Set<T> withoutUs)
        {
            Set<T> result = new Set<T>();

            foreach (T item in withUs)
            {
                if (!withoutUs.Contains(item))
                {
                    result.Add(item);
                }
            }

            return result;
        }

        public static bool AreEqual(Set<T> set1, Set<T> set2)
        {
            if (set1.Count != set2.Count)
            {
                // Can't be equal if sizes are different
                return false;
            }

            if (set1.Count == 0)
            {
                // Empty sets are equal to each other. 
                // We know that set1.Count=set2.Count, so no need to check set2.Count for 0 as well.
                return true;
            }

            // At this point we know that either everything in set1 is in set2, or
            // that there is something in set1 which is not in set2.
            foreach (T item in set1)
            {
                if (!set2.Contains(item))
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsEqualTo(Set<T> set2)
        {
            return AreEqual(this, set2);
        }

        public bool IsSubsetOf(Set<T> set2)
        {
            foreach (T item in this)
            {
                if (!set2.Contains(item))
                {
                    return false;
                }
            }

            return true;
        }

        public Set<T> Without(Set<T> withoutUs)
        {
            return Set<T>.Without(this, withoutUs);
        }

        /// <summary>
        /// Adds an element to the set.
        /// </summary>
        /// <param name="item">The object reference to be included in the set.</param>
        /// <exception cref="ArgumentNullException">item is a null reference</exception>
        /// <exception cref="ArgumentException">item is already in the Set</exception>
        public void Add(T item)
        {
            try
            {
                this.dictionary.Add(item, null);
            }

            catch (ArgumentNullException e1)
            {
                throw e1;
            }

            catch (ArgumentException e2)
            {
                throw e2;
            }
        }

        public void AddRange(IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                Add(item);
            }
        }

        public void AddRange(params T[] items)
        {
            AddRange((IEnumerable<T>)items);
        }

        /// <summary>
        /// Removes an element from the set.
        /// </summary>
        /// <param name="item">The object reference to be excluded from the set.</param>
        /// <exception cref="ArgumentNullException">item is a null reference</exception>
        public bool Remove(T item)
        {
            try
            {
                this.dictionary.Remove(item);
                return true;
            }

            catch (ArgumentNullException)
            {
                return false;
            }
        }

        /// <summary>
        /// Determines whether the Set includes a specific element.
        /// </summary>
        /// <param name="item">The object reference to check for.</param>
        /// <returns>true if the Set includes item, false if it doesn't.</returns>
        /// <exception cref="ArgumentNullException">item is a null reference.</exception>
        public bool Contains(T item)
        {
            try
            {
                return this.dictionary.ContainsKey(item);
            }

            catch (ArgumentNullException e1)
            {
                throw e1;
            }
        }

        /// <summary>
        /// Constructs an empty Set.
        /// </summary>
        public Set()
        {
            this.dictionary = new Dictionary<T, object>();
        }

        /// <summary>
        /// Constructs a Set with data copied from the given list.
        /// </summary>
        /// <param name="cloneMe"></param>
        public Set(IEnumerable<T> cloneMe)
        {
            this.dictionary = new Dictionary<T, object>();

            foreach (T theObject in cloneMe)
            {
                Add(theObject);
            }
        }

        public Set(params T[] items)
            : this((IEnumerable<T>)items)
        {
        }

        /// <summary>
        /// Constructs a copy of a Set.
        /// </summary>
        /// <param name="copyMe">The Set to copy from.</param>
        private Set(Set<T> copyMe)
        {
            this.dictionary = new Dictionary<T, object>(copyMe.dictionary);
        }

        #region IEnumerable<T> Members

        /// <summary>
        /// Returns an IEnumerator that can be used to enumerate through the items in the Set.
        /// </summary>
        /// <returns>An IEnumerator for the Set.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.dictionary.Keys.GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this.dictionary.Keys.GetEnumerator();
        }

        #endregion

        public Set<T> Clone()
        {
            return new Set<T>(this);
        }

        #region ICloneable Members

        /// <summary>
        /// Returns a copy of the Set. The elements in the Set are copied by-reference only.
        /// </summary>
        /// <returns></returns>
        object ICloneable.Clone()
        {
            return Clone();
        }

        #endregion

        #region ICollection<T> Members

        /// <summary>
        /// Gets a value indicating whether or not the Set is synchronized (thread-safe).
        /// </summary>
        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating how many elements are contained within the Set.
        /// </summary>
        public int Count
        {
            get
            {
                return this.dictionary.Count;
            }
        }

        /// <summary>
        /// Copies the Set elements to a one-dimensional Array instance at a specified index.
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination of the objects copied from the Set. The Array must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        /// <exception cref="ArgumentNullException">array is a null reference.</exception>
        /// <exception cref="ArgumentOutOfRangeException">index is less than zero.</exception>
        /// <exception cref="ArgumentException">The array is not one-dimensional, or the array could not contain the objects copied to it.</exception>
        /// <exception cref="IndexOutOfRangeException">The Array does not have enough space, starting from the given offset, to contain all the Set's objects.</exception>
        public void CopyTo(T[] array, int index)
        {
            int i = index;

            if (array == null)
            {
                throw new ArgumentNullException("array");
            }

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            foreach (T o in this)
            {
                try
                {
                    array.SetValue(o, i);
                }

                catch (ArgumentException e1)
                {
                    throw e1;
                }

                catch (IndexOutOfRangeException e2)
                {
                    throw e2;
                }

                ++i;
            }
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the Set.
        /// </summary>
        public object SyncRoot
        {
            get
            {
                return this;
            }
        }

        #endregion

        /// <summary>
        /// Copies the elements of the Set to a new generic array.
        /// </summary>
        /// <returns>An array of object references.</returns>
        public T[] ToArray()
        {
            T[] array = new T[Count];
            int index = 0;

            foreach (T o in this)
            {
                array[index] = o;
                ++index;
            }

            return array;
        }

        #region ICollection<T> Members

        public void Clear()
        {
            this.dictionary = new Dictionary<T, object>();
        }

        public bool IsReadOnly
        {
            get 
            {
                return false;
            }
        }

        #endregion
    }
}
