// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

//classtodo: Needs implementation (may require Runtime upgrades first)

using System;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Convert", "Path", DefaultParameterSetName = "Path")]
    public class ConvertPathCommand : CoreCommandBase
    {
        protected override void ProcessRecord()
        {
        }

        [Parameter(Position = 0, 
            ParameterSetName = "LiteralPath", 
            Mandatory = true, 
            ValueFromPipeline = false, 
            ValueFromPipelineByPropertyName = true), 
        Alias(new string[] { "PSPath" })]
        public string[] LiteralPath { get; set; }

        [Parameter(Position = 0, 
            ParameterSetName = "Path", 
            Mandatory = true, 
            ValueFromPipeline = true, 
            ValueFromPipelineByPropertyName = true)]
        public string[] Path { get; set; }
      
    }
}

