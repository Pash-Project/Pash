using System;
using System.IO;
using System.Management.Automation.Host;

namespace Microsoft.PowerShell.Commands.Utility
{
    internal class HostOutputWriter : OutputWriter
    {
        private PSHost _host;

        public HostOutputWriter(PSHost host) : base()
        {
            try
            {
                Rows = host.UI.RawUI.BufferSize.Height;
                Columns = host.UI.RawUI.BufferSize.Width;
            }
            catch (IOException)
            {
                // this could occur if Pash is used in a process and the I/O is redirected. We just use the default values then
            }
            _host = host;
        }

        public override void WriteLine(string output)
        {
            if (WriteToErrorStream)
            {
                _host.UI.WriteErrorLine(output);
            }
            else
            {
                _host.UI.WriteLine(output);
            }
        }
    }
}

