using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Ast;
using Irony.Parsing;
using System.Diagnostics;
using System.Reflection;
using Pash.Implementation;
using System.Management.Automation;

namespace Pash.ParserIntrinsics.AstNodes
{
    public class additive_expression_astnode : _astnode
    {
        readonly _astnode leftOperandAstNode;
        readonly _astnode rightOperandAstNode;

        public additive_expression_astnode(AstContext astContext, ParseTreeNode parseTreeNode)
            : base(astContext, parseTreeNode)
        {
            if (parseTreeNode.ChildNodes.Count == 3)
            {
                leftOperandAstNode = (_astnode)parseTreeNode.ChildNodes[0].AstNode;
                KeywordTerminal keywordTerminal = (KeywordTerminal)parseTreeNode.ChildNodes[1].Term;
                // if you hit this exception, it's probably subtraction ("dash")
                if (keywordTerminal.Text != "+") throw new NotImplementedException();
                rightOperandAstNode = (_astnode)parseTreeNode.ChildNodes[2].AstNode;
            }
            else if (parseTreeNode.ChildNodes.Count == 1)
            {
                var childNode = parseTreeNode.ChildNodes.Single();
                // TODO: find a way to get rid of this hard-coded string.
                Debug.Assert(childNode.Term.Name == "multiplicative_expression");
            }
        }

        // TODO: sum the values in both pipelines
        // TODO: if there are more than one value in the left - just copy left results and then the right results to the resulting pipe
        // TODO: if there is only one value on the left - convert the value on the right to the type of the left and then Sum
        internal override object Execute(ExecutionContext context, ICommandRuntime commandRuntime)
        {
            // if only 1 child node, then the default (base) implementation will forward to that child
            if (parseTreeNode.ChildNodes.Count == 1)
            {
                return base.Execute(context, commandRuntime);
            }

            var leftValue = leftOperandAstNode.Execute(context, commandRuntime);
            var rightValue = rightOperandAstNode.Execute(context, commandRuntime);

            // TODO: need to generalize this via MethodInfo (somewhere in the compiler libraries via LanguageBasics)
            // usualy operators defined as: "public static int operator +", but in the MSIL are translated to op_Add
            MethodInfo mi = null;

            if (leftValue != null)
            {
                Type leftType = leftValue.GetType();
                mi = leftType.GetMethod("op_Addition", new Type[] { leftType, leftType });
            }
            else if (rightValue != null)
            {
                Type rightType = rightValue.GetType();
                mi = rightType.GetMethod("op_Addition", new Type[] { rightType, rightType });
            }
            else
                // If both of the values equal Null - return Null
                return null;

            if (mi == null)
            {
                if (leftValue != null)
                {
                    if (leftValue is string)
                    {
                        return string.Concat(leftValue, rightValue.ToString());
                    }
                    if (leftValue is int)
                    {
                        return (int)leftValue + Convert.ToInt32(rightValue);
                    }
                }
                if (rightValue != null)
                {
                    if (rightValue is string)
                    {
                        return string.Concat(leftValue.ToString(), rightValue);
                    }
                    if (rightValue is int)
                    {
                        return (int)rightValue + Convert.ToInt32(leftValue);
                    }
                }
                throw new InvalidOperationException("The operator \"+\" is not defined for type " + leftValue.GetType().ToString());
            }

            return null;
        }
    }
}
