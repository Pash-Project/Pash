// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.ObjectModel;
using System.Reflection;

namespace System.Management.Automation
{
    public class PSMethodInfo : PSMemberInfo
    {
        private MethodInfo _methodInfo;
        private object _owner;

        protected PSMethodInfo()
        {
        }

        internal PSMethodInfo(MethodInfo info, object owner)
        {
            Name = info.Name;
            _methodInfo = info;
            _owner = owner;
        }

        public override string TypeNameOfValue {
            get
            {
                return _methodInfo.GetType().ToString();
            }
        }

        public override PSMemberTypes MemberType
        {
            get
            {
                return PSMemberTypes.Method;
            }
        }

        public override sealed object Value
        {
            get
            {
                return _methodInfo;
            }
            set
            {
                throw new SetValueException("Can't change Method Info");
            }
        }

        public Collection<string> OverloadDefinitions 
        { 
            get
            {
                throw new NotImplementedException();
            }
        }

        public  object Invoke(params object[] arguments)
        {
            return _methodInfo.Invoke(_owner, arguments);
        }

        public override PSMemberInfo Copy()
        {
            return new PSMethodInfo(_methodInfo, _owner);
        }
    }
}
