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
        private MethodInfo[] Overloads
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
            object[] newArgs;
            var methodInfo = FindBestMethod(arguments, out newArgs);
            return methodInfo.Invoke(_instance, newArgs);
        }

        public override PSMemberInfo Copy()
        {
            return new PSMethod(Name, _classType, _instance, IsInstance);
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

        private MethodInfo FindBestMethod(object[] arguments, out object[] newArguments)
        {
            var candidates = new List<Tuple<MethodInfo, object[]>>();
            var numArgs = arguments.Length;
            // try direct match first
            var argTypes = (from arg in arguments
                                     select arg.GetType()).ToArray();
            var methodInfo = _classType.GetMethod(Name, argTypes);
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

            // then check methods with conersion, optionall parameters or "params" parameter
            foreach (var method in Overloads)
            {
                object[] fittingArgs;
                if (MethodFitsArgs(method, arguments, out fittingArgs))
                {
                    candidates.Add(new Tuple<MethodInfo, object[]>(method, fittingArgs));
                }
            }
            if (candidates.Count < 1)
            {
                throw new PSArgumentException("The method (or none of its overloads) takes the given arguments!");
            }
            else if (candidates.Count > 1)
            {
                var candidateStrings = String.Join(Environment.NewLine, from cand in candidates
                    select MethodWithParametersToString(cand.Item1));
                throw new PSArgumentException("Multiple overloaded functions match the given parameters: "
                    + Environment.NewLine + candidateStrings);
            }
            else
            {
                var tuple = candidates[0];
                newArguments = tuple.Item2;
                return tuple.Item1;
            }
        }

        private string MethodWithParametersToString(MethodInfo method)
        {
            string paras = String.Join(", ", from param in method.GetParameters()
                                                      select param.ParameterType.Name);
            return String.Format("{0}({1})", method.Name, paras);
        }
    }
}

