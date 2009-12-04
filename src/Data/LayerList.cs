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

namespace PaintDotNet
{
    // TODO: reimplement to not use ArrayList, although we'll need to keep this class around
    //       for compatibility with .PDN's saved with older versions
    /// <summary>
    /// Basically an ArrayList, but lets the containing Document instance be
    /// notified when the list is modified so it can know that it needs to
    /// re-render itself.
    /// This implementation also enforces that any contained layer must be
    /// of the same dimensions as the document it is contained within.
    /// If you try to add a layer that is the wrong size, an exception will
    /// be thrown.
    /// </summary>
    [Serializable]
    public sealed class LayerList
        : ArrayList
    {
        private Document parent;

        /// <summary>
        /// Defines a generic "the collection is changing" event
        /// This is always followed with a more specific event (RemovedAt, for instance).
        /// </summary>
        [field: NonSerialized]
        public event EventHandler Changing;

        /// <summary>
        /// Defines a generic "the collection's contents have changed" event
        /// This is always preceded by a more specific event.
        /// </summary>
        [field: NonSerialized]
        public event EventHandler Changed;

        /// <summary>
        /// This event is raised after the collection has been cleared out;
        /// thus, when you handle this event the collection is empty.
        /// </summary>
        [field: NonSerialized]
        public EventHandler Cleared;

        /// <summary>
        /// This event is raised when a new element is inserted into the collection.
        /// The new element is at the array index specified by the Index property
        /// of the IndexEventArgs.
        /// </summary>
        [field: NonSerialized]
        public event IndexEventHandler Inserted;

        /// <summary>
        /// This event is raised before an element is removed from the collection.
        /// The index specified by the Index property of the IndexEventArgs is where
        /// the element currently is.
        /// </summary>
        [field: NonSerialized]
        public event IndexEventHandler RemovingAt;

        /// <summary>
        /// This event is raised when an element is removed from the collection.
        /// The index specified by the Index property of the IndexEventArgs is where
        /// the element used to be.
        /// </summary>
        [field: NonSerialized]
        public event IndexEventHandler RemovedAt;

        private void OnRemovingAt(int index)
        {
            if (RemovingAt != null)
            {
                RemovingAt(this, new IndexEventArgs(index));
            }
        }

        private void OnRemovedAt(int index)
        {
            if (RemovedAt != null)
            {
                RemovedAt(this, new IndexEventArgs(index));
            }
        }

        private void OnInserted(int index)
        {
            if (Inserted != null)
            {
                Inserted(this, new IndexEventArgs(index));
            }
        }

        private void OnCleared()
        {
            if (Cleared != null)
            {
                Cleared(this, EventArgs.Empty);
            }
        }

        private void OnChanging()
        {
            if (Changing != null)
            {
                Changing(this, EventArgs.Empty);
            }
        }

        private void OnChanged()
        {
            if (Changed != null)
            {
                Changed(this, EventArgs.Empty);
            }
        }

        public LayerList(Document parent)
        {
            this.parent = parent;
        }

        private void CheckLayerSize(object value)
        {
            Layer layer = (Layer)value;

            if (layer.Width != parent.Width || layer.Height != parent.Height)
            {
                throw new ArgumentException("Size of layer does not match size of containing document");
            }
        }

        public override int Add(object value)
        {
            if (!(value is BitmapLayer))
            {
                throw new ArgumentException("can only add bitmap layers");
            }

            OnChanging();
            CheckLayerSize(value);
            parent.Invalidate(); // TODO: is this necessary? shouldn't Document just hook in to the Inserted event?
            int index = base.Add(value);
            OnInserted(index);
            OnChanged();
            return index;
        }

        public override void AddRange(ICollection c)
        {
            // Implemented using Add(), and thus we don't raise our own events
            foreach (object o in c)
            {
                Add(o);
            }
        }

        public override void Clear()
        {
            OnChanging();
            base.Clear();
            OnCleared();
            OnChanged();
        }

        public override void Insert(int index, object value)
        {
            OnChanging();
            CheckLayerSize(value);
            parent.Invalidate(); // TODO: is this necessary? shouldn't Document just hook in to the Inserted event?
            base.Insert(index, value);
            OnInserted(index);
            OnChanged();
        }

        public override void InsertRange(int index, ICollection c)
        {
            // implemented using Insert, thus we don't raise our own events
            foreach (object o in c)
            {
                Insert(index, o);
            }
        }

        /*
        // Undocumented behavior of ArrayList: ArrayList.Remove actually uses ArrayList.RemoveAt!
        public override void Remove(object obj)
        {
            //OnChanging();
            int index = IndexOf(obj);
            RemoveAt(index);
            //base.Remove (obj);
            //OnRemovedAt(index);
            //OnChanged();
        }
        */

        public override void RemoveAt(int index)
        {
            OnChanging();
            OnRemovingAt(index);
            base.RemoveAt(index);
            OnRemovedAt(index);
            OnChanged();
        }

        public override void RemoveRange(int index, int count)
        {   
            // Implemented by calling RemoveAt, thus we don't raise our own events
            while (count > 0)
            {
                RemoveAt(index);
            }
        }

        public override void Reverse()
        {
            throw new NotSupportedException();
        }

        public override void Reverse(int index, int count)
        {
            throw new NotSupportedException();
        }

        public override void SetRange(int index, ICollection c)
        {
            throw new NotSupportedException();
        }

        public override void Sort()
        {
            throw new NotSupportedException();
        }

        public override void Sort(int index, int count, IComparer comparer)
        {
            throw new NotSupportedException();
        }

        public override void Sort(IComparer comparer)
        {
            throw new NotSupportedException();
        }

        public override void TrimToSize()
        {
            throw new NotSupportedException();
        }

        public Layer GetAt(int index)
        {
            return (Layer)this[index];
        }

        public void SetAt(int index, Layer newValue)
        {
            this[index] = newValue;
        }

        public override object this[int index]
        {
            get
            {
                return base[index];
            }

            set
            {
                OnChanging();
                RemoveAt(index);
                Insert(index, value);
                OnChanged();
            }
        }
    }
}
