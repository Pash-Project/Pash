// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    public class WriteContentCommandBase : PassThroughContentCommandBase
    {
        [AllowNull]
        [Parameter(Mandatory = true, Position = 1, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [AllowEmptyCollection]
        public object[] Value { get; set; }
    }
}
