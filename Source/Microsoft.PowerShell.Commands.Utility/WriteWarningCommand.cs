// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    /// <summary>
    /// NAME
    ///   Write-Warning
    /// 
    /// DESCRIPTION
    ///   Displays a warning message to the console.
    /// 
    /// RELATED PASH COMMANDS
    ///   Write-Error
    ///   Write-Host
    ///   
    /// RELATED POSIX COMMANDS
    ///   echo 
    /// </summary>
    [Cmdlet("Write", "Warning")]
    public sealed class WriteWarningCommand : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            WriteWarning(Message);
        }

        /// <summary>
        /// The message to write.
        /// </summary>
        [Parameter(
            Position = 0,
            Mandatory = true,
            ValueFromPipeline = true),
        AllowEmptyString]
        public string Message { get; set; }

    }
}

