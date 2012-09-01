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
    public class expandable_string_literal_astnode : _astnode
    {
        public readonly string Value;

        public expandable_string_literal_astnode(AstContext astContext, ParseTreeNode parseTreeNode)
            : base(astContext, parseTreeNode)
        {
            ////        expandable_string_literal:
            ////            double_quote_character   expandable_string_characters_opt   dollars_opt   double_quote_character

            var matches = Regex.Match(Text, PowerShellGrammar.Terminals.expandable_string_literal.Pattern);
            this.Value = matches.Groups[PowerShellGrammar.Terminals.expandable_string_characters.Name].Value;
        }
    }
}
