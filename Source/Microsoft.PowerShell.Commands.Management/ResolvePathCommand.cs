using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands.Management
{
    [Cmdlet("Resolve", "Path", DefaultParameterSetName="Path"
        /*, SupportsTransactions=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113384"*/)]
    public class ResolvePathCommand : CoreCommandWithPathsBase
    {
        [Parameter]
        public SwitchParameter Relative { get; set; }

        protected override void ProcessRecord()
        {

        }
    }
}

