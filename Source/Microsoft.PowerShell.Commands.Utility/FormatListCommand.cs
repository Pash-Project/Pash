// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

//Classtodo: Still needs a ton of work.

using System;
using System.Management.Automation;
using Microsoft.PowerShell.Commands.Utility;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Format", "List")]
    public class FormatListCommand : FormatShapeCommandBase
    {
        
        [Parameter(Position = 0)]
        public object[] Property { get; set; }

        public FormatListCommand() : base(FormatShape.List)
        {
        }

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            FormatManager.Options.Properties = Property;
        }
    }
}

