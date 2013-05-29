// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Management.Automation;
using System.Diagnostics;

namespace Microsoft.PowerShell.Commands
{
    /// <summary>
    /// NAME
    ///   Get-Process
    /// 
    /// DESCRIPTION
    ///   Displays processes running on the system.
    /// 
    /// RELATED PASH COMMANDS
    ///   Stop-Process
    ///   
    /// RELATED POSIX COMMANDS
    ///   ps 
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "Process", DefaultParameterSetName = "Name")]
    public sealed class GetProcessCommand : Cmdlet
    {

        protected override void ProcessRecord()
        {
            if (Name != null)
                foreach (String _name in Name)
                    WriteObject(Process.GetProcessesByName(_name), true);

            else if (Id != null)
                foreach (int _id in Id)
                    WriteObject(Process.GetProcessById(_id));

            else if (InputObject != null)
                WriteObject(InputObject, true);

            else WriteObject(Process.GetProcesses(), true);
        }

        /// <summary>
        /// Return a process that has the given ID.
        /// </summary>
        [Alias(new string[] { "PID" }),
        Parameter(
            ParameterSetName = "Id",
            Mandatory = true,
            ValueFromPipelineByPropertyName = true)]
        public int[] Id { get; set; }

        /// <summary>
        /// Return all process(es) which have the given name.
        /// </summary>
        [Alias(new string[] { "ProcessName" }),
        Parameter(
            ValueFromPipelineByPropertyName = true,
            Position = 0,
            ParameterSetName = "Name"),
        ValidateNotNullOrEmpty]
        public String[] Name { get; set; }

        /// <summary>
        /// Return processes which match the given object.
        /// </summary>
        [Parameter(
            Mandatory = true,
            ValueFromPipeline = true,
            ParameterSetName = "InputObject")]
        public Process[] InputObject { get; set; }
    }
}
