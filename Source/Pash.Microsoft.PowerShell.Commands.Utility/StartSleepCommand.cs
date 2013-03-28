// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Management.Automation;
using System.Threading;

namespace Microsoft.PowerShell.Commands
{
    /// <summary>
    /// NAME
    ///   Start-Sleep
    /// 
    /// DESCRIPTION
    ///   Suspends execution of a program for the given time.
    ///   
    /// RELATED POSIX COMMANDS
    ///   sleep 
    /// </summary>
    [Cmdlet("Start", "Sleep", DefaultParameterSetName = "Seconds")]
    public sealed class StartSleepCommand : PSCmdlet
    {   
        protected override void ProcessRecord()
        {
            int _sleeptime;
            ManualResetEvent _thread = new ManualResetEvent(false);

            if (Milliseconds == 0)
                _sleeptime = Seconds * 1000;

            else _sleeptime = Milliseconds;

            _thread.WaitOne(_sleeptime, true);
        }

        /// <summary>
        /// The amount of time in milliseconds to suspend operation for.
        /// </summary>
        [ValidateRange(0, 0x7fffffff),
        Parameter(
            ParameterSetName = "Milliseconds",
            ValueFromPipelineByPropertyName = true,
            Mandatory = true)]
        public int Milliseconds { get; set; }

        /// <summary>
        /// The amount of time in seconds to suspend operation for.
        /// </summary>
        [ValidateRange(0, 0x7fffffff),
        Parameter(
            Mandatory = true, 
            ParameterSetName = "Seconds",
            Position = 0, 
            ValueFromPipeline = true, 
            ValueFromPipelineByPropertyName = true)]
        public int Seconds { get; set; }
        
    }

}
