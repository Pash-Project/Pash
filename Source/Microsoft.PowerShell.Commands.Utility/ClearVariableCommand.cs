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
        public SwitchParameter PassThru { get; set; }

        public ClearVariableCommand()
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
                        CheckVariableCanBeChanged(variable, Force);
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

        private bool IsExcluded(PSVariable variable)
        {
            return variable.Visibility != SessionStateEntryVisibility.Public ||
                IsExcluded(variable.Name);
        }
    }
}
