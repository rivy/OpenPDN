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
using System.Diagnostics;
using System.Threading;

namespace PaintDotNet.Threading
{
    /// <summary>
    /// Threading primitive that allows you to "count" and to wait on two conditions:
    /// 1. Empty -- this is when we have not dished out any "tokens"
    /// 2. NotFull -- this is when we currently have 1 or more "tokens" out in the wild
    /// Note that the tokens given by Acquire() *must* be disposed. Otherwise things
    /// won't work right!
    /// </summary>
    public class WaitableCounter
    {
        /// <summary>
        /// The minimum value that may be passed to the constructor for initialization.
        /// </summary>
        public static int MinimumCount
        {
            get
            {
                return WaitHandleArray.MinimumCount;
            }
        }

        /// <summary>
        /// The maximum value that may be passed to the construct for initialization.
        /// </summary>
        public static int MaximumCount
        {
            get
            {
                return WaitHandleArray.MaximumCount;
            }
        }

        private sealed class CounterToken
            : IDisposable
        {
            private WaitableCounter parent;
            private int index;

            public int Index
            {
                get
                {
                    return this.index;
                }
            }

            public CounterToken(WaitableCounter parent, int index)
            {
                this.parent = parent;
                this.index = index;
            }

            public void Dispose()
            {
                parent.Release(this);
            }
        }

        private WaitHandleArray freeEvents;    // each of these is signaled (set) when the corresponding slot is 'free'
        private WaitHandleArray inUseEvents;   // each of these is signaled (set) when the corresponding slot is 'in use'

        private object theLock;

        public WaitableCounter(int maxCount)
        {
            if (maxCount < 1 || maxCount > 64)
            {
                throw new ArgumentOutOfRangeException("maxCount", "must be between 1 and 64, inclusive");
            }

            this.freeEvents = new WaitHandleArray(maxCount);
            this.inUseEvents = new WaitHandleArray(maxCount);

            for (int i = 0; i < maxCount; ++i)
            {
                this.freeEvents[i] = new ManualResetEvent(true);
                this.inUseEvents[i] = new ManualResetEvent(false);
            }

            this.theLock = new object();
        }

        private void Release(CounterToken token)
        {
            ((ManualResetEvent)this.inUseEvents[token.Index]).Reset();
            ((ManualResetEvent)this.freeEvents[token.Index]).Set();
        }

        public IDisposable AcquireToken()
        {
            lock (this.theLock)
            {
                int index = WaitForNotFull();
                ((ManualResetEvent)this.freeEvents[index]).Reset();
                ((ManualResetEvent)this.inUseEvents[index]).Set();
                return new CounterToken(this, index);
            }
        }

        public bool IsEmpty()
        {
            return IsEmpty(0);
        }

        public bool IsEmpty(uint msTimeout)
        {
            return freeEvents.AreAllSignaled(msTimeout);
        }

        public void WaitForEmpty()
        {
            freeEvents.WaitAll();
        }

        public int WaitForNotFull()
        {
            int returnVal = freeEvents.WaitAny();
            return returnVal;
        }

    }
}
