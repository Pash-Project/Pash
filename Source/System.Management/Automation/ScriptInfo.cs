// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation.Language;

namespace System.Management.Automation
{
    public class ScriptInfo : CommandInfo
    {
        public override string Definition { get { return ScriptBlock.ToString(); } }
        public ScriptBlock ScriptBlock { get; private set; }

        public override string ToString() { return Definition; }

        // internals
        internal ScriptInfo(string name, ScriptBlock script)
            : base(name, CommandTypes.Script)
        {
            ScriptBlock = script;
        }

        internal ReadOnlyCollection<ParameterAst> GetParameters()
        {
            var scriptBlockAst = (ScriptBlockAst)ScriptBlock.Ast;
            if (scriptBlockAst.ParamBlock != null)
                return scriptBlockAst.ParamBlock.Parameters;

            return new ReadOnlyCollection<ParameterAst>(new List<ParameterAst>());
        }
    }
}
