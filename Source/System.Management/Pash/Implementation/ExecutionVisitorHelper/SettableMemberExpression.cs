using System;
using System.Management.Automation.Language;
using System.Management.Automation;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace System.Management.Pash.Implementation
{
    public class SettableMemberExpression : SettableExpression
    {
        private readonly Dictionary<Type, BaseTypeSettable> _settableTypes = new Dictionary<Type, BaseTypeSettable>()
        {
            {typeof(Hashtable), new HashtableTypeSettable()},
            {typeof(System.Xml.XmlNode), new XmlNodeTypeSettable()},
            {typeof(object[]), new XmlObjectArrayTypeSettable()},
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
            if (settableType != null && settableType.TrySetValue(unwraped, memberNameObj, value))
            {
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
            if (unwrappedType != null)
            {
                var itemType = unwrappedType;
                while(itemType != null && !_settableTypes.ContainsKey(itemType))
                {
                    itemType = itemType.BaseType;
                }
                if (itemType != null)
                {
                    var settableType = _settableTypes[itemType];
                    if(settableType.CanResolve(memberNameObj))
                    {
                        return settableType;
                    }
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
            if (settableType != null)
            {
                object result = null;
                if (settableType.TryGetValue(unwraped, memberNameObj, out result))
                {
                    return result;
                }

            }

            // otherwise a PSObject
            var member = PSObject.GetMemberInfoSafe(psobj, memberNameObj, _expressionAst.Static);
            return (member == null) ? null : PSObject.WrapOrNull(member.Value);
        }


        abstract class BaseTypeSettable
        {
            public virtual bool CanResolve(object memberNameObj)
            {
                if (memberNameObj == null)
                {
                    return false;
                }
                return true;
            }
            public abstract bool TryGetValue(object unwrapped, object memberNameObj, out object result);
            public abstract bool TrySetValue(object unwrapped, object memberNameObj, object value);
        }

        class HashtableTypeSettable : BaseTypeSettable
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

            public override bool TryGetValue(object unwrapped, object memberNameObj, out object result)
            {
                result = ((Hashtable) unwrapped)[memberNameObj];
                return true;
            }

            public override bool TrySetValue(object unwrapped, object memberNameObj, object value)
            {
                ((Hashtable) unwrapped)[memberNameObj] = value;
                return true;
            }
        }



        private static bool TryGetTextNode(System.Xml.XmlNode node, out string value)
        {
            if (node.ChildNodes.Count == 1)
            {
                if (node.ChildNodes[0].NodeType == System.Xml.XmlNodeType.Text)
                {
                    value = node.ChildNodes[0].InnerText;
                    return true;
                }
            }
            value = null;
            return false;
        }

        class XmlNodeTypeSettable : BaseTypeSettable
        {
            
            public override bool TryGetValue(object unwrapped, object memberNameObj, out object result)
            {
                var memberNameString = (string)memberNameObj;

                var childNodes = ((System.Xml.XmlNode)unwrapped).SelectNodes("//" + memberNameString);

                if (childNodes.Count == 1)
                {
                    var childNode = childNodes[0];
                    string stringResult = null;
                    if(TryGetTextNode(childNode, out stringResult))
                    {
                        result = stringResult;
                        return true;
                    }
        
                    result = childNode;
                    return true;
                }

                if (childNodes.Count > 1)
                {

                    var resultNodes = new List<object>();
                    foreach (System.Xml.XmlNode node in childNodes)
                    {
                        string stringResult = null;
                        if (TryGetTextNode(node, out stringResult))
                        {
                            resultNodes.Add(stringResult);
                        }
                        else
                        {
                            resultNodes.Add(node);
                        }
                    }
                    result = resultNodes.ToArray();
                    return true;
                }

                result = childNodes;
                return true;
            }

            public override bool TrySetValue(object unwrapped, object memberNameObj, object value)
            {
                throw new NotImplementedException();
            }
        }

        class XmlObjectArrayTypeSettable : BaseTypeSettable
        {
           
            public override bool TryGetValue(object unwrapped, object memberNameObj, out object result)
            {
                var memberNameString = (string)memberNameObj;

                // NOTE: we need to be careful to only work on arrays of XmlNode objects
                // if we detect that an item in the array is not an XmlNode, we short circut
                // and leave the loop returning false.

                var resultingItems = new List<object>();
                foreach (var item in (object[])unwrapped)
                {
                    if (typeof(System.Xml.XmlNode).IsInstanceOfType(item))
                    {
                        var xmlNode = (System.Xml.XmlNode)item;
                        var value = xmlNode[memberNameString];
                        string stringResult = null;
                        if (TryGetTextNode(value, out stringResult))
                        {
                            resultingItems.Add(stringResult);
                        }
                        else
                        {
                            resultingItems.Add(value);
                        }
                    }
                    else
                    {
                        result = unwrapped;
                        return false;
                    }
                }

                if (resultingItems.Count > 0)
                {
                    result = resultingItems.ToArray();
                    return true;
                }
                result = unwrapped;
                return false;
            }

            public override bool TrySetValue(object unwrapped, object memberNameObj, object value)
            {
                throw new NotImplementedException();
            }
        }

    }
}

