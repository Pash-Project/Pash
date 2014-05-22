// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Management.Automation;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.Net.Configuration;

namespace Microsoft.PowerShell.Commands
{
    /// <summary>
    /// NAME
    ///   Import-Csv
    /// 
    /// DESCRIPTION
    ///   Imports the data from a comma delimited file to an object.
    /// 
    /// RELATED PASH COMMANDS
    ///   Export-Csv
    ///   ConvertTo-Html
    ///   
    /// RELATED POSIX COMMANDS
    ///   n/a 
    /// </summary>
    [Cmdlet(VerbsData.Import, "Csv")]
    public sealed class ImportCsvCommand : PSCmdlet
    {
        private System.Text.Encoding _useEncoding;
        
        /// <summary>
        /// The path of the CSV file to import.
        /// </summary>
        [Parameter(Position = 1,
                   Mandatory = true,
                   ValueFromPipeline = true),
         Alias(new string[] { "PSPath" })]
        public String[] Path { get; set; }

        /// <summary>
        /// Specifies an alternate column header row for the imported file.
        /// </summary>
        [Parameter]
        public String[] Header { get; set; }

        /// <summary>
        /// Specifies a delimiter to separate the property values. The default is a comma (,).
        /// </summary>
        [Parameter(Position = 2)]
        public char Delimiter { get; set; }

        /// <summary>
        /// What encoding you want the input file is in.
        /// </summary>
        [Parameter,
         ValidateSet(new string[] { "Unicode", "UTF7", "UTF8", "ASCII", "UTF32", "BigEndianUnicode", "Default", "OEM" })]
        public string Encoding { get; set; }

        protected override void ProcessRecord()
        {
            foreach (String curPath in Path)
            {
                var file = OpenFile(curPath);
                string typename = "";
                if (file.Peek() == '#')
                {
                    var typeline = file.ReadLine();
                    if (typeline.StartsWith("#TYPE"))
                    {
                        typename = typeline.Substring(5).Trim();
                    }
                }

                string[] useHeader;
                if (Header != null && Header.Length > 0)
                {
                    useHeader = Header;
                }
                else
                {
                    var headerLine = file.ReadLine();
                    if (headerLine == null)
                    {
                        return;
                    }
                    // first line is property names
                    useHeader = ParseValuesFromLine(headerLine).ToArray();
                }

                string line;
                for (line = file.ReadLine(); line != null; line = file.ReadLine())
                {
                    PSObject obj = new PSObject();
                    if (!String.IsNullOrEmpty(typename))
                    {
                        obj.TypeNames.Add(typename);
                    }
                    var values = ParseValuesFromLine(line);
                    for (int i = 0; i < useHeader.Length; i++)
                    {
                        string value = (i < values.Count) ? values[i] : "";
                        var psprop = new PSNoteProperty(useHeader[i], value);
                        obj.Properties.Add(psprop);
                        obj.Members.Add(psprop);
                    }
                    WriteObject(obj);
                }
                file.Close();
            }
        }

        private StreamReader OpenFile(string path)
        {
            if (_useEncoding == null)
            {
                _useEncoding = System.Text.Encoding.ASCII;
                if (!String.IsNullOrEmpty(Encoding))
                {
                    try
                    {
                        _useEncoding = System.Text.Encoding.GetEncoding(Encoding);
                    }
                    catch (ArgumentException)
                    {
                        // shouldn't happen as Encoding gets validated
                        var msg = String.Format("Invalid encoding '{0}'", Encoding);
                        ThrowTerminatingError(new PSArgumentException(msg).ErrorRecord);
                    }
                } 
            }
            return new StreamReader(path, _useEncoding);
        }

        private List<string> ParseValuesFromLine(string line)
        {
            var values = new List<string>();
            char delim = Delimiter == 0 ? ',' : Delimiter;
            var curValue = new StringBuilder();
            bool waitForQuote = false;
            bool inValue = false;
            // Powershell behavior: skip only double quotes at the beginning of a value and ignore delimiters
            // until the next double quotes
            foreach (char c in line)
            {
                if (!inValue && c == '"')
                {
                    waitForQuote = true;
                    continue;
                }
                inValue = true;
                if (!waitForQuote && c == delim)
                {
                    values.Add(curValue.ToString());
                    curValue.Clear();
                    inValue = false;
                    continue;
                }
                if (c == '"')
                {
                    waitForQuote = false;
                    continue;
                }
                curValue.Append(c);
            }
            values.Add(curValue.ToString()); // last value still in builder (no trailing delimiter)
            return values;
        }

    }
}