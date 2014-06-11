// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Management.Automation;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Reflection.Emit;
using System.Reflection;
using System.Collections.Generic;

namespace Microsoft.PowerShell.Commands
{
    /// <summary>
    /// NAME
    ///   Export-Csv
    /// 
    /// DESCRIPTION
    ///   Exports the input object to a comma delimited file.
    /// 
    /// RELATED PASH COMMANDS
    ///   Import-Csv
    ///   ConvertTo-Html
    ///   
    /// RELATED POSIX COMMANDS
    ///   n/a 
    /// </summary>
    [Cmdlet(VerbsData.Export, "Csv", SupportsShouldProcess = true)]
    public sealed class ExportCsvCommand : GenerateCsvCommandBase
    {

        private StreamWriter file;


        protected override void BeginProcessing()
        {
            if (NoClobber.ToBool())
            {
                if (File.Exists(Path))
                    ThrowTerminatingError(new ErrorRecord(
                        new IOException("File already exists. Use -Force to override."),
                        "FileExists",
                        ErrorCategory.ResourceExists,
                        null));
            }
            System.Text.Encoding useEnc = System.Text.Encoding.ASCII;
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
            file = new StreamWriter(Path, false, useEnc);
        }


        protected override void ProcessRecord()
        {
            var lines = ProcessObject(InputObject);
            lines.ForEach(file.WriteLine);
        }

        protected override void EndProcessing()
        {
            file.Flush();
            file.Close();
        }

        /// <summary>
        /// What encoding you want the output file to be in.
        /// </summary>
        [Parameter,
        ValidateSet(new string[] { "Unicode", "UTF7", "UTF8", "ASCII", "UTF32", "BigEndianUnicode", "Default", "OEM" })]
        public string Encoding { get; set; }

        /// <summary>
        /// Force the command to complete, even if a file with the same name exists.
        /// </summary>
        [Parameter]
        public SwitchParameter Force { get; set; }

        /// <summary>
        /// Do not override any files or damage any existing data.
        /// </summary>
        [Parameter]
        public SwitchParameter NoClobber { get; set; }

        /// <summary>
        /// The location for which you want to save the CSV file.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0), Alias(new string[] { "PSPath" })]
        public string Path { get; set; }

    }
}