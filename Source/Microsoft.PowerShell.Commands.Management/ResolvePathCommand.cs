using System;
using System.Linq;
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
            var resolved = SessionState.Path.GetResolvedPSPathFromPSPath(InternalPaths, ProviderRuntime);
            if (!Relative.IsPresent)
            {
                WriteObject(resolved, true);
                return;
            }
            var currentLocation = SessionState.Path.CurrentLocation.Path;
            var relatives = from r in resolved select SessionState.Path.NormalizeRelativePath(r.Path, currentLocation, ProviderRuntime);
            WriteObject(relatives, true);
        }
    }
}

