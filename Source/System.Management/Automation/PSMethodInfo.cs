// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Collections.Generic;

namespace System.Management.Automation
{
    public abstract class PSMethodInfo : PSMemberInfo
    {
        protected PSMethodInfo()
        {
        }

        public override string TypeNameOfValue {
            get
            {
                return GetType().FullName;
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
                return this;
            }
            set
            {
                throw new SetValueException("Can't change Method");
            }
        }

        public abstract Collection<string> OverloadDefinitions { get; }

        public abstract object Invoke(params object[] arguments);
    }
}
