// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;

namespace System.Management.Automation.Runspaces
{
    public sealed class FormatConfigurationEntry : RunspaceConfigurationEntry
    {
        private string filename;

        public string FileName
        {
            get
            {
                return this.filename;
            }
        }

        public FormatConfigurationEntry(string name, string fileName)
            : base(name)
        {
            this.filename = fileName;
        }

        public FormatConfigurationEntry(string fileName)
            : base(fileName)
        {
            this.filename = fileName;
        }
    }
}
