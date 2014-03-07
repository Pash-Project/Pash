// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Text;
using Pash.Implementation;
using System.Management.Automation;
using System.Collections.ObjectModel;

namespace System.Management.Automation
{
    internal class CommandProcessor : CommandProcessorBase
    {
        private Dictionary<MemberInfo, object> _defaultArguments;
        private Collection<MemberInfo> _boundArguments;
        private Dictionary<MemberInfo, object> _commandLineArgumentsBackup;
        internal Cmdlet Command { get; set; }
        readonly CmdletInfo _cmdletInfo;
        bool _beganProcessing;

        public CommandProcessor(CmdletInfo cmdletInfo)
            : base(cmdletInfo)
        {
            _cmdletInfo = cmdletInfo;
            _beganProcessing = false;
            _defaultArguments = new Dictionary<MemberInfo, object>();
            _boundArguments = new Collection<MemberInfo>();
            _commandLineArgumentsBackup = new Dictionary<MemberInfo, object>();
        }

        // TODO: move parameter related stuff to a separate class

        /// <summary>
        /// Binds the parameters from the command line to the command.
        /// </summary>
        /// <remarks>
        /// All abount Cmdlet parameters: http://msdn2.microsoft.com/en-us/library/ms714433(VS.85).aspx
        /// About the lifecycle: http://msdn.microsoft.com/en-us/library/ms714429(v=vs.85).aspx
        /// </remarks>
        internal void BindCommandLineArguments()
        {
            // TODO: If parameter has ValueFromRemainingArguments any unmatched arguments should be bound to this parameter as an array
            // TODO: If no parameter has ValueFromRemainingArguments any supplied parameters are unmatched then fail with exception
            // TODO: try to get missing parameters that cannot be acquired by pipeline input, and fail otherwise

            if (Parameters.Count == 0)
                return;

            IEnumerable<string> namedParameters = Parameters.Select (x => x.Name).Where (x => !string.IsNullOrEmpty (x));
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


            if (Parameters.Count > 0)
            {
                // bind by position location
                for (int i = 0; i < Parameters.Count; i++)
                {
                    CommandParameterInfo paramInfo = null;

                    CommandParameter parameter = Parameters[i];

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
                            && i < Parameters.Count - 1 && Parameters[i + 1].Name == null)
                        {
                            BindArgument(paramInfo.Name, Parameters[i + 1].Value, paramInfo.ParameterType);
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
        internal void BindPipelineArguments(object curInput)
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
            MemberInfo memberInfo = Command.GetType().GetProperty(name, type);

            if (memberInfo != null)
            {
                memberType = ((PropertyInfo)memberInfo).PropertyType;
            }
            else  // No property found try bind to field instead
            {
                memberInfo = Command.GetType().GetField(name);

                if (memberInfo != null)
                {
                    memberType = ((FieldInfo)memberInfo).FieldType;
                }
                else
                {
                    throw new Exception("Unable to get field or property named: " + name);
                }
            }

            // TODO: make this generic
            if (memberType == typeof(PSObject[]))
            {
                SetCommandValue(memberInfo, new[] { PSObject.AsPSObject(value) });
            }

            else if (memberType == typeof(String[]))
            {
                SetCommandValue(memberInfo, ConvertToStringArray(value));
            }

            else if (memberType == typeof(String))
            {
                SetCommandValue(memberInfo, value.ToString());
            }

            else if (memberType.IsEnum) {
                  SetCommandValue (memberInfo, Enum.Parse (type, value.ToString(), true));
            }

            else if (memberType == typeof(PSObject))
            {
                SetCommandValue(memberInfo, PSObject.AsPSObject(value));
            }

            else if (memberType == typeof(SwitchParameter))
            {
                SetCommandValue(memberInfo, new SwitchParameter(true));
            }

            else if (memberType == typeof(Object[]))
            {
                SetCommandValue(memberInfo, new[] { value });
            }

            else
            {
                SetCommandValue(memberInfo, value is PSObject ? ((PSObject)value).BaseObject : value);
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
                fieldInfo.SetValue(Command, value);
            }
            else if (info.MemberType == MemberTypes.Property)
            {
                PropertyInfo propertyInfo = info as PropertyInfo;
                propertyInfo.SetValue(Command, value, null);
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
                return fieldInfo.GetValue(Command);
            }
            else if (info.MemberType == MemberTypes.Property)
            {
                PropertyInfo propertyInfo = info as PropertyInfo;
                return propertyInfo.GetValue(Command, null);
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
        private void RestoreCommandLineArguments()
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
        private void CheckParameterSet()
        {
            // TODO: implement
        }

        /// <summary>
        /// First phase of cmdlet lifecycle: "Binding Parameters that Take Command-Line Input"
        /// </summary>
        public override void Prepare()
        {
            Cmdlet cmdlet = (Cmdlet)Activator.CreateInstance(_cmdletInfo.ImplementingType);
            cmdlet.CommandInfo = _cmdletInfo;
            cmdlet.ExecutionContext = base.ExecutionContext;
            cmdlet.CommandRuntime = CommandRuntime;
            Command = cmdlet;
            BindCommandLineArguments();
        }

        /// <summary>
        /// Second phase. Basically calling the command's "BeginProcessing" method
        /// </summary>
        public override void BeginProcessing()
        {
            if (!_beganProcessing)
            {
                Command.DoBeginProcessing();
                _beganProcessing = true;
            }
        }

        /// <summary>
        /// In this phase, the "ProcessRecord" method of the command will be called for each
        /// object from the input pipeline, but at least once. Doing so, the input object
        /// will be bound as parameters, but only for the specific invocation.
        /// </summary>
        public override void ProcessRecords()
        {
            // check if we already called BeginProcessing for this command
            if (!_beganProcessing)
            {
                // this can happen if the previous element in the pipeline produces output in the BeginProcessing phase
                // than this command is asked to process the records but wasn't in the BeginProcessing phase, yet.
                BeginProcessing();
            }
            var inputObjects = CommandRuntime.InputStream.Read();
            foreach (var curInput in inputObjects)
            {
                RestoreCommandLineArguments();
                BindPipelineArguments(curInput);
                try
                {
                    CheckParameterSet();
                }
                catch (Exception e)
                {
                    var error = new ErrorRecord(e, "NotAllParametersProvided", ErrorCategory.InvalidOperation, null);
                    CommandRuntime.WriteError(error);
                    continue;
                }
                Command.DoProcessRecord();
            }
            RestoreCommandLineArguments();
        }

        /// <summary>
        /// In the cleanup phase, the command's "EndProcessing" method will be called to do own cleanup
        /// </summary>
        public override void EndProcessing()
        {
            Command.DoEndProcessing();
        }
    }
}
