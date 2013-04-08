// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Management.Automation;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;

namespace Microsoft.PowerShell.Commands
{
    /// <summary>
    /// NAME
    ///   ConvertTo-Html
    /// 
    /// DESCRIPTION
    ///   Exports the input object to a HTML file.
    /// 
    /// RELATED PASH COMMANDS
    ///   Import-Csv
    ///   Export-Csv
    ///   
    /// RELATED POSIX COMMANDS
    ///   n/a 
    /// </summary>
    [Cmdlet("ConvertTo", "Html")]
    public sealed class ConvertToHtmlCommand : PSCmdlet
    {
        private bool notHeader;

        protected override void BeginProcessing()
        {
            WriteObject
                ("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">\n<html>\n<head>");

            if (Head != null)
                WriteObject(Head, true);

            WriteObject("<title>" + Title + "</title>\n</head>\n<body>");

            if (Body != null)
                WriteObject(Body, true);

            WriteObject("<table>");
        }

        protected override void ProcessRecord()
        {
            WriteObject("<tr>");

            if (!notHeader)
            {
                // This has O(n^2) complexity, can it be optimized?
                if (Property != null)
                {
                    foreach (PSPropertyInfo _prop in InputObject.Properties)
                    {
                        foreach (object _match in Property)
                        {
                            if (_match.ToString() == _prop.Name)
                            {
                                WriteObject("<th>" + _prop.Name + "</th>");
                                break;
                            }
                        }
                    }
                }
                else
                {
                    foreach (PSPropertyInfo _prop in InputObject.Properties)
                    {
                        WriteObject("<th>" + _prop.Name + "</th>");
                    }
                }
                notHeader = true;

            }

            foreach (PSPropertyInfo _prop in InputObject.Properties)
            {
                WriteObject("<td>" + _prop.Value + "</td>");
            }

        }

        protected override void EndProcessing()
        {
            WriteObject("</table>\n</body>\n</html>");
        }

        /// <summary>
        /// Specify text to include in the HTML body.
        /// </summary>
        [Parameter]
        public string[] Body { get; set; }

        /// <summary>
        /// Specify text to include in the HTML head.
        /// </summary>
        [Parameter]
        public string[] Head { get; set; }

        /// <summary>
        /// The object to convert to HTML.
        /// </summary>
        [Parameter(ValueFromPipeline = true)]
        public PSObject InputObject { get; set; }

        /// <summary>
        /// The properties to include in the HTML table. The default is all properties.
        /// </summary>
        [Parameter(Position = 0)]
        public object[] Property { get; set; }

        /// <summary>
        /// Format the table in the default way for this type. This parameter is not in PowerShell.
        /// </summary>
        [Parameter]
        public SwitchParameter FormatDefault { get; set; }

        /// <summary>
        /// Specify the title of the HTML page.
        /// </summary>
        [Parameter, ValidateNotNullOrEmpty]
        public string Title { get; set; }


    }
}