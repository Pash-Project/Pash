using System;
using System.Management.Automation.Host;

namespace Microsoft.PowerShell.Commands.Utility
{
    internal class HostOutputWriter : OutputWriter
    {
        private PSHost _host;

        public HostOutputWriter(PSHost host)
            : base(host.UI.RawUI.WindowSize.Height, host.UI.RawUI.WindowSize.Width)
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

