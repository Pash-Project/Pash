// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Linq;
using System.Collections.Generic;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Text;
using Pash.Implementation;
using System.Management.Automation;

namespace System.Management.Automation
{
    internal class CommandProcessor : CommandProcessorBase
    {
        internal Cmdlet Command { get; set; }
        readonly CmdletInfo _cmdletInfo;
        bool _beganProcessing;

        public CommandProcessor(CmdletInfo cmdletInfo)
            : base(cmdletInfo)
        {
            _cmdletInfo = cmdletInfo;
            _beganProcessing = false;
        }

        internal override ICommandRuntime CommandRuntime
        {
            get
            {
                return Command.CommandRuntime;
            }
            set
            {
                Command.CommandRuntime = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <remarks>
        /// All abount Cmdlet parameters: http://msdn2.microsoft.com/en-us/library/ms714433(VS.85).aspx
        /// </remarks>
        internal override void BindArguments(PSObject obj)
        {
            if ((obj == null) && (Parameters.Count == 0))
                return;

            // TODO: bind the arguments to the parameters
            CommandParameterSetInfo paramSetInfo = _cmdletInfo.GetDefaultParameterSet();
            // TODO: refer to the Command._ParameterSetName for a param set name

            if (obj != null)
            {
                var valueFromPipelineParameter = paramSetInfo.Parameters.Where(paramInfo => paramInfo.ValueFromPipeline).SingleOrDefault();

                if (valueFromPipelineParameter != null)
                    BindArgument(valueFromPipelineParameter.Name, obj, valueFromPipelineParameter.ParameterType);
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
                        paramInfo = paramSetInfo.GetParameterByName(parameter.Name);

                        if (paramInfo != null)
                        {
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
                SetValue(memberInfo, Command, new[] { PSObject.AsPSObject(value) });
            }

            else if (memberType == typeof(String[]))
            {
                SetValue(memberInfo, Command, new[] { value.ToString() });
            }

            else if (memberType == typeof(String))
            {
                SetValue(memberInfo, Command, value.ToString());
            }

            else if (memberType == typeof(PSObject))
            {
                SetValue(memberInfo, Command, PSObject.AsPSObject(value));
            }

            else if (memberType == typeof(SwitchParameter))
            {
                SetValue(memberInfo, Command, new SwitchParameter(true));
            }

            else if (memberType == typeof(Object[]))
            {
                SetValue(memberInfo, Command, new[] { value });
            }

            else
            {
                SetValue(memberInfo, Command, value);
            }
        }

        public static void SetValue(MemberInfo info, object targetObject, object value)
        {
            if (info.MemberType == MemberTypes.Field)
                ((FieldInfo)info).SetValue(targetObject, value);
            else if (info.MemberType == MemberTypes.Property)
                ((PropertyInfo)info).SetValue(targetObject, value, null);
            else
                throw new Exception("SetValue only implemented for fields and properties");
        }

        internal override void Initialize()
        {
            try
            {
                Cmdlet cmdlet = (Cmdlet)Activator.CreateInstance(_cmdletInfo.ImplementingType);

                cmdlet.CommandInfo = _cmdletInfo;
                cmdlet.ExecutionContext = base.ExecutionContext;
                Command = cmdlet;
            }
            catch (Exception e)
            {
                // TODO: work out the failure
                System.Console.WriteLine(e);
            }
        }

        internal override void ProcessRecord()
        {
            // TODO: initialize Cmdlet parameters
            if (!_beganProcessing)
            {
                Command.DoBeginProcessing();
                _beganProcessing = true;
            }

            Command.DoProcessRecord();
        }

        internal void ProcessObject(object inObject)
        {
            /*
            PSObject inPsObject = null;
            if (inputObject != null)
            {
                inPsObject = PSObject.AsPSObject(inObject);
            }
            */
        }

        internal override void Complete()
        {
            Command.DoEndProcessing();
        }
    }
}
