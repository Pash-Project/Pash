using System;
using System.Management.Automation.Host;

namespace Microsoft.PowerShell.Commands.Utility
{
    internal class HostOutputWriter : OutputWriter
    {
        private PSHost _host;

        public HostOutputWriter(PSHost host)
        {
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

