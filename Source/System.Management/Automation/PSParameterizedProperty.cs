// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace System.Management.Automation
{
    public class PSParameterizedProperty : PSMethodInfo
    {
        private Type _classType;
        private object _instance;
        private PropertyInfo _propertyInfo;

        public bool IsGettable { get; private set; }
        public bool IsSettable { get; private set; }

        internal PSParameterizedProperty(PropertyInfo propertyInfo, Type classType, object owner, bool isInstance)
             : base()
        {
            _classType = classType;
            _instance = owner;
            _propertyInfo = propertyInfo;

            IsInstance = isInstance;
            Name = propertyInfo.Name;
            IsGettable = propertyInfo.CanRead;
            IsSettable = propertyInfo.CanWrite;
        }

        public override PSMemberTypes MemberType
        {
            get
            {
                return PSMemberTypes.ParameterizedProperty;
            }
        }

        public override string TypeNameOfValue
        {
            get
            {
                return _propertyInfo.PropertyType.FullName;
            }
        }

        public override Collection<string> OverloadDefinitions
        {
            get { throw new NotImplementedException(); }
        }

        public override object Invoke(params object[] arguments)
        {
            throw new NotImplementedException();
        }

        public override PSMemberInfo Copy()
        {
            throw new NotImplementedException();
        }

        internal static bool IsParameterizedProperty(PropertyInfo propertyInfo)
        {
            if (propertyInfo.CanRead)
            {
                MethodInfo getMethodInfo = propertyInfo.GetGetMethod();
                if (getMethodInfo.GetParameters().Any())
                {
                    return true;
                }
            }

            if (propertyInfo.CanWrite)
            {
                MethodInfo setMethodInfo = propertyInfo.GetSetMethod();
                if (setMethodInfo != null && setMethodInfo.GetParameters().Count() > 1)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
