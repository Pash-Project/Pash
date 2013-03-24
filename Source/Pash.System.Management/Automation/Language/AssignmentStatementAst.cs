﻿// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace System.Management.Automation.Language
{
    public class AssignmentStatementAst : PipelineBaseAst
    {
        public AssignmentStatementAst(IScriptExtent extent, ExpressionAst left, TokenKind @operator, StatementAst right, IScriptExtent errorPosition)
            : base(extent)
        {
            this.Left = left;
            this.Operator = @operator;
            this.Right = right;
            this.ErrorPosition = errorPosition;
        }

        public IScriptExtent ErrorPosition { get; private set; }
        public ExpressionAst Left { get; private set; }
        public TokenKind Operator { get; private set; }
        public StatementAst Right { get; private set; }

        internal override IEnumerable<Ast> Children
        {
            get
            {
                yield return this.Left;
                yield return this.Right;
                foreach (var item in base.Children) yield return item;
            }
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}", this.Left, Enum.GetName(typeof(TokenKind), this.Operator), this.Right);
        }

    }
}
