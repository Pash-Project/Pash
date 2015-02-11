using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Test", "Path", DefaultParameterSetName="Path" /*, SupportsTransactions=true
            , HelpUri="http://go.microsoft.com/fwlink/?LinkID=113418" */)]
    public class TestPathCommand : CoreCommandWithFilteredPathsBase
    {
        protected override bool ProviderSupportsShouldProcess {
            get { return false; } // this cmdlet doesn't change anything anyhow
        }

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
                // check if only syntactical check
                if (IsValid.IsPresent)
                {
                    WriteObject(SessionState.Path.IsValid(curPath, runtime));
                    continue;
                }

                // otherwise we might check if it's a container
                if (PathType.Equals(TestPathType.Container))
                {
                    WriteObject(InvokeProvider.Item.IsContainer(curPath, ProviderRuntime));
                    continue;
                }

                // alrgiht, either any item or leaf, so check if it exists
                if (!InvokeProvider.Item.Exists(curPath, ProviderRuntime))
                {
                    WriteObject(false);
                    continue;
                }

                // if we check against leaf, make sure it's not a container
                if (PathType.Equals(TestPathType.Leaf))
                {
                    WriteObject(!InvokeProvider.Item.IsContainer(curPath));
                    continue;
                }

                // otherwise we check against any object. As it exists, we can just write true
                WriteObject(true);
            }
        }
    }
}

