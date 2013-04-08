// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.IO;

namespace System.Management.Automation
{
    /// <summary>
    /// Object which holds info on commands which aren't cmdlets 
    /// Examples: ifconfig, lspci
    /// </summary>
    public class ApplicationInfo : CommandInfo
    {
        internal ApplicationInfo(string name, string path, string extension) :
            base(name, CommandTypes.Application)
        {
            Path = path;
            Extension = extension;
        }

        public override string Definition
        {
            get
            {
                return this.Path;
            }
        }

        public string Extension { get; private set; }

        public string Path { get; private set; }
    }
}

