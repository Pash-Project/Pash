// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;

namespace System.Management.Automation.Runspaces
{
    public enum PipelineState
    {
        NotStarted = 0,
        Running = 1,
        Stopping = 2,
        Stopped = 3,
        Completed = 4,
        Failed = 5,
    }
}
