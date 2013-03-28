// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
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

        public CommandTypes CommandType { get; private set; }

        public abstract string Definition { get; }

        public string Name { get; private set; }

        public override string ToString()
        {
            // TODO: implement CommandInfo.ToString
            return Name;
        }
    }
}
