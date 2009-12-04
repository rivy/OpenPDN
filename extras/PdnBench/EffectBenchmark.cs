using PaintDotNet;
using PaintDotNet.Effects;
using PaintDotNet.SystemLayer;
using System;
using System.Drawing;

namespace PdnBench
{
	/// <summary>
	/// Summary description for EffectBenchmark.
	/// </summary>
	public class EffectBenchmark
        : Benchmark
	{
        private Effect effect;
        private EffectConfigToken token;
        private Surface image;

        private Surface dst;
        private PdnRegion region;

        private int iterations;
        public int Iterations
        {
            get
            {
                return this.iterations;
            }
        }

        protected override void OnBeforeExecute()
        {
            this.dst = image.Clone();
            this.region = new PdnRegion(dst.Bounds);
        }

        protected sealed override void OnExecute()
        {
            for (int i = 0; i < this.iterations; ++i)
            {
                EffectConfigToken localToken;

                if (this.token == null)
                {
                    localToken = null;
                }
                else
                {
                    localToken = (EffectConfigToken)this.token.Clone();
                }

                RenderArgs srcArgs = new RenderArgs(image);
                RenderArgs dstArgs = new RenderArgs(dst);

                BackgroundEffectRenderer ber = new BackgroundEffectRenderer(effect, localToken, dstArgs, srcArgs, region,
                    25 * Processor.LogicalCpuCount, Processor.LogicalCpuCount);

                ber.Start();
                ber.Join();

                ber.Dispose();
                ber = null;
            }
        }

        protected override void OnAfterExecute()
        {
            region.Dispose();
            dst.Dispose();
        }

        public EffectBenchmark(string name, int iterations, Effect effect, EffectConfigToken token, Surface image)
            : base(name + " (" + iterations + "x)")
		{
            this.effect = effect;
            this.token = token;
            this.image = image;
            this.iterations = iterations;
		}
	}
}
