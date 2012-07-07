using System;
using System.Management.Automation;
using Microsoft.PowerShell.Commands;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Remove", "PSSnapin", SupportsShouldProcess = true)]
    public sealed class RemovePSSnapinCommand : PSSnapInCommandBase
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true)]
        public string[] Name { get; set; }

        [Parameter]
        public SwitchParameter PassThru { get; set; }

        public RemovePSSnapinCommand()
        {
            
        }

        protected override void ProcessRecord()
        {
            throw new NotImplementedException();
        }
    }
}