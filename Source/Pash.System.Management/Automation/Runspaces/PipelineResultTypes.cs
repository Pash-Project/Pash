using System;

namespace System.Management.Automation.Runspaces
{
    [Flags]
    public enum PipelineResultTypes
    {
        None = 0,
        Output = 1,
        Error = 2,
    }
}
