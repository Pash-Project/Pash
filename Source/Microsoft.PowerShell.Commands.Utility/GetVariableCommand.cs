// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Get", "Variable")]
    [OutputType(typeof(PSVariable))]
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
            if (Name == null)
            {
                WriteAllVariables();
                return;
            }

            foreach (string name in Name)
            {
                PSVariable variable = Scope == null ? SessionState.PSVariable.Get(name)
                                                    : SessionState.PSVariable.GetAtScope(name, Scope);

                if (variable != null)
                {
                    if (!IsExcluded(variable))
                    {
                        WriteVariable(variable);
                    }
                }
            }
        }

        private void WriteAllVariables()
        {
            foreach (PSVariable variable in GetAllVariables())
            {
                if (!IsExcluded(variable))
                {
                    WriteVariable(variable);
                }
            }
        }

        private IEnumerable<PSVariable> GetAllVariables()
        {
            if (Scope == null)
            {
                return SessionState.PSVariable.GetAll().Values;
            }
            return SessionState.PSVariable.GetAllAtScope(Scope).Values;
        }

        private bool IsExcluded(PSVariable variable)
        {
            if (Include != null)
            {
                if (!Include.Any(name => IsMatch(name, variable)))
                {
                    return true;
                }
            }

            if (Exclude != null)
            {
                if (Exclude.Any(name => IsMatch(name, variable)))
                {
                    return true;
                }
            }

            return false;
        }

        static bool IsMatch(string name, PSVariable variable)
        {
            return variable.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase);
        }

        private void WriteVariable(PSVariable variable)
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
