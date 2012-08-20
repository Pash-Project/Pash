using System;
using Irony.Parsing;

namespace Pash.ParserIntrinsics
{
    public abstract class CaseInsensitiveGrammar : Grammar
    {
        public CaseInsensitiveGrammar()
            : base(true) // case insensitive flag
        {
        }
    }

}

