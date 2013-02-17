// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;
using System.Management.Automation.Runspaces;

namespace System.Management.Automation
{
    public sealed class PSCommand
    {
        private CommandCollection commands;

        public CommandCollection Commands
        {
            get
            {
                return this.commands;
            }
        }

        public PSCommand()
        {
            commands = new CommandCollection();
        }

        public PSCommand AddCommand(string command)
        {
            throw new NotImplementedException();
        }

        public PSCommand AddCommand(string cmdlet, bool useLocalScope)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// TODO: Adds a scrip to the command queue.
        /// </summary>
        /// <param name="script">Script to be run.</param>
        public PSCommand AddScript(string script)
        {
            return this;
        }

        public PSCommand AddScript(string script, bool useLocalScope)
        {
            throw new NotImplementedException();
        }

        public PSCommand AddCommand(Command command)
        {
            throw new NotImplementedException();
        }

        public PSCommand AddParameter(string parameterName, object value)
        {
            throw new NotImplementedException();
        }

        public PSCommand AddParameter(string parameterName)
        {
            throw new NotImplementedException();
        }

        public PSCommand AddArgument(object value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Clears all commands from the queue.
        /// </summary>
        public void Clear()
        {
            commands.Clear();
        }

        public PSCommand Clone()
        {
            throw new NotImplementedException();
        }
    }
}
