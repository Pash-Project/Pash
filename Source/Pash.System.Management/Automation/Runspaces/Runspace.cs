using System;
using Pash.Implementation;

namespace System.Management.Automation.Runspaces
{
    public abstract class Runspace : IDisposable
    {
        internal Runspace()
        {
            InstanceId = Guid.NewGuid();
        }

        public static Runspace DefaultRunspace { get; set; }
        public Guid InstanceId { get; private set; }
        public abstract RunspaceConfiguration RunspaceConfiguration { get; }
        public abstract RunspaceStateInfo RunspaceStateInfo { get; }
        public SessionStateProxy SessionStateProxy { get { return GetSessionStateProxy(); } }
        public abstract Version Version { get; }

        public abstract event EventHandler<RunspaceStateEventArgs> StateChanged;

        public abstract void Close();
        public abstract void CloseAsync();
        public abstract Pipeline CreateNestedPipeline();
        public abstract Pipeline CreateNestedPipeline(string command, bool addToHistory);
        public abstract Pipeline CreatePipeline();
        public abstract Pipeline CreatePipeline(string command);
        public abstract Pipeline CreatePipeline(string command, bool addToHistory);
        public abstract void Open();
        public abstract void OpenAsync();

        // internals
        //internal long GeneratePipelineId();
        internal abstract SessionStateProxy GetSessionStateProxy();
        internal ExecutionContext ExecutionContext { get; set; }
        //internal abstract ExecutionContext GetExecutionContext { get; }
        //internal bool SkipUserProfile { set; get; }

        #region IDisposable Members

        public void Dispose()
        {
            // TODO: implement dispose pattern
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            // TODO: implement dispose pattern
        }


        #endregion
    }
}
