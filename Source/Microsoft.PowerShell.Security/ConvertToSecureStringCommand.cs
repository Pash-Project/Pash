using System;
using System.Management.Automation;
using System.Security;

namespace Microsoft.PowerShell.Commands
{
    [OutputType(typeof(SecureString))]
    [Cmdlet("ConvertTo", "SecureString", DefaultParameterSetName="Secure"
        /*, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113291"*/)] 
    public sealed class ConvertToSecureStringCommand : ConvertFromToSecureStringCommandBase
    {
        [Parameter(Position=0, ValueFromPipeline=true, Mandatory=true)]
        public string String { get; set; }

        [Parameter(Position=1, ParameterSetName="PlainText")]
        public SwitchParameter AsPlainText { get; set; }

        [Parameter(Position=2, ParameterSetName="PlainText")]
        public SwitchParameter Force { get; set; }

        protected override void ProcessRecord()
        {
            if (!AsPlainText.IsPresent)
            {
                throw new NotImplementedException("No support for encryption/decryption of secure strings, yet.");
            }
            ConvertFromPlaintext();
        }

        private void ConvertFromPlaintext()
        {
            const string msg = "Converting plain text to a SecureString is not secure as the information can be leaked.";
            if (!Force && !ShouldContinue("Do you want to proceed?", msg))
            {
                return;
            }
            var secureStr = new SecureString();
            foreach (var c in String)
            {
                secureStr.AppendChar(c);
            }
            WriteObject(secureStr);
        }
    }
}

