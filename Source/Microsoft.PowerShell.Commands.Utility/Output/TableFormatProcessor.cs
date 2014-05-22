using System;
using System.Linq;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Management.Automation;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using Extensions.Enumerable;

namespace Microsoft.PowerShell.Commands.Utility
{

    internal class TableFormatProcessor : FormatProcessor
    {
        private KeyValuePair<string, int>[] _currentColumns;
        private int _fullWidth;

        internal TableFormatProcessor(OutputWriter writer) : base(FormatShape.Table, writer)
        {
        }

        protected override void ProcessFormatEntry(FormatEntryData data)
        {
            var tableEntry = data as TableFormatEntryData;
            if (tableEntry == null)
            {
                throw new PSInvalidOperationException("TableFormatProcessor can only process TableFormatEntryData");
            }
            OutputWriter.WriteToErrorStream = data.WriteToErrorStream;
            if (_currentColumns == null)
            {
                CalculateColumns(tableEntry.Row);
            }
            if (tableEntry.ShowHeader)
            {
                WriteHeader(tableEntry);
            }
            WriteRow(tableEntry);
        }

        protected override void ProcessGroupEnd(GroupEndData data)
        {
            // make sure column widths are computed again for the next group
            _currentColumns = null; 
            base.ProcessGroupEnd(data);
        }

        private void CalculateColumns(List<FormatObjectProperty> row)
        {
            _fullWidth = OutputWriter.Columns - 1; // -1 because of newline
            if (_fullWidth <= 0)
            {
                _fullWidth = OutputWriter.DefaultColumns - 1;
            }
            var cols = row.Count;
            // make sure it fits
            if (2 * cols - 1 > _fullWidth)
            {
                cols = (_fullWidth + 1) / 2;
                string format = "Warning: {0} columns have to be omitted in this format" +
                                "as they don't fit the available width.";
                OutputWriter.WriteLine(String.Format(format, row.Count - cols));
            }
            int perColumn = (_fullWidth - cols + 1) / cols;
            int rest = _fullWidth - cols + 1 - perColumn * cols; // because of rounding
            _currentColumns = new KeyValuePair<string, int>[cols];
            for (int i = 0; i < cols; i++)
            {
                _currentColumns[i] = new KeyValuePair<string,int>(row[i].PropertyName,
                                                     i < rest ? perColumn + 1 : perColumn);
            }
        }

        private void WriteHeader(TableFormatEntryData tableEntry)
        {
            List<string> values = (from cell in tableEntry.Row select cell.PropertyName).ToList();
            var referenceEntry = new TableFormatEntryData(tableEntry);
            referenceEntry.Wrap = true;
            WriteValuesInRows(values, referenceEntry);
            // now print borders
            StringBuilder line = new StringBuilder(_fullWidth);
            OutputWriter.WriteToErrorStream = referenceEntry.WriteToErrorStream;
            for (int i = 0; i < _currentColumns.Length; i++)
            {
                var cellWidth = _currentColumns[i].Value;
                var curVal = values[i];
                var borderWidth = curVal.Length > cellWidth ? cellWidth : curVal.Length;
                var borderLine = "".PadLeft(borderWidth, '-');
                if (referenceEntry.Row[i].Align.Equals(Alignment.Right))
                {
                    borderLine = borderLine.PadLeft(cellWidth);
                }
                else
                {
                    borderLine = borderLine.PadRight(cellWidth);
                }
                line.Append(borderLine);
                if (i < values.Count - 1)
                {
                    line.Append(" ");
                }
            }
            OutputWriter.WriteLine(line.ToString());
        }

        private void WriteRow(TableFormatEntryData tableEntry)
        {
            var rowEntries = new Dictionary<string, string>();
            foreach (var cell in tableEntry.Row)
            {
                rowEntries[cell.PropertyName] = cell.Value;
            }
            var values = (from column in _currentColumns
                          select rowEntries.ContainsKey(column.Key) ? rowEntries[column.Key] : ""
                          ).ToList();
            WriteValuesInRows(values, tableEntry);
        }

        private void WriteValuesInRows(List<string> originalValues, TableFormatEntryData referenceEntry)
        {
            StringBuilder line = new StringBuilder(_fullWidth);
            var values = originalValues.ToArray();
            OutputWriter.WriteToErrorStream = referenceEntry.WriteToErrorStream;
            var printOneMoreLine = true;
            while (printOneMoreLine)
            {
                printOneMoreLine = false;
                for (int i = 0; i < _currentColumns.Length; i++)
                {
                    var alignRight = referenceEntry.Row[i].Align.Equals(Alignment.Right);
                    string rest;
                    line.Append(TrimString(values[i], _currentColumns[i].Value, alignRight,
                                           !referenceEntry.Wrap, out rest));
                    values[i] = referenceEntry.Wrap ? rest : "";
                    printOneMoreLine = printOneMoreLine || (referenceEntry.Wrap && !String.IsNullOrEmpty(rest));
                    // add space between columns
                    if (i < values.Length - 1)
                    {
                        line.Append(" ");
                    }
                }
                OutputWriter.WriteLine(line.ToString());
                line.Clear();
            }
        }

        private string TrimString(string str, int width, bool alignRight, bool useDotsIfPossible, out string rest)
        {
            if (str.Length < width)
            {
                rest = "";
                return alignRight ? str.PadLeft(width) : str.PadRight(width);
            }
            string dots = useDotsIfPossible && width > 3 ? "..." : "";
            string shortened;
            if (alignRight)
            {
                shortened = str.Substring(str.Length - width + dots.Length);
                rest = str.Substring(0, str.Length - width + dots.Length);
            }
            else
            {
                shortened = str.Substring(0, width - dots.Length);
                rest = str.Substring(width - dots.Length);
            }
            StringBuilder sb = new StringBuilder();
            sb.Append(alignRight ? dots : "");
            sb.Append(shortened);
            sb.Append(alignRight ? "" : dots);
            return sb.ToString();
        }
    }
}

