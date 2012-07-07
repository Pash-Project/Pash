using System;
using System.Management.Automation;
using Pash.Implementation;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Get", "Command", DefaultParameterSetName = "CmdletSet")]
    public sealed class GetCommandCommand : PSCmdlet
    {
        public GetCommandCommand()
        {
        }

        public string Hello;

        [AllowEmptyCollection]
        [Parameter(Position = 1, ValueFromRemainingArguments = true)]
        [AllowNull]
        public object[] ArgumentList { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = "AllCommandSet")]
        public CommandTypes CommandType { get; set; }

        [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "AllCommandSet")]
        [ValidateNotNullOrEmpty]
        public string[] Name { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = "CmdletSet")]
        public string[] Noun { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = "CmdletSet")]
        public string[] PSSnapin { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true)]
        public SwitchParameter Syntax { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true)]
        public int TotalCount { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = "CmdletSet")]
        public string[] Verb { get; set; }

        protected override void EndProcessing() 
        {
            foreach(CommandInfo cmdInfo in ((LocalRunspace)LocalRunspace.DefaultRunspace).CommandManager.FindCommands("*"))
            {
                WriteObject(cmdInfo);
            }
        }

        protected override void ProcessRecord() 
        {
            // TODO: apply wild cards
        }
    }
}
