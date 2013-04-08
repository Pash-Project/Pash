// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
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
