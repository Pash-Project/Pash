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

        ////            dash   and				dash   band				dash   bnot
        ////            dash   bor				dash   bxor				dash   not
        ////            dash   or				dash   xor
        public readonly BinaryOperatorTerminal _operator_and = new BinaryOperatorTerminal("and", TokenKind.And);
        public readonly BinaryOperatorTerminal _operator_band = new BinaryOperatorTerminal("band", TokenKind.Band);
        public readonly BinaryOperatorTerminal _operator_bnot = new BinaryOperatorTerminal("bnot", TokenKind.Bnot);
        public readonly BinaryOperatorTerminal _operator_bor = new BinaryOperatorTerminal("bor", TokenKind.Bor);
        public readonly BinaryOperatorTerminal _operator_bxor = new BinaryOperatorTerminal("bxor", TokenKind.Bxor);
        public readonly BinaryOperatorTerminal _operator_not = new BinaryOperatorTerminal("not", TokenKind.Not);
        public readonly BinaryOperatorTerminal _operator_or = new BinaryOperatorTerminal("or", TokenKind.Or);
        public readonly BinaryOperatorTerminal _operator_xor = new BinaryOperatorTerminal("xor", TokenKind.Xor);
    }
}
