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

        private bool _baseIsEvaluated;
        private object _baseValue;
        public object EvaluatedBase
        {
            get
            {
                return GetEventuallyEvaluatedValue(_expressionAst.Expression, ref _baseIsEvaluated, ref _baseValue);
            }
        }

        private bool _memberIsEvaluated;
        private object _memberValue;
        public object EvaluatedMember
        {
            get
            {
                return GetEventuallyEvaluatedValue(_expressionAst.Member, ref _memberIsEvaluated, ref _memberValue);
            }
        }


        internal SettableMemberExpression(MemberExpressionAst expressionAst, ExecutionVisitor currentExecution)
            : base(currentExecution)
        {
            _expressionAst = expressionAst;
        }

        public override void SetValue(object value)
        {
            var psobj = PSObject.AsPSObject(EvaluatedBase);
            var unwraped = PSObject.Unwrap(psobj);
            var memberNameObj = EvaluatedMember;
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

            member.Value = PSObject.Unwrap(value);
        }

       public override object GetValue()
       {
            var psobj = PSObject.WrapOrNull(EvaluatedBase);
            var unwraped = PSObject.Unwrap(psobj);
            var memberNameObj = EvaluatedMember;
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

