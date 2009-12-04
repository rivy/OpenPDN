using PaintDotNet;
using System;
using System.Collections.Generic;
using System.Text;

namespace PdnBench
{
    public delegate void SetLayerInfoDelegate(int layerIndex, Layer layer);

    public class CompositionBenchmark
        : Benchmark
    {
        public const int Iterations = 30;

        private Document composeMe;
        private Surface dstSurface;
        private SetLayerInfoDelegate sliDelegate;

        protected override void OnBeforeExecute()
        {
            for (int i = 0; i < this.composeMe.Layers.Count; ++i)
            {
                this.sliDelegate(i, (Layer)this.composeMe.Layers[i]);
            }

            base.OnBeforeExecute();
        }

        protected override void OnExecute()
        {
            for (int i = 0; i < Iterations; ++i)
            {
                composeMe.Invalidate();
                composeMe.Update(new RenderArgs(this.dstSurface));
            }
        }

        public CompositionBenchmark(string name, Document composeMe, 
            Surface dstSurface, SetLayerInfoDelegate sliDelegate)
            : base(name)
        {
            this.composeMe = composeMe;
            this.dstSurface = dstSurface;
            this.sliDelegate = sliDelegate;
        }
    }
}
