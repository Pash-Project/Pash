// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace System.Management.Automation.Language
{
    public class TrapStatementAst : StatementAst
    {
        public TrapStatementAst(IScriptExtent extent, TypeConstraintAst trapType, StatementBlockAst body)
            : base(extent)
        {
            this.TrapType = trapType;
            this.Body = body;
        }

        public StatementBlockAst Body { get; private set; }
        public TypeConstraintAst TrapType { get; private set; }

        internal override IEnumerable<Ast> Children
        {
            get
            {
                yield return this.TrapType; yield return this.Body;
                foreach (var item in base.Children) yield return item;
            }
        }

        public override string ToString()
        {
            return string.Format("trap {0}", this.TrapType);
        }
    }
}
