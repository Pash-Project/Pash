// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    public abstract class ContentCommandBase : CoreCommandWithCredentialsBase, IDisposable
    {
        public void Dispose()
        {
        }

        [ParameterAttribute(ParameterSetName = "LiteralPath", ValueFromPipelineByPropertyName = true)]
        public string[] LiteralPath { get; set; }

        [ParameterAttribute(Position = 0, ParameterSetName = "Path", ValueFromPipelineByPropertyName = true)]
        public string[] Path { get; set; }
    }
}
