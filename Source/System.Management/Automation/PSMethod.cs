using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Collections.Generic;

namespace System.Management.Automation
{
    public class PSMethod : PSMethodInfo
    {
        private Type _classType;
        private object _instance;

        private MethodInfo[] _overloads;
        protected override MethodInfo[] Overloads
        {
            get
            {
                if (_overloads == null)
                {
                    var flags = BindingFlags.Public;
                    flags |= IsInstance ? BindingFlags.Instance : BindingFlags.Static;
                    _overloads = (from method in _classType.GetMethods(flags)
                                                 where method.Name.Equals(Name)
                                                 select method).ToArray();
                }
                return _overloads;
            }
        }

        internal PSMethod(string methodName, Type classType, object owner, bool isInstance)
             : base()
        {
            Name = methodName;
            _classType = classType;
            _instance = owner;
            IsInstance = isInstance;
        }

        public override Collection<string> OverloadDefinitions 
        { 
            get
            {
                throw new NotImplementedException();
            }
        }

        public override object Invoke(params object[] arguments)
        {
            return InvokeMethod(_instance, arguments);
        }

        public override PSMemberInfo Copy()
        {
            return new PSMethod(Name, _classType, _instance, IsInstance);
        }

        protected override MethodInfo GetMethod(Type[] argTypes)
        {
            return _classType.GetMethod(Name, argTypes);
        }
    }
}

