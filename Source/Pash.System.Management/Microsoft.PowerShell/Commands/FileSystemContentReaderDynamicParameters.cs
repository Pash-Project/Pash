using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    public class FileSystemContentReaderDynamicParameters : FileSystemContentDynamicParametersBase
    {
        [Parameter]
        public string Delimiter { get; set; }
        public bool DelimiterSpecified { get; private set; }
        [Parameter]
        public SwitchParameter Wait { get; set; }

        public FileSystemContentReaderDynamicParameters()
        {
            Delimiter = "\n";
        }
    }
}