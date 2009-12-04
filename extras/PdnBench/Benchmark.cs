using PaintDotNet.SystemLayer;
using System;



namespace PdnBench
{
	/// <summary>
	/// Base class that runs a benchmark
	/// </summary>
	public abstract class Benchmark
        : IDisposable
	{
        private string name;
        private Timing timing;

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        protected virtual void OnBeforeExecute()
        {
        }

        protected abstract void OnExecute();

        protected virtual void OnAfterExecute()
        {
        }

        public TimeSpan Execute()
        {
            OnBeforeExecute();
            ulong start = timing.GetTickCount();
            OnExecute();
            ulong end = timing.GetTickCount();
            OnAfterExecute();
            return new TimeSpan(0, 0, 0, 0, (int)(end - start));
        }

		public Benchmark(string name)
		{
            this.name = name;
            timing = new Timing();
        }

        ~Benchmark()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}
