// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace System.Management.Automation.Language
{
    public class ThrowStatementAst : StatementAst
    {
        public ThrowStatementAst(IScriptExtent extent, PipelineBaseAst pipeline)
            : base(extent)
        {
            this.Pipeline = pipeline;
        }

        public bool IsRethrow { get { throw new NotImplementedException(this.ToString()); } }
        public PipelineBaseAst Pipeline { get; private set; }

        public override string ToString()
        {
            return string.Format("throw {0}", this.Pipeline);
        }
    }
}
