using PaintDotNet;
using PaintDotNet.SystemLayer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading;

namespace PdnBench
{
    public class GradientBenchmark
        : Benchmark
    {
        private Surface surface;
        private GradientRenderer renderer;
        private PaintDotNet.Threading.ThreadPool threadPool;
        private Rectangle[] rois;
        private int iterations;

        public int Iterations
        {
            get
            {
                return this.iterations;
            }
        }

        private void RenderThreadProc(object indexObj)
        {
            int index = (int)indexObj;
            this.renderer.Render(this.surface, this.rois, index, 1);
        }

        protected override void OnBeforeExecute()
        {
            this.renderer.StartColor = ColorBgra.Black;
            this.renderer.EndColor = ColorBgra.FromBgra(255, 128, 64, 64);
            this.renderer.StartPoint = new PointF(surface.Width / 2, surface.Height / 2);
            this.renderer.EndPoint = new PointF(0, 0);
            this.renderer.AlphaBlending = true;
            this.renderer.AlphaOnly = false;

            this.renderer.BeforeRender();

            this.rois = new Rectangle[Processor.LogicalCpuCount];
            Utility.SplitRectangle(this.surface.Bounds, rois);

            this.threadPool = new PaintDotNet.Threading.ThreadPool(Processor.LogicalCpuCount, false);

            base.OnBeforeExecute();
        }

        protected override void OnExecute()
        {
            WaitCallback wc = new WaitCallback(RenderThreadProc);

            for (int n = 0; n < this.iterations; ++n)
            {
                for (int i = 0; i < this.rois.Length; ++i)
                {
                    object iObj = BoxedConstants.GetInt32(i);
                    this.threadPool.QueueUserWorkItem(wc, iObj);
                }
            }

            this.threadPool.Drain();
        }

        protected override void OnAfterExecute()
        {
            this.renderer.AfterRender();
            this.threadPool = null;
            base.OnAfterExecute();
        }

        public GradientBenchmark(string name, Surface surface, GradientRenderer renderer, int iterations)
            : base(name)
        {
            this.surface = surface;
            this.renderer = renderer;
            this.iterations = iterations;
        }
    }
}
