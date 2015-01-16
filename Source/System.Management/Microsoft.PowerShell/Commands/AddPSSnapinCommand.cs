// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System.Management.Automation;
using System;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Add, "PSSnapin")]
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
            foreach (var curName in Name)
            {
                try
                {
                    var snapin = SessionState.SessionStateGlobal.AddPSSnapIn(curName, ExecutionContext);
                    if (PassThru.IsPresent)
                    {
                        WriteObject(snapin);
                    }
                }
                catch (PSArgumentException e)
                {
                    WriteError(e.ErrorRecord);
                }
            }
        }
    }
}
