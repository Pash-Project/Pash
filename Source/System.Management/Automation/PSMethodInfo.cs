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

        protected abstract MethodInfo GetMethod(Type[] argTypes);

        protected abstract MethodInfo[] Overloads { get; }

        internal object InvokeMethod(object instance, object[] arguments)
        {
            object[] newArgs;
            var methodInfo = FindBestMethod(arguments, out newArgs);
            try
            {
                return methodInfo.Invoke(instance, newArgs);
            }
            catch (TargetInvocationException e)
            {
                var msg = e.InnerException == null ? "Error invoking method '" + methodInfo.ToString() + "'"
                                                   : e.InnerException.Message;
                throw new MethodInvocationException(msg, e.InnerException);
            }
        }

        static private bool MethodFitsArgs(MethodInfo method, object[] arguments, out object[] newArguments)
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

        static private bool IsParamsParameter(ParameterInfo info)
        {
            return info.GetCustomAttributes(typeof(ParamArrayAttribute), true).Any();
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
                throw new MethodException(string.Format("Cannot find an overload for \"{0}\" and the argument count: \"{1}\".", Name, GetArgumentsLength(arguments)));
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

        protected virtual int GetArgumentsLength(object[] arguments)
        {
            return arguments.Length;
        }

        private string MethodWithParametersToString(MethodInfo method)
        {
            string paras = String.Join(", ", from param in method.GetParameters()
                                             select param.ParameterType.Name);
            return String.Format("{0}({1})", method.Name, paras);
        }
    }
}
