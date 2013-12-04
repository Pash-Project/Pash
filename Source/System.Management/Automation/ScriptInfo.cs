// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation.Language;
using Pash.Implementation;


namespace System.Management.Automation
{

    public class ScriptInfo : CommandInfo, IScriptBlockInfo
    {
        public override string Definition { get { return ScriptBlock.ToString(); } }

        internal ScriptInfo(string name, ScriptBlock script, ScopeUsages scopeUsage = ScopeUsages.NewScope)
            : base(name, CommandTypes.Script)
        {
            ScriptBlock = script;
            ScopeUsage = scopeUsage;
        }

        #region IScriptBlockInfo Members

        public ScriptBlock ScriptBlock { get; private set; }
        public ScopeUsages ScopeUsage{ get; set; }

        public ReadOnlyCollection<ParameterAst> GetParameters()
        {
            var scriptBlockAst = (ScriptBlockAst)ScriptBlock.Ast;
            if (scriptBlockAst.ParamBlock != null)
                return scriptBlockAst.ParamBlock.Parameters;

            return new ReadOnlyCollection<ParameterAst>(new List<ParameterAst>());
        }

        #endregion

        public override string ToString() { return Definition; }
    }
}
