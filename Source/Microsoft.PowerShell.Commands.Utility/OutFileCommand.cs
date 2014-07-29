// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Management.Automation;
using Microsoft.PowerShell.Commands.Internal.Format;
using Microsoft.PowerShell.Commands.Utility;

namespace Microsoft.PowerShell.Commands
{
    // TODO: nice explanation here - http://www.computerperformance.co.uk/powershell/powershell_file_outfile.htm

    [Cmdlet("Out", "File", SupportsShouldProcess = true)]
    public class OutFileCommand : OutCommandBase
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

        protected override void BeginProcessing()
        {
            System.Text.Encoding useEnc = System.Text.Encoding.UTF8;
            if (!String.IsNullOrEmpty(Encoding))
            {
                try
                {
                    useEnc = System.Text.Encoding.GetEncoding(Encoding);
                }
                catch (ArgumentException)
                {
                    // shouldn't happen as Encoding gets validated
                    var msg = String.Format("Invalid encoding '{0}'", Encoding);
                    ThrowTerminatingError(new PSArgumentException(msg).ErrorRecord);
                }
            }
            int width = Width == 0 ? -1 : Width;
            OutputWriter = new FileOutputWriter(FilePath, Append, !NoClobber, useEnc, width);
        }
    }
}
