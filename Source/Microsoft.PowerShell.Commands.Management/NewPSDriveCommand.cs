using System;
using System.Management.Automation;
using System.Management.Automation.Provider;
using Pash.Implementation;

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

        internal override ProviderRuntime ProviderRuntime
        {
            get
            {
                var runtime = base.ProviderRuntime;
                runtime.DynamicParameters = _dynamicParameters;
                return runtime;
            }
        }

        private object _dynamicParameters;

        public override object GetDynamicParameters()
        {
            _dynamicParameters = SessionState.Drive.NewDriveDynamicParameters(PSProvider, ProviderRuntime);
            return _dynamicParameters;
        }

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

