// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Get", "Variable")]
    [OutputType(typeof(PSVariable))]
    public class GetVariableCommand : VariableCommandBase
    {
        [Parameter]
        public string[] Exclude
        {
            get { return ExcludeFilters; }
            set { ExcludeFilters = value; }
        }

        [Parameter]
        public string[] Include
        {
            get { return IncludeFilters; }
            set { IncludeFilters = value; }
        }

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
            foreach (PSVariable variable in GetVariables()
                .Where(v => v.Visibility == SessionStateEntryVisibility.Public)
                .OrderBy(v => v.Name))
            {
                if (!IsExcluded(variable.Name))
                {
                    WriteVariable(variable);
                }
            }
        }

        private IEnumerable<PSVariable> GetVariables()
        {
            if (Name == null)
            {
                return GetAllVariables();
            }
            return GetAllVariablesByName();
        }

        private IEnumerable<PSVariable> GetAllVariablesByName()
        {
            foreach (string name in Name)
            {
                foreach (PSVariable variable in GetVariables(name))
                {
                    yield return variable;
                }
            }
        }

        private IEnumerable<PSVariable> GetVariables(string name)
        {
            if (WildcardPattern.ContainsWildcardCharacters(name))
            {
                foreach (PSVariable variable in GetVariablesUsingWildcard(name))
                {
                    yield return variable;
                }
            }
            else
            {
                PSVariable variable = GetVariable(name);
                if (variable != null)
                {
                    yield return variable;
                }
            }
        }

        private PSVariable GetVariable(string name)
        {
            try
            {
                string unescapedName = WildcardPattern.Unescape(name);

                PSVariable variable = Scope == null ? SessionState.PSVariable.Get(unescapedName)
                                                    : SessionState.PSVariable.GetAtScope(unescapedName, Scope);
                if (variable != null)
                {
                    return variable;
                }
                else
                {
                    WriteVariableNotFoundError(name);
                }
                return variable;
            }
            catch (SessionStateException ex)
            {
                WriteError(ex);
            }
            return null;
        }

        private IEnumerable<PSVariable> GetVariablesUsingWildcard(string pattern)
        {
            if (Scope == null)
            {
                return SessionState.PSVariable.Find(pattern).Values;
            }
            return SessionState.PSVariable.FindAtScope(pattern, Scope).Values;
        }

        private IEnumerable<PSVariable> GetAllVariables()
        {
            if (Scope == null)
            {
                return SessionState.PSVariable.GetAll().Values;
            }
            return SessionState.PSVariable.GetAllAtScope(Scope).Values;
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
