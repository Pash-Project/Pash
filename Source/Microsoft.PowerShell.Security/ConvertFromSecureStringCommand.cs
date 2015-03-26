using System;
using System.Management.Automation;
using System.Security;

namespace Microsoft.PowerShell.Commands
{
    [OutputType(typeof(string))]
    [Cmdlet("ConvertFrom", "SecureString", DefaultParameterSetName="Secure"
        /*, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113287"*/)] 
    public sealed class ConvertFromSecureStringCommand : ConvertFromToSecureStringCommandBase
    {
        [Parameter(Position=0, ValueFromPipeline=true, Mandatory=true)]
        public SecureString SecureString { get; set; }

        protected override void ProcessRecord()
        {
            throw new NotImplementedException("No support for encryption/decryption of secure strings, yet.");
        }
    }
}

