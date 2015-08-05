// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Management.Automation.Language;
using System.Management.Automation;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace System.Management.Pash.Implementation
{
    class SettableInvokeMemberExpression : SettableMemberExpression
    {
        private InvokeMemberExpressionAst _expressionAst;

        internal SettableInvokeMemberExpression(InvokeMemberExpressionAst expressionAst, ExecutionVisitor currentExecution)
            : base(expressionAst, currentExecution)
        {
            _expressionAst = expressionAst;
        }

        public override object GetValue()
        {
            return null;
        }

        public override void SetValue(object value)
        {
            var psobj = PSObject.AsPSObject(EvaluatedBase);
            var memberNameObj = EvaluatedMember;

            var member = PSObject.GetMemberInfoSafe(psobj, memberNameObj, _expressionAst.Static);
            if (member == null)
            {
                var msg = String.Format("Member '{0}' to be assigned is null", memberNameObj.ToString());
                throw new PSArgumentNullException(msg);
            }

            var property = member as PSParameterizedProperty;
            if (property == null)
            {
                var msg = String.Format("Member '{0}' to be assigned is not a ParameterizedProperty", memberNameObj.ToString());
                throw new PSArgumentException(msg);
            }

            List<object> arguments = _expressionAst.Arguments.Select(CurrentExecution.EvaluateAst)
                .Select(obj => obj is PSObject ? ((PSObject)obj).BaseObject : obj)
                .ToList();

            arguments.Add(value);

            property.Invoke(arguments.ToArray());
        }
    }
}
