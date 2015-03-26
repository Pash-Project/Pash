using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "Credential", DefaultParameterSetName="CredentialSet"
        /*, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113311"*/)]
    public class GetCredentialCommand : PSCmdlet
    {
        private const string _defaultCaption = "Pash Credential Request";

        [Credential]
        [Parameter(Position=0, Mandatory=true, ParameterSetName="CredentialSet")]
        public PSCredential Credential { get; set; }

        [Parameter(Mandatory=true, ParameterSetName="MessageSet")]
        public string Message { get; set; }

        [Parameter(Position=0, Mandatory=false, ParameterSetName="MessageSet")]
        public string UserName { get; set; }

        protected override void EndProcessing()
        {
            // if the user provided a string as 'Credential' parameter, the parameter binding process has already
            // prompted the user for the password in order to 'convert' the string to PSCredential
            if (String.IsNullOrEmpty(Message))
            {
                if (Credential != null)
                {
                    WriteObject(Credential);
                }
                return;
            }
            // otherwise we query it with custom message by the host
            var credential = Host.UI.PromptForCredential(_defaultCaption, Message, UserName, "");
            WriteObject(credential);
        }
    }
}

