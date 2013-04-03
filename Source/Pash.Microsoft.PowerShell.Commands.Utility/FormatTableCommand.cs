// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

//TODO: Still needs a ton of work.

using System;
using System.Management.Automation;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System.Text;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Format", "Table")]
    public class FormatTableCommand : PSCmdlet
    {


        bool isNotFirst;

        protected override void BeginProcessing()
        {

        }
        /*
        protected override void ProcessRecord()
        {
            List<List<String>> _list = new List<List<String>>();

            List<String> _row = new List<String>();



            if ((!isNotFirst) && (!HideTableHeaders.ToBool()))
            {
                foreach (PSPropertyInfo _prop in InputObject.Properties)
                {
                    _row.Add(_prop.Name);
                }

                _list.Add(_row);
                isNotFirst = true;
            }

            
        }*/

        protected override void ProcessRecord()
        {
            int maxProp = 5;

           // String line = "";

            FormatElement line = new FormatElement();

            line.Values = new String[maxProp];

            if ((!isNotFirst) && (!HideTableHeaders.ToBool()))
            {
                int i = 0;
                foreach (PSPropertyInfo _prop in InputObject.Properties)
                {
                    line.Values[i] = _prop.Name;

                    //todo: replace with better system
                    i++;
                    if (i == maxProp) { break; } 
                }
           }

            int j = 0;
            foreach (PSPropertyInfo _prop in InputObject.Properties)
            {
                line.Values[j] = _prop.Value.ToString();

                //todo: replace with better system
                j++;
                if (j == maxProp) { break; }
            }
            WriteObject(line);
        }

        [Parameter]
        public SwitchParameter AutoSize { get; set; }

        [Parameter]
        public SwitchParameter DisplayError { get; set; }

        [Parameter,
        ValidateSet(new string[] { "CoreOnly", "EnumOnly", "Both" }, IgnoreCase = true)]
        public string Expand { get; set; }

        [Parameter]
        public SwitchParameter Force { get; set; }

        [Parameter]
        public object GroupBy { get; set; }

        [Parameter]
        public SwitchParameter HideTableHeaders { get; set; }

        [Parameter(ValueFromPipeline = true)]
        public PSObject InputObject { get; set; }

        [Parameter(Position = 0)]
        public object[] Property { get; set; }

        [Parameter]
        public SwitchParameter ShowError { get; set; }

        [Parameter]
        public string View { get; set; }
        
        [Parameter]
        public SwitchParameter Wrap { get; set; }
    }
}
