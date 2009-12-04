using PaintDotNet;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;

namespace PdnBench
{
    class TransformBenchmark
        : Benchmark
    {
        public const int Iterations = 30;
        private Surface dst;
        private MaskedSurface src;
        private Matrix transform;
        private bool highQuality;

        protected override void OnExecute()
        {
            for (int i = 0; i < Iterations; ++i)
            {
                this.src.Draw(this.dst, this.transform, this.highQuality ? ResamplingAlgorithm.Bilinear : ResamplingAlgorithm.NearestNeighbor);
            }
        }

        public TransformBenchmark(string name, Surface dst, MaskedSurface src, Matrix transform, bool highQuality)
            : base(name)
        {
            this.dst = dst;
            this.src = src;
            this.transform = transform.Clone();
            this.highQuality = highQuality;
        }
    }
}
