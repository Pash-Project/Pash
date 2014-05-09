using System;

namespace Microsoft.PowerShell.Commands.Utility
{
    internal abstract class OutputWriter
    {
        public bool WriteToErrorStream { get; set; }

        public abstract void WriteLine(string output);

        public virtual void Close()
        {
        }
    }
}

