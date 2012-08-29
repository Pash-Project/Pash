using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands.Utility
{
    public abstract class ConsoleColorCmdlet : PSCmdlet
    {
        [ParameterAttribute]
        public ConsoleColor BackgroundColor { get; set; }

        [ParameterAttribute]
        public ConsoleColor ForegroundColor { get; set; }

        public ConsoleColorCmdlet()
        {
            this.BackgroundColor = ConsoleColor.Black;
            this.ForegroundColor = ConsoleColor.Gray;
        }
    }
}
