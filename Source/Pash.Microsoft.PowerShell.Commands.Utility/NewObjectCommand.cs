using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Text;

namespace Microsoft.PowerShell.Commands.Utility
{
    [Cmdlet("New", "Object")]
    public sealed class NewObjectCommand : PSCmdlet
    {
        [Parameter(ParameterSetName = "Net", Mandatory = false, Position = 1)]
        public object[] ArgumentList { get; set; }

        [Parameter(ParameterSetName = "Com", Mandatory = true, Position = 0)]
        public string ComObject { get; set; }

        [Parameter]
        public IDictionary Property { get; set; }

        [Parameter(ParameterSetName = "Com")]
        public SwitchParameter Strict { get; set; }

        [Parameter(/*ParameterSetName = "Net", */Mandatory = true, Position = 0)]
        public string TypeName { get; set; }

        protected override void ProcessRecord()
        {
            /*
             * In PowerShell, I ran:
             *      [System.AppDomain]::CurrentDomain.GetAssemblies() | select -ExpandProperty fullname | sort | clip
             *  And removed a couple items.
             */
            var defaultSearchAssemblies = new[] {
                // "Anonymously Hosted DynamicMethods Assembly, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"
                "Microsoft.CSharp, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
                // "Microsoft.Management.Infrastructure, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35",
                // "Microsoft.PowerShell.Commands.Management, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35",
                // "Microsoft.PowerShell.Commands.Utility, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35",
                // "Microsoft.PowerShell.ConsoleHost, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35",
                // "Microsoft.PowerShell.Security, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35",
                "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                // "PSEventHandler, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
                "System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                // "System.Configuration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
                // "System.Configuration.Install, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
                // "System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                // "System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                // "System.Data.SqlXml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                // "System.DirectoryServices, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
                // "System.Management, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
                // "System.Management.Automation, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35",
                // "System.Numerics, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                // "System.Security, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
                // "System.Transactions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                "System.Xml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
            };

            // I tried to write this in LINQ but it didn't out clean. 
            Type type = null;
            foreach (var item in defaultSearchAssemblies)
            {
                var assembly = Assembly.Load(item);
                type = type ?? assembly.GetType(this.TypeName, false, true);
                type = type ?? assembly.GetType("System." + this.TypeName, false, true);
            }

            var result = Activator.CreateInstance(type);
            WriteObject(result);
        }
    }
}
