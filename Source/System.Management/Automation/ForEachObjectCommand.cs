// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Management.Automation;
using System.Management.Pash.Implementation;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("ForEach", "Object", SupportsShouldProcess = true, DefaultParameterSetName = "ScriptBlockSet"/*, HelpUri = "http://go.microsoft.com/fwlink/?LinkID=113300", RemotingCapability = RemotingCapability.None*/)]
    public sealed class ForEachObjectCommand : PSCmdlet
    {
        [Parameter(ParameterSetName = "PropertyAndMethodSet", ValueFromRemainingArguments = true)]
        public object[] ArgumentList { get; set; }

        [Parameter(ParameterSetName = "ScriptBlockSet")]
        public ScriptBlock Begin { get; set; }

        [Parameter(ParameterSetName = "ScriptBlockSet")]
        public ScriptBlock End { get; set; }

        [Parameter(ValueFromPipeline = true, ParameterSetName = "PropertyAndMethodSet")]
        [Parameter(ValueFromPipeline = true, ParameterSetName = "ScriptBlockSet")]
        public PSObject InputObject { get; set; }

        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "PropertyAndMethodSet")]
        [ValidateNotNullOrEmpty]
        public string MemberName { get; set; }

        [AllowEmptyCollection]
        [AllowNull]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "ScriptBlockSet")]
        // See test ForeachObjectCmdletManyScriptBlocks
        public ScriptBlock/*[]*/ Process { get; set; }

        [AllowEmptyCollection]
        [AllowNull]
        [Parameter(ParameterSetName = "ScriptBlockSet", ValueFromRemainingArguments = true)]
        public ScriptBlock[] RemainingScripts { get; set; }

        protected override void BeginProcessing()
        {
            if (this.Begin != null) throw new NotImplementedException(this.ToString());
        }

        protected override void EndProcessing()
        {
            if (this.End != null) throw new NotImplementedException(this.ToString());
        }

        protected override void ProcessRecord()
        {
            this.ExecutionContext.SetVariable("_", this.InputObject);

            var executionVisitor = new ExecutionVisitor(
                this.ExecutionContext,
                (PipelineCommandRuntime)CommandRuntime
                );

            this.Process.Ast.Visit(executionVisitor);

            this.ExecutionContext.outputStreamWriter.Write(((PipelineCommandRuntime)CommandRuntime).outputResults.Read(), true);
        }
    }
}
