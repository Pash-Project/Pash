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
    public class literal_astnode : _astnode
    {
        public readonly int? IntegerValue;
        public readonly string StringValue;

        public literal_astnode(AstContext astContext, ParseTreeNode parseTreeNode)
            : base(astContext, parseTreeNode)
        {

            ////        literal:
            ////            integer_literal
            ////            real_literal
            ////            string_literal

            if (this.parseTreeNode.ChildNodes.Single().Term == Grammar.integer_literal)
            {
                this.IntegerValue = this.ChildAstNodes.Single().Cast<integer_literal_astnode>().Value;
            }

            else if (this.parseTreeNode.ChildNodes.Single().Term == Grammar.string_literal)
            {
                this.StringValue = this.ChildAstNodes.Single().Cast<string_literal_astnode>().Value;
            }

            else throw new NotImplementedException(this.ToString());
        }
    }
}
