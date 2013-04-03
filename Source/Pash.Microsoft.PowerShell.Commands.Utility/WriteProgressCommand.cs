// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    /// <summary>
    /// NAME
    ///   Write-Progress
    /// 
    /// DESCRIPTION
    ///   Displays a progress bar which you can use to reflect the status of a running job.
    /// 
    /// RELATED PASH COMMANDS
    ///   Write-Error
    ///   Write-Host
    ///   
    /// RELATED POSIX COMMANDS
    ///   n/a 
    /// </summary>
    [Cmdlet("Write", "Progress")]
    public sealed class WriteProgressCommand : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            //[!TODO!]
        }

        [Parameter]
        public int SecondsRemaining { get; set; }

        [Parameter(
            Position = 0,
            Mandatory = true)]
        public string Activity { get; set; }

        [Parameter]
        public SwitchParameter Completed { get; set; }

        [Parameter]
        public string CurrentOperation { get; set; }

        [ValidateRange(-1, 100), Parameter]
        public int PercentComplete { get; set; }

        [ValidateRange(0, 0x7fffffff), Parameter(Position = 2)]
        public int Id { get; set; }

        [ValidateRange(-1, 0x7fffffff), Parameter]
        public int ParentId { get; set; }

        [Parameter]
        public int SourceId { get; set; }

        [Parameter(Position = 1, Mandatory = true)]
        public string Status { get; set; }

    }
}

