// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

        [ParameterAttribute]
        public string Scope { get; set; }

        public ClearVariableCommand()
        {
            // MUST: take these out into the base
            Include = new string[0];
            Exclude = new string[0];
        }

        protected override void ProcessRecord()
        {
            // TODO: deal with ShouldProcess

            foreach (string name in Name)
            {
                foreach (PSVariable variable in GetVariables(name)
                    .Where(v => v.Visibility == SessionStateEntryVisibility.Public))
                {
                    try
                    {
                        CheckVariableCanBeChanged(variable);
                        variable.Value = null;
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
        }

        IEnumerable<PSVariable> GetVariables(string name)
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

        private void CheckVariableCanBeChanged(PSVariable variable)
        {
            if ((variable.ItemOptions.HasFlag(ScopedItemOptions.ReadOnly) && !Force) ||
                variable.ItemOptions.HasFlag(ScopedItemOptions.Constant))
            {
                throw SessionStateUnauthorizedAccessException.CreateVariableNotWritableError(variable);
            }
        }

        private IEnumerable<PSVariable> GetVariablesUsingWildcard(string pattern)
        {
            if (Scope == null)
            {
                return SessionState.PSVariable.Find(pattern).Values;
            }
            return SessionState.PSVariable.FindAtScope(pattern, Scope).Values;
        }

        private PSVariable GetVariable(string name)
        {
            string unescapedName = WildcardPattern.Unescape(name);
            PSVariable variable = SessionState.PSVariable.GetAtScope(unescapedName, Scope);
            if (variable == null)
            {
                WriteVariableNotFoundError(name);
            }
            return variable;
        }
    }
}
