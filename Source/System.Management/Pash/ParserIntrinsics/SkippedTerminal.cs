using Irony.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pash.ParserIntrinsics
{
    class SkippedTerminal : Terminal
    {
        public SkippedTerminal(Terminal terminalToSkip)
            : base("skipped " + terminalToSkip.Name, TokenCategory.Outline, TermFlags.IsNonGrammar)
        {
        }
    }
}
