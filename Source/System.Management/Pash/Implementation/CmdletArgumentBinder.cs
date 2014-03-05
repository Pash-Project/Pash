// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Collections;
using System.Management.Automation.Runspaces;

namespace System.Management.Automation
{
    internal class CmdletArgumentBinder
    {
        private Dictionary<MemberInfo, object> _defaultArguments;
        private Collection<MemberInfo> _boundArguments;
        private Dictionary<MemberInfo, object> _commandLineArgumentsBackup;
        private CmdletInfo _cmdletInfo;
        private Cmdlet _cmdlet;

        public CmdletArgumentBinder(CmdletInfo cmdletInfo, Cmdlet cmdlet)
        {
            _cmdletInfo = cmdletInfo;
            _cmdlet = cmdlet;
            _defaultArguments = new Dictionary<MemberInfo, object>();
            _boundArguments = new Collection<MemberInfo>();
            _commandLineArgumentsBackup = new Dictionary<MemberInfo, object>();
        }

        /// <summary>
        /// Binds the parameters from the command line to the command.
        /// </summary>
        /// <remarks>
        /// All abount Cmdlet parameters: http://msdn2.microsoft.com/en-us/library/ms714433(VS.85).aspx
        /// About the lifecycle: http://msdn.microsoft.com/en-us/library/ms714429(v=vs.85).aspx
        /// </remarks>
        public void BindCommandLineArguments(CommandParameterCollection parameters)
        {
            // TODO: sburnicki - implement the behavior:
            // How command line arguments are bound:
            // 1. Bind all named parameters, fail if some name is bound twice
            // 2. Check if there is an "active" parameter set, yet. Active: Identified to be used (unique parameter set)
            //    If there is more than one "active" parameter set, fail with ambigous error
            // 3. Bind positional parameters per parameter set
            //    To do so, sort parameters of set by position, ignore bound parameters and set the first unbound position
            // 4. This would be the place for common parameters, support of ShouldProcess and dynamic parameters
            // 5. Check if there is an "active" parameter set, yet, and fail if there are more than one
            // 6. For either the (active or default) parameter set: Gather missing mandatory parameters by UI
            //    But not pipeline related parameters, they might get passed later on
            // 7. Check if (active or default) parameter set has unbound mandatory parameters and fail if so
            //    But ignore pipeline related parameters for that check, they might get passed later on
            // 8. Define "candidate" parameter sets: All that have mandatory parameters set, ignoring pipeline related
            //    If one set is already active, this should be the only candidate set
            // 9. Bind all set parameters for (active or default) parameter set and back them up 

            if (parameters.Count == 0)
                return;

            IEnumerable<string> namedParameters = parameters.Select (x => x.Name).Where (x => !string.IsNullOrEmpty (x));
            CommandParameterSetInfo paramSetInfo = _cmdletInfo.GetDefaultParameterSet();
            IDictionary<string, CommandParameterInfo> namedParametersLookup = paramSetInfo.LookupAllParameters(namedParameters);
            IEnumerable<KeyValuePair<string, CommandParameterInfo>> unknownParameters = namedParametersLookup.ToList ().Where(x => x.Value == null);
            if (unknownParameters.Any ())
            {
                // Cannot find all named parameters in default parameter set.
                // Lets try other parameter sets.
                bool found = false;
                foreach (CommandParameterSetInfo nonDefaultParamSetInfo in _cmdletInfo.GetNonDefaultParameterSets())
                {
                    namedParametersLookup = nonDefaultParamSetInfo.LookupAllParameters(namedParameters);
                    if (!namedParametersLookup.Values.Where (x => x == null).Any ())
                    {
                        paramSetInfo = nonDefaultParamSetInfo;
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    throw new ParameterBindingException("No parameter found matching '" + unknownParameters.First().Key + "'");
                }
            }


            if (parameters.Count > 0)
            {
                // bind by position location
                for (int i = 0; i < parameters.Count; i++)
                {
                    CommandParameterInfo paramInfo = null;

                    CommandParameter parameter = parameters[i];

                    if (string.IsNullOrEmpty(parameter.Name))
                    {
                        paramInfo = paramSetInfo.GetParameterByPosition(i);

                        if (paramInfo != null)
                        {
                            BindArgument(paramInfo.Name, parameter.Value, paramInfo.ParameterType);
                        }
                    }
                    else
                    {
                        namedParametersLookup.TryGetValue(parameter.Name, out paramInfo);
                        if (parameter.Value == null && paramInfo.ParameterType != typeof(SwitchParameter)
                            && i < parameters.Count - 1 && parameters[i + 1].Name == null)
                        {
                            BindArgument(paramInfo.Name, parameters[i + 1].Value, paramInfo.ParameterType);
                            i++;
                        }
                        else
                            BindArgument(paramInfo.Name, parameter.Value, paramInfo.ParameterType);
                    }
                }
            }
            BackupCommandLineArguments();
        }

        /// <summary>
        /// Binds the arguments provided by pipeline to be processed as a separate record
        /// </summary>
        /// <param name="curInput">Current input object from the pipeline</param>
        public void BindPipelineArguments(object curInput)
        {
            // TODO: sburnicki - implement the behavior:
            // How pipeline parameters are bound
            // 1. If this command is the first in pipeline and there are no input objects:
            //    Then get all left mandatory parameters for the (active or default) parameter set from UI
            // 2. If there is an input object:
            //    1. If the default parameter set is still a candidate:
            //       1. Try to bind the object to the parameter *without* type conversion with "ValueFromPipeline" set, if exists
            //       2. If this is not sucessfull, try to bind the object to parameters with "ValueFromPipelineByPropertyName"
            //         *without" conversion, if exists
            //       3. If this fails, try 1. step but *with* type conversion
            //       4. If this fails, try 2. step but *with* type conversion
            //    2. If binding to the default set was not possible, try the same with all parameters
            // 3. This would be the place to process dynamic pipeline parameters
            // 3. Check for active sets, fail the record (ambiguous) if there are multiples (can only be no-defaults)
            // 4. Check for candidate sets, not ignoring pipeline related parameters
            //    1. If there is only one candidate: Choose that parameter set
            //    3. If there is more than one candidate, and there is one active set among them: choose that set
            //    4. If there is more than one candidate, no active set, but the default set: choose the default set
            //    4. If there is more than one candidate, and no active set: fail the record (ambiguous)
            //    4. If there is no candidate, fail the record and tell why the (active or default) set wasn't satisfied
            // 5. At this point we know the chosen parameter set. The parameters can be bound to the command

            if (curInput == null)
            {
                return;
            }

            CommandParameterSetInfo paramSetInfo = _cmdletInfo.GetDefaultParameterSet();
            var valueFromPipelineParameter = paramSetInfo.Parameters.Where(paramInfo => paramInfo.ValueFromPipeline).SingleOrDefault();

            if (valueFromPipelineParameter != null)
            {
                // TODO: throw an error if this parameter is already bound (in _boundArguments)
                BindArgument(valueFromPipelineParameter.Name, curInput, valueFromPipelineParameter.ParameterType);
            }
        }

        private void BindArgument(string name, object value, Type type)
        {
            Type memberType = null;

            // Look for Property to bind to
            MemberInfo memberInfo = _cmdlet.GetType().GetProperty(name, type);

            if (memberInfo != null)
            {
                memberType = ((PropertyInfo)memberInfo).PropertyType;
            }
            else  // No property found try bind to field instead
            {
                memberInfo = _cmdlet.GetType().GetField(name);

                if (memberInfo != null)
                {
                    memberType = ((FieldInfo)memberInfo).FieldType;
                }
                else
                {
                    throw new Exception("Unable to get field or property named: " + name);
                }
            }

            // ConvertTo throws an exception if conversion isn't possible. That's just fine.
            object converted = LanguagePrimitives.ConvertTo(value, memberType);
            SetCommandValue(memberInfo, converted);
            _boundArguments.Add(memberInfo);
        }

        private void SetCommandValue(MemberInfo info, object value)
        {
            // make a backup of the default values first, if we don't have one
            if (!_defaultArguments.ContainsKey(info))
            {
                _defaultArguments[info] = GetCommandValue(info);
            }
            // now set the field or property
            if (info.MemberType == MemberTypes.Field)
            {
                FieldInfo fieldInfo = info as FieldInfo;
                fieldInfo.SetValue(_cmdlet, value);
            }
            else if (info.MemberType == MemberTypes.Property)
            {
                PropertyInfo propertyInfo = info as PropertyInfo;
                propertyInfo.SetValue(_cmdlet, value, null);
            }
            else
            {
                throw new Exception("SetValue only implemented for fields and properties");
            }
        }

        private object GetCommandValue(MemberInfo info)
        {
            if (info.MemberType == MemberTypes.Field)
            {
                FieldInfo fieldInfo = info as FieldInfo;
                return fieldInfo.GetValue(_cmdlet);
            }
            else if (info.MemberType == MemberTypes.Property)
            {
                PropertyInfo propertyInfo = info as PropertyInfo;
                return propertyInfo.GetValue(_cmdlet, null);
            }
            else
            {
                throw new Exception("SetValue only implemented for fields and properties");
            }
        }

        /// <summary>
        /// Makes and internal backup of the command line arguments, so they can be restored
        /// </summary>
        private void BackupCommandLineArguments()
        {
            _commandLineArgumentsBackup.Clear();
            foreach (var info in _boundArguments)
            {
                _commandLineArgumentsBackup[info] = GetCommandValue(info);
            }
        }

        /// <summary>
        /// Restores arguments to those provided by command line, so multiple
        /// records can be processed without influencing each other
        /// </summary>
        public void RestoreCommandLineArguments()
        {
            foreach (var bound in _boundArguments)
            {
                if (_commandLineArgumentsBackup.ContainsKey(bound))
                {
                    SetCommandValue(bound, _commandLineArgumentsBackup[bound]);
                }
                else // if it wasn't a command line argument, restor the default
                {
                    SetCommandValue(bound, _defaultArguments[bound]);
                }
            }
            // reset the bound arguments list
            _boundArguments.Clear();
            foreach (MemberInfo info in _commandLineArgumentsBackup.Keys)
            {
                _boundArguments.Add(info);
            }
        }

        /// <summary>
        /// Checks wether we can clearly identify a parameter set to use which is not
        /// umbigious and has all mandatory parameters set. Fails with an exception if
        /// this is not the case.
        /// </summary>
        public void CheckParameterSet()
        {
            // TODO: implement
        }


    }
}

