// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;
using Pash.Implementation;

namespace System.Management.Automation
{
    /// <summary>
    /// Contains information about a Pash Alias.
    /// </summary>
    public class AliasInfo : CommandInfo, IScopedItem
    {
        private string _definition;
        public override string Definition { get { return _definition; } }
        public string Description { get; set; }
        public ScopedItemOptions Options { get; set; }

        // TODO: what is the difference?
        public CommandInfo ReferencedCommand { get; private set; }
        public CommandInfo ResolvedCommand { get; private set; }

        internal AliasInfo(string name, string definition, CommandManager cmdManager)
            : this(name, definition, cmdManager, ScopedItemOptions.None)
        {
        }

        internal AliasInfo(string name, string definition, CommandManager cmdManager, ScopedItemOptions options)
            :  this(name, definition, "", cmdManager, options)
        {
        }

        internal AliasInfo(string name, string definition, string description, CommandManager cmdManager, ScopedItemOptions options)
            : base(name, CommandTypes.Alias)
        {

            Options = options;
            Description = description;
            SetDefinition(definition, cmdManager);
        }

        // internals
        //internal string UnresolvedCommandName { get; }
        internal void SetDefinition(string definition, CommandManager cmdManager)
        {
            _definition = definition;

            //only set referenced command if found
            //aliases only cause errors when they are used, not at instanciation
            CommandInfo refInfo = null;
            try
            {
                refInfo = cmdManager.FindCommand(definition);
            }
            catch (CommandNotFoundException)
            {
            }

            ReferencedCommand = refInfo;
            ResolvedCommand = ReferencedCommand;
        }
        //internal void SetOptions(ScopedItemOptions newOptions, bool force);


        #region IScopedItem Members

        public string ItemName
        {
            get { return Name; }
        }

        public ScopedItemOptions ItemOptions
        {
            get { return Options; }
            set { Options = value; }
        }

        #endregion
    }

}
