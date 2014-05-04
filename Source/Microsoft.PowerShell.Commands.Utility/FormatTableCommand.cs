// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

//TODO: Still needs a ton of work.

using System;
using System.Management.Automation;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System.Text;
using Microsoft.PowerShell.Commands.Utility;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Format", "Table")]
    public class FormatTableCommand : FormatShapeCommandBase
    {
        public FormatTableCommand() : base(FormatShape.Table)
        {
        }

        [Parameter]
        public SwitchParameter AutoSize { get; set; }

        [Parameter]
        public SwitchParameter HideTableHeaders { get; set; }

        [Parameter(Position = 0)]
        public object[] Property { get; set; }

        [Parameter]
        public SwitchParameter Wrap { get; set; }

        protected override void BeginProcessing()
        {
            var tableOptions = new TableFormatGeneratorOptions(GetOptions());
            tableOptions.AutoSize = AutoSize.IsPresent;
            tableOptions.HideTableHeaders = HideTableHeaders.IsPresent;
            tableOptions.Wrap = Wrap.IsPresent;
            tableOptions.Properties = Property;
            Options = tableOptions;
        }
    }
}
