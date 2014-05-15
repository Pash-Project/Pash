using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands.Utility
{
    public class FormatObjectProperty
    {
        public string PropertyName { get; set; }
        public string Value { get; set; }
        public Alignment Align { get; set; }

        public FormatObjectProperty() : this("", "", Alignment.Left)
        {
        }

        public FormatObjectProperty(string column, string value, Alignment align)
        {
            PropertyName = column;
            Value = value;
            Align = align;
        }
    }
}

