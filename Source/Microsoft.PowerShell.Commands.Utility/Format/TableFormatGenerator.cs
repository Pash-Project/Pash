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

        public override GroupStartData GenerateGroupStart(PSObject data)
        {
            // TODO: sburnicki
            return new GroupStartData(Shape);
        }

        public override FormatEntryData GenerateObjectFormatEntry (PSObject data)
        {
            return new SimpleFormatEntryData(Shape, data.ToString()) {
                WriteToErrorStream =  data.WriteToErrorStream
            };
        }

        public override GroupEndData GenerateGroupEnd()
        {
            // TODO: sburnicki
            return new GroupEndData(Shape);
        }
    }
}

