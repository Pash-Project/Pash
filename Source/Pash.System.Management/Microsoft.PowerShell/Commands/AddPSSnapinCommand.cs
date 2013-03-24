// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System.Management.Automation;
using System;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Add", "PSSnapin")]
    public sealed class AddPSSnapinCommand : PSSnapInCommandBase
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true)]
        public string[] Name { get; set; }

        [Parameter]
        public SwitchParameter PassThru { get; set; }

        public AddPSSnapinCommand()
        {

        }

        protected override void ProcessRecord()
        {
            throw new NotImplementedException();
        }
    }
}
