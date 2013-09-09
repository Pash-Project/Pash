// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace System.Management.Automation.Language
{
    // "The ast representing a begin, process, end, or dynamic parameter block in a scriptblock. This ast is used even when the block is unnamed, in which case the block is either an end block (for functions) or process block (for filters)."
    public class ScriptBlockAst : Ast
    {
        public ScriptBlockAst(IScriptExtent extent, ParamBlockAst paramBlock, StatementBlockAst statements, bool isFilter)
            : base(extent)
        {
            this.ParamBlock = paramBlock;
            this.EndBlock = new NamedBlockAst(extent, TokenKind.End, statements, true);
            if (isFilter) throw new NotImplementedException(this.ToString());
        }

        public ScriptBlockAst(IScriptExtent extent, ParamBlockAst paramBlock, NamedBlockAst beginBlock, NamedBlockAst processBlock, NamedBlockAst endBlock, NamedBlockAst dynamicParamBlock)
            : base(extent)
        {
            this.BeginBlock = beginBlock;
            this.DynamicParamBlock = dynamicParamBlock;
            this.EndBlock = endBlock;
            this.ParamBlock = paramBlock;
            this.ProcessBlock = processBlock;
        }

        public NamedBlockAst BeginBlock { get; private set; }
        public NamedBlockAst DynamicParamBlock { get; private set; }
        public NamedBlockAst EndBlock { get; private set; }
        public ParamBlockAst ParamBlock { get; private set; }
        public NamedBlockAst ProcessBlock { get; private set; }
        //public ScriptRequirements ScriptRequirements { get; internal set; }

        //public CommentHelpInfo GetHelpContent();
        public ScriptBlock GetScriptBlock()
        {
            return new ScriptBlock(this);
        }

        internal override IEnumerable<Ast> Children
        {
            get
            {
                if (this.BeginBlock != null) yield return this.BeginBlock;
                if (this.DynamicParamBlock != null) yield return this.DynamicParamBlock;
                if (this.EndBlock != null) yield return this.EndBlock;
                if (this.ParamBlock != null) yield return this.ParamBlock;
                if (this.ProcessBlock != null) yield return this.ProcessBlock;
                foreach (var item in base.Children) yield return item;
            }
        }

        public override string ToString()
        {
            return this.EndBlock.ToString();
        }
    }
}
