/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;

namespace PaintDotNet
{
    [Serializable]
    public struct Quadruple<T, U, V, W>
    {
        private T first;
        private U second;
        private V third;
        private W fourth;

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

        public W Fourth
        {
            get
            {
                return this.fourth;
            }
        }

        public override int GetHashCode()
        {
            int firstHash;
            int secondHash;
            int thirdHash;
            int fourthHash;

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

            if (object.ReferenceEquals(this.fourth, null))
            {
                fourthHash = 0;
            }
            else
            {
                fourthHash = this.fourth.GetHashCode();
            }

            return firstHash ^ secondHash ^ thirdHash ^ fourthHash;
        }

        public override bool Equals(object obj)
        {
            return ((obj != null) && (obj is Quadruple<T, U, V, W>) && (this == (Quadruple<T, U, V, W>)obj));
        }

        public static bool operator ==(Quadruple<T, U, V, W> lhs, Quadruple<T, U, V, W> rhs)
        {
            bool firstEqual;
            bool secondEqual;
            bool thirdEqual;
            bool fourthEqual;

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

            if (object.ReferenceEquals(lhs.Fourth, null) && object.ReferenceEquals(rhs.Fourth, null))
            {
                fourthEqual = true;
            }
            else if (object.ReferenceEquals(lhs.Fourth, null) || object.ReferenceEquals(rhs.Fourth, null))
            {
                fourthEqual = false;
            }
            else
            {
                fourthEqual = lhs.Fourth.Equals(rhs.Fourth);
            }

            return firstEqual && secondEqual && thirdEqual && fourthEqual;
        }

        public static bool operator !=(Quadruple<T, U, V, W> lhs, Quadruple<T, U, V, W> rhs)
        {
            return !(lhs == rhs);
        }

        public Triple<T, U, V> GetTriple123()
        {
            return Triple.Create<T, U, V>(this.first, this.second, this.third);
        }

        public Triple<U, V, W> GetTriple234()
        {
            return Triple.Create<U, V, W>(this.second, this.third, this.fourth);
        }

        public Quadruple(T first, U second, V third, W fourth)
        {
            this.first = first;
            this.second = second;
            this.third = third;
            this.fourth = fourth;
        }
    }
}
