using System;

namespace Microsoft.PowerShell.Commands.Utility
{
    internal abstract class OutputWriter
    {
        internal const int DefaultColumns = 100;
        internal const int DefaultRows = 400;

        public bool WriteToErrorStream { get; set; }
        public int Rows { get; internal set; }
        public int Columns { get; internal set; }

        protected OutputWriter()
            : this(DefaultRows, DefaultColumns)
        {
        }

        protected OutputWriter(int rows, int cols)
        {
            SetSize(rows, cols);
        }

        protected void SetSize(int rows, int cols)
        {
            Rows = rows;
            Columns = cols;
        }

        public abstract void WriteLine(string output);

        public virtual void Close()
        {
        }
    }
}

