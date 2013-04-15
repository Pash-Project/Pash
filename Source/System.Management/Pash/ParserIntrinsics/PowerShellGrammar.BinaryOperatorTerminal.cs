using Irony.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using System.Text;

namespace Pash.ParserIntrinsics
{
    partial class PowerShellGrammar
    {
        public class BinaryOperatorTerminal : RegexBasedTerminal
        {
            public readonly string Operator;
            public readonly TokenKind TokenKind;

            public BinaryOperatorTerminal(string @operator, TokenKind tokenKind)
                : base("-" + @operator, "(?<_operator_" + @operator + ">" + dash_pattern + @operator + ")")
            {
                this.Operator = @operator;
                this.TokenKind = tokenKind;
                Priority = TerminalPriority.ReservedWords;
            }
        }
    }
}
