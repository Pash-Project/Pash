using System;
using System.Security;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    public abstract class SecureStringCommandBase : PSCmdlet
    {
        protected SecureString SecureStringData { get; set; }
    }
}

