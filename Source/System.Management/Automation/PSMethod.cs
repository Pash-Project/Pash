using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Collections.Generic;

namespace System.Management.Automation
{
    public class PSMethod : PSMethodInfo
    {

        private MethodInfo _methodInfo;
        private object _owner;

        internal PSMethod(MethodInfo info, object owner, bool isInstance)
             : base()
        {
            Name = info.Name;
            _methodInfo = info;
            _owner = owner;
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
            var stuffedArgs = StuffVariableParameters(arguments);
            return _methodInfo.Invoke(_owner, stuffedArgs);
        }

        public override PSMemberInfo Copy()
        {
            return new PSMethod(_methodInfo, _owner, IsInstance);
        }

        private object[] StuffVariableParameters(object[] arguments)
        {
            // TODO: support for better type conversion through LanguagePrimitives? This would be a good place
            var parameters = _methodInfo.GetParameters();
            var lastParam = parameters.LastOrDefault();
            // check if parameters exist and if last parameter is actually defined with "params"
            if (lastParam == null ||
                !lastParam.GetCustomAttributes(typeof(ParamArrayAttribute), true).Any())
            {
                return arguments;
            }

            List<object> argList = new List<object>();
            // invocations with less arguments provided than parameters exist will fail anyway later on
            var numNonVariableParams = parameters.Count() - 1;
            // copy the first not variable parameters to the new list
            argList.AddRange(arguments.Take(numNonVariableParams));
            // add the others a an array for the last parameter
            argList.Add(arguments.Skip(numNonVariableParams).ToArray());
            return argList.ToArray();
        }
    }
}

