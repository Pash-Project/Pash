using System;
using System.Collections.Generic;
using System.Text;

namespace System.Management.Automation
{
    public abstract class CommandInfo
    {
        internal CommandInfo(string name, CommandTypes type)
        {
            CommandType = type;
            Name = name;
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
