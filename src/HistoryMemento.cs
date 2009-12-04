/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;
using System.Threading;

namespace PaintDotNet
{
    /// <summary>
    /// A HistoryMemento is generally used to save part of the state of the Document
    /// so that an action that is yet to be performed can be undone at a later time.
    /// For example, if you are going to paint in a certain region, you first create a
    /// HistoryMemento that saves the contents of the area you are painting to. Then you
    /// paint. Then you push the history action on to the history stack.
    /// 
    /// Using the HistoryMementoData class you can serialize your data to disk so that it
    /// doesn't fester in memory. There are important rules to follow here though:
    /// 1. Don't hold a reference to a Layer. Store a reference to the DocumentWorkspace and
    ///    the layer's index instead, and access it via Workspace.Document.Layers[index].
    /// 2. The exception to #1 is if you are deleting a layer. But you should use
    ///    DeleteLayerHistoryMemento for that. If you need to delete a layer as part of a
    ///    compound action, use CompoundHistoryMemento in conjunction with 
    ///    DeleteLayerHistoryMemento.
    /// 3. To generalize, avoid serializing something unless you're replacing or deleting it.
    ///    (and by 'serializing' I mean 'putting it in your HistoryMementoData class')
    ///    It is better to hold a 'navigation reference' as opposed to a real reference.
    ///    An example of a 'navigation reference' is listed in #1, where we don't store a ref
    ///    to the layer itself but we store the information needed to navigate to it.
    ///    The reasoning for this is made clear if you consider the following case. Assume you
    ///    are holding on to a layer reference ("private Layer theLayer;"). Next, assume that
    ///    the layer is deleted. Then the deletion is undone. The new layer in memory is not
    ///    the layer you have a reference to even though they hold the same data. Changes made
    ///    to one do not show up in the other one. Put another way, history actions should
    ///    store large objects and their locations "by value," and not "by reference."
    /// </summary>
    internal abstract class HistoryMemento
    {
        private string name;
        public string Name
        {
            get
            {
                return this.name;
            }

            set
            {
                this.name = value;
            }
        }

        private ImageResource image;
        public ImageResource Image
        {
            get
            {
                return this.image;
            }

            set
            {
                this.image = value;
            }
        }

        protected int id;
        private static int nextId = 0;
        public int ID
        {
            get
            {
                return this.id;
            }

            set
            {
                this.id = value;
            }
        }

        private Guid seriesGuid = Guid.Empty;
        public Guid SeriesGuid
        {
            get
            {
                return this.seriesGuid;
            }

            set
            {
                this.seriesGuid = value;
            }
        }

        private PersistedObject<HistoryMementoData> historyMementoData = null;

        /// <summary>
        /// Gets or sets the HistoryMementoData associated with this HistoryMemento.
        /// </summary>
        /// <remarks>
        /// Setting this property will immediately serialize the given object to disk.
        /// </remarks>
        protected HistoryMementoData Data
        {
            get
            {
                if (historyMementoData == null)
                {
                    return null;
                }
                else
                {
                    return (HistoryMementoData)historyMementoData.Object;
                }
            }

            set
            {
                this.historyMementoData = new PersistedObject<HistoryMementoData>(value, false);
            }
        }

        /// <summary>
        /// Ensures that the memory held by the Data property is serialized to disk and
        /// freed from memory.
        /// </summary>
        public void Flush()
        {
            if (historyMementoData != null)
            {
                historyMementoData.Flush();
            }

            OnFlush();
        }

        protected virtual void OnFlush()
        {
        }

        /// <summary>
        /// This will perform the necessary work required to undo an action.
        /// Note that the returned HistoryMemento should have the same ID.
        /// </summary>
        /// <returns>
        /// Returns a HistoryMemento that can be used to redo the action.
        /// Note that this property should hold: undoAction = undoAction.PerformUndo().PerformUndo()
        /// </returns>
        protected abstract HistoryMemento OnUndo();

        /// <summary>
        /// This method ensures that the returned HistoryMemento has the appropriate ID tag.
        /// </summary>
        /// <returns>Returns a HistoryMemento that can be used to redo the action. 
        /// The ID of this HistoryMemento will be the same as the object that this 
        /// method was called on.</returns>
        public HistoryMemento PerformUndo()
        {
            HistoryMemento ha = OnUndo();
            ha.ID = this.ID;
            ha.SeriesGuid = this.SeriesGuid;
            return ha;
        }

        public HistoryMemento(string name, ImageResource image)
        {
            SystemLayer.Tracing.LogFeature("HM(" + GetType().Name + ")");

            this.name = name;
            this.image = image;
            this.id = Interlocked.Increment(ref nextId);
        }
    }
}
