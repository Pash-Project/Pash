using System;
using System.Collections.ObjectModel;
using System.Threading;
using Pash.Implementation;

namespace System.Management.Automation.Runspaces
{
    public abstract class PipelineReader<T>
    {
        protected PipelineReader() { }

        public abstract int Count { get; }
        public abstract bool EndOfPipeline { get; }
        public abstract bool IsOpen { get; }
        public abstract int MaxCapacity { get; }
        public abstract WaitHandle WaitHandle { get; }

        public abstract event EventHandler DataReady;

        public abstract void Close();
        public abstract Collection<T> NonBlockingRead();
        public abstract Collection<T> NonBlockingRead(int maxRequested);
        public abstract T Peek();
        public abstract T Read();
        public abstract Collection<T> Read(int count);
        public abstract Collection<T> ReadToEnd();
    }
}
