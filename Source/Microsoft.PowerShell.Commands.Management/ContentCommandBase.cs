// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    public abstract class ContentCommandBase : CoreCommandWithFilteredPathsBase, IDisposable
    {
        public void Dispose()
        {
        }

        [Parameter]
        public override SwitchParameter Force { get; set; }
    }
}
