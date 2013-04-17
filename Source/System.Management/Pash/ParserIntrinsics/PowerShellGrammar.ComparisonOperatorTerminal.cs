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
        public class ComparisonOperatorTerminal : BinaryOperatorTerminal
        {
            internal ComparisonOperatorTerminal(string @operator, TokenKind tokenKind)
                : base(@operator, tokenKind)
            {
            }
        }

        ////        comparison_operator:  one of
        ////            dash   as					dash   ccontains				dash   ceq
        ////            dash   cge					dash   cgt						dash   cle
        ////            dash   clike				dash   clt						dash   cmatch
        ////            dash   cne					dash   cnotcontains				dash   cnotlike
        ////            dash   cnotmatch			dash   contains					dash   creplace
        ////            dash   csplit				dash   eq						dash   ge
        ////            dash   gt					dash   icontains				dash   ieq
        ////            dash   ige					dash   igt						dash   ile
        ////            dash   ilike				dash   ilt						dash   imatch
        ////            dash   ine					dash   inotcontains				dash   inotlike
        ////            dash   inotmatch			dash   ireplace					dash   is
        ////            dash   isnot				dash   isplit					dash   join
        ////            dash   le					dash   like						dash   lt
        ////            dash   match				dash   ne						dash   notcontains
        ////            dash   notlike				dash   notmatch					dash   replace
        ////            dash   split

        // `comparison_operator` is a `NonTerminal` based on these terminals:

        public readonly ComparisonOperatorTerminal _comparison_operator_as = new ComparisonOperatorTerminal("as", TokenKind.As);
        public readonly ComparisonOperatorTerminal _comparison_operator_ccontains = new ComparisonOperatorTerminal("ccontains", TokenKind.Ccontains);
        public readonly ComparisonOperatorTerminal _comparison_operator_ceq = new ComparisonOperatorTerminal("ceq", TokenKind.Ceq);
        public readonly ComparisonOperatorTerminal _comparison_operator_cge = new ComparisonOperatorTerminal("cge", TokenKind.Cge);
        public readonly ComparisonOperatorTerminal _comparison_operator_cgt = new ComparisonOperatorTerminal("cgt", TokenKind.Cgt);
        public readonly ComparisonOperatorTerminal _comparison_operator_cle = new ComparisonOperatorTerminal("cle", TokenKind.Cle);
        public readonly ComparisonOperatorTerminal _comparison_operator_clike = new ComparisonOperatorTerminal("clike", TokenKind.Clike);
        public readonly ComparisonOperatorTerminal _comparison_operator_clt = new ComparisonOperatorTerminal("clt", TokenKind.Clt);
        public readonly ComparisonOperatorTerminal _comparison_operator_cmatch = new ComparisonOperatorTerminal("cmatch", TokenKind.Cmatch);
        public readonly ComparisonOperatorTerminal _comparison_operator_cne = new ComparisonOperatorTerminal("cne", TokenKind.Cne);
        public readonly ComparisonOperatorTerminal _comparison_operator_cnotcontains = new ComparisonOperatorTerminal("cnotcontains", TokenKind.Cnotcontains);
        public readonly ComparisonOperatorTerminal _comparison_operator_cnotlike = new ComparisonOperatorTerminal("cnotlike", TokenKind.Cnotlike);
        public readonly ComparisonOperatorTerminal _comparison_operator_cnotmatch = new ComparisonOperatorTerminal("cnotmatch", TokenKind.Cnotmatch);
        public readonly ComparisonOperatorTerminal _comparison_operator_contains = new ComparisonOperatorTerminal("contains", TokenKind.Icontains);
        public readonly ComparisonOperatorTerminal _comparison_operator_creplace = new ComparisonOperatorTerminal("creplace", TokenKind.Creplace);
        public readonly ComparisonOperatorTerminal _comparison_operator_csplit = new ComparisonOperatorTerminal("csplit", TokenKind.Csplit);
        public readonly ComparisonOperatorTerminal _comparison_operator_eq = new ComparisonOperatorTerminal("eq", TokenKind.Ieq);
        public readonly ComparisonOperatorTerminal _comparison_operator_ge = new ComparisonOperatorTerminal("ge", TokenKind.Ige);
        public readonly ComparisonOperatorTerminal _comparison_operator_gt = new ComparisonOperatorTerminal("gt", TokenKind.Igt);
        public readonly ComparisonOperatorTerminal _comparison_operator_icontains = new ComparisonOperatorTerminal("icontains", TokenKind.Icontains);
        public readonly ComparisonOperatorTerminal _comparison_operator_ieq = new ComparisonOperatorTerminal("ieq", TokenKind.Ieq);
        public readonly ComparisonOperatorTerminal _comparison_operator_ige = new ComparisonOperatorTerminal("ige", TokenKind.Ige);
        public readonly ComparisonOperatorTerminal _comparison_operator_igt = new ComparisonOperatorTerminal("igt", TokenKind.Igt);
        public readonly ComparisonOperatorTerminal _comparison_operator_ile = new ComparisonOperatorTerminal("ile", TokenKind.Ile);
        public readonly ComparisonOperatorTerminal _comparison_operator_ilike = new ComparisonOperatorTerminal("ilike", TokenKind.Ilike);
        public readonly ComparisonOperatorTerminal _comparison_operator_ilt = new ComparisonOperatorTerminal("ilt", TokenKind.Ilt);
        public readonly ComparisonOperatorTerminal _comparison_operator_imatch = new ComparisonOperatorTerminal("imatch", TokenKind.Imatch);
        public readonly ComparisonOperatorTerminal _comparison_operator_ine = new ComparisonOperatorTerminal("ine", TokenKind.Ine);
        public readonly ComparisonOperatorTerminal _comparison_operator_inotcontains = new ComparisonOperatorTerminal("inotcontains", TokenKind.Inotcontains);
        public readonly ComparisonOperatorTerminal _comparison_operator_inotlike = new ComparisonOperatorTerminal("inotlike", TokenKind.Inotlike);
        public readonly ComparisonOperatorTerminal _comparison_operator_inotmatch = new ComparisonOperatorTerminal("inotmatch", TokenKind.Inotmatch);
        public readonly ComparisonOperatorTerminal _comparison_operator_ireplace = new ComparisonOperatorTerminal("ireplace", TokenKind.Ireplace);
        public readonly ComparisonOperatorTerminal _comparison_operator_is = new ComparisonOperatorTerminal("is", TokenKind.Is);
        public readonly ComparisonOperatorTerminal _comparison_operator_isnot = new ComparisonOperatorTerminal("isnot", TokenKind.IsNot);
        public readonly ComparisonOperatorTerminal _comparison_operator_isplit = new ComparisonOperatorTerminal("isplit", TokenKind.Isplit);
        public readonly ComparisonOperatorTerminal _comparison_operator_join = new ComparisonOperatorTerminal("join", TokenKind.Join);
        public readonly ComparisonOperatorTerminal _comparison_operator_le = new ComparisonOperatorTerminal("le", TokenKind.Ile);
        public readonly ComparisonOperatorTerminal _comparison_operator_like = new ComparisonOperatorTerminal("like", TokenKind.Ilike);
        public readonly ComparisonOperatorTerminal _comparison_operator_lt = new ComparisonOperatorTerminal("lt", TokenKind.Ilt);
        public readonly ComparisonOperatorTerminal _comparison_operator_match = new ComparisonOperatorTerminal("match", TokenKind.Imatch);
        public readonly ComparisonOperatorTerminal _comparison_operator_ne = new ComparisonOperatorTerminal("ne", TokenKind.Ine);
        public readonly ComparisonOperatorTerminal _comparison_operator_notcontains = new ComparisonOperatorTerminal("notcontains", TokenKind.Inotcontains);
        public readonly ComparisonOperatorTerminal _comparison_operator_notlike = new ComparisonOperatorTerminal("notlike", TokenKind.Inotlike);
        public readonly ComparisonOperatorTerminal _comparison_operator_notmatch = new ComparisonOperatorTerminal("notmatch", TokenKind.Inotmatch);
        public readonly ComparisonOperatorTerminal _comparison_operator_replace = new ComparisonOperatorTerminal("replace", TokenKind.Ireplace);
        public readonly ComparisonOperatorTerminal _comparison_operator_split = new ComparisonOperatorTerminal("split", TokenKind.Isplit);
    }
}
