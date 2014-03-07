// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands.Utility
{
    public abstract class ConsoleColorCmdlet : PSCmdlet
    {
        [ParameterAttribute]
        public ConsoleColor BackgroundColor { get; set; }

        [ParameterAttribute]
        public ConsoleColor ForegroundColor { get; set; }

        public ConsoleColorCmdlet()
        {
            this.BackgroundColor = Console.BackgroundColor;
            this.ForegroundColor = Console.ForegroundColor;
        }
    }
}
