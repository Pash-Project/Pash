using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands.Utility
{
    [Cmdlet(VerbsData.ConvertTo, "Csv")]
    public sealed class ConverToCsvCommand : GenerateCsvCommandBase
    {
        protected override void ProcessRecord()
        {
            var lines = ProcessObject(InputObject);
            lines.ForEach(WriteObject);
        }
    }
}

