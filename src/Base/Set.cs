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
    public class Set
        : IEnumerable, 
          ICloneable, 
          ICollection
    {
        private Hashtable hashtable;

        /// <summary>
        /// Adds an element to the set.
        /// </summary>
        /// <param name="item">The object reference to be included in the set.</param>
        /// <exception cref="ArgumentNullException">item is a null reference</exception>
        /// <exception cref="ArgumentException">item is already in the Set</exception>
        public void Add(object item)
        {
            try
            {
                hashtable.Add(item, null);
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

        /// <summary>
        /// Removes an element from the set.
        /// </summary>
        /// <param name="item">The object reference to be excluded from the set.</param>
        /// <exception cref="ArgumentNullException">item is a null reference</exception>
        public void Remove(object item)
        {
            try
            {
                hashtable.Remove(item);
            }

            catch (ArgumentNullException e1)
            {
                throw e1;
            }
        }

        /// <summary>
        /// Determines whether the Set includes a specific element.
        /// </summary>
        /// <param name="item">The object reference to check for.</param>
        /// <returns>true if the Set includes item, false if it doesn't.</returns>
        /// <exception cref="ArgumentNullException">item is a null reference.</exception>
        public bool Contains(object item)
        {
            try
            {
                return hashtable.ContainsKey(item);
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
            this.hashtable = new Hashtable();
        }

        /// <summary>
        /// Constructs a Set with data copied from the given list.
        /// </summary>
        /// <param name="cloneMe"></param>
        public Set(IEnumerable cloneMe)
        {
            this.hashtable = new Hashtable();

            foreach (object theObject in cloneMe)
            {
                Add(theObject);
            }
        }

        public static Set<T> Create<T>(params T[] items)
        {
            return new Set<T>(items);
        }

        /// <summary>
        /// Constructs a copy of a Set.
        /// </summary>
        /// <param name="copyMe">The Set to copy from.</param>
        private Set(Set copyMe)
        {
            hashtable = (Hashtable)copyMe.Clone();
        }

        #region IEnumerable Members

        /// <summary>
        /// Returns an IEnumerator that can be used to enumerate through the items in the Set.
        /// </summary>
        /// <returns>An IEnumerator for the Set.</returns>
        public IEnumerator GetEnumerator()
        {
            return hashtable.Keys.GetEnumerator();
        }

        #endregion

        #region ICloneable Members

        /// <summary>
        /// Returns a copy of the Set. The elements in the Set are copied by-reference only.
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return new Set(this);
        }

        #endregion

        #region ICollection Members

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
                return hashtable.Count;
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
        public void CopyTo(Array array, int index)
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

            foreach (object o in this)
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
        public object[] ToArray()
        {
            object[] array = new object[Count];
            int index = 0;

            foreach (object o in this)
            {
                array[index] = o;
                ++index;
            }

            return array;
        }

        /// <summary>
        /// Copies the elements of the Set to a new array of the requested type.
        /// </summary>
        /// <param name="type">The Type of array to create and copy elements to.</param>
        /// <returns>An array of objects of the requested type.</returns>
        public Array ToArray(Type type)
        {
            Array array = Array.CreateInstance(type, Count);
            int index = 0;
            
            foreach (object o in this)
            {
                array.SetValue(o, index);
                ++index;
            }
            
            return array;
        }
    }
}
