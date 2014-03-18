// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Reflection;
using System.Text;

namespace Microsoft.PowerShell.Commands.Utility
{
    [Cmdlet("New", "Object", DefaultParameterSetName = "Net")]
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

        [Parameter(ParameterSetName = "Net", Mandatory = true, Position = 0)]
        public string TypeName { get; set; }

        protected override void ProcessRecord()
        {
            Type type = new TypeName(this.TypeName).GetReflectionType();

            var result = Activator.CreateInstance(type, this.ArgumentList);
            WriteObject(result);
        }
    }
}
