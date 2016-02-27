// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Set", "Variable", SupportsShouldProcess = true)]
    [OutputType(typeof(PSVariable))]
    public sealed class SetVariableCommand : PSCmdlet
    {
        [Parameter]
        public string Description { get; set; }

        [Parameter]
        public string[] Exclude { get; set; }

        [Parameter]
        public SwitchParameter Force { get; set; }

        [Parameter]
        public string[] Include { get; set; }

        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true, Mandatory = true)]
        public string[] Name { get; set; }

        [Parameter]
        public ScopedItemOptions Option { get; set; }

        [Parameter]
        public SwitchParameter PassThru { get; set; }

        [Parameter(Position = 1, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        public object Value { get; set; }

        private PSObject _default;
        private ArrayList _values;

        //private bool nameIsFormalParameter;
        //private ScopedItemOptions? options;
        //private bool valueIsFormalParameter;
        //private ArrayList valueList;

        public SetVariableCommand()
        {
            _default = new PSObject();
            Value = _default;
            _values = new ArrayList();
        }

        protected override void ProcessRecord()
        {
            if (Value != _default)
            {
                _values.Add(Value);
            }
        }

        protected override void EndProcessing()
        {
            object value = GetVariableValue();

            foreach (string name in Name)
            {
                try
                {
                    PSVariable variable = SetVariable(name, value);

                    if (PassThru.ToBool())
                    {
                        WriteObject(variable);
                    }
                }
                catch (SessionStateUnauthorizedAccessException ex)
                {
                    WriteUnauthorizedError(ex, name);
                    return;
                }
            }
        }

        private object GetVariableValue()
        {
            if (_values.Count == 1)
            {
                return _values[0];
            }
            else if (_values.Count == 0)
            {
                return null;
            }

            return _values.ToArray();
        }

        private PSVariable SetVariable(string name, object value)
        {
            PSVariable variable = SessionState.PSVariable.Get(name);

            if (variable == null)
            {
                variable = new PSVariable(name, value, Option);
            }
            else
            {
                variable.Value = value;
                SetVariableOptions(variable);
            }

            variable.Description = Description ?? String.Empty;

            SessionState.PSVariable.Set(variable);

            return variable;
        }

        private void SetVariableOptions(PSVariable variable)
        {
            if (variable.Options != Option)
            {
                CheckVariableOptionCanBeChanged(variable);
                variable.Options = Option;
            }
        }

        private void CheckVariableOptionCanBeChanged(PSVariable variable)
        {
            if ((variable.ItemOptions.HasFlag(ScopedItemOptions.ReadOnly)) ||
                variable.ItemOptions.HasFlag(ScopedItemOptions.Constant))
            {
                throw SessionStateUnauthorizedAccessException.CreateVariableNotWritableError(variable);
            }
            else if (Option == ScopedItemOptions.Constant)
            {
                throw SessionStateUnauthorizedAccessException.CreateVariableCannotBeMadeConstantError(variable);
            }
        }

        private void WriteUnauthorizedError(SessionStateUnauthorizedAccessException ex, string name)
        {
            string errorId = String.Format("{0},{1}", ex.ErrorRecord.ErrorId, typeof(SetVariableCommand).FullName);
            var error = new ErrorRecord(ex, errorId, ErrorCategory.WriteError, name);
            error.CategoryInfo.Activity = "Set-Variable";

            WriteError(error);
        }
    }
}
