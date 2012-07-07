using System;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Clear", "Variable", SupportsShouldProcess = true)]
    public sealed class ClearVariableCommand : PSCmdlet
    {
        [Parameter]
        public string[] Exclude { get; set; }

        [Parameter]
        public SwitchParameter Force { get; set; }

        [Parameter]
        public string[] Include { get; set; }

        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true, Mandatory = true)]
        public string[] Name { get; set; }

        [Parameter]
        public SwitchParameter PassThru { get; set; }

        public ClearVariableCommand()
        {
            // MUST: take these out into the base
            Include = new string[0];
            Exclude = new string[0];
        }

        protected override void ProcessRecord()
        {
            // TODO: deal with scope
            // TODO: deal with ShouldProcess
            // TODO: deal with read-only variables

            foreach (string name in Name)
            {
                PSVariable variable = SessionState.PSVariable.Get(name);

                try
                {
                    SessionState.PSVariable.Set(variable.Name, null);
                }
                catch (Exception ex)
                {
                    WriteError(new ErrorRecord(ex, "", ErrorCategory.InvalidOperation, variable));
                    continue;
                }
                if (PassThru.ToBool())
                {
                    WriteObject(variable);
                }
            }
        }
    }
}