using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Remove", "PSDrive", DefaultParameterSetName="Name", SupportsShouldProcess=true
                    /*, SupportsTransactions=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113376"*/)] 
    public class RemovePSDriveCommand : DriveMatchingCoreCommandBase
    {
        [Parameter(Position=0, ParameterSetName="LiteralName", Mandatory=true, ValueFromPipeline=false,
                            ValueFromPipelineByPropertyName=true)]
        public string[] LiteralName { get; set; }

        [AllowEmptyCollection]
        [AllowNull]
        [Parameter(Position=0, ParameterSetName="Name", Mandatory=true, ValueFromPipelineByPropertyName=true)]
        public string[] Name { get; set; }

        [ParameterAttribute(ValueFromPipelineByPropertyName=true)]
        public string Scope { get; set; }

        [Parameter(ValueFromPipelineByPropertyName=true)]
        public string[] PSProvider { get; set; }

        [Parameter]
        public override SwitchParameter Force { get; set; }

        protected override void ProcessRecord()
        {
            var drives = GetDrives(LiteralName, Name, PSProvider, Scope);
            foreach (var drive in drives)
            {
                SessionState.Drive.Remove(drive, Scope, ProviderRuntime);
            }
        }

    }
}

