using System;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Remove", "Variable", SupportsShouldProcess = true)]
    public sealed class RemoveVariableCommand : PSCmdlet
    {
        [Parameter]
        public string[] Exclude { get; set; }

        [Parameter]
        public SwitchParameter Force { get; set; }

        [Parameter]
        public string[] Include { get; set; }

        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true, Mandatory = true)]
        public string[] Name { get; set; }

        public RemoveVariableCommand()
        {
        }

        protected override void ProcessRecord()
        {
            // TODO: deal with scope
            // TODO: deal with wildcards in names
            // TODO: deal with ShouldProcess

            foreach (string name in Name)
            {
                PSVariable variable = SessionState.PSVariable.Get(name);

                try
                {
                    SessionState.PSVariable.Remove(variable);
                }
                catch (Exception ex)
                {
                    WriteError(new ErrorRecord(ex, "", ErrorCategory.InvalidOperation, variable));
                    continue;
                }
            }
        }
    }
}