using System;
using System.Management.Automation;
using System.ComponentModel;
using Microsoft.PowerShell.Commands.Utility;

namespace Microsoft.PowerShell.Commands.Utility
{
    public class FormatShapeCommandBase : FormatCommandBase
    {
        [Parameter,
         ValidateSet(new string[] { "CoreOnly", "EnumOnly", "Both" }, IgnoreCase = true)]
        public string Expand { get; set; }

        [Parameter]
        public SwitchParameter Force { get; set; }

        [Parameter]
        public object GroupBy { get; set; }

        [Parameter]
        public SwitchParameter DisplayError { get; set; }

        [Parameter]
        public SwitchParameter ShowError { get; set; }

        [Parameter]
        public string View { get; set; }

        internal FormatShapeCommandBase(FormatShape shape) : base(shape)
        {
        }

        protected override void BeginProcessing()
        {
            FormatManager.Options = GetOptions();
            base.BeginProcessing();
        }

        internal FormatGeneratorOptions GetOptions()
        {
            var parsed = FormatGeneratorOptions.ExpandingOptions.EnumOnly;
            if (!String.IsNullOrEmpty(Expand))
            {
                parsed = (FormatGeneratorOptions.ExpandingOptions) 
                    Enum.Parse(typeof(FormatGeneratorOptions.ExpandingOptions), Expand);
            }
            return new FormatGeneratorOptions {
                Expand = parsed,
                Force = Force.IsPresent,
                GroupBy = GroupBy,
                DisplayError = DisplayError.IsPresent,
                ShowError = ShowError.IsPresent,
                View = View
            };
        }
    }
}

