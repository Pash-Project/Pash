using System;
using System.Management.Automation;
using Pash.Implementation;

namespace Microsoft.PowerShell.Commands.Utility
{
    internal class ListFormatGenerator : FormatGenerator
    {
        public ListFormatGenerator(ExecutionContext context, FormatGeneratorOptions options)
            : base(context, FormatShape.List, options)
        {
        }

        public override GroupStartData GenerateGroupStart(PSObject data)
        {
            throw new NotImplementedException();
        }

        public override FormatEntryData GenerateObjectFormatEntry(PSObject data)
        {
            throw new NotImplementedException();
        }
    }
}

