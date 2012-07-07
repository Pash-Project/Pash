using System;
using System.Collections;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Set", "Variable", SupportsShouldProcess = true)]
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
            foreach(string name in Name)
            {
                foreach (object value in _values)
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

                    SessionState.PSVariable.Set(variable);
                }
            }
        }

        private void SetVariable(string[] varNames, object varValue)
        {
            throw new NotImplementedException();
        }
    }
}