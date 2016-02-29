// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    public abstract class VariableCommandBase : PSCmdlet
    {
        protected string[] ExcludeFilters { get; set; }
        protected string[] IncludeFilters { get; set; }

        protected internal bool IsExcluded(string variableName)
        {
            if (IncludeFilters != null)
            {
                if (!IncludeFilters.Any(name => IsMatch(name, variableName)))
                {
                    return true;
                }
            }

            if (ExcludeFilters != null)
            {
                if (ExcludeFilters.Any(name => IsMatch(name, variableName)))
                {
                    return true;
                }
            }

            return false;
        }

        static bool IsMatch(string name, string variableName)
        {
            if (WildcardPattern.ContainsWildcardCharacters(name))
            {
                var wildcard = new WildcardPattern(name, WildcardOptions.IgnoreCase);
                return wildcard.IsMatch(variableName);
            }
            return variableName.Equals(name, StringComparison.CurrentCultureIgnoreCase);
        }

        protected internal void WriteVariableNotFoundError(string name)
        {
            var exception = new ItemNotFoundException(String.Format("Cannot find a variable with name '{0}'.", name));
            string errorId = "VariableNotFound," + GetType().FullName;
            var error = new ErrorRecord(exception, errorId, ErrorCategory.ObjectNotFound, name);
            error.CategoryInfo.Activity = GetActivityName();
            WriteError(error);
        }

        private string GetActivityName()
        {
            return GetType().Name.Replace("VariableCommand", "-Variable");
        }

        protected internal void WriteError(SessionStateException ex)
        {
            string errorId = String.Format("{0},{1}", ex.ErrorRecord.ErrorId, GetType().FullName);
            var error = new ErrorRecord(ex, errorId, ex.ErrorRecord.CategoryInfo.Category, ex.ItemName);
            error.CategoryInfo.Activity = GetActivityName();

            WriteError(error);
        }
    }
}
