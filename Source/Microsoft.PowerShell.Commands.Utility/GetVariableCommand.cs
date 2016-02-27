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
            try
            {
                foreach (PSVariable variable in GetVariables()
                    .Where(v => v.Visibility == SessionStateEntryVisibility.Public)
                    .OrderBy(v => v.Name))
                {
                    if (!IsExcluded(variable))
                    {
                        WriteVariable(variable);
                    }
                }
            }
            catch (SessionStateException ex)
            {
                WriteError(ex);
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
                string unescapedName = WildcardPattern.Unescape(name);

                PSVariable variable = Scope == null ? SessionState.PSVariable.Get(unescapedName)
                                                    : SessionState.PSVariable.GetAtScope(unescapedName, Scope);
                if (variable != null)
                {
                    yield return variable;
                }
                else
                {
                    WriteVariableNotFoundError(name);
                }
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
            if (WildcardPattern.ContainsWildcardCharacters(name))
            {
                var wildcard = new WildcardPattern(name, WildcardOptions.IgnoreCase);
                return wildcard.IsMatch(variable.Name);
            }
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

        private void WriteVariableNotFoundError(string name)
        {
            var exception = new ItemNotFoundException(String.Format("Cannot find a variable with name '{0}'.", name));
            string errorId = "VariableNotFound," + typeof(GetVariableCommand).FullName;
            var error = new ErrorRecord(exception, errorId, ErrorCategory.ObjectNotFound, name);
            error.CategoryInfo.Activity = "Get-Variable";
            WriteError(error);
        }

        private void WriteError(SessionStateException ex)
        {
            string errorId = String.Format("{0},{1}", ex.ErrorRecord.ErrorId, typeof(GetVariableCommand).FullName);
            var error = new ErrorRecord(ex, errorId, ex.ErrorRecord.CategoryInfo.Category, ex.ItemName);
            error.CategoryInfo.Activity = "Get-Variable";

            WriteError(error);
        }
    }
}
