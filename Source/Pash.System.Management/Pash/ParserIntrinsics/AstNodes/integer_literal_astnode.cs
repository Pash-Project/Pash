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
    public class integer_literal_astnode : _astnode
    {
        public readonly int Value;

        public integer_literal_astnode(AstContext astContext, ParseTreeNode parseTreeNode)
            : base(astContext, parseTreeNode)
        {

            ////        integer_literal:
            ////            decimal_integer_literal
            ////            hexadecimal_integer_literal

            if (this.parseTreeNode.ChildNodes.Single().Term == PowerShellGrammar.Terminals.decimal_integer_literal)
            {
                this.Value = this.ChildAstNodes.Single().As<decimal_integer_literal_astnode>().Value;
            }

            else if (this.parseTreeNode.ChildNodes.Single().Term == PowerShellGrammar.Terminals.hexadecimal_integer_literal)
            {
                this.Value = this.ChildAstNodes.Single().As<hexadecimal_integer_literal_astnode>().Value;
            }

            else throw new InvalidOperationException(this.ToString());
        }
    }
}
