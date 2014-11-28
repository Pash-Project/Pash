using System;
using System.Management.Automation.Language;
using System.Management.Automation;
using System.Collections.Generic;
using System.Collections;

namespace System.Management.Pash.Implementation
{
    public class SettableMemberExpression : SettableExpression
    {
        private readonly HashSet<string> _hashtableAccessibleMembers = 
            new HashSet<string>(StringComparer.InvariantCultureIgnoreCase) {
                "Count", "Keys", "Values", "Remove"
        };

        private MemberExpressionAst _expressionAst;
        private ExecutionVisitor _currentExecution;

        internal SettableMemberExpression(MemberExpressionAst expressionAst, ExecutionVisitor currentExecution)
        {
            _expressionAst = expressionAst;
            _currentExecution = currentExecution;
        }

        public override void SetValue(object value)
        {
            var psobj = PSObject.AsPSObject(_currentExecution.EvaluateAst(_expressionAst.Expression, false));
            var unwraped = PSObject.Unwrap(psobj);
            var memberNameObj = _currentExecution.EvaluateAst(_expressionAst.Member, false);
            // check for Hashtable first
            if (unwraped is Hashtable && memberNameObj != null &&
                !_hashtableAccessibleMembers.Contains(memberNameObj.ToString()))
            { 
                ((Hashtable) unwraped)[memberNameObj] = value;
                return;
            }
            // else it's a PSObject
            var member = PSObject.GetMemberInfoSafe(psobj, memberNameObj, _expressionAst.Static);
            if (member == null)
            {
                var msg = String.Format("Member '{0}' to be assigned is null", memberNameObj.ToString());
                throw new PSArgumentNullException(msg);
            }
            member.Value = value;
        }

       public override object GetValue()
        {
            var psobj = PSObject.AsPSObject(_currentExecution.EvaluateAst(_expressionAst.Expression));
            var unwraped = PSObject.Unwrap(psobj);
            var memberNameObj = _currentExecution.EvaluateAst(_expressionAst.Member, false);
            // check for Hastable first
            if (unwraped is Hashtable && memberNameObj != null &&
                !_hashtableAccessibleMembers.Contains(memberNameObj.ToString()))
            {
                return ((Hashtable)unwraped)[memberNameObj];
            }
            // otherwise a PSObject
            var member = PSObject.GetMemberInfoSafe(psobj, memberNameObj, _expressionAst.Static);
            return (member == null) ? null : PSObject.AsPSObject(member.Value);
        }
    }
}

