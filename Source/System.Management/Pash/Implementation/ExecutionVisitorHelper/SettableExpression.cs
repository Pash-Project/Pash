using System;
using System.Management.Automation.Language;

namespace System.Management.Pash.Implementation
{
    public abstract class SettableExpression
    {
        internal ExecutionVisitor CurrentExecution { get; private set; }

        public static readonly Type[] SupportedExpressions = new [] {
            typeof(VariableExpressionAst), typeof(MemberExpressionAst), typeof(IndexExpressionAst)
        };

        internal SettableExpression(ExecutionVisitor currentExecution)
        {
            CurrentExecution = currentExecution;
        }

        public abstract object GetValue();

        public abstract void SetValue(object value);

        internal static SettableExpression Create(ExpressionAst valueExpression, ExecutionVisitor currentExecution)
        {
            if (valueExpression is VariableExpressionAst)
            {
                return new SettableVariableExpression((VariableExpressionAst)valueExpression, currentExecution);
            }
            else if (valueExpression is MemberExpressionAst)
            {
                return new SettableMemberExpression((MemberExpressionAst)valueExpression, currentExecution);
            }
            else if (valueExpression is IndexExpressionAst)
            {
                return new SettableIndexExpression((IndexExpressionAst)valueExpression, currentExecution);
            }
            var msg = String.Format("The expression is not a modifiable value, but of type '{0}'. Please report this!",
                                    valueExpression.GetType().FullName);
            throw new InvalidOperationException(msg);
        }

        protected object GetEventuallyEvaluatedValue(ExpressionAst expression, ref bool isEvaluated, ref object value)
        {
            if (!isEvaluated)
            {
                value = CurrentExecution.EvaluateAst(expression);
                isEvaluated = true;
            }
            return value;
        }
    }
}

