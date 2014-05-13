using System;

namespace Microsoft.PowerShell.Commands.Utility
{
    internal abstract class OutputWriter
    {
        internal const int DefaultColumns = 80;
        internal const int DefaultRows = 100;

        public bool WriteToErrorStream { get; set; }
        public int Rows { get; private set; }
        public int Columns { get; private set; }

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

