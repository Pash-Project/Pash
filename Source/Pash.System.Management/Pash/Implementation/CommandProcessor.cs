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

        public CommandProcessor(CmdletInfo cmdletInfo) : base(cmdletInfo)
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
                        // TODO: extract this into a method
                        PropertyInfo pi = Command.GetType().GetProperty(paramInfo.Name, paramInfo.ParameterType);
                        pi.SetValue(Command, obj, null);
                    }
                }
            }

            if (Parameters.Count > 0)
            {
                // bind by position location
                for (int i = 0; i < Parameters.Count; i++ )
                {
                    CommandParameterInfo paramInfo = null;

                    CommandParameter parameter = Parameters[i];

                    if (string.IsNullOrEmpty(parameter.Name))
                    {
                        paramInfo = paramSetInfo.GetParameterByPosition(i);

                        if (paramInfo != null)
                        {
                            // TODO: extract this into a method
                            PropertyInfo pi = Command.GetType().GetProperty(paramInfo.Name, paramInfo.ParameterType);
                            // TODO: make this generic
                            if (pi.PropertyType == typeof(PSObject[]))
                            {
                                PSObject[] arr = new PSObject[] { PSObject.AsPSObject(Parameters[i].Value) };
                                pi.SetValue(Command, arr, null);
                            }
                            else if(pi.PropertyType == typeof(String[]))
                            {
                                String[] arr = new String[] { Parameters[i].Value.ToString() };
                                pi.SetValue(Command, arr, null);
                            }
                            else
                            {
                                pi.SetValue(Command, Parameters[i].Value, null);
                            }
                        }
                    }
                    else
                    {
                        paramInfo = paramSetInfo.GetParameterByName(parameter.Name);

                        if (paramInfo != null)
                        {
                            // TODO: extract this into a method
                            PropertyInfo pi = Command.GetType().GetProperty(paramInfo.Name, paramInfo.ParameterType);
                            pi.SetValue(Command, Parameters[i].Value, null);
                        }
                    }
                }
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
