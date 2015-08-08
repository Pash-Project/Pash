// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using Extensions.Types;

namespace System.Management.Automation
{
    public class PSParameterizedProperty : PSMethodInfo
    {
        private Type _classType;
        private object _instance;
        private PropertyInfo _propertyInfo;
        private Collection<string> _overloadDefinitions;

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
            get
            {
                if (_overloadDefinitions == null)
                {
                    _overloadDefinitions = new Collection<string>();
                    _overloadDefinitions.Add(GetDefinition());
                }
                return _overloadDefinitions;
            }
        }

        /// <summary>
        /// TODO: Refactor. This code is taken directory from PSMethod with a few small changes.
        /// </summary>
        public override object Invoke(params object[] arguments)
        {
            object[] newArgs;
            var methodInfo = FindBestMethod(arguments, out newArgs);
            try
            {
                return methodInfo.Invoke(_instance, newArgs);
            }
            catch (TargetInvocationException e)
            {
                var msg = e.InnerException == null ? "Error invoking method '" + methodInfo.ToString() + "'"
                                                   : e.InnerException.Message;
                throw new MethodInvocationException(msg, e.InnerException);
            }
        }

        public override PSMemberInfo Copy()
        {
            return new PSParameterizedProperty(_propertyInfo, _classType, _instance, IsInstance);
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

        private MethodInfo FindBestMethod(object[] arguments, out object[] newArguments)
        {
            var candidates = new List<Tuple<MethodInfo, object[]>>();
            var numArgs = arguments.Length;
            // try direct match first
            if (!arguments.Contains(null))
            {
                var argTypes = (from arg in arguments
                                select arg.GetType()).ToArray();
                var methodInfo = GetMethod(argTypes);
                if (methodInfo != null && methodInfo.IsPublic && methodInfo.IsStatic != IsInstance)
                {
                    // this doesn't mean that we got all arguments for methodInfo, e.g. we'd have a
                    // methodInfo for a method that takes an optional argument although argTypes doesn't
                    // include it. As old mono versions don't support invoking with optional arguments
                    // we need to take care of it
                    object[] fittingArgs;
                    if (MethodFitsArgs(methodInfo, arguments, out fittingArgs))
                    {
                        newArguments = fittingArgs;
                        return methodInfo;
                    }
                    // if we weren't successful (shouldn't be the case, but who knows), we try all overloads
                }
            }

            throw new PSArgumentException("The method (or none of its overloads) takes the given arguments!");
        }

        private MethodInfo GetMethod(Type[] argTypes)
        {
            if (_propertyInfo.CanWrite)
            {
                var setMethod = _propertyInfo.GetSetMethod();
                if (setMethod != null && setMethod.GetParameters().Count() == argTypes.Length)
                {
                    return setMethod;
                }
            }
            return _propertyInfo.GetGetMethod();
        }

        private bool MethodFitsArgs(MethodInfo method, object[] arguments, out object[] newArguments)
        {
            var numArgs = arguments.Length;
            var paras = method.GetParameters();
            var numParams = paras.Length;
            newArguments = new object[numParams];
            var minCommon = numArgs;
            if (numArgs < numParams)
            {
                for (int i = numArgs; i < numParams; i++)
                {
                    var curParam = paras[i];
                    if (curParam.IsOptional)
                    {
                        newArguments[i - numArgs] = curParam.DefaultValue;
                    }
                    else if (IsParamsParameter(curParam))
                    {
                        newArguments[i - numArgs] = Array.CreateInstance(curParam.ParameterType.GetElementType(), 0);
                    }
                    else
                    {
                        return false;
                    }
                }
                minCommon = numArgs;
            }
            else if (numArgs > numParams)
            {
                if (numParams == 0)
                {
                    return false;
                }

                var lastParam = paras[numParams - 1];
                if (!IsParamsParameter(lastParam))
                {
                    return false;
                }
                var paramsType = lastParam.ParameterType.GetElementType();
                var paramsArray = Array.CreateInstance(paramsType, numArgs - numParams + 1);
                for (int i = numParams - 1; i < numArgs; i++)
                {
                    object converted;
                    if (!LanguagePrimitives.TryConvertTo(arguments[i], paramsType, out converted))
                    {
                        return false;
                    }
                    paramsArray.SetValue(converted, i - numParams + 1);
                }
                newArguments[numParams - 1] = paramsArray;
                minCommon = numParams - 1;
            }
            for (int i = 0; i < minCommon; i++)
            {
                object converted;
                if (!LanguagePrimitives.TryConvertTo(arguments[i], paras[i].ParameterType, out converted))
                {
                    return false;
                }
                newArguments[i] = converted;
            }
            return true;
        }

        private bool IsParamsParameter(ParameterInfo info)
        {
            return info.GetCustomAttributes(typeof(ParamArrayAttribute), true).Any();
        }


        private string GetDefinition()
        {
            MethodInfo getMethod = _propertyInfo.GetGetMethod();

            var definition = new StringBuilder();
            if (_propertyInfo.CanRead)
            {
                definition.Append(getMethod.ReturnType.FriendlyName());
            }
            else
            {
                definition.Append("void");
            }
            definition.Append(' ');

            definition.Append(Name);

            ParameterInfo[] parameters = null;
            if (_propertyInfo.CanRead)
            {
                parameters = getMethod.GetParameters();
            }
            else
            {
                parameters = _propertyInfo.GetSetMethod().GetParameters();
                parameters = parameters.Take(parameters.Length - 1).ToArray();
            }

            definition.Append('(');
            definition.Append(string.Join(", ", parameters.Select(parameter => GetParameterDefinition(parameter))));
            definition.Append(") ");

            definition.Append('{');
            if (_propertyInfo.CanRead)
            {
                definition.Append("get;");
            }

            if (_propertyInfo.CanWrite && _propertyInfo.GetSetMethod() != null)
            {
                definition.Append("set;");
            }
            definition.Append('}');

            return definition.ToString();
        }

        private static string GetParameterDefinition(ParameterInfo parameter)
        {
            return parameter.ParameterType.FriendlyName() + " " + parameter.Name;
        }
    }
}
