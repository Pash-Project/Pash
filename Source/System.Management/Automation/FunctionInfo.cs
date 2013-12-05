// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;
using Pash.Implementation;
using System.Collections.ObjectModel;
using System.Management.Automation.Language;

namespace System.Management.Automation
{
    public class FunctionInfo : CommandInfo, IScopedItem, IScriptBlockInfo
    {
        public override string Definition { get { return ScriptBlock.ToString(); } }
        public ScopedItemOptions Options { get; set; }
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
            ScopeUsage = ScopeUsages.NewScope;
        }

        internal FunctionInfo(string name, ScriptBlock function, ScopedItemOptions options)
            : base(name, CommandTypes.Function)
        {
            ScriptBlock = function;
            Options = options;
            ScopeUsage = ScopeUsages.NewScope;
        }

        #region IScriptBlockInfo Members

        public ScriptBlock ScriptBlock { get; private set; }
        public ScopeUsages ScopeUsage { get; private set; }

        public ReadOnlyCollection<ParameterAst> GetParameters()
        {
            var scriptBlockAst = (ScriptBlockAst)ScriptBlock.Ast;
            if (scriptBlockAst.ParamBlock != null)
                return scriptBlockAst.ParamBlock.Parameters;

            return new ReadOnlyCollection<ParameterAst>(new List<ParameterAst>());
        }

        #endregion

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
