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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Text;

namespace PaintDotNet
{
    using ThumbnailReadyArgs = EventArgs<Pair<IThumbnailProvider, Surface>>;
    using ThumbnailReadyHandler = EventHandler<EventArgs<Pair<IThumbnailProvider, Surface>>>;
    using ThumbnailStackItem = Triple<IThumbnailProvider, EventHandler<EventArgs<Pair<IThumbnailProvider, Surface>>>, int>;
    using ThumbnailReadyEventDetails = Triple<EventHandler<EventArgs<Pair<IThumbnailProvider, Surface>>>, object, EventArgs<Pair<IThumbnailProvider, Surface>>>;

    // TODO: Add calls to VerifyNotDispose() for the next release where we can get enough testing for it

    public sealed class ThumbnailManager
        : IDisposable
    {
        private bool disposed = false;
        private int updateLatency = 67;

        public bool IsDisposed
        {
            get
            {
                return this.disposed;
            }
        }

        public int UpdateLatency
        {
            get
            {
                return this.updateLatency;
            }

            set
            {
                this.updateLatency = value;
            }
        }

        private Stack<ThumbnailStackItem> renderQueue;
        private ISynchronizeInvoke syncContext;
        private Thread renderThread;
        private volatile bool quitRenderThread;
        private object updateLock;

        /*
        private void VerifyNotDisposed()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException("ThumbnailManager");
            }
        }
         * */

        // This event is non-signaled during the period of time between when the rendering thread has popped
        // an item from the renderQueue, and when it finishes holding a reference to it (i.e. either it
        // finishes rendering the thumbnail, or it decides not to). At all other times, this event is signaled.
        private ManualResetEvent renderingInactive;

        private List<ThumbnailReadyEventDetails> thumbnailReadyInvokeList = 
            new List<ThumbnailReadyEventDetails>();

        private void DrainThumbnailReadyInvokeList()
        {
            List<ThumbnailReadyEventDetails> invokeListCopy = null;

            lock (this.thumbnailReadyInvokeList)
            {
                invokeListCopy = this.thumbnailReadyInvokeList;
                this.thumbnailReadyInvokeList = new List<ThumbnailReadyEventDetails>();
            }

            foreach (ThumbnailReadyEventDetails invokeMe in invokeListCopy)
            {
                invokeMe.First.Invoke(invokeMe.Second, invokeMe.Third);
            }
        }

        private void OnThumbnailReady(IThumbnailProvider dw, ThumbnailReadyHandler callback, Surface thumb)
        {
            Pair<IThumbnailProvider, Surface> data = Pair.Create(dw, thumb);
            ThumbnailReadyArgs e = new ThumbnailReadyArgs(data);

            lock (this.thumbnailReadyInvokeList)
            {
                this.thumbnailReadyInvokeList.Add(new ThumbnailReadyEventDetails(callback, this, e));
            }

            try
            {
                this.syncContext.BeginInvoke(new Procedure(DrainThumbnailReadyInvokeList), null);
            }

            catch (ObjectDisposedException)
            {
                // Ignore this error
            }

            catch (InvalidOperationException)
            {
                // If syncContext was destroyed, then ignore
            }
        }

        public ThumbnailManager(ISynchronizeInvoke syncContext)
        {
            this.syncContext = syncContext;
            this.updateLock = new object();
            this.quitRenderThread = false;
            this.renderQueue = new Stack<ThumbnailStackItem>();
            this.renderingInactive = new ManualResetEvent(true);
            this.renderThread = new Thread(new ThreadStart(RenderThread));
            this.renderThread.Start();
        }

        ~ThumbnailManager()
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
            this.disposed = true;

            if (disposing)
            {
                this.quitRenderThread = true;

                lock (this.updateLock)
                {
                    Monitor.Pulse(this.updateLock);
                }

                if (this.renderThread != null)
                {
                    this.renderThread.Join();
                    this.renderThread = null;
                }

                if (this.renderingInactive != null)
                {
                    this.renderingInactive.Close();
                    this.renderingInactive = null;
                }
            }
        }

        public void DrainQueue()
        {
            int oldLatency = this.updateLatency;
            this.updateLatency = 0;

            int count = 1;
            while (count > 0)
            {
                lock (this.updateLock)
                {
                    count = this.renderQueue.Count;
                }

                this.renderingInactive.WaitOne();
            }

            this.updateLatency = oldLatency;
            DrainThumbnailReadyInvokeList();
        }

