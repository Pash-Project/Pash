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

