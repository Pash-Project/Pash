using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands.Utility
{
    internal class ListFormatGenerator : FormatGenerator
    {
		public ListFormatGenerator(FormatGeneratorOptions options) : base(FormatShape.List, options)
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

