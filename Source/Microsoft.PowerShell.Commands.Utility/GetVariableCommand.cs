// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
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

        [Parameter]
        [ValidateNotNullOrEmpty]
        public string Scope { get; set; }

        public GetVariableCommand()
        {
        }

        protected override void ProcessRecord()
        {
            foreach (string name in Name)
            {
                PSVariable variable = Scope == null ? SessionState.PSVariable.Get(name)
                                                    : SessionState.PSVariable.GetAtScope(name, Scope);

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
