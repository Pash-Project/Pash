using System.Management.Automation.Host;
using System.Xml.Schema;
using Pash.Implementation;

namespace System.Management.Automation.Internal
{
    public abstract class InternalCommand
    {
        internal CommandInfo CommandInfo { get; set; }
        internal PSHost PSHostInternal { get; private set; }
        internal PSObject CurrentPipelineObject { get; set; }
        internal SessionState State { get; private set; }
        internal ICommandRuntime CommandRuntime { get; set; }

        private ExecutionContext _executionContext;
        internal ExecutionContext ExecutionContext
        {
            get
            {
                return _executionContext;
            }
            set
            {
                _executionContext = value;
                State = new SessionState(_executionContext.SessionState.SessionStateGlobal);
                PSHostInternal = _executionContext.LocalHost;
            }
        }

        internal bool IsStopping
        {
            get
            {
                return ((PipelineCommandRuntime)CommandRuntime).IsStopping;
            }
        }

        internal InternalCommand() {}

        internal virtual void DoBeginProcessing() {}

        internal virtual void DoEndProcessing() {}

        internal virtual void DoProcessRecord() {}

        internal virtual void DoStopProcessing() {}

        internal void ThrowIfStopping() {}
    }
}