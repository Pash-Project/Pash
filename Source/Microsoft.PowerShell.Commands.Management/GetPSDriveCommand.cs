// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Get", "PSDrive", DefaultParameterSetName = "Name")]
    public class GetPSDriveCommand : DriveMatchingCoreCommandBase
    {
        [Parameter(Position = 0, ParameterSetName = "LiteralName", Mandatory = true, ValueFromPipeline = false, ValueFromPipelineByPropertyName = true)]
        public string[] LiteralName { get; set; }

        [Parameter(Position = 0, ParameterSetName = "Name", ValueFromPipelineByPropertyName = true)]
        public string[] Name { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true)]
        public string[] PSProvider { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true)]
        public string Scope { get; set; }

        protected override void ProcessRecord()
        {
            var drives = GetDrives(LiteralName, Name, PSProvider, Scope);
            WriteObject(drives, true);
        }
    }
}
