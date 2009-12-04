/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.ComponentModel;
using System.Threading;

namespace PaintDotNet
{
    internal abstract class HistoryFunction
    {
        private int criticalRegionCount = 0;
        private bool executed = false;
        private volatile bool pleaseCancel = false;
        private ISynchronizeInvoke eventSink = null;

        public bool IsAsync
        {
            get
            {
                return this.eventSink != null;
            }
        }

        public ISynchronizeInvoke EventSink
        {
            get
            {
                if (!IsAsync)
                {
                    throw new InvalidOperationException("EventSink property is only accessible when IsAsync is true");
                }

                return this.eventSink;
            }
        }

        public bool Cancellable
        {
            get
            {
                return ((this.actionFlags & ActionFlags.Cancellable) == ActionFlags.Cancellable);
            }
        }

        private ActionFlags actionFlags;
        public ActionFlags ActionFlags
        {
            get
            {
                return this.actionFlags;
            }
        }

        protected bool PleaseCancel
        {
            get
            {
                return this.pleaseCancel;
            }
        }

        private void ExecuteTrampoline(object context)
        {
            Execute((IHistoryWorkspace)context);
        }

        /// <summary>
        /// Executes the HistoryFunction.
        /// </summary>
        /// <returns>
        /// A HistoryMemento instance if an operation was performed successfully, 
        /// or null for success but no operation was performed.</returns>
        /// <exception cref="HistoryFunctionNonFatalException">
        /// There was error while performing the operation. No changes have been made to the HistoryWorkspace (no-op).
        /// </exception>
        /// <remarks>
        /// If this HistoryFunction's ActionFlags contain the HistoryFlags.Cancellable bit, then it will be executed in
        /// a background thread.
        /// </remarks>
        public HistoryMemento Execute(IHistoryWorkspace historyWorkspace)
        {
            SystemLayer.Tracing.LogFeature("HF(" + GetType().Name + ")");

            HistoryMemento returnVal = null;
            Exception exception = null;

            try
            {
                try
                {
                    if (this.executed)
                    {
                        throw new InvalidOperationException("Already executed this HistoryFunction");
                    }

                    this.executed = true;

                    returnVal = OnExecute(historyWorkspace);
                    return returnVal;
                }

                catch (ArgumentOutOfRangeException aoorex)
                {
                    if (this.criticalRegionCount > 0)
                    {
                        throw;
                    }
                    else
                    {
                        throw new HistoryFunctionNonFatalException(null, aoorex);
                    }
                }

                catch (OutOfMemoryException oomex)
                {
                    if (this.criticalRegionCount > 0)
                    {
                        throw;
                    }
                    else
                    {
                        throw new HistoryFunctionNonFatalException(null, oomex);
                    }
                }
            }

            catch (Exception ex)
            {
                if (IsAsync)
                {
                    exception = ex;
                    return returnVal;
                }
                else
                {
                    throw;
                }
            }

            finally
            {
                if (IsAsync)
                {
                    OnFinished(returnVal, exception);
                }
            }
        }

        /// <summary>
        /// Executes the function asynchronously.
        /// </summary>
        /// <param name="eventSink"></param>
        /// <param name="historyWorkspace"></param>
        /// <remarks>
        /// If you use this method to execute the function, completion will be signified
        /// using the Finished event. This will be raised no matter if the function completes
        /// successfully or not.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1500:VariableNamesShouldNotMatchFieldNames", MessageId = "eventSink")]
        public void BeginExecute(ISynchronizeInvoke eventSink, IHistoryWorkspace historyWorkspace, EventHandler<EventArgs<HistoryMemento>> finishedCallback)
        {
            if (finishedCallback == null)
            {
                throw new ArgumentNullException("finishedCallback");
            }

            if (this.eventSink != null)
            {
                throw new InvalidOperationException("already executing this function");
            }

            this.eventSink = eventSink;
            this.Finished += finishedCallback;
            System.Threading.ThreadPool.QueueUserWorkItem(new WaitCallback(ExecuteTrampoline), historyWorkspace);
        }

