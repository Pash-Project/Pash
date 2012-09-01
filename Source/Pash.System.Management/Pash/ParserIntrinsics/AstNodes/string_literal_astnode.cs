using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using Irony.Ast;
using Irony.Parsing;
using Pash.Implementation;

namespace Pash.ParserIntrinsics.AstNodes
{
    public class string_literal_astnode : _astnode
    {
        public readonly string Value;

        public string_literal_astnode(AstContext astContext, ParseTreeNode parseTreeNode)
            : base(astContext, parseTreeNode)
        {
            ////        string_literal:
            ////            expandable_string_literal
            ////            expandable_here_string_literal
            ////            verbatim_string_literal
            ////            verbatim_here_string_literal

            if (this.parseTreeNode.ChildNodes.Single().Term == PowerShellGrammar.Terminals.verbatim_string_literal)
            {
                this.Value = this.ChildAstNodes.Single().As<verbatim_string_literal_astnode>().Value;
            }

            else if (this.parseTreeNode.ChildNodes.Single().Term == PowerShellGrammar.Terminals.expandable_string_literal)
            {
                this.Value = this.ChildAstNodes.Single().As<expandable_string_literal_astnode>().Value;
            }

            else throw new NotImplementedException(this.ToString());
        }
    }
}
