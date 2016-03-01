// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Clear", "Variable", SupportsShouldProcess = true)]
    [OutputType(typeof(PSVariable))]
    public sealed class ClearVariableCommand : VariableCommandBase
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

                if (variable == null)
                {
                    WriteVariableNotFoundError(name);
                    continue;
                }

                try
                {
                    CheckVariableCanBeChanged(variable);
                    variable.Value = null;
                    SessionState.PSVariable.Set(variable, Force);
                }
                catch (SessionStateException ex)
                {
                    WriteError(ex);
                    continue;
                }
                if (PassThru.ToBool())
                {
                    WriteObject(variable);
                }
            }
        }

        private void CheckVariableCanBeChanged(PSVariable variable)
        {
            if ((variable.ItemOptions.HasFlag(ScopedItemOptions.ReadOnly) && !Force) ||
                variable.ItemOptions.HasFlag(ScopedItemOptions.Constant))
            {
                throw SessionStateUnauthorizedAccessException.CreateVariableNotWritableError(variable);
            }
        }
    }
}
