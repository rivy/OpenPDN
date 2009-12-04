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
    internal class ExecutingHistoryMementoEventArgs
        : EventArgs
    {
        private HistoryMemento historyMemento;
        private bool mayAlterSuspendToolProperty;
        private bool suspendTool;

        public HistoryMemento HistoryMemento
        {
            get
            {
                return this.historyMemento;
            }
        }

        public bool MayAlterSuspendTool
        {
            get
            {
                return this.mayAlterSuspendToolProperty;
            }
        }

        public bool SuspendTool
        {
            get
            {
                return this.suspendTool;
            }

            set
            {
                if (!this.mayAlterSuspendToolProperty)
                {
                    throw new InvalidOperationException("May not alter the SuspendTool property when MayAlterSuspendToolProperty is false");
                }

                this.suspendTool = value;
            }
        }

        public ExecutingHistoryMementoEventArgs(HistoryMemento historyMemento, bool mayAlterSuspendToolProperty, bool suspendTool)
        {
            this.historyMemento = historyMemento;
            this.mayAlterSuspendToolProperty = mayAlterSuspendToolProperty;
            this.suspendTool = suspendTool;
        }
    }
}
