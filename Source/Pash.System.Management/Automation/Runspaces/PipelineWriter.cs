using System;
using System.Threading;

namespace System.Management.Automation.Runspaces
{
    public abstract class PipelineWriter
    {
        protected PipelineWriter() { }

        public abstract int Count { get; }
        public abstract bool IsOpen { get; }
        public abstract int MaxCapacity { get; }
        public abstract WaitHandle WaitHandle { get; }

        public abstract void Close();
        public abstract void Flush();
        public abstract int Write(object obj);
        public abstract int Write(object obj, bool enumerateCollection);
    }
}
