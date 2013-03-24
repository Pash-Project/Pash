// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using Irony.Parsing;

namespace Pash.ParserIntrinsics
{
    public abstract class CaseInsensitiveGrammar : Grammar
    {
        public CaseInsensitiveGrammar()
            : base(false) // case insensitive flag
        {
        }
    }

}

