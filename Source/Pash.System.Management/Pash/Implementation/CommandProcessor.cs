using System;
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
        private CmdletInfo _cmdletInfo;
        private bool _beganProcessing;

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
                foreach (CommandParameterInfo paramInfo in paramSetInfo.Parameters)
                {
                    if (paramInfo.ValueFromPipeline)
                    {
                        BindArgument(paramInfo.Name, obj, paramInfo.ParameterType);
                    }
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
                        paramInfo = paramSetInfo.GetParameterByName(parameter.Name);

                        if (paramInfo != null)
                        {
                            BindArgument(paramInfo.Name, parameter.Value, paramInfo.ParameterType);
                        }
                    }
                }
            }
        }

        private void BindArgument(string name, object value, Type type)
        {
            // TODO: extract this into a method
            PropertyInfo propertyInfo = Command.GetType().GetProperty(name, type);

            // TODO: make this generic
            if (propertyInfo.PropertyType == typeof(PSObject[]))
            {
                propertyInfo.SetValue(Command, new[] { PSObject.AsPSObject(value) }, null);
            }

            else if (propertyInfo.PropertyType == typeof(String[]))
            {
                propertyInfo.SetValue(Command, new[] { value.ToString() }, null);
            }

            else if (propertyInfo.PropertyType == typeof(String))
            {
                propertyInfo.SetValue(Command, value.ToString(), null);
            }

            else
            {
                propertyInfo.SetValue(Command, value, null);
            }
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
