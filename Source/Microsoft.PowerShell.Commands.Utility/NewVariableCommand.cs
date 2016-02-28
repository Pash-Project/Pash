// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
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

        [ParameterAttribute]
        public SessionStateEntryVisibility Visibility { get; set; }

        [Parameter]
        public SwitchParameter PassThru { get; set; }

        [ParameterAttribute]
        public string Scope { get; set; }

        [Parameter(Position = 1, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        public object Value { get; set; }

        public NewVariableCommand()
        {
        }

        protected override void ProcessRecord()
        {
            // TODO: deal with ShouldProcess

            var variable = new PSVariable(Name, Value, Option);
            variable.Visibility = Visibility;
            if (Description != null)
            {
                variable.Description = Description;
            }

            try
            {
                if (!Force && VariableAlreadyExists(variable))
                {
                    WriteVariableAlreadyExistsError(variable);
                    return;
                }
                SetVariable(variable);
            }
            catch (SessionStateUnauthorizedAccessException ex)
            {
                WriteError(ex.ErrorRecord);
                return;
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

        private void SetVariable(PSVariable variable)
        {
            if (Scope != null)
            {
                SessionState.PSVariable.SetAtScope(variable, Scope, Force);
            }
            else
            {
                SessionState.PSVariable.Set(variable, Force);
            }
        }

        private bool VariableAlreadyExists(PSVariable variable)
        {
            PSVariable originalVariable = SessionState.PSVariable.GetAtScope(variable.Name, Scope);
            return originalVariable != null;
        }

        private void WriteVariableAlreadyExistsError(PSVariable variable)
        {
            var ex = new SessionStateException(
                String.Format("A variable with name '{0}' already exists.", variable.Name),
                variable.Name,
                SessionStateCategory.Variable);

            string errorId = String.Format("VariableAlreadyExists,{0}", typeof(NewVariableCommand).FullName);
            var error = new ErrorRecord(ex, errorId, ErrorCategory.ResourceExists, variable.Name);
            error.CategoryInfo.Activity = "New-Variable";

            WriteError(error);
        }
    }
}
