// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System.Collections.Generic;
using System.Management.Automation;

namespace System.Management.Automation.Language
{
    public abstract class RedirectionAst : Ast
    {
        protected RedirectionAst(IScriptExtent extent, RedirectionStream from)
            : base(extent)
        {
            this.FromStream = from;
        }

        public RedirectionStream FromStream { get; private set; }

        public override string ToString()
        {
            return string.Format("< {0}", Enum.GetName(typeof(RedirectionStream), this.FromStream));
        }
    }
}
