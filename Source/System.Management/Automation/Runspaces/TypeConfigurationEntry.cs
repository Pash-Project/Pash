// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;

namespace System.Management.Automation.Runspaces
{
    public sealed class TypeConfigurationEntry : RunspaceConfigurationEntry
    {
        private string filename;

        public string FileName
        {
            get
            {
                return this.filename;
            }
        }

        public TypeConfigurationEntry(string name, string fileName)
            : base(name)
        {
            this.filename = fileName;
        }

        public TypeConfigurationEntry(string fileName)
            : base(fileName)
        {
            this.filename = fileName;
        }
    }
}