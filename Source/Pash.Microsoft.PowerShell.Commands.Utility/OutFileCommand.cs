using System;
using Microsoft.PowerShell.Commands.Internal.Format;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    // TODO: nice explanation here - http://www.computerperformance.co.uk/powershell/powershell_file_outfile.htm

    [Cmdlet("Out", "File", SupportsShouldProcess = true)]
    public class OutFileCommand : FrontEndCommandBase
    {
        [Parameter]
        public SwitchParameter Append { get; set; }

        [ValidateNotNullOrEmpty, ValidateSet(new string[] { "unicode", "utf7", "utf8", "utf32", "ascii", "bigendianunicode", "default", "oem" }), Parameter(Position = 1)]
        public string Encoding { get; set; }

        [Alias(new string[] { "PSPath" }), Parameter(Mandatory = true, Position = 0)]
        public string FilePath { get; set; }

        [Parameter]
        public SwitchParameter Force { get; set; }

        //NoClobber means don't over-write the file.  It seems to me that you would 
        //incorporate the -noclobber in circumstances where your intention was to save 
        //lots of files with slightly different names, and you did not want to risk 
        //losing their contents by over-writing.
        [Parameter]
        public SwitchParameter NoClobber { get; set; }

        [Parameter, ValidateRange(2, 0x7fffffff)]
        public int Width { get; set; }

        public OutFileCommand()
        {
            throw new NotImplementedException();
        }
    }
}