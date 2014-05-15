using System;
using System.Linq;
using System.Management.Automation;
using Pash.Implementation;
using System.Collections.Generic;

namespace Microsoft.PowerShell.Commands.Utility
{
    internal class ListFormatGenerator : FormatGenerator
    {
        public ListFormatGenerator(ExecutionContext context, FormatGeneratorOptions options)
            : base(context, FormatShape.List, options)
        {
        }

        public override FormatEntryData GenerateObjectFormatEntry(PSObject data)
        {
            var entryData = GetSelectedProperties(data);
            var entries = new List<FormatObjectProperty>();
            foreach (var curEntry in entryData)
            {
                string value = "";
                // getting the value might throw an exception!
                try
                {
                    value = PSObject.AsPSObject(curEntry.Value).ToString();
                }
                catch (GetValueException)
                {
                }
                entries.Add(new FormatObjectProperty(curEntry.Name, value, Alignment.Left));
            }
            return new ListFormatEntryData(entries);
        }
    }
}

