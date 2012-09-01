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
    public class format_expression_astnode : _astnode
    {
        public readonly range_expression_astnode RangeExpression;

        public format_expression_astnode(AstContext astContext, ParseTreeNode parseTreeNode)
            : base(astContext, parseTreeNode)
        {
            ////        format_expression:
            ////            range_expression
            ////            format_expression   format_operator    new_lines_opt   range_expression

            if (this.ChildAstNodes.Count == 1)
            {
                this.RangeExpression = this.ChildAstNodes.Single().Cast<range_expression_astnode>();
            }

            else throw new NotImplementedException(this.ToString());
        }

        internal object Execute(ExecutionContext context, ICommandRuntime commandRuntime)
        {
            return this.RangeExpression.Execute(context, commandRuntime);
        }
    }
}