        public void RemoveFromQueue(IThumbnailProvider nukeMe)
        {
            lock (this.updateLock)
            {
                ThumbnailStackItem[] tsiArray = this.renderQueue.ToArray();
                List<ThumbnailStackItem> tsiAccumulate = new List<ThumbnailStackItem>();

                for (int i = 0; i < tsiArray.Length; ++i)
                {
                    if (tsiArray[i].First != nukeMe)
                    {
                        tsiAccumulate.Add(tsiArray[i]);
                    }
                }

                this.renderQueue.Clear();

                for (int i = 0; i < tsiAccumulate.Count; ++i)
                {
                    this.renderQueue.Push(tsiAccumulate[i]);
                }
            }
        }

        public void ClearQueue()
        {
            int oldUpdateLatency = this.updateLatency;
            this.updateLatency = 0;

            lock (this.updateLock)
            {
                this.renderQueue.Clear();
            }

            this.renderingInactive.WaitOne();
            this.updateLatency = oldUpdateLatency;
        }

        public void QueueThumbnailUpdate(IThumbnailProvider updateMe, int thumbSideLength, ThumbnailReadyHandler callback)
        {
            if (thumbSideLength < 1)
            {
                throw new ArgumentOutOfRangeException("thumbSideLength", "must be greater than or equal to 1");
            }

            lock (this.updateLock)
            {
                bool doIt = false;
                ThumbnailStackItem addMe = new ThumbnailStackItem(updateMe, callback, thumbSideLength);

                if (this.renderQueue.Count == 0)
                {
                    doIt = true;
                }
                else
                {
                    ThumbnailStackItem top = this.renderQueue.Peek();

                    if (addMe != top)
                    {
                        doIt = true;
                    }
                }

                // Only add this item to the queue if the item is not already at the top of the queue
                if (doIt)
                {
                    this.renderQueue.Push(addMe);
                }

                Monitor.Pulse(this.updateLock);
            }
        }

        private void RenderThread()
        {
            try
            {
                RenderThreadImpl();
            }

            finally
            {
                this.renderingInactive.Set();
            }
        }

        private void RenderThreadImpl()
        {
            while (true)
            {
                ThumbnailStackItem renderMe = new ThumbnailStackItem();

                // Wait for either a new item to render, or a signal to quit
                lock (this.updateLock)
                {
                    if (this.quitRenderThread)
                    {
                        return;
                    }

                    while (this.renderQueue.Count == 0)
                    {
                        Monitor.Wait(this.updateLock);

                        if (this.quitRenderThread)
                        {
                            return;
                        }
                    }

                    this.renderingInactive.Reset();
                    renderMe = this.renderQueue.Pop();
                }

                // Sleep for a short while. Our main goal is to ensure that the
                // item is not re-queued for updating very soon after we start
                // rendering it.
                Thread.Sleep(this.updateLatency);

                bool doRender = true;

                // While we were asleep, ensure that this same item has not been
                // re-added to the render queue. If it has, we will skip rendering
                // it. This covers the scenario where an item is updated many
                // times in rapid succession, in which case we want to wait until
                // the item has settled down before spending any CPU time rendering
                // its thumbnail.
                lock (this.updateLock)
                {
                    if (this.quitRenderThread)
                    {
                        return;
                    }

                    if (this.renderQueue.Count > 0)
                    {
                        if (renderMe == this.renderQueue.Peek())
                        {
                            doRender = false;
                        }
                    }
                }

                if (doRender)
                {
                    try
                    {
                        Surface thumb;

                        using (new ThreadBackground(ThreadBackgroundFlags.All))
                        {
                            thumb = renderMe.First.RenderThumbnail(renderMe.Third);
                        }

                        // If this same item has already been re-queued for an update, then throw
                        // away what we just rendered. Otherwise we may get flickering as the
                        // item was being updated while we were rendering the preview.
                        bool discard = false;

                        lock (this.updateLock)
                        {
                            if (this.quitRenderThread)
                            {
                                thumb.Dispose();
                                thumb = null;
                                return;
                            }

                            if (this.renderQueue.Count > 0)
                            {
                                if (renderMe == this.renderQueue.Peek())
                                {
                                    discard = true;
                                    thumb.Dispose();
                                    thumb = null;
                                }
                            }
                        }

                        if (!discard)
                        {
                            OnThumbnailReady(renderMe.First, renderMe.Second, thumb);
                        }
                    }

                    catch (Exception ex)
                    {
                        try
                        {
                            Tracing.Ping("Exception in RenderThread while calling CreateThumbnail: " + ex.ToString());
                        }

                        catch (Exception)
                        {
                        }
                    }
                }

                this.renderingInactive.Set();
            }
        }
    }
}