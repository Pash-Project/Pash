using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    [CmdletAttribute("New", "PSDrive", SupportsShouldProcess=true /* , SupportsTransactions=true,
                     HelpUri="http://go.microsoft.com/fwlink/?LinkID=113357"*/)] 
    public class NewPSDriveCommand : CoreCommandWithCredentialsBase
    {
        [Parameter(Position=0, Mandatory=true, ValueFromPipelineByPropertyName=true)]
        public string Name { get; set; }

        [Parameter(Position=1, Mandatory=true, ValueFromPipelineByPropertyName=true)]
        public string PSProvider { get; set; }

        [Parameter(Position=2, Mandatory=true, ValueFromPipelineByPropertyName=true)]
        [AllowEmptyString]
        public string Root { get; set; }

        [Parameter(ValueFromPipelineByPropertyName=true)]
        public string Description { get; set; }

        [ParameterAttribute(ValueFromPipelineByPropertyName=true)]
        public string Scope { get; set; }

        /*
        [Parameter(ValueFromPipelineByPropertyName=true)] 
        public SwitchParameter Persist { get; set; }
        */

        protected override bool ProviderSupportsShouldProcess { get { return true; } }

        protected override void ProcessRecord()
        {
            var provider = SessionState.Provider.GetOne(PSProvider);
            var driveInfo = new PSDriveInfo(Name, provider, Root, Description, Credential);
            var realDrive = SessionState.Drive.New(driveInfo, Scope ?? "local", ProviderRuntime);
            WriteObject(realDrive);
        }
    }
}

