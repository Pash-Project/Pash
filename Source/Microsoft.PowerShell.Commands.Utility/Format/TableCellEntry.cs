using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands.Utility
{
    public class TableCellEntry
    {
        public string ColumnName { get; set; }
        public string Value { get; set; }
        public Alignment Align { get; set; }

        public TableCellEntry() : this("", "", Alignment.Left)
        {
        }

        public TableCellEntry(string column, string value, Alignment align)
        {
            ColumnName = column;
            Value = value;
            Align = align;
        }
    }
}

