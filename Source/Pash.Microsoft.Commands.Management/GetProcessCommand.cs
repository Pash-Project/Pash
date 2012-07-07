using System;
using System.Diagnostics;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Get", "Process", DefaultParameterSetName = "Name")]
    public sealed class GetProcessCommand : ProcessBaseCommand
    {
        [Parameter(ParameterSetName = "Id", Mandatory = true, ValueFromPipelineByPropertyName = true)]
        public int[] Id
        {
            get
            {
                return _processIds;
            }
            set
            {
                _matchType = MatchType.ById;
                _processIds = value;
            }
        }

        [Alias(new string[] { "ProcessName" }), Parameter(Position = 0, ParameterSetName = "Name", ValueFromPipelineByPropertyName = true), ValidateNotNullOrEmpty]
        public string[] Name
        {
            get
            {
                return _processNames;
            }
            set
            {
                _matchType = MatchType.ByName;
                _processNames = value;
            }
        }

        public GetProcessCommand()
        {
        }

        protected override void ProcessRecord()
        {
            foreach (Process process in FindProcesses())
            {
                WriteObject(process);
            }
        }
    }
}