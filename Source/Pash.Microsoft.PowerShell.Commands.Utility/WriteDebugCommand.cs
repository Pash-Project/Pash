// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    /// <summary>
    /// NAME
    ///   Write-Debug
    /// 
    /// DESCRIPTION
    ///   Writes a debug message to the console debug output.
    /// 
    /// RELATED PASH COMMANDS
    ///   Write-Error
    ///   Write-Host
    ///   
    /// RELATED POSIX COMMANDS
    ///   echo 
    /// </summary>
    [Cmdlet("Write", "Debug")]
    public sealed class WriteDebugCommand : PSCmdlet
    {

        protected override void ProcessRecord()
        {
            WriteDebug(Message);
        }

        /// <summary>
        /// The debug message to display.
        /// </summary>
        [Parameter(
            Position = 0, 
            Mandatory = true, 
            ValueFromPipeline = true), 
        AllowEmptyString]
        public string Message { get; set; }
    }
}
