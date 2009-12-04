/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.HistoryMementos;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// The HistoryStack class for the History "concept".  
    /// Serves as the undo and redo stacks.  
    /// </summary>
    [Serializable]
    internal class HistoryStack
    {
        private List<HistoryMemento> undoStack;
        private List<HistoryMemento> redoStack;
        private DocumentWorkspace documentWorkspace;
        private int stepGroupDepth;
        private int isExecutingMemento = 0; // 0 -> false, >0 -> true

        public bool IsExecutingMemento
        {
            get
            {
                return this.isExecutingMemento > 0;
            }
        }

        private void PushExecutingMemento()
        {
            ++this.isExecutingMemento;
        }

        private void PopExecutingMemento()
        {
            --this.isExecutingMemento;
        }

        public List<HistoryMemento> UndoStack
        {
            get
            {
                return this.undoStack;
            }
        }

        public List<HistoryMemento> RedoStack
        {
            get
            {
                return this.redoStack;
            }
        }

        public void BeginStepGroup()
        {
            ++this.stepGroupDepth;
        }

        public void EndStepGroup()
        {
            --this.stepGroupDepth;

            if (this.stepGroupDepth == 0)
            {
                OnFinishedStepGroup();
            }
        }

        public event EventHandler FinishedStepGroup;
        protected void OnFinishedStepGroup()
        {
            if (FinishedStepGroup != null)
            {
                FinishedStepGroup(this, EventArgs.Empty);
            }
        }

        public event EventHandler SteppedBackward;
        protected void OnSteppedBackward()
        {
            if (SteppedBackward != null)
            {
                SteppedBackward(this, EventArgs.Empty);
            }
        }

        public event EventHandler SteppedForward;
        protected void OnSteppedForward()
        {
            if (SteppedForward != null)
            {
                SteppedForward(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Event handler for when a new history memento has been added.
        /// </summary>
        public event EventHandler NewHistoryMemento;
        protected void OnNewHistoryMemento()
        {
            if (NewHistoryMemento != null)
            {
                NewHistoryMemento(this, EventArgs.Empty);
            }
        }
                
        /// <summary>
        /// Event handler for when changes have been made to the history.
        /// </summary>
        public event EventHandler Changed;
        protected void OnChanged()
        {
            if (Changed != null)
            {
                Changed(this, EventArgs.Empty);
            }
        }

        public event EventHandler Changing;
        protected void OnChanging()
        {
            if (Changing != null)
            {
                Changing(this, EventArgs.Empty);
            }
        }

        public event EventHandler HistoryFlushed;
        protected void OnHistoryFlushed()
        {
            if (HistoryFlushed != null)
            {
                HistoryFlushed(this, EventArgs.Empty);
            }
        }

        public event ExecutingHistoryMementoEventHandler ExecutingHistoryMemento;
        protected void OnExecutingHistoryMemento(ExecutingHistoryMementoEventArgs e)
        {
            if (ExecutingHistoryMemento != null)
            {
                ExecutingHistoryMemento(this, e);
            }
        }

        public event ExecutedHistoryMementoEventHandler ExecutedHistoryMemento;
        protected void OnExecutedHistoryMemento(ExecutedHistoryMementoEventArgs e)
        {
            if (ExecutedHistoryMemento != null)
            {
                ExecutedHistoryMemento(this, e);
            }
        }

        public void PerformChanged()
        {
            OnChanged();
        }

        public HistoryStack(DocumentWorkspace documentWorkspace)
        {
            this.documentWorkspace = documentWorkspace;
            undoStack = new List<HistoryMemento>();
            redoStack = new List<HistoryMemento>();
        }

        private HistoryStack(
            List<HistoryMemento> undoStack,
            List<HistoryMemento> redoStack)
        {
            this.undoStack = new List<HistoryMemento>(undoStack);
            this.redoStack = new List<HistoryMemento>(redoStack);
        }

        /// <summary>
        /// When the user does something new, it will clear out the redo stack.
        /// </summary>
        public void PushNewMemento(HistoryMemento value)
        {
            Utility.GCFullCollect();

            OnChanging();

            ClearRedoStack();
            undoStack.Add(value);
            OnNewHistoryMemento();

            OnChanged();

            value.Flush();
            Utility.GCFullCollect();
        }

        /// <summary>
        /// Takes one item from the redo stack, "redoes" it, then places the redo
        /// memento object to the top of the undo stack.
        /// </summary>
        public void StepForward()
        {
            PushExecutingMemento();

            try
            {
                StepForwardImpl();
            }

            finally
            {
                PopExecutingMemento();
            }
        }

        private void StepForwardImpl()
        {
            HistoryMemento topMemento = redoStack[0];
            ToolHistoryMemento asToolHistoryMemento = topMemento as ToolHistoryMemento;

            if (asToolHistoryMemento != null && asToolHistoryMemento.ToolType != this.documentWorkspace.GetToolType())
            {
                this.documentWorkspace.SetToolFromType(asToolHistoryMemento.ToolType);
                StepForward();
            }
            else
            {
                OnChanging();

                ExecutingHistoryMementoEventArgs ehaea1 = new ExecutingHistoryMementoEventArgs(topMemento, true, false);

                if (asToolHistoryMemento == null && topMemento.SeriesGuid != Guid.Empty)
                {
                    ehaea1.SuspendTool = true;
                }

                OnExecutingHistoryMemento(ehaea1);

                if (ehaea1.SuspendTool)
                {
                    this.documentWorkspace.PushNullTool();
                }
            
                HistoryMemento redoMemento = redoStack[0];

                // Possibly useful invariant here:
                //     ehaea1.HistoryMemento.SeriesGuid == ehaea2.HistoryMemento.SeriesGuid == ehaea3.HistoryMemento.SeriesGuid
                ExecutingHistoryMementoEventArgs ehaea2 = new ExecutingHistoryMementoEventArgs(redoMemento, false, ehaea1.SuspendTool);
                OnExecutingHistoryMemento(ehaea2);

                HistoryMemento undoMemento = redoMemento.PerformUndo();
            
                redoStack.RemoveAt(0);
                undoStack.Add(undoMemento);

                ExecutedHistoryMementoEventArgs ehaea3 = new ExecutedHistoryMementoEventArgs(undoMemento);
                OnExecutedHistoryMemento(ehaea3);

                OnChanged();
                OnSteppedForward();

                undoMemento.Flush();

                if (ehaea1.SuspendTool)
                {
                    this.documentWorkspace.PopNullTool();
                }       
            }

            if (this.stepGroupDepth == 0)
            {
                OnFinishedStepGroup();
            }
        }

        /// <summary>
        /// Undoes the top of the undo stack, then places the redo memento object to the
        /// top of the redo stack.
        /// </summary>
        public void StepBackward()
        {
            PushExecutingMemento();

            try
            {
                StepBackwardImpl();
            }

            finally
            {
                PopExecutingMemento();
            }
        }

        private void StepBackwardImpl()
        {
            HistoryMemento topMemento = undoStack[undoStack.Count - 1];
            ToolHistoryMemento asToolHistoryMemento = topMemento as ToolHistoryMemento;

            if (asToolHistoryMemento != null && asToolHistoryMemento.ToolType != this.documentWorkspace.GetToolType())
            {
                this.documentWorkspace.SetToolFromType(asToolHistoryMemento.ToolType);
                StepBackward();
            }
            else
            {
                OnChanging();

                ExecutingHistoryMementoEventArgs ehaea1 = new ExecutingHistoryMementoEventArgs(topMemento, true, false);

                if (asToolHistoryMemento == null && topMemento.SeriesGuid == Guid.Empty)
                {
                    ehaea1.SuspendTool = true;
                }

                OnExecutingHistoryMemento(ehaea1);

                if (ehaea1.SuspendTool)
                {
                    this.documentWorkspace.PushNullTool();
                }

                HistoryMemento undoMemento = undoStack[undoStack.Count - 1];

                ExecutingHistoryMementoEventArgs ehaea2 = new ExecutingHistoryMementoEventArgs(undoMemento, false, ehaea1.SuspendTool);
                OnExecutingHistoryMemento(ehaea2);

                HistoryMemento redoMemento = undoStack[undoStack.Count - 1].PerformUndo();
                undoStack.RemoveAt(undoStack.Count - 1);
                redoStack.Insert(0, redoMemento);

                // Possibly useful invariant here:
                //     ehaea1.HistoryMemento.SeriesGuid == ehaea2.HistoryMemento.SeriesGuid == ehaea3.HistoryMemento.SeriesGuid
                ExecutedHistoryMementoEventArgs ehaea3 = new ExecutedHistoryMementoEventArgs(redoMemento);
                OnExecutedHistoryMemento(ehaea3);

                OnChanged();
                OnSteppedBackward();

                redoMemento.Flush();

                if (ehaea1.SuspendTool)
                {
                    this.documentWorkspace.PopNullTool();
                }
            }

            if (this.stepGroupDepth == 0)
            {
                OnFinishedStepGroup();
            }
        }

        public void ClearAll()
        {
            OnChanging();

            foreach (HistoryMemento ha in undoStack)
            {
                ha.Flush();
            }

            foreach (HistoryMemento ha in redoStack)
            {
                ha.Flush();
            }

            undoStack = new List<HistoryMemento>();
            redoStack = new List<HistoryMemento>();
            OnChanged();
            OnHistoryFlushed();
        }

        public void ClearRedoStack()
        {
            foreach (HistoryMemento ha in redoStack)
            {
                ha.Flush();
            }

            OnChanging();
            redoStack = new List<HistoryMemento>();
            OnChanged();
        }
    }
}
