// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace System.Management.Automation
{
    /// <summary>
    /// Contains information about a Command.
    /// </summary>
    public abstract class CommandInfo
    {
        internal CommandInfo(string name, CommandTypes type)
        {
            CommandType = type;
            Name = name;
        }

        internal CommandInfo()
        {
        }

        virtual internal void Validate()
        {
            // does nothing by default
        }

        public CommandTypes CommandType { get; private set; }

        public abstract string Definition { get; }

        public string Name { get; private set; }

        public PSModuleInfo Module { get; set; }

        public string  ModuleName
        {
            get
            {
                return Module == null ? "" : this.Module.Name;
            }
        }

        public abstract ReadOnlyCollection<PSTypeName> OutputType { get; }

        public override string ToString()
        {
            // TODO: implement CommandInfo.ToString
            return Name;
        }
    }
}
