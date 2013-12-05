// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
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
        public ICommandRuntime CommandRuntime { get; set; }

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
                State = _executionContext.SessionState;
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

        internal InternalCommand() { }

        internal virtual void DoBeginProcessing() { }

        internal virtual void DoEndProcessing() { }

        internal virtual void DoProcessRecord() { }

        internal virtual void DoStopProcessing() { }

        internal void ThrowIfStopping() { }
    }
}
