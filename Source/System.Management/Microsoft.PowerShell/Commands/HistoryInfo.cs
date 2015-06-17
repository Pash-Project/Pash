// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Management.Automation.Runspaces;

namespace Microsoft.PowerShell.Commands
{
    public class HistoryInfo
    {
        public string CommandLine { get; private set; }
        public DateTime EndExecutionTime { get; private set; }
        public PipelineState ExecutionStatus { get; private set; }
        public long Id { get; private set; }
        public DateTime StartExecutionTime { get; private set; }
    }
}
