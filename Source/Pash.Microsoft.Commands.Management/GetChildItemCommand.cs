using System;
using System.Management.Automation;
using Pash.Implementation;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Get", "ChildItem", DefaultParameterSetName = "Items")]
    public class GetChildItemCommand : CoreCommandBase
    {
        [Parameter]
        public override string[] Exclude { get; set; }

        [Parameter(Position = 1)]
        public override string Filter { get; set; }

        [Parameter]
        public override SwitchParameter Force { get; set; }

        [Parameter]
        public override string[] Include { get; set; }

        [Parameter(Position = 0, ParameterSetName = "LiteralItems", Mandatory = true, ValueFromPipeline = false, ValueFromPipelineByPropertyName = true), Alias(new string[] { "PSPath" })]
        public string[] LiteralPath { get; set; }

        [Parameter]
        public SwitchParameter Name { get; set; }

        [Parameter(Position = 0, ParameterSetName = "Items", ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        public string[] Path { get; set; }

        [Parameter]
        public SwitchParameter Recurse { get; set; }

        public GetChildItemCommand()
        {
        }

        protected override void ProcessRecord()
        {
            if ((Path == null) || (Path.Length == 0))
            {
                Path = new string[] { string.Empty };
            }

            foreach (string str in Path)
            {
                try
                {
                    if (Name.ToBool())
                    {
                        InvokeProvider.ChildItem.GetNames(str, ReturnContainers.ReturnMatchingContainers,
                                                          Recurse.ToBool(), ProviderRuntime);
                    }
                    else
                    {
                        InvokeProvider.ChildItem.Get(str, Recurse.ToBool(), ProviderRuntime);
                    }
                }
                catch (Exception ex)
                {
                    WriteError(new ErrorRecord(ex, "", ErrorCategory.InvalidOperation, this));
                }
            }
        }
    }
}