using PaintDotNet;
using PaintDotNet.SystemLayer;
using PaintDotNet.Threading;
using System;
using System.Drawing;
using System.Threading;


namespace PdnBench
{
	/// <summary>
	/// Summary description for ResizeBenchmark.
	/// </summary>
	public class ResizeBenchmark
		: Benchmark
	{
        private Surface src;
        private Surface dst;
        private PaintDotNet.Threading.ThreadPool threadPool;
        private Rectangle[] rects;

        private sealed class FitSurfaceContext
        {
            private Surface dstSurface;
            private Surface srcSurface;
            private Rectangle[] dstRois;
            private ResamplingAlgorithm algorithm;

            public Surface DstSurface
            {
                get
                {
                    return dstSurface;
                }
            }

            public Surface SrcSurface
            {
                get
                {
                    return srcSurface;
                }
            }

            public Rectangle[] DstRois
            {
                get
                {
                    return dstRois;
                }
            }

            public ResamplingAlgorithm Algorithm
            {
                get
                {
                    return algorithm;
                }
            }

            public event Procedure RenderedRect;
            private void OnRenderedRect()
            {
                if (RenderedRect != null)
                {
                    RenderedRect();
                }
            }

            public void FitSurface(object context)
            {
                int index = (int)context;
                dstSurface.FitSurface(algorithm, srcSurface, dstRois[index]);
            }

            public FitSurfaceContext(Surface dstSurface, Surface srcSurface, Rectangle[] dstRois, ResamplingAlgorithm algorithm)
            {
                this.dstSurface = dstSurface;
                this.srcSurface = srcSurface;
                this.dstRois = dstRois;
                this.algorithm = algorithm;
            }
        }

		protected override void OnBeforeExecute()
		{
            this.threadPool = new PaintDotNet.Threading.ThreadPool();
            rects = new Rectangle[Processor.LogicalCpuCount];
            Utility.SplitRectangle(this.dst.Bounds, rects);
            base.OnBeforeExecute();
		}

		protected override void OnExecute()
		{
            FitSurfaceContext fsc = new FitSurfaceContext(this.dst, this.src, rects, ResamplingAlgorithm.Bicubic);
            for (int i = 0; i < this.rects.Length; ++i)
            {
                this.threadPool.QueueUserWorkItem(new WaitCallback(fsc.FitSurface), (object)i);
            }

            this.threadPool.Drain();
		}

		protected override void OnAfterExecute()
		{
            this.threadPool = null;
			base.OnAfterExecute ();
		}

		public ResizeBenchmark(string name, Surface src, Surface dst)
			: base(name)
		{
            this.src = src;
            this.dst = dst;
		}
	}
}
