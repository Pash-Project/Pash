using System;
using System.Management.Automation;
using System.Collections.Generic;
using Pash.Implementation;
using Extensions.Reflection;

namespace Microsoft.PowerShell.Commands.Utility
{
    internal class TableFormatGenerator : FormatGenerator
    {
        private bool _nextWithHeader;
        private Alignment[] _currentAlignments;

        private TableFormatGeneratorOptions TableOptions
        {
            get
            {
                var opts = (TableFormatGeneratorOptions) Options;
                if (opts == null)
                {
                    throw new PSInvalidOperationException("TableOptions are of the wrong type");
                }
                return opts;
            }
        }

        public TableFormatGenerator(ExecutionContext context, FormatGeneratorOptions options)
            : this(context, new TableFormatGeneratorOptions(options))
        {
        }

        public TableFormatGenerator(ExecutionContext context, TableFormatGeneratorOptions options)
            : base(context, FormatShape.Table, options)
        {
        }

        public override GroupStartData GenerateGroupStart(PSObject data)
        {
            _nextWithHeader = !TableOptions.HideTableHeaders;
            return new GroupStartData(Shape);
        }

        public override FormatEntryData GenerateObjectFormatEntry(PSObject data)
        {
            var rowData= GetSelectedProperties(data);
            var row = new List<FormatObjectProperty>();
            if (_nextWithHeader)
            {
                _currentAlignments = new Alignment[rowData.Count];
            }
            for (int i = 0; i < rowData.Count; i++)
            {
                var curData = rowData[i];
                object value = null;
                // getting the value might throw an exception, so we just print nothing for it
                try
                {
                    value = PSObject.Unwrap(curData.Value);
                } catch (GetValueException)
                {
                }
                Alignment align = Alignment.Left;
                if (_nextWithHeader)
                {
                    if (value != null)
                    {
                        align = (value.GetType().IsNumeric() || value is bool) ? Alignment.Right : Alignment.Left;
                    }
                    _currentAlignments[i] = align;
                }
                var strValue = value == null ? "" : PSObject.AsPSObject(value).ToString();
                row.Add(new FormatObjectProperty(curData.Name, strValue,
                                                 _currentAlignments[i]));
            }

            var entry = new TableFormatEntryData(row);
            if (_nextWithHeader)
            {
                entry.ShowHeader = !TableOptions.HideTableHeaders;
                _nextWithHeader = false;
            }
            entry.Wrap = TableOptions.Wrap;
            return entry;
        }
    }
}

