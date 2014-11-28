using System;
using System.Management.Automation.Language;

namespace System.Management.Pash.Implementation
{
    public class SettableVariableExpression : SettableExpression
    {
        public override object GetValue()
        {
            throw new NotImplementedException();
        }

        public override void SetValue(object value)
        {
            throw new NotImplementedException();
        }

        internal SettableVariableExpression(VariableExpressionAst expressionAst, ExecutionVisitor currentExecution)
        {
        }
    }
}

