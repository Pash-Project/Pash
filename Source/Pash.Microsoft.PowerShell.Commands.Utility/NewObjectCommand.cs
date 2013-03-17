using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
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
            var type = Type.GetType(this.TypeName);
            if (type == null) type = Type.GetType("System." + this.TypeName);
            var ctor = type.GetConstructor(Type.EmptyTypes);
            var result = ctor.Invoke(null);
            WriteObject(result);
        }
    }
}
