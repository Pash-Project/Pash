using System;
using System.Management.Automation;
using System.Security;

namespace Microsoft.PowerShell.Commands
{
    public abstract class ConvertFromToSecureStringCommandBase : SecureStringCommandBase
    {
        [Parameter(ParameterSetName="Open")]
        public byte[] Key { get; set; }

        [Parameter(Position=1, ParameterSetName="Secure")]
        public SecureString SecureKey { get; set; }
    }
}

