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
using System.Threading;

namespace PaintDotNet.Threading
{
    /// <summary>
    /// Uses the .NET ThreadPool to do our own type of thread pool. The main difference
    /// here is that we limit our usage of the thread pool, and that we can also drain
    /// the threads we have ("fence"). The default maximum number of threads is
    /// Processor.LogicalCpuCount.
    /// </summary>
    public class ThreadPool
    {
        private static ThreadPool global = new ThreadPool(2 * Processor.LogicalCpuCount);
        public static ThreadPool Global
        {
            get
            {
                return global;
            }
        }

        private ArrayList exceptions = ArrayList.Synchronized(new ArrayList());
        private bool useFXTheadPool;

        public static int MinimumCount
        {
            get
            {
                return WaitableCounter.MinimumCount;
            }
        }

        public static int MaximumCount
        {
            get
            {
                return WaitableCounter.MaximumCount;
            }
        }

        public Exception[] Exceptions
        {
            get
            {
                return (Exception[])this.exceptions.ToArray(typeof(Exception));
            }
        }

        public void ClearExceptions()
        {
            exceptions.Clear();
        }

        public void DrainExceptions()
        {
            if (this.exceptions.Count > 0)
            {
                throw new WorkerThreadException("Worker thread threw an exception", (Exception)this.exceptions[0]);
            }

            ClearExceptions();
        }

        private WaitableCounter counter;

        public ThreadPool()
            : this(Processor.LogicalCpuCount)
        {
        }

        public ThreadPool(int maxThreads)
            : this(maxThreads, true)
        {
        }

        public ThreadPool(int maxThreads, bool useFXThreadPool)
        {
            if (maxThreads < MinimumCount || maxThreads > MaximumCount)
            {
                throw new ArgumentOutOfRangeException("maxThreads", "must be between " + MinimumCount.ToString() + " and " + MaximumCount.ToString() + " inclusive");
            }

            this.counter = new WaitableCounter(maxThreads);
            this.useFXTheadPool = useFXThreadPool;
        }

        /*
        private sealed class FunctionCallTrampoline
        {
            private Delegate theDelegate;
            private object[] parameters;

            public void WaitCallback(object ignored)
            {
                theDelegate.DynamicInvoke(this.parameters);
            }

            public FunctionCallTrampoline(Delegate theDelegate, object[] parameters)
            {
                this.theDelegate = theDelegate;
                this.parameters = parameters;
            }
        }

        public void QueueFunctionCall(Delegate theDelegate, params object[] parameters)
        {
            FunctionCallTrampoline fct = new FunctionCallTrampoline(theDelegate, parameters);
            QueueUserWorkItem(fct.WaitCallback, null);
        }           
        */

        public void QueueUserWorkItem(WaitCallback callback)
        {
            QueueUserWorkItem(callback, null);
        }

        public void QueueUserWorkItem(WaitCallback callback, object state)
        {
            IDisposable token = counter.AcquireToken();
            ThreadWrapperContext twc = new ThreadWrapperContext(callback, state, token, this.exceptions);

            if (this.useFXTheadPool)
            {
                System.Threading.ThreadPool.QueueUserWorkItem(new WaitCallback(twc.ThreadWrapper), twc);
            }
            else
            {
                Thread thread = new Thread(new ThreadStart(twc.ThreadWrapper));
                thread.IsBackground = true;
                thread.Start();
            }
        }

        public bool IsDrained(uint msTimeout)
        {
            bool result = counter.IsEmpty(msTimeout);

            if (result)
            {
                Drain();
            }

            return result;
        }

        public bool IsDrained()
        {
            return IsDrained(0);
        }

        public void Drain()
        {
            counter.WaitForEmpty();
            DrainExceptions();
        }

        private sealed class ThreadWrapperContext
        {
            private WaitCallback callback;
            private object context;
            private IDisposable counterToken;
            private ArrayList exceptionsBucket;

            public ThreadWrapperContext(WaitCallback callback, object context, 
                IDisposable counterToken, ArrayList exceptionsBucket)
            {
                this.callback = callback;
                this.context = context;
                this.counterToken = counterToken;
                this.exceptionsBucket = exceptionsBucket;
            }

            public void ThreadWrapper()
            {
                using (IDisposable token = this.counterToken)
                {
                    try
                    {
                        this.callback(this.context);
                    }

                    catch (Exception ex)
                    {
                        this.exceptionsBucket.Add(ex);
                    }
                }
            }

            public void ThreadWrapper(object state)
            {
                ThreadWrapper();
            }
        }
    }
}
