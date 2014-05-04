using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands.Utility
{
    internal class TableFormatGenerator : FormatGenerator
    {
        public TableFormatGenerator(FormatGeneratorOptions options) : this(new TableFormatGeneratorOptions(options))
        {
        }

        public TableFormatGenerator(TableFormatGeneratorOptions options) : base(FormatShape.Table, options)
        {
        }

        public override GroupStartData GenerateGroupStart (PSObject data)
        {
            // TODO: sburnicki
            return new GroupStartData(Shape);
        }

        public override FormatEntryData GenerateFormatEntry (PSObject data)
        {
            // TODO: sburnicki
            var entry = new FormatEntryData(Shape, data.ToString());
            var errFlag = data.Properties["writeToErrorStream"];
            entry.WriteToErrorStream = errFlag != null && errFlag.Value is bool && (bool) errFlag.Value;
            return entry;
        }

        public override GroupEndData GenerateGroupEnd ()
        {
            // TODO: sburnicki
            return new GroupEndData(Shape);
        }
    }
}

