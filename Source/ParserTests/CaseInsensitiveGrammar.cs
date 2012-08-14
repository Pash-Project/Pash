using System;
using Irony.Parsing;

namespace ParserTests
{
    abstract class CaseInsensitiveGrammar : Grammar
    {
        public CaseInsensitiveGrammar()
            : base(true) // case insensitive flag
        {
        }
    }

}

