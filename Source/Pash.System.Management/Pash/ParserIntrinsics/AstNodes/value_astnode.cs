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
    public class value_astnode : _astnode
    {
        public readonly parenthesized_expression_astnode ParenthesizedExpression;
        public readonly literal_astnode Literal;
        public readonly variable_astnode Variable;

        public value_astnode(AstContext astContext, ParseTreeNode parseTreeNode)
            : base(astContext, parseTreeNode)
        {
            ////        value:
            ////            parenthesized_expression
            ////            sub_expression
            ////            array_expression
            ////            script_block_expression
            ////            hash_literal_expression
            ////            literal
            ////            type_literal
            ////            variable

            if (this.parseTreeNode.ChildNodes.Single().Term == Grammar.parenthesized_expression)
            {
                this.ParenthesizedExpression = this.ChildAstNodes.Single().As<parenthesized_expression_astnode>();
            }

            else if (this.parseTreeNode.ChildNodes.Single().Term == Grammar.literal)
            {
                this.Literal = this.ChildAstNodes.Single().As<literal_astnode>();
            }

            else if (this.parseTreeNode.ChildNodes.Single().Term == PowerShellGrammar.Terminals.variable)
            {
                this.Variable = this.ChildAstNodes.Single().As<variable_astnode>();
            }

            else throw new NotImplementedException(this.ToString());
        }

        internal object Execute(ExecutionContext context, ICommandRuntime commandRuntime)
        {
            if (this.ParenthesizedExpression != null) return this.ParenthesizedExpression.Execute(context, commandRuntime);

            if (this.Literal != null) {
                if (this.Literal.IntegerValue.HasValue) return this.Literal.IntegerValue.Value;

                if (this.Literal.StringValue != null) return this.Literal.StringValue;

                throw new NotImplementedException(this.ToString());
            }

            if (this.Variable != null)
            {
                return this.Variable.Evaluate(context, commandRuntime);
            }

            throw new NotImplementedException(this.ToString());
        }
    }
}
