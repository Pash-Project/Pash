using System;
using System.Collections.Generic;
using System.Text;
using Pash.Implementation;

namespace System.Management.Automation
{
    public class AliasInfo : CommandInfo
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
            : base(name, CommandTypes.Alias)
        {
            
            Options = options;

            SetDefinition(definition, cmdManager);
        }

        // internals
        //internal string UnresolvedCommandName { get; }
        internal void SetDefinition(string definition, CommandManager cmdManager)
        {
            _definition = definition;

            ReferencedCommand = cmdManager.FindCommand(definition);
            ResolvedCommand = ReferencedCommand;
        }
        //internal void SetOptions(ScopedItemOptions newOptions, bool force);
    }


}
