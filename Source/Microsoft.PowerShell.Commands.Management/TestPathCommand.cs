using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Test", "Path", DefaultParameterSetName="Path" /*, SupportsTransactions=true
            , HelpUri="http://go.microsoft.com/fwlink/?LinkID=113418" */)]
    public class TestPathCommand : CoreCommandWithPathsBase
    {
        [Alias("Type")]
        [Parameter]
        public TestPathType PathType { get; set; }

        [Parameter]
        public SwitchParameter IsValid { get; set; }

        // TODO: support for DynamicParameters (calling the providers appropriate method)

        protected override void ProcessRecord()
        {
            var runtime = ProviderRuntime;
            foreach (var curPath in InternalPaths)
            {
                if (IsValid.IsPresent)
                {
                    WriteObject(SessionState.Path.IsValid(curPath, runtime));
                    continue;
                }

                throw new NotImplementedException("Only syntax checks for paths are already implemented");
            }
        }
    }
}

