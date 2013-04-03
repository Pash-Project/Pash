// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Management.Automation;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Reflection.Emit;
using System.Reflection;

namespace Microsoft.PowerShell.Commands
{
    /// <summary>
    /// NAME
    ///   Export-Csv
    /// 
    /// DESCRIPTION
    ///   Exports the input object to a comma deliminated file.
    /// 
    /// RELATED PASH COMMANDS
    ///   Import-Csv
    ///   ConvertTo-Html
    ///   
    /// RELATED POSIX COMMANDS
    ///   n/a 
    /// </summary>
    [Cmdlet("Export", "Csv", SupportsShouldProcess = true)]
    public sealed class ExportCsvCommand : PSCmdlet
    {

        private StreamWriter file;
        private bool typeWritten;
        

        protected override void BeginProcessing()
        {
            if (NoClobber.ToBool())
            {
                if (File.Exists(Path))
                    WriteError(new ErrorRecord(
                        new Exception("File already exists. Use -Force to override."),
                        "FileExists",
                        ErrorCategory.ResourceExists,
                        null));
            }

            file = new StreamWriter(Path);
        }


        protected override void ProcessRecord()
        {
            StringBuilder line = new StringBuilder();

            if ((!NoTypeInformation.ToBool()) && (!typeWritten))
            {
                file.WriteLine("#TYPE " + InputObject.GetType().ToString());
                typeWritten = true;
            }

            foreach (PSPropertyInfo _prop in InputObject.Properties)
            {
                line.Append(_prop.Value.ToString());
                line.Append(',');
            }

            // Remove the trailing comma
            line.Remove((line.Length - 1), 1);

            file.WriteLine(line.ToString());
         
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
        /// The object to be exported as a CSV file.
        /// </summary>
        [Parameter(ValueFromPipeline = true, Mandatory = true)]
        public PSObject InputObject { get; set; }

        /// <summary>
        /// Do not override any files or damage any existing data.
        /// </summary>
        [Parameter]
        public SwitchParameter NoClobber { get; set; }

        /// <summary>
        /// Omits type information from the generated CSV file. By the default the first line of the CSV file contains type information.
        /// </summary>
        [Parameter]
        public SwitchParameter NoTypeInformation { get; set; }

        /// <summary>
        /// The location for which you want to save the CSV file.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0), Alias(new string[] { "PSPath" })]
        public string Path { get; set; }

    }
}