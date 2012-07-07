using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Set", "Location", DefaultParameterSetName = "Path")]
    public class SetLocationCommand : CoreCommandBase
    {
        [Alias(new string[] { "PSPath" }), Parameter(Position = 0, ParameterSetName = "LiteralPath", Mandatory = true, ValueFromPipeline = false, ValueFromPipelineByPropertyName = true)]
        public string LiteralPath { get; set; }

        [Parameter]
        public SwitchParameter PassThru { get; set; }

        [Parameter(Position = 0, ParameterSetName = "Path", ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        public string Path { get; set; }

        [Parameter(ParameterSetName = "Stack", ValueFromPipelineByPropertyName = true)]
        public string StackName { get; set; }

        public SetLocationCommand()
        {
            Path = string.Empty;
        }

        protected override void ProcessRecord()
        {
            object obj = null;

            if (ParameterSetName == "Stack")
                obj = SessionState.Path.SetDefaultLocationStack(StackName);
            else
                obj = SessionState.Path.SetLocation(Path, ProviderRuntime);

            if (PassThru.ToBool())
                WriteObject(obj);
        }
    }
}