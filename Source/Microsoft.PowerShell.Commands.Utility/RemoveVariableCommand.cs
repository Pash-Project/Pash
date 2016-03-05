// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Linq;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Remove", "Variable", SupportsShouldProcess = true)]
    public sealed class RemoveVariableCommand : VariableCommandBase
    {
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

        public RemoveVariableCommand()
        {
        }

        protected override void ProcessRecord()
        {
            // TODO: deal with ShouldProcess

            foreach (string name in Name)
            {
                foreach (PSVariable variable in GetVariables(name)
                    .Where(v => !IsExcluded(v)))
                {
                    try
                    {
                        CheckVariableCanBeRemoved(variable);
                        string qualifiedName = GetQualifiedVariableName(variable);
                        SessionState.PSVariable.Remove(qualifiedName);
                    }
                    catch (SessionStateException ex)
                    {
                        WriteError(ex);
                    }
                }
            }
        }

        private string GetQualifiedVariableName(PSVariable variable)
        {
            if (Scope == null)
            {
                return variable.Name;
            }
            return String.Format("{0}:{1}", Scope, variable.Name);
        }

        private bool IsExcluded(PSVariable variable)
        {
            return variable.Visibility != SessionStateEntryVisibility.Public ||
                IsExcluded(variable.Name);
        }

        private void CheckVariableCanBeRemoved(PSVariable variable)
        {
            if ((variable.ItemOptions.HasFlag(ScopedItemOptions.ReadOnly) && !Force) ||
                variable.ItemOptions.HasFlag(ScopedItemOptions.Constant))
            {
                throw SessionStateUnauthorizedAccessException.CreateVariableNotRemovableError(variable);
            }
        }
    }
}
