using System;
using System.Management.Automation.Language;
using System.Management.Automation;
using System.Collections.Generic;
using System.Collections;

namespace System.Management.Pash.Implementation
{
    public class SettableMemberExpression : SettableExpression
    {
        private readonly Dictionary<Type, BaseTypeSettable> _settableTypes = new Dictionary<Type, BaseTypeSettable>()
        {
            {typeof(Hashtable), new HashTableTypeSettable()},
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
            var unwrappedType = (unwraped == null) ? null : unwraped.GetType();
            var memberNameObj = EvaluatedMember;

            var settableType = GetSettableType(unwrappedType, memberNameObj);
            if (settableType != null && settableType.CanResolve(memberNameObj))
            {
                settableType.SetValue(unwraped, memberNameObj, value);
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

        private BaseTypeSettable GetSettableType(Type unwrappedType, object memberNameObj)
        {
            if (unwrappedType != null && _settableTypes.ContainsKey(unwrappedType))
            {
                var settableType = _settableTypes[unwrappedType];
                if(settableType.CanResolve(memberNameObj))
                {
                    return settableType;
                }
            }
            return null;
        }

       public override object GetValue()
       {
            var psobj = PSObject.WrapOrNull(EvaluatedBase);
            var unwraped = PSObject.Unwrap(psobj);
            var unwrappedType = (unwraped == null) ? null : unwraped.GetType();
            var memberNameObj = EvaluatedMember;

            var settableType = GetSettableType(unwrappedType, memberNameObj);
            if (settableType != null && settableType.CanResolve(memberNameObj))
            {
                return _settableTypes[unwrappedType].GetValue(unwraped, memberNameObj);
            }

            // otherwise a PSObject
            var member = PSObject.GetMemberInfoSafe(psobj, memberNameObj, _expressionAst.Static);
            return (member == null) ? null : PSObject.WrapOrNull(member.Value);
        }


        abstract class BaseTypeSettable
        {
            public abstract bool CanResolve(object memberNameObj);
            public abstract object GetValue(object unwrapped, object memberNameObj);
            public abstract void SetValue(object unwrapped, object memberNameObj, object value);
        }

        class HashTableTypeSettable : BaseTypeSettable
        {

            private readonly HashSet<string> _hashtableAccessibleMembers = 
                new HashSet<string>(StringComparer.InvariantCultureIgnoreCase) {
                "Count", "Keys", "Values", "Remove"
            };


            public override bool CanResolve(object memberNameObj)
            {
                if (memberNameObj == null)
                {
                    return false;
                }
                return !_hashtableAccessibleMembers.Contains(memberNameObj.ToString());
            }

            public override object GetValue(object unwrapped, object memberNameObj)
            {
                return ((Hashtable) unwrapped)[memberNameObj];
            }

            public override void SetValue(object unwrapped, object memberNameObj, object value)
            {
                ((Hashtable) unwrapped)[memberNameObj] = value;
            }

        }

    }
}

