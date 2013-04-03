// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    /// <summary>
    /// NAME
    ///   Write-Verbose
    /// 
    /// DESCRIPTION
    ///   Writes a message to the verbose message stream. This is not usually visible but can be switched on and off per the user's perferences.
    /// 
    /// RELATED PASH COMMANDS
    ///   Write-Error
    ///   Write-Host
    ///   
    /// RELATED POSIX COMMANDS
    ///   echo 
    /// </summary>
    [Cmdlet("Write", "Verbose")]
    public sealed class WriteVerboseCommand : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            WriteVerbose(Message);
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

