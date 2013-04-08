// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;

namespace Microsoft.PowerShell.Commands
{
    public class FormatTableShape
    {
        public static String Format(FormatElement formatable)
        {
            String _row = new String(new char[] { '\0' });

            foreach (String _cell in formatable.Values)
            {
                // If the info doesn't fit into the column, strip it down and fit it in
                if (_cell.Length > formatable.ColumnSize)
                {
                    _row += _cell.Substring(0, (formatable.ColumnSize - 4)) + "... ";
                }

                else
                {
                    _row += _cell;
                    _row.PadRight(formatable.ColumnSize);
                }
            }

            if (formatable.isHeader)
            {
                _row += "\n";

                foreach (String _cell in formatable.Values)
                {
                    // Make the underlines for the headers
                    _row.PadRight((_row.Length + _cell.Length), '-');

                    // Space out the underlines
                    _row.PadRight((_row.Length + (formatable.ColumnSize - _cell.Length)));
                }

            }
            return _row;
        }
    }
}

