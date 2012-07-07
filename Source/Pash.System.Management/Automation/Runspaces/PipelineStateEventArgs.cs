using System;

namespace System.Management.Automation.Runspaces
{
    public sealed class PipelineStateEventArgs : EventArgs
    {
        public PipelineStateEventArgs(PipelineStateInfo state)
        {
            PipelineStateInfo = state;
        }

        public PipelineStateInfo PipelineStateInfo { get; private set; }
    }
}
