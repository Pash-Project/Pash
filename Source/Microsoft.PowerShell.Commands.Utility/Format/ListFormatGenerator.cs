using System;

namespace Microsoft.PowerShell.Commands.Utility
{
    internal class ListFormatGenerator : FormatGenerator
    {
		public ListFormatGenerator(FormatGeneratorOptions options) : base(FormatShape.List, options)
        {
        }

        public override GroupStartData GenerateGroupStart (System.Management.Automation.PSObject data)
        {
            throw new NotImplementedException ();
        }

        public override FormatEntryData GenerateFormatEntry (System.Management.Automation.PSObject data)
        {
            throw new NotImplementedException ();
        }

        public override GroupEndData GenerateGroupEnd ()
        {
            throw new NotImplementedException ();
        }
    }
}

