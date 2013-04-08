// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

/// <summary>
/// This class defines the styling for any individual field in a FormatTable. Can not be used directly, only derivied from.
/// </summary>

using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    public abstract class StyleInfo
    {
        public ConsoleColor ForegroundColor { get; set; }
        public ConsoleColor BackgroundColor { get; set; }
    }
}