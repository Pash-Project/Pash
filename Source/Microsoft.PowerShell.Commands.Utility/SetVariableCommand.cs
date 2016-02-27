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
                PSVariable variable = SessionState.PSVariable.Get(name);

                if (variable == null)
                {
                    variable = new PSVariable(name, value);
                }
                else
                {
                    variable.Value = value;
                }

                variable.Description = Description ?? String.Empty;

                SessionState.PSVariable.Set(variable);

                if (PassThru.ToBool())
                {
                    WriteObject(variable);
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
    }
}
