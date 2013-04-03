// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

//classtodo: Implement, may require runtime improvements.

using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Tee", "Object")]
    public sealed class TeeObjectCommand : PSCmdlet
    {
        protected override void ProcessRecord()
        {
        }

        protected override void BeginProcessing()
        {
       
        }

        [Alias(new string[] { "PSPath" }), 
        Parameter(Mandatory = true, Position = 0, ParameterSetName = "File")]
        public string FilePath { get; set; }

        [Parameter(ValueFromPipeline = true)]
        public PSObject InputObject { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = "Variable")]
        public string Variable { get; set; }
    }
   
}

