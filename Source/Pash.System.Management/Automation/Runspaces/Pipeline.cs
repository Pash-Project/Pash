using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Diagnostics;

namespace System.Management.Automation.Runspaces
{
    public abstract class Pipeline : IDisposable
    {
        public CommandCollection Commands { get; protected set; }
        public abstract PipelineReader<object> Error { get; }
        public abstract PipelineWriter Input { get; }
        public long InstanceId { get; private set; }
        public abstract bool IsNested { get; }
        public abstract PipelineReader<PSObject> Output { get; }
        public abstract PipelineStateInfo PipelineStateInfo { get; }
        public abstract Runspace Runspace { get; }
        public bool SetPipelineSessionState { get; set; }

        public abstract event EventHandler<PipelineStateEventArgs> StateChanged;

        public abstract Pipeline Copy();
        protected virtual void Dispose(bool disposing) { throw new NotImplementedException(); }

        [DebuggerStepThrough]
        public Collection<PSObject> Invoke()
        {
            return Invoke(new object[] { });
        }

        public abstract Collection<PSObject> Invoke(IEnumerable input);
        public abstract void InvokeAsync();
        public abstract void Stop();
        public abstract void StopAsync();

        // internals
        internal Pipeline()
        {
            Commands = new CommandCollection();
        }

        #region IDisposable Members

        public void Dispose()
        {
            // TODO: implement Pipeline.Dispose
        }

        #endregion
    }
}
