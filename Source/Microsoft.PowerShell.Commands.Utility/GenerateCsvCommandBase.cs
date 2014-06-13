using System;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Collections.Generic;

namespace Microsoft.PowerShell.Commands
{
    public class GenerateCsvCommandBase : PSCmdlet
    {
        private List<string> _header;

        /// <summary>
        /// Specifies a delimiter to separate the property values. The default is a comma (,).
        /// </summary>
        [Parameter(Position = 2)]
        public char Delimiter { get; set; }

        /// <summary>
        /// The object to be exported as a CSV file.
        /// </summary>
        [Parameter(ValueFromPipeline = true, Mandatory = true, Position = 1)]
        public PSObject InputObject { get; set; }

        /// <summary>
        /// Omits type information from the generated CSV file. By the default the first line of the CSV file contains type information.
        /// </summary>
        [Parameter]
        public SwitchParameter NoTypeInformation { get; set; }

        protected List<string> ProcessObject(PSObject obj)
        {
            var lines = new List<string>();
            char delim = Delimiter == 0 ? ',' : Delimiter;

            if (_header == null)
            {
                _header = new List<string>();
                if (!NoTypeInformation.IsPresent)
                {
                    lines.Add("#TYPE " + InputObject.BaseObject.GetType().FullName);
                }
                foreach (PSPropertyInfo prop in InputObject.Properties)
                {
                    _header.Add(prop.Name);
                }
                var escaped = (from h in _header select SimpleEscapeString(h)).ToArray();
                lines.Add(String.Join(delim.ToString(), escaped));
            }

            StringBuilder line = new StringBuilder();
            foreach (var propName in _header)
            {
                var prop = InputObject.Properties[propName];
                if (prop != null && PSObject.Unwrap(prop.Value) != null)
                {
                    line.Append(SimpleEscapeString(prop.Value.ToString()));
                }
                line.Append(delim);
            }
            // Remove the trailing comma
            line.Remove((line.Length - 1), 1);
            lines.Add(line.ToString());

            return lines;
        }

        private string SimpleEscapeString(string val)
        {
            return String.Format("\"{0}\"", val.Replace("\"", "\"\""));
        }
    }
}

