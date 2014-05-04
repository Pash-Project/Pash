using System;
using System.Management.Automation;
using System.ComponentModel;

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
            Options = GetOptions();
        }

        internal FormatGeneratorOptions GetOptions()
        {
            var parsed = Enum.Parse(typeof(FormatGeneratorOptions.ExpandingOptions), Expand);
            return new FormatGeneratorOptions {
                Expand = (FormatGeneratorOptions.ExpandingOptions) parsed,
                Force = Force.IsPresent,
                GroupBy = GroupBy,
                DisplayError = DisplayError.IsPresent,
                ShowError = ShowError.IsPresent,
                View = View
            };
        }
    }
}

