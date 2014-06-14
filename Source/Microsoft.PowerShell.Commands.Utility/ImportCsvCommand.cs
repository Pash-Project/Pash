// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Linq;
using System.Management.Automation;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Collections;

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
        [Parameter(Position = 0,
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
                    // first line is property names
                    useHeader = ParseRowValuesFromStream(file).ToArray();
                    if (useHeader.Length < 1 || file.EndOfStream) // empty file
                    {
                        file.Close();
                        return;
                    }
                }

                // validate header
                if (useHeader.Distinct().Count() != useHeader.Count() ||
                    useHeader.Contains("") ||
                    useHeader.Contains(null)
                    )
                {
                    file.Close();
                    var er = new PSArgumentException("Invalid CSV header with duplicate or empty values!").ErrorRecord;
                    ThrowTerminatingError(er);
                }

                while (!file.EndOfStream)
                {
                    var values = ParseRowValuesFromStream(file);
                    if (values.Count < 1)
                    {
                        continue;
                    }
                    PSObject obj = new PSObject();
                    if (!String.IsNullOrEmpty(typename))
                    {
                        obj.TypeNames.Add("CSV:" + typename);
                    }
                    for (int i = 0; i < useHeader.Length; i++)
                    {
                        string value = (i < values.Count) ? values[i] : null;
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

        private List<string> ParseRowValuesFromStream(StreamReader stream)
        {
            var values = new List<string>();
            char delim = Delimiter == 0 ? ',' : Delimiter;
            var curValue = new StringBuilder();
            bool waitForQuote = false;
            bool inValue = false;
            // Powershell behavior: skip only double quotes at the beginning of a value and ignore delimiters
            // until the next double quotes
            while(!stream.EndOfStream)
            {
                char c = (char) stream.Read();
                // check if the value is quoted
                if (!inValue && c == '"')
                {
                    waitForQuote = true;
                    inValue = true;
                    continue;
                }
                // skip whitespaces at the beginning
                if (!inValue && (c == ' ' || c == '\t'))
                {
                    continue;
                }

                // if we're here, we're definitely inside a value
                inValue = true;
                // check if the value ends
                if (!waitForQuote && c == delim)
                {
                    values.Add(curValue.ToString());
                    curValue.Clear();
                    inValue = false;
                    continue;
                }
                // check for line end
                if (!waitForQuote && (c == '\n' || c == '\r'))
                {
                    if (c == '\r' && stream.Peek() == '\n')
                    {
                        stream.Read(); // advance one position if end of line is \r\n
                    }
                    break;
                }
                // check for quotes: take it if we're outside of quoting, otherwise check for end or escaped
                if (c == '"' && waitForQuote)
                {
                    var nextIsQuote = stream.Peek() == '"';
                    if (stream.EndOfStream || !nextIsQuote)
                    {
                        waitForQuote = false;
                        continue;
                    }
                    // check for double quote: an "escaped" quote char in a quoted string
                    if (nextIsQuote)
                    {
                        stream.Read(); //advance one position
                    }
                    // continue with appending the character
                }
                curValue.Append(c);
            }
            // last value ist still in builder (no trailing delimiter)
            values.Add(curValue.ToString());
            return values;
        }

    }
}