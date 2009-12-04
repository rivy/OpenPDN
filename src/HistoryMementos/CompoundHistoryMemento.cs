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
using System.Collections.Generic;
using System.Drawing;

namespace PaintDotNet.HistoryMementos
{
    /// <summary>
    /// Lets you combine multiple HistoryMementos that can be undon/redone
    /// in a single operation, and be referred to by one name.
    /// The actions will be undone in the reverse order they are given to
    /// the constructor via the actions array.
    /// You can use 'null' for a HistoryMemento and it will be ignored.
    /// </summary>
    internal class CompoundHistoryMemento
        : HistoryMemento
    {
        private List<HistoryMemento> actions;

        protected override void OnFlush()
        {
            for (int i = 0; i < actions.Count; ++i)
            {
                if (actions[i] != null)
                {
                    actions[i].Flush();
                }
            }
        }

        protected override HistoryMemento OnUndo()
        {
            List<HistoryMemento> redoActions = new List<HistoryMemento>(actions.Count);

            for (int i = 0; i < actions.Count; ++i)
            {
                HistoryMemento ha = actions[actions.Count - i - 1];
                HistoryMemento rha = null;

                if (ha != null)
                {
                    rha = ha.PerformUndo();
                }

                redoActions.Add(rha);
            }

            CompoundHistoryMemento cha = new CompoundHistoryMemento(Name, Image, redoActions);
            return cha;
        }

        public void PushNewAction(HistoryMemento newHA)
        {
            actions.Add(newHA);
        }

        public CompoundHistoryMemento(string name, ImageResource image, List<HistoryMemento> actions)
            : base(name, image)
        {
            this.actions = new List<HistoryMemento>(actions);
        }

        public CompoundHistoryMemento(string name, ImageResource image, HistoryMemento[] actions)
            : base(name, image)
        {
            this.actions = new List<HistoryMemento>(actions);
        }

        public CompoundHistoryMemento(string name, ImageResource image)
            : this(name, image, new HistoryMemento[0])
        {
        }
    }
}
