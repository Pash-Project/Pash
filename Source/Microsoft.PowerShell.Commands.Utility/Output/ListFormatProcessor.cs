using System;
using System.Linq;
using System.Management.Automation;
using System.Text;

namespace Microsoft.PowerShell.Commands.Utility
{
    internal class ListFormatProcessor : FormatProcessor
    {
        internal ListFormatProcessor(OutputWriter writer) : base(FormatShape.List, writer)
        {
        }

        protected override void ProcessFormatEntry(FormatEntryData data)
        {
            var list = data as ListFormatEntryData;
            if (list == null)
            {
                throw new PSInvalidOperationException("ListFormatProcessor can only process ListFormatEntryData");
            }
            OutputWriter.WriteToErrorStream = data.WriteToErrorStream;
            int maxPropNameWidth = list.Entries.Max(entry => entry.PropertyName.Length);
            int totalWidth = OutputWriter.Columns - 1; // -1 because of newline
            if (totalWidth <= 0)
            {
                totalWidth = OutputWriter.DefaultColumns - 1;
            }
            int availableForName = totalWidth - 4; // need place for " : x" where x is the first char of the value
            if (maxPropNameWidth > availableForName)
            {
                maxPropNameWidth = availableForName;
            }
            foreach (var curEntry in list.Entries)
            {
                WriteEntry(curEntry, maxPropNameWidth, totalWidth);
            }
        }

        private void WriteEntry(FormatObjectProperty entry, int maxNameWidth, int totalWidth)
        {
            bool nameWritten = false;
            string value = entry.Value;
            StringBuilder line = new StringBuilder();
            int spaceForValue = totalWidth - maxNameWidth - 3; // - space for " : "
            while (value != "")
            {
                if (!nameWritten)
                {
                    if (entry.PropertyName.Length > maxNameWidth)
                    {
                        line.Append(entry.PropertyName.Substring(0, maxNameWidth));
                    }
                    else
                    {
                        line.Append(entry.PropertyName.PadRight(maxNameWidth));
                    }
                    line.Append(" : ");
                    nameWritten = true;
                }
                else
                {
                    line.Append("".PadLeft(maxNameWidth + 3)); // + space for " : "
                }
                var len = spaceForValue > value.Length ? value.Length : spaceForValue;
                line.Append(value.Substring(0, len));
                value = value.Substring(len);
                OutputWriter.WriteLine(line.ToString());
                line.Clear();
            }
        }
    }
}

