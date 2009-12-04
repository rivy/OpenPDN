/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace PaintDotNet
{
    [Serializable]
    public struct Triple<T, U, V>
    {
        private T first;
        private U second;
        private V third;

        public T First
        {
            get
            {
                return this.first;
            }
        }

        public U Second
        {
            get
            {
                return this.second;
            }
        }

        public V Third
        {
            get
            {
                return this.third;
            }
        }

        public override int GetHashCode()
        {
            int firstHash;
            int secondHash;
            int thirdHash;

            if (object.ReferenceEquals(this.first, null))
            {
                firstHash = 0;
            }
            else
            {
                firstHash = this.first.GetHashCode();
            }

            if (object.ReferenceEquals(this.second, null))
            {
                secondHash = 0;
            }
            else
            {
                secondHash = this.second.GetHashCode();
            }

            if (object.ReferenceEquals(this.third, null))
            {
                thirdHash = 0;
            }
            else
            {
                thirdHash = this.third.GetHashCode();
            }

            return firstHash ^ secondHash ^ thirdHash;
        }

        public override bool Equals(object obj)
        {
            return ((obj != null) && (obj is Triple<T, U, V>) && (this == (Triple<T, U, V>)obj));
        }

        public static bool operator ==(Triple<T, U, V> lhs, Triple<T, U, V> rhs)
        {
            bool firstEqual;
            bool secondEqual;
            bool thirdEqual;

            if (object.ReferenceEquals(lhs.First, null) && object.ReferenceEquals(rhs.First, null))
            {
                firstEqual = true;
            }
            else if (object.ReferenceEquals(lhs.First, null) || object.ReferenceEquals(rhs.First, null))
            {
                firstEqual = false;
            }
            else
            {
                firstEqual = lhs.First.Equals(rhs.First);
            }

            if (object.ReferenceEquals(lhs.Second, null) && object.ReferenceEquals(rhs.Second, null))
            {
                secondEqual = true;
            }
            else if (object.ReferenceEquals(lhs.Second, null) || object.ReferenceEquals(rhs.Second, null))
            {
                secondEqual = false;
            }
            else
            {
                secondEqual = lhs.Second.Equals(rhs.Second);
            }

            if (object.ReferenceEquals(lhs.Third, null) && object.ReferenceEquals(rhs.Third, null))
            {
                thirdEqual = true;
            }
            else if (object.ReferenceEquals(lhs.Third, null) || object.ReferenceEquals(rhs.Third, null))
            {
                thirdEqual = false;
            }
            else
            {
                thirdEqual = lhs.Third.Equals(rhs.Third);
            }

            return firstEqual && secondEqual && thirdEqual;
        }

        public static bool operator !=(Triple<T, U, V> lhs, Triple<T, U, V> rhs)
        {
            return !(lhs == rhs);
        }

        public Triple(T first, U second, V third)
        {
            this.first = first;
            this.second = second;
            this.third = third;
        }

        private sealed class TripleComparer
            : IEqualityComparer<Triple<T, U, V>>
        {
            private IEqualityComparer<T> tComparer;
            private IEqualityComparer<U> uComparer;
            private IEqualityComparer<V> vComparer;

            public TripleComparer(IEqualityComparer<T> tComparer, IEqualityComparer<U> uComparer, IEqualityComparer<V> vComparer)
            {
                this.tComparer = tComparer;
                this.uComparer = uComparer;
                this.vComparer = vComparer;
            }

            public bool Equals(Triple<T, U, V> x, Triple<T, U, V> y)
            {
                return this.tComparer.Equals(x.First, y.First) && this.uComparer.Equals(x.Second, y.Second) && this.vComparer.Equals(x.Third, y.Third);
            }

            public int GetHashCode(Triple<T, U, V> obj)
            {
                return this.tComparer.GetHashCode(obj.First) ^ this.uComparer.GetHashCode(obj.Second) ^ this.vComparer.GetHashCode(obj.Third);
            }
        }

        public static IEqualityComparer<Triple<T, U, V>> CreateComparer(
            IEqualityComparer<T> tComparer, 
            IEqualityComparer<U> uComparer, 
            IEqualityComparer<V> vComparer)
        {
            return new TripleComparer(
                tComparer ?? EqualityComparer<T>.Default,
                uComparer ?? EqualityComparer<U>.Default,
                vComparer ?? EqualityComparer<V>.Default);
        }
    }
}
