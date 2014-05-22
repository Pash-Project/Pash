using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands.Utility
{
    [Cmdlet("Format", "Default")]
    public class FormatDefaultCommand : FormatCommandBase
    {
        public FormatDefaultCommand() : base(FormatShape.Undefined)
        {
        }
    }
}