        /// <summary>
        /// This event is raised when the function has finished execution, whether it finished successfully or not.
        /// </summary>
        public event EventHandler<EventArgs<HistoryMemento>> Finished;

        private void OnFinished(HistoryMemento memento, Exception exception)
        {
            if (this.eventSink.InvokeRequired)
            {
                this.eventSink.BeginInvoke(
                    new Procedure<HistoryMemento, Exception>(OnFinished),
                    new object[2] { memento, exception });
            }
            else
            {
                if (exception != null)
                {
                    throw new WorkerThreadException(exception);
                }

                if (Finished != null)
                {
                    Finished(this, new EventArgs<HistoryMemento>(memento));
                }
            }
        }

        /// <summary>
        /// This event is raised when the function wants to report its progress.
        /// </summary>
        /// <remarks>
        /// This event is only ever raised if the function has the ActionFlags.ReportsProgres flag set.
        /// There is no guarantee that the value reported via this event will start at 0 or finish at 100,
        /// nor that it will report values in non-descending order. Clients of this event are advised
        /// to clamp the values reported to the range [0, 100] and to define their own policy for
        /// handling progress values that are less than the previously reported progress values.
        /// </remarks>
        public event ProgressEventHandler Progress;
        protected virtual void OnProgress(double percent)
        {
            if ((this.actionFlags & ActionFlags.ReportsProgress) != ActionFlags.ReportsProgress)
            {
                System.Diagnostics.Debug.WriteLine("This HistoryFunction does not support reporting progress, yet it called OnProgress()");
            }
            else if (Progress != null)
            {
                this.eventSink.BeginInvoke(Progress, new object[2] { this, new ProgressEventArgs(percent) });
            }
        }

        /// <summary>
        /// Call this method from within OnExecute() in order to mark areas of your code where
        /// the throwing of an OutOfMemoryException does not guarantee the coherency of data.
        /// </summary>
        /// <remarks>
        /// If OnExecute() is not within a critical region and throws an OutOfMemoryException,
        /// it will be wrapped inside a HistoryFunctionNonFatalException as the InnerException.
        /// This prevents the execution host from treating it as an operation that must
        /// close the application.
        /// Once you enter a critical region, you may not leave it. Therefore, it is recommended
        /// that you do as much preparatory work as possible before entering a critical region.
        /// Once a HistoryFunction has entered its critical region, it may not be cancelled.
        /// </remarks>
        protected void EnterCriticalRegion()
        {
            Interlocked.Increment(ref this.criticalRegionCount);
        }

        /// <summary>
        /// If the HistoryFunction is being executed asynchronously using BeginExecute() and EndExecute(),
        /// and if it also has the ActionFlags.Cancellable flag, then you may request that it cancel its
        /// long running operation by calling this method.
        /// </summary>
        /// <remarks>
        /// The request to cancel may have no effect, and the history function may still complete normally. 
        /// This may happen if the function has already entered its critical region, or if it has already
        /// completed before this method is called.
        /// </remarks>
        public void RequestCancel()
        {
            if (!Cancellable)
            {
                throw new InvalidOperationException("This HistoryFunction is not cancellable");
            }

            if (!IsAsync)
            {
                throw new InvalidOperationException("This function is not in the process of being executed asynchronously, and therefore cannot be cancelled");
            }

            this.pleaseCancel = true;
            OnCancelRequested();
        }

        public event EventHandler CancelRequested;
        protected virtual void OnCancelRequested()
        {
            if (!this.pleaseCancel)
            {
                throw new InvalidOperationException("OnCancelRequested() was called when pleaseCancel equaled false");
            }

            if (CancelRequested != null)
            {
                this.eventSink.BeginInvoke(CancelRequested, new object[2] { this, EventArgs.Empty });
            }
        }

        public abstract HistoryMemento OnExecute(IHistoryWorkspace historyWorkspace);

        public HistoryFunction(ActionFlags actionFlags)
        {
            this.actionFlags = actionFlags;
        }
    }
}
