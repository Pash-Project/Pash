using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Get", "PSSnapin")]
    public sealed class GetPSSnapinCommand : PSSnapInCommandBase
    {
        [Parameter(Position = 0, Mandatory = false)]
        public string[] Name { get; set; }

        [Parameter(Mandatory = false)]
        public SwitchParameter Registered { get; set; }

        public GetPSSnapinCommand()
        {
            
        }

        protected override void BeginProcessing()
        {
            // TODO: write out all the registred snapins
        }
    }
}