using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Get", "PSProvider")]
    public class GetPSProviderCommand : PSCmdlet
    {
        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true)]
        public string[] PSProvider { get; private set; }

        public GetPSProviderCommand()
        {
            PSProvider = new string[0];
        }

        protected override void ProcessRecord()
        {
            foreach (ProviderInfo info in SessionState.Provider.GetAll())
            {
                WriteObject(info);
            }
        }
    }
}