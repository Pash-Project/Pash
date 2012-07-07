using System;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Get", "Help", DefaultParameterSetName = "AllUsersView")]
    public sealed class GetHelpCommand : PSCmdlet
    {
        internal enum HelpView
        {
            Default,
            DetailedView,
            FullView,
            ExamplesView
        }

        [ValidateSet(new string[] { "Alias", "Cmdlet", "Provider", "General", "FAQ", "Glossary", "HelpFile", "All" }), Parameter]
        public string[] Category { get; set; }

        [Parameter]
        public string[] Component { get; set; }

        [Parameter(ParameterSetName = "DetailedView")]
        public SwitchParameter Detailed { set; internal get; }

        [Parameter(ParameterSetName = "Examples")]
        public SwitchParameter Examples { set; internal get; }

        [Parameter(ParameterSetName = "AllUsersView")]
        public SwitchParameter Full { set; internal get; }

        [Parameter]
        public string[] Functionality { get; set; }

        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true)]
        public string Name { get; set; }

        [Parameter(ParameterSetName = "Parameters")]
        public string Parameter { get; set; }

        [Parameter]
        public string[] Role { get; set; }

        public GetHelpCommand()
        {
            
        }

        protected override void ProcessRecord()
        {
            throw new NotImplementedException();
        }
    }
}