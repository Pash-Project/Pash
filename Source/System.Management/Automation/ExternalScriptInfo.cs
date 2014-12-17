// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using Pash.Implementation;
using System.Management.Automation.Language;
using System.Collections.Generic;
using Pash.ParserIntrinsics;

namespace System.Management.Automation
{
    /// <summary>
    /// Provides information on scripts executable by Pash but are external to it.
    /// </summary>
    public class ExternalScriptInfo : CommandInfo, IScriptBlockInfo
    {
        private ScriptBlock _scriptBlock;
        private string _scriptContents;

        public string Path { get; private set; }
        public string ScriptContents
        {
            get
            {
                if (_scriptContents == null)
                {
                    _scriptContents = File.ReadAllText(Path);
                }
                return _scriptContents;
            }
        }

        public override string Definition
        {
            get
            {
                return this.Path;
            }
        }

        internal ExternalScriptInfo(string path, ScopeUsages scopeUsage = ScopeUsages.NewScope)
            : base(path, CommandTypes.ExternalScript)
        {
            Path = path;
            ScopeUsage = scopeUsage;
            _scriptContents = null;
            _scriptBlock = null;
        }

        #region IScriptBlockInfo Members

        public ScriptBlock ScriptBlock
        {
            get
            {
                if (_scriptBlock == null)
                {
                    _scriptBlock = Parser.ParseInput(ScriptContents).GetScriptBlock();
                }
                return _scriptBlock;
            }
        }

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

