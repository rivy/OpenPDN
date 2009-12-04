using PaintDotNet;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace PdnBench
{
    public class ZoomOutBlitBenchmark
        : Benchmark
    {
        public const int IterationCount = 1000;

        private Surface source;
        private Surface dst;
        private Size blitSize;
        private Rectangle[] blitRects;
        private Surface[] blitWindows;
        private PaintDotNet.Threading.ThreadPool threadPool;

        protected override void OnBeforeExecute()
        {
            Rectangle blitRect = new Rectangle(0, 0, blitSize.Width, blitSize.Height);

            this.blitRects = new Rectangle[PaintDotNet.SystemLayer.Processor.LogicalCpuCount];
            Utility.SplitRectangle(blitRect, this.blitRects);

            this.blitWindows = new Surface[this.blitRects.Length];
            for (int i = 0; i < blitRects.Length; ++i)
            {
                blitWindows[i] = this.dst.CreateWindow(this.blitRects[i]);
            }

            this.threadPool = new PaintDotNet.Threading.ThreadPool();

            base.OnBeforeExecute();
        }

        private void Render(object indexObj)
        {
            int index = (int)indexObj;
            SurfaceBoxBaseRenderer.RenderZoomOutRotatedGridMultisampling(this.blitWindows[index], this.source,
                this.blitRects[index].Location, this.blitSize);
        }

        protected override void OnExecute()
        {
            System.Threading.WaitCallback renderDelegate = new System.Threading.WaitCallback(Render);

            for (int i = 0; i < IterationCount; ++i)
            {
                for (int j = 0; j < this.blitRects.Length; ++j)
                {
                    object jObj = BoxedConstants.GetInt32(j);
                    this.threadPool.QueueUserWorkItem(renderDelegate, jObj);
                }

                this.threadPool.Drain();
            }
        }

        protected override void OnAfterExecute()
        {
            for (int i = 0; i < this.blitWindows.Length; ++i)
            {
                this.blitWindows[i].Dispose();
                this.blitWindows[i] = null;
            }

            this.threadPool = null;
            base.OnAfterExecute();
        }

        public ZoomOutBlitBenchmark(string name, Surface source, Surface dst, Size blitSize)
            : base(name)
        {
            this.source = source;
            this.dst = dst;
            this.blitSize = blitSize;
        }
    }
}
