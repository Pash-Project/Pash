using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands.Utility
{
    [Cmdlet(VerbsData.ConvertTo, "Csv")]
    [OutputType(typeof(string))]
    public sealed class ConvertToCsvCommand : GenerateCsvCommandBase
    {
        protected override void ProcessRecord()
        {
            var lines = ProcessObject(InputObject);
            lines.ForEach(WriteObject);
        }
    }
}

