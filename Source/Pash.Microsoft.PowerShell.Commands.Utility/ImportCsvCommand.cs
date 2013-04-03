// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
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
    ///   Imports the from a comma deliminated file to an object.
    /// 
    /// RELATED PASH COMMANDS
    ///   Export-Csv
    ///   ConvertTo-Html
    ///   
    /// RELATED POSIX COMMANDS
    ///   n/a 
    /// </summary>
    [Cmdlet("Import", "Csv")]
    public sealed class ImportCsvCommand : PSCmdlet
    {
        //todo
        protected override void ProcessRecord()
        {
            foreach (String _path in Path)
            {
                StreamReader _file = new StreamReader(_path);
                PSObject _obj = new PSObject();

                if (_file.Peek() == '#')
                {
                    String _line = _file.ReadLine();
                    if (_line.StartsWith("#TYPE"))
                    {
                        _obj.TypeNames.Add(_line.Substring(5).Trim());
                    }
                }


                // Hold the names of the properties, the capacity is pretty arbitrary
                List<String> _names = new List<String>(16);

                bool inValues = false;

                while (!_file.EndOfStream)
                {
                    char _ch = (char)_file.Read();
                }
                /* if ((_ch == ',')
                 {
                     _obj.Properties.Add(new PSNoteProperty());
                 } */
            }
        }

        /// <summary>
        /// The path of the CSV file to import.
        /// </summary>
        [Parameter(Position = 0,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true),
        Alias(new string[] { "PSPath" })]
        public String[] Path { get; set; }


    }
}