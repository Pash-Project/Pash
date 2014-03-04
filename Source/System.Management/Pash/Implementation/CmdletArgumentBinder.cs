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
            // TODO: If parameter has ValueFromRemainingArguments any unmatched arguments should be bound to this parameter as an array
            // TODO: If no parameter has ValueFromRemainingArguments any supplied parameters are unmatched then fail with exception
            // TODO: try to get missing parameters that cannot be acquired by pipeline input, and fail otherwise

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
            // TODO: Bind obj properties to ValueFromPipelinebyPropertyName parameters
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

            bool didSet = false;
            // TODO: make this generic
            if (memberType == typeof(PSObject[])) // check for simple PSObjects / arrays of it first
            {
                SetCommandValue(memberInfo, new[] { PSObject.AsPSObject(value) });
                didSet = true;
            }
            else if (memberType == typeof(PSObject))
            {
                SetCommandValue(memberInfo, PSObject.AsPSObject(value));
                didSet = true;
            }

            if (didSet)
            {
                _boundArguments.Add(memberInfo);
                return;
            }

            // unpack the value if it's a PSObject
            if (value is PSObject)
            {
                value = ((PSObject)value).BaseObject;
            }
            if (memberType == typeof(String[]))  // check for strings and convert
            {
                SetCommandValue(memberInfo, ConvertToStringArray(value));
            }
            else if (memberType == typeof(String))
            {
                SetCommandValue(memberInfo, value.ToString());
            }
            else if (memberType.IsEnum) // enums have to be parsed
            {
                SetCommandValue(memberInfo, Enum.Parse(type, value.ToString(), true));
            }
            else if (memberType == typeof(SwitchParameter)) // switch parameters can simply be present
            {
                SetCommandValue(memberInfo, new SwitchParameter(true));
            }
            else if (value != null && memberType == value.GetType()) // same type
            {
                SetCommandValue(memberInfo, value);
            }
            // check if array alements need to be casted
            else if (memberType.IsArray)
            {
                var elementType = memberType.GetElementType();
                Array convertedValues;
                if (value.GetType().IsArray)
                {
                    Array valueArray = (Array)value;
                    int valueCount = valueArray.Length;
                    convertedValues = Array.CreateInstance(elementType, valueCount);
                    // try to cast each element to the desired type
                    for (int i = 0; i < valueCount; i++)
                    {
                        var converted = ConvertOrCast(valueArray.GetValue(i), elementType);
                        convertedValues.SetValue(converted, i);
                    }
                }
                else
                {
                    convertedValues = Array.CreateInstance(elementType, 1);
                    convertedValues.SetValue(ConvertOrCast(value, elementType), 0);
                }
                SetCommandValue(memberInfo, convertedValues);
            }
            else // last option: try direct cast
            {
                SetCommandValue(memberInfo, ConvertOrCast(value, memberType));
            }
            _boundArguments.Add(memberInfo);
        }

        private object ConvertToStringArray(object value)
        {
            if ((value is IEnumerable) && !(value is string))
            {
                return (from object item in (IEnumerable)value
                    select item.ToString()).ToArray();
            }
            else
            {
                return new[] { value.ToString() };
            }
        }

        private object ConvertOrCast(object value, Type type)
        {
            if (value == null)
            {
                return null;
            }
            // check for convertibles
            try
            {
                if (value is IConvertible)
                {
                    return Convert.ChangeType(value, type);
                }
            }
            catch (Exception) //ignore exception and try cast
            {
            }
            // following idea from http://stackoverflow.com/questions/3062807/dynamic-casting-based-on-type-information
            var castMethod = this.GetType().GetMethod("Cast").MakeGenericMethod(type);
            // it's okay to have an excpetion if we can't do anything anymore, then the parameter just doesn't work
            return castMethod.Invoke(null, new object[] { value });
        }

        public static T Cast<T>(object obj)
        {
            return (T) obj;
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

