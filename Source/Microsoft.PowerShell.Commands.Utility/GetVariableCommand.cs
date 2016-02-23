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

                PSVariable variable = Scope == null ? SessionState.PSVariable.Get(unescapedName, false)
                                                    : SessionState.PSVariable.GetAtScope(unescapedName, Scope, false);
                if (variable != null)
                {
                    if (variable.Visibility == SessionStateEntryVisibility.Public)
                    {
                        yield return variable;
                    }
                    else
                    {
                        WriteVariableIsPrivateError(variable.Name);
                    }
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

        private void WriteVariableIsPrivateError(string name)
        {
            var exception = new SessionStateException(String.Format("Cannot access the variable '${0}' because it is a private variable", name));
            string errorId = "VariableIsPrivate," + typeof(GetVariableCommand).FullName;
            var error = new ErrorRecord(exception, errorId, ErrorCategory.PermissionDenied, name);
            error.CategoryInfo.Activity = "Get-Variable";
            WriteError(error);
        }
    }
}
