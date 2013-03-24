// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
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
