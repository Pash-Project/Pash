using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;
using GoldParser;
using Pash.Implementation;

namespace Pash.ParserIntrinsics
{
    public class AdditionExpressionNode : NonTerminalNode
    {
        private ASTNode leftExpression;
        private ASTNode rightExpression;
        private string action;

        public AdditionExpressionNode(Parser parser)
            : base(parser)
        {
            leftExpression = Node(parser, 0);
            action = Token(parser, 1).ToString();
            rightExpression = Node(parser, 2);
        }

        internal override object GetValue(ExecutionContext context)
        {
            object leftVal = leftExpression.GetValue(context);
            object rightVal = rightExpression.GetValue(context);

            // TODO: need to generalize this via MethodInfo (somewhere in the compiler libraries via LanguageBasics)
            // usualy operators defined as: "public static int operator +", but in the MSIL are translated to op_Add
            MethodInfo mi = null;

            if (leftVal != null)
            {
                Type leftType = leftVal.GetType();
                mi = leftType.GetMethod("op_Addition", new Type[] { leftType, leftType });
            }
            else if (rightVal != null)
            {
                Type rightType = rightVal.GetType();
                mi = rightType.GetMethod("op_Addition", new Type[] { rightType, rightType });
            }
            else
                // If both of the values equal Null - return Null
                return null;

            if (mi == null)
            {
                if (leftVal != null)
                {
                    if (leftVal is string)
                    {
                        return string.Concat(leftVal, rightVal.ToString());
                    }
                    if (leftVal is int)
                    {
                        return (int)leftVal + Convert.ToInt32(rightVal);
                    }
                }
                if (rightVal != null)
                {
                    if (rightVal is string)
                    {
                        return string.Concat(leftVal.ToString(), rightVal);
                    }
                    if (rightVal is int)
                    {
                        return (int)rightVal + Convert.ToInt32(leftVal);
                    }
                }
                throw new InvalidOperationException("The operator \"+\" is not defined for type " + leftVal.GetType().ToString());
            }

            return null;
        }

        internal override void Execute(ExecutionContext context, ICommandRuntime commandRuntime)
        {
            // TODO: sum the values in both pipelines
            // TODO: if there are more than one value in the left - just copy left results and then the right results to the resulting pipe
            // TODO: if there is only one value on the left - convert the value on the right to the type of the left and then Sum

            commandRuntime.WriteObject(GetValue(context));
        }
    }
}