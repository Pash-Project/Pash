using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("New", "Variable", SupportsShouldProcess = true)]
    public sealed class NewVariableCommand : PSCmdlet
    {
        [Parameter]
        public string Description { get; set; }

        [Parameter]
        public SwitchParameter Force { get; set; }

        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true, Mandatory = true)]
        public string Name { get; set; }

        [Parameter]
        public ScopedItemOptions Option { get; set; }

        [Parameter]
        public SwitchParameter PassThru { get; set; }

        [Parameter(Position = 1, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        public object Value { get; set; }

        public NewVariableCommand()
        {
        }

        protected override void ProcessRecord()
        {
            // TODO: deal with scope
            // TODO: deal with Force
            // TODO: deal with ShouldProcess

            PSVariable variable = new PSVariable(Name, Value, Option);
            if (Description != null)
            {
                variable.Description = Description;
            }
            try
            {
                SessionState.SessionStateGlobal.NewVariable(variable, (bool) this.Force);
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "", ErrorCategory.InvalidOperation, variable));
                return;
            }
            if (PassThru.ToBool())
            {
                WriteObject(variable);
            }
        }
    }
}