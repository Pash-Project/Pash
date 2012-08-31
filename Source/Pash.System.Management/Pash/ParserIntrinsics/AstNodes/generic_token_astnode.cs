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
    public class generic_token_astnode : _astnode
    {
        public generic_token_astnode(AstContext astContext, ParseTreeNode parseTreeNode)
            : base(astContext, parseTreeNode)
        {
        }

        internal object Execute(ExecutionContext context, ICommandRuntime commandRuntime)
        {
            ////        generic_token:
            ////            generic_token_parts
            ////
            ////        generic_token_parts:
            ////            generic_token_part
            ////            generic_token_parts   generic_token_part
            ////
            ////        generic_token_part:
            ////            expandable_string_literal
            ////            verbatim_here_string_literal
            ////            variable
            ////            generic_token_char
            ////
            ////        generic_token_char:
            ////            Any Unicode character except
            ////                    {		}		(		)		;		,		|		&		$
            ////                    `   (The backtick character U+0060)
            ////                    double_quote_character
            ////                    single_quote_character
            ////                    whitespace
            ////                    new_line_character
            ////            escaped_character

            return Text;
        }
    }
}
