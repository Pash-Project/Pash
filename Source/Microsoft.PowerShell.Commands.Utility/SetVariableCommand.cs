// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Set", "Variable", SupportsShouldProcess = true)]
    [OutputType(typeof(PSVariable))]
    public sealed class SetVariableCommand : VariableCommandBase
    {
        [Parameter]
        public string Description { get; set; }

        [Parameter]
        public string[] Exclude
        {
            get { return ExcludeFilters; }
            set { ExcludeFilters = value; }
        }

        [Parameter]
        public SwitchParameter Force { get; set; }

        [Parameter]
        public string[] Include
        {
            get { return IncludeFilters; }
            set { IncludeFilters = value; }
        }

        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true, Mandatory = true)]
        public string[] Name { get; set; }

        [Parameter]
        public ScopedItemOptions Option {
            get { return _option ?? ScopedItemOptions.None; }
            set { _option = value; }
        }

        [Parameter]
        public SwitchParameter PassThru { get; set; }

        [Parameter(Position = 1, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        public object Value { get; set; }

        [ParameterAttribute]
        public SessionStateEntryVisibility Visibility { get; set; }

        private PSObject _default;
        private ArrayList _values;
        private ScopedItemOptions? _option;

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

            foreach (string name in GetNames())
            {
                try
                {
                    PSVariable variable = SetVariable(name, value);

                    if (PassThru.ToBool())
                    {
                        WriteObject(variable);
                    }
                }
                catch (SessionStateException ex)
                {
                    WriteError(ex);
                }
            }
        }

        private IEnumerable<string> GetNames()
        {
            return Name.Where(name => !IsExcluded(name));
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
            PSVariable variable = SessionState.PSVariable.GetAtScope(name, Scope);

            if (variable == null)
            {
                variable = new PSVariable(name, value, Option);
                variable.Visibility = Visibility;
            }
            else
            {
                CheckVariableCanBeChanged(variable, Force);
                variable.Value = value;
                SetVariableOptions(variable);
            }

            variable.Description = Description ?? String.Empty;

            SessionState.PSVariable.SetAtScope(variable, Scope, Force);

            return variable;
        }

        private void SetVariableOptions(PSVariable variable)
        {
            if (_option.HasValue && variable.Options != _option.Value)
            {
                CheckVariableOptionCanBeChanged(variable);
                variable.Options = _option.Value;
            }
        }

        private void CheckVariableOptionCanBeChanged(PSVariable variable)
        {
            CheckVariableCanBeChanged(variable, Force);

            if (_option == ScopedItemOptions.Constant)
            {
                throw SessionStateUnauthorizedAccessException.CreateVariableCannotBeMadeConstantError(variable);
            }
        }
    }
}
