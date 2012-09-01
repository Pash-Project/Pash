using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Ast;
using Irony.Parsing;
using Pash.Implementation;
using System.Management.Automation;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace Pash.ParserIntrinsics.AstNodes
{
    public class verbatim_string_literal_astnode : _astnode
    {
        public readonly string Value;

        public verbatim_string_literal_astnode(AstContext astContext, ParseTreeNode parseTreeNode)
            : base(astContext, parseTreeNode)
        {
            ////        verbatim_string_literal:
            ////            single_quote_character   verbatim_string_characters_opt   single_quote_char [sic]

            var matches = Regex.Match(Text, PowerShellGrammar.Terminals.verbatim_string_literal.Pattern);
            this.Value = matches.Groups[PowerShellGrammar.Terminals.verbatim_string_characters.Name].Value;
        }
    }
}
