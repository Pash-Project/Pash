using System.Collections.ObjectModel;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Get", "Variable")]
    public class GetVariableCommand : PSCmdlet
    {
        [Parameter]
        public string[] Exclude { get; set; }

        [Parameter]
        public string[] Include { get; set; }

        [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true), ValidateNotNull]
        public string[] Name { get; set; }

        [Parameter]
        public SwitchParameter ValueOnly { get; set; }

        public GetVariableCommand()
        {
        }

        protected override void ProcessRecord()
        {
            foreach (string name in Name)
            {
                PSVariable variable = SessionState.PSVariable.Get(name);

                if (variable != null)
                {
                    if (ValueOnly.ToBool())
                    {
                        WriteObject(variable.Value);
                    }
                    else
                    {
                        WriteObject(variable);
                    }
                }
            }
        }
    }
}