using System;
using System.Management.Automation.Language;

namespace System.Management.Pash.Implementation
{
    public class SettableIndexExpression : SettableExpression
    {
        internal SettableIndexExpression(IndexExpressionAst expressionAst, ExecutionVisitor currentExecution)
        {
        }

        public override object GetValue()
        {
            throw new NotImplementedException();
        }

        public override void SetValue(object value)
        {
            throw new NotImplementedException();
        }
    }
}

