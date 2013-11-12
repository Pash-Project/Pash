// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;
using Pash.Implementation;

namespace System.Management.Automation
{
    public class FunctionInfo : CommandInfo, IScopedItem
    {
        public override string Definition { get { return Name; } }
        public ScopedItemOptions Options { get; set; }
        public ScriptBlock ScriptBlock { get; private set; }
        public string Noun { get; private set; }
        public string Verb { get; private set; }
        public string Description { get; set; }

        internal FunctionInfo(string name, ScriptBlock function)
            : this(name, function, ScopedItemOptions.None) { }

        internal FunctionInfo(string verb, string noun, ScriptBlock function, ScopedItemOptions options)
            : base(verb + "-" + noun, CommandTypes.Function)
        {
            ScriptBlock = function;
            Options = options;
            Verb = verb;
            Noun = noun;
        }

        internal FunctionInfo(string name, ScriptBlock function, ScopedItemOptions options)
            : base(name, CommandTypes.Function)
        {
            ScriptBlock = function;
            Options = options;
        }


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
        // internals
        //internal void SetScriptBlock(ScriptBlock function, bool force);
    }
}
