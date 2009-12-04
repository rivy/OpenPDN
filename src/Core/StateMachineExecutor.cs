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
using System.ComponentModel;
using System.Threading;

namespace PaintDotNet
{
    public sealed class StateMachineExecutor
        : IDisposable
    {
        private bool disposed = false;
        private bool isStarted = false;
        private Thread stateMachineThread;
        private Exception threadException;
        private StateMachine stateMachine;
        private ISynchronizeInvoke syncContext;
        private ManualResetEvent stateMachineInitialized = new ManualResetEvent(false);
        private ManualResetEvent stateMachineNotBusy = new ManualResetEvent(false); // non-signaled when busy, signaled when not busy
        private ManualResetEvent inputAvailable = new ManualResetEvent(false); // non-signaled when no input sent from main thread, signaled when there is input or an abort signal
        private volatile bool pleaseAbort = false;
        private object queuedInput;
        private bool lowPriorityExecution = false;

        public event EventHandler StateMachineBegin;
        private void OnStateMachineBegin()
        {
            if (this.syncContext != null && this.syncContext.InvokeRequired)
            {
                this.syncContext.BeginInvoke(new Procedure(OnStateMachineBegin), null);
            }
            else
            {
                if (StateMachineBegin != null)
                {
                    StateMachineBegin(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler<EventArgs<State>> StateBegin;
        private void OnStateBegin(State state)
        {
            if (this.syncContext != null && this.syncContext.InvokeRequired)
            {
                this.syncContext.BeginInvoke(new Procedure<State>(OnStateBegin), new object[] { state });
            }
            else
            {
                if (StateBegin != null)
                {
                    StateBegin(this, new EventArgs<State>(state));
                }
            }
        }

        public event ProgressEventHandler StateProgress;
        private void OnStateProgress(double percent)
        {
            if (this.syncContext != null && this.syncContext.InvokeRequired)
            {
                this.syncContext.BeginInvoke(new Procedure<double>(OnStateProgress), new object[] { percent });
            }
            else
            {
                if (StateProgress != null)
                {
                    StateProgress(this, new ProgressEventArgs(percent));
                }
            }
        }

        public event EventHandler<EventArgs<State>> StateWaitingForInput;
        private void OnStateWaitingForInput(State state)
        {
            if (this.syncContext != null && this.syncContext.InvokeRequired)
            {
                this.syncContext.BeginInvoke(new Procedure<State>(OnStateWaitingForInput), new object[] { state });
            }
            else
            {
                if (StateWaitingForInput != null)
                {
                    StateWaitingForInput(this, new EventArgs<State>(state));
                }
            }
        }

        public event EventHandler StateMachineFinished;
        private void OnStateMachineFinished()
        {
            if (this.syncContext != null && this.syncContext.InvokeRequired)
            {
                this.syncContext.BeginInvoke(new Procedure(OnStateMachineFinished), null);
            }
            else
            {
                if (StateMachineFinished != null)
                {
                    StateMachineFinished(this, EventArgs.Empty);
                }
            }
        }

        public bool IsStarted
        {
            get
            {
                return this.isStarted;
            }
        }

        public bool LowPriorityExecution
        {
            get
            {
                return this.lowPriorityExecution;
            }

            set
            {
                if (IsStarted)
                {
                    throw new InvalidOperationException("Can only enable low priority execution before the state machine begins execution");
                }

                this.lowPriorityExecution = value;
            }
        }

        public ISynchronizeInvoke SyncContext
        {
            get
            {
                return this.syncContext;
            }

            set
            {
                this.syncContext = value;
            }
        }

        public State CurrentState
        {
            get
            {
                return this.stateMachine.CurrentState;
            }
        }

        public bool IsInFinalState
        {
            get
            {
                return this.stateMachine.IsInFinalState;
            }
        }

        private void StateMachineThread()
        {
            ThreadBackground tbm = null;

            try
            {
                if (this.lowPriorityExecution)
                {
                    tbm = new ThreadBackground(ThreadBackgroundFlags.Cpu);
                }

                StateMachineThreadImpl();
            }

            finally
            {
                if (tbm != null)
                {
                    tbm.Dispose();
                    tbm = null;
                }
            }
        }

        private void StateMachineThreadImpl()
        {
            this.threadException = null;

            EventHandler<EventArgs<State>> newStateHandler =
                delegate(object sender, EventArgs<State> e)
                {
                    this.stateMachineInitialized.Set();
                    OnStateBegin(e.Data);
                };

            ProgressEventHandler stateProgressHandler =
                delegate(object sender, ProgressEventArgs e)
                {
                    OnStateProgress(e.Percent);
                };

            try
            {
                this.stateMachineNotBusy.Set();

                OnStateMachineBegin();

                this.stateMachineNotBusy.Reset();
                this.stateMachine.NewState += newStateHandler;                
                this.stateMachine.StateProgress += stateProgressHandler;
                this.stateMachine.Start();

                while (true)
                {
                    this.stateMachineNotBusy.Set();
                    OnStateWaitingForInput(this.stateMachine.CurrentState);
                    this.inputAvailable.WaitOne();
                    this.inputAvailable.Reset();
                    // main thread should call Reset() on stateMachineNotBusy

                    if (this.pleaseAbort)
                    {
                        break;
                    }

                    this.stateMachine.ProcessInput(this.queuedInput);

                    if (this.stateMachine.IsInFinalState)
                    {
                        break;
                    }
                }

                this.stateMachineNotBusy.Set();
            }

            catch (Exception ex)
            {
                this.threadException = ex;
            }

            finally
            {
                this.stateMachineNotBusy.Set();
                this.stateMachineInitialized.Set();
                this.stateMachine.NewState -= newStateHandler;
                this.stateMachine.StateProgress -= stateProgressHandler;
                OnStateMachineFinished();
            }
        }

        public void Start()
        {
            if (this.isStarted)
            {
                throw new InvalidOperationException("State machine thread is already executing");
            }

            this.isStarted = true;

            this.stateMachineThread = new Thread(new ThreadStart(StateMachineThread));
            this.stateMachineInitialized.Reset();
            this.stateMachineThread.Start();
            this.stateMachineInitialized.WaitOne();
        }

        public void ProcessInput(object input)
        {
            this.stateMachineNotBusy.WaitOne();
            this.stateMachineNotBusy.Reset();
            this.queuedInput = input;
            this.inputAvailable.Set();
        }

        public void Abort()
        {
            if (this.disposed)
            {
                return;
            }

            this.pleaseAbort = true;

            State currentState2 = this.stateMachine.CurrentState;
            if (currentState2 != null && currentState2.CanAbort)
            {
                this.stateMachine.CurrentState.Abort();
            }

            this.stateMachineNotBusy.WaitOne();
            this.inputAvailable.Set();
            this.stateMachineThread.Join();

            if (this.threadException != null)
            {
                throw new WorkerThreadException("State machine thread threw an exception", this.threadException);
            }
        }

        public StateMachineExecutor(StateMachine stateMachine)
        {
            this.stateMachine = stateMachine;
        }

        ~StateMachineExecutor()
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
            if (disposing)
            {
                Abort();

                if (this.stateMachineInitialized != null)
                {
                    this.stateMachineInitialized.Close();
                    this.stateMachineInitialized = null;
                }

                if (this.stateMachineNotBusy != null)
                {
                    this.stateMachineNotBusy.Close();
                    this.stateMachineNotBusy = null;
                }

                if (this.inputAvailable != null)
                {
                    this.inputAvailable.Close();
                    this.inputAvailable = null;
                }
            }

            this.disposed = true;
        }
    }
}
