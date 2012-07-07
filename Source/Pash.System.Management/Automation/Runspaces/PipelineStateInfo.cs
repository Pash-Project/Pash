using System;

namespace System.Management.Automation.Runspaces
{
    public sealed class PipelineStateInfo
    {
        public Exception Reason { get; private set; }
        public PipelineState State { get; private set; }

        // internals
        internal PipelineStateInfo Clone()
        {
            return new PipelineStateInfo(this);
        }

        internal PipelineStateInfo(PipelineStateInfo pipelineStateInfo)
        {
            Reason = pipelineStateInfo.Reason;
            State = pipelineStateInfo.State;
        }

        internal PipelineStateInfo(PipelineState state, Exception reason)
        {
            State = state;
        }

        internal PipelineStateInfo(PipelineState state)
        {
            State = state;
        }
    }
}
