// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace System.Management.Automation
{
    /// <summary>
    /// Provides information on scripts executable by Pash but are external to it.
    /// </summary>
    public class ExternalScriptInfo : CommandInfo
    {
        public override string Definition
        {
            get
            {
                return this.Path;
            }
        }

        public string Path { get; private set; }
    }
}

