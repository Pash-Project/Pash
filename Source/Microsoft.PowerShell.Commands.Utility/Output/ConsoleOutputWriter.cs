using System;
using System.Management.Automation.Host;

namespace Microsoft.PowerShell.Commands.Utility
{
    internal class ConsoleOutputWriter : OutputWriter
    {
        private PSHost _host;

        public ConsoleOutputWriter(PSHost host)
        {
            _host = host;
        }

        public override void WriteLine(string output)
        {
            _host.UI.WriteLine(output);
        }

        public override void WriteErrorLine(string output)
        {
            _host.UI.WriteErrorLine(output);
        }
    }
}

