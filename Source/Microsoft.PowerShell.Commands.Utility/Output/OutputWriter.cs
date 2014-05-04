using System;

namespace Microsoft.PowerShell.Commands.Utility
{
    internal abstract class OutputWriter
    {
        public abstract void WriteLine(string output);
        public abstract void WriteErrorLine(string output);

        public virtual void Close()
        {
        }
    }
}

