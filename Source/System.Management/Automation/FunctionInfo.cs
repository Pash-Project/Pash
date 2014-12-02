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
        private ReadOnlyCollection<ParameterAst> _explicitParameters;

        public override string Definition { get { return ScriptBlock.ToString(); } }
        public ScopedItemOptions Options { get; set; }
        public string Noun { get; private set; }
        public string Verb { get; private set; }
        public string Description { get; set; }

        internal FunctionInfo(string name, ScriptBlock function, IEnumerable<ParameterAst> explicitParams)
        : this(name, function, explicitParams, ScopedItemOptions.None) { }

        internal FunctionInfo(string verb, string noun, ScriptBlock function, IEnumerable<ParameterAst> explicitParams,
                              ScopedItemOptions options)
            : this(verb + "-" + noun, function, explicitParams, options)
        {
            Verb = verb;
            Noun = noun;
        }
        internal FunctionInfo(string name, ScriptBlock function, IEnumerable<ParameterAst> explicitParams,
                              ScopedItemOptions options)
            : base(name, CommandTypes.Function)
        {
            ScriptBlock = function;
            Options = options;
            ScopeUsage = ScopeUsages.NewScope;
            _explicitParameters = explicitParams == null ? new ReadOnlyCollection<ParameterAst>(new ParameterAst[0])
                                                         : explicitParams.ToReadOnlyCollection();
        }

        #region IScriptBlockInfo Members

        public ScriptBlock ScriptBlock { get; private set; }
        public ScopeUsages ScopeUsage { get; private set; }

        public ReadOnlyCollection<ParameterAst> GetParameters()
        {
            if (_explicitParameters.Count > 0)
            {
                return _explicitParameters;
            }

            var scriptBlockAst = ScriptBlock.Ast as ScriptBlockAst;
            if (scriptBlockAst.ParamBlock != null)
            {
                return scriptBlockAst.ParamBlock.Parameters;
            }

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
