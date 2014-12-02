using System;
using System.Linq;
using System.Management.Automation.Runspaces;
using System.Management.Automation.Language;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Pash.Implementation;
using System.Threading;

namespace Pash.Implementation
{
    public class ScriptBlockParameterBinder
    {
        private ExecutionContext _executionContext;
        private ReadOnlyCollection<ParameterAst> _scriptParameters;
        private ExecutionVisitor _executionVisitor;

        internal ScriptBlockParameterBinder(ReadOnlyCollection<ParameterAst> scriptParameters, ExecutionContext context,
                                            ExecutionVisitor executionVisitor)
        {
            _scriptParameters = scriptParameters;
            _executionContext = context;
            _executionVisitor = executionVisitor;
        }

        public void BindCommandLineParameters(CommandParameterCollection parameters)
        {
            // TODO: as soon as we support cmdlet functions, we should think about using the CmdletParameterBinder
            // or at least extract common functionality
            var stringComparer = StringComparer.InvariantCultureIgnoreCase;

            var unboundArguments = new List<CommandParameter>(parameters);
            var unboundScriptParameters = new List<ParameterAst>(_scriptParameters);

            // bind arguments by name
            var boundArgumentNames = BindArgumentsByName(unboundArguments);
            unboundArguments.RemoveAll(arg => boundArgumentNames.Contains(arg.Name, stringComparer));
            unboundScriptParameters.RemoveAll(
                param => boundArgumentNames.Contains(param.Name.VariablePath.UserPath, stringComparer));

            // then we set all parameters by position
            boundArgumentNames = BindArgumentsByPosition(unboundScriptParameters, unboundArguments);
            unboundArguments.RemoveAll(arg => boundArgumentNames.Contains(arg.Name, stringComparer));
            unboundScriptParameters.RemoveAll(
                param => boundArgumentNames.Contains(param.Name.VariablePath.UserPath, stringComparer));

            // parameters that are still unbound should get their default value
            SetUnboundParametersToDefaultValue(unboundScriptParameters);

            // finally we will set the $Args variable with all values that are left
            SetArgsVariableFromUnboundArgs(unboundArguments);
        }

        private List<string> BindArgumentsByName(List<CommandParameter> unboundArguments)
        {
            var boundArgumentNames = new List<string>();
            // first find all named arguments for those parameters that exist
            var scriptParamDict = _scriptParameters.ToDictionary<ParameterAst, string>(
                param => param.Name.VariablePath.UserPath, StringComparer.InvariantCultureIgnoreCase);
            var namedArgs = from arg in unboundArguments
                    where !String.IsNullOrEmpty(arg.Name) && scriptParamDict.Keys.Contains(arg.Name)
                    select arg;
            foreach (var namedArg in namedArgs)
            {
                // if the parameter is bound multiple times, throw an error
                if (boundArgumentNames.Contains(namedArg.Name))
                {
                    var msg = String.Format("The parameter '{0}' has already been bound", namedArg.Name);
                    throw new ParameterBindingException(msg, "ParameterAlreadyBound");
                }
                BindVariable(scriptParamDict[namedArg.Name], namedArg);
                boundArgumentNames.Add(namedArg.Name);
            }
            return boundArgumentNames;
        }

        private List<string> BindArgumentsByPosition(List<ParameterAst> unboundParameters, List<CommandParameter> unboundArguments)
        {
            var boundArgumentNames = new List<string>();
            var unnamedArguments = (from arg in unboundArguments
                                    where String.IsNullOrEmpty(arg.Name) select arg).ToList();
            var unnamedArgCount = unnamedArguments.Count;
            var paramCount = unboundParameters.Count;
            for (int i = 0; i < Math.Min(paramCount, unnamedArgCount); i++)
            {
                var curArg = unnamedArguments[i];
                BindVariable(unboundParameters[i], curArg);
                boundArgumentNames.Add(curArg.Name);
            }
            return boundArgumentNames;
        }

        private void SetUnboundParametersToDefaultValue(List<ParameterAst> unboundScriptParameters)
        {
            foreach (var unboundParam in unboundScriptParameters)
            {
                BindVariable(unboundParam, null);
            }
        }

        private void SetArgsVariableFromUnboundArgs(List<CommandParameter> unboundArguments)
        {
            var otherArgs = new List<object>();
            foreach (var unboundArg in unboundArguments)
            {
                var nameSet = false;
                if (!String.IsNullOrEmpty(unboundArg.Name))
                {
                    otherArgs.Add(unboundArg.GetDecoratedName());
                    nameSet = true;
                }
                if (!nameSet || unboundArg.HasExplicitArgument)
                {
                    otherArgs.Add(unboundArg.Value);
                }
            }
            _executionContext.SetVariable("Args", otherArgs.ToArray());
        }

        private void BindVariable(ParameterAst scriptParameter, CommandParameter argument)
        {
            var value = GetParameterValue(scriptParameter, argument);
            _executionContext.SetVariable(scriptParameter.Name.VariablePath.UserPath, value);
        }

        private object GetParameterValue(ParameterAst scriptParameter, CommandParameter argument)
        {
            if (argument != null)
            {
                return argument.Value;
            }
            if (scriptParameter.DefaultValue == null)
            {
                return null;
            }
            return _executionVisitor.EvaluateAst(scriptParameter.DefaultValue);
        }
    }
}

