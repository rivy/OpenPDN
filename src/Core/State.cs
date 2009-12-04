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
    public abstract class State
    {
        private StateMachine stateMachine;
        private bool isFinalState;

        private bool abortedRequested = false;

        protected bool AbortRequested
        {
            get
            {
                return this.abortedRequested;
            }
        }

        public StateMachine StateMachine
        {
            get
            {
                return this.stateMachine;
            }

            set
            {
                this.stateMachine = value;
            }
        }

        public bool IsFinalState
        {
            get
            {
                return this.isFinalState;
            }
        }

        protected virtual void OnAbort()
        {
        }

        public virtual bool CanAbort
        {
            get
            {
                return false;
            }
        }

        public void Abort()
        {
            if (CanAbort)
            {
                this.abortedRequested = true;
                OnAbort();
            }
        }

        public virtual void OnEnteredState()
        {
        }

        public abstract void ProcessInput(object input, out State newState);

        protected void OnProgress(double percent)
        {
            if (this.StateMachine != null)
            {
                this.StateMachine.OnStateProgress(percent);
            }
        }

        protected State()
            : this(false)
        {
        }

        protected State(bool isFinalState)
        {
            this.isFinalState = isFinalState;
        }
    }
}
