// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Collections.Generic;
using System.Collections;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    public class FormatElement
    {
        public String[] Values { get; set; }
        public StyleInfo Style { get; set; }
        public bool isHeader { get; set; }
        public int ColumnSize { get; set; }
    }
}